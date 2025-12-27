using MTA.Client;
using MTA.Game.Items;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles PrayingStone(L) item that grants 30 days of Heaven Blessing and 30 online training points.
    /// </summary>
    [ItemHandler(PrayingStone_L)]
    public static class PrayingStoneLHandler {
        public static void Handle(GameState client, ConquerItem item) {
            uint value = 30 * 24 * 60 * 60;
            client.OnlineTrainingPoints += 30;
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.AddBless(value);
            client.Entity.Update(Update.OnlineTraining, client.OnlineTrainingPoints, false);
        }
    }
}

