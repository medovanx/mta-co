using MTA.Client;
using MTA.Database;
using MTA.Game.ConquerStructures;
using MTA.Network.GamePackets;
using Data = MTA.Network.GamePackets.Data;
using MapStatus = MTA.Network.GamePackets.MapStatus;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles BoothsItem that opens a booth for the player.
    /// </summary>
    [ItemHandler(BoothsItem)]
    public static class BoothItemHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Booth != null) return;
            client.Send(new MapStatus() {
                BaseID = client.Map.BaseID,
                ID = client.Map.ID,
                Status = MapsTable.MapInformations[1036].Status
            });
            client.Booth = new Booth(client, new Data(true) { UID = client.Entity.UID });
            client.Send(new Data(true) {
                ID = Data.ChangeAction,
                UID = client.Entity.UID,
                dwParam = 0
            });
            // Item is not removed - it's reusable
        }
    }
}

