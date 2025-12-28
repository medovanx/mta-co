using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.GameConstants;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles SerenityPouch item that gives 5 SerenityPills when used.
    /// </summary>
    [ItemHandler(SerenityPouch)]
    public static class SerenityPouchHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count <= 40) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.Inventory.Add(SerenityPill, 0, 5);
            }
            else {
                client.Send(FullInventory);
            }
        }
    }
}

