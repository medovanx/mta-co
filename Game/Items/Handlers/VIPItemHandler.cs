using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles VIP item that sets player VIP level to 7.
    /// </summary>
    [ItemHandler(VIP7Item)]
    public static class VIPItemHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Entity.VIPLevel = 7;
            client.MessageBox("congratulations You get vip 7");
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
        }
    }
}

