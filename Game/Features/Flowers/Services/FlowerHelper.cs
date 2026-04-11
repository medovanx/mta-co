using System;
using System.Collections.Generic;
using System.Linq;

namespace MTA.Game.Features.Flowers.Services;

/// <summary>
///     Helper methods for flower-related operations
/// </summary>
public static class FlowerHelper {
    /// <summary>
    ///     Determines the flower type based on item ID
    /// </summary>
    public static FlowersT GetFlowerType(uint itemId) {
        return itemId switch {
            >= 751001 and <= 751999 or >= 755001 and <= 755999 => FlowersT.Roses,
            >= 752001 and <= 752999 or >= 756001 and <= 756999 => FlowersT.Lilies,
            >= 753001 and <= 753999 or >= 757001 and <= 757999 => FlowersT.Orchids,
            >= 754001 and <= 754999 or >= 758001 and <= 758999 => FlowersT.Tulips,
            _ => FlowersT.Roses
        };
    }

    /// <summary>
    ///     Creates the best rank for a client's flowers
    /// </summary>
    public static byte CreateMyRank(Flowers flowers, out int rank) {
        var fRanks = new List<ClientRank> {
            new() { Rank = (uint)flowers.RankLilies, Amount = flowers.Lilies },
            new() { Rank = (uint)flowers.RankOrchids, Amount = flowers.Orchids },
            new() { Rank = (uint)flowers.RankRoses, Amount = flowers.RedRoses },
            new() { Rank = (uint)flowers.RankTulops, Amount = flowers.Tulips }
        };

        var array = fRanks.Where(f1 => f1.Rank != 0).ToArray();
        Array.Sort(array, (f1, f2) => {
            var nRank = f1.Rank.CompareTo(f2.Rank);
            return f2.Rank == f1.Rank ? f2.Amount.CompareTo(f1.Amount) : nRank;
        });

        if (array is { Length: > 0 }) {
            var bestRank = array[0];
            if (bestRank.Rank != 0) {
                rank = (int)bestRank.Rank;
                if (flowers.RankLilies == bestRank.Rank && flowers.Lilies == bestRank.Amount)
                    return (byte)FlowersT.Lilies;
                if (flowers.RankOrchids == bestRank.Rank && flowers.Orchids == bestRank.Amount)
                    return (byte)FlowersT.Orchids;
                if (flowers.RankRoses == bestRank.Rank && flowers.RedRoses == bestRank.Amount)
                    return (byte)FlowersT.Roses;
                if (flowers.RankTulops == bestRank.Rank && flowers.Tulips == bestRank.Amount)
                    return (byte)FlowersT.Tulips;
            }
        }

        rank = -1;
        return (byte)FlowersT.None;
    }

    private class ClientRank {
        public uint Amount { get; init; }
        public uint Rank { get; init; }
    }
}