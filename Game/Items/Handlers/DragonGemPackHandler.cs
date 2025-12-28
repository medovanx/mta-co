using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.GameConstants;
using static MTA.Game.Constants.Items.BasicItems;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles DragonGemPack item that gives 5 dragon gems.
    /// </summary>
    [ItemHandler(DragonGemPack)]
    public static class DragonGemPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count <= 31) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.Inventory.Add(DragonGem, 0, 5);
            }
            else {
                client.Send(FullInventory);
            }
        }
    }
}

