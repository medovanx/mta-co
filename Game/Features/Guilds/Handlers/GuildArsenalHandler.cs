using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Network;
using MTA.Network.GamePackets;
using MTA.Network.PacketHandlers;
using Message = MTA.Network.GamePackets.Message;

namespace MTA.Game.Features.Guilds.Handlers;

[PacketHandler(2202, 2203, 2204)]
public static class GuildArsenalHandler {
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        switch (packetId) {
            case 2202: {
                var view = new ArsenalView(false);
                view.Deserialize(packet);
                ViewArsenalPage(view, client);
                break;
            }
            case 2203: {
                var command = new ArsenalCommand();
                command.Deserialize(packet);
                switch (command.Type) {
                    case ArsenalCommand.Unlock:
                        UnlockArsenal(command, client);
                        break;
                    case ArsenalCommand.Inscribe:
                        InscribeArsenalItem(command, client);
                        break;
                    case ArsenalCommand.Uninscribe:
                        UniscribeArsenalItem(command, client);
                        break;
                    case ArsenalCommand.Enchant:
                        EnchantArsenal(command, client);
                        break;
                    case ArsenalCommand.View:
                        ViewGuildArsenal(client);
                        break;
                }

                break;
            }
            case 2204: {
                HandleFastInscription(packet, client);
                break;
            }
        }

        return true;
    }

    private static void HandleFastInscription(byte[] packet, GameState client) {
        var count = packet[4];
        List<uint> itemUids = [];
        var offset = 5;
        for (var i = 0; i < count; i++) {
            itemUids.Add(BitConverter.ToUInt32(packet, offset));
            offset += 4;
        }

        foreach (var uid in itemUids) {
            var item = ConquerItemTable.LoadItem(uid);
            var arsenalRealPosition = ArsenalPosition(item.ID);
            if (item.ID % 10 < 8 || item.Inscribed) continue;
            if (client.Entity.GuildID == 0 || client.Guild == null ||
                !client.Guild.Arsenals[arsenalRealPosition].Unlocked) continue;
            item.Inscribed = true;
            item.Mode = Enums.ItemMode.Update;
            item.Send(client);
            var arsenal = client.Guild.Arsenals[arsenalRealPosition];
            arsenal.AddItem(item, client);
            client.Guild.SaveArsenal();
        }
    }

    public static int ArsenalPosition(uint id) {
        var pos = PacketHandler.ItemPosition(id);
        return pos switch {
            ConquerItem.Head => 0,
            ConquerItem.Armor => 1,
            ConquerItem.LeftWeapon or ConquerItem.RightWeapon => 2,
            ConquerItem.Ring => 3,
            ConquerItem.Boots => 4,
            ConquerItem.Necklace => 5,
            ConquerItem.Fan => 6,
            ConquerItem.Tower => 7,
            _ => -1
        };
    }

    private static void ViewArsenalPage(ArsenalView view, GameState client) {
        if (client.Entity.GuildID == 0 || client.Guild == null) return;
        if (client.Guild.Arsenals.Length < view.ArsenalType) return;
        var arsenal = client.Guild.Arsenals[view.ArsenalType];
        if (!arsenal.Unlocked) return;
        var beginAt = view.BeginAt - 1;
        var length = (uint)arsenal.OrderedList.Count;
        length -= beginAt;
        length = Math.Min(length, 8);
        view = new ArsenalView(true, length) {
            BeginAt = beginAt + 1
        };
        for (var i = (int)beginAt; i < beginAt + length; i++)
            view.AppendItem(arsenal.OrderedList[i]);
        view.EndAt = length + view.BeginAt - 1;
        view.ArsenalType = (uint)(arsenal.Position - 1);
        view.Count = length;
        view.Donation = arsenal.Donation;
        view.Enchantment = arsenal.Enhancement;
        view.EnchantmentExpirationDate = arsenal.EnhancementExpirationDate();
        view.SharedBattlepower = arsenal.SharedBattlePower;
        view.TotalInscribed = arsenal.OrderedList.Count;
        client.Send(view);
    }

    private static void InscribeArsenalItem(ArsenalCommand command, GameState client) {
        if (!client.Inventory.TryGetItem(command.dwParam2, out var item)) return;
        var arsenalRealPosition = ArsenalPosition(item.ID);
        if (item.ID % 10 < 8 || arsenalRealPosition != command.dwParam || item.Inscribed) return;
        if (client.Entity.GuildID == 0 || client.Guild == null ||
            !client.Guild.Arsenals[command.dwParam].Unlocked) return;
        item.Inscribed = true;
        item.Mode = Enums.ItemMode.Update;
        item.Send(client);
        var arsenal = client.Guild.Arsenals[command.dwParam];
        arsenal.AddItem(item, client);
        client.Guild.SaveArsenal();
    }

    private static void UniscribeArsenalItem(ArsenalCommand command, GameState client) {
        if (client.Entity.GuildID == 0 || client.Guild == null) return;
        var arsenal = client.Guild.Arsenals[command.dwParam];
        if (!arsenal.Unlocked) return;
        if (!arsenal.ItemDictionary.TryGetValue(command.dwParam2, out var item)) return;
        if (item.OwnerUID != client.Entity.UID) return;
        
        // Find the item
        if (!client.Inventory.TryGetItem(item.UID, out var foundItem)) {
            var found = false;
            foreach (var eqItem in client.Equipment.Objects)
                if (eqItem.UID == item.UID) {
                    foundItem = eqItem;
                    found = true;
                    break;
                }

            if (!found)
                foreach (var wh in client.Warehouses.Values)
                foreach (var eqItem in wh.Objects)
                    if (eqItem.UID == item.UID) {
                        foundItem = eqItem;
                        break;
                    }
        }

        foundItem.Inscribed = false;
        if (foundItem.Warehouse == 0) {
            foundItem.Mode = Enums.ItemMode.Update;
            foundItem.Send(client);
        }

        arsenal.RemoveItem(item, client);
        client.Guild.ArsenalBpChanged = true;
        client.Guild.GetMaxSharedBattlePower();
        client.Guild.SaveArsenal();
    }

    private static void UnlockArsenal(ArsenalCommand command, GameState client) {
        if (client.Entity.GuildID == 0) return;
        var guild = client.Guild;
        if (guild == null) return;
        if (guild.Arsenals[command.dwParam].Unlocked) {
            client.Send(new Message("This arsenal was already unlocked!", Color.Red, Message.Talk));
            return;
        }

        var cost = guild.GetCurrentArsenalCost();
        if (guild.SilverFund >= cost) {
            guild.SilverFund -= cost;
            guild.Arsenals[command.dwParam].Unlocked = true;
            guild.SendGuild(client);

            guild.ArsenalBpChanged = true;
            guild.GetMaxSharedBattlePower();
            guild.SaveArsenal();
        }
        else {
            client.Send(new Message("Your guild doesn't have enough funds!", Color.Red, Message.Talk));
        }
    }

    private static void ViewGuildArsenal(GameState client) {
        if (client.Entity.GuildID == 0) return;
        if (client.Guild == null) return;
        var view = new ArsenalTab(true) {
            SharedBattlepower = (uint)client.Guild.GetMaxSharedBattlePower(),
            ArsenalCount = 8
        };
        foreach (var arsenal in client.Guild.Arsenals)
            view.AppendArsenal(arsenal);
        view.HeroDonation = client.GetArsenalDonation();
        view.HeroSharedBattlepower = client.Entity.GuildBattlePower;
        client.Send(view);
    }

    public static void EnchantArsenal(ArsenalCommand command, GameState client) {
        if (client.Entity.GuildID == 0 || client.Guild == null) return;
        var guild = client.Guild;
        var arsenal = guild.Arsenals[command.dwParam];
        if (!arsenal.Unlocked) return;
        if (arsenal.SharedBattlePower == 3) return;
        var cost = 20000000 + command.dwParam3 * 40000000;
        if (guild.SilverFund >= cost) {
            guild.SilverFund -= cost;
            arsenal.Enhancement = command.dwParam3;
            arsenal.EnhancementExpDate = DateTime.Now.AddDays(30);

            guild.ArsenalBpChanged = true;
            guild.GetMaxSharedBattlePower();
            client.Guild.SaveArsenal();
        }
    }

    public static void UniscribeItem(ConquerItem item, GameState client) {
        if (client.Entity.GuildID == 0 || client.Guild == null) return;
        var arsenalPosition = ArsenalPosition(item.ID);
        client.Guild.Arsenals[arsenalPosition].RemoveItem(item, client);
        item.Inscribed = false;
        item.Mode = Enums.ItemMode.Update;
        item.Send(client);
        //Save is done other else.
    }

    public static void UniscribeAllItems(GameState client) {
        if (client.Guild == null) return;
        
        foreach (var item in client.Inventory.Objects)
            if (item.Inscribed)
                UniscribeItem(item, client);
        foreach (var item in client.Equipment.Objects)
            if (item is { Inscribed: true })
                UniscribeItem(item, client);
        foreach (var item in from wh in client.Warehouses.Values from item in wh.Objects where item.Inscribed select item)
            UniscribeItem(item, client);

        client.Guild.ArsenalBpChanged = true;
        client.Guild.GetMaxSharedBattlePower();
        client.Guild.SaveArsenal();
    }
}