using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Game.Features.Guilds.Packets.Writers;
using MTA.Network;
using MTA.Network.GamePackets;
using MTA.Network.PacketHandlers;
using Message = MTA.Network.GamePackets.Message;

namespace MTA.Game.Features.Guilds.Packets.Handlers;

/// <summary>
///     Handles guild arsenal packets (2202, 2203, 2204) for viewing, inscribing, and managing guild arsenal items.
/// </summary>
[PacketHandler(2202, 2203, 2204)]
public static class GuildArsenalHandler {
    /// <summary>
    ///     Routes arsenal-related packets to appropriate handlers based on packet ID.
    /// </summary>
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        switch (packetId) {
            case 2202: {
                var view = new GuildArsenalViewPacket(false);
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

    /// <summary>
    ///     Handles bulk item inscription, allowing multiple items to be inscribed at once.
    /// </summary>
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

    /// <summary>
    ///     Maps item ID to arsenal slot position based on item type (Head, Armor, Weapon, etc.).
    /// </summary>
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

    /// <summary>
    ///     Displays paginated arsenal items for a specific arsenal slot, showing inscribed items with their stats.
    /// </summary>
    private static void ViewArsenalPage(GuildArsenalViewPacket view, GameState client) {
        if (client.Entity.GuildID == 0 || client.Guild == null) return;
        if (client.Guild.Arsenals.Length < view.ArsenalType) return;
        var arsenal = client.Guild.Arsenals[view.ArsenalType];
        if (!arsenal.Unlocked) return;
        var beginAt = view.BeginAt - 1;
        var length = (uint)arsenal.OrderedList.Count;
        length -= beginAt;
        length = Math.Min(length, 8);
        view = new GuildArsenalViewPacket(true, length) {
            BeginAt = beginAt + 1
        };
        for (var i = (int)beginAt; i < beginAt + length; i++) {
            view.AppendItem(arsenal.OrderedList[i]);
        }

        view.EndAt = length + view.BeginAt - 1;
        view.ArsenalType = (uint)(arsenal.Position - 1);
        view.Count = length;
        view.Donation = arsenal.Donation;
        view.Enchantment = arsenal.Enhancement;
        view.EnchantmentExpirationDate = arsenal.EnhancementExpirationDate();
        view.SharedBattlePower = arsenal.SharedBattlePower;
        view.TotalInscribed = arsenal.OrderedList.Count;
        client.Send(view);
    }

    /// <summary>
    ///     Inscribes item to guild arsenal, making it contribute to shared battle power and tracking donation worth.
    /// </summary>
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

    /// <summary>
    ///     Removes inscription from arsenal item, returning it to normal item status and updating arsenal donation.
    /// </summary>
    private static void UniscribeArsenalItem(ArsenalCommand command, GameState client) {
        if (client.Entity.GuildID == 0 || client.Guild == null) return;
        var arsenal = client.Guild.Arsenals[command.dwParam];
        if (!arsenal.Unlocked) return;
        if (!arsenal.ItemDictionary.TryGetValue(command.dwParam2, out var item)) return;
        if (item.OwnerUid != client.Entity.UID) return;

        // Find the item
        if (!client.Inventory.TryGetItem(item.Uid, out var foundItem)) {
            var found = false;
            foreach (var eqItem in client.Equipment.Objects) {
                if (eqItem.UID != item.Uid) continue;
                foundItem = eqItem;
                found = true;
                break;
            }

            if (!found)
                foreach (var wh in client.Warehouses.Values)
                foreach (var eqItem in wh.Objects) {
                    if (eqItem.UID != item.Uid) continue;
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

    /// <summary>
    ///     Unlocks an arsenal slot using guild funds, allowing members to inscribe items to that slot.
    /// </summary>
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

    /// <summary>
    ///     Shows complete arsenal overview with all slots, shared battle power, and player's personal donation contribution.
    /// </summary>
    private static void ViewGuildArsenal(GameState client) {
        var view = new GuildArsenalTabPacket(true) {
            SharedBattlePower = (uint)client.Guild!.GetMaxSharedBattlePower(),
            ArsenalCount = 8
        };
        foreach (var arsenal in client.Guild.Arsenals) {
            view.AppendArsenal(arsenal);
        }

        view.HeroDonation = client.GetArsenalDonation();
        view.HeroSharedBattlePower = client.Entity.GuildBattlePower;
        client.Send(view);
    }

    /// <summary>
    ///     Enhances arsenal slot with temporary boost using guild funds, increasing shared battle power for 30 days.
    /// </summary>
    private static void EnchantArsenal(ArsenalCommand command, GameState client) {
        if (client.Entity.GuildID == 0 || client.Guild == null) return;
        var guild = client.Guild;
        var arsenal = guild.Arsenals[command.dwParam];
        if (!arsenal.Unlocked) return;
        if (arsenal.SharedBattlePower == 3) return;
        var cost = 20000000 + command.dwParam3 * 40000000;
        if (guild.SilverFund < cost) return;
        guild.SilverFund -= cost;
        arsenal.Enhancement = command.dwParam3;
        arsenal.EnhancementExpDate = DateTime.Now.AddDays(30);

        guild.ArsenalBpChanged = true;
        guild.GetMaxSharedBattlePower();
        client.Guild.SaveArsenal();
    }

    /// <summary>
    ///     Removes inscription from a specific item, updating arsenal donation and shared battle power.
    /// </summary>
    private static void UniscribeItem(ConquerItem item, GameState client) {
        if (client.Entity.GuildID == 0 || client.Guild == null) return;
        var arsenalPosition = ArsenalPosition(item.ID);
        var arsenal = client.Guild.Arsenals[arsenalPosition];
        if (arsenal.ItemDictionary.TryGetValue(item.UID, out var arsenalItem))
            arsenal.RemoveItem(arsenalItem, client);
        item.Inscribed = false;
        item.Mode = Enums.ItemMode.Update;
        item.Send(client);
    }

    /// <summary>
    ///     Removes all inscriptions from player's items across inventory, equipment, and warehouses.
    /// </summary>
    public static void UniscribeAllItems(GameState client) {
        if (client.Guild == null) return;

        foreach (var item in client.Inventory.Objects) {
            if (item.Inscribed)
                UniscribeItem(item, client);
        }

        foreach (var item in client.Equipment.Objects) {
            if (item is { Inscribed: true })
                UniscribeItem(item, client);
        }

        foreach (var item in from wh in client.Warehouses.Values
                 from item in wh.Objects
                 where item.Inscribed
                 select item) {
            UniscribeItem(item, client);
        }

        client.Guild.ArsenalBpChanged = true;
        client.Guild.GetMaxSharedBattlePower();
        client.Guild.SaveArsenal();
    }
}