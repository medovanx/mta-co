using System;
using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Constants;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles SacredRefineryPack item that grants random sacred refinery materials when used.
    /// </summary>
    [ItemHandler(SacredRefineryPack)]
    public static class SacredRefineryPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count > 38) {
                return;
            }

            client.Inventory.Remove(item, Enums.ItemUse.Remove);

            var r = new Random();
            var nr = r.Next(1, 32);

            var materialId = nr switch {
                1 => SacredMDefenseMaterial1,
                2 => SacredMDefenseMaterial2,
                3 => SacredMDefenseMaterial3,
                4 => SacredMDefenseMaterial4,
                5 => SacredCriticalStrikeMaterial1,
                6 => SacredCriticalStrikeMaterial3,
                7 => SacredCriticalStrikeMaterial2,
                8 => SacredCriticalStrikeMaterial4,
                9 => SacredSkillCStrikeMaterial1,
                10 => SacredSkillCStrikeMaterial2,
                11 => SacredImmunityMaterial1,
                12 => SacredImmunityMaterial2,
                13 => SacredIntensificationMaterial,
                14 => SacredBreakthroughMaterial1,
                15 => SacredBreakthroughMaterial2,
                16 => SacredBreakthroughMaterial3,
                17 => SacredBreakthroughMaterial4,
                18 => SacredBreakthroughMaterial5,
                19 => SacredCounteractionMaterial1,
                20 => SacredCounteractionMaterial2,
                21 => SacredCounteractionMaterial3,
                22 => SacredDetoxicationMaterial1,
                23 => SacredDetoxicationMaterial2,
                24 => SacredDetoxicationMaterial3,
                25 => SacredDetoxicationMaterial4,
                26 => SacredDetoxicationMaterial5,
                27 => SacredBlockMaterial,
                28 => SacredBlockMaterial2,
                29 => SacredPenetrationMaterial1,
                30 => SacredPenetrationMaterial2,
                31 => SacredPenetrationMaterial3,
                _ => 0u
            };

            if (materialId != 0) {
                client.Inventory.Add(materialId, 0, 1);
            }
        }
    }
}