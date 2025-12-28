using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.GameConstants;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles LifeFruitBasket item that gives 10 LifeFruits when used.
    /// </summary>
    [ItemHandler(LifeFruitBasket)]
    public static class LifeFruitBasketHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count <= 31) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.Inventory.Add(LifeFruit, 0, 10);
            }
            else {
                client.Send(FullInventory);
            }
        }
    }
}

