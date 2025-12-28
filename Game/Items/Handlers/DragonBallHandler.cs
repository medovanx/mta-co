using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles DragonBall item that grants CPs based on its worth.
    /// </summary>
    [ItemHandler(DragonBall)]
    public static class DragonBallHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Entity.ConquerPoints +=
                ConquerItemInformation.BaseInformations[DragonBall].ConquerPointsWorth;
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
        }
    }
}

