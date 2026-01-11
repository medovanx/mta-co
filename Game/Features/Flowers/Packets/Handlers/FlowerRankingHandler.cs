using MTA.Client;
using MTA.Game.Features.Flowers;
using MTA.Game.Features.Flowers.Services;
using MTA.Network;
using MTA.Network.GamePackets;
using MTA.Network.PacketHandlers;

namespace MTA.Game.Features.Flowers.Packets.Handlers;

/// <summary>
///     Handles packet 1151 for flower ranking display
/// </summary>
[PacketHandler(Constants.Packets.MsgRank)]
public static class FlowerRankingHandler {
    /// <summary>
    ///     Handles flower ranking requests
    /// </summary>
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        switch (packet[4]) {
            case 2: {
                if (client.Entity.Flowers == null) {
                    return false;
                }

                // Calculate rankings first to ensure RankRoses, RankLilies, etc. are set
                // For girls: calculate all flower types
                // For boys: only calculate Red Roses
                if (PacketHandler.IsGirl(client.Entity.Body)) {
                    // Calculate all flower type rankings for girls
                    if (client.Entity.Flowers.RedRoses > 0)
                        Flowers.CalculateRoseRank(client.Entity.Flowers);
                    if (client.Entity.Flowers.Lilies > 0)
                        Flowers.CalculateRankLilies(client.Entity.Flowers);
                    if (client.Entity.Flowers.Orchids > 0)
                        Flowers.CalculateRankOrchids(client.Entity.Flowers);
                    if (client.Entity.Flowers.Tulips > 0)
                        Flowers.CalculateRankTulips(client.Entity.Flowers);
                }
                else {
                    // Calculate Red Roses ranking for boys
                    if (client.Entity.Flowers.RedRoses > 0)
                        Flowers.CalculateRoseRank(client.Entity.Flowers);
                }

                if (PacketHandler.IsGirl(client.Entity.Body)) {
                    // Girl ranking display
                    var rank = FlowerHelper.CreateMyRank(client.Entity.Flowers, out var myRank);

                    packet[4] = 5;
                    client.Send(packet);

                    client.Entity.FlowerRank =
                        (uint)client.Entity.Flowers.SendScreenValue((FlowersT)rank, myRank);
                    var ranking = new GenericRanking(true) {
                        Mode = 2,
                        RankingType = client.Entity.FlowerRank,
                        Count = 1
                    };

                    switch (rank) {
                        case (byte)FlowersT.Roses:
                            ranking.Append((uint)myRank, client.Entity.Flowers.RedRoses,
                                client.Entity.UID, client.Entity.Name);
                            break;
                        case (byte)FlowersT.Lilies:
                            ranking.Append((uint)myRank, client.Entity.Flowers.Lilies,
                                client.Entity.UID, client.Entity.Name);
                            break;
                        case (byte)FlowersT.Orchids:
                            ranking.Append((uint)myRank, client.Entity.Flowers.Orchids,
                                client.Entity.UID, client.Entity.Name);
                            break;
                        case (byte)FlowersT.Tulips:
                            ranking.Append((uint)myRank, client.Entity.Flowers.Tulips,
                                client.Entity.UID, client.Entity.Name);
                            break;
                    }

                    client.Send(ranking.ToArray());
                    packet[4] = 5;
                    client.Send(packet);
                }
                else {
                    // Boy ranking display
                    var ranking = new GenericRanking(true) {
                        Mode = 2,
                        Count = 1
                    };
                    if (client.Entity.Flowers.RankRoses is < 100 and > 0) {
                        ranking.Append((uint)client.Entity.Flowers.RankRoses,
                            client.Entity.Flowers.RedRoses, client.Entity.UID, client.Entity.Name);
                        client.Send(ranking.ToArray());
                    }

                    packet[4] = 5;
                    client.Send(packet);
                }

                return true;
            }
        }

        return false;
    }
}