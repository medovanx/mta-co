// ReSharper disable InconsistentNaming

namespace MTA.Game.Features.Guilds.Database.Schema;

/// <summary>
///     Centralized schema definition for all guild-related database tables and columns.
///     All database column name references should use these constants.
/// </summary>
public static class GuildSchema {
    /// <summary>
    ///     Table name constants
    /// </summary>
    public static class Tables {
        public const string GuildsTable = "guilds";
        public const string GuildRelationsTable = "guild_relations";
        public const string GuildArsenalTable = "guildarsenal";
        public const string GuildWarHistoryTable = "guild_war_history";
        public const string GuildMembersTable = "guild_members";
    }

    /// <summary>
    ///     Column names for the `guilds` table
    /// </summary>
    public static class Guilds {
        public const string Id = "ID";
        public const string Name = "Name";
        public const string Bulletin = "Bulletin";
        public const string SilverFund = "SilverFund";
        public const string LeaderID = "LeaderID";
        public const string Wins = "Wins";
        public const string Losts = "Losts";
        public const string CTFPoints = "CTFPoints";
        public const string CTFReward = "CTFReward";
        public const string ConquerPointFund = "ConquerPointFund";
        public const string LevelRequirement = "LevelRequirement";
        public const string RebornRequirement = "RebornRequirement";
        public const string ClassRequirement = "ClassRequirement";
        public const string Advertise = "Advertise";
        public const string GuildEnroll = "GuildEnroll";
        public const string BulletinEnroll = "BulletinEnroll";
        public const string CTFDonationCps = "CTFDonationCps";
        public const string CTFDonationSilver = "CTFDonationSilver";
        public const string CTFdonationSilverold = "CTFdonationSilverold";
        public const string CTFdonationCpsold = "CTFdonationCpsold";
    }

    /// <summary>
    ///     Column names for the `guild_relations` table
    /// </summary>
    public static class GuildRelations {
        public const string GuildId = "guild_id";
        public const string RelatedGuildId = "related_guild_id";
        public const string RelationType = "relation_type";
    }

    /// <summary>
    ///     Column names for the `guildarsenal` table
    /// </summary>
    public static class GuildArsenal {
        public const string Id = "ID";
        public const string Data = "Data";
        public const string DataLength = "DataLength";
    }

    /// <summary>
    ///     Column names for the `guild_war_history` table
    /// </summary>
    public static class GuildWarHistory {
        public const string Id = "id";
        public const string GuildId = "guild_id";
        public const string GuildLeaderEntityId = "guild_leader_entity_id";
        public const string GuildLeaderName = "guild_leader_name";
        public const string GuildLeaderClaimed = "guild_leader_claimed";
        public const string DeputyClaimedIds = "deputy_claimed_ids";
        public const string WarEndTime = "war_end_time";
    }

    /// <summary>
    ///     Column names for the `guild_members` table
    /// </summary>
    public static class GuildMembers {
        public const string EntityId = "entity_id";
        public const string GuildId = "guild_id";
        public const string Rank = "rank";
        public const string SilverDonation = "silver_donation";
        public const string ConquerPointDonation = "conquer_point_donation";
        public const string ArsenalDonation = "arsenal_donation";
        public const string Lilies = "lilies";
        public const string Roses = "roses";
        public const string Orchids = "orchids";
        public const string Tulips = "tulips";
        public const string PkDonation = "pk_donation";
        public const string LastLogin = "last_login";
        public const string Exploits = "exploits";
        public const string CtfCpsReward = "ctf_cps_reward";
        public const string CtfSilverReward = "ctf_silver_reward";
    }
}