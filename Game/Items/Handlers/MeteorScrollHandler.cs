using MTA.Client;
using MTA.Game.Items;
using MTA.Network.GamePackets;
using static MTA.Constants;
using static MTA.Game.ItemConstants;

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

