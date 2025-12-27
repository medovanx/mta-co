using MTA.Client;
using MTA.Game.Items;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles PrayingStone(S) item that grants 3 days of Heaven Blessing and 10 online training points.
    /// </summary>
    [ItemHandler(PrayingStone_S)]
    public static class PrayingStoneSHandler {
        public static void Handle(GameState client, ConquerItem item) {
            uint value = 3 * 24 * 60 * 60;
            client.OnlineTrainingPoints += 10;
            client.AddBless(value);
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.Entity.Update(Update.OnlineTraining, client.OnlineTrainingPoints, false);
        }
    }
}

