using System.Collections.Generic;
using MTA.Database;
using MTA.Game.ConquerStructures;
using MTA.Game.Features.Guilds.Constants;
using MTA.Game.Features.Guilds.Database.Mappers;
using MTA.Game.Features.Guilds.Database.Schema;

namespace MTA.Game.Features.Guilds.Database;

public static class GuildMemberTable {
    /// <summary>
    ///     Loads all guild members from the database and returns them grouped by guild ID
    /// </summary>
    public static Dictionary<uint, SafeDictionary<uint, GuildMember>> LoadAll() {
        var dict = new Dictionary<uint, SafeDictionary<uint, GuildMember>>();

        // Use raw SQL to join guild_members with entities table
        const string sql = $"SELECT gm.*, e.name, e.level, e.Spouse, e.Class, e.Face, e.Body " +
                           $"FROM `{GuildSchema.Tables.GuildMembersTable}` gm " +
                           $"INNER JOIN `entities` e ON e.UID = gm.{GuildSchema.GuildMembers.EntityId}";

        // Create a MySqlCommand with custom SQL
        using var cmd = new MySqlCommand(MySqlCommandType.SELECT);
        cmd.Command = sql;
        using var reader = new MySqlReader(cmd);

        while (reader.Read()) {
            var record = GuildMappers.MapGuildMember(reader);

            var member = new GuildMember(record.GuildId) {
                Id = record.EntityId,
                Name = reader.ReadString("name"),
                Level = reader.ReadByte("level"),
                Spouse = reader.ReadString("Spouse"),
                Rank = (MemberRank)record.Rank,
                SilverDonation = record.SilverDonation,
                ConquerPointDonation = record.ConquerPointDonation,
                ArsenalDonation = record.ArsenalDonation,
                Class = reader.ReadByte("Class"),
                Lilies = record.Lilies,
                Roses = record.Roses,
                Orchids = record.Orchids,
                Tulips = record.Tulips,
                PkDonation = record.PkDonation,
                LastLogin = record.LastLogin,
                Exploits = record.Exploits,
                CtfCpsReward = record.CtfCpsReward,
                CtfSilverReward = record.CtfSilverReward
            };

            if (Nobility.Board.TryGetValue(member.Id, out var value)) {
                member.NobilityRank = value.Rank;
                member.Gender = value.Gender;
            }

            member.Mesh = uint.Parse(reader.ReadUInt16("Face") + reader.ReadUInt16("Body"));

            if (!dict.ContainsKey(member.GuildId))
                dict.Add(member.GuildId, new SafeDictionary<uint, GuildMember>());
            dict[member.GuildId].Add(member.Id, member);
        }

        return dict;
    }

    /// <summary>
    ///     Saves a guild member's data to the database
    /// </summary>
    public static void Save(GuildMember member) {
        using var cmd = new MySqlCommand(MySqlCommandType.UPDATE)
            .Update(GuildSchema.Tables.GuildMembersTable)
            .Set(GuildSchema.GuildMembers.GuildId, member.GuildId)
            .Set(GuildSchema.GuildMembers.Rank, (ushort)member.Rank)
            .Set(GuildSchema.GuildMembers.SilverDonation, member.SilverDonation)
            .Set(GuildSchema.GuildMembers.ConquerPointDonation, member.ConquerPointDonation)
            .Set(GuildSchema.GuildMembers.ArsenalDonation, member.ArsenalDonation)
            .Set(GuildSchema.GuildMembers.Lilies, member.Lilies)
            .Set(GuildSchema.GuildMembers.Roses, member.Roses)
            .Set(GuildSchema.GuildMembers.Orchids, member.Orchids)
            .Set(GuildSchema.GuildMembers.Tulips, member.Tulips)
            .Set(GuildSchema.GuildMembers.PkDonation, member.PkDonation)
            .Set(GuildSchema.GuildMembers.LastLogin, member.LastLogin)
            .Set(GuildSchema.GuildMembers.Exploits, member.Exploits)
            .Set(GuildSchema.GuildMembers.CtfCpsReward, member.CtfCpsReward)
            .Set(GuildSchema.GuildMembers.CtfSilverReward, member.CtfSilverReward)
            .Where(GuildSchema.GuildMembers.EntityId, member.Id);
        cmd.Execute();
    }

    /// <summary>
    ///     Inserts a new guild member into the database
    /// </summary>
    public static void Insert(GuildMember member) {
        using var cmd = new MySqlCommand(MySqlCommandType.INSERT)
            .Insert(GuildSchema.Tables.GuildMembersTable)
            .Insert(GuildSchema.GuildMembers.EntityId, member.Id)
            .Insert(GuildSchema.GuildMembers.GuildId, member.GuildId)
            .Insert(GuildSchema.GuildMembers.Rank, (ushort)member.Rank)
            .Insert(GuildSchema.GuildMembers.SilverDonation, member.SilverDonation)
            .Insert(GuildSchema.GuildMembers.ConquerPointDonation, member.ConquerPointDonation)
            .Insert(GuildSchema.GuildMembers.ArsenalDonation, member.ArsenalDonation)
            .Insert(GuildSchema.GuildMembers.Lilies, member.Lilies)
            .Insert(GuildSchema.GuildMembers.Roses, member.Roses)
            .Insert(GuildSchema.GuildMembers.Orchids, member.Orchids)
            .Insert(GuildSchema.GuildMembers.Tulips, member.Tulips)
            .Insert(GuildSchema.GuildMembers.PkDonation, member.PkDonation)
            .Insert(GuildSchema.GuildMembers.LastLogin, member.LastLogin)
            .Insert(GuildSchema.GuildMembers.Exploits, member.Exploits)
            .Insert(GuildSchema.GuildMembers.CtfCpsReward, member.CtfCpsReward)
            .Insert(GuildSchema.GuildMembers.CtfSilverReward, member.CtfSilverReward);
        cmd.Execute();
    }

    /// <summary>
    ///     Updates a guild member's guild ID and rank (used when joining/leaving guilds)
    /// </summary>
    public static void UpdateGuildAndRank(uint entityId, uint guildId, ushort rank) {
        using var cmd = new MySqlCommand(MySqlCommandType.UPDATE)
            .Update(GuildSchema.Tables.GuildMembersTable)
            .Set(GuildSchema.GuildMembers.GuildId, guildId)
            .Set(GuildSchema.GuildMembers.Rank, rank)
            .Where(GuildSchema.GuildMembers.EntityId, entityId);
        cmd.Execute();
    }

    /// <summary>
    ///     Deletes a guild member from the database (used when leaving a guild)
    /// </summary>
    public static void Delete(uint entityId) {
        using var cmd = new MySqlCommand(MySqlCommandType.DELETE)
            .Delete(GuildSchema.Tables.GuildMembersTable, GuildSchema.GuildMembers.EntityId, entityId);
        cmd.Execute();
    }

    /// <summary>
    ///     Updates the last login timestamp for a guild member
    /// </summary>
    public static void UpdateLastLogin(uint entityId, ulong lastLogin) {
        using var cmd = new MySqlCommand(MySqlCommandType.UPDATE)
            .Update(GuildSchema.Tables.GuildMembersTable)
            .Set(GuildSchema.GuildMembers.LastLogin, lastLogin)
            .Where(GuildSchema.GuildMembers.EntityId, entityId);
        cmd.Execute();
    }
}