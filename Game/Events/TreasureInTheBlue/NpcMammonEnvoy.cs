using System;
using MTA.Client;
using MTA.Database;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;

namespace MTA.Game.Events.TreasureInTheBlue;

/// <summary>
/// Mammon Envoy NPC - Exchanges event coins for rewards in the Treasure in the Blue event
/// </summary>
/// <event>Treasure in the Blue</event>
/// <npc>Mammon Envoy</npc>
/// <description>Exchanges event coins for rewards</description>
[NpcHandler(115522011)]
public static class NpcMammonEnvoy {
    private static readonly Random Random = new();

    // Reward item IDs and odds per coin type
    private static readonly (uint itemId, double weight)[] CopperCoinRewards = [
        (ItemConstants.Meteor, 0.50),        
        (ItemConstants.Class1MoneyBag, 0.25),
        (ItemConstants.SmallJoyStone, 0.60),
        (ItemConstants.ExpBallScrap, 0.60)   
    ];

    private static readonly (uint itemId, double weight)[] SilverCoinRewards = [
        (ItemConstants.EnduranceBook, 0.35),
        (ItemConstants.Class2MoneyBag, 0.15),
        (ItemConstants.HorseRacingPointsPack3K, 0.40),
        (ItemConstants.ExpBallScrap, 0.50),  
        (ItemConstants.SmallJoyStone, 0.80)  
    ];

    private static readonly (uint itemId, double weight)[] GoldCoinRewards = [
        (ItemConstants.SmallLotteryTicket, 0.60),
        (ItemConstants.JadeHare, 0.05),
        (ItemConstants.CelestialBird, 0.25),    
        (ItemConstants.GreenEyedBeast, 0.25)    
    ];

    public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
        var treasureEvent = (TreasureInTheBlueEvent)EventScheduler.GetEvent("TREASURE_IN_THE_BLUE")!;
        var coinTracker = treasureEvent.CoinTracker;

        switch (npcRequest.OptionID) {
            case 0: {
                var (copperLeft, silverLeft, goldLeft) = coinTracker.GetRemainingRewards();

                dialog.Text("Welcome to the Prize Center! Here you can exchange your ancient coins for rewards.\n\n" +
                            "Remember: coins expire after 60 minutes, so exchange them quickly!");

                dialog.Option($"Exchange Copper Coin [{copperLeft} left]", 1);
                dialog.Option($"Exchange Silver Coin [{silverLeft} left]", 2);
                dialog.Option($"Exchange Gold Coin [{goldLeft} left]", 3);

                // Temporary testing option
                dialog.Option("Claim All", 4);

                dialog.Option("Not now.", 255);
                dialog.Send();
                break;
            }

            case 1: {
                ExchangeCoin(client, dialog, coinTracker, ItemConstants.CopperCoin, CopperCoinRewards);
                break;
            }

            case 2: {
                ExchangeCoin(client, dialog, coinTracker, ItemConstants.SilverCoin, SilverCoinRewards);
                break;
            }

            case 3: {
                ExchangeCoin(client, dialog, coinTracker, ItemConstants.GoldCoin, GoldCoinRewards);
                break;
            }
        }
    }

    /// <summary>
    /// Check if the player has the coin and if they can claim the reward
    /// </summary>
    private static void ExchangeCoin(GameState client, MTA.Npcs dialog, TreasureInTheBlueCoinTracker coinTracker,
        uint coinType, (uint itemId, double weight)[] rewards) {
        var coinName = ConquerItemInformation.BaseInformations[coinType].Name;

        if (!client.Inventory.Contains(coinType, 1)) {
            dialog.Text($"You don't have a {coinName}!");
            dialog.Option("I understand.", 255);
            dialog.Send();
            return;
        }

        if (!coinTracker.CanClaimReward(coinType)) {
            dialog.Text($"Sorry, all {coinName} rewards have been claimed!");
            dialog.Option("I understand.", 255);
            dialog.Send();
            return;
        }

        // Remove coin and claim reward
        client.Inventory.Remove(coinType, 1);
        coinTracker.ClaimReward(coinType);

        // Give weighted random reward
        var randomReward = SelectWeightedReward(rewards);
        client.Inventory.Add(randomReward, 0, 1);

        // Get item name and show message
        var itemName = ConquerItemInformation.BaseInformations[randomReward].Name;
        client.MessageBox($"You received a {itemName}!");
    }

    /// <summary>
    /// Select a weighted random reward from the given rewards
    /// 
    /// How it works:
    /// 1. Calculate total weight (sum of all weights)
    /// 2. Normalize weights to sum to 1.0
    /// 3. Generate random number 0.0 to 1.0
    /// 4. Find which normalized cumulative range contains the random number
    /// 
    /// Example with weights [0.5, 0.5]:
    /// - Total = 1.0, normalized = [0.5, 0.5] = 50% each
    /// - Ranges: [0.0-0.5), [0.5-1.0)
    /// 
    /// Example with weights [1.0, 1.0, 1.0]:
    /// - Total = 3.0, normalized = [0.33, 0.33, 0.33] = 33.3% each
    /// </summary>
    private static uint SelectWeightedReward((uint itemId, double weight)[] rewards) {
        // Calculate total weight
        var totalWeight = 0.0;
        foreach (var (_, weight) in rewards) {
            totalWeight += weight;
        }

        // Generate random number and find which item it falls into
        var random = Random.NextDouble() * totalWeight; // 0.0 to totalWeight
        var cumulative = 0.0;

        foreach (var (itemId, weight) in rewards) {
            cumulative += weight;
            if (random < cumulative) {
                return itemId;
            }
        }

        return rewards[^1].itemId; // Fallback to last item
    }
}