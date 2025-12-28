using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.QuestAndOther;
using static MTA.Game.Constants.Items.BasicItems;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Random Steed Pack item that grants a random +6 steed with sockets.
    /// </summary>
    [ItemHandler(RandomSteedPack)]
    public static class RandomSteedPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);

            var steed = new ConquerItem(true) {
                ID = Steed
            };
            var itemInfo = new ConquerItemInformation(steed.ID, 0);
            steed.Durability = steed.MaximDurability = itemInfo.BaseInformation.Durability;
            steed.Plus = 6;
            steed.Effect = Enums.ItemEffect.Horse;

            // Random socket configuration
            if (Kernel.Random.Sign() == 1)
                steed.SocketProgress = 150 << 8 | 255 << 16;
            else if (Kernel.Random.Sign() == 1)
                steed.SocketProgress = 150 | 255 << 8;
            else
                steed.SocketProgress = 255 | 150 << 16;

            client.Inventory.Add(steed, Enums.ItemUse.CreateAndAdd);
        }
    }
}

