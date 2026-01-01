#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
namespace MTA.Game.Features.Guilds.Constants;

/// <summary>
///     Provides maximum rank limits based on guild level (1-9)
/// </summary>
public static class GuildRankLimits {
    /// <summary>
    ///     Gets the maximum number of Deputy Leaders allowed for a given guild level
    /// </summary>
    public static byte GetMaxDeputyLeader(byte level) {
        return level switch {
            1 => 2,
            2 => 2,
            3 => 2,
            4 => 3,
            5 => 3,
            6 => 3,
            7 => 4,
            8 => 4,
            9 => 4
        };
    }

    /// <summary>
    ///     Gets the maximum number of Honorary Deputy Leaders allowed for a given guild level
    /// </summary>
    public static byte GetMaxHonoraryDeputyLeader(byte level) {
        return level switch {
            1 => 1,
            2 => 1,
            3 => 1,
            4 => 1,
            5 => 1,
            6 => 2,
            7 => 2,
            8 => 2,
            9 => 2
        };
    }

    /// <summary>
    ///     Gets the maximum number of Managers allowed for a given guild level
    /// </summary>
    public static byte GetMaxManager(byte level) {
        return level switch {
            1 => 1,
            2 => 1,
            3 => 2,
            4 => 2,
            5 => 4,
            6 => 4,
            7 => 6,
            8 => 6,
            9 => 8
        };
    }

    /// <summary>
    ///     Gets the maximum number of Honorary Managers allowed for a given guild level
    /// </summary>
    public static byte GetMaxHonoraryManager(byte level) {
        return level switch {
            1 => 1,
            2 => 1,
            3 => 1,
            4 => 1,
            5 => 2,
            6 => 2,
            7 => 4,
            8 => 4,
            9 => 6
        };
    }

    /// <summary>
    ///     Gets the maximum number of Supervisors per type (Donation/Flower) allowed for a given guild level
    ///     Note: There are 9 types of Supervisors (one for each donation type and flower type)
    /// </summary>
    public static byte GetMaxSupervisorPerType(byte level) {
        return level switch {
            >= 1 and <= 3 => 0, // No supervisors for levels 1-3
            >= 4 and <= 6 => 1, // 1 Supervisor per type for levels 4-6
            >= 7 and <= 9 => 2 // 2 Supervisors per type for levels 7-9
        };
    }

    /// <summary>
    ///     Gets the maximum number of Honorary Supervisors allowed for a given guild level
    /// </summary>
    public static byte GetMaxHonorarySupervisor(byte level) {
        return level switch {
            1 => 1,
            2 => 1,
            3 => 1,
            4 => 1,
            5 => 2,
            6 => 2,
            7 => 6,
            8 => 6,
            9 => 8
        };
    }

    /// <summary>
    ///     Gets the maximum number of Stewards allowed for a given guild level
    /// </summary>
    public static byte GetMaxSteward(byte level) {
        return level switch {
            1 => 0,
            2 => 1,
            3 => 2,
            4 => 3,
            5 => 4,
            6 => 5,
            7 => 6,
            8 => 8,
            9 => 8
        };
    }

    /// <summary>
    ///     Gets the maximum number of Honorary Stewards allowed for a given guild level
    /// </summary>
    public static byte GetMaxHonorarySteward(byte level) {
        return level switch {
            1 => 1,
            2 => 1,
            3 => 2,
            4 => 2,
            5 => 4,
            6 => 4,
            7 => 6,
            8 => 6,
            9 => 8
        };
    }

    /// <summary>
    ///     Gets the maximum number of Aides allowed (applies to various aide types)
    /// </summary>
    public static byte GetMaxAide(byte level) {
        return 6;
    }

    /// <summary>
    ///     Gets the maximum number of Followers allowed (applies to various follower types)
    /// </summary>
    public static byte GetMaxFollower(byte level) {
        // Followers have no limit, but we return a high value for validation purposes
        return 255;
    }

    /// <summary>
    ///     Gets the maximum number of guild members allowed (includes all ranks)
    /// </summary>
    public static ushort GetMaxMembers(byte level) {
        return 800;
    }
}