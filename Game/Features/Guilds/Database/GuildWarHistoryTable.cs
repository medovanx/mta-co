using System;
using System.Collections.Generic;
using MTA.Database;
using MTA.Game.Features.Guilds.Database.Mappers;
using MTA.Game.Features.Guilds.Database.Models;
using MTA.Game.Features.Guilds.Database.Schema;

namespace MTA.Game.Features.Guilds.Database;

public static class GuildWarHistoryTable {
    private static string SerializeDeputyIds(List<uint>? ids) {
        if (ids == null || ids.Count == 0)
            return "[]";

        return "[" + string.Join(",", ids) + "]";
    }

    private static List<uint> DeserializeDeputyIds(string json) {
        var result = new List<uint>();
        if (string.IsNullOrEmpty(json) || json == "[]" || json == "null")
            return result;

        // Simple JSON array parsing: [123,456,789]
        json = json.Trim();
        if (!json.StartsWith("[") || !json.EndsWith("]")) return result;
        json = json.Substring(1, json.Length - 2);
        if (string.IsNullOrEmpty(json)) return result;
        var parts = json.Split(',');
        foreach (var part in parts)
            if (uint.TryParse(part.Trim(), out var id))
                result.Add(id);

        return result;
    }

    public static void Create(Guild winnerGuild, uint leaderEntityId,
        string leaderName, DateTime warEndTime) {
        using var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert(GuildSchema.Tables.GuildWarHistoryTable)
            .Insert(GuildSchema.GuildWarHistory.GuildId, winnerGuild.Id)
            .Insert(GuildSchema.GuildWarHistory.GuildLeaderEntityId, leaderEntityId)
            .Insert(GuildSchema.GuildWarHistory.GuildLeaderName, leaderName)
            .Insert(GuildSchema.GuildWarHistory.GuildLeaderClaimed, 0)
            .Insert(GuildSchema.GuildWarHistory.DeputyClaimedIds, "[]")
            .Insert(GuildSchema.GuildWarHistory.WarEndTime, warEndTime.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Execute();
    }

    public static GuildWarHistoryRecord? GetLatest() {
        var cmd = new MySqlCommand(MySqlCommandType.SELECT)
            .Select(GuildSchema.Tables.GuildWarHistoryTable)
            .Order($"{GuildSchema.GuildWarHistory.WarEndTime} DESC");
        cmd.Command = cmd.Command + " LIMIT 1";
        using (cmd)
        using (var reader = new MySqlReader(cmd)) {
            return !reader.Read()
                ? null
                : // No rows found
                GuildMappers.MapGuildWarHistory(reader, _ => DateTime.MinValue, DeserializeDeputyIds);
        }
    }

    public static void SetGuildLeaderClaimed(uint historyId) {
        using var cmd = new MySqlCommand(MySqlCommandType.UPDATE)
            .Update(GuildSchema.Tables.GuildWarHistoryTable)
            .Set(GuildSchema.GuildWarHistory.GuildLeaderClaimed, 1)
            .Where(GuildSchema.GuildWarHistory.Id, historyId);
        cmd.Execute();
    }

    public static void AddDeputyClaim(uint historyId, uint deputyEntityId) {
        var history = GetById(historyId);
        if (history == null) return; // History not found
        if (history.DeputyClaimedIds.Contains(deputyEntityId) || history.DeputyClaimedIds.Count >= 5) return;
        history.DeputyClaimedIds.Add(deputyEntityId);
        var json = SerializeDeputyIds(history.DeputyClaimedIds);
        using var cmd = new MySqlCommand(MySqlCommandType.UPDATE)
            .Update(GuildSchema.Tables.GuildWarHistoryTable)
            .Set(GuildSchema.GuildWarHistory.DeputyClaimedIds, json)
            .Where(GuildSchema.GuildWarHistory.Id, historyId);
        cmd.Execute();
    }

    private static GuildWarHistoryRecord? GetById(uint historyId) {
        using var cmd = new MySqlCommand(MySqlCommandType.SELECT)
            .Select(GuildSchema.Tables.GuildWarHistoryTable)
            .Where(GuildSchema.GuildWarHistory.Id, historyId);
        using var reader = new MySqlReader(cmd);
        if (!reader.Read()) return null; // No rows found
        return GuildMappers.MapGuildWarHistory(reader, _ => DateTime.MinValue, DeserializeDeputyIds);
    }

    public static List<uint> GetDeputyClaimedIds(uint historyId) {
        var history = GetById(historyId);
        return history?.DeputyClaimedIds ?? [];
    }

    public static bool HasDeputyClaimed(uint historyId, uint deputyEntityId) {
        var history = GetById(historyId);
        return history?.DeputyClaimedIds.Contains(deputyEntityId) ?? false;
    }

    public static bool CanDeputyClaim(uint historyId) {
        var history = GetById(historyId);
        return history is { DeputyClaimedIds.Count: < 5 };
    }

    public static List<GuildWarHistoryRecord> GetLastNWins(int count) {
        var results = new List<GuildWarHistoryRecord>();
        var cmd = new MySqlCommand(MySqlCommandType.SELECT)
            .Select(GuildSchema.Tables.GuildWarHistoryTable)
            .Order($"{GuildSchema.GuildWarHistory.WarEndTime} DESC");
        cmd.Command = cmd.Command + " LIMIT " + count;
        using (cmd)
        using (var reader = new MySqlReader(cmd)) {
            while (reader.Read()) {
                var history = GuildMappers.MapGuildWarHistory(reader, _ => DateTime.MinValue, DeserializeDeputyIds);
                results.Add(history);
            }
        }

        return results;
    }
}