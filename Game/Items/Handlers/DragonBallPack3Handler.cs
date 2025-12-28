using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.GameConstants;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles DragonBallPack(3) item that gives 3 dragon balls.
    /// </summary>
    [ItemHandler(DragonBallPack3)]
    public static class DragonBallPack3Handler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count <= 37) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.Inventory.Add(DragonBall, 0, 3);
            }
            else {
                client.Send(FullInventory);
            }
        }
    }
}

