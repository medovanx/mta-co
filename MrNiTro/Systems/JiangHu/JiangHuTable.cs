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
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void SaveJiangHu()
        {
            using (Write _wr = new Write(Constants.DatabaseBasePath + "JiangHu\\JiangHu.txt"))
            {
                var dictionary = Game.JiangHu.JiangHuClients.Values.ToArray();
                string[] items = new string[Game.JiangHu.JiangHuClients.Count];

                for (uint x = 0; x < Game.JiangHu.JiangHuClients.Count; x++)
                    items[x] = dictionary[x].ToString();

                _wr.Add(items, items.Length).Execute(Mode.Open);
            }
        }

        public static void LoadJiangHu()
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
    }
}
