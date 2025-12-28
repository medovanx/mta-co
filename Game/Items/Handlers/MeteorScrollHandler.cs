using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.GameConstants;
using static MTA.Game.Constants.Items.BasicItems;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles MeteorScroll item that gives 10 meteors.
    /// </summary>
    [ItemHandler(MeteorScroll)]
    public static class MeteorScrollHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count <= 31) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.Inventory.Add(Meteor, 0, 10);
            }
            else {
                client.Send(FullInventory);
            }
        }
    }
}

