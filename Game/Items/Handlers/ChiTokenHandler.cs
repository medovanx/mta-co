using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.Constants.Items.QuestAndOther;

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
                client.Entity.SubClasses.StudyPoints += 2500;
                client.Send(new Message("Congratulations, you have got 25000 Study Points.",
                    Color.Tan, Message.TopLeft));
            }
            else {
                client.Send(new Message("You must have 7 Chi Tokens stacked up together to be able to exchange.",
                    Color.Tan, Message.TopLeft));
            }
        }
    }
}