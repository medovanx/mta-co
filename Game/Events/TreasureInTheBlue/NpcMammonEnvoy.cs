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

    // Reward item IDs per coin type
    private static readonly uint[] CopperCoinRewards = [
        ItemConstants.Meteor,
        ItemConstants.Class1MoneyBag,
        ItemConstants.SmallJoyStone,
        ItemConstants.ExpBallScrap
    ];

    private static readonly uint[] SilverCoinRewards = [
        ItemConstants.EnduranceBook,
        ItemConstants.Class2MoneyBag,
        ItemConstants.HorseRacingPointsPack3K,
        ItemConstants.ExpBallScrap,
        ItemConstants.SmallJoyStone
    ];

    private static readonly uint[] GoldCoinRewards = [
        ItemConstants.SmallLotteryTicket,
        ItemConstants.JadeHare,
        ItemConstants.CelestialBird,
        ItemConstants.GreenEyedBeast
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

            case 4: {
                // Temporary testing: Claim All - give all possible rewards without checks
                foreach (var reward in CopperCoinRewards) {
                    client.Inventory.Add(reward, 0, 1);
                }

                foreach (var reward in SilverCoinRewards) {
                    client.Inventory.Add(reward, 0, 1);
                }

                foreach (var reward in GoldCoinRewards) {
                    client.Inventory.Add(reward, 0, 1);
                }

                client.MessageBox("Claimed all rewards!");

                break;
            }
        }
    }

    private static void ExchangeCoin(GameState client, MTA.Npcs dialog, TreasureInTheBlueCoinTracker coinTracker,
        uint coinType, uint[] rewards) {
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

        // Give random reward
        var randomReward = rewards[Random.Next(rewards.Length)];
        client.Inventory.Add(randomReward, 0, 1);

        // Get item name and show message
        var itemName = ConquerItemInformation.BaseInformations[randomReward].Name;
        client.MessageBox($"You received a {itemName}!");
    }
}