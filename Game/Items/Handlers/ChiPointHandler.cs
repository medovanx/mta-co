using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Chi Point items that grant chi points when used.
    /// </summary>
    [ItemHandler(ChiPoint5, ChiPoint100, ChiPoint200, ChiPoint300, ChiPoint400)]
    public static class ChiPointHandler {
        public static void Handle(GameState client, ConquerItem item) {
            var chiPoints = item.ID switch {
                ChiPoint5 => 5u,
                ChiPoint100 => 100u,
                ChiPoint200 => 200u,
                ChiPoint300 => 300u,
                ChiPoint400 => 400u,
                _ => 0u
            };

            if (chiPoints <= 0) return;
            client.ChiPoints += chiPoints;
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
        }
    }
}