using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Constants;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles MountPack items that grant mount armors when used.
    /// </summary>
    [ItemHandler(BlessedFancyAlpacaPack, JadeHarePack, ChaosBullPack, SilverBeastPack, GreenEyedBeastPack,
        RoyalApePack, PolarBearPack, RoaringChowPack, AuspiciousKylinPack, AncientElephantPack,
        GoldGlobefishPack, CelestialBirdPack, SaintDragonPack, WinebibberPandaPack, WildCamelPack,
        PegasusPack, IcePhoenixPack, FieryLionPack, TurkeyRunPack, FancyAlpacaPack)]
    public static class MountPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            // TurkeyRunPack and FancyAlpacaPack need inventory space check
            if (item.ID == TurkeyRunPack || item.ID == FancyAlpacaPack) {
                if (client.Inventory.Count <= 38) {
                    client.Inventory.Remove(item, Enums.ItemUse.Remove);
                    uint mountId = item.ID == TurkeyRunPack ? 200490u : 200499u;
                    client.Inventory.Add(mountId, 0, 1);
                }
                else {
                    client.Send(FullInventory);
                }

                return;
            }

            // Other mount packs
            var mountId = item.ID switch {
                BlessedFancyAlpacaPack => FancyAlpaca,
                JadeHarePack => JadeHare,
                ChaosBullPack => ChaosBull,
                SilverBeastPack => SilverBeast,
                GreenEyedBeastPack => GreenEyedBeast,
                RoyalApePack => RoyalApe,
                PolarBearPack => PolarBear,
                RoaringChowPack => RoaringChow,
                AuspiciousKylinPack => AuspiciousKylin,
                AncientElephantPack => AncientElephant,
                GoldGlobefishPack => GoldGlobefish,
                CelestialBirdPack => CelestialBird,
                SaintDragonPack => SaintDragon,
                WinebibberPandaPack => WinebibberPanda,
                WildCamelPack => WildCamel,
                PegasusPack => Pegasus,
                IcePhoenixPack => IcePhoenix,
                FieryLionPack => FieryLion,
                _ => 0u // Default case (should not occur)
            };

            client.Inventory.Add(mountId, 0, 1);
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
        }
    }
}

