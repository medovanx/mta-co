using System;
using MTA.Network.GamePackets;

namespace MTA.Database
{
    public class ArenaTable
    {
        public static void Load()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("arena"))
            {
                cmd.Command = cmd.Command.Replace("SELECT * FROM `arena`",
                    "SELECT a.EntityID, a.LastSeasonRank, a.ArenaPoints, a.TodayWin, a.TodayBattles, a.LastSeasonWin, a.LastSeasonLose, a.TotalWin, a.TotalLose, a.HistoryHonor, a.CurrentHonor, a.Level, a.Class, a.Model, a.ArenaPointFill, a.LastSeasonArenaPoints, e.Name FROM `arena` a INNER JOIN `entities` e ON a.EntityID = e.UID");
                using (var reader = new MySqlReader(cmd))
                {
                    while (reader.Read())
                    {
                        ArenaStatistic stat = new ArenaStatistic(true);
                        stat.EntityID = reader.ReadUInt32("EntityID");
                        stat.Name = reader.ReadString("Name");
                    stat.LastSeasonRank = reader.ReadUInt32("LastSeasonRank");
                    stat.LastSeasonArenaPoints = reader.ReadUInt32("LastSeasonArenaPoints");
                    stat.ArenaPoints = reader.ReadUInt32("ArenaPoints");
                    stat.TodayWin = reader.ReadUInt32("TodayWin");
                    stat.TodayBattles = reader.ReadUInt32("TodayBattles");
                    stat.LastSeasonWin = reader.ReadUInt32("LastSeasonWin");
                    stat.LastSeasonLose = reader.ReadUInt32("LastSeasonLose");
                    stat.TotalWin = reader.ReadUInt32("TotalWin");
                    stat.TotalLose = reader.ReadUInt32("TotalLose");
                    stat.HistoryHonor = reader.ReadUInt32("HistoryHonor");
                    stat.CurrentHonor = reader.ReadUInt32("CurrentHonor");
                    stat.Level = reader.ReadByte("Level");
                    stat.Class = reader.ReadByte("Class");
                    stat.Model = reader.ReadUInt32("Model");
                    stat.LastArenaPointFill = DateTime.FromBinary(reader.ReadInt64("ArenaPointFill"));

                    if (DateTime.Now.DayOfYear != stat.LastArenaPointFill.DayOfYear)
                    {
                        stat.LastSeasonArenaPoints = stat.ArenaPoints;
                        stat.LastSeasonWin = stat.TodayWin;
                        stat.LastSeasonLose = stat.TodayBattles - stat.TodayWin;
                        stat.ArenaPoints = ArenaPointFill(stat.Level);
                        stat.LastArenaPointFill = DateTime.Now;
                        stat.TodayWin = 0;
                        stat.TodayBattles = 0;
                    }

                        Game.Arena.ArenaStatistics.Add(stat.EntityID, stat);
                    }
                }
            }

            Game.Arena.Sort();
            Game.Arena.YesterdaySort();
            Console.WriteLine("Arena information loaded.");
        }

        public static uint ArenaPointFill(byte level)
        {
            if (level is >= 70 and < 100)
                return 1000;
            else if (level is >= 100 and < 110)
                return 2000;
            else if (level is >= 110 and < 120)
                return 3000;
            else if (level >= 120)
                return 4000;
            return 0;
        }

        public static void SaveArenaStatistics(ArenaStatistic stats, MySql.Data.MySqlClient.MySqlConnection conn)
        {
            if (stats == null) return;
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("arena"))
                cmd.Set("LastSeasonRank", stats.LastSeasonRank)
                .Set("ArenaPoints", stats.ArenaPoints).Set("TodayWin", stats.TodayWin)
                .Set("TodayBattles", stats.TodayBattles).Set("LastSeasonWin", stats.LastSeasonWin)
                .Set("LastSeasonLose", stats.LastSeasonLose).Set("TotalWin", stats.TotalWin)
                .Set("TotalLose", stats.TotalLose).Set("HistoryHonor", stats.HistoryHonor)
                .Set("CurrentHonor", stats.CurrentHonor).Set("Level", stats.Level).Set("Class", stats.Class)
                .Set("ArenaPointFill", stats.LastArenaPointFill.Ticks).Set("Model", stats.Model)
                .Set("Class", stats.Class).Set("LastSeasonArenaPoints", stats.LastSeasonArenaPoints).Where("EntityID", stats.EntityID)
                .Execute();
        }
        public static void SaveArenaStatistics(ArenaStatistic stats)
        {
            using (var conn = DataHolder.MySqlConnection)
            {
                conn.Open();
                SaveArenaStatistics(stats, conn);
            }
        }
        public static void InsertArenaStatistic(Client.GameState client)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("arena")
              .Insert("ArenaPoints", client.ArenaStatistic.ArenaPoints)
              .Insert("Level", client.ArenaStatistic.Level).Insert("Class", client.ArenaStatistic.Class).Insert("Model", client.ArenaStatistic.Model)
              .Insert("ArenaPointFill", client.ArenaStatistic.LastArenaPointFill.Ticks).Insert("EntityID", client.ArenaStatistic.EntityID))
                cmd.Execute();
        }

        public static void Reset(Client.GameState client, ArenaStatistic stat)
        {
            stat.LastSeasonArenaPoints = stat.ArenaPoints;
            stat.LastSeasonWin = stat.TodayWin;
            stat.LastSeasonLose = stat.TodayBattles - stat.TodayWin;
            stat.LastSeasonRank = stat.Rank;
            stat.TodayWin = 0;
            stat.TodayBattles = 0;
            if (stat.Rank != 0)
            {
                if (client == null)
                {
                    stat.CurrentHonor += (1001 - stat.Rank) * 1000;
                    stat.HistoryHonor += (1001 - stat.Rank) * 1000;
                }
                else
                {
                    client.CurrentHonor += (1001 - stat.Rank) * 1000;
                    client.HistoryHonor += (1001 - stat.Rank) * 1000;
                }
            }
            stat.Rank = 0;
            if (client == null)
                stat.ArenaPoints = ArenaPointFill(stat.Level);
            else
                client.ArenaPoints = ArenaPointFill(stat.Level);
            stat.LastArenaPointFill = DateTime.Now;
        }
    }
}
