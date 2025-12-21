using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;

namespace MTA.Game.Events.CaptainsCastleConquest;

/// <summary>
///     Handles rewards for Captain's Castle Conquest event
/// </summary>
public static class CaptainsCastleConquestRewards {
    /// <summary>
    ///     Give reward to a player
    /// </summary>
    private static void GiveReward(GameState client, string rewardType, object rewardData) {
        switch (rewardType) {
            case RewardTypes.ConquerPoints:
                if (rewardData is uint cpAmount) {
                    client.Entity.ConquerPoints += cpAmount;
                    client.Send(new Message($"You received {cpAmount} Conquer Points!", Color.Black,
                        Message.TopLeft));
                }

                break;
        }
    }

    /// <summary>
    ///     Handle monster kill rewards for Captain's Castle Conquest event
    /// </summary>
    public static void OnMonsterKilled(GameState client, string monsterName, ushort mapId) {
        if (monsterName != "Captain")
            return;

        // Different rewards based on map
        if (mapId == MapConstants.CP_CASTLE_BEGINNER)
            // Beginner Map: 500 CPs per Captain kill
            GiveReward(client, RewardTypes.ConquerPoints, 500u);
        else if (mapId == MapConstants.CP_CASTLE_ADVANCED)
            // Advanced Map: 2,000 CPs per Captain kill
            GiveReward(client, RewardTypes.ConquerPoints, 2000u);
    }

    /// <summary>
    ///     Reward type constants for Captain's Castle Conquest event
    /// </summary>
    private static class RewardTypes {
        public const string ConquerPoints = "ConquerPoints";
    }
}

