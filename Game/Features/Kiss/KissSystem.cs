using System;
using MTA.Client;
using MTA.Database;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using static MTA.Kernel;

namespace MTA.Game.Features.Kiss {
    public class KissSystem {
        public KissSystem(byte[] basePacket, GameState caller) {
            var packet = new KissPacket(false);
            packet.Deserialize(basePacket);
            if (!GamePool.TryGetValue(packet.UID1, out var value)) return;
            if (caller.Entity.Level < 50) return;
            if (caller.Entity.Body is 2003 or 2004) return;
            if (value.Entity.Body is 1001 or 1002) return;
            if (packet.ItemUID == 0) {
                if (packet.KissesType != KissType.Kisses && packet.Amount != 1) return;
                if (caller.Entity.Kisses.LastKissesSent.AddDays(1) <= DateTime.Now) {
                    caller.Entity.Kisses.LastKissesSent = DateTime.Now;
                    var newPacket = new KissPacket(true) {
                        sub = 1,
                        SenderName = caller.Entity.Name,
                        ReceiverName = GamePool[packet.UID1].Entity.Name,
                        SendAmount = 1,
                        SendKissesType = KissType.Kisses,
                        remove = 1
                    };
                    GamePool[packet.UID1].SendScreen(newPacket);
                    GamePool[packet.UID1].Entity.Kisses.Kisses2++;
                    GamePool[packet.UID1].Entity.Kisses.Kisses2day++;
                    KissSystemTable.SaveKissTable(GamePool[packet.UID1]);
                    KissSystemTable.SaveKissTable(caller);
                }
                else
                    caller.Send(GameConstants.OneKissADay);
            }
            else {
                if (!caller.Inventory.TryGetValue(packet.ItemUID, out var item)) return;
                var kisses = (item.ID / 1000) switch {
                    755 => KissType.Kisses,
                    756 => KissType.Letters,
                    757 => KissType.Wine,
                    758 => KissType.Jades,
                    _ => KissType.Unknown
                };

                switch (kisses) {
                    case KissType.Unknown:
                        return;
                    case KissType.Kisses:
                        GamePool[packet.UID1].Entity.Kisses.Kisses2 += item.Durability;
                        GamePool[packet.UID1].Entity.Kisses.Kisses2day += item.Durability;
                        break;
                    case KissType.Letters:
                        GamePool[packet.UID1].Entity.Kisses.Letters1 += item.Durability;
                        GamePool[packet.UID1].Entity.Kisses.LetterToday1 += item.Durability;
                        break;
                    case KissType.Wine:
                        GamePool[packet.UID1].Entity.Kisses.Wine += item.Durability;
                        GamePool[packet.UID1].Entity.Kisses.Wine2day += item.Durability;
                        break;
                    case KissType.Jades:
                        GamePool[packet.UID1].Entity.Kisses.Jades += item.Durability;
                        GamePool[packet.UID1].Entity.Kisses.Jades2day += item.Durability;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var newPacket = new KissPacket(true) {
                    SenderName = caller.Entity.Name,
                    ReceiverName = GamePool[packet.UID1].Entity.Name,
                    SendAmount = item.Durability,
                    SendKissesType = kisses
                };
                KissSystemTable.SaveKissTable(GamePool[packet.UID1]);
                GamePool[packet.UID1].SendScreen(newPacket);
                caller.Inventory.Remove(item, Enums.ItemUse.Remove);
                ConquerItemTable.RemoveItem(item.UID);
            }
        }
    }
}