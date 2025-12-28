using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.GameConstants;
using static MTA.Game.Constants.Items.BasicItems;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles SpeedArrowPack item that gives SpeedArrow when used.
    /// </summary>
    [ItemHandler(SpeedArrowPack)]
    public static class SpeedArrowPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count <= 35) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.Inventory.Add(SpeedArrow, 0, 1);
            }
            else {
                client.Send(FullInventory);
            }
        }
    }
}

