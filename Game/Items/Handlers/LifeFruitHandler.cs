using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles LifeFruit item that restores HP and MP to maximum.
    /// </summary>
    [ItemHandler(LifeFruit)]
    public static class LifeFruitHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.Entity.Hitpoints = client.Entity.MaxHitpoints;
            client.Entity.Mana = client.Entity.MaxMana;
        }
    }
}

