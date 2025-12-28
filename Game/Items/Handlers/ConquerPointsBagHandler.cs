using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.ConquerPointsBags;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Conquer Points bag items that grant CPs when used.
    /// </summary>
    [ItemHandler(CPBag5, CPBag5_2, CPBag10, CPBag10_2, CPBag20, CPBag20_2, CPBag25, CPBag50, CPBag50_2, CPBag100,
        CPBag100_2, CPBag1Billion, CPBag250, CPBag270, CPBag500, CPBag500_2, CPBag500_3, CPBag1000, CPBag1000_2,
        CPBag1000_3, CPBag1350, CPBag1380, CPBag2000, CPBag2000_2, CPBag2500, CPBag2700, CPBag4000, CPBag5000,
        CPBag6900, CPBag10000, CPBag13500, CPBag13800)]
    public static class ConquerPointsBagHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (item.ID == CPBag1Billion) {
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
                CPBag5 or CPBag5_2 => 5u,
                CPBag10 or CPBag10_2 => 10u,
                CPBag20 or CPBag20_2 => 20u,
                CPBag25 => 25u,
                CPBag50 or CPBag50_2 => 50u,
                CPBag100 or CPBag100_2 => 100u,
                CPBag250 => 250u,
                CPBag270 => 270u,
                CPBag500 or CPBag500_2 or CPBag500_3 => 500u,
                CPBag1000 or CPBag1000_2 or CPBag1000_3 => 1000u,
                CPBag1350 => 1350u,
                CPBag1380 => 1380u,
                CPBag2000 or CPBag2000_2 => 2000u,
                CPBag2500 => 2500u,
                CPBag2700 => 2700u,
                CPBag4000 => 4000u,
                CPBag5000 => 5000u,
                CPBag6900 => 6900u,
                CPBag10000 => 10000u,
                CPBag13500 => 13500u,
                CPBag13800 => 13800u,
                _ => 0u
            };

            if (cps <= 0) return;
            client.Entity.ConquerPoints += cps;
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
        }
    }
}
