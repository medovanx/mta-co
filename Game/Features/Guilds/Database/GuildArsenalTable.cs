using System.IO;
using MTA.Database;
using MTA.Game.Features.Guilds.Database.Mappers;
using MTA.Game.Features.Guilds.Database.Schema;
using MTA.Game.Features.Guilds.Models;

namespace MTA.Game.Features.Guilds.Database;

/// <summary>
///     Database operations for guild arsenal data, handling loading and saving of inscribed items and arsenal states.
/// </summary>
public class GuildArsenalTable {
    /// <summary>
    ///     Loads arsenal data for a guild from database, deserializing binary data into arsenal slots and items.
    /// </summary>
    public static void Load(Guild guild) {
        using var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select(GuildSchema.Tables.GuildArsenalTable)
            .Where(GuildSchema.GuildArsenal.Id, guild.Id);
        using var rdr = new MySqlReader(cmd);
        if (rdr.Read()) {
            var record = GuildMappers.MapGuildArsenal(rdr);
            var stream = new MemoryStream(record.Data);
            var reader = new BinaryReader(stream);
            for (var i = 0; i < 8; i++) {
                guild.Arsenals[i].Load(reader);
            }

            guild.ArsenalBpChanged = true;
            guild.GetMaxSharedBattlePower();
        }
        else {
            Insert(guild.Id);
        }
    }

    /// <summary>
    ///     Saves arsenal data to database, serializing all arsenal slots and inscribed items to binary format.
    /// </summary>
    public static void Save(Guild guild) {
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);
        for (var i = 0; i < 8; i++) {
            guild.Arsenals[i].Save(writer);
        }

        var sql =
            $"UPDATE `{GuildSchema.Tables.GuildArsenalTable}` SET {GuildSchema.GuildArsenal.Data}=@Data, {GuildSchema.GuildArsenal.DataLength}=@DataLength where {GuildSchema.GuildArsenal.Id} = {guild.Id} ;";
        var rawData = stream.ToArray();
        using var conn = DataHolder.MySqlConnection;
        conn.Open();
        using var cmd = new MySql.Data.MySqlClient.MySqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@Data", rawData);
        cmd.Parameters.AddWithValue("@DataLength", rawData.Length);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    ///     Saves all guild arsenals to database, typically called during server shutdown or periodic saves.
    /// </summary>
    public static void SaveAll() {
        foreach (var guild in Kernel.Guilds.Values) {
            Save(guild);
        }
    }

    /// <summary>
    ///     Creates new arsenal record for a guild, initializing empty arsenal slots.
    /// </summary>
    public static void Insert(uint id) {
        using var cmd = new MySqlCommand(MySqlCommandType.INSERT);
        cmd.Insert(GuildSchema.Tables.GuildArsenalTable).Insert(GuildSchema.GuildArsenal.Id, id);
        cmd.Execute();
    }
}