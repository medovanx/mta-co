using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.BasicItems;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles BlackTulip item that dyes armor black. This does not work for Ninja Vest.
    /// </summary>
    [ItemHandler(BlackTulip)]
    public static class BlackTulipHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Equipment.TryGetItem(3).ID == 0)
                return;
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.Equipment.TryGetItem(3).Color = Enums.Color.Black;
            ConquerItemTable.UpdateColor(client.Equipment.TryGetItem(3));
            client.Equipment.TryGetItem(3).Mode = Enums.ItemMode.Update;
            client.Equipment.TryGetItem(3).Send(client);
            client.Equipment.UpdateEntityPacket();
        }
    }
}