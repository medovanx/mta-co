using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.CPsBags;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Conquer Points bag items that grant CPs when used.
    /// </summary>
    [ItemHandler(HopeCPPack, HopeCPPack_2, MascotCPPack, CPBag10_2, MascotCPPack_2, MammonCPPack, FestivalCPPack, VioletCPPack, VioletCPPack_2, LegendCPPack,
        FlareCPPack, CuteCPPack, GhostCPPack, DreamCPPack, DeityCPPack, SoulCPPack, DeityCPPack_2, BloodCPPack, FlowerCPPack,
        CloudCPPack, PureCPPack, JoyCPPack, HeartCPPack, JewelCPPack, FogCPPack, StarCPPack, ShadowCPPack, MoonCPPack,
        MysticCPPack, EarthCPPack, LifeCPPack, FantasyCPPack)]
    public static class CPsBagHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (item.ID == CuteCPPack) {
                if (client.Entity.ConquerPoints <= 1000000050) {
                    client.Entity.ConquerPoints += 1000000000;
                    client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                }
                else {
                    client.MessageBox("You cannot have more than 1 billion CPs to open this.");
                }

                return;
            }

            var cps = item.ID switch {
                HopeCPPack or HopeCPPack_2 => 5u,
                MascotCPPack or CPBag10_2 => 10u,
                MascotCPPack_2 or MammonCPPack => 20u,
                FestivalCPPack => 25u,
                VioletCPPack or VioletCPPack_2 => 50u,
                LegendCPPack or FlareCPPack => 100u,
                GhostCPPack => 250u,
                DreamCPPack => 270u,
                DeityCPPack or SoulCPPack or DeityCPPack_2 => 500u,
                BloodCPPack or FlowerCPPack or CloudCPPack => 1000u,
                PureCPPack => 1350u,
                JoyCPPack => 1380u,
                HeartCPPack or JewelCPPack => 2000u,
                FogCPPack => 2500u,
                StarCPPack => 2700u,
                ShadowCPPack => 4000u,
                MoonCPPack => 5000u,
                MysticCPPack => 6900u,
                EarthCPPack => 10000u,
                LifeCPPack => 13500u,
                FantasyCPPack => 13800u,
                _ => 0u
            };

            if (cps <= 0) return;
            client.Entity.ConquerPoints += cps;
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
        }
    }
}
