using MTA.Client;
using MTA.Network.GamePackets;
using System.Drawing;

namespace MTA.Game.Events.CpCastle
{
    /// <summary>
    /// Handles rewards for CP Castle event
    /// </summary>
    public class CpCastleRewards
    {
        private const uint DRAGON_BALL_ITEM_ID = 729122;

        /// <summary>
        /// Give reward to a player
        /// </summary>
        public static void GiveReward(Client.GameState client, string rewardType, object rewardData)
        {
            switch (rewardType)
            {
                case RewardTypes.CONQUER_POINTS:
                    if (rewardData is uint cpAmount)
                    {
                        client.Entity.ConquerPoints += cpAmount;
                        client.Send(new Message($"You received {cpAmount} Conquer Points!", Color.Black, Message.TopLeft));
                    }
                    break;

                case RewardTypes.DRAGON_BALLS:
                    if (rewardData is uint ballAmount && ballAmount <= byte.MaxValue)
                    {
                        client.Inventory.Add(DRAGON_BALL_ITEM_ID, 0, (byte)ballAmount);
                        client.Send(new Message($"You received {ballAmount} Dragon Ball(s)!", Color.Black, Message.TopLeft));
                    }
                    break;
            }
        }

        /// <summary>
        /// Handle monster kill rewards for CP Castle event
        /// Called when a Captain monster is killed in map 3033
        /// </summary>
        public static void OnMonsterKilled(Client.GameState client, string monsterName, ushort mapId)
        {
            // Only handle Captain monsters in CP Castle map (3033)
            if (mapId == 3033 && monsterName == "Captain")
            {
                // Give rewards for killing Captain
                GiveReward(client, RewardTypes.CONQUER_POINTS, 1000u);
            }
        }

        /// <summary>
        /// Reward type constants for CP Castle event
        /// </summary>
        public static class RewardTypes
        {
            public const string CONQUER_POINTS = "ConquerPoints";
            public const string DRAGON_BALLS = "DragonBalls";
        }
    }
}

