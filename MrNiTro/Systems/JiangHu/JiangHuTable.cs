using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MTA.Database
{
    public static class JiangHu
    {
        private static ushort[] MinutesOnTalent = new ushort[5] { 1000, 500, 384, 322, 192 };
        private static ushort[] FreeCourseInCastle = new ushort[5] { 294, 625, 833, 1000, 1562 };
        private static byte[] FreeCourse = new byte[5] { 10, 20, 26, 31, 52 };
        private static double[] MinutesInCastle = new double[5] { 31.25, 31.25, 32, 32.2, 30 };
        private static ushort[] GetPoints = new ushort[6] { 100, 120, 150, 200, 300, 500 };
        private static ushort[] GetAlignmentExtraPoints = new ushort[9] { 0, 10, 13, 15, 18, 21, 25, 30, 50 };

        public static Dictionary<byte, List<byte>> CultivateStatus = new Dictionary<byte, List<byte>>();

        public static ushort AlignmentExtraPoints(byte amount)
        {
            return GetAlignmentExtraPoints[Math.Min(8, (int)amount)];
        }

        public static ushort GetMinutesOnTalent(byte Talent)
        {
            if (Talent == 0)
                Talent = 1;
            return MinutesOnTalent[Math.Min(4, (int)(Talent - 1))];
        }
        public static ushort GetFreeCourseInCastle(byte Talent)
        {
            if (Talent == 0)
                Talent = 1;
            return FreeCourseInCastle[Math.Min(4, (int)(Talent - 1))];
        }
        public static ushort GetFreeCourse(byte Talent)
        {
            if (Talent == 0)
                Talent = 1;
            return FreeCourse[Math.Min(4, (int)(Talent - 1))];
        }
        public static double GetMinutesInCastle(byte Talent)
        {
            if (Talent == 0)
                Talent = 1;
            return MinutesInCastle[Math.Min(4, (int)(Talent - 1))];
        }
        public static ushort GetStatusPoints(byte Level)
        {
            if (Level == 0)
                Level = 1;
            return GetPoints[Math.Min(5, (int)(Level - 1))];
        }

        public class Atribut
        {
            public byte Level;
            public byte Type;
            public ushort Power;
        }
        public static Dictionary<ushort, Atribut> Atributes = new Dictionary<ushort, Atribut>();
        public static ushort ValueToRoll(byte typ, byte level)
        {
            return (ushort)((ushort)typ + level * 256);
        }
        public static ushort GetPower(ushort UID)
        {
            return Atributes[UID].Power;
        }
        public static void LoadStatus()
        {
            try
            {
                using (Read r = new Read("JiangHu\\JianghuAttributes.txt"))
                {
                    if (r.Reader())
                    {
                        uint count = (uint)r.Count;
                        for (uint x = 0; x < count; x++)
                        {
                            string line = r.ReadString("");
                            string[] data = line.Split(' ');
                            Atribut atr = new Atribut();
                            atr.Type = byte.Parse(data[1]);
                            atr.Level = byte.Parse(data[2]);
                            atr.Power = ushort.Parse(data[3]);

                            if (atr.Type == (byte)Game.JiangHu.JiangStages.AtributesType.CriticalStrike
                                || atr.Type == (byte)Game.JiangHu.JiangStages.AtributesType.SkillCriticalStrike
                                || atr.Type == (byte)Game.JiangHu.JiangStages.AtributesType.Immunity)
                                atr.Power = (ushort)(atr.Power * 10);

                            ushort atr_val = ValueToRoll(atr.Type, atr.Level);
                            Atributes.Add(atr_val, atr);
                        }
                    }
                }
                using (Read r = new Read("JiangHu\\JingHuCultivateStatus.txt"))
                {
                    if (r.Reader())
                    {
                        uint count = (uint)r.Count;
                        for (uint x = 0; x < count; x++)
                        {
                            string line = r.ReadString("");
                            string[] data = line.Split(' ');
                            byte stage = byte.Parse(data[0]);
                            byte count_status = byte.Parse(data[1]);
                            List<byte> StatusAllows = new List<byte>();
                            for (byte i = 0; i < count_status; i++)
                                StatusAllows.Add(byte.Parse(data[2 + i]));

                            CultivateStatus.Add(stage, StatusAllows);
                        }
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public static void LoadJiangHuOLD()
        {
            using (Read r = new Read("JiangHu\\JiangHu.txt"))
            {
                if (r.Reader())
                {
                    int count = r.Count;
                    for (uint x = 0; x < count; x++)
                    {
                        string data = r.ReadString("");
                        if (data != null)
                        {
                            Game.JiangHu jiang = new Game.JiangHu(0);
                            jiang.Load(data);
                            Game.JiangHu.JiangHuClients.TryAdd(jiang.UID, jiang);
                            Game.JiangHu.JiangHuRanking.UpdateRank(jiang);
                        }
                    }
                }
            }
        }
        public static void SaveJiangHu()
        {
            using (Write _wr = new Write("JiangHu\\JiangHu.txt"))
            {
                var dictionary = Game.JiangHu.JiangHuClients.Values.ToArray();
                string[] items = new string[Game.JiangHu.JiangHuClients.Count];

                for (uint x = 0; x < Game.JiangHu.JiangHuClients.Count; x++)
                    items[x] = dictionary[x].ToString();

                _wr.Add(items, items.Length).Execute(Mode.Open);
            }
        }

        public static void LoadTableJiangHu()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT))
            {
                cmd.Select("jiang");
                using (MySqlReader rdr = new MySqlReader(cmd))
                {
                    while (rdr.Read())
                    {
                        Game.JiangHu jiang = new Game.JiangHu(0);
                        jiang.UID = rdr.ReadUInt32("UID");
                        jiang.OwnName = rdr.ReadString("OwnName");
                        jiang.CustomizedName = rdr.ReadString("CustomizedName");
                        byte[] data = rdr.ReadBlob("Powers");
                        if (data.Length > 0)
                        {
                            using (var stream = new MemoryStream(data))
                            using (var reader = new BinaryReader(stream))
                            {
                                jiang.Deserialize(reader);
                            }
                        }
                        Game.JiangHu.JiangHuClients.TryAdd(jiang.UID, jiang);
                        jiang.CreateStatusAtributes(null);
                    }
                }
            }

        }
        public static void LoadJiangHu()
        {
            if (!File.Exists("JiangHu\\JiangHu.txt"))
            {
                LoadTableJiangHu();
                SaveJiangHu();
            }
            else
                LoadJiangHuOLD();
        }

        //public static void LoadJiangHu()
        //{
        //    if (File.Exists("JiangHu\\JiangHu.txt"))
        //    {
        //        LoadJiangHuOLD();
        //        foreach (var ijiang in Game.JiangHu.JiangHuClients.Values)
        //        {
        //            var jiang = ijiang as Game.JiangHu;
        //            if (jiang != null)
        //            {
        //                if (jiang.UID == 0)
        //                    continue;
        //                using (var cmd = new MySqlCommand(MySqlCommandType.SELECT))
        //                {
        //                    cmd.Select("jiang").Where("UID", jiang.UID);
        //                    using (MySqlReader rdr = new MySqlReader(cmd))
        //                    {
        //                        if (!rdr.Read())
        //                        {
        //                            using (var command = new MySqlCommand(MySqlCommandType.INSERT))
        //                            {
        //                                command.Insert("jiang").Insert("UID", jiang.UID).Insert("OwnName", jiang.OwnName);
        //                                command.Execute();
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        SaveJiangHu();
        //        File.Delete("JiangHu\\JiangHu.txt");
        //    }
        //    else
        //    {
        //        using (var cmd = new MySqlCommand(MySqlCommandType.SELECT))
        //        {
        //            cmd.Select("jiang");
        //            using (MySqlReader rdr = new MySqlReader(cmd))
        //            {
        //                while (rdr.Read())
        //                {
        //                    Game.JiangHu jiang = new Game.JiangHu(0);
        //                    jiang.UID = rdr.ReadUInt32("UID");
        //                    jiang.OwnName = rdr.ReadString("OwnName");
        //                    jiang.CustomizedName = rdr.ReadString("CustomizedName");
        //                    byte[] data = rdr.ReadBlob("Powers");
        //                    if (data.Length > 0)
        //                    {
        //                        using (var stream = new MemoryStream(data))
        //                        using (var reader = new BinaryReader(stream))
        //                        {
        //                            jiang.Deserialize(reader);
        //                        }
        //                    }
        //                    Game.JiangHu.JiangHuClients.TryAdd(jiang.UID, jiang);
        //                    jiang.CreateStatusAtributes(null);
        //                }
        //            }
        //        }
        //    }
        //}
        //public static void New(Client.GameState client)
        //{
        //    if (client.Entity.MyJiang == null)
        //        return;
        //    using (var cmd = new MySqlCommand(MySqlCommandType.SELECT))
        //    {
        //        cmd.Select("jiang").Where("UID", client.Entity.UID);
        //        using (MySqlReader rdr = new MySqlReader(cmd))
        //        {
        //            if (!rdr.Read())
        //            {
        //                using (var command = new MySqlCommand(MySqlCommandType.INSERT))
        //                {
        //                    command.Insert("jiang").Insert("UID", client.Entity.UID).Insert("CustomizedName", client.Entity.MyJiang.CustomizedName).Insert("OwnName", client.Entity.MyJiang.OwnName);
        //                    command.Execute();
        //                }
        //            }
        //        }
        //    }            
        //}
        //public static void SaveJiangHu()
        //{
        //    foreach (var ijiang in Game.JiangHu.JiangHuClients.Values)
        //    {
        //        var jiang = ijiang as Game.JiangHu;
        //        if (jiang != null)
        //        {
        //            if (jiang.UID == 0)
        //                continue;
        //            try
        //            {
        //                MemoryStream stream = new MemoryStream();
        //                BinaryWriter writer = new BinaryWriter(stream);
        //                (jiang as Game.JiangHu).Serialize(writer);
        //                ///////////                    
        //                string SQL = "UPDATE `jiang` SET Powers=@Powers,OwnName=@OwnName,CustomizedName=@CustomizedName where UID = " + jiang.UID + " ;";
        //                byte[] rawData = stream.ToArray();
        //                using (var conn = DataHolder.MySqlConnection)
        //                {
        //                    conn.Open();
        //                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand())
        //                    {
        //                        cmd.Connection = conn;
        //                        cmd.CommandText = SQL;
        //                        cmd.Parameters.AddWithValue("@Powers", rawData);
        //                        cmd.Parameters.AddWithValue("@OwnName", jiang.OwnName);
        //                        cmd.Parameters.AddWithValue("@CustomizedName", jiang.CustomizedName);
        //                        cmd.ExecuteNonQuery();
        //                    }
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                Console.WriteLine(e);
        //            }

        //        }
        //    }
        //}
        //public static void SaveJiangHu(Client.GameState client)
        //{
        //    try
        //    {
        //        MemoryStream stream = new MemoryStream();
        //        BinaryWriter writer = new BinaryWriter(stream);
        //        client.Entity.MyJiang.Serialize(writer);
        //        ///////////                    
        //        string SQL = "UPDATE `jiang` SET Powers=@Powers,OwnName=@OwnName,CustomizedName=@CustomizedName where UID = " + client.Entity.UID + " ;";
        //        byte[] rawData = stream.ToArray();
        //        using (var conn = DataHolder.MySqlConnection)
        //        {
        //            conn.Open();
        //            using (var cmd = new MySql.Data.MySqlClient.MySqlCommand())
        //            {
        //                cmd.Connection = conn;
        //                cmd.CommandText = SQL;
        //                cmd.Parameters.AddWithValue("@Powers", rawData);
        //                cmd.Parameters.AddWithValue("@OwnName", client.Entity.MyJiang.OwnName);
        //                cmd.Parameters.AddWithValue("@CustomizedName", client.Entity.MyJiang.CustomizedName);
        //                cmd.ExecuteNonQuery();
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }

        //}       

    }
}
