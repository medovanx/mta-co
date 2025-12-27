using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Study Book [Arena] item that grants study points.
    /// </summary>
    [ItemHandler(StudyBook_Arena)]
    public static class StudyBookHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.Entity.SubClasses.StudyPoints += 50;
            client.Send(new SubClassShow()
                { ID = 8, Study = client.Entity.SubClasses.StudyPoints, StudyReceive = 50 }.ToArray());
            var str = new _String(true) {
                Type = 10,
                UID = client.Entity.UID
            };
            str.Texts.Add("zf2-e300");
            client.SendScreen(str.ToArray());
        }
    }
}