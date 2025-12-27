using System;
using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;
using Update = MTA.Network.GamePackets.Update;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles ExpPotion item that grants double experience for 60 minutes.
    /// </summary>
    [ItemHandler(ExpPotion)]
    public static class ExpPotionHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.Entity.DoubleExperienceTime = 3600;
            client.Entity.DoubleExpStamp = Time32.Now;
            client.SuperPotion = 0;
            client.Entity.Update(Update.DoubleExpTimer, client.Entity.DoubleExperienceTime, 200, true);
        }
    }
}

