using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles GoldenExorcismPack item that grants rewards when stacked.
    /// </summary>
    [ItemHandler(GoldenExorcismPack)]
    public static class GoldenExorcismPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (item.StackSize >= 1) {
                item.StackSize = 0;
                client.Inventory.Remove(item, Enums.ItemUse.Remove);
                client.Entity.ConquerPoints += 500;
                client.Entity.SubClasses.StudyPoints += 50;
                client.Entity.Money += 2000000;
                client.Send(new Message(
                    "Congratulations , you have got . 500 Cps , 50 StudyPoints and 2 Million Money", Color.Tan,
                    Message.TopLeft));
            }
            else {
                client.Send(new Message("You must have 1 GoldenExorcismPack stacked up Together.", Color.Tan,
                    Message.TopLeft));
            }
        }
    }
}

