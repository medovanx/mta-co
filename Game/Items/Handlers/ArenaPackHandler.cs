using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Arena pack items that grant CPs and VIP points.
    /// </summary>
    [ItemHandler(ArenaEXPPack, ChampionPack)]
    public static class ArenaPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            switch (item.ID) {
                case ArenaEXPPack:
                    client.Entity.ConquerPoints += 1;
                    client.Entity.killerpoints += 1;
                    client.Send(new Message(
                        "Congratulations! " + client.Entity.Name + " You get " + 1 +
                        " ConquerPoints From Arena Pack and Get 1 Vip Point's .", Color.Red, Message.Whisper));
                    client.Inventory.Remove(item, Enums.ItemUse.Delete);
                    break;
                case ChampionPack:
                    client.Entity.ConquerPoints += 500;
                    client.Entity.killerpoints += 200;
                    client.Send(new Message(
                        "Congratulations! " + client.Entity.Name +
                        " You get 500 ConquerPoints From Arena Pack and Get 100 Vip Point's.", Color.Red,
                        Message.Whisper));
                    client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                    break;
            }
        }
    }
}

