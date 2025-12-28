using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.GameConstants;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles MeteorScrollPack item that gives 7 meteor scrolls.
    /// </summary>
    [ItemHandler(MeteorScrollPack)]
    public static class MeteorScrollPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count <= 31) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.Inventory.Add(MeteorScroll, 0, 7);
            }
            else {
                client.Send(FullInventory);
            }
        }
    }
}

