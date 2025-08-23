using System;
using MTA.Network.GamePackets;

namespace MTA.Database
{
    public class ArenaTable
    {
        public static void Load()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("arena"))
            using (var reader = new MySqlReader(cmd))
            {
                while (reader.Read())
                {
                    Network.GamePackets.ArenaStatistic stat = new Network.GamePackets.ArenaStatistic(true);
                    stat.EntityID = reader.ReadUInt32("EntityID");
                    stat.Name = reader.ReadString("EntityName");
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

            Game.Arena.Sort();
            Game.Arena.YesterdaySort();
            Console.WriteLine("Arena information loaded.");
        }

        public static uint ArenaPointFill(byte level)
        {
            if (level >= 70 && level < 100)
                return 1000;
            else if (level >= 100 && level < 110)
                return 2000;
            else if (level >= 110 && level < 120)
                return 3000;
            else if (level >= 120)
                return 4000;
            return 0;
        }

        public static void SaveArenaStatistics(Network.GamePackets.ArenaStatistic stats, MySql.Data.MySqlClient.MySqlConnection conn)
        {
            if (stats == null) return;
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("arena"))
                cmd.Set("LastSeasonRank", stats.LastSeasonRank)
                .Set("ArenaPoints", stats.ArenaPoints).Set("TodayWin", stats.TodayWin)
                .Set("TodayBattles", stats.TodayBattles).Set("LastSeasonWin", stats.LastSeasonWin)
                .Set("LastSeasonLose", stats.LastSeasonLose).Set("TotalWin", stats.TotalWin)
                .Set("TotalLose", stats.TotalLose).Set("HistoryHonor", stats.HistoryHonor)
                .Set("CurrentHonor", stats.CurrentHonor).Set("Level", stats.Level).Set("Class", stats.Class)
                .Set("EntityName", stats.Name).Set("ArenaPointFill", stats.LastArenaPointFill.Ticks).Set("Model", stats.Model)
                .Set("Class", stats.Class).Set("LastSeasonArenaPoints", stats.LastSeasonArenaPoints).Where("EntityID", stats.EntityID)
                .Execute();
        }
        public static void SaveArenaStatistics(Network.GamePackets.ArenaStatistic stats)
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
              .Insert("EntityName", client.ArenaStatistic.Name).Insert("ArenaPoints", client.ArenaStatistic.ArenaPoints)
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
                stat.ArenaPoints = Database.ArenaTable.ArenaPointFill(stat.Level);
            else
                client.ArenaPoints = Database.ArenaTable.ArenaPointFill(stat.Level);
            stat.LastArenaPointFill = DateTime.Now;
        }
    }
}
