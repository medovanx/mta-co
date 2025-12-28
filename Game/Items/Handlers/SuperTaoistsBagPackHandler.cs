using System.Drawing;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles SuperTaoist`sBagPack item that gives a Level 67 one Soc +5 Taoist`s Bag, small Praying Stone (B), EXP Ball (B), and 5 EXP Potions.
    /// </summary>
    [ItemHandler(SuperTaoistsBagPack)]
    public static class SuperTaoistsBagPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Level >= 10) {
                if (client.Inventory.Count < 32) {
                    client.Inventory.Add(PrayingStone_S, 0, 1);
                    client.Inventory.Add(ExpBall, 0, 1);
                    client.Inventory.Add(ExpPotion, 0, 5);
                    var items = new ConquerItem(true) {
                        ID = AmbergrisBag_Super,
                        Color = Enums.Color.White,
                        Plus = 5,
                        SocketOne = Enums.Gem.EmptySocket
                    };
                    items.Durability = items.MaximDurability =
                        ConquerItemInformation.BaseInformations[AmbergrisBag_Super].Durability;
                    client.Inventory.Add(items, Enums.ItemUse.CreateAndAdd);
                    client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                }
                else {
                    client.Send(new Message("You need to make at least 8 free spots in your inventory.",
                        Color.Red, Message.TopLeft));
                }
            }
            else {
                client.Send(new Message("You must be at least level 10 to open the Pack", Color.Red,
                    Message.TopLeft));
            }
        }
    }
}