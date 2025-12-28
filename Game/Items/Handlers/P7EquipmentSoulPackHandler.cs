using System;
using MTA.Client;
using static MTA.Constants;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles P7EquipmentSoulPack item that grants random P7 equipment souls when used.
    /// </summary>
    [ItemHandler(P7EquipmentSoulPack)]
    public static class P7EquipmentSoulPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count > 38) {
                return;
            }

            client.Inventory.Remove(item, Enums.ItemUse.Remove);

            var r = new Random();
            var nr = r.Next(1, 17);

            uint soulId = nr switch {
                1 => MoonHeadgear,
                2 => SunHeadgear,
                3 => StarHeadgear,
                4 => IceHeadgear,
                5 => HeavenNecklace,
                6 => FervorBag,
                7 => NetherArmor,
                8 => EclipseArmor,
                9 => CraneRing,
                10 => DragonRing,
                11 => RainbowBracelet,
                12 => FoxBoots,
                13 => DragonBoots,
                14 => CraneBoots,
                15 => LionHeavyRing,
                16 => TigerHeavyRing,
                _ => 0u
            };

            if (soulId != 0) {
                client.Inventory.Add(soulId, 0, 1);
            }
        }
    }
}

