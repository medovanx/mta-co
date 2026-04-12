using System;
using System.Collections.Generic;
using System.Linq;

namespace MTA.Game.Features.Kisses.Services;

/// <summary>
///     Helper methods for kiss-related operations
/// </summary>
public static class KissHelper {
    /// <summary>
    ///     Creates the best rank for a client's kisses
    /// </summary>
    public static byte CreateMyRank(Kisses kisses, out int rank) {
        var kRanks = new List<ClientRank> {
            new() { Rank = (uint)kisses.RankKisses, Amount = kisses.Kisses2 },
            new() { Rank = (uint)kisses.RankLetters, Amount = kisses.Letters1 },
            new() { Rank = (uint)kisses.RankWine, Amount = kisses.Wine },
            new() { Rank = (uint)kisses.RankJades, Amount = kisses.Jades }
        };

        var array = kRanks.Where(k1 => k1.Rank != 0).ToArray();
        Array.Sort(array, (k1, k2) => {
            var nRank = k1.Rank.CompareTo(k2.Rank);
            return k2.Rank == k1.Rank ? k2.Amount.CompareTo(k1.Amount) : nRank;
        });

        if (array is { Length: > 0 }) {
            var bestRank = array[0];
            if (bestRank.Rank != 0) {
                rank = (int)bestRank.Rank;
                if (kisses.RankKisses == bestRank.Rank && kisses.Kisses2 == bestRank.Amount)
                    return (byte)KissTypeT.Kisses;
                if (kisses.RankLetters == bestRank.Rank && kisses.Letters1 == bestRank.Amount)
                    return (byte)KissTypeT.Letters;
                if (kisses.RankWine == bestRank.Rank && kisses.Wine == bestRank.Amount)
                    return (byte)KissTypeT.Wine;
                if (kisses.RankJades == bestRank.Rank && kisses.Jades == bestRank.Amount)
                    return (byte)KissTypeT.Jades;
            }
        }

        rank = -1;
        return 0;
    }

    private class ClientRank {
        public uint Amount { get; init; }
        public uint Rank { get; init; }
    }
}