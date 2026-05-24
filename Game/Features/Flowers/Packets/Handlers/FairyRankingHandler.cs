using System;
using MTA.Client;
using MTA.Network.GamePackets;
using MTA.Network.PacketHandlers;

namespace MTA.Game.Features.Flowers.Packets.Handlers;

/// <summary>
///     Handles packet 1151 (MsgRank) sub-mode 1 for the four flower fairy ranking lists
///     (RedRose / Lily / Orchid / Tulip top-100, paginated). Returns false for any other
///     ranking type, so the existing PacketHandler switch can handle it (Perfection, etc.).
/// </summary>
[PacketHandler(Constants.Packets.MsgRank)]
public static class FairyRankingHandler {
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        if (packet[4] != 1) return false; // sub-mode 1 = ranking list query

        var uid = BitConverter.ToUInt32(packet, 8);
        var source = SelectSource(uid);
        if (source is null) return false;

        var pageNumber = BitConverter.ToUInt16(packet, 14);
        if (pageNumber > 9) return true;

        const int max = 10;
        var (info, getRank, getValue) = source.Value;
        var offset = pageNumber * max;
        if (offset >= info.Length) return true;

        var count = Math.Min(max, info.Length - offset);
        var ranking = new GenericRanking(true, 10) {
            Mode = 1,
            Page = pageNumber,
            RankingType = uid,
            RegisteredCount = 100,
            Count = (uint)count
        };

        for (byte x = 0; x < count; x++) {
            if (x + offset >= info.Length) break;
            var entity = info[x + offset];
            if (entity.Uid == 0) break;
            ranking.Append(getRank(entity), getValue(entity), entity.Uid, entity.Name);
        }

        client.Send(ranking.ToArray());
        return true;
    }

    private static (Flowers[] info, Func<Flowers, uint> getRank, Func<Flowers, uint> getValue)? SelectSource(uint uid) {
        return uid switch {
            GenericRanking.RoseFairy   => (Flowers.RedRousesTop100, e => (uint)e.RankRoses,   e => e.RedRoses),
            GenericRanking.LilyFairy   => (Flowers.LiliesTop100,    e => (uint)e.RankLilies,  e => e.Lilies),
            GenericRanking.OrchidFairy => (Flowers.OrchidsTop100,   e => (uint)e.RankOrchids, e => e.Orchids),
            GenericRanking.TulipFairy  => (Flowers.TulipsTop100,    e => (uint)e.RankTulips,  e => e.Tulips),
            _ => null
        };
    }
}
