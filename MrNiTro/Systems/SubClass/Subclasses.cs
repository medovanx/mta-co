using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Game
{
    public enum ClassID : byte
    {
        MartialArtist = 1,
        Warlock = 2,
        ChiMaster = 3,
        Sage = 4,
        Apothecary = 5,
        Performer = 6,
        Wrangler = 9
    }
    public class Subclasses
    {

        public void Activate(Client.GameState client, byte clas)
        {
            Active = clas;
            client.Entity.SubClass = clas;
        }
        public uint GetHashPoint()
        {
            uint val = 0;
            if (Classes.ContainsKey((byte)ClassID.MartialArtist))
                val += Classes[(byte)ClassID.MartialArtist].Phase;
            if (Classes.ContainsKey((byte)ClassID.Warlock))
                val += (uint)(Classes[(byte)ClassID.Warlock].Phase * 10);
            if (Classes.ContainsKey((byte)ClassID.ChiMaster))
                val += (uint)(Classes[(byte)ClassID.ChiMaster].Phase * 100);
            if (Classes.ContainsKey((byte)ClassID.Sage))
                val += (uint)(Classes[(byte)ClassID.Sage].Phase * 1000);
            if (Classes.ContainsKey((byte)ClassID.Apothecary))
                val += (uint)(Classes[(byte)ClassID.Apothecary].Phase * 10000);
            if (Classes.ContainsKey((byte)ClassID.Performer))
                val += (uint)(Classes[(byte)ClassID.Performer].Phase * 100000);
            if (Classes.ContainsKey((byte)ClassID.Wrangler))
                val += (uint)(Classes[(byte)ClassID.Wrangler].Phase * 100000000);
            return val;
        }

        public Dictionary<byte, SubClass> Classes;
        ushort study;
        public ushort StudyPoints
        {
            get { return study; }
            set
            {
                if (value > 62000)
                    study = 62000;
                else
                    study = value;
            }
        }
        public byte Active;
        public Subclasses()
        {
            Classes = new Dictionary<byte, SubClass>();
            StudyPoints = 0;
            Active = 0;
        }
        public override string ToString()
        {
            StringBuilder build = new StringBuilder();
            foreach (var clases in Classes.Values)
                build.Append(clases.ToString() + "/");
            return build.ToString();
        }
        public ushort Critical = 0;
        public ushort SCritical = 0;
        public ushort Imunity = 0;
        public ushort Penetration = 0;
        public ushort Detoxication = 0;
        public ushort PhsicalAttack = 0;
        public ushort MagiAttack = 0;
        public ushort HitPoints = 0;
        public void UpgradeStatus(Client.GameState client, bool removeold)
        {
            if (removeold)
                RemoveStatus(client);
            ClearStatus();
            foreach (var info in Classes.Values)
            {
                switch ((ClassID)info.ID)
                {
                    case ClassID.Wrangler:
                        HitPoints = GetDamage(info);
                        break;
                    case ClassID.Warlock:
                        SCritical = (ushort)(GetDamage(info) * 100);
                        break;
                    case ClassID.MartialArtist:
                        Critical = (ushort)(GetDamage(info) * 100);
                        break;
                    case ClassID.ChiMaster:
                        Imunity = (ushort)(GetDamage(info) * 100);
                        break;
                    case ClassID.Sage:
                        Penetration = (ushort)(GetDamage(info) * 100);
                        break;
                    case ClassID.Apothecary:
                        Detoxication = GetDamage(info);
                        break;
                    case ClassID.Performer:
                        MagiAttack = PhsicalAttack = GetDamage(info);
                        break;
                }
            }
            AddStatus(client);
        }



        public void ClearStatus()
        {
            HitPoints = Critical = SCritical = Imunity = Penetration = Detoxication = PhsicalAttack = MagiAttack = 0;
        }
        public void RemoveStatus(Client.GameState client)
        {
            client.Entity.ItemHP -= HitPoints;
            // client.Entity.MaxHitpoints -= HitPoints;
            client.Entity.CriticalStrike -= Critical;
            client.Entity.SkillCStrike -= SCritical;
            client.Entity.Immunity -= Imunity;
            client.Entity.Penetration -= Penetration;
            client.Entity.Detoxication -= Detoxication;
            client.Entity.PhysicalDamageIncrease -= PhsicalAttack;
            client.Entity.MagicDamageIncrease -= MagiAttack;
        }
        public void AddStatus(Client.GameState client)
        {
            client.Entity.ItemHP += HitPoints;
            client.Entity.CriticalStrike += Critical;
            client.Entity.SkillCStrike += SCritical;
            client.Entity.Immunity += Imunity;
            client.Entity.Penetration += Penetration;
            client.Entity.Detoxication += Detoxication;
            client.Entity.BaseMinAttack += PhsicalAttack;
            client.Entity.BaseMaxAttack += PhsicalAttack;
            client.Entity.BaseMagicAttack += MagiAttack;
        }
        public void resend(Client.GameState client)
        {
            foreach (SubClass current in this.Classes.Values)
            {
                SendLearn((ClassID)current.ID, current.Level, client);
            }
        }
        public void Send(Client.GameState client)
        {
            Network.GamePackets.SubClassShow inf = new Network.GamePackets.SubClassShow((ushort)Classes.Count);
            {
                inf.ID = Network.GamePackets.SubClassShow.ShowGUI;
                inf.Study = StudyPoints;
                foreach (var sub in Classes.Values)
                    inf.Apprend(sub.ID, sub.Phase, sub.Level);
                client.Send(inf.ToArray());
            }
        }
        public void SendLearn(ClassID ID, byte Level, Client.GameState client)
        {
            Network.GamePackets.SubClassShow inf = new Network.GamePackets.SubClassShow();
            {
                inf.ID = Network.GamePackets.SubClassShow.LearnSubClass;
                inf.Class = (byte)ID;
                inf.Level = Level;
                client.Send(inf.ToArray());
            }
        }
        public void SendPromoted(ClassID ID, byte Phase, Client.GameState client)
        {
            Network.GamePackets.SubClassShow inf = new Network.GamePackets.SubClassShow();
            {
                inf.ID = Network.GamePackets.SubClassShow.MartialPromoted;
                inf.Class = (byte)ID;
                inf.Level = Phase;
                client.Send(inf.ToArray());
            }
        }
        public static ushort GetDamage(SubClass Sc)
        {
            ushort Required = 0;

            switch ((ClassID)Sc.ID)
            {
                case ClassID.Sage:
                case ClassID.ChiMaster:
                case ClassID.Warlock:
                case ClassID.MartialArtist:
                    switch (Sc.Level)
                    {
                        case 1: Required = 1; break;
                        case 2: Required = 2; break;
                        case 3: Required = 3; break;
                        case 4: Required = 4; break;
                        case 5: Required = 6; break;
                        case 6: Required = 8; break;
                        case 7: Required = 10; break;
                        case 8: Required = 12; break;
                        case 9: Required = 15; break;
                    }
                    break;
                case ClassID.Apothecary:
                    switch (Sc.Level)
                    {
                        case 1: Required = 8; break;
                        case 2: Required = 16; break;
                        case 3: Required = 24; break;
                        case 4: Required = 32; break;
                        case 5: Required = 40; break;
                        case 6: Required = 48; break;
                        case 7: Required = 56; break;
                        case 8: Required = 64; break;
                        case 9: Required = 72; break;
                    }
                    break;
                case ClassID.Wrangler:
                    switch (Sc.Level)
                    {
                        case 1: Required = 100; break;
                        case 2: Required = 200; break;
                        case 3: Required = 300; break;
                        case 4: Required = 400; break;
                        case 5: Required = 500; break;
                        case 6: Required = 600; break;
                        case 7: Required = 800; break;
                        case 8: Required = 1000; break;
                        case 9: Required = 1200; break;
                    }
                    break;
                case ClassID.Performer:
                    switch (Sc.Level)
                    {
                        case 1: Required = 100; break;
                        case 2: Required = 200; break;
                        case 3: Required = 300; break;
                        case 4: Required = 400; break;
                        case 5: Required = 500; break;
                        case 6: Required = 600; break;
                        case 7: Required = 700; break;
                        case 8: Required = 800; break;
                        case 9: Required = 1000; break;
                    }
                    break;
            }

            return Required;
        }

        public static ushort GetRequired(SubClass Sc)
        {
            ushort Required = 0;
            switch ((ClassID)Sc.ID)
            {
                case ClassID.Warlock:
                case ClassID.MartialArtist:
                    switch (Sc.Level)
                    {
                        case 1: Required = 300; break;
                        case 2: Required = 900; break;
                        case 3: Required = 1800; break;
                        case 4: Required = 2700; break;
                        case 5: Required = 3600; break;
                        case 6: Required = 5100; break;
                        case 7: Required = 6900; break;
                        case 8: Required = 8700; break;
                        case 9: Required = ushort.MaxValue; break;
                    }
                    break;
                case ClassID.ChiMaster:
                    switch (Sc.Level)
                    {
                        case 1: Required = 600; break;
                        case 2: Required = 1800; break;
                        case 3: Required = 3600; break;
                        case 4: Required = 5400; break;
                        case 5: Required = 7200; break;
                        case 6: Required = 10200; break;
                        case 7: Required = 13800; break;
                        case 8: Required = 17400; break;
                        case 9: Required = ushort.MaxValue; break;
                    }
                    break;
                case ClassID.Apothecary:
                    switch (Sc.Level)
                    {
                        case 1: Required = 100; break;
                        case 2: Required = 200; break;
                        case 3: Required = 300; break;
                        case 4: Required = 400; break;
                        case 5: Required = 500; break;
                        case 6: Required = 1000; break;
                        case 7: Required = 4000; break;
                        case 8: Required = 9000; break;
                        case 9: Required = ushort.MaxValue; break;
                    }
                    break;
                case ClassID.Sage:
                case ClassID.Wrangler:
                case ClassID.Performer:
                    switch (Sc.Level)
                    {
                        case 1: Required = 400; break;
                        case 2: Required = 1200; break;
                        case 3: Required = 2400; break;
                        case 4: Required = 3600; break;
                        case 5: Required = 4800; break;
                        case 6: Required = 6800; break;
                        case 7: Required = 9200; break;
                        case 8: Required = 11600; break;
                        case 9: Required = ushort.MaxValue; break;
                    }
                    break;
            }
            return Required;
        }
    }
}
