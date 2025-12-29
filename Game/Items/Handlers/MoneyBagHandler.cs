using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.MoneyBags;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles MoneyBag items that grant silvers when used.
    /// </summary>
    [ItemHandler(Class1MoneyBag, Class2MoneyBag, Class3MoneyBag, Class4MoneyBag, Class5MoneyBag,
        Class6MoneyBag, Class7MoneyBag, Class8MoneyBag, Class9MoneyBag, Class10MoneyBag, TopMoneyBag)]
    public static class MoneyBagHandler {
        public static void Handle(GameState client, ConquerItem item) {
            var amount = item.ID switch {
                Class1MoneyBag => 300000u,
                Class2MoneyBag => 800000u,
                Class3MoneyBag => 1200000u,
                Class4MoneyBag => 1800000u,
                Class5MoneyBag => 5000000u,
                Class6MoneyBag => 20000000u,
                Class7MoneyBag => 25000000u,
                Class8MoneyBag => 80000000u,
                Class9MoneyBag => 100000000u,
                Class10MoneyBag => 300000000u,
                TopMoneyBag => 500000000u,
                _ => 0u // Default case (should not occur)
            };

            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            client.Entity.Money += amount;
        }
    }
}