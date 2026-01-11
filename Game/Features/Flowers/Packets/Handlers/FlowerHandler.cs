using System;
using MTA.Client;
using MTA.Database;
using MTA.Game.Constants;
using MTA.Game.Features.Flowers.Database;
using MTA.Game.Features.Flowers.Packets.Writers;
using MTA.Network.PacketHandlers;
using static MTA.Kernel;

namespace MTA.Game.Features.Flowers.Packets.Handlers;

/// <summary>
///     Handles packet 1150 for sending flowers between players
/// </summary>
[PacketHandler(Constants.Packets.MsgFlower)]
public static class FlowerHandler {
    /// <summary>
    ///     Handles flower sending logic (packet 1150 with type 2)
    /// </summary>
    public static bool Handle(ushort packetId, byte[] packet, GameState caller) {
        // Check if this is a flower packet (type 2) - not a kiss packet (type 3)
        var packetType = System.BitConverter.ToUInt32(packet, 4);
        if (packetType != 2) return false; // Not a flower packet, let other handlers process it

        var flowerPacket = new FlowerPacket(false);
        flowerPacket.Deserialize(packet);

        if (!GamePool.TryGetValue(flowerPacket.Uid1, out GameState? value)) return false;
        if (caller.Entity.Level < 50) return false;
        if (value.Entity.Body == 1003 ||
            value.Entity.Body == 1004) return false; // Male

        if (flowerPacket.ItemUid == 0) {
            // Free flower (daily limit)
            if (caller.Entity.Flowers.LastFlowerSent == null)
                caller.Entity.Flowers.LastFlowerSent = DateTime.Now.Subtract(TimeSpan.FromDays(1));
            if (flowerPacket.FlowerType != FlowerType.RedRoses && flowerPacket.Amount < 1) return false;
            if (caller.Entity.Flowers.LastFlowerSent.AddDays(1) <= DateTime.Now) {
                caller.Entity.Flowers.LastFlowerSent = DateTime.Now;
                var newPacket = new FlowerPacket(true) {
                    SenderName = caller.Entity.Name,
                    ReceiverName = GamePool[flowerPacket.Uid1].Entity.Name,
                    SendAmount = 30,
                    SendFlowerType = FlowerType.RedRoses
                };
                GamePool[flowerPacket.Uid1].Send(newPacket);
                GamePool[flowerPacket.Uid1].Entity.Flowers.RedRoses += 30;
                GamePool[flowerPacket.Uid1].Entity.Flowers.RedRosesToday += 30;
                GamePool[flowerPacket.Uid1].Entity.Flowers.LastFlowerSent = DateTime.Now;
                FlowerTable.Save(GamePool[flowerPacket.Uid1]);
                FlowerTable.Save(caller);
                caller.Send(new FlowerPacket(caller.Entity.Flowers, caller));
            }
            else {
                caller.Send(GameConstants.OneFlowerADay);
            }
        }
        else {
            // Item-based flower
            if (!caller.Inventory.TryGetValue(flowerPacket.ItemUid, out var item)) return true;
            var flower = (item.ID / 1000) switch {
                751 => FlowerType.RedRoses,
                752 => FlowerType.Lilies,
                753 => FlowerType.Orchids,
                754 => FlowerType.Tulips,
                _ => FlowerType.Unknown
            };

            if (flower == FlowerType.Unknown) return true;
            switch (flower) {
                case FlowerType.RedRoses:
                    value.Entity.Flowers.RedRoses += item.Durability;
                    value.Entity.Flowers.RedRosesToday += item.Durability;
                    break;
                case FlowerType.Lilies:
                    value.Entity.Flowers.Lilies += item.Durability;
                    value.Entity.Flowers.Lilies2day += item.Durability;
                    break;
                case FlowerType.Orchids:
                    value.Entity.Flowers.Orchids += item.Durability;
                    value.Entity.Flowers.OrchidsToday += item.Durability;
                    break;
                case FlowerType.Tulips:
                    value.Entity.Flowers.Tulips += item.Durability;
                    value.Entity.Flowers.TulipsToday += item.Durability;
                    break;
            }

            var newPacket = new FlowerPacket(true) {
                SenderName = caller.Entity.Name,
                ReceiverName = value.Entity.Name,
                SendAmount = item.Durability,
                SendFlowerType = flower
            };
            value.Entity.Flowers.LastFlowerSent = DateTime.Now;
            FlowerTable.Save(value);
            value.Send(newPacket);
            caller.Inventory.Remove(item, Enums.ItemUse.Remove);
            ConquerItemTable.RemoveItem(item.UID);
        }

        return true;
    }
}