using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conquer_Online_Server.Network.GamePackets;
using Conquer_Online_Server.Database;


namespace Conquer_Online_Server.Game
{
    class Countrys
    {        
        public string Name = "";
        public ulong Score = 0;
        public uint wins = 0;
        public uint id = 0;
        public uint playres = 0;
    }
    class CrossServer
    {
        public static SobNpcSpawn Pole;
        public static bool IsWar = false;
        public static Dictionary<uint, Countrys> Countrys = new Dictionary<uint, Countrys>(1000);
        public static Time32 ScoreSendStamp;
        public static Countrys PoleKeeper, Current;
        private static string[] scoreMessages;
        private static bool changed = false;
        public static DateTime StartTime;
        public static uint KeeperID;
        public static ushort mapid = 2578;
        public static uint npc = 854;
        public static uint hour = 22;

        public static void Initiate()
        {
            var Map = Kernel.Maps[mapid];
            Pole = (SobNpcSpawn)Map.Npcs[npc];
        }
        public static void Reward()
        {

            var top8 = Countrys.Values.Where(p => p.wins != 0).OrderByDescending(p => p.wins).ToArray();
            for (int i = 0; i < top8.Length; i++)
            {
                foreach (var client in Kernel.GamePool.Values)
                {

                    if ((uint)client.Entity.CountryID == top8[i].id)
                    {
                        top8[i].playres += 1;
                    }

                }
            }
            for (int i = 0; i < top8.Length; i++)
            {
                foreach (var client in Kernel.GamePool.Values)
                {

                    if ((uint)client.Entity.CountryID == top8[i].id)
                    {
                        uint prize = ((((uint)1200000 - ((uint)i * 200000)) / top8[i].playres));
                        if (prize > 0)
                        {
                            client.Entity.ConquerPoints += prize;
                        }
                    }
                }

            }
            Save();

        }
        public static void Save()
        {
            uint i = 0;
            foreach (var C in Countrys.Values)
            {
                if (i <= 7)
                {
                    using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("ctfcross")
                   .Insert("Name", C.Name).Insert("Wins", C.wins).Insert("ID", C.id))
                        cmd.Execute();
                }
                i += 1;
            }
        }
        public static void Load()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("ctfcross"))
            using (var reader = new MySqlReader(cmd))
            {
                while (reader.Read())
                {
                    Countrys c = new Countrys();
                    c.Name = reader.ReadString("Name");
                    c.wins = reader.ReadUInt32("Wins");
                    c.id = reader.ReadUInt32("ID");
                    Countrys.Add(c.id, c);
                }
            }
        }
        //public static void Delete()
        //{
        //    using (var cmdd = new MySqlCommand(MySqlCommandType.UPDATE))
        //        cmdd.UPDATE("ctfcross").Execute();
        //}
        public static void End()
        {
            uint place = 0;
            foreach (var C in Countrys.Values.OrderByDescending((p) => p.wins))
            {
                if (place == 0)
                {
                    PoleKeeper = C;
                }
                place++;
            }
            if (PoleKeeper != null)
            {
                Kernel.SendWorldMessage(new Message("[CrossServer CTF], " + PoleKeeper.Name + ", has won this CrossServer CTF", System.Drawing.Color.White, Message.Center), Program.Values);

            }
            IsWar = false;
            UpdatePole(Pole);
            Reward();
        }
        public static void Start()
        {            
            Countrys.Clear();
            StartTime = DateTime.Now;
            Kernel.SendWorldMessage(new Message("CrossServer CTF Has began!", System.Drawing.Color.Red, Message.Center), Program.Values);
            foreach (var client in Kernel.GamePool.Values)
            {
                client.Country = GetCountry((uint)client.Entity.CountryID);
                Countrys country = new Countrys();
                country.id = (uint)client.Entity.CountryID;
                country.Name = GetCountry((uint)client.Entity.CountryID);
                country.Score = 0;
                country.wins = 0;
                if (!Countrys.ContainsKey(country.id))
                {
                    Countrys.Add(country.id, country);
                }

            }
            IsWar = true;
            StartTime = DateTime.Now;

        }
        public static void Reset()
        {
            Pole.Hitpoints = Pole.MaxHitpoints;
            foreach (var C in Countrys.Values)
            {
                C.Score = 0;
            }
            IsWar = true;
        }
        public static void FinishRound()
        {

            SortScores(out PoleKeeper);
            if (PoleKeeper != null)
            {
                KeeperID = PoleKeeper.id;
                Pole.Name = PoleKeeper.Name;
                PoleKeeper.wins++;
                Kernel.SendWorldMessage(new Message("حرب عبر السيرفيرات, " + PoleKeeper.Name + ", فاز بمرات " + PoleKeeper.wins + " وقد فاز هذا حرب عبر السيرفيرات جولة الحرب!", System.Drawing.Color.Red, Message.Center), Program.Values);
            }
            Pole.Hitpoints = Pole.MaxHitpoints;
            Kernel.SendWorldMessage(Pole, Program.Values, (ushort)mapid);
            Reset();
        }
        public static void AddScore(uint addScore, uint id)
        {
            foreach (var C in Countrys.Values)
            {
                if (C != null)
                {
                    if (C.id == id)
                    {
                        C.Score += addScore;
                        changed = true;
                        if ((int)Pole.Hitpoints <= 0)
                        {
                            FinishRound();
                            return;
                        }
                    }
                }
            }
        }
        public static void SendScores()
        {
            if (scoreMessages == null)
                scoreMessages = new string[0];
            if (Countrys.Count == 0)
                return;
            if (changed)
                SortScores(out Current);

            for (int c = 0; c < scoreMessages.Length; c++)
            {
                Message msg = new Message(scoreMessages[c], System.Drawing.Color.Red, c == 0 ? Message.FirstRightCorner : Message.ContinueRightCorner);
                Kernel.SendWorldMessage(msg, Program.Values, (ushort)mapid);
            }
        }
        private static void SortScores(out Countrys winner)
        {
            winner = null;
            List<string> ret = new List<string>();
            int Place = 0;
            foreach (var C in Countrys.Values.OrderByDescending((p) => p.Score))
            {
                if (Place == 0)
                    winner = C;
                string str = "No  " + (Place + 1).ToString() + ": " + C.Name + "(" + C.Score + ")";
                ret.Add(str);
                Place++;
                if (Place == 4)
                    break;
            }

            changed = false;
            scoreMessages = ret.ToArray();
        }
        public static string GetCountry(uint ID)
        {
            string Country = "";
            switch (ID)
            {
                case 0: Country = "قهر"; break;
                case 1: Country = "مصر"; break;
                case 2: Country = "سعودية"; break;
                case 3: Country = "امارات"; break;
                case 4: Country = "الأردن"; break;
                case 5: Country = "الجزائر"; break;
                case 6: Country = "الكويت"; break;
                case 7: Country = "قطر"; break;
                case 8: Country = "عمان"; break;
                case 9: Country = "لبنان"; break;
                case 10: Country = "اليمن"; break;
                case 11: Country = "ليبيا"; break;
                case 12: Country = "العراق"; break;
                case 13: Country = "فلسطين"; break;
                case 14: Country = "سودان"; break;
                case 15: Country = "سوريا"; break;
                case 16: Country = "المغرب"; break;
                case 17: Country = "تونس"; break;
                case 18: Country = "مورتانيا"; break;
                case 19: Country = "البحرين"; break;
                case 20: Country = "قهر"; break;

            }
            return Country;

        }
        private static void UpdatePole(SobNpcSpawn pole)
        {
            if (pole == null) return;
            new Database.MySqlCommand(Conquer_Online_Server.Database.MySqlCommandType.UPDATE)
            .Update("sobnpcs").Set("name", pole.Name).Set("life", Pole.Hitpoints).Where("id", pole.UID).Execute();
        }
        public static byte[] generateRanking(uint m)
        {
            var array = Countrys.Values.Where(p => p.wins != 0).OrderByDescending(p => p.wins).ToArray();
            return generateList(m, 10, array, p => p.wins);
        }
        private static byte[] generateList(uint m, int type, Countrys[] array = null, Func<Countrys, UInt32> select = null)
        {

            byte[] data = null;
            if (array == null)
                data = new byte[708];
            else
                data = new byte[708];
            if (array == null)
                return data;
            Network.Writer.WriteInt32(700, 0, data);
            Network.Writer.WriteUInt16(1063, 2, data);
            Network.Writer.WriteInt32(type, 4, data);
            Network.Writer.WriteInt32(3, 6, data);
            Network.Writer.WriteInt32(array.Length, 10, data);
            Network.Writer.WriteInt32(8, 14, data);
            if (array != null)
            {
                uint b = 0;
                uint num2 = (m - 1u) * 8u;
                int offset = 30;
                while (b < array.Length)
                {
                    if ((uint)b >= num2)
                    {
                        if ((uint)b >= num2 + 8u || (int)b >= array.Length)
                        {
                            break;
                        }
                        Network.Writer.WriteUInt32(b + 1, offset, data); offset += 2;
                        Network.Writer.WriteString(Constants.ServerName, offset, data); offset += 16;
                        Network.Writer.WriteString(array[(int)b].Name, offset, data); offset += 36;
                        Network.Writer.WriteUInt32(array[(int)b].wins, offset, data); offset += 4;
                        Network.Writer.WriteUInt32(8, offset, data); offset += 4;
                    }
                    b += 1;
                }
            }
            return data;
        }
    }
}
