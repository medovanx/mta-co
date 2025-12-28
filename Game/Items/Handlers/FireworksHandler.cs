using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles firework items that create visual effects when used.
    /// </summary>
    [ItemHandler(Firework, EndlessLoveFirework, MyWishFirework)]
    public static class FireworksHandler {
        public static void Handle(GameState client, ConquerItem item) {
            var effectName = item.ID switch {
                Firework => "firework-like", // Firework
                EndlessLoveFirework => "firework-1love", // Endless Love
                MyWishFirework => "firework-2love", // My Wish
                _ => "firework-2love" // Default case (should not occur)
            };

            client.Entity.Update(_String.Effect, effectName, true);
            client.Inventory.Remove(item, Enums.ItemUse.Remove);
        }
    }
}
