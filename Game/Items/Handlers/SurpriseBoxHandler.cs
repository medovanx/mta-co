using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles SurpriseBox item that grants Conquer Points when used.
    /// </summary>
    [ItemHandler(SurpriseBox)]
    public static class SurpriseBoxHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count <= 38) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.Entity.ConquerPoints += 2;
            }
            else {
                client.Send(new Message("You Must have 1 space in you Inventory To Open You Box", Color.White, 255));
            }
        }
    }
}

