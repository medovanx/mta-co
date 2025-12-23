using System;

namespace MTA.Game.Events.TreasureInTheBlue;

/// <summary>
///     Shared utility methods for the Treasure in the Blue event
/// </summary>
public static class TreasureInTheBlueHelpers {
    private static readonly Random Random = new();

    /// <summary>
    ///     Select a weighted random reward from the rewards array
    ///     Weights are automatically normalized, so percentages don't need to sum to 1.0
    /// </summary>
    /// <param name="rewards">Array of (itemId, weight) tuples</param>
    /// <returns>Selected item ID</returns>
    public static uint SelectWeightedReward((uint itemId, double weight)[] rewards) {
        // Calculate total weight
        var totalWeight = 0.0;
        foreach (var (_, weight) in rewards) {
            totalWeight += weight;
        }

        // Generate random number and find which item it falls into
        var random = Random.NextDouble() * totalWeight; // 0.0 to totalWeight
        var cumulative = 0.0;

        foreach (var (itemId, weight) in rewards) {
            cumulative += weight;
            if (random < cumulative) {
                return itemId;
            }
        }

        return rewards[^1].itemId; // Fallback to last item
    }
}