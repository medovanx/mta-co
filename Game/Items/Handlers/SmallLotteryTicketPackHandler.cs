using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles SmallLotteryTicketPack item that grants lottery tickets when used.
    /// </summary>
    [ItemHandler(SmallLotteryTicketPack)]
    public static class SmallLotteryTicketPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.Inventory.Add(SmallLotteryTicket, 0, 3);
        }
    }
}

