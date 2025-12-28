using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.GameConstants;
using static MTA.Game.Constants.Items.BasicItems;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Dragon Ball Pack (5) item that gives 5 dragon balls when used.
    /// </summary>
    [ItemHandler(DragonBallPack)]
    public static class DragonBallPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count <= 35) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.Inventory.Add(DragonBall, 0, 5);
            }
            else {
                client.Send(FullInventory);
            }
        }
    }
}
