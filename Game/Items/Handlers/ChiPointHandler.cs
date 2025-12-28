using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.ChiItems;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Chi Point items that grant chi points when used.
    /// </summary>
    [ItemHandler(SmallChiPill, ChiPill_100, ChiPill_200, ChiPill_300, ChiPill_400)]
    public static class ChiPointHandler {
        public static void Handle(GameState client, ConquerItem item) {
            var chiPoints = item.ID switch {
                SmallChiPill => 5u,
                ChiPill_100 => 100u,
                ChiPill_200 => 200u,
                ChiPill_300 => 300u,
                ChiPill_400 => 400u,
                _ => 0u
            };

            if (chiPoints <= 0) return;
            client.ChiPoints += chiPoints;
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
        }
    }
}
