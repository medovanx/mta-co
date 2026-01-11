using System;
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
    private static bool Exists(uint id) {
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
        try {
            using var cmd = new MySqlCommand(MySqlCommandType.SELECT)
                .Select(KissSchema.Tables.KissesTable)
                .Where(KissSchema.Kisses.EntityId, id);
            using var reader = new MySqlReader(cmd);
            if (reader.Read()) return KissMappers.MapKiss(reader);
        }
        catch {
            // Return null if not found or error
        }

        return null;
    }

    /// <summary>
    ///     Inserts a new kiss record
    /// </summary>
    public static void Insert(GameState client) {
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
    /// </summary>
    public static void Save(GameState client) {
        if (!Exists(client.Entity.UID))
            Insert(client);
        else
            Update(client);
    }
}