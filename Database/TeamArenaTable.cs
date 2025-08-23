using MTA.Network.GamePackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Game.ConquerStructures;
using MTA.Game;

namespace MTA.Database
{
    public class TeamArenaTable
    {
        public static bool NewMonth = false;
        public static void Load()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("teamarena"))
            using (var reader = cmd.CreateReader())
            {
                while (reader.Read())
                {
                    Network.GamePackets.TeamArenaStatistic stat = new Network.GamePackets.TeamArenaStatistic(true);
                    stat.EntityID = reader.ReadUInt32("EntityID");
                    stat.Name = reader.ReadString("EntityName");
                    stat.LastSeasonRank = reader.ReadUInt32("LastSeasonRank");
                    stat.LastSeasonArenaPoints = reader.ReadUInt32("LastSeasonArenaPoints");
                    stat.ArenaPoints = reader.ReadUInt32("ArenaPoints");
                    stat.TodayWin = reader.ReadByte("TodayWin");
                    stat.TodayBattles = reader.ReadByte("TodayBattles");
                    stat.LastSeasonWin = reader.ReadUInt32("LastSeasonWin");
                    stat.LastSeasonLose = reader.ReadUInt32("LastSeasonLose");
                    stat.TotalWin = reader.ReadUInt32("TotalWin");
                    stat.TotalLose = reader.ReadUInt32("TotalLose");
                    stat.HistoryHonor = reader.ReadUInt32("HistoryHonor");
                    stat.CurrentHonor = reader.ReadUInt32("CurrentHonor");
                    stat.Level = reader.ReadByte("Level");
                    stat.Class = reader.ReadByte("Class");
                    stat.Model = reader.ReadUInt32("Model");
                    TeamArena.ArenaStatistics.Add(stat.EntityID, stat);
                }
            }
            TeamArena.Sort();
            TeamArena.YesterdaySort();
            Console.WriteLine("Team Arena information loaded.");
        }

        public static uint ArenaPointFill(byte level)
        {
            if (level >= 70 && level < 100) return 1000;
            else if (level >= 100 && level < 110) return 2000;
            else if (level >= 110 && level < 120) return 3000;
            else if (level >= 120) return 4000;
            return 0;
        }

        public static void SaveArenaStatistics(Network.GamePackets.TeamArenaStatistic stats)
        {
            if (stats == null) return;
            using (var conn = DataHolder.MySqlConnection)
            {
                conn.Open();
                SaveArenaStatistics(stats, conn);
            }
        }
        public static void SaveArenaStatistics(Network.GamePackets.TeamArenaStatistic stats, MySql.Data.MySqlClient.MySqlConnection conn)
        {
            if (stats == null) return;
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("teamarena"))
                cmd.Set("LastSeasonRank", stats.LastSeasonRank)
                .Set("ArenaPoints", stats.ArenaPoints)
                .Set("TodayWin", stats.TodayWin)
                .Set("TodayBattles", stats.TodayBattles)
                .Set("LastSeasonWin", stats.LastSeasonWin)
                .Set("LastSeasonLose", stats.LastSeasonLose)
                .Set("TotalWin", stats.TotalWin)
                .Set("TotalLose", stats.TotalLose)
                .Set("HistoryHonor", stats.HistoryHonor)
                .Set("CurrentHonor", stats.CurrentHonor)
                .Set("Level", stats.Level)
                .Set("Class", stats.Class)
                .Set("EntityName", stats.Name)
                .Set("Model", stats.Model)
                .Set("LastSeasonArenaPoints", stats.LastSeasonArenaPoints)
                .Where("EntityID", stats.EntityID)
                 .Execute();
        }
        public static void InsertArenaStatistic(Client.GameState client)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("teamarena"))
                cmd
                .Insert("ArenaPoints", client.ArenaPoints)
                .Insert("Level", client.TeamArenaStatistic.Level)
                .Insert("EntityName", client.TeamArenaStatistic.Name)
                .Insert("Class", client.TeamArenaStatistic.Class)
                .Insert("Model", client.TeamArenaStatistic.Model)
                .Insert("EntityID", client.TeamArenaStatistic.EntityID).Execute();
        }

        public static void Reset(TeamArenaStatistic stat)
        {
            stat.LastSeasonArenaPoints = stat.ArenaPoints;
            stat.LastSeasonWin = stat.TodayWin;
            stat.LastSeasonLose = stat.TodayBattles - stat.TodayWin;
            stat.LastSeasonRank = stat.Rank;
            stat.TodayWin = 0;
            stat.TodayBattles = 0;
            stat.Rank = 0;
        }
    }
}
