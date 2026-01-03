using System;
using System.Linq;
using MTA.Game.Features.Guilds.Constants;
using MTA.Game.Features.Guilds.Models;

namespace MTA.Game.Features.Guilds.Services;

public static class GuildRankAssignment {
    private const byte MaxAgent = 2;
    private const byte MaxFollower = 2;

    private record DonationRankConfig(
        Func<GuildMember, ulong> DonationSelector,
        MemberRank SupervisorRank,
        MemberRank AgentRank,
        MemberRank FollowerRank,
        Action<Guild, GuildMember[]> SetRankingArray);

    private record ArsenalRankConfig(
        MemberRank ManagerRank,
        MemberRank SupervisorRank,
        MemberRank StewardRank,
        MemberRank FollowerRank);

    /// <summary>
    ///     Automatically assigns guild ranks to members based on their donation performance across all donation types.
    ///     Resets all ranks below leadership level, then assigns Manager/Supervisor/Agent/Follower ranks based on donation
    ///     rankings.
    /// </summary>
    public static void AssignRanks(Guild guild) {
        lock (guild) {
            // Remove all ranks below 920 (keep Guild Leader, Deputy Leader, etc.)
            ResetRanks(guild);

            // Assign ranks based on donation types
            AssignArsenalRanks(guild);
            AssignConquerPointRanks(guild);
            AssignPkRanks(guild);
            AssignRoseRanks(guild);
            AssignLilyRanks(guild);
            AssignTulipRanks(guild);
            AssignOrchidRanks(guild);
            AssignSilverRanks(guild);
            AssignGuideRanks(guild);

            // Calculate total donation rankings (for display purposes only)
            CalculateTotalDonationRankings(guild);
        }
    }

    /// <summary>
    ///     Resets all member ranks to basic Member rank, preserving only leadership ranks (Guild Leader, Deputy Leader, etc.).
    /// </summary>
    private static void ResetRanks(Guild guild) {
        foreach (var member in guild.Members.Values.Where(member => (ushort)member.Rank < 920)) {
            if (guild.RanksCounts[(ushort)member.Rank] > 0)
                guild.RanksCounts[(ushort)member.Rank]--;
            member.Rank = MemberRank.Member;
            guild.RanksCounts[(ushort)member.Rank]++;
        }
    }

    /// <summary>
    ///     Assigns CP donation ranks: top donors become CP Supervisor, Agent, or Follower based on their Conquer Point
    ///     donations.
    /// </summary>
    private static void AssignConquerPointRanks(Guild guild) {
        AssignStandardDonationRanks(guild, new DonationRankConfig(
            m => m.ConquerPointDonation,
            MemberRank.CPSupervisor,
            MemberRank.CPAgent,
            MemberRank.CPFollower,
            (g, poll) => g.RankCpDonations = poll));
    }

    /// <summary>
    ///     Assigns PK donation ranks: top donors become PK Supervisor, Agent, or Follower based on their PK donations.
    /// </summary>
    private static void AssignPkRanks(Guild guild) {
        AssignStandardDonationRanks(guild, new DonationRankConfig(
            m => m.PkDonation,
            MemberRank.PKSupervisor,
            MemberRank.PKAgent,
            MemberRank.PKFollower,
            (g, poll) => g.RankPkDonations = poll));
    }

    /// <summary>
    ///     Assigns Rose donation ranks: top donors become Rose Supervisor, Agent, or Follower based on their Rose donations.
    /// </summary>
    private static void AssignRoseRanks(Guild guild) {
        AssignStandardDonationRanks(guild, new DonationRankConfig(
            m => m.Roses,
            MemberRank.RoseSupervisor,
            MemberRank.RoseAgent,
            MemberRank.RoseFollower,
            (g, poll) => g.RankRoseDonations = poll));
    }

    /// <summary>
    ///     Assigns Lily donation ranks: top donors become Lily Supervisor, Agent, or Follower based on their Lily donations.
    /// </summary>
    private static void AssignLilyRanks(Guild guild) {
        AssignStandardDonationRanks(guild, new DonationRankConfig(
            m => m.Lilies,
            MemberRank.LilySupervisor,
            MemberRank.LilyAgent,
            MemberRank.LilyFollower,
            (g, poll) => g.RankLiliesDonations = poll));
    }

    /// <summary>
    ///     Assigns Tulip donation ranks: top donors become Tulip Supervisor, Agent, or Follower based on their Tulip
    ///     donations.
    /// </summary>
    private static void AssignTulipRanks(Guild guild) {
        AssignStandardDonationRanks(guild, new DonationRankConfig(
            m => m.Tulips,
            MemberRank.TSupervisor,
            MemberRank.TulipAgent,
            MemberRank.TulipFollower,
            (g, poll) => g.RankTulipsDonations = poll));
    }

    /// <summary>
    ///     Assigns Orchid donation ranks: top donors become Orchid Supervisor, Agent, or Follower based on their Orchid
    ///     donations.
    /// </summary>
    private static void AssignOrchidRanks(Guild guild) {
        AssignStandardDonationRanks(guild, new DonationRankConfig(
            m => m.Orchids,
            MemberRank.OSupervisor,
            MemberRank.OrchidAgent,
            MemberRank.OrchidFollower,
            (g, poll) => g.RankOrchidsDonations = poll));
    }

    /// <summary>
    ///     Assigns Silver donation ranks: top donors become Silver Supervisor, Agent, or Follower based on their Silver
    ///     donations.
    /// </summary>
    private static void AssignSilverRanks(Guild guild) {
        AssignStandardDonationRanks(guild, new DonationRankConfig(
            m => m.SilverDonation,
            MemberRank.SSupervisor,
            MemberRank.SilverAgent,
            MemberRank.SilverFollower,
            (g, poll) => g.RankSilversDonations = poll));
    }

    /// <summary>
    ///     Assigns Guide donation ranks: top donors become Guide Supervisor, Agent, or Follower based on their Virtue Point
    ///     donations.
    /// </summary>
    private static void AssignGuideRanks(Guild guild) {
        AssignStandardDonationRanks(guild, new DonationRankConfig(
            m => m.VirtuePoints,
            MemberRank.GSupervisor,
            MemberRank.GuideAgent,
            MemberRank.GuideFollower,
            (g, poll) => g.RankGuideDonations = poll));
    }

    /// <summary>
    ///     Calculates total donation rankings for display purposes. Does not assign ranks (HDeputyLeader and HonorarySteward
    ///     are manual-only).
    /// </summary>
    private static void CalculateTotalDonationRankings(Guild guild) {
        guild.RankTotalDonations = guild.Members.Values
            .OrderByDescending(m => m.TotalDonation)
            .ToArray();
    }

    /// <summary>
    ///     Assigns ranks for standard donation types: Supervisor → Agent → Follower based on donation amounts and guild level
    ///     limits.
    /// </summary>
    private static void AssignStandardDonationRanks(Guild guild, DonationRankConfig config) {
        var maxSupervisor = GuildRankLimits.GetMaxSupervisorPerType(guild.Level);
        var poll = guild.Members.Values
            .OrderByDescending(config.DonationSelector)
            .Where(m => config.DonationSelector(m) > 0)
            .ToArray();

        byte assigned = 0;
        foreach (var member in poll) {
            if (member.Rank > config.SupervisorRank) continue;

            if (assigned < maxSupervisor) {
                AssignRank(guild, member, config.SupervisorRank);
            }
            else if (assigned < maxSupervisor + MaxAgent) {
                if (member.Rank > config.AgentRank) continue;
                AssignRank(guild, member, config.AgentRank);
            }
            else if (assigned < maxSupervisor + MaxAgent + MaxFollower) {
                if (member.Rank > config.FollowerRank) continue;
                AssignRank(guild, member, config.FollowerRank);
            }
            else {
                break;
            }

            assigned++;
        }

        config.SetRankingArray(guild, poll);
    }

    /// <summary>
    ///     Assigns ranks for Arsenal donations: Manager → Supervisor → Steward → Follower based on donation amounts and guild
    ///     level limits.
    /// </summary>
    private static void AssignArsenalRanks(Guild guild) {
        var maxManager = GuildRankLimits.GetMaxManager(guild.Level);
        var maxSupervisor = GuildRankLimits.GetMaxSupervisorPerType(guild.Level);
        var maxSteward = GuildRankLimits.GetMaxSteward(guild.Level);
        const byte maxFollower = 2;

        var poll = guild.Members.Values
            .OrderByDescending(m => m.ArsenalDonation)
            .Where(m => m.ArsenalDonation > 0)
            .ToArray();

        var config = new ArsenalRankConfig(
            MemberRank.Manager,
            MemberRank.Supervisor,
            MemberRank.Steward,
            MemberRank.ArsFollower);

        byte assigned = 0;
        foreach (var member in poll) {
            if (maxManager > 0 && assigned < maxManager) {
                if (member.Rank > config.ManagerRank) continue;
                AssignRank(guild, member, config.ManagerRank);
            }
            else if (assigned < maxManager + maxSupervisor) {
                if (member.Rank > config.SupervisorRank) continue;
                AssignRank(guild, member, config.SupervisorRank);
            }
            else if (maxSteward > 0 && assigned < maxManager + maxSupervisor + maxSteward) {
                if (member.Rank > config.StewardRank) continue;
                AssignRank(guild, member, config.StewardRank);
            }
            else if (assigned < maxManager + maxSupervisor + maxSteward + maxFollower) {
                if (member.Rank > config.FollowerRank) continue;
                AssignRank(guild, member, config.FollowerRank);
            }
            else {
                break;
            }

            assigned++;
        }

        guild.RankArsenalDonations = poll;
    }

    /// <summary>
    ///     Updates a member's rank and adjusts the guild's rank count tracking.
    /// </summary>
    private static void AssignRank(Guild guild, GuildMember member, MemberRank newRank) {
        var oldRankIndex = (ushort)member.Rank;
        if (oldRankIndex < guild.RanksCounts.Length && guild.RanksCounts[oldRankIndex] > 0)
            guild.RanksCounts[oldRankIndex]--;

        member.Rank = newRank;
        var newRankIndex = (ushort)newRank;
        if (newRankIndex < guild.RanksCounts.Length)
            guild.RanksCounts[newRankIndex]++;
    }
}