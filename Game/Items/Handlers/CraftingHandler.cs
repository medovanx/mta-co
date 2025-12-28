using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles crafting items that transform into other items when used.
    /// </summary>
    [ItemHandler(EmptyBottle, CommonSoap, FlowerSoap, SoilPigment)]
    public static class CraftingHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (item.ID == EmptyBottle) {
                if (client.Inventory.Contains(EmptyBottle, 1)) {
                    client.Inventory.Remove(EmptyBottle, 1);
                    client.Inventory.Add(RiverWater, 0, 1);
                }

                var npc = new NpcReply(6, "Congratulations you got RiverWater.");
            }
            else if (item.ID == CommonSoap) {
                if (client.Inventory.Contains(CommonSoap, 1)) {
                    client.Inventory.Remove(CommonSoap, 1);
                    client.Inventory.Add(SoapPowder, 0, 1);
                }

                var npc = new NpcReply(6, "Congratulations you got SoapPowder.");
            }
            else if (item.ID == FlowerSoap) {
                if (client.Inventory.Contains(FlowerSoap, 1)) {
                    client.Inventory.Remove(FlowerSoap, 1);
                    client.Inventory.Add(FruitSoap, 0, 1);
                }

                var npc = new NpcReply(6, "Congratulations you got FruitSoap.");
            }
            else if (item.ID == SoilPigment) {
                if (client.Inventory.Contains(SoilPigment, 1) &&
                    client.Inventory.Contains(FlowerMudPigment, 1) &&
                    client.Inventory.Contains(MineralPigment, 1) &&
                    client.Inventory.Contains(FruitSoap, 1) &&
                    client.Inventory.Contains(SoapPowder, 1) &&
                    client.Inventory.Contains(BasicSoapyWater, 1)) {
                    client.Inventory.Remove(BasicSoapyWater, 1);
                    client.Inventory.Remove(SoilPigment, 1);
                    client.Inventory.Remove(FlowerMudPigment, 1);
                    client.Inventory.Remove(MineralPigment, 1);
                    client.Inventory.Remove(FruitSoap, 1);
                    client.Inventory.Remove(SoapPowder, 1);
                    client.Inventory.Add(BubbleWater, 0, 1);
                }

                var npc = new NpcReply(6, "Congratulations you got BubbleWater give it to TaoistYun.");
            }
        }
    }
}

