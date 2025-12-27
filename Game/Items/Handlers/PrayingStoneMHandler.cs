using MTA.Client;
using MTA.Game.Items;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles PrayingStone(M) item that grants 7 days of Heaven Blessing and 20 online training points.
    /// </summary>
    [ItemHandler(PrayingStone_M)]
    public static class PrayingStoneMHandler {
        public static void Handle(GameState client, ConquerItem item) {
            uint value = 7 * 24 * 60 * 60;
            client.OnlineTrainingPoints += 20;
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddBless(value);
            client.Entity.Update(Update.OnlineTraining, client.OnlineTrainingPoints, false);
        }
    }
}

