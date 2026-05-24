using System;
using MTA.Client;
using MTA.Game.Constants;
using MTA.Game.Features.Flowers.Database;
using MTA.Game.Features.Flowers.Packets.Writers;
using MTA.Game.Features.Kisses;
using static MTA.Kernel;

namespace MTA.Game.Features.Flowers.Services;

/// <summary>
///     Service for handling flower sending between players
/// </summary>
public static class FlowerSendingService {
    /// <summary>
    ///     Handles sending flowers (boys to girls, girls to boys)
    /// </summary>
    public static void SendFlower(GameState client, byte[] packet) {
        var typ1 = packet[4];
        var target = System.BitConverter.ToUInt32(packet, 8);
        var itemUid = System.BitConverter.ToUInt32(packet, 12);

        if (BodyTypes.IsBoy(client.Entity.Body) && typ1 == 0)
            // Boy sending to girl
            SendFlowerBoyToGirl(client, target, itemUid);
        else if (BodyTypes.IsGirl(client.Entity.Body) && typ1 == 1)
            // Girl sending to boy
            SendFlowerGirlToBoy(client, target, itemUid);
    }

    private static void SendFlowerBoyToGirl(GameState client, uint target, uint itemUid) {
        switch (itemUid) {
            case 0: // Send my flower
                if (client.Entity.Flowers.AFlower == 0) break;

                if (GamePool.TryGetValue(target, out var targetClient)) {
                    if (!BodyTypes.IsGirl(targetClient.Entity.Body))
                        return;
                    client.Entity.Flowers.AFlower = 0;
                    client.Entity.Flowers.SendDay = (uint)DateTime.Now.Day;
                    FlowerTable.Save(client);

                    targetClient.Entity.Flowers.RedRosesToday += 1;
                    targetClient.Entity.Flowers.RedRoses += 1;
                    FlowerTable.Save(targetClient);
                    var flow = new SendFlower {
                        Typing = 0,
                        Effect = (byte)FlowerEffect.Tulips,
                        Amount = 1,
                        SenderName = client.Entity.Name,
                        ReceiverName = targetClient.Entity.Name,
                        FType = (byte)FlowerType.RedRoses
                    };
                    if (targetClient.AsMember != null)
                        targetClient.AsMember.Roses += 1;

                    client.SendScreen(flow.ToArray());
                }

                break;
            default:
                if (client.Inventory.TryGetItem(itemUid, out var item))
                    if (GamePool.TryGetValue(target, out var targetClient2)) {
                        if (!BodyTypes.IsGirl(targetClient2.Entity.Body))
                            return;

                        var amount = item.ID % 1000;
                        var flow2 = new SendFlower {
                            Typing = 0,
                            Amount = amount,
                            SenderName = client.Entity.Name,
                            ReceiverName = targetClient2.Entity.Name
                        };

                        var flowerType = FlowerHelper.GetFlowerType(item.ID);
                        ProcessFlowerSending(targetClient2, flowerType, amount, flow2, true);

                        client.Inventory.Remove(item, Enums.ItemUse.Remove);
                        client.SendScreen(flow2.ToArray());
                    }

                break;
        }
    }

    private static void SendFlowerGirlToBoy(GameState client, uint target, uint itemUid) {
        switch (itemUid) {
            case 0: // Current flower
                if (client.Entity.Flowers.AFlower == 0)
                    return;
                if (GamePool.TryGetValue(target, out var targetClient)) {
                    if (!BodyTypes.IsBoy(targetClient.Entity.Body))
                        return;
                    client.Entity.Flowers.AFlower = 0;
                    client.Entity.Flowers.SendDay = (uint)DateTime.Now.Day;
                    FlowerTable.Save(client);

                    targetClient.Entity.Flowers.RedRoses += 1;
                    targetClient.Entity.Flowers.RedRosesToday += 1;
                    FlowerTable.Save(targetClient);
                    var flow = new SendFlower {
                        Typing = 1,
                        Effect = (byte)KissEffect.Kiss,
                        Amount = 1,
                        SenderName = client.Entity.Name,
                        ReceiverName = targetClient.Entity.Name,
                        FType = (byte)KissesT.Kiss
                    };

                    if (targetClient.AsMember != null)
                        targetClient.AsMember.Roses += 1;

                    client.SendScreen(flow.ToArray());
                }

                break;
            default:
                if (client.Inventory.TryGetItem(itemUid, out var item))
                    if (GamePool.TryGetValue(target, out var targetClient2)) {
                        if (!BodyTypes.IsBoy(targetClient2.Entity.Body))
                            return;

                        var amount = item.ID % 1000;
                        var flow2 = new SendFlower {
                            Typing = 1,
                            Amount = amount,
                            SenderName = client.Entity.Name,
                            ReceiverName = targetClient2.Entity.Name
                        };

                        var flowerType = FlowerHelper.GetFlowerType(item.ID);
                        ProcessFlowerSending(targetClient2, flowerType, amount, flow2, false);

                        client.Inventory.Remove(item, Enums.ItemUse.Remove);
                        client.SendScreen(flow2.ToArray());
                    }

                break;
        }
    }

    private static void ProcessFlowerSending(GameState targetClient, FlowerType flowerType, uint amount,
        SendFlower flow, bool isBoyToGirl) {
        switch (flowerType) {
            case FlowerType.RedRoses: {
                flow.Effect = isBoyToGirl ? (byte)FlowerEffect.Rose : (byte)KissEffect.Kiss;
                flow.FType = isBoyToGirl ? (byte)FlowerType.RedRoses : (byte)KissesT.Kiss;

                targetClient.Entity.Flowers.RedRosesToday += amount;
                targetClient.Entity.Flowers.RedRoses += amount;
                UpdateRanking(targetClient.Entity.Flowers,
                    isBoyToGirl ? Flowers.CalculateRoseRank : Flowers.CalculateRankKiss,
                    isBoyToGirl ? Flowers.RedRousesTop100 : Flowers.KissTop100,
                    targetClient.Entity.Flowers.RedRoses, flowerType);
                FlowerTable.Save(targetClient);
                if (targetClient.AsMember != null)
                    targetClient.AsMember.Roses += amount;
                break;
            }
            case FlowerType.Lilies: {
                flow.Effect = isBoyToGirl ? (byte)FlowerEffect.Lilies : (byte)KissEffect.Love;
                flow.FType = isBoyToGirl ? (byte)FlowerType.Lilies : (byte)KissesT.Love;

                targetClient.Entity.Flowers.LiliesToday += amount;
                targetClient.Entity.Flowers.Lilies += amount;
                UpdateRanking(targetClient.Entity.Flowers,
                    isBoyToGirl ? Flowers.CalculateRankLilies : Flowers.CalculateRankLove,
                    isBoyToGirl ? Flowers.LiliesTop100 : Flowers.LoveTop100,
                    targetClient.Entity.Flowers.Lilies, flowerType);
                FlowerTable.Save(targetClient);
                if (targetClient.AsMember != null)
                    targetClient.AsMember.Lilies += amount;
                break;
            }
            case FlowerType.Orchids: {
                flow.Effect = isBoyToGirl ? (byte)FlowerEffect.Orchids : (byte)KissEffect.Wine;
                flow.FType = isBoyToGirl ? (byte)FlowerType.Orchids : (byte)KissesT.Wine;

                targetClient.Entity.Flowers.OrchidsToday += amount;
                targetClient.Entity.Flowers.Orchids += amount;
                UpdateRanking(targetClient.Entity.Flowers,
                    isBoyToGirl ? Flowers.CalculateRankOrchids : Flowers.CalculateRankWine,
                    isBoyToGirl ? Flowers.OrchidsTop100 : Flowers.WineTop100,
                    targetClient.Entity.Flowers.Orchids, flowerType);
                FlowerTable.Save(targetClient);
                if (targetClient.AsMember != null)
                    targetClient.AsMember.Orchids += amount;
                break;
            }
            case FlowerType.Tulips: {
                flow.Effect = isBoyToGirl ? (byte)FlowerEffect.Tulips : (byte)KissEffect.Jade;
                flow.FType = isBoyToGirl ? (byte)FlowerType.Tulips : (byte)KissesT.Jade;

                targetClient.Entity.Flowers.TulipsToday += amount;
                targetClient.Entity.Flowers.Tulips += amount;
                UpdateRanking(targetClient.Entity.Flowers,
                    isBoyToGirl ? Flowers.CalculateRankTulips : Flowers.CalculateRankJade,
                    isBoyToGirl ? Flowers.TulipsTop100 : Flowers.JadeTop100,
                    targetClient.Entity.Flowers.Tulips, flowerType);
                FlowerTable.Save(targetClient);
                if (targetClient.AsMember != null)
                    targetClient.AsMember.Tulips += amount;
                break;
            }
        }
    }

    private static void UpdateRanking(Flowers flowers, Action<Flowers> calculateRank, Flowers[] top100,
        uint currentValue, FlowerType flowerType) {
        if (top100.Length > 98) {
            uint topValue = flowerType switch {
                FlowerType.RedRoses => top100[98].RedRoses,
                FlowerType.Lilies => top100[98].Lilies,
                FlowerType.Orchids => top100[98].Orchids,
                FlowerType.Tulips => top100[98].Tulips,
                _ => 0
            };
            if (topValue <= currentValue) calculateRank(flowers);
        }
        else {
            calculateRank(flowers);
        }
    }
}