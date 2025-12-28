using System.Drawing;
using MTA.Client;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles MemoryAgate item that records locations (cannot be used in certain maps).
    /// </summary>
    [ItemHandler(MemoryAgate)]
    public static class MemoryAgateHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (GameConstants.revnomap.Contains(client.Entity.MapID) ||
                GameConstants.MemoryAgateNotAllowedMap.Contains(client.Entity.MapID)) {
                client.Send(new Message("You Can't record here !", Color.Tan, 0x7dc));
                return;
            }

            item.SendAgate(client);
        }
    }
}

