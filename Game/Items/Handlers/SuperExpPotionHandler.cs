using System;
using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.BasicItems;
using Update = MTA.Network.GamePackets.Update;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles SuperExpPotion item that grants 5x experience for 2 hours.
    /// </summary>
    [ItemHandler(SuperExpPotion)]
    public static class SuperExpPotionHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.Remove);
            client.Entity.DoubleExperienceTime = 7200;
            client.Entity.DoubleExpStamp = Time32.Now;
            client.SuperPotion = 5;
            client.Entity.Update(Update.DoubleExpTimer, client.Entity.DoubleExperienceTime, 500, true);
        }
    }
}
