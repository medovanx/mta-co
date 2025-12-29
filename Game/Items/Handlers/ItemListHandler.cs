using MTA.Client;
using MTA.Network.GamePackets;
using QuestID = MTA.Network.GamePackets.QuestID;
using static MTA.Game.Constants.Items.QuestAndOther;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles ItemList item that shows quest information in a dialog.
    /// </summary>
    [ItemHandler(ItemList)]
    public static class ItemListHandler {
        public static void Handle(GameState client, ConquerItem item) {
            var dialog = new MTA.Npcs(client) {
                Client = client,
                Replies = []
            };
            var quest = client.Quests.GetQuest(QuestID.Eth_has_price);
            dialog.Text("************************");
            dialog.Text("Item : [" + quest.Mob + "]");
            dialog.Text("Amount : [" + quest.Kills + "]");
            dialog.Text("************************");
            dialog.Option("i see", 255);
            dialog.Send();
        }
    }
}