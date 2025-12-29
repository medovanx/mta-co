using System;
using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.BasicItems;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles MoonBox items that grants a random gift when used.
    /// </summary>
    [ItemHandler(MoonBox)]
    public static class MoonBoxHandler {
        public static void Handle(GameState client, ConquerItem item) {
            var random = new Random();
            var gift = random.Next(1, 100);
            switch (gift) {
                case <= 50:
                    client.Entity.Money += 100000;
                    client.MessageBox("You have received 100,000 gold from the Moon Box.");
                    break;
                case > 50:
                    client.Inventory.Add(Meteor, 0, 1);
                    break;
            }
        }
    }
}