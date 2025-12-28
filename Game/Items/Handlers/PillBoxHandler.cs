using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Constants;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles PillBox items that grant 3 pills when used.
    /// </summary>
    [ItemHandler(Amrita, Panacea, Ginseng, Vanilla, RecoveryPill, SoulPill, RefreshingPill, ChantPill, MilGinseng)]
    public static class PillBoxHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count <= 38) {
                var pillId = item.ID switch {
                    Amrita => AmritaPill,
                    Panacea => PanaceaPill,
                    Ginseng => GinsengPill,
                    Vanilla => VanillaPill,
                    RecoveryPill => RecoveryPillPill,
                    SoulPill => SoulPillPill,
                    RefreshingPill => RefreshingPillPill,
                    ChantPill => ChantPillPill,
                    MilGinseng => MilGinsengPill,
                    _ => 0u // Default case (should not occur)
                };

                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                client.Inventory.Add(pillId, 0, 3);
            }
            else {
                client.Send(FullInventory);
            }
        }
    }
}

