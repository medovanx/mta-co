using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Constants;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles teleport gate items that teleport players to specific locations.
    /// </summary>
    [ItemHandler(TwinCityGate, DesertCityGate, ApeCityGate, CastleGate, BirdIslandGate, ArroyoScroll)]
    public static class GateHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.MapID == 601) return;

            if (client.Map.BaseID is 6000 or 6001 or 1844 or 1801 or 8883 ||
                client.Map.BaseID == 1005 && client.Entity.MapID != 1005 || client.Map.BaseID == 700) {
                client.Send(JailItemUnusable);
                return;
            }

            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);

            if (item.ID == TwinCityGate) {
                client.Entity.Teleport(1002, 303, 278);
            }
            else if (item.ID == DesertCityGate) {
                client.Entity.Teleport(1000, 500, 650);
            }
            else if (item.ID == ApeCityGate) {
                client.Entity.Teleport(1020, 565, 562);
            }
            else if (item.ID == CastleGate) {
                client.Entity.Teleport(1011, 188, 264);
            }
            else if (item.ID == BirdIslandGate) {
                client.Entity.Teleport(1015, 717, 571);
            }
            else if (item.ID == ArroyoScroll) {
                client.Entity.Teleport(1217, 535, 558);
            }
        }
    }
}

