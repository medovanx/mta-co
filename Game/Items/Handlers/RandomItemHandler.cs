using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles random item pack items that grant random items when used.
    /// </summary>
    [ItemHandler(RandomSuperItemPack, RandomItemPack350k)]
    public static class RandomItemHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);

            if (item.ID == RandomSuperItemPack) {
                // Random Super item (ID range 724130-724499 with "Super" in name)
                var array = ConquerItemInformation.BaseInformations.Values
                    .Where(p => p.ID is >= 724130 and <= 724499 && p.Name.Contains("Super")).ToArray();
                if (array.Length > 0) {
                    client.Inventory.Add(array[Kernel.Random.Next(array.Length)].ID, 0, 1);
                }
            }
            else if (item.ID == RandomItemPack350k) {
                // Random item (ID range 350001-380030)
                var array = ConquerItemInformation.BaseInformations.Keys
                    .Where(p => p is >= 350001 and <= 380030).ToArray();
                if (array.Length > 0) {
                    client.Inventory.Add(array[Kernel.Random.Next(array.Length)], 0, 1);
                }
            }
        }
    }
}

