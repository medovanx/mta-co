using MTA.Client;
using MTA.Game.Items;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles PrayingStone items that grant Heaven Blessing and online training points.
    /// </summary>
    [ItemHandler(PrayingStone_S, PrayingStone_M, PrayingStone_L)]
    public static class PrayingStoneHandler {
        public static void Handle(GameState client, ConquerItem item) {
            var (days, points) = item.ID switch {
                PrayingStone_S => (3u, 10u), // 3 days, 10 points
                PrayingStone_M => (7u, 20u), // 7 days, 20 points
                PrayingStone_L => (30u, 30u), // 30 days, 30 points
                _ => (3u, 10u) // Default case (should not occur)
            };

            uint value = days * 24 * 60 * 60;
            client.OnlineTrainingPoints += points;
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddBless(value);
            client.Entity.Update(Update.OnlineTraining, client.OnlineTrainingPoints, false);
        }
    }
}

