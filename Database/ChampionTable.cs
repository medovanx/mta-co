using System;
using MTA.Network.GamePackets;

namespace MTA.Database
{
    public class ChampionTable
    {
        public static void Load()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("championarena"))
            using (var reader = new MySqlReader(cmd))
            {
                while (reader.Read())
                {
                    Network.GamePackets.ChampionStatistic stat = new Network.GamePackets.ChampionStatistic(true);
                    stat.UID = reader.ReadUInt32("EntityID");
                    stat.Name = reader.ReadString("EntityName");
                    stat.YesterdayRank = reader.ReadUInt32("YesterdayRank");
                    stat.TotalMatches = reader.ReadByte("TotalMatches");
                    stat.WinStreak = reader.ReadByte("WinStreak");
                    stat.SignedUp = reader.ReadBoolean("SignedUp");
                    stat.Points = reader.ReadUInt32("Points");
                    stat.TotalPoints = reader.ReadUInt32("TotalPoints");
                    stat.TodayPoints = reader.ReadUInt32("TodayPoints");
                    stat.YesterdayPoints = reader.ReadUInt32("YesterdayPoints");
                    stat.Grade = (byte)stat.IsGrade;
                    stat.Level = reader.ReadByte("Level");
                    stat.Class = reader.ReadByte("Class");
                    stat.Model = reader.ReadUInt32("Model");
                    stat.LastReset = DateTime.FromBinary(reader.ReadInt64("LastReset"));
                    if (stat.LastReset.DayOfYear != DateTime.Now.DayOfYear)
                        Reset(stat);
                    Game.Champion.ChampionStats.Add(stat.UID, stat);
                }
            }

            Game.Champion.Sort();
            Game.Champion.YesterdaySort();
            Console.WriteLine("Champion information loaded.");
        }

        public static void SaveStatistics(Network.GamePackets.ChampionStatistic stats, MySql.Data.MySqlClient.MySqlConnection conn)
        {
            if (stats == null) return;
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("championarena"))
                cmd.Set("YesterdayRank", stats.YesterdayRank)
                .Set("Points", stats.Points).Set("YesterdayPoints", stats.YesterdayPoints)
                .Set("TodayPoints", stats.TodayPoints).Set("TotalPoints", stats.TotalPoints)
                .Set("WinStreak", stats.WinStreak).Set("SignedUp", stats.SignedUp)
                .Set("Level", stats.Level).Set("Level", stats.Level).Set("Class", stats.Class)
                .Set("EntityName", stats.Name).Set("LastReset", stats.LastReset.Ticks).Set("Model", stats.Model)
                .Set("Class", stats.Class).Where("EntityID", stats.UID)
                .Execute(conn);
        }
        public static void SaveStats(Network.GamePackets.ChampionStatistic stats)
        {
            using (var conn = DataHolder.MySqlConnection)
            {
                conn.Open();
                SaveStatistics(stats, conn);
            }
        }
        public static void InsertStatistic(Client.GameState client)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("championarena")
              .Insert("EntityName", client.ChampionStats.Name).Insert("Points", client.ChampionStats.Points)
              .Insert("Level", client.ChampionStats.Level).Insert("Class", client.ChampionStats.Class).Insert("Model", client.ChampionStats.Model)
              .Insert("EntityID", client.ChampionStats.UID))
                cmd.Execute();
        }

        public static void Reset(ChampionStatistic stat)
        {
            stat.YesterdayPoints = stat.TodayPoints;
            stat.YesterdayRank = stat.Rank;
            stat.TodayPoints = 0;
            stat.TotalMatches = 0;
            stat.WinStreak = 0;
            stat.Rank = 0;
            stat.SignedUp = false;
            stat.LastReset = DateTime.Now;
        }
    }
}
