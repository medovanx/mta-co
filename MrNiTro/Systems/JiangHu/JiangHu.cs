using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Client;
using System.IO;

namespace MTA.Game
{
    public interface IJiangHu
    {
        string OwnName { get; set; }
        string CustomizedName { get; set; }
        uint UID { get; set; }
        byte Talent { get; set; }
        byte Stage { get; set; }
        byte Star { get; set; }
        DateTime StartCountDwon { get; set; }
        DateTime CountDownEnd { get; set; }
        uint Time { get; }
        uint FreeTimeTodey { get; set; }
        uint FreeTimeTodeyUsed { get; set; }
        uint Inner_Strength { get; set; }
        uint FreeCourse { get; set; }
        JiangHu.JiangStages[] Stagers { get; set; }
        byte Rank { get; set; }
        byte Level { get; set; }
        ushort RoundBuyPoints { get; set; }
        bool OnJiangMode { get; set; }
    }
    public class JiangHu : IJiangHu
    {
        public enum AttackFlag
        {
            None = 0,
            NotHitFriends = 1,
            NotHitClanMembers = 2,
            NotHitGuildMembers = 4,
            NotHitAlliedGuild = 8,
            NoHitAlliesClan = 16

        }
        public static System.Collections.Concurrent.ConcurrentDictionary<uint, IJiangHu> JiangHuClients = new System.Collections.Concurrent.ConcurrentDictionary<uint, IJiangHu>();

        public static class JiangHuRanking
        {
            private static System.Collections.Concurrent.ConcurrentDictionary<uint, IJiangHu> TopRank = new System.Collections.Concurrent.ConcurrentDictionary<uint, IJiangHu>();
            public static IJiangHu[] TopRank100 = null;
            private static object SyncRoot = new object();
            public static void UpdateRank(IJiangHu jiang)
            {
                lock (SyncRoot)
                {
                    if (!TopRank.ContainsKey(jiang.UID))
                        TopRank.TryAdd(jiang.UID, jiang);
                    CalculateRanks();
                }
            }
            private static void CalculateRanks()
            {
                foreach (var jiang in TopRank.Values)
                    jiang.Rank = 0;
                var rankdictionar = TopRank.Values.ToArray();
                var ordonateRank = from jiang in rankdictionar orderby jiang.Inner_Strength descending select jiang;
                List<IJiangHu> BackUp = new List<IJiangHu>();
                byte x = 1;
                foreach (var jiang in ordonateRank)
                {
                    if (x == 101)
                        break;
                    jiang.Rank = x;
                    BackUp.Add(jiang);
                    x++;
                }
                TopRank100 = BackUp.ToArray();
                TopRank = new System.Collections.Concurrent.ConcurrentDictionary<uint, IJiangHu>();
                foreach (var jiang in BackUp)
                    TopRank.TryAdd(jiang.UID, jiang);
                BackUp = null;

            }
        }

        public class GetNewStar
        {
            public byte Stage;
            public byte PositionStar;
            public JiangStages.Star Star;
        }
        public class JiangStages
        {
            public class Star
            {
                public AtributesType Typ;
                public byte Level;
                public ushort UID;
                public bool Activate = false;

                public override string ToString()
                {
                    StringBuilder build = new StringBuilder();
                    build.Append((byte)(Activate ? 1 : 0) + "#" + UID + "#");
                    return build.ToString();
                }
            }
            public enum AtributesType
            {
                None = 0,
                MaxLife,//1
                PAttack,//2
                MAttack,//3
                PDefense,//4
                Mdefense,//5
                FinalAttack,//6
                FinalMagicAttack,//7
                FinalDefense,//8
                FinalMagicDefense,//9
                CriticalStrike,//10
                SkillCriticalStrike,//11
                Immunity,//12
                Breakthrough,//13
                Counteraction,//14
                MaxMana//15
            }
            public Star[] Stars;
            public bool Activate = false;
            public JiangStages()
            {
                Stars = new Star[9];
                for (byte x = 0; x < 9; x++)
                    Stars[x] = new Star();
            }
            public bool ContainAtribut(AtributesType typ)
            {
                foreach (var atr in Stars)
                    if (atr.Typ == typ)
                        return true;
                return false;
            }
            public override string ToString()
            {

                StringBuilder build = new StringBuilder();
                build.Append((byte)(Activate ? 1 : 0) + "#");
                foreach (var obj in Stars)
                    build.Append(obj.ToString());
                return build.ToString();
            }
        }

        public ushort ValueToRoll(JiangStages.AtributesType status, byte level)
        {
            return (ushort)((ushort)status + level * 256);
        }
        public byte GetValueLevel(ushort val)
        {
            return (byte)((val - (ushort)(val % 256)) / 256);
        }
        public JiangStages.AtributesType GetValueType(uint val)
        {
            return (JiangStages.AtributesType)(val % 256);
        }

        public GetNewStar MyNewStart = null;
        public void CreateRollValue(Client.GameState client, byte mStar, byte mStage, bool super = false, byte Higher = 0)
        {
            JiangStages n_stage = Stagers[mStage - 1];
            if (!n_stage.Activate) return;
            MyNewStart = new GetNewStar();
            MyNewStart.PositionStar = mStar;
            MyNewStart.Stage = mStage;

            MyNewStart.Star = new JiangStages.Star();
            MyNewStart.Star.Activate = true;
            var level = MyNewStart.Star.Level;
            MyNewStart.Star.Level = GetStatusLevel(super);



            //if (Higher == 1)//higher
            //    if (Kernel.Rate(20 / Math.Max(1,(int)level)))
            //        MyNewStart.Star.Level = (byte)Random.Next(level, 5);
            //if (Higher == 2)//highest
            //    if (Kernel.Rate(50 / Math.Max(1, (int)level)))
            //        MyNewStart.Star.Level = (byte)Random.Next(4, 6);

            MyNewStart.Star.Typ = (JiangStages.AtributesType)Random.Next(1, 16);

            do
            {
                MyNewStart.Star.Typ = (JiangStages.AtributesType)Random.Next(1, 16);
            }
            while (!Database.JiangHu.CultivateStatus[mStage/*MyNewStart.Star.Level*/].Contains((byte)MyNewStart.Star.Typ));

            //do
            //{
            //    MyNewStart.Star.Typ = (JiangStages.AtributesType)Random.Next(1, 16);
            //}
            //while (!CanKeepChiPower(n_stage, MyNewStart, client));


            MyNewStart.Star.UID = ValueToRoll(MyNewStart.Star.Typ, MyNewStart.Star.Level);

            Network.GamePackets.JiangHuUpdate upd = new Network.GamePackets.JiangHuUpdate();

            upd.Atribute = MyNewStart.Star.UID;
            upd.FreeCourse = FreeCourse;
            ///  upd.Talent = Talent;
            upd.Stage = mStage;
            upd.Star = mStar;
            upd.FreeTimeTodeyUsed = (byte)FreeTimeTodeyUsed;
            upd.RoundBuyPoints = RoundBuyPoints;
            client.Send(upd.ToArray());


        }

        private static bool CanKeepChiPower(JiangStages strct, GetNewStar nstar, GameState client)
        {
            //#if NOTMULTIPLECHIPOWERS
            if (client.Entity.VIPLevel > 6)
            {
                var stars = strct.Stars;
                for (int i = 0; i < stars.Length; i++)
                    if (i != (nstar.PositionStar - 1))
                        if (stars[i].Typ == nstar.Star.Typ)
                            return false;
            }
            //#endif
            return true;
        }

        public void ApplayNewStar(Client.GameState client)
        {
            if (MyNewStart == null)
                return;
            JiangStages n_stage = Stagers[MyNewStart.Stage - 1];
            if (!n_stage.Activate) return;

            JiangStages.Star n_star = n_stage.Stars[MyNewStart.PositionStar - 1];
            if (!n_star.Activate)
            {
                Star++;
                n_star.Activate = true;
            }
            n_star.Level = MyNewStart.Star.Level;
            n_star.Typ = MyNewStart.Star.Typ;
            n_star.UID = MyNewStart.Star.UID;

            client.LoadItemStats();
            if (MyNewStart.Stage < 9)
            {
                if (MyNewStart.PositionStar == 9 && !Stagers[MyNewStart.Stage].Activate)
                {
                    Stage++;
                    Stagers[MyNewStart.Stage].Activate = true;
                    SendInfo(client, Network.GamePackets.JiangHu.OpenStage, Stage.ToString());
                }
            }
            //JiangHuRanking.UpdateRank(this);
            MyNewStart = null;
        }
        public static byte Getlve = 6;
        public byte GetStatusLevel(bool super = false)
        {
            //  return Getlve;
            if (super)
                return 6;
            byte first = (byte)Random.Next(1, 6);
            if (first >= 6)
            {
                first = (byte)Random.Next(1, 6);
                if (first > 6)
                    first = 6;
            }
            return first;
        }


        public void CreateStatusAtributes(Game.Entity client)// cand loghez datele.
        {
            uint oldInner_Strength = Inner_Strength;
            Inner_Strength = 0;
            foreach (var nstage in Stagers)
            {
                if (!nstage.Activate) continue;

                var atr = nstage.Stars.Where(p => p.UID != 0).ToArray();

                byte count_doble = 0;

                Dictionary<uint, List<JiangStages.Star>> alignementstars = new Dictionary<uint, List<JiangStages.Star>>();
                List<JiangStages.Star> normalstarts = new List<JiangStages.Star>();
                ushort counts_alignements = 0;
                for (byte x = 0; x < atr.Length; x++)
                {
                    var atribut = atr[x];
                    count_doble = 0;
                    bool wasadd = false;
                    for (byte y = (byte)(x + 1); y < atr.Length; y++)
                    {
                        var atr2nd = atr[y];
                        if (atr2nd.Typ == atribut.Typ)
                        {
                            if (!wasadd)
                            {
                                if (!alignementstars.ContainsKey(counts_alignements))
                                {
                                    alignementstars.Add(counts_alignements, new List<JiangStages.Star>());
                                    alignementstars[counts_alignements].Add(atribut);
                                }
                            }
                            if (!alignementstars.ContainsKey(counts_alignements))
                            {
                                alignementstars.Add(counts_alignements, new List<JiangStages.Star>());
                                alignementstars[counts_alignements].Add(atr2nd);
                            }
                            else
                                alignementstars[counts_alignements].Add(atr2nd);
                            wasadd = true;
                            x = y;
                            count_doble++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    uint counts = 1;
                    if (count_doble != 0)
                    {
                        counts = (byte)(count_doble + 1);
                        counts_alignements++;
                    }
                    if (counts == 1)
                        normalstarts.Add(atribut);

                }
                byte percent = (byte)Database.JiangHu.AlignmentExtraPoints(count_doble);
                foreach (var allignement in alignementstars.Values)
                {
                    for (int i = 0; i < allignement.Count; i++)
                    {

                        Database.JiangHu.Atribut Atri_bas = Database.JiangHu.Atributes[allignement[i].UID];
                        if (client != null)
                            IncreaseStatus(client, (JiangStages.AtributesType)Atri_bas.Type, (ushort)((Atri_bas.Power + (Atri_bas.Power * percent) / 100)));
                    }

                }
                count_doble = 0;
                foreach (var allignement in alignementstars.Values)
                    CalculateInner_StrengthAlignements(allignement);
                for (int x = 0; x < normalstarts.Count; x++)
                {
                    Database.JiangHu.Atribut Atri_bas = Database.JiangHu.Atributes[normalstarts[x].UID];
                    if (client != null)
                        IncreaseStatus(client, (JiangStages.AtributesType)Atri_bas.Type, (ushort)Atri_bas.Power);
                    Inner_Strength += Database.JiangHu.GetStatusPoints(normalstarts[x].Level);
                }
            }
            if (oldInner_Strength != Inner_Strength)
                JiangHuRanking.UpdateRank(this);
        }
        public void CalculateInner_StrengthAlignements(List<JiangStages.Star> collection)
        {
            ushort points = 0;
            for (int x = 0; x < collection.Count; x++)
            {
                points += Database.JiangHu.GetStatusPoints(collection[x].Level);
            }
            if (collection.Count > 0 && collection.Count < 9)
                Inner_Strength += (ushort)(points + ((points * ((collection.Count) * 10) / 100)));
            else if (collection.Count == 9)
                Inner_Strength += (ushort)(points * 2);
        }
        public void IncreaseStatus(Entity client, JiangStages.AtributesType status, ushort Power)
        {
            switch (status)
            {
                case JiangStages.AtributesType.Breakthrough:
                    client.Breaktrough += Power;
                    break;
                case JiangStages.AtributesType.Counteraction:
                    client.Counteraction += Power; break;
                case JiangStages.AtributesType.CriticalStrike:
                    client.CriticalStrike += Power; break;
                case JiangStages.AtributesType.FinalAttack:
                    client.PhysicalDamageIncrease += 1;
                    break;
                case JiangStages.AtributesType.FinalDefense:
                    client.PhysicalDamageDecrease += Power; break;
                case JiangStages.AtributesType.FinalMagicAttack:
                    client.MagicDamageIncrease += Power; break;
                case JiangStages.AtributesType.FinalMagicDefense:
                    client.MagicDamageDecrease += Power; break;
                case JiangStages.AtributesType.Immunity:
                    client.Immunity += Power; break;
                case JiangStages.AtributesType.MAttack:
                    {


                        client.MAttack += Power;
                        client.BaseMagicAttack += Power; break;
                    }
                case JiangStages.AtributesType.MaxLife:
                    client.ItemHP += Power; break;//24283 27118 - 24283
                case JiangStages.AtributesType.MaxMana:
                    client.ItemMP += Power; break;
                case JiangStages.AtributesType.Mdefense:
                    {
                        //client.MDefense += Power;
                        client.MagicDefence += Power; break;
                    }
                case JiangStages.AtributesType.PAttack:
                    {
                        // client.TQMAXATTACK += Power;
                        //  client.TQMINATTACK += Power;

                        client.BaseMaxAttack += Power;
                        client.BaseMinAttack += Power; break;
                    }
                case JiangStages.AtributesType.PDefense:
                    client.Defence += Power; break;
                case JiangStages.AtributesType.SkillCriticalStrike:
                    client.SkillCStrike += Power; break;
            }
        }
        public void DecreaseStatus(Entity client, JiangStages.AtributesType status, ushort Power)
        {
            switch (status)
            {
                case JiangStages.AtributesType.Breakthrough:
                    client.Breaktrough -= Power;
                    break;
                case JiangStages.AtributesType.Counteraction:
                    client.Counteraction -= Power; break;
                case JiangStages.AtributesType.CriticalStrike:
                    client.CriticalStrike -= Power; break;
                case JiangStages.AtributesType.FinalAttack:
                    client.PhysicalDamageIncrease -= Power; break;
                case JiangStages.AtributesType.FinalDefense:
                    client.PhysicalDamageDecrease -= Power; break;
                case JiangStages.AtributesType.FinalMagicAttack:
                    client.MagicDamageIncrease -= Power; break;
                case JiangStages.AtributesType.FinalMagicDefense:
                    client.MagicDamageDecrease -= Power; break;
                case JiangStages.AtributesType.Immunity:
                    client.Immunity -= Power; break;
                case JiangStages.AtributesType.MAttack:
                    client.MagicAttack -= Power; break;
                case JiangStages.AtributesType.MaxLife:
                    client.MaxHitpoints -= Power; break;
                case JiangStages.AtributesType.MaxMana:
                    client.MaxMana -= Power; break;
                case JiangStages.AtributesType.Mdefense:
                    client.MagicDefence -= Power; break;
                case JiangStages.AtributesType.PAttack:
                    {
                        client.BaseMaxAttack -= Power;
                        client.BaseMinAttack -= Power; break;
                    }
                case JiangStages.AtributesType.PDefense:
                    client.Defence -= Power; break;
                case JiangStages.AtributesType.SkillCriticalStrike:
                    client.SkillCStrike -= Power; break;
            }
        }
        public void CreateStarDMG(JiangStages stage)
        {

        }
        public uint UID { get; set; }
        public string OwnName { get; set; }
        public string CustomizedName { get; set; }
        public byte Level { get; set; }

        public byte Talent { get; set; }
        public byte Stage { get; set; }
        public byte Star { get; set; }
        public uint FreeCourse { get; set; }

        public byte Rank { get; set; }
        public DateTime StartCountDwon { get; set; }
        public DateTime CountDownEnd { get; set; }
        public DateTime RemoveJiangMod;
        public DateTime TimerStamp;
        public uint Time
        {
            get
            {
                return (uint)(CountDownEnd - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
            }
        }
        public bool OnJiangMode { get; set; }

        public uint FreeTimeTodey { get; set; }
        public uint FreeTimeTodeyUsed { get; set; }

        public uint Inner_Strength { get; set; }
        public ushort RoundBuyPoints { get; set; }
        public Random Random = new Random();
        public JiangStages[] Stagers { get; set; }

        public override string ToString()
        {
            if (StartCountDwon.Ticks > CountDownEnd.Ticks)
                CreateTime();
            uint SecoundesLeft = (uint)((CountDownEnd.Ticks - StartCountDwon.Ticks) / 10000000);
            if (OwnName.Contains('#'))
                OwnName = OwnName.Replace("#", "");
            if (CustomizedName.Contains('#'))
                CustomizedName = CustomizedName.Replace("#", "");

            StringBuilder build = new StringBuilder();
            build.Append(UID + "#" + OwnName + "#" + CustomizedName + "#" + Level + "#" +
                Talent + "#" + Stage + "#" + Star + "#" + FreeTimeTodeyUsed + "#" +
                (byte)(OnJiangMode ? 0 : 1) + "#" + FreeCourse + "#" + SecoundesLeft + "#" + RoundBuyPoints + "#");
            foreach (var obj in Stagers)
                build.Append(obj.ToString());
            return build.ToString();
        }

        public void Load(string Line, uint nUID = 0)
        {
            try
            {
                if (Line == null) return;
                if (Line == "") return;
                if (!Line.Contains('#')) return;
                string[] data = Line.Split('#');
                if (nUID != 0)
                    UID = nUID;
                else
                    UID = uint.Parse(data[0]);
                OwnName = data[1];
                CustomizedName = data[2];

                try
                {
                    Level = byte.Parse(data[3]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                Talent = byte.Parse(data[4]);
                Stage = byte.Parse(data[5]);
                Star = byte.Parse(data[6]);
                FreeTimeTodey = byte.Parse(data[7]);
                OnJiangMode = byte.Parse(data[8]) == 1;
                FreeCourse = uint.Parse(data[9]);
                StartCountDwon = DateTime.Now;
                TimerStamp = DateTime.Now;
                CountDownEnd = DateTime.Now.AddSeconds(uint.Parse(data[10]));
                RoundBuyPoints = ushort.Parse(data[11]);
                ushort position = 12;

                foreach (var nstage in Stagers)
                {
                    nstage.Activate = byte.Parse(data[position]) == 1;
                    position++;
                    foreach (var nstar in nstage.Stars)
                    {

                        nstar.Activate = byte.Parse(data[position]) == 1;
                        position++;
                        nstar.UID = ushort.Parse(data[position]);
                        position++;
                        if (nstar.Activate)
                        {
                            nstar.Typ = GetValueType(nstar.UID);
                            nstar.Level = GetValueLevel(nstar.UID);
                            Inner_Strength += Database.JiangHu.GetStatusPoints(nstar.Level);
                        }
                    }

                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public void Deserialize(BinaryReader reader)
        {
            try
            {
                Level = reader.ReadByte();
                Talent = reader.ReadByte();
                Stage = reader.ReadByte();
                Star = reader.ReadByte();
                FreeTimeTodey = reader.ReadUInt32();
                OnJiangMode = reader.ReadBoolean();
                FreeCourse = reader.ReadUInt32();
                StartCountDwon = DateTime.Now;
                TimerStamp = DateTime.Now;
                CountDownEnd = DateTime.Now.AddSeconds(reader.ReadUInt32());
                RoundBuyPoints = reader.ReadUInt16();
                foreach (var nstage in Stagers)
                {
                    nstage.Activate = reader.ReadBoolean();
                    foreach (var nstar in nstage.Stars)
                    {
                        nstar.Activate = reader.ReadBoolean();
                        nstar.UID = reader.ReadUInt16();
                        if (nstar.Activate)
                        {
                            nstar.Typ = GetValueType(nstar.UID);
                            nstar.Level = GetValueLevel(nstar.UID);
                            Inner_Strength += Database.JiangHu.GetStatusPoints(nstar.Level);
                        }
                    }

                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public void Serialize(BinaryWriter writer)
        {
            try
            {
                if (StartCountDwon.Ticks > CountDownEnd.Ticks)
                    CreateTime();
                uint SecoundesLeft = (uint)((CountDownEnd.Ticks - StartCountDwon.Ticks) / 10000000);
                writer.Write(Level);
                writer.Write(Talent);
                writer.Write(Stage);
                writer.Write(Star);
                writer.Write(FreeTimeTodey);
                writer.Write(OnJiangMode);
                writer.Write(FreeCourse);
                writer.Write(SecoundesLeft);
                writer.Write(RoundBuyPoints);
                foreach (var nstage in Stagers)
                {
                    writer.Write(nstage.Activate);
                    foreach (var nstar in nstage.Stars)
                    {
                        writer.Write(nstar.Activate);
                        writer.Write(nstar.UID);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public JiangHu(uint m_UID)
        {
            Talent = 3;
            Stage = 1;
            Star = 1;
            FreeCourse = 10000;
            FreeTimeTodey = 10;
            UID = m_UID;
            CountDownEnd = new DateTime();
            StartCountDwon = new DateTime();
            TimerStamp = new DateTime();
            RemoveJiangMod = new DateTime();
            Stagers = new JiangStages[9];
            for (byte x = 0; x < 9; x++)
                Stagers[x] = new JiangStages();
            if (!JiangHuClients.ContainsKey(UID) && UID != 0)
                JiangHuClients.TryAdd(UID, this);
            OnJiangMode = true;
            TimerStamp = DateTime.Now;
        }
        public void ResetDay(Client.GameState client)
        {
            RoundBuyPoints = 0;
            FreeTimeTodeyUsed = 0;
            SendStatus(client, client);
        }

        public void OnloginClient(Client.GameState client)
        {
            SendStatus(client, client);
            SendStatusMode(client);
            TimerStamp = DateTime.Now;
        }
        public void SendStatusMode(Client.GameState client)
        {

            client.Entity.JiangTalent = Talent;
            client.Entity.JiangActive = OnJiangMode;
            SendInfo(client, Network.GamePackets.JiangHu.InfoStauts, client.Entity.UID.ToString(), Talent.ToString(),
                OnJiangMode ? "1" : "2");
            if (OnJiangMode)
                RemoveJiangMod = DateTime.Now;
            client.SendScreen(client.Entity.SpawnPacket, false);

        }

        public void CreateTime()
        {
            StartCountDwon = DateTime.Now;
            CountDownEnd = DateTime.Now.AddMinutes(Database.JiangHu.GetMinutesOnTalent(Talent));
        }
        public void TheadTime(Client.GameState client)
        {
            try
            {
                if (client == null)
                    return;
                if (client.Entity == null)
                    return;
                if (!client.Entity.FullyLoaded)
                    return;
                if (DateTime.Now > TimerStamp.AddMinutes(1))
                {

                    if (client.Entity.PKMode != Enums.PkMode.Jiang)
                    {
                        if (OnJiangMode)
                        {
                            if (DateTime.Now >= RemoveJiangMod.AddMinutes(1))
                            {
                                OnJiangMode = false;
                                SendStatusMode(client);
                            }
                        }
                    }
                    if (client.Entity.PKMode == Enums.PkMode.Jiang)
                    {
                        OnJiangMode = true;
                        RemoveJiangMod = DateTime.Now;
                    }

                    if (FreeCourse < 10000000 && FreeTimeTodeyUsed < 10)
                    {
                        if (InTwinCastle(client.Entity))
                        {
                            StartCountDwon = StartCountDwon.AddMinutes(Database.JiangHu.GetMinutesInCastle(Talent));
                            FreeCourse += Database.JiangHu.GetFreeCourseInCastle(Talent);
                        }
                        else
                        {
                            FreeCourse += Database.JiangHu.GetFreeCourse(Talent);
                            StartCountDwon = StartCountDwon.AddMinutes(1);
                        }
                        if (StartCountDwon > CountDownEnd)
                            GetReward(client);
                        SendInfo(client, Network.GamePackets.JiangHu.UpdateTime, FreeCourse.ToString(), Time.ToString());

                        if (FreeCourse % 10000 == 0)
                            GetReward(client);

                    }
                    else
                        FreeCourse = 10000000;
                    TimerStamp = DateTime.Now;

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public bool GetRate(byte val)
        {
            return (Random.Next() % 100) < val;
        }
        public void GetKill(Client.GameState attacker, JiangHu attacked)
        {
            if (attacked != null)
            {
                //if (attacked.FreeCourse <= FreeCourse && attacked.FreeCourse != 0)
                {
                    // uint getCource = attacked.FreeCourse % 10000;
                    // if (getCource != 0)
                    {
                        // uint damage = (uint)(getCource * 30 / 100);
                        // attacked.FreeCourse -= damage;
                        //uint oldCourse = FreeCourse;
                        //FreeCourse = (uint)Math.Min(1000000, FreeCourse + damage);
                        //if (Random.Next(1, 100) > 40)
                        {

                            if (GetRate(35))
                            {

                                //attacked.Talent = (byte)Math.Min(1, Talent - 1);
                                Talent = (byte)Math.Min(5, Talent + 1);
                                attacker.Entity.JiangTalent = Talent;
                                SendInfo(attacker, Network.GamePackets.JiangHu.UpdateTalent, attacker.Entity.UID.ToString(), Talent.ToString());
                            }
                        }
                        /*  if (!SameCourseStage(oldCourse, FreeCourse))
                          {
                              uint RemovePlus = FreeCourse % 10000;
                              FreeCourse = (uint)(RemovePlus - RemovePlus);
                              GetReward(attacker);
                          }*/
                    }
                }
            }
        }
        public bool SameCourseStage(uint first, uint last)
        {
            if (first > 100000)
                return first.ToString()[2] == last.ToString()[2];
            else
                return first.ToString()[1] == last.ToString()[1];
        }
        public void GetReward(Client.GameState client)
        {
            do FreeCourse++; while (FreeCourse % 1000 != 0);
            SendInfo(client, Network.GamePackets.JiangHu.UpdateTime, FreeCourse.ToString(), Time.ToString());
            CreateTime();
        }
        public bool InTwinCastle(Entity location)
        {
            ushort x = location.X;
            ushort y = location.Y;
            if (location.MapID != 1002)
                return false;

            if (x <= 357 && x >= 274 && y <= 277 && y >= 110)
                return true;
            if (x >= 335 && x <= 370 && y <= 235 && y >= 232)
                return true;
            if (x >= 281 && x <= 350 && y >= 268 && y <= 322)
                return true;
            if (x >= 247 && x <= 291 && y >= 200 && y <= 243)
                return true;

            return false;
        }
        public object sync = new object();
        public void SendInfo(Client.GameState client, byte mode = 1, params string[] data)
        {
            lock (sync)
            {
                try
                {
                    Network.GamePackets.JiangHu jinag = new Network.GamePackets.JiangHu();
                    jinag.Mode = mode;
                    jinag.Texts = new List<string>(data);
                    jinag.CreateArray();
                    jinag.Send(client);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
        public void SendStatus(Client.GameState client, Client.GameState Attacked)
        {
            try
            {
                var dictionary = Stagers.Where(p => p.Activate);
                Network.GamePackets.JiangHuStatus stat = new Network.GamePackets.JiangHuStatus((byte)dictionary.Count());
                stat.Name = CustomizedName;
                stat.FreeTimeTodey = 10;//FreeTimeTodey;
                stat.Talent = Talent;
                stat.Stage = Stage;
                stat.RoundBuyPoints = RoundBuyPoints;
                stat.FreeTimeTodeyUsed = (byte)FreeTimeTodeyUsed;
                stat.StudyPoints = Attacked.Entity.SubClasses.StudyPoints;
                if (client.Entity.UID != Attacked.Entity.UID)
                    stat.Timer = 13751297;
                else
                    stat.Timer = 15500800;
                stat.Apprend(dictionary.ToArray());
                client.Send(stat.ToArray());
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public void UpdateStundyPoints(Client.GameState client, ushort amount)
        {
            client.Entity.SubClasses.StudyPoints += amount;
            Network.GamePackets.SubClassShow cls = new Network.GamePackets.SubClassShow();
            {
                cls.ID = 8;
                cls.Study = client.Entity.SubClasses.StudyPoints;
                cls.StudyReceive = amount;
                client.Send(cls.ToArray());
            }
            Network.GamePackets._String str = new Network.GamePackets._String(true);//(client.Player.UID, new string[] { "zf2-e300" });
            str.Type = Network.GamePackets._String.Effect;
            str.UID = client.Entity.UID;
            str.Texts.Add("zf2-e300");
            client.SendScreen(str.ToArray(), true);
        }
        public static bool AllowNameCaracters(string Name)
        {
            if (Name.Contains('['))
                return false;
            if (Name.Contains(']'))
                return false;
            if (Name.Contains("GM"))
                return false;
            if (Name.Contains("PM"))
                return false;
            if (Name.Contains("!") || Name.Contains("@") || Name.Contains("#"))
                return false;
            if (Name.Contains("$") || Name.Contains("%") || Name.Contains("^"))
                return false;
            if (Name.Contains("&") || Name.Contains("*") || Name.Contains("/") || Name.Contains("|"))
                return false;
            return true;
        }
    }
}
