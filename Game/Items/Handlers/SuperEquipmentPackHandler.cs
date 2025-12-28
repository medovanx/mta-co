using System.Collections.Generic;
using System.Drawing;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Super Equipment Pack items that grant equipment, PrayingStone, ExpBall, and ExpPotion when used.
    /// </summary>
    [ItemHandler(SuperWarriorsArmorPack, SuperWarriorsHelmetPack, SuperTrojansArmorPack, SuperTrojansCoronetPack,
        SuperNinjasVestPack, SuperNinjasVeilPack, SuperTaoistsRobePack)]
    public static class SuperEquipmentPackHandler {
        private const byte RequiredLevel = 10;
        private const byte RequiredInventorySlots = 32; // Need at least 8 free slots
        private const byte ExpPotionCount = 5;

        private static readonly Dictionary<uint, uint> PackToEquipment = new Dictionary<uint, uint> {
            { SuperWarriorsArmorPack, LightArmor_Super },
            { SuperWarriorsHelmetPack, GoldHelmet_Super },
            { SuperTrojansArmorPack, RageArmor_Super },
            { SuperTrojansCoronetPack, WarCoronet_Super },
            { SuperNinjasVestPack, TigerVest_Super },
            { SuperNinjasVeilPack, BloodVeil_Super },
            { SuperTaoistsRobePack, CraneVestment_Super }
        };

        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Level < RequiredLevel) {
                client.Send(new Message("You must be atleast level 10 to open the Pack", Color.Red,
                    Message.TopLeft));
                return;
            }

            if (client.Inventory.Count >= RequiredInventorySlots) {
                client.Send(new Message("You need to make atleast 8 free spots in your inventory.",
                    Color.Red, Message.TopLeft));
                return;
            }

            if (!PackToEquipment.TryGetValue(item.ID, out var equipmentId)) {
                return;
            }

            // Add bonus items
            client.Inventory.Add(PrayingStone_S, 0, 1);
            client.Inventory.Add(ExpBall_B, 0, 1);
            client.Inventory.Add(ExpPotion, 0, ExpPotionCount);

            // Create equipment item
            var equipment = new ConquerItem(true) {
                ID = equipmentId,
                Color = Enums.Color.White,
                Plus = 5,
                SocketOne = Enums.Gem.EmptySocket
            };
            equipment.Durability = equipment.MaximDurability =
                ConquerItemInformation.BaseInformations[equipmentId].Durability;
            client.Inventory.Add(equipment, Enums.ItemUse.CreateAndAdd);

            // Remove the pack
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
        }
    }
}