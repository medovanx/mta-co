using MTA.Client;
using MTA.Game.Constants;
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
                if (!BodyTypes.IsGirl(client.Entity.Body)) return false;

                // Calculate rankings first to ensure RankRoses, RankLilies, etc. are set
                if (client.Entity.Flowers.RedRoses > 0)
                    Flowers.CalculateRoseRank(client.Entity.Flowers);
                if (client.Entity.Flowers.Lilies > 0)
                    Flowers.CalculateRankLilies(client.Entity.Flowers);
                if (client.Entity.Flowers.Orchids > 0)
                    Flowers.CalculateRankOrchids(client.Entity.Flowers);
                if (client.Entity.Flowers.Tulips > 0)
                    Flowers.CalculateRankTulips(client.Entity.Flowers);

                var queryRanking = new GenericRanking(true) { Mode = GenericRanking.QueryCount };

                if (client.Entity.Flowers.RankRoses > 0)
                    PacketHandler.SendRankingQuery(queryRanking, client, GenericRanking.RoseFairy,
                        (uint)client.Entity.Flowers.RankRoses, client.Entity.Flowers.RedRoses);
                if (client.Entity.Flowers.RankLilies > 0)
                    PacketHandler.SendRankingQuery(queryRanking, client, GenericRanking.LilyFairy,
                        (uint)client.Entity.Flowers.RankLilies, client.Entity.Flowers.Lilies);
                if (client.Entity.Flowers.RankOrchids > 0)
                    PacketHandler.SendRankingQuery(queryRanking, client, GenericRanking.OrchidFairy,
                        (uint)client.Entity.Flowers.RankOrchids, client.Entity.Flowers.Orchids);
                if (client.Entity.Flowers.RankTulops > 0)
                    PacketHandler.SendRankingQuery(queryRanking, client, GenericRanking.TulipFairy,
                        (uint)client.Entity.Flowers.RankTulops, client.Entity.Flowers.Tulips);

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

                return true;
            }
        }

        return false;
    }
}