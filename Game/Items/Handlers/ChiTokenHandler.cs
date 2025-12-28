using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles ChiToken item that grants rewards when 7 are stacked.
    /// </summary>
    [ItemHandler(ChiToken)]
    public static class ChiTokenHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (item.StackSize >= 7) {
                item.StackSize = 0;
                client.Inventory.Remove(item, Enums.ItemUse.Remove);
                client.Entity.ConquerPoints += 5000;
                client.Entity.SubClasses.StudyPoints += 2500;
                client.Entity.Money += 10000000;
                client.Send(new Message(
                    "Congratulations , you have got . 5000 Cps , 2500 StudyPoints and 10 Million Money",
                    Color.Tan, Message.TopLeft));
            }
            else {
                client.Send(new Message("You must have 7 ChiTokens stacked up Together.", Color.Tan,
                    Message.TopLeft));
            }
        }
    }
}

