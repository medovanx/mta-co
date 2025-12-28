using System.Drawing;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles PowerEXPBall items that grant 20% of current level EXP when used (level < 140).
    /// </summary>
    [ItemHandler(PowerEXPBall, PowerEXPBallBound)]
    public static class PowerEXPBallHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.Level < 140) {
                client.IncreaseExperience(((DataHolder.LevelExperience(client.Entity.Level) / 100) * 20), false);
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            }
            else {
                client.Send(new Message("You Already level 140, you do not need the EXP", Color.Red, Message.TopLeft));
            }
        }
    }
}

