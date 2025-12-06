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
        /// <summary>
        /// Give reward to a player
        /// </summary>
        public static void GiveReward(GameState client, string rewardType, object rewardData)
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
            }
        }

        /// <summary>
        /// Handle monster kill rewards for CP Castle event
        /// </summary>
        public static void OnMonsterKilled(GameState client, string monsterName, ushort mapId)
        {
            if (monsterName != "Captain")
                return;

            // Different rewards based on map
            if (mapId == MapConstants.CP_CASTLE_BEGINNER)
            {
                // Beginner Map: 500 CPs per Captain kill
                GiveReward(client, RewardTypes.CONQUER_POINTS, 500u);
            }
            else if (mapId == MapConstants.CP_CASTLE_ADVANCED)
            {
                // Advanced Map: 2,000 CPs per Captain kill
                GiveReward(client, RewardTypes.CONQUER_POINTS, 2000u);
            }
        }

        /// <summary>
        /// Reward type constants for CP Castle event
        /// </summary>
        public static class RewardTypes
        {
            public const string CONQUER_POINTS = "ConquerPoints";
        }
    }
}

