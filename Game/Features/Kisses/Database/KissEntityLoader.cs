using System;
using MTA.Client;
using MTA.Database;
using MTA.Game.Constants;
using MTA.Game.Features.Kisses.Database.Models;

namespace MTA.Game.Features.Kisses.Database;

/// <summary>
///     Loads (or initializes) the per-player <see cref="Kisses"/> object on login.
///     Only boys hydrate from the database; for non-boys the field is set to null.
/// </summary>
public static class KissEntityLoader {
    public static void Load(GameState client) {
        if (!BodyTypes.IsBoy(client.Entity.Body)) {
            client.Entity.Kisses = null;
            return;
        }

        client.Entity.Kisses = new Kisses {
            Uid = client.Entity.UID,
            name = client.Entity.Name
        };

        // Load from database — try multiple times if needed
        KissRecord? record = null;
        for (var attempt = 0; attempt < 3 && record == null; attempt++) {
            record = KissTable.LoadByEntityId(client.Entity.UID);
        }

        if (record != null) {
            ApplyRecord(client, record);
            return;
        }

        // Only create new record if it truly doesn't exist
        if (!KissTable.Exists(client.Entity.UID)) {
            KissTable.Insert(client);
            return;
        }

        // Record exists, but LoadByEntityId failed — try direct query as fallback
        using var cmd = new MySqlCommand(MySqlCommandType.SELECT)
            .Select("kisses")
            .Where("entity_id", client.Entity.UID);
        using var reader = new MySqlReader(cmd);
        if (!reader.Read()) return;
        client.Entity.Kisses.Count = reader.ReadUInt32("kisses");
        client.Entity.Kisses.TodayCount = reader.ReadUInt32("kisses_today");
        client.Entity.Kisses.Letters = reader.ReadUInt32("letters");
        client.Entity.Kisses.LetterToday = reader.ReadUInt32("letters_today");
        client.Entity.Kisses.Wine = reader.ReadUInt32("wine");
        client.Entity.Kisses.WineToday = reader.ReadUInt32("wine_today");
        client.Entity.Kisses.Jades = reader.ReadUInt32("jades");
        client.Entity.Kisses.JadesToday = reader.ReadUInt32("jades_today");
        try {
            client.Entity.Kisses.LastKissesSent = DateTime.FromBinary(reader.ReadInt64("last_kiss_sent"));
        } catch {
            client.Entity.Kisses.LastKissesSent = DateTime.Now.Subtract(TimeSpan.FromDays(1));
        }

        ResetDailyAndRebuildRanks(client);
    }

    private static void ApplyRecord(GameState client, KissRecord record) {
        client.Entity.Kisses?.Count = record.Kisses;
        client.Entity.Kisses?.TodayCount = record.KissesToday;
        client.Entity.Kisses?.Letters = record.Letters;
        client.Entity.Kisses?.LetterToday = record.LettersToday;
        client.Entity.Kisses?.Wine = record.Wine;
        client.Entity.Kisses?.WineToday = record.WineToday;
        client.Entity.Kisses?.Jades = record.Jades;
        client.Entity.Kisses?.JadesToday = record.JadesToday;
        try {
            client.Entity.Kisses?.LastKissesSent = DateTime.FromBinary(record.LastKissesSent);
        } catch {
            client.Entity.Kisses?.LastKissesSent = DateTime.Now.Subtract(TimeSpan.FromDays(1));
        }

        ResetDailyAndRebuildRanks(client);
    }

    private static void ResetDailyAndRebuildRanks(GameState client) {
        if (client.Entity.Kisses?.LastKissesSent.AddDays(1) <= DateTime.Now) {
            client.Entity.Kisses.LastKissesSent = DateTime.Now;
            client.Entity.Kisses.TodayCount = 0;
            client.Entity.Kisses.LetterToday = 0;
            client.Entity.Kisses.JadesToday = 0;
            client.Entity.Kisses.WineToday = 0;
        }

        if (client.Entity.Kisses?.Count > 0)
            Kisses.CalculateRankKisses(client.Entity.Kisses);
        if (client.Entity.Kisses?.Letters > 0)
            Kisses.CalculateRankLetters(client.Entity.Kisses);
        if (client.Entity.Kisses?.Wine > 0)
            Kisses.CalculateRankWine(client.Entity.Kisses);
        if (client.Entity.Kisses?.Jades > 0)
            Kisses.CalculateRankJades(client.Entity.Kisses);
    }
}
