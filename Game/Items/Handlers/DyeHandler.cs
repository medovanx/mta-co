using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.BasicItems;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles hair dye items that change hair color.
    /// </summary>
    [ItemHandler(BlackDye, VioletDye, BlueDye, GreenDye, BrownDye, RedDye, WhiteDye)]
    public static class DyeHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.Entity.HairColor = item.ID switch {
                BlackDye => 3,
                VioletDye => 9,
                BlueDye => 8,
                GreenDye => 7,
                BrownDye => 6,
                RedDye => 5,
                WhiteDye => 4,
                _ => 3 // Default to black (should not occur)
            };
        }
    }
}

