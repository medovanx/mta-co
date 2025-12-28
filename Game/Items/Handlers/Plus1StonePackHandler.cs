using MTA.Client;
using MTA.Game.Items;
using MTA.Network.GamePackets;
using static MTA.Constants;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Plus1StonePack item that gives 5 +1Stones when used.
    /// </summary>
    [ItemHandler(Plus1StonePack)]
    public static class Plus1StonePackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count <= 36) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.Inventory.Add(Stone_1, 1, 5);
            }
            else {
                client.Send(FullInventory);
            }
        }
    }
}

