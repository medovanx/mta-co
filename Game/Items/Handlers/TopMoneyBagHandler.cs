using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles TopMoneyBag item that gives 500000000 silvers.
    /// </summary>
    [ItemHandler(TopMoneyBag)]
    public static class TopMoneyBagHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.Entity.Money += 500000000;
        }
    }
}

