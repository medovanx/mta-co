using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.GameConstants;
using static MTA.Game.Constants.Items.BasicItems;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles MeteorTearPack item that gives 5 MeteorTears when used.
    /// </summary>
    [ItemHandler(MeteorTearPack)]
    public static class MeteorTearPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count <= 36) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.Inventory.Add(MeteorTear, 0, 5);
            }
            else {
                client.Send(FullInventory);
            }
        }
    }
}

