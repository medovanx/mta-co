using System;
using System.Collections.Generic;

namespace MTA.Database {
    public class GuildWarHistory {
        public uint Id { get; init; }
        public uint GuildId { get; init; }
        public uint GuildLeaderEntityId { get; init; }
        public required string GuildLeaderName { get; init; }
        public bool GuildLeaderClaimed { get; init; }
        public List<uint> DeputyClaimedIds { get; init; } = [];
        public DateTime WarEndTime { get; init; }
    }

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
            if (!json.StartsWith($"[") || !json.EndsWith($"]")) return result;
            json = json.Substring(1, json.Length - 2);
            if (string.IsNullOrEmpty(json)) return result;
            var parts = json.Split(',');
            foreach (var part in parts) {
                if (uint.TryParse(part.Trim(), out var id))
                    result.Add(id);
            }

            return result;
        }

        private static DateTime ReadDateTime(MySqlReader reader, string columnName) {
            var dateStr = reader.ReadString(columnName);
            if (string.IsNullOrEmpty(dateStr))
                return DateTime.MinValue;
            return DateTime.TryParse(dateStr, out var result) ? result : DateTime.MinValue;
        }

        public static void Create(Game.ConquerStructures.Society.Guild winnerGuild, uint leaderEntityId,
            string leaderName, DateTime warEndTime) {
            using var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("guild_war_history")
                .Insert("guild_id", winnerGuild.ID)
                .Insert("guild_leader_entity_id", leaderEntityId)
                .Insert("guild_leader_name", leaderName)
                .Insert("guild_leader_claimed", 0)
                .Insert("deputy_claimed_ids", "[]")
                .Insert("war_end_time", warEndTime.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Execute();
        }

        public static GuildWarHistory? GetLatest() {
            var cmd = new MySqlCommand(MySqlCommandType.SELECT)
                .Select("guild_war_history")
                .Order("war_end_time DESC");
            cmd.Command = cmd.Command + " LIMIT 1";
            using (cmd)
            using (var reader = new MySqlReader(cmd)) {
                if (!reader.Read()) {
                    return null; // No rows found
                }

                var history = new GuildWarHistory {
                    Id = reader.ReadUInt32("id"),
                    GuildId = reader.ReadUInt32("guild_id"),
                    GuildLeaderEntityId = reader.ReadUInt32("guild_leader_entity_id"),
                    GuildLeaderName = reader.ReadString("guild_leader_name"),
                    GuildLeaderClaimed = reader.ReadBoolean("guild_leader_claimed"),
                    WarEndTime = ReadDateTime(reader, "war_end_time"),
                    DeputyClaimedIds = DeserializeDeputyIds(reader.ReadString("deputy_claimed_ids"))
                };
                return history;
            }
        }

        public static void SetGuildLeaderClaimed(uint historyId) {
            using var cmd = new MySqlCommand(MySqlCommandType.UPDATE)
                .Update("guild_war_history")
                .Set("guild_leader_claimed", 1)
                .Where("id", historyId);
            cmd.Execute();
        }

        public static void AddDeputyClaim(uint historyId, uint deputyEntityId) {
            var history = GetById(historyId);
            if (history == null) return; // History not found
            if (history.DeputyClaimedIds.Contains(deputyEntityId) || history.DeputyClaimedIds.Count >= 5) return;
            history.DeputyClaimedIds.Add(deputyEntityId);
            var json = SerializeDeputyIds(history.DeputyClaimedIds);
            using var cmd = new MySqlCommand(MySqlCommandType.UPDATE)
                .Update("guild_war_history")
                .Set("deputy_claimed_ids", json)
                .Where("id", historyId);
            cmd.Execute();
        }

        private static GuildWarHistory? GetById(uint historyId) {
            using var cmd = new MySqlCommand(MySqlCommandType.SELECT)
                .Select("guild_war_history")
                .Where("id", historyId);
            using var reader = new MySqlReader(cmd);
            if (!reader.Read()) {
                return null; // No rows found
            }
            var history = new GuildWarHistory {
                Id = reader.ReadUInt32("id"),
                GuildId = reader.ReadUInt32("guild_id"),
                GuildLeaderEntityId = reader.ReadUInt32("guild_leader_entity_id"),
                GuildLeaderName = reader.ReadString("guild_leader_name"),
                GuildLeaderClaimed = reader.ReadBoolean("guild_leader_claimed"),
                WarEndTime = ReadDateTime(reader, "war_end_time"),
                DeputyClaimedIds = DeserializeDeputyIds(reader.ReadString("deputy_claimed_ids"))
            };
            return history;
        }

        public static List<uint> GetDeputyClaimedIds(uint historyId) {
            var history = GetById(historyId);
            return history?.DeputyClaimedIds ?? new List<uint>();
        }

        public static bool HasDeputyClaimed(uint historyId, uint deputyEntityId) {
            var history = GetById(historyId);
            return history?.DeputyClaimedIds.Contains(deputyEntityId) ?? false;
        }

        public static bool CanDeputyClaim(uint historyId) {
            var history = GetById(historyId);
            return history != null && history.DeputyClaimedIds.Count < 5;
        }

        public static List<GuildWarHistory> GetLastNWins(int count) {
            var results = new List<GuildWarHistory>();
            var cmd = new MySqlCommand(MySqlCommandType.SELECT)
                .Select("guild_war_history")
                .Order("war_end_time DESC");
            cmd.Command = cmd.Command + " LIMIT " + count;
            using (cmd)
            using (var reader = new MySqlReader(cmd)) {
                while (reader.Read()) {
                    var history = new GuildWarHistory {
                        Id = reader.ReadUInt32("id"),
                        GuildId = reader.ReadUInt32("guild_id"),
                        GuildLeaderEntityId = reader.ReadUInt32("guild_leader_entity_id"),
                        GuildLeaderName = reader.ReadString("guild_leader_name"),
                        GuildLeaderClaimed = reader.ReadBoolean("guild_leader_claimed"),
                        WarEndTime = ReadDateTime(reader, "war_end_time"),
                        DeputyClaimedIds = DeserializeDeputyIds(reader.ReadString("deputy_claimed_ids"))
                    };
                    results.Add(history);
                }
            }

            return results;
        }
    }
}