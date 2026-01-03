using System;
using System.Linq;
using MTA.Game.Features.Guilds.Constants;
using MTA.Game.Features.Guilds.Models;

namespace MTA.Game.Features.Guilds.Services;

public static class GuildRankAssignment {
    private const byte MaxAgent = 2;
    private const byte MaxFollower = 2;

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
    ///     Assigns ranks based on Arsenal donations: top donor becomes Manager, followed by Supervisor, Steward, and Arsenal
    ///     Follower.
    /// </summary>
    private static void AssignArsenalRanks(Guild guild) {
        var maxManager = GuildRankLimits.GetMaxManager(guild.Level);
        var maxSupervisor = GuildRankLimits.GetMaxSupervisorPerType(guild.Level);
        var maxSteward = GuildRankLimits.GetMaxSteward(guild.Level);
        const byte maxArsFollower = 2;

        var poll = guild.Members.Values
            .OrderByDescending(m => m.ArsenalDonation)
            .ToArray();

        AssignRanksForDonationType(
            guild,
            poll,
            maxManager,
            maxSupervisor,
            maxSteward,
            maxArsFollower,
            MemberRank.Manager,
            MemberRank.Supervisor,
            MemberRank.Steward,
            MemberRank.ArsFollower);

        guild.RankArsenalDonations = poll;
    }

    /// <summary>
    ///     Assigns CP donation ranks: top donors become CP Supervisor, Agent, or Follower based on their Conquer Point
    ///     donations.
    /// </summary>
    private static void AssignConquerPointRanks(Guild guild) {
        AssignStandardDonationRanks(
            guild,
            m => m.ConquerPointDonation,
            MemberRank.CPSupervisor,
            MemberRank.CPAgent,
            MemberRank.CPFollower,
            (g, poll) => g.RankCpDonations = poll);
    }

    /// <summary>
    ///     Assigns PK donation ranks: top donors become PK Supervisor, Agent, or Follower based on their PK donations.
    /// </summary>
    private static void AssignPkRanks(Guild guild) {
        AssignStandardDonationRanks(
            guild,
            m => m.PkDonation,
            MemberRank.PKSupervisor,
            MemberRank.PKAgent,
            MemberRank.PKFollower,
            (g, poll) => g.RankPkDonations = poll);
    }

    /// <summary>
    ///     Assigns Rose donation ranks: top donors become Rose Supervisor, Agent, or Follower based on their Rose donations.
    /// </summary>
    private static void AssignRoseRanks(Guild guild) {
        AssignStandardDonationRanks(
            guild,
            m => m.Roses,
            MemberRank.RoseSupervisor,
            MemberRank.RoseAgent,
            MemberRank.RoseFollower,
            (g, poll) => g.RankRoseDonations = poll);
    }

    /// <summary>
    ///     Assigns Lily donation ranks: top donors become Lily Supervisor, Agent, or Follower based on their Lily donations.
    /// </summary>
    private static void AssignLilyRanks(Guild guild) {
        AssignStandardDonationRanks(
            guild,
            m => m.Lilies,
            MemberRank.LilySupervisor,
            MemberRank.LilyAgent,
            MemberRank.LilyFollower,
            (g, poll) => g.RankLiliesDonations = poll);
    }

    /// <summary>
    ///     Assigns Tulip donation ranks: top donors become Tulip Supervisor, Agent, or Follower based on their Tulip
    ///     donations.
    /// </summary>
    private static void AssignTulipRanks(Guild guild) {
        AssignStandardDonationRanks(
            guild,
            m => m.Tulips,
            MemberRank.TSupervisor,
            MemberRank.TulipAgent,
            MemberRank.TulipFollower,
            (g, poll) => g.RankTulipsDonations = poll);
    }

    /// <summary>
    ///     Assigns Orchid donation ranks: top donors become Orchid Supervisor, Agent, or Follower based on their Orchid
    ///     donations.
    /// </summary>
    private static void AssignOrchidRanks(Guild guild) {
        AssignStandardDonationRanks(
            guild,
            m => m.Orchids,
            MemberRank.OSupervisor,
            MemberRank.OrchidAgent,
            MemberRank.OrchidFollower,
            (g, poll) => g.RankOrchidsDonations = poll);
    }

    /// <summary>
    ///     Assigns Silver donation ranks: top donors become Silver Supervisor, Agent, or Follower based on their Silver
    ///     donations.
    /// </summary>
    private static void AssignSilverRanks(Guild guild) {
        AssignStandardDonationRanks(
            guild,
            m => m.SilverDonation,
            MemberRank.SSupervisor,
            MemberRank.SilverAgent,
            MemberRank.SilverFollower,
            (g, poll) => g.RankSilversDonations = poll);
    }

    /// <summary>
    ///     Assigns Guide donation ranks: top donors become Guide Supervisor, Agent, or Follower based on their Virtue Point
    ///     donations.
    /// </summary>
    private static void AssignGuideRanks(Guild guild) {
        AssignStandardDonationRanks(
            guild,
            m => m.VirtuePoints,
            MemberRank.GSupervisor,
            MemberRank.GuideAgent,
            MemberRank.GuideFollower,
            (g, poll) => g.RankGuideDonations = poll);
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
    ///     Assigns ranks for Arsenal donations: Manager → Supervisor → Steward → Follower based on donation amounts and guild
    ///     level limits.
    /// </summary>
    private static void AssignRanksForDonationType(
        Guild guild,
        GuildMember[] poll,
        byte maxManager,
        byte maxSupervisor,
        byte maxSteward,
        byte maxFollower,
        MemberRank managerRank,
        MemberRank supervisorRank,
        MemberRank stewardRank,
        MemberRank followerRank) {
        byte amount = 0;

        for (byte x = 0; x < poll.Length; x++) {
            var member = poll[x];

            if (maxManager > 0 && amount < maxManager) {
                if (member.Rank > managerRank) continue;
                AssignRank(guild, member, managerRank);
                amount++;
            }
            else if (amount < maxManager + maxSupervisor) {
                if (member.Rank > supervisorRank) continue;
                AssignRank(guild, member, supervisorRank);
                amount++;
            }
            else if (maxSteward > 0 && amount < maxManager + maxSupervisor + maxSteward) {
                if (member.Rank > stewardRank) continue;
                AssignRank(guild, member, stewardRank);
                amount++;
            }
            else if (amount < maxManager + maxSupervisor + maxSteward + maxFollower) {
                if (member.Rank > followerRank) continue;
                AssignRank(guild, member, followerRank);
                amount++;
            }
            else {
                break;
            }
        }
    }

    /// <summary>
    ///     Assigns ranks for standard donation types: Supervisor → Agent → Follower based on donation amounts and guild level
    ///     limits.
    /// </summary>
    private static void AssignRanksForDonationType(
        Guild guild,
        GuildMember[] poll,
        byte maxSupervisor,
        byte maxAgent,
        byte maxFollower,
        MemberRank supervisorRank,
        MemberRank agentRank,
        MemberRank followerRank) {
        byte amount = 0;

        for (byte x = 0; x < poll.Length; x++) {
            var member = poll[x];

            if (amount < maxSupervisor) {
                if (member.Rank > supervisorRank) continue;
                AssignRank(guild, member, supervisorRank);
                amount++;
            }
            else if (amount < maxSupervisor + maxAgent) {
                if (member.Rank > agentRank) continue;
                AssignRank(guild, member, agentRank);
                amount++;
            }
            else if (amount < maxSupervisor + maxAgent + maxFollower) {
                if (member.Rank > followerRank) continue;
                AssignRank(guild, member, followerRank);
                amount++;
            }
            else {
                break;
            }
        }
    }

    /// <summary>
    ///     Generic helper that assigns Supervisor, Agent, and Follower ranks for any donation type based on donation amounts.
    /// </summary>
    private static void AssignStandardDonationRanks(
        Guild guild,
        Func<GuildMember, ulong> donationSelector,
        MemberRank supervisorRank,
        MemberRank agentRank,
        MemberRank followerRank,
        Action<Guild, GuildMember[]> setRankingArray) {
        var maxSupervisor = GuildRankLimits.GetMaxSupervisorPerType(guild.Level);

        var poll = guild.Members.Values
            .OrderByDescending(donationSelector)
            .ToArray();

        AssignRanksForDonationType(
            guild,
            poll,
            maxSupervisor,
            MaxAgent,
            MaxFollower,
            supervisorRank,
            agentRank,
            followerRank);

        setRankingArray(guild, poll);
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