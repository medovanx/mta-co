using System.Collections.Generic;
using MTA.Game.Features.Guilds.Constants;

namespace MTA.Game.Features.Guilds.Handlers;

/// <summary>
/// Defines promotion options for each rank in a data-driven way.
/// </summary>
public static class GuildPromotionOptions {
    /// <summary>
    /// Represents a promotion option with its configuration.
    /// </summary>
    public record PromotionOption(
        MemberRank Rank,
        int MaxLimit, // Use 999 for unlimited, or call GuildRankLimits method
        int ConquerPointsCost,
        bool UseGuildRankLimits = false // If true, MaxLimit will be calculated from GuildRankLimits
    );

    /// <summary>
    /// Gets all promotion options available for a given rank.
    /// </summary>
    public static IEnumerable<PromotionOption> GetPromotionOptions(MemberRank promotingRank) {
        return promotingRank switch {
            MemberRank.GuildLeader => GetGuildLeaderOptions(),
            MemberRank.DeputyLeader or MemberRank.HDeputyLeader or MemberRank.LeaderSpouse => GetDeputyLeaderOptions(),
            MemberRank.Manager or MemberRank.HonoraryManager => GetManagerOptions(),
            MemberRank.Supervisor or MemberRank.HonorarySupervisor or
                MemberRank.TSupervisor or MemberRank.OSupervisor or
                MemberRank.CPSupervisor or MemberRank.ASupervisor or
                MemberRank.SSupervisor or MemberRank.GSupervisor or
                MemberRank.PKSupervisor or MemberRank.RoseSupervisor or
                MemberRank.LilySupervisor => GetSupervisorOptions(),
            MemberRank.Agent => GetAgentOptions(),
            _ => []
        };
    }

    private static IEnumerable<PromotionOption> GetGuildLeaderOptions() {
        return [
            // Leadership transfer
            new PromotionOption(MemberRank.GuildLeader, 1, 0),

            // Deputy Leaders
            new PromotionOption(MemberRank.DeputyLeader, 0, 0, UseGuildRankLimits: true),

            // Honorary officials (cost CPs)
            new PromotionOption(MemberRank.HDeputyLeader, 0, 650, UseGuildRankLimits: true),
            new PromotionOption(MemberRank.HonoraryManager, 0, 320, UseGuildRankLimits: true),
            new PromotionOption(MemberRank.HonorarySupervisor, 0, 270, UseGuildRankLimits: true),
            new PromotionOption(MemberRank.HonorarySteward, 0, 100, UseGuildRankLimits: true),

            // Leader Aides
            new PromotionOption(MemberRank.LSpouseAide, 0, 0, UseGuildRankLimits: true),

            // Stewards
            new PromotionOption(MemberRank.Steward, 0, 0, UseGuildRankLimits: true),
            new PromotionOption(MemberRank.DeputySteward, 999, 0), // No limit
            new PromotionOption(MemberRank.DLeaderSpouse, 1, 0),
            new PromotionOption(MemberRank.DLeaderAide, 0, 0, UseGuildRankLimits: true),

            // Aides
            new PromotionOption(MemberRank.Aide, 0, 0, UseGuildRankLimits: true),
            new PromotionOption(MemberRank.ManagerAide, 0, 0, UseGuildRankLimits: true),
            new PromotionOption(MemberRank.SupervisorAide, 0, 0, UseGuildRankLimits: true),

            // Agents (no limit per guide)
            new PromotionOption(MemberRank.Agent, 999, 0),
            new PromotionOption(MemberRank.TulipAgent, 1, 0), // 1 per flower type
            new PromotionOption(MemberRank.OrchidAgent, 1, 0),
            new PromotionOption(MemberRank.RoseAgent, 1, 0),
            new PromotionOption(MemberRank.LilyAgent, 1, 0),
            new PromotionOption(MemberRank.CPAgent, 1, 0), // 1 per donation type
            new PromotionOption(MemberRank.ArsenalAgent, 1, 0),
            new PromotionOption(MemberRank.SilverAgent, 1, 0),
            new PromotionOption(MemberRank.GuideAgent, 1, 0),
            new PromotionOption(MemberRank.PKAgent, 1, 0),

            // Spouse ranks
            new PromotionOption(MemberRank.SupervSpouse, 1, 0),
            new PromotionOption(MemberRank.ManagerSpouse, 1, 0),
            new PromotionOption(MemberRank.StewardSpouse, 1, 0),

            // Followers
            new PromotionOption(MemberRank.Follower, 0, 0, UseGuildRankLimits: true),
            new PromotionOption(MemberRank.TulipFollower, 1, 0), // 1 per flower type
            new PromotionOption(MemberRank.OrchidFollower, 1, 0),
            new PromotionOption(MemberRank.RoseFollower, 1, 0),
            new PromotionOption(MemberRank.LilyFollower, 1, 0),
            new PromotionOption(MemberRank.CPFollower, 1, 0), // 1 per donation type
            new PromotionOption(MemberRank.ArsFollower, 1, 0),
            new PromotionOption(MemberRank.SilverFollower, 1, 0),
            new PromotionOption(MemberRank.GuideFollower, 1, 0),
            new PromotionOption(MemberRank.PKFollower, 1, 0),

            // Senior Member
            new PromotionOption(MemberRank.SeniorMember, 999, 0), // No limit

            // Members
            new PromotionOption(MemberRank.Member, 0, 0, UseGuildRankLimits: true)
        ];
    }

    private static IEnumerable<PromotionOption> GetDeputyLeaderOptions() {
        return [
            new PromotionOption(MemberRank.Steward, 0, 0, UseGuildRankLimits: true),
            new PromotionOption(MemberRank.HonorarySteward, 0, 0, UseGuildRankLimits: true),
            new PromotionOption(MemberRank.DLeaderAide, 0, 0, UseGuildRankLimits: true),
            new PromotionOption(MemberRank.Follower, 0, 0, UseGuildRankLimits: true),
            new PromotionOption(MemberRank.Member, 0, 0, UseGuildRankLimits: true)
        ];
    }

    private static IEnumerable<PromotionOption> GetManagerOptions() {
        return [
            new PromotionOption(MemberRank.ManagerAide, 0, 0, UseGuildRankLimits: true)
        ];
    }

    private static IEnumerable<PromotionOption> GetSupervisorOptions() {
        return [
            new PromotionOption(MemberRank.SupervisorAide, 0, 0, UseGuildRankLimits: true)
        ];
    }

    private static IEnumerable<PromotionOption> GetAgentOptions() {
        return [
            new PromotionOption(MemberRank.Aide, 0, 0, UseGuildRankLimits: true)
        ];
    }

    /// <summary>
    /// Gets the max limit for a promotion option, using GuildRankLimits if needed.
    /// </summary>
    public static int GetMaxLimit(PromotionOption option, byte guildLevel) {
        if (!option.UseGuildRankLimits) {
            return option.MaxLimit;
        }

        return option.Rank switch {
            MemberRank.DeputyLeader => GuildRankLimits.GetMaxDeputyLeader(guildLevel),
            MemberRank.HDeputyLeader => GuildRankLimits.GetMaxHonoraryDeputyLeader(guildLevel),
            MemberRank.HonoraryManager => GuildRankLimits.GetMaxHonoraryManager(guildLevel),
            MemberRank.HonorarySupervisor => GuildRankLimits.GetMaxHonorarySupervisor(guildLevel),
            MemberRank.HonorarySteward => GuildRankLimits.GetMaxHonorarySteward(guildLevel),
            MemberRank.LSpouseAide or MemberRank.DLeaderAide or MemberRank.Aide or
                MemberRank.ManagerAide or MemberRank.SupervisorAide => GuildRankLimits.GetMaxAide(guildLevel),
            MemberRank.Steward => GuildRankLimits.GetMaxSteward(guildLevel),
            MemberRank.Follower => GuildRankLimits.GetMaxFollower(guildLevel),
            MemberRank.Member => GuildRankLimits.GetMaxMembers(guildLevel),
            _ => option.MaxLimit
        };
    }
}