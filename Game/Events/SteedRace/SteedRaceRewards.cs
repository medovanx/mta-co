using System;

namespace MTA.Game.Events.SteedRace;

/// <summary>
///     Steed Race reward calculations
/// </summary>
public static class SteedRaceRewards {
    /// <summary>
    ///     Calculate award for finishing the race
    /// </summary>
    /// <param name="time">Race completion time in milliseconds</param>
    /// <param name="rank">Finishing position (1st = 1, 2nd = 2, 3rd = 3, etc.)</param>
    /// <returns>Points awarded (minimum 3,500)</returns>
    /// <remarks>
    ///     Formula: Math.Max(3500, 100000 / rank - time * 2)
    ///     <para>
    ///         Base reward = 100000 / rank (1st place: 100,000, 2nd: 50,000, 3rd: ~33,333, etc.)
    ///         Time penalty = time * 2 (subtracts 2 points per millisecond)
    ///         Minimum guarantee = 3,500 points (ensures everyone gets at least this amount)
    ///     </para>
    ///     <para>
    ///         Example: 1st place in 30 seconds (30,000ms) = 100000 - 60000 = 40,000 points
    ///     </para>
    /// </remarks>
    public static int AwardPlayer(int time, int rank) {
        return Math.Max(3500, 100000 / rank - time * 2);
    }
}