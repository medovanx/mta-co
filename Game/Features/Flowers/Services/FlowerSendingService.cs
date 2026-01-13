using System;
using MTA.Client;
using MTA.Game.Constants;
using MTA.Game.Features.Flowers.Database;
using MTA.Game.Features.Flowers.Packets.Writers;
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
                        Effect = (byte)Effect.Tulips,
                        Amount = 1,
                        SenderName = client.Entity.Name,
                        ReceiverName = targetClient.Entity.Name,
                        FType = (byte)FlowersT.Roses
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
                        Effect = (byte)Effect.Kiss,
                        Amount = 1,
                        SenderName = client.Entity.Name,
                        ReceiverName = targetClient.Entity.Name,
                        FType = (byte)FlowersT.Kiss
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

    private static void ProcessFlowerSending(GameState targetClient, FlowersT flowerType, uint amount,
        SendFlower flow, bool isBoyToGirl) {
        switch (flowerType) {
            case FlowersT.Roses: {
                flow.Effect = (byte)(isBoyToGirl ? Effect.Rose : Effect.Kiss);
                flow.FType = (byte)(isBoyToGirl ? FlowersT.Roses : FlowersT.Kiss);

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
            case FlowersT.Lilies: {
                flow.Effect = (byte)(isBoyToGirl ? Effect.Lilies : Effect.Love);
                flow.FType = (byte)(isBoyToGirl ? FlowersT.Lilies : FlowersT.Love);

                targetClient.Entity.Flowers.Lilies2day += amount;
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
            case FlowersT.Orchids: {
                flow.Effect = (byte)(isBoyToGirl ? Effect.Orchids : Effect.Wine);
                flow.FType = (byte)(isBoyToGirl ? FlowersT.Orchids : FlowersT.Wine);

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
            case FlowersT.Tulips: {
                flow.Effect = (byte)(isBoyToGirl ? Effect.Tulips : Effect.Jade);
                flow.FType = (byte)(isBoyToGirl ? FlowersT.Tulips : FlowersT.Jade);

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
        uint currentValue, FlowersT flowerType) {
        if (top100.Length > 98) {
            uint topValue = flowerType switch {
                FlowersT.Roses => top100[98].RedRoses,
                FlowersT.Lilies => top100[98].Lilies,
                FlowersT.Orchids => top100[98].Orchids,
                FlowersT.Tulips => top100[98].Tulips,
                _ => 0
            };
            if (topValue <= currentValue) calculateRank(flowers);
        }
        else {
            calculateRank(flowers);
        }
    }
}