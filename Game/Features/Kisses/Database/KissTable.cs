using System;
using System.Collections.Generic;
using MTA.Client;
using MTA.Database;
using MTA.Game.Features.Kisses.Database.Mappers;
using MTA.Game.Features.Kisses.Database.Models;
using MTA.Game.Features.Kisses.Database.Schema;

namespace MTA.Game.Features.Kisses.Database;

/// <summary>
///     Database operations for the kisses table
/// </summary>
public static class KissTable {
    /// <summary>
    ///     Checks if a kiss record exists for the given entity ID
    /// </summary>
    public static bool Exists(uint id) {
        try {
            using var cmd = new MySqlCommand(MySqlCommandType.SELECT)
                .Select(KissSchema.Tables.KissesTable)
                .Where(KissSchema.Kisses.EntityId, id);
            using var reader = new MySqlReader(cmd);
            return reader.Read();
        }
        catch {
            return false;
        }
    }

    /// <summary>
    ///     Loads a kiss record by entity ID
    /// </summary>
    public static KissRecord? LoadByEntityId(uint id) {
        using var cmd = new MySqlCommand(MySqlCommandType.SELECT)
            .Select(KissSchema.Tables.KissesTable)
            .Where(KissSchema.Kisses.EntityId, id);
        using var reader = new MySqlReader(cmd);
        if (reader.Read()) return KissMappers.MapKiss(reader);

        return null;
    }

    /// <summary>
    ///     Inserts a new kiss record
    /// </summary>
    public static void Insert(GameState client) {
        // Only boys should be in the kisses table (Body IDs: 1001, 1002, 1003, 1004)
        if (client.Entity.Body is not (1001 or 1002 or 1003 or 1004)) {
            Console.WriteLine(
                $"Warning: Attempted to insert girl (Body: {client.Entity.Body}, UID: {client.Entity.UID}) into kisses table");
            return;
        }

        using var cmd = new MySqlCommand(MySqlCommandType.INSERT)
            .Insert(KissSchema.Tables.KissesTable)
            .Insert(KissSchema.Kisses.EntityId, client.Entity.UID)
            .Insert(KissSchema.Kisses.KissesCount, client.Entity.Kisses.Kisses2)
            .Insert(KissSchema.Kisses.KissesToday, client.Entity.Kisses.Kisses2day)
            .Insert(KissSchema.Kisses.Letters, client.Entity.Kisses.Letters1)
            .Insert(KissSchema.Kisses.LettersToday, client.Entity.Kisses.LetterToday1)
            .Insert(KissSchema.Kisses.Wine, client.Entity.Kisses.Wine)
            .Insert(KissSchema.Kisses.WineToday, client.Entity.Kisses.Wine2day)
            .Insert(KissSchema.Kisses.Jades, client.Entity.Kisses.Jades)
            .Insert(KissSchema.Kisses.JadesToday, client.Entity.Kisses.Jades2day)
            .Insert(KissSchema.Kisses.LastKissesSent,
                DateTime.Now.Subtract(TimeSpan.FromDays(1)).ToBinary());
        cmd.Execute();
    }

    /// <summary>
    ///     Updates an existing kiss record
    /// </summary>
    public static void Update(GameState client) {
        using var cmd = new MySqlCommand(MySqlCommandType.UPDATE)
            .Update(KissSchema.Tables.KissesTable)
            .Set(KissSchema.Kisses.KissesCount, client.Entity.Kisses.Kisses2)
            .Set(KissSchema.Kisses.KissesToday, client.Entity.Kisses.Kisses2day)
            .Set(KissSchema.Kisses.Letters, client.Entity.Kisses.Letters1)
            .Set(KissSchema.Kisses.LettersToday, client.Entity.Kisses.LetterToday1)
            .Set(KissSchema.Kisses.Wine, client.Entity.Kisses.Wine)
            .Set(KissSchema.Kisses.WineToday, client.Entity.Kisses.Wine2day)
            .Set(KissSchema.Kisses.Jades, client.Entity.Kisses.Jades)
            .Set(KissSchema.Kisses.JadesToday, client.Entity.Kisses.Jades2day)
            .Set(KissSchema.Kisses.LastKissesSent, client.Entity.Kisses.LastKissesSent.ToBinary())
            .Where(KissSchema.Kisses.EntityId, client.Entity.UID);
        cmd.Execute();
    }

    /// <summary>
    ///     Saves a kiss record (inserts if not exists, updates if exists)
    ///     Only saves for boys - girls should not be in the kisses table
    /// </summary>
    public static void Save(GameState client) {
        // Only save boys to the kisses table
        if (client.Entity.Body is not (1001 or 1002 or 1003 or 1004)) return;

        if (!Exists(client.Entity.UID))
            Insert(client);
        else
            Update(client);
    }

    /// <summary>
    ///     Deletes a kiss record by entity ID
    /// </summary>
    public static void DeleteByEntityId(uint entityId) {
        using var cmd = new MySqlCommand(MySqlCommandType.DELETE)
            .Delete(KissSchema.Tables.KissesTable, KissSchema.Kisses.EntityId, entityId);
        cmd.Execute();
    }

    /// <summary>
    ///     Top 100 kiss rows for one leaderboard column, with player name from <c>entities</c>.
    /// </summary>
    public static List<(KissRecord Record, string Name)> LoadTop100WithNames(string whereNonZeroColumn,
        string orderByColumnDesc) {
        var list = new List<(KissRecord, string)>();
        var t = KissSchema.Tables.KissesTable;
        var sql =
            $"SELECT k.{KissSchema.Kisses.EntityId} AS entity_id, k.{KissSchema.Kisses.KissesCount} AS kisses, k.{KissSchema.Kisses.KissesToday} AS kisses_today, " +
            $"k.{KissSchema.Kisses.Letters} AS letters, k.{KissSchema.Kisses.LettersToday} AS letters_today, k.{KissSchema.Kisses.Wine} AS wine, " +
            $"k.{KissSchema.Kisses.WineToday} AS wine_today, k.{KissSchema.Kisses.Jades} AS jades, k.{KissSchema.Kisses.JadesToday} AS jades_today, " +
            $"k.{KissSchema.Kisses.LastKissesSent} AS last_kiss_sent, e.Name AS Name " +
            $"FROM `{t}` k INNER JOIN `entities` e ON k.{KissSchema.Kisses.EntityId} = e.UID WHERE {whereNonZeroColumn} " +
            $"ORDER BY {orderByColumnDesc} DESC LIMIT 100";
        try {
            using var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select(t);
            cmd.Command = cmd.Command.Replace($"SELECT * FROM `{t}`", sql);
            using var reader = new MySqlReader(cmd);
            while (reader.Read()) {
                var record = KissMappers.MapKiss(reader);
                var name = reader.ReadString("Name");
                list.Add((record, name));
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"KissTable.LoadTop100WithNames: {ex.Message}");
        }

        return list;
    }
}