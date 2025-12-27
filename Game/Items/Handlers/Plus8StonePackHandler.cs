using System.Drawing;
using MTA.Client;
using MTA.Game.Items;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles +8StonePack item that gives a +8 Stone and a +6 Stone.
    /// </summary>
    [ItemHandler(Plus8StonePack)]
    public static class Plus8StonePackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Level >= 10) {
                if (client.Inventory.Count < 38) {
                    client.Inventory.Add(Stone_8, 8, 1); //+8
                    client.Inventory.Add(Stone_6, 6, 1); //+6
                    client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                }
                else {
                    client.Send(new Message("You need to make atleast 2 free spots in your inventory.",
                        Color.Red, Message.TopLeft));
                }
            }
            else {
                client.Send(new Message("You must be atleast level 10 to open the Pack", Color.Red,
                    Message.TopLeft));
            }
        }
    }
}

