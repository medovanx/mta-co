using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using _String = MTA.Network.GamePackets._String;
using static MTA.Game.Constants.Items.BasicItems;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles StolenGift item that gives 10 CPs and shows an effect.
    /// </summary>
    [ItemHandler(StolenGift)]
    public static class StolenGiftHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.Delete);
            client.Entity.ConquerPoints += 10;
            var str4 = new _String(true) {
                UID = client.Entity.UID,
                TextsCount = 1,
                Type = _String.Effect
            };
            str4.Texts.Add("accession6");
            client.SendScreen(str4);

            client.Send(new Message("Congratulations! You have just found 10 CPs.",
                Color.Tan, Message.TopLeft));
        }
    }
}

