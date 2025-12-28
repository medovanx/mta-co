using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles NinjaAmulet item that changes body size.
    /// </summary>
    [ItemHandler(NinjaAmulet)]
    public static class NinjaAmuletHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            switch (client.Entity.Body % 10) {
                case 2:
                case 4:
                    client.Entity.Body--;
                    break;
                case 1:
                case 3:
                    client.Entity.Body++;
                    break;
            }
        }
    }
}

