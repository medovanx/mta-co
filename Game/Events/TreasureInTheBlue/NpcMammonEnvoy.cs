using System;
using MTA.Client;
using MTA.Database;
using MTA.Game.Constants;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.Items.BasicItems;
using static MTA.Game.Constants.Items.Mounts;
using static MTA.Game.Constants.Items.StudyBooks;
using static MTA.Game.Constants.Items.MoneyBags;

namespace MTA.Game.Events.TreasureInTheBlue;

/// <summary>
/// Mammon Envoy NPC - Exchanges event coins for rewards in the Treasure in the Blue event
/// </summary>
/// <event>Treasure in the Blue</event>
/// <npc>Mammon Envoy</npc>
/// <description>Exchanges event coins for rewards</description>
[NpcHandler(15)]
public static class NpcMammonEnvoy {
    private static readonly Random Random = new();

    // Reward item IDs and odds per coin type
    private static readonly (uint itemId, double weight)[] CopperCoinRewards = [
        (Meteor, 0.50),        
        (Class1MoneyBag, 0.25),
        (SmallJoyStone, 0.60),
        (ExpBallScrap, 0.60)   
    ];

    private static readonly (uint itemId, double weight)[] SilverCoinRewards = [
        (EnduranceBook, 0.35),
        (Class2MoneyBag, 0.15),
        (HorseRacingPointsPack3K, 0.40),
        (ExpBallScrap, 0.50),  
        (SmallJoyStone, 0.80)  
    ];

    private static readonly (uint itemId, double weight)[] GoldCoinRewards = [
        (SmallLotteryTicket, 0.60),
        (JadeHare, 0.05),
        (CelestialBird, 0.25),    
        (GreenEyedBeast, 0.25)    
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
                dialog.Option("Teleport me back to the Proud Sea", 4);
                dialog.Option("Not now.", 255);
                dialog.Send();
                break;
            }

            case 1: {
                ExchangeCoin(client, dialog, coinTracker, CopperCoin, CopperCoinRewards);
                break;
            }

            case 2: {
                ExchangeCoin(client, dialog, coinTracker, SilverCoin, SilverCoinRewards);
                break;
            }

            case 3: {
                ExchangeCoin(client, dialog, coinTracker, GoldCoin, GoldCoinRewards);
                break;
            }

            case 4: {
                client.Entity.Teleport(Maps.ProudSea, 200, 067);
                client.Entity.Update(_String.Effect, "accession3", true);
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
        var randomReward = TreasureInTheBlueHelpers.SelectWeightedReward(rewards);
        client.Inventory.Add(randomReward, 0, 1);

        // Get item name and show message
        var itemName = ConquerItemInformation.BaseInformations[randomReward].Name;
        client.Entity.Update(_String.Effect, "angelwing", true);
        client.MessageBox($"You received a {itemName}!");
    }
}