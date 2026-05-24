using System;
using MTA.Client;
using MTA.Network.GamePackets;
using MTA.Network.PacketHandlers;

namespace MTA.Game.Features.Kisses.Packets.Handlers;

/// <summary>
///     Handles packet 1151 (MsgRank) sub-mode 1 for the four kiss fairy ranking lists
///     (Kiss / Love / Tine / Jade top-100, paginated). Returns false for any other ranking
///     type, so the existing PacketHandler switch can handle it (Perfection, etc.).
/// </summary>
[PacketHandler(Constants.Packets.MsgRank)]
public static class FairyRankingHandler {
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        if (packet[4] != 1) return false;

        var uid = BitConverter.ToUInt32(packet, 8);
        if (uid != GenericRanking.KissFairy &&
            uid != GenericRanking.LoveFairy &&
            uid != GenericRanking.TineFairy &&
            uid != GenericRanking.JadeFairy) {
            return false;
        }

        var pageNumber = BitConverter.ToUInt16(packet, 14);
        if (pageNumber > 9) return true;

        const int max = 10;
        var ranking = new GenericRanking(true, 10) {
            Mode = 1,
            Page = pageNumber,
            RankingType = uid,
            RegisteredCount = 100
        };

        switch (uid) {
            case GenericRanking.KissFairy: {
                var info = Kisses.KissesTop100;
                var offset = pageNumber * max;
                if (offset >= info.Length) return true;
                var count = Math.Min(max, info.Length - offset);
                ranking.Count = (uint)count;

                for (byte x = 0; x < count; x++) {
                    if (x + offset >= info.Length) break;
                    var entity = info[x + offset];
                    if (entity.Uid == 0) break;
                    ranking.Append((uint)entity.RankKisses, entity.Count, entity.Uid, entity.name);
                }

                break;
            }
            case GenericRanking.LoveFairy: {
                var info = Kisses.LettersTop100;
                var offset = pageNumber * max;
                if (offset >= info.Length) return true;
                var count = Math.Min(max, info.Length - offset);
                ranking.Count = (uint)count;

                for (byte x = 0; x < count; x++) {
                    if (x + offset >= info.Length) break;
                    var entity = info[x + offset];
                    if (entity.Uid == 0) break;
                    ranking.Append((uint)entity.RankLetters, entity.Letters, entity.Uid, entity.name);
                }

                break;
            }
            case GenericRanking.TineFairy: {
                var info = Kisses.WineTop100;
                var offset = pageNumber * max;
                if (offset >= info.Length) return true;
                var count = Math.Min(max, info.Length - offset);
                ranking.Count = (uint)count;

                for (byte x = 0; x < count; x++) {
                    if (x + offset >= info.Length) break;
                    var entity = info[x + offset];
                    if (entity.Uid == 0) break;
                    ranking.Append((uint)entity.RankWine, entity.Wine, entity.Uid, entity.name);
                }

                break;
            }
            case GenericRanking.JadeFairy: {
                var info = Kisses.JadesTop100;
                var offset = pageNumber * max;
                if (offset >= info.Length) return true;
                var count = Math.Min(max, info.Length - offset);
                ranking.Count = (uint)count;

                for (byte x = 0; x < count; x++) {
                    if (x + offset >= info.Length) break;
                    var entity = info[x + offset];
                    if (entity.Uid == 0) break;
                    ranking.Append((uint)entity.RankJades, entity.Jades, entity.Uid, entity.name);
                }

                break;
            }
        }

        client.Send(ranking.ToArray());
        return true;
    }
}