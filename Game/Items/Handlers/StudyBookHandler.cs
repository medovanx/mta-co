using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using _String = MTA.Network.GamePackets._String;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles study point book items that grant study points when used.
    /// </summary>
    [ItemHandler(DiligenceBook, ModestyBook, EnduranceBook)]
    public static class StudyBookHandler {
        public static void Handle(GameState client, ConquerItem item) {
            uint studyPoints = 0;
            string message = "";

            if (item.ID == DiligenceBook) {
                studyPoints = 5;
                message = "Congratulations you got 5 study Points keep going";
            }
            else if (item.ID == ModestyBook) {
                studyPoints = 500;
                message = "Congratulations you got 500 study Points keep going";
            }
            else if (item.ID == EnduranceBook) {
                studyPoints = 20;
                message = "Congratulations you got 50 study Points keep going";
            }

            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.Entity.SubClasses.StudyPoints += (int)studyPoints;
            client.Send(new SubClassShow() {
                ID = 8,
                Study = client.Entity.SubClasses.StudyPoints,
                StudyReceive = (int)studyPoints
            }.ToArray());
            var str = new _String(true) {
                Type = 10,
                UID = client.Entity.UID
            };
            str.Texts.Add("zf2-e300");
            client.SendScreen(str.ToArray());

            if (item.ID == DiligenceBook) {
                client.Inventory.Add(723903, 0, 10);
            }

            client.Send(new Message(message, Color.Red, Message.TopLeft));
        }
    }
}
