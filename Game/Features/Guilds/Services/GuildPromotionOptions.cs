using System;
using System.Collections.Generic;
using System.Linq;
using MTA.Game.Features.Guilds.Constants;

namespace MTA.Game.Features.Guilds.Services;

/// <summary>
///     Delegate for checking if a rank limit is reached.
///     Parameters: ranksCounts (array of current counts per rank), targetRank, guildLevel
///     Returns: true if limit is reached
/// </summary>
public delegate bool LimitCheckDelegate(ushort[] ranksCounts, MemberRank targetRank, byte guildLevel);

/// <summary>
///     Defines the promotion matrix that determines which ranks each guild rank can promote members to.
///     This is the single source of truth for promotion permissions, Conquer Points costs, and rank limits.
///     Each rank has a specific set of ranks they can promote to, with some requiring CP costs (honorary ranks)
///     and others having limits based on guild level. Auto-assigned ranks (Stewards, Agents, Followers) are
///     excluded from manual promotion options as they are assigned automatically by the system.
/// </summary>
public static class GuildPromotionOptions {
    /// <summary>
    ///     Returns all ranks that a given rank can promote members to. This is used to populate the
    ///     promotion UI on the client, showing available promotion targets with their CP costs and
    ///     current counts. Different ranks have different promotion capabilities based on the guild hierarchy.
    /// </summary>
    /// <param name="promotingRank">The rank of the player attempting to promote</param>
    /// <returns>Collection of promotion options available to this rank</returns>
    public static IEnumerable<PromotionOption> GetPromotionOptions(MemberRank promotingRank) {
        return promotingRank switch {
            MemberRank.GuildLeader => GetGuildLeaderOptions(),

            MemberRank.DeputyLeader => GetDeputyLeaderOptions(),
            MemberRank.HDeputyLeader => GetDeputyLeaderOptions(),
            MemberRank.LeaderSpouse => GetDeputyLeaderOptions(),

            MemberRank.Manager => GetManagerOptions(),
            MemberRank.HonoraryManager => GetManagerOptions(),

            MemberRank.Supervisor => GetSupervisorOptions(),
            MemberRank.HonorarySupervisor => GetSupervisorOptions(),
            MemberRank.TSupervisor => GetSupervisorOptions(),
            MemberRank.OSupervisor => GetSupervisorOptions(),
            MemberRank.CPSupervisor => GetSupervisorOptions(),
            MemberRank.ASupervisor => GetSupervisorOptions(),
            MemberRank.SSupervisor => GetSupervisorOptions(),
            MemberRank.GSupervisor => GetSupervisorOptions(),
            MemberRank.PKSupervisor => GetSupervisorOptions(),
            MemberRank.RoseSupervisor => GetSupervisorOptions(),
            MemberRank.LilySupervisor => GetSupervisorOptions(),

            MemberRank.Agent => GetAgentOptions(),
            _ => []
        };
    }

    /// <summary>
    ///     Creates a delegate function that checks if a rank has reached its maximum limit based on
    ///     the current guild level. Used for server-side validation to prevent exceeding rank quotas.
    /// </summary>
    /// <param name="targetRank">The rank to check limits for</param>
    /// <param name="getMaxLimit">Function that returns the max limit for a given guild level</param>
    /// <returns>Delegate that returns true if the rank limit is reached</returns>
    private static LimitCheckDelegate CreateLimitCheck(MemberRank targetRank, Func<byte, byte> getMaxLimit) {
        return (ranksCounts, _, guildLevel) => {
            var rankIndex = (ushort)targetRank;
            if (rankIndex >= ranksCounts.Length) return false;
            var currentCount = ranksCounts[rankIndex];
            var maxLimit = getMaxLimit(guildLevel);
            return currentCount >= maxLimit;
        };
    }

    /// <summary>
    ///     Defines all ranks that a Guild Leader can promote members to. Guild Leaders have the
    ///     broadest promotion authority, including Deputy Leaders, Honorary officials (with CP costs),
    ///     various Aide positions, and can demote members back to Member rank. They can also transfer
    ///     leadership to another member.
    /// </summary>
    /// <returns>Promotion options available to Guild Leaders</returns>
    private static IEnumerable<PromotionOption> GetGuildLeaderOptions() {
        return [
            // Leadership transfer
            new PromotionOption(MemberRank.GuildLeader, 1, 0),

            // Deputy Leaders
            new PromotionOption(MemberRank.DeputyLeader, 0, 0, true,
                CreateLimitCheck(MemberRank.DeputyLeader, GuildRankLimits.GetMaxDeputyLeader),
                "Sorry all DeputyLeader ranks are occupied!"),

            // Honorary officials (cost CPs)
            new PromotionOption(MemberRank.HDeputyLeader, 0, 650, true,
                CreateLimitCheck(MemberRank.HDeputyLeader, GuildRankLimits.GetMaxHonoraryDeputyLeader),
                "Sorry all Honorary Deputy Leader ranks are occupied!"),
            new PromotionOption(MemberRank.HonoraryManager, 0, 320, true,
                CreateLimitCheck(MemberRank.HonoraryManager, GuildRankLimits.GetMaxHonoraryManager),
                "Sorry all Honorary Manager ranks are occupied!"),
            new PromotionOption(MemberRank.HonorarySupervisor, 0, 270, true,
                CreateLimitCheck(MemberRank.HonorarySupervisor, GuildRankLimits.GetMaxHonorarySupervisor),
                "Sorry all Honorary Supervisor ranks are occupied!"),
            new PromotionOption(MemberRank.HonorarySteward, 0, 100, true,
                CreateLimitCheck(MemberRank.HonorarySteward, GuildRankLimits.GetMaxHonorarySteward),
                "Sorry all Honorary Steward ranks are occupied!"),

            // Leader Aides
            new PromotionOption(MemberRank.LSpouseAide, 0, 0, true,
                CreateLimitCheck(MemberRank.LSpouseAide, _ => GuildRankLimits.GetMaxAide(0)),
                "Sorry all Leader Aide ranks are occupied!"),

            // Deputy Leader Aides (manually assignable)
            new PromotionOption(MemberRank.DLeaderAide, 0, 0, true,
                CreateLimitCheck(MemberRank.DLeaderAide, _ => GuildRankLimits.GetMaxAide(0)),
                "Sorry all Deputy Leader Aide ranks are occupied!"),

            // Aides (manually assignable)
            new PromotionOption(MemberRank.Aide, 0, 0, true,
                CreateLimitCheck(MemberRank.Aide, _ => GuildRankLimits.GetMaxAide(0)),
                "Sorry all Aide ranks are occupied!"),
            new PromotionOption(MemberRank.ManagerAide, 0, 0, true,
                CreateLimitCheck(MemberRank.ManagerAide, _ => GuildRankLimits.GetMaxAide(0)),
                "Sorry all Manager Aide ranks are occupied!"),
            new PromotionOption(MemberRank.SupervisorAide, 0, 0, true,
                CreateLimitCheck(MemberRank.SupervisorAide, _ => GuildRankLimits.GetMaxAide(0)),
                "Sorry all Supervisor Aide ranks are occupied!"),

            // Members (can be demoted to)
            new PromotionOption(MemberRank.Member, 0, 0, true)
        ];
    }

    /// <summary>
    ///     Defines ranks that Deputy Leaders (including Honorary Deputy Leaders and Leader Spouses)
    ///     can promote members to. Deputy Leaders have limited promotion authority, primarily for
    ///     Honorary Stewards, Deputy Leader Aides, and can demote members to Member rank.
    /// </summary>
    /// <returns>Promotion options available to Deputy Leaders</returns>
    private static IEnumerable<PromotionOption> GetDeputyLeaderOptions() {
        return [
            // Honorary Steward (manually assignable)
            new PromotionOption(MemberRank.HonorarySteward, 0, 0, true,
                CreateLimitCheck(MemberRank.HonorarySteward, GuildRankLimits.GetMaxHonorarySteward),
                "Sorry all Honorary Steward ranks are occupied!"),
            // Deputy Leader Aide (manually assignable)
            new PromotionOption(MemberRank.DLeaderAide, 0, 0, true,
                CreateLimitCheck(MemberRank.DLeaderAide, _ => GuildRankLimits.GetMaxAide(0)),
                "Sorry all Deputy Leader Aide ranks are occupied!"),
            // Members (can be demoted to)
            new PromotionOption(MemberRank.Member, 0, 0, true)
        ];
    }

    /// <summary>
    ///     Defines ranks that Managers (including Honorary Managers) can promote members to.
    ///     Managers can only promote members to Manager Aide positions, reflecting their limited
    ///     authority in the guild hierarchy.
    /// </summary>
    /// <returns>Promotion options available to Managers</returns>
    private static IEnumerable<PromotionOption> GetManagerOptions() {
        return [
            new PromotionOption(MemberRank.ManagerAide, 0, 0, true,
                CreateLimitCheck(MemberRank.ManagerAide, _ => GuildRankLimits.GetMaxAide(0)),
                "Sorry all Manager Aide ranks are occupied!")
        ];
    }

    /// <summary>
    ///     Defines ranks that Supervisors (including all specialized Supervisor types like Tulip,
    ///     Orchid, CP, Arsenal, Silver, Guide, PK, Rose, and Lily Supervisors) can promote members to.
    ///     Supervisors can only promote members to Supervisor Aide positions.
    /// </summary>
    /// <returns>Promotion options available to Supervisors</returns>
    private static IEnumerable<PromotionOption> GetSupervisorOptions() {
        return [
            new PromotionOption(MemberRank.SupervisorAide, 0, 0, true,
                CreateLimitCheck(MemberRank.SupervisorAide, _ => GuildRankLimits.GetMaxAide(0)),
                "Sorry all Supervisor Aide ranks are occupied!")
        ];
    }

    /// <summary>
    ///     Defines ranks that Agents can promote members to. Agents have the most limited promotion
    ///     authority, only able to promote members to basic Aide positions.
    /// </summary>
    /// <returns>Promotion options available to Agents</returns>
    private static IEnumerable<PromotionOption> GetAgentOptions() {
        return [
            new PromotionOption(MemberRank.Aide, 0, 0, true,
                CreateLimitCheck(MemberRank.Aide, _ => GuildRankLimits.GetMaxAide(0)),
                "Sorry all Aide ranks are occupied!")
        ];
    }

    /// <summary>
    ///     Calculates the maximum number of members allowed for a specific rank based on the guild's
    ///     current level. Some ranks have fixed limits, while others scale with guild level (e.g.,
    ///     higher level guilds can have more Deputy Leaders). Used for client-side display and validation.
    /// </summary>
    /// <param name="option">The promotion option to get the limit for</param>
    /// <param name="guildLevel">The current level of the guild</param>
    /// <returns>The maximum number of members allowed for this rank</returns>
    public static int GetMaxLimit(PromotionOption option, byte guildLevel) {
        if (!option.UseGuildRankLimits) return option.MaxLimit;

        return option.Rank switch {
            MemberRank.DeputyLeader => GuildRankLimits.GetMaxDeputyLeader(guildLevel),
            MemberRank.HDeputyLeader => GuildRankLimits.GetMaxHonoraryDeputyLeader(guildLevel),
            MemberRank.HonoraryManager => GuildRankLimits.GetMaxHonoraryManager(guildLevel),
            MemberRank.HonorarySupervisor => GuildRankLimits.GetMaxHonorarySupervisor(guildLevel),
            MemberRank.HonorarySteward => GuildRankLimits.GetMaxHonorarySteward(guildLevel),
            MemberRank.LSpouseAide or MemberRank.DLeaderAide or MemberRank.Aide or
                MemberRank.ManagerAide or MemberRank.SupervisorAide => GuildRankLimits.GetMaxAide(guildLevel),
            MemberRank.Member => GuildRankLimits.GetMaxMembers(guildLevel),
            _ => option.MaxLimit
        };
    }

    /// <summary>
    ///     Retrieves a specific promotion option for server-side validation. Returns null if the
    ///     promoting rank doesn't have permission to promote to the target rank, which is used to
    ///     deny unauthorized promotion attempts.
    /// </summary>
    /// <param name="promotingRank">The rank of the player attempting to promote</param>
    /// <param name="targetRank">The rank they want to promote to</param>
    /// <returns>The promotion option if valid, null if not permitted</returns>
    public static PromotionOption? GetPromotionOption(MemberRank promotingRank, MemberRank targetRank) {
        var options = GetPromotionOptions(promotingRank);
        return options.FirstOrDefault(opt => opt.Rank == targetRank);
    }

    /// <summary>
    ///     Represents a single promotion option that defines what rank can be promoted to, its cost,
    ///     limits, and validation rules. Each option specifies the target rank, maximum allowed count,
    ///     Conquer Points cost (for honorary ranks), and whether limits scale with guild level.
    /// </summary>
    public record PromotionOption(
        MemberRank Rank,
        int MaxLimit, // Use 999 for unlimited, or call GuildRankLimits method
        int ConquerPointsCost,
        bool UseGuildRankLimits = false, // If true, MaxLimit will be calculated from GuildRankLimits
        LimitCheckDelegate? LimitCheck = null, // Returns true if limit is reached (for server-side validation)
        string? LimitErrorMessage = null // Error message to show when limit is reached
    );
}