using System;
using System.Collections.Generic;
using MTA.Database;
using MTA.Game.Features.Guilds.Database.Models;
using MTA.Game.Features.Guilds.Database.Schema;

namespace MTA.Game.Features.Guilds.Database.Mappers;

/// <summary>
///     Mapper functions that convert MySqlReader to strongly-typed models.
///     All database column name references use GuildSchema constants.
/// </summary>
public static class GuildMappers {
    /// <summary>
    ///     Maps a MySqlReader to a GuildRecord from the `guilds` table
    /// </summary>
    public static GuildRecord MapGuild(MySqlReader reader) {
        return new GuildRecord {
            Id = reader.ReadUInt32(GuildSchema.Guilds.Id),
            Name = reader.ReadString(GuildSchema.Guilds.Name),
            Bulletin = reader.ReadString(GuildSchema.Guilds.Bulletin),
            SilverFund = reader.ReadUInt64(GuildSchema.Guilds.SilverFund),
            LeaderID = reader.ReadUInt64(GuildSchema.Guilds.LeaderID),
            Wins = reader.ReadUInt32(GuildSchema.Guilds.Wins),
            Loses = reader.ReadUInt32(GuildSchema.Guilds.Losts),
            CTFPoints = reader.ReadUInt32(GuildSchema.Guilds.CTFPoints),
            CTFReward = reader.ReadUInt32(GuildSchema.Guilds.CTFReward),
            ConquerPointFund = reader.ReadUInt32(GuildSchema.Guilds.ConquerPointFund),
            LevelRequirement = reader.ReadUInt32(GuildSchema.Guilds.LevelRequirement),
            RebornRequirement = reader.ReadUInt32(GuildSchema.Guilds.RebornRequirement),
            ClassRequirement = reader.ReadUInt32(GuildSchema.Guilds.ClassRequirement),
            Advertise = reader.ReadString(GuildSchema.Guilds.Advertise),
            GuildEnroll = reader.ReadString(GuildSchema.Guilds.GuildEnroll),
            BulletinEnroll = reader.ReadString(GuildSchema.Guilds.BulletinEnroll),
            CTFDonationCps = reader.ReadString(GuildSchema.Guilds.CTFDonationCps),
            CTFDonationSilver = reader.ReadString(GuildSchema.Guilds.CTFDonationSilver),
            CTFdonationSilverold = reader.ReadString(GuildSchema.Guilds.CTFdonationSilverold),
            CTFdonationCpsold = reader.ReadString(GuildSchema.Guilds.CTFdonationCpsold)
        };
    }

    /// <summary>
    ///     Maps a MySqlReader to a GuildRelationRecord from the `guild_relations` table
    /// </summary>
    public static GuildRelationRecord MapGuildRelation(MySqlReader reader) {
        return new GuildRelationRecord {
            GuildId = reader.ReadUInt32(GuildSchema.GuildRelations.GuildId),
            RelatedGuildId = reader.ReadUInt32(GuildSchema.GuildRelations.RelatedGuildId),
            RelationType = reader.ReadByte(GuildSchema.GuildRelations.RelationType)
        };
    }

    /// <summary>
    ///     Maps a MySqlReader to a GuildArsenalRecord from the `guildarsenal` table
    /// </summary>
    public static GuildArsenalRecord MapGuildArsenal(MySqlReader reader) {
        return new GuildArsenalRecord {
            Id = reader.ReadUInt32(GuildSchema.GuildArsenal.Id),
            Data = reader.ReadBlob(GuildSchema.GuildArsenal.Data)
        };
    }

    /// <summary>
    ///     Maps a MySqlReader to a GuildWarHistoryRecord from the `guild_war_history` table
    /// </summary>
    public static GuildWarHistoryRecord MapGuildWarHistory(MySqlReader reader, Func<string, DateTime> readDateTime,
        Func<string, List<uint>> deserializeDeputyIds) {
        return new GuildWarHistoryRecord {
            Id = reader.ReadUInt32(GuildSchema.GuildWarHistory.Id),
            GuildId = reader.ReadUInt32(GuildSchema.GuildWarHistory.GuildId),
            GuildLeaderEntityId = reader.ReadUInt32(GuildSchema.GuildWarHistory.GuildLeaderEntityId),
            GuildLeaderName = reader.ReadString(GuildSchema.GuildWarHistory.GuildLeaderName),
            GuildLeaderClaimed = reader.ReadBoolean(GuildSchema.GuildWarHistory.GuildLeaderClaimed),
            WarEndTime = readDateTime(reader.ReadString(GuildSchema.GuildWarHistory.WarEndTime)),
            DeputyClaimedIds = deserializeDeputyIds(reader.ReadString(GuildSchema.GuildWarHistory.DeputyClaimedIds))
        };
    }

    /// <summary>
    ///     Maps a MySqlReader to a GuildMemberRecord from the `guild_members` table
    /// </summary>
    public static GuildMemberRecord MapGuildMember(MySqlReader reader) {
        return new GuildMemberRecord {
            EntityId = reader.ReadUInt32(GuildSchema.GuildMembers.EntityId),
            GuildId = reader.ReadUInt32(GuildSchema.GuildMembers.GuildId),
            Rank = reader.ReadUInt16(GuildSchema.GuildMembers.Rank),
            SilverDonation = reader.ReadUInt64(GuildSchema.GuildMembers.SilverDonation),
            ConquerPointDonation = reader.ReadUInt64(GuildSchema.GuildMembers.ConquerPointDonation),
            ArsenalDonation = reader.ReadUInt32(GuildSchema.GuildMembers.ArsenalDonation),
            Lilies = reader.ReadUInt32(GuildSchema.GuildMembers.Lilies),
            Roses = reader.ReadUInt32(GuildSchema.GuildMembers.Roses),
            Orchids = reader.ReadUInt32(GuildSchema.GuildMembers.Orchids),
            Tulips = reader.ReadUInt32(GuildSchema.GuildMembers.Tulips),
            PkDonation = reader.ReadUInt32(GuildSchema.GuildMembers.PkDonation),
            LastLogin = reader.ReadUInt64(GuildSchema.GuildMembers.LastLogin),
            Exploits = reader.ReadUInt32(GuildSchema.GuildMembers.Exploits),
            GuideDonation = reader.ReadUInt32(GuildSchema.GuildMembers.GuideDonation),
            CtfCpsReward = reader.ReadUInt32(GuildSchema.GuildMembers.CtfCpsReward),
            CtfSilverReward = reader.ReadUInt32(GuildSchema.GuildMembers.CtfSilverReward)
        };
    }
}