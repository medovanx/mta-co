using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.GameConstants;
using static MTA.Game.Constants.Items.BasicItems;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles DragonBallPack(11) item that gives 11 dragon balls.
    /// </summary>
    [ItemHandler(DragonBallPack11)]
    public static class DragonBallPack11Handler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count <= 29) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.Inventory.Add(DragonBall, 0, 11);
            }
            else {
                client.Send(FullInventory);
            }
        }
    }
}

