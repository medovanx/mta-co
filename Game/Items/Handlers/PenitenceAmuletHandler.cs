using MTA.Client;
using MTA.Game.Items;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles PenitenceAmulet items that decrease 30 PK points.
    /// </summary>
    [ItemHandler(PenitenceAmulet, PenitenceAmulet2)]
    public static class PenitenceAmuletHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.PKPoints >= 30) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.Entity.PKPoints -= 30;
            }
        }
    }
}

