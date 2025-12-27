using System.Drawing;
using MTA.Client;
using MTA.Game.Items;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles ToughDrillPack item that gives a Tough Drill and a +3 Stone.
    /// </summary>
    [ItemHandler(ToughDrillPack)]
    public static class ToughDrillPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Level >= 10) {
                if (client.Inventory.Count < 38) {
                    client.Inventory.Add(ToughDrill, 0, 1); //ToughDrill
                    client.Inventory.Add(Stone_3, 3, 1); //+3
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

