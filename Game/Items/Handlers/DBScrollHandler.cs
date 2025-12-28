using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Constants;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles DBScroll item that gives 10 dragon balls.
    /// </summary>
    [ItemHandler(DBScroll)]
    public static class DBScrollHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count <= 31) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.Inventory.Add(DragonBall, 0, 10);
            }
            else {
                client.Send(FullInventory);
            }
        }
    }
}

