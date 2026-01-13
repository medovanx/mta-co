using MTA.Client;
using MTA.Game.Features.Kisses.Services;
using MTA.Network;
using MTA.Network.GamePackets;
using MTA.Network.PacketHandlers;

namespace MTA.Game.Features.Kisses.Packets.Handlers;

/// <summary>
///     Handles packet 1151 for kiss ranking display
/// </summary>
[PacketHandler(Constants.Packets.MsgRank)]
public static class KissRankingHandler {
    /// <summary>
    ///     Handles kiss ranking requests
    /// </summary>
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        switch (packet[4]) {
            case 2: {
                if (Constants.BodyTypes.IsGirl(client.Entity.Body)) return false;

                // Calculate rankings first to ensure RankKisses, RankLetters, etc. are set
                if (client.Entity.Kisses.Kisses2 > 0)
                    Kisses.CalculateRankKisses(client.Entity.Kisses);
                if (client.Entity.Kisses.Letters1 > 0)
                    Kisses.CalculateRankLetters(client.Entity.Kisses);
                if (client.Entity.Kisses.Wine > 0)
                    Kisses.CalculateRankWine(client.Entity.Kisses);
                if (client.Entity.Kisses.Jades > 0)
                    Kisses.CalculateRankJades(client.Entity.Kisses);

                var queryRanking = new GenericRanking(true) { Mode = GenericRanking.QueryCount };

                if (client.Entity.Kisses.RankKisses > 0)
                    PacketHandler.SendRankingQuery(queryRanking, client, GenericRanking.KissFairy,
                        (uint)client.Entity.Kisses.RankKisses, client.Entity.Kisses.Kisses2);
                if (client.Entity.Kisses.RankLetters > 0)
                    PacketHandler.SendRankingQuery(queryRanking, client, GenericRanking.LoveFairy,
                        (uint)client.Entity.Kisses.RankLetters, client.Entity.Kisses.Letters1);
                if (client.Entity.Kisses.RankWine > 0)
                    PacketHandler.SendRankingQuery(queryRanking, client, GenericRanking.TineFairy,
                        (uint)client.Entity.Kisses.RankWine, client.Entity.Kisses.Wine);
                if (client.Entity.Kisses.RankJades > 0)
                    PacketHandler.SendRankingQuery(queryRanking, client, GenericRanking.JadeFairy,
                        (uint)client.Entity.Kisses.RankJades, client.Entity.Kisses.Jades);

                var rank = KissHelper.CreateMyRank(client.Entity.Kisses, out var myRank);

                packet[4] = 5;
                client.Send(packet);

                client.Entity.KissRank =
                    (uint)client.Entity.Kisses.SendScreenValue((KissTypeT)rank, myRank);
                var ranking = new GenericRanking(true) {
                    Mode = 2,
                    RankingType = client.Entity.KissRank,
                    Count = 1
                };

                switch (rank) {
                    case (byte)KissTypeT.Kisses:
                        ranking.Append((uint)myRank, client.Entity.Kisses.Kisses2,
                            client.Entity.UID, client.Entity.Name);
                        break;
                    case (byte)KissTypeT.Letters:
                        ranking.Append((uint)myRank, client.Entity.Kisses.Letters1,
                            client.Entity.UID, client.Entity.Name);
                        break;
                    case (byte)KissTypeT.Wine:
                        ranking.Append((uint)myRank, client.Entity.Kisses.Wine,
                            client.Entity.UID, client.Entity.Name);
                        break;
                    case (byte)KissTypeT.Jades:
                        ranking.Append((uint)myRank, client.Entity.Kisses.Jades,
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