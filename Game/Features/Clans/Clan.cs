using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using MTA.Client;
using MTA.Database;
using MTA.Interfaces;
using MTA.Network;
using MTA.Network.GamePackets;
using MTA.Game;

namespace MTA
{
    public class Clan : Writer, IPacket
    {
        private Byte[] mData;
        private Int32 Position = 17;
        private UInt32 mLeader, mFund, mAllyRequest;
        private String mName, mInfo, mAnnouncement;
        private Byte mLevel, mBPTower;
        public Client.GameState client;
        public UInt32 WarScore = 0;
        public bool PoleKeeper = false;

        private Dictionary<UInt32, ClanMember> mMembers;
        private Dictionary<UInt32, Clan> mAllies, mEnemies;

        public Clan(UInt32 leaderid, UInt32 clanid, String clanname, String leadername)
        {
            LeaderId = leaderid;

            mMembers = new Dictionary<UInt32, ClanMember>();
            mAllies = new Dictionary<UInt32, Clan>();
            mEnemies = new Dictionary<UInt32, Clan>();


            mData = new byte[141 + (Byte)(clanname.Length + leadername.Length) + 8];
            WriteUInt16((UInt16)(mData.Length - 8), 0, mData);
            WriteUInt16((UInt16)1312, 2, mData);

            ID = clanid;
            Name = clanname;
        }
        public Clan()
        {
            mData = new byte[141 + 8];
            WriteUInt16((UInt16)(mData.Length - 8), 0, mData);
            WriteUInt16((UInt16)1312, 2, mData);
        }
        public UInt32 LeaderId
        {
            get { return mLeader; }
            set { mLeader = value; }
        }
        public Types Type
        {
            get { return (Types)BitConverter.ToUInt32(mData, 4); }
            set { WriteByte((Byte)value, 4, mData); }
        }
        public UInt32 ID
        {
            get { return BitConverter.ToUInt32(mData, 8); }
            set { WriteUInt32((UInt32)value, 8, mData); }
        }
        public Byte Offset16
        {
            get { return mData[16]; }
            set { mData[16] = value; }
        }
        public Byte Offset17
        {
            get { return mData[17]; }
            set { mData[17] = value; }
        }
        public String Offset18String
        {
            get { return Program.Encoding.GetString(mData, 18, mData[17]).Trim(new Char[] { '\0' }); }
            set { WriteString(value, 18, mData); }
        }
        public String Name
        {
            get { return mName; }
            set { mName = value; }
        }

        private string leaderName;
        public bool CalnWar;
        public string LeaderName
        {
            get
            {
                return leaderName;
            }
            set
            {
                leaderName = value;
                Writer.WriteString(value, 32, mData);
            }
        }
        public UInt32 Fund
        {
            get { return mFund; }
            set { mFund = value; }
        }
        public Byte Level
        {
            get { return mLevel; }
            set { mLevel = value; }
        }
        public Byte BPTower
        {
            get { return mBPTower; }
            set { mBPTower = value; }
        }
        public String Announcement
        {
            get { return mAnnouncement; }
            set { mAnnouncement = value; }
        }
        public String Info
        {
            get { return mInfo; }
            set { mInfo = value; }
        }
        public UInt32 AllyRequest
        {
            get { return mAllyRequest; }
            set { mAllyRequest = value; }
        }

        public void Build(GameState c, Types type)
        {
            this.Type = type;
            switch (type)
            {
                case Types.Info:
                    {
                        ClanMember member;
                        if (Members.TryGetValue(c.Entity.UID, out member))
                        {
                            Info = System.String.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}"
                                , ID, mMembers.Count(), 0, Fund, Level - 1, Convert.ToByte(c.Entity.ClanRank), 0, BPTower, 0, 0, 0, member.Donation);

                            Offset16 = 6;

                            WriteStringWithLength(Info, Position, mData);
                            Position += Info.Length;
                            Position++;

                            WriteStringWithLength(Name, Position, mData);
                            Position += Name.Length;
                            Position++;

                            WriteStringWithLength(LeaderName, Position, mData);
                            Position += LeaderName.Length;
                            Position++;

                            string text2 = "0 0 0 0 0 0 0";
                            Writer.WriteStringWithLength(text2, Position, mData);
                            ClanWarArena.ClientWar clientWar;
                            if (ClanWarArena.GetMyWar(this.ID, out clientWar))
                            {
                                string dominationMap = clientWar.DominationMap;
                                Position += text2.Length;
                                Position++;
                                Writer.WriteStringWithLength(dominationMap, Position, mData);
                                string curentMap = clientWar.CurentMap;
                                Position += dominationMap.Length;
                                Position++;
                                Writer.WriteStringWithLength(curentMap, Position, mData);
                            }
                            Position = 17;
                        }
                        break;
                    }
                case Types.MyClan:
                    {
                        string text = "0 0 0 0 0 0 0";
                        ClanWarArena.ClientWar clientWar;
                        if (ClanWarArena.GetMyWar(this.ID, out clientWar))
                        {
                            text = string.Concat(new object[]{"1 ",
                        clientWar.OccupationDays," ",
                        clientWar.Reward," ",
                        clientWar.NextReward," 0 0 0"});
                        }
                        this.Offset16 = 1;
                        this.Offset17 = (byte)text.Length;
                        Writer.WriteString(text, 18, this.mData);
                        break;
                    }
            }
        }
        private static void UpdateData(Client.GameState client, string column, object value)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update("clans").Set(column, value).Where("identifier", client.Entity.ClanId)
                    .Execute();
        }
        public void Save(Client.GameState client, String row, UInt16 value)
        {
            UpdateData(client, row, value);
        }
        public void Save(Client.GameState client, String row, Byte value)
        {
            UpdateData(client, row, value);
        }
        public void Save(Client.GameState client, String row, String value)
        {
            UpdateData(client, row, value);
        }
        public void Save(Client.GameState client, String row, UInt32 value)
        {
            UpdateData(client, row, value);
        }
        public UInt32 GetClanId(String name)
        {
            lock (Kernel.Clans)
            {
                foreach (Clan clans in Kernel.Clans.Values)
                {
                    if (clans.Name == name)
                        return clans.ID;
                }
            }
            return 0;
        }
        public Dictionary<UInt32, ClanMember> Members { get { return this.mMembers; } }
        public Dictionary<UInt32, Clan> Allies { get { return this.mAllies; } }
        public Dictionary<UInt32, Clan> Enemies { get { return this.mEnemies; } }

        public void InfoToMembers()
        {
            GameState mem;
            foreach (ClanMember member in this.Members.Values)
            {
                if (Kernel.GamePool.TryGetValue(member.Identifier, out mem))
                {
                    mem.Entity.GetClan.Build(mem, Types.Info);
                    mem.Send(mem.Entity.GetClan.ToArray());
                    mem.Send(new ClanMembers(mem.Entity.GetClan).ToArray());

                }
            }
        }
        public void SendMessage(IPacket packet)
        {
            GameState mem;
            foreach (ClanMember member in this.Members.Values)
            {
                if (Kernel.GamePool.TryGetValue(member.Identifier, out mem))
                    mem.Send(packet);
            }
        }
        public static void nobmas(Client.GameState client)
        {
            Kernel.SendWorldMessage(new Message("Congratulation! " + client.Entity.Name + "Donation To " + client.NobilityInformation.Rank + " in Nobility Rank!", System.Drawing.Color.White, 2011), Program.Values);
        }
        public static UInt32 NextClanId
        {
            get
            {
                UInt32 start = 600;
                while (Kernel.Clans.ContainsKey(start))
                    start++;
                return start;
            }
        }
        public static Boolean ValidName(String name)
        {
            if (name.Length < 1 || name.Length > 35) return false;
            foreach (Clan clans in Kernel.Clans.Values)
                if (clans.Name == name)
                    return false;
            return true;
        }
        public void AddRelation(UInt32 Relative, ClanRelations.RelationTypes type)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("clanrelation"))
                cmd.Insert("id", ID).Insert("clanid", ID).Insert("associatedid", Relative).Insert("type", (byte)type).Execute();
        }
        public void DeleteRelation(UInt32 Relative, ClanRelations.RelationTypes type)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.DELETE))
                cmd.Delete("clanrelation", "clanid", ID).And("associatedid", Relative)
                    .And("type", (byte)type).Execute();
        }
        public void Join(GameState c)
        {
            c.Entity.ClanId = ID;
            c.Entity.ClanRank = Ranks.Member;
            c.Entity.ClanName = Name;
            Database.EntityTable.UpdateClanID(c);
            Database.EntityTable.UpdateClanRank(c);

            Members.Add(c.Entity.UID, new ClanMember()
            {
                Class = c.Entity.Class,
                Donation = 0,
                Identifier = c.Entity.UID,
                Level = c.Entity.Level,
                Name = c.Entity.Name,
                Rank = c.Entity.ClanRank
            });

            Build(c, Types.Info);
            c.Send(this);

            c.Entity.Teleport(c.Entity.MapID, c.Entity.X, c.Entity.Y);
            /*
                        if (c.Team != null)
                            c.Team.GetClanShareBp(c);
                        */
            SendMessage(new Message(System.String.Format("{0} Has Joined the Clan!", c.Entity.Name), Color.Red, Message.Clan));
        }
        public static void CreateClan(GameState c, String cname)
        {
            UInt32 id = NextClanId;
            Clan clan = new Clan(c.Entity.UID, id, cname, c.Entity.Name);
            clan.Fund = 250000;
            clan.ID = id;
            clan.BPTower = 4;
            clan.Level = 4;
            clan.Name = cname;
            clan.LeaderName = c.Entity.Name;

            clan.Members.Add(c.Entity.UID, new ClanMember()
            {
                Class = c.Entity.Class,
                Donation = 250000,
                Identifier = c.Entity.UID,
                Level = c.Entity.Level,
                Name = c.Entity.Name,
                Rank = Ranks.ClanLeader
            });

            Kernel.Clans.Add(id, clan);

            Kernel.SendWorldMessage(new Message(System.String.Format("{0} has succesfully set up a new Clan {1}", c.Entity.Name, cname), Color.Red, Message.TopLeft));

            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT))
                cmd.Insert("clans").Insert("identifier", id)
                    .Insert("leaderid", clan.LeaderId).Insert("name", clan.Name)
                    .Insert("fund", clan.Fund).Insert("announcement", "")
                    .Insert("BPTower", clan.BPTower).Insert("Level", clan.Level)
                    .Insert("LeaderName", c.Entity.Name).Insert("polekeeper", 0).Execute();

            c.Entity.ClanId = id;
            c.Entity.ClanRank = Ranks.ClanLeader;
            Database.EntityTable.UpdateClanID(c);
            Database.EntityTable.UpdateClanRank(c);

            clan.Build(c, Types.Info);
            c.Send(clan);

            clan = c.Entity.GetClan;
            if (clan != null)
            {
                clan.Build(c, Clan.Types.Info);
                c.Send(clan);

                c.Entity.ClanName = clan.Name;

                c.Send(new ClanRelations(clan, ClanRelations.RelationTypes.Allies));
                c.Send(new ClanRelations(clan, ClanRelations.RelationTypes.Enemies));
            }
            c.Screen.FullWipe();
            c.Screen.Reload(null);
        }
        public static void LoadClans()
        {
            Dictionary<uint, Dictionary<uint, ClanMember>> dict = new Dictionary<uint, Dictionary<uint, ClanMember>>();

            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("entities").Where("clanid", 0, true))
            using (var reader = cmd.CreateReader())
            {
                while (reader.Read())
                {
                    uint clanId = reader.ReadUInt32("clanid");
                    ClanMember mem = new ClanMember()
                    {
                        UID = reader.ReadUInt32("Uid"),
                        Identifier = reader.ReadUInt32("Uid"),
                        Class = reader.ReadByte("Class"),
                        Donation = reader.ReadUInt32("ClanDonation"),
                        Level = reader.ReadByte("Level"),
                        Name = reader.ReadString("Name"),
                        Rank = (Ranks)reader.ReadByte("ClanRank"),
                    };
                    if (!dict.ContainsKey(clanId)) dict.Add(clanId, new Dictionary<uint, ClanMember>());
                    dict[clanId].Add(mem.UID, mem);
                }
            }
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("clans"))
            using (var reader = cmd.CreateReader())
            {
                while (reader.Read())
                {
                    UInt32 HeroId = reader.ReadUInt32("LeaderId");
                    Clan c = new Clan(HeroId, reader.ReadUInt32("Identifier"), reader.ReadString("Name"), reader.ReadString("LeaderName"));
                    {
                        c.Announcement = reader.ReadString("Announcement");
                        c.BPTower = reader.ReadByte("BPTower");
                        c.ID = reader.ReadUInt32("Identifier");
                        c.Fund = reader.ReadUInt32("Fund");
                        c.LeaderId = reader.ReadUInt32("LeaderId");
                        c.Level = reader.ReadByte("Level");
                        c.LeaderName = reader.ReadString("LeaderName");
                        c.PoleKeeper = reader.ReadBoolean("PoleKeeper");
                    }
                    Kernel.Clans.Add(c.ID, c);
                    if (dict.ContainsKey(c.ID))
                        c.mMembers = dict[c.ID];
                    else
                        c.mMembers = new Dictionary<uint, ClanMember>();
                }
            }
            foreach (Clan c in Kernel.Clans.Values)
            {
                c.LoadAssociates();
            }
        }
        public static void DisbandClan(GameState c)
        {
            byte[] Packet = new byte[90];
            Writer.WriteUInt16(82, 0, Packet);
            Writer.WriteUInt16(1312, 2, Packet);
            Writer.WriteUInt32(23, 4, Packet);
            Writer.WriteUInt32(c.Entity.UID, 8, Packet);
            /*
            if (c.Team != null)
                c.Team.GetClanShareBp(c);
            */
            Database.EntityTable.RemoveClan(c);
            using (var cmd = new MySqlCommand(MySqlCommandType.DELETE))
                cmd.Delete("clans", "leaderid", c.Entity.UID).Execute();

            foreach (var h in c.Entity.GetClan.Members.Values)
            {
                var hero = Program.Values.SingleOrDefault(x => x.Entity.UID == h.Identifier);
                if (hero != null)
                {
                    hero.Entity.ClanRank = Clan.Ranks.None;
                    hero.Entity.ClanName = "";
                    hero.Entity.ClanId = 0;
                    hero.Send(Packet);
                    hero.SendScreenSpawn(hero.Entity, true);
                }
            }
            Kernel.Clans.Remove(c.Entity.ClanId);
        }
        public static void SaveClan(Clan clan)
        {
            MySqlCommand cmd = new MySqlCommand(MySqlCommandType.UPDATE);
            cmd.Update("Clans").Set("Fund", clan.Fund).Set("Level", clan.Level)
                .Set("Bulletin", clan.Announcement).Set("Leader", clan.leaderName).Where("ClanID", clan.ID).Execute();
        }
        public static void TransferClan(string name)
        {
            MySqlCommand cmd3 = new MySqlCommand(MySqlCommandType.UPDATE);
            cmd3.Update("entities")
                .Set("ClanRank", 100).Where("Name", name).Execute();
        }
        private void LoadAssociates()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("clanrelation").Where("clanid", this.ID))
            using (var reader = cmd.CreateReader())
            {
                while (reader.Read())
                {
                    UInt32 AssociateId = reader.ReadUInt32("AssociatedId");
                    ClanRelations.RelationTypes Type = (ClanRelations.RelationTypes)(reader.ReadUInt32("Type"));
                    Clan c;
                    if (Kernel.Clans.TryGetValue(AssociateId, out c))
                    {
                        if (Type == ClanRelations.RelationTypes.Allies)
                            this.Allies.Add(AssociateId, c);
                        else
                            this.Enemies.Add(AssociateId, c);
                    }
                }
            }
        }
        public enum Types : byte
        {
            Info = 1,
            Members = 4,
            Recruit = 9,
            AcceptRecruit = 10,
            Join = 11,
            AcceptJoinRequest = 12,
            AddEnemy = 14,
            DeleteEnemy = 15,
            AddAlly = 17,
            AcceptAlliance = 18,
            DeleteAlly = 20,
            TransferLeader = 21,
            Kick = 22,
            Quit = 23,
            Announce = 24,
            SetAnnouncement = 25,
            Dedicate = 26,
            MyClan = 29
        }
        public enum Ranks : ushort
        {
            ClanLeader = 100,
            Spouse = 11,
            Member = 10,
            None = 0
        }
        public void Send(GameState client) { client.Send(mData); }
        public byte[] ToArray() { return mData; }
        public void Deserialize(byte[] buffer) { mData = buffer; }

        public void SendClanShareBp(uint leaderUID, uint BpShare, Client.GameState client)
        {
            Network.GamePackets.Update update = new Network.GamePackets.Update(true) { UID = client.Entity.UID };
            update.Append(Network.GamePackets.Update.ClanShareBp, leaderUID);
            update.Append4(Network.GamePackets.Update.ClanShareBp, BpShare);
            client.Send(update.ToArray());

        }
    }
}
