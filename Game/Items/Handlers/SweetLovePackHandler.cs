using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.GameConstants;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles SweetLovePack item that grants wedding items when used.
    /// </summary>
    [ItemHandler(SweetLovePack)]
    public static class SweetLovePackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count <= 38) {
                client.Inventory.Remove(item, Enums.ItemUse.Remove);
                client.Inventory.Add(RedRose, 0, 1);
                client.Inventory.Add(Kisses99, 0, 1);
            }
            else {
                client.Send(FullInventory);
            }
        }
    }
}

