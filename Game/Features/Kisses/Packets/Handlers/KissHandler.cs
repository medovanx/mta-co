using System;
using MTA.Client;
using MTA.Database;
using MTA.Game.Constants;
using MTA.Game.Features.Kisses.Database;
using MTA.Game.Features.Kisses.Packets.Writers;
using MTA.Network.PacketHandlers;
using static MTA.Kernel;

namespace MTA.Game.Features.Kisses.Packets.Handlers;

/// <summary>
///     Handles packet 1150 with sub-type 3 for sending kisses between players
/// </summary>
[PacketHandler(Constants.Packets.MsgFlower)]
public static class KissHandler {
    /// <summary>
    ///     Handles kiss sending logic
    /// </summary>
    public static bool Handle(ushort packetId, byte[] packet, GameState caller) {
        // Check if this is a kiss packet (sub-type 3)
        var subType = System.BitConverter.ToUInt32(packet, 4);
        if (subType != 3) return false; // Not a kiss packet, let other handlers process it

        var kissPacket = new KissPacket(false);
        kissPacket.Deserialize(packet);

        if (!GamePool.TryGetValue(kissPacket.UID1, out var value)) return false;
        if (caller.Entity.Level < 50) return false;
        if (caller.Entity.Body is 2003 or 2004) return false; // Female
        if (value.Entity.Body is 1001 or 1002) return false; // Male

        if (kissPacket.ItemUID == 0) {
            // Free kiss (daily limit)
            if (kissPacket.KissesType != KissType.Kisses && kissPacket.Amount != 1) return false;
            if (caller.Entity.Kisses.LastKissesSent.AddDays(1) <= DateTime.Now) {
                caller.Entity.Kisses.LastKissesSent = DateTime.Now;
                var newPacket = new KissPacket(true) {
                    sub = 1,
                    SenderName = caller.Entity.Name,
                    ReceiverName = GamePool[kissPacket.UID1].Entity.Name,
                    SendAmount = 1,
                    SendKissesType = KissType.Kisses,
                    remove = 1
                };
                GamePool[kissPacket.UID1].SendScreen(newPacket);
                GamePool[kissPacket.UID1].Entity.Kisses.Kisses2++;
                GamePool[kissPacket.UID1].Entity.Kisses.Kisses2day++;
                KissTable.Save(GamePool[kissPacket.UID1]);
                KissTable.Save(caller);
            }
            else {
                caller.Send(GameConstants.OneKissADay);
            }
        }
        else {
            // Item-based kiss
            if (!caller.Inventory.TryGetValue(kissPacket.ItemUID, out var item)) return false;

            var kisses = (item.ID / 1000) switch {
                755 => KissType.Kisses,
                756 => KissType.Letters,
                757 => KissType.Wine,
                758 => KissType.Jades,
                _ => KissType.Unknown
            };

            switch (kisses) {
                case KissType.Unknown:
                    return false;
                case KissType.Kisses:
                    GamePool[kissPacket.UID1].Entity.Kisses.Kisses2 += item.Durability;
                    GamePool[kissPacket.UID1].Entity.Kisses.Kisses2day += item.Durability;
                    break;
                case KissType.Letters:
                    GamePool[kissPacket.UID1].Entity.Kisses.Letters1 += item.Durability;
                    GamePool[kissPacket.UID1].Entity.Kisses.LetterToday1 += item.Durability;
                    break;
                case KissType.Wine:
                    GamePool[kissPacket.UID1].Entity.Kisses.Wine += item.Durability;
                    GamePool[kissPacket.UID1].Entity.Kisses.Wine2day += item.Durability;
                    break;
                case KissType.Jades:
                    GamePool[kissPacket.UID1].Entity.Kisses.Jades += item.Durability;
                    GamePool[kissPacket.UID1].Entity.Kisses.Jades2day += item.Durability;
                    break;
            }

            var newPacket = new KissPacket(true) {
                SenderName = caller.Entity.Name,
                ReceiverName = GamePool[kissPacket.UID1].Entity.Name,
                SendAmount = item.Durability,
                SendKissesType = kisses
            };
            KissTable.Save(GamePool[kissPacket.UID1]);
            GamePool[kissPacket.UID1].SendScreen(newPacket);
            caller.Inventory.Remove(item, Enums.ItemUse.Remove);
            ConquerItemTable.RemoveItem(item.UID);
        }

        return true;
    }
}