using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.ArenaItems;
using Message = MTA.Network.GamePackets.Message;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Arena pack items that grant CPs and VIP points.
    /// </summary>
    [ItemHandler(ArenaEXPPack, ChampionPack)]
    public static class ArenaPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            switch (item.ID) {
                case ArenaEXPPack:
                    client.Entity.Money += 100000;
                    client.Send(new Message(
                        "Congratulations! You have received 100,000 gold from the Arena Pack.",
                        Color.Red,
                        Message.Whisper));
                    client.Inventory.Remove(item, Enums.ItemUse.Delete);
                    break;
                case ChampionPack:
                    client.Entity.Money += 200000;
                    client.Send(new Message(
                        "Congratulations! You have received 200,000 gold from the Arena Pack.",
                        Color.Red,
                        Message.Whisper));
                    client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                    break;
            }
        }
    }
}