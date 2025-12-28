using MTA.Client;
using MTA.Game.Items;
using MTA.Network.GamePackets;
using static MTA.Constants;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles ExpBall_B item that grants experience when used (max 10 per day, level < 137).
    /// </summary>
    [ItemHandler(ExpBall_B)]
    public static class ExpBallHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.ExpBalls < 10) {
                if (client.Entity.Level < 137) {
                    client.IncreaseExperience(client.ExpBall, false);
                    client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                    client.ExpBalls++;
                }
            }
            else {
                client.Send(ExpBallsUsed);
            }
        }
    }
}

