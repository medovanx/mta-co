using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.GameConstants;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles SevenStarPouch item that gives 5 SevenStarOintments when used.
    /// </summary>
    [ItemHandler(SevenStarPouch)]
    public static class SevenStarPouchHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count <= 40) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.Inventory.Add(SevenStarOintment, 0, 5);
            }
            else {
                client.Send(FullInventory);
            }
        }
    }
}

