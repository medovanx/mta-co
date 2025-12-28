using MTA.Client;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles empty items that are simply removed when used.
    /// </summary>
    [ItemHandler(EmptyItem1, EmptyItem2)]
    public static class EmptyItemHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
        }
    }
}

