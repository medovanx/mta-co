//Project by BaussHacker aka. L33TS

using System;
using Conquer_Online_Server;
using Conquer_Online_Server.Client;
using Conquer_Online_Server.Interfaces;
using System.Collections.Concurrent;
using Conquer_Online_Server.Game;
using Conquer_Online_Server.Network.GamePackets;
using Conquer_Online_Server.Database;

using ProjectX_V3_Game.Threads;
using Conquer_Online_Server.Network;
using Albetros.Core;
using Conquer_Online_Server.Game.ConquerStructures;
using System.Collections.Generic;

namespace ProjectX_V3_Game.Entities
{
    public enum BotType
    {
        EventBot = 0,
        DuelBot = 1,
        TournamentBot = 2,
        BoothBot = 4,
        AFKBot = 3
    }
    public enum BotLevel
    {
        Noob = 0,
        Easy = 1,
        Normal = 2,
        Medium = 3,
        Hard = 4,
        Insane = 5,
        MaTrix = 6
    }
    public enum BotDelayedActions
    {
        FightStart,
        RunConditionChecks,
    }
    public enum DelayedActionType : ushort
    {
        ToGhost = 0,
        UpStam,
        UpXP,
        PotionHealth,
        PotionMana,
        XpTimeout,
        ContinueLogin,
        ArenaJoinTimeout,
        UpdateQualifierScores,
        ArenaMatchTimeout,
        UpdateGWScores,
        GwRoundStart,
        CTFStart,
        ReturnFlag,
        DoNobilityRank,
        ResendNobilityIcon,
        MobSequence,
        ThreadCheck,
        PediodicSave,
        AutoRevive,
        LowerPKPt,
        Summon,
    }
    public class ArenaMatchPacket : Writer
    {
        private byte[] _buffer;
        public ArenaMatchPacket()
        {
            _buffer = new byte[0x38 + 8];
            WriteUshort(0x38, 0, _buffer);
            WriteUshort(0x8a2, 2, _buffer);
        }

        public uint Player1EntityUID { get { return Conquer_Online_Server.BitConverter.ReadUint(_buffer, 4); } set { WriteUint(value, 4, _buffer); } }
        public string Player1Name
        {
            get { return System.BitConverter.ToString(_buffer, 8, 16); }
            set { WriteStringWithLength(value, 8, _buffer); }
        }
        public uint Player1Damage { get { return Conquer_Online_Server.BitConverter.ReadUint(_buffer, 24); } set { WriteUint(value, 24, _buffer); } }

        public uint Player2EntityUID { get { return Conquer_Online_Server.BitConverter.ReadUint(_buffer, 28); } set { WriteUint(value, 28, _buffer); } }
        public string Player2Name
        {
            get { return System.BitConverter.ToString(_buffer, 32, 16); }
            set { WriteStringWithLength(value, 32, _buffer); }
        }
        public uint Player2Damage { get { return Conquer_Online_Server.BitConverter.ReadUint(_buffer, 48); } set { WriteUint(value, 48, _buffer); } }

        public void Send(GameState client)
        {
            client.Send(this.ToArray());
        }

        public void Deserialize(byte[] Data)
        {
            _buffer = Data;
        }

        public byte[] ToArray()
        {
            return _buffer;
        }

    }

    /// <summary>
    /// Description of AIBot.
    /// </summary>     
    /// 
    public class AIBot
    {

        public GenericActionList<BotDelayedActions> BotActions = new GenericActionList<BotDelayedActions>();
        public GenericActionList<DelayedActionType> EntityActions = new GenericActionList<DelayedActionType>();

        private int JumpSpeed = 0;
        private int AttackSpeed = 0;
        private int ShootChance = 0;
        private int Accuracy = 0;

        public void SetLevel(BotLevel Level)
        {
            switch (Level)
            {
                case BotLevel.Noob:
                    JumpSpeed = 3000;
                    ShootChance = 10;
                    Accuracy = 5;
                    break;

                case BotLevel.Easy:
                    JumpSpeed = 1500;
                    ShootChance = 25;
                    Accuracy = 10;
                    break;

                case BotLevel.Normal:
                    JumpSpeed = 1250;
                    ShootChance = 33;
                    Accuracy = 20;
                    break;

                case BotLevel.Medium:
                    JumpSpeed = 1000;
                    ShootChance = 50;
                    Accuracy = 33;
                    break;

                case BotLevel.Hard:
                    JumpSpeed = 1000;
                    ShootChance = 75;
                    Accuracy = 50;
                    break;

                case BotLevel.Insane:
                    JumpSpeed = 1000;
                    ShootChance = 90;
                    Accuracy = 80;
                    break;
                case BotLevel.MaTrix:
                    JumpSpeed = 1000;
                    ShootChance = 100;
                    Accuracy = 100;
                    break;
            }
        }
        private GameState Bot;
        private Entity Target;
        // private Map dynamicMap;
        public uint Count_AISkills = 0;
        public Dictionary<uint, skills> AISkills = new Dictionary<uint, skills>();
        public class skills
        {
            public ushort ID;
            public byte Level;
        }

        public AIBot(BotLevel diff)
        {
            Bot = new GameState(null);
            SetLevel(diff);
        }

        private ushort MaxX = 0, MaxY = 0, MinX = 0, MinY = 0;
        public void SetMinMaxLoc(ushort maxX, ushort maxY, ushort minX, ushort minY)
        {
            MaxX = maxX;
            MaxY = maxY;
            MinX = minX;
            MinY = minY;
        }

        public void LoadBot(BotType BotType, GameState client)
        {
            try
            {
                switch (BotType)
                {
                    #region afk bot
                    case BotType.AFKBot:
                        {
                            Bot.Entity = new Entity(EntityFlag.Player, false);
                            Bot.Entity.Owner = Bot;
                            Bot.Entity.MapObjType = MapObjectType.Player;
                            Bot.Entity.Name = client.Entity.Name + "[BoT]";
                            Bot.Entity.Face = client.Entity.Face;
                            Bot.Entity.Body = client.Entity.Body;
                            Bot.Entity.HairStyle = client.Entity.HairStyle;
                            Bot.Entity.Strength = client.Entity.Strength;
                            Bot.Entity.Agility = client.Entity.Agility;
                            Bot.Entity.Vitality = client.Entity.Vitality;
                            Bot.Entity.Spirit = client.Entity.Spirit;
                            Bot.Entity.PKPoints = client.Entity.PKPoints;
                            Bot.Entity.Level = client.Entity.Level;
                            Bot.Entity.Class = client.Entity.Class;
                            Bot.Entity.MyTitle = client.Entity.MyTitle;
                            Bot.Entity.Reborn = client.Entity.Reborn;

                            Bot.Entity.MapID = client.Entity.MapID;
                            //   Bot.Entity.LastMapID = client.Entity.MapID;
                            Bot.Entity.X = client.Entity.X;
                            Bot.Entity.Y = client.Entity.Y;
                            //    Bot.Entity.LastMapX = client.Entity.X;
                            //    Bot.Entity.LastMapY = client.Entity.Y;
                            //    Bot.Entity.LastX = client.Entity.X;
                            //    Bot.Entity.LastY = client.Entity.Y;

                            Bot.Entity.Action = Enums.ConquerAction.Sit;
                            Bot.Entity.Facing = (Enums.ConquerAngle)Kernel.Random.Next(8);
                           
                            uint entityuid = (uint)Kernel.Random.Next(700000000, 999999999);
                            Bot.Entity.UID = entityuid;
                            Bot.Entity.UpdateEffects(true);
                            client.Map.AddAI(Bot.Entity);
                            client.SendScreenSpawn(Bot.Entity, true);
                            //       Bot.Entity.SendSpawn(client, true);       

                            //      uint WeaponR = sql.ReadUInt32("BotWeaponR");
                            //      if (WeaponR > 0)
                            //      {
                            //          Data.ItemInfo item = Core.Kernel.ItemInfos[WeaponR].Copy();
                            //      Original.Equipments.Equip(item, Enums.ItemLocation.WeaponR, false, false);
                            //    }
                            //     uint WeaponL = sql.ReadUInt32("BotWeaponL");
                            //     if (WeaponL > 0)
                            //      {
                            //          Data.ItemInfo item = Core.Kernel.ItemInfos[WeaponL].Copy();
                            //         Original.Equipments.Equip(item, Enums.ItemLocation.WeaponL, false, false);
                            //      }
                            //      uint Armor = sql.ReadUInt32("BotArmor");
                            //      if (Armor > 0)
                            //      {
                            //           Data.ItemInfo item = Core.Kernel.ItemInfos[Armor].Copy();
                            //          Original.Equipments.Equip(item, Enums.ItemLocation.Armor, false, false);
                            //      }
                            //     uint Head = sql.ReadUInt32("BotHead");
                            //      if (Head > 0)
                            //     {
                            //        Data.ItemInfo item = Core.Kernel.ItemInfos[Head].Copy();
                            //       Original.Equipments.Equip(item, Enums.ItemLocation.Head, false, false);
                            //    }
                            //     bool UseSteed = sql.ReadBoolean("BotSteed");
                            //     if (UseSteed)
                            //    {
                            //        uint Steed = sql.ReadUInt32("BotSteedColor");
                            //       Data.ItemInfo item = Core.Kernel.ItemInfos[300000].Copy();
                            //       item.SocketAndRGB = Steed;
                            //      Original.Equipments.Equip(item, Enums.ItemLocation.Steed, false, false);

                            //       uint MountArmor = sql.ReadUInt32("BotMountArmor");
                            //       if (MountArmor > 0)
                            //      {
                            //         Data.ItemInfo item2 = Core.Kernel.ItemInfos[MountArmor].Copy();
                            //         Original.Equipments.Equip(item2, Enums.ItemLocation.SteedArmor, false, false);
                            //     }

                            //      Original.Action = Enums.ActionType.None;
                            //       Original.AddStatusEffect1(Enums.Effect1.Riding);
                            //   }
                            //    uint Garment = sql.ReadUInt32("BotGarment");
                            //     if (Garment > 0)
                            //     {
                            //         Data.ItemInfo item = Core.Kernel.ItemInfos[Garment].Copy();
                            //         Original.Equipments.Equip(item, Enums.ItemLocation.Garment, false, false);
                            //     }
                            Bot.LoggedIn = true;
                            break;
                        }
                    #endregion
                    #region duel bot
                    case BotType.DuelBot:
                        {
                            if (client == null)
                                return;
                            #region New Arena
                            /*   client.Entity.Ressurect2();
                        client.Entity.RemoveFlag(0x4000000000000L);
                        if (!Kernel.Maps.ContainsKey((int)702L))
                        {
                            new Map(702, DMaps.MapPaths[702]);
                        }
                        Map map = Kernel.Maps[(int)702];
                        this.dynamicMap = map.MakeDynamicMap();
                        client.Entity.Teleport(map.ID, this.dynamicMap.ID, 38, 38); 
                     //   client.Entity.Teleport(map.ID, this.dynamicMap.ID, (ushort)Kernel.Random.Next(40, 50), (ushort)Kernel.Random.Next(40, 50));                                                
                      //  client.Entity.Teleport(702, 50, (ushort)Kernel.Random.Next(50, 60));                                                                       
                        P2Mode = client.Entity.PKMode;
                        client.Entity.PKMode = Enums.PKMode.PK;
                        Conquer_Online_Server.Network.GamePackets.Data data2 = new Conquer_Online_Server.Network.GamePackets.Data(true)
                        {
                            UID = client.Entity.UID,
                            ID = 0x60,
                            dwParam = (uint)client.Entity.PKMode
                        };
                        client.Send(data2); */
                            #endregion New Arena

                            //    if (client.Entity.MapID == 702)
                            {
                                uint UID = client.Entity.UID + 70000000;
                                Bot.ReadyToPlay();
                                Bot.Account = new Conquer_Online_Server.Database.AccountTable(null);
                                Bot.Account.EntityID = UID;

                                Bot.Entity = new Entity(EntityFlag.Player, false);
                                Bot.Entity.Owner = Bot;
                                Bot.Entity.MapObjType = MapObjectType.Player;
                                Bot.Entity.Name = client.Entity.Name + "[BoT]";
                                if (Bot.Entity.Name.Length > 0x10)
                                {
                                    string Name = client.Entity.Name;
                                    Bot.Entity.Name = Name.Substring(0, 11) + "[BoT]";

                                }
                                Bot.Entity.Face = client.Entity.Face;
                                Bot.Entity.Body = client.Entity.Body;
                                Bot.Entity.HairStyle = client.Entity.HairStyle;
                                Bot.Entity.PKPoints = client.Entity.PKPoints;
                                Bot.Entity.Level = client.Entity.Level;
                                Bot.Entity.Class = client.Entity.Class;
                                Bot.Entity.MyTitle = client.Entity.MyTitle;
                                Bot.Entity.Reborn = client.Entity.Reborn;
                                Bot.Entity.Level = client.Entity.Level;
                                Bot.Entity.MapID = client.Entity.MapID;
                                Bot.Entity.X = (ushort)(client.Entity.X + Kernel.Random.Next(5));
                                Bot.Entity.Y = (ushort)(client.Entity.Y + Kernel.Random.Next(5));
                                Bot.Entity.MinAttack = client.Entity.MinAttack;
                                Bot.Entity.MaxAttack = client.Entity.MagicAttack;

                                Bot.Entity.Action = Enums.ConquerAction.None;
                                Bot.Entity.Facing = (Enums.ConquerAngle)Kernel.Random.Next(8);
                                Bot.IsAIBot = true;


                                Bot.Entity.UID = UID;
                                Bot.Entity.UpdateEffects(true);

                                Bot.Equipment.ForceEquipments(client.Equipment);
                                Bot.Entity.Stamina = 150;
                                Bot.Entity.MaxHitpoints = client.Entity.MaxHitpoints;                              
                                Bot.Entity.Hitpoints = Bot.Entity.MaxHitpoints;
                                Bot.Entity.Mana = Bot.Entity.MaxMana;
                                #region Arena
                                Bot.ElitePKStats = new ElitePK.FighterStats(Bot.Entity.UID, Bot.Entity.Name, Bot.Entity.Mesh);
                                if (!Conquer_Online_Server.Game.ConquerStructures.Nobility.Board.TryGetValue(Bot.Entity.UID, out Bot.NobilityInformation))
                                {
                                    Bot.NobilityInformation = new NobilityInformation();
                                    Bot.NobilityInformation.EntityUID = Bot.Entity.UID;
                                    Bot.NobilityInformation.Name = Bot.Entity.Name;
                                    Bot.NobilityInformation.Donation = 0L;
                                    Bot.NobilityInformation.Rank = NobilityRank.Serf;
                                    Bot.NobilityInformation.Position = -1;
                                    Bot.NobilityInformation.Gender = 1;
                                    Bot.NobilityInformation.Mesh = Bot.Entity.Mesh;
                                    if ((Bot.Entity.Body % 10) >= 3)
                                    {
                                        Bot.NobilityInformation.Gender = 0;
                                    }
                                }
                                else
                                {
                                    Bot.Entity.NobilityRank = Bot.NobilityInformation.Rank;
                                }
                                Arena.ArenaStatistics.TryGetValue(Bot.Entity.UID, out Bot.ArenaStatistic);
                                if ((Bot.ArenaStatistic == null) || (Bot.ArenaStatistic.EntityID == 0))
                                {
                                    Bot.ArenaStatistic = new ArenaStatistic(true);
                                    Bot.ArenaStatistic.EntityID = Bot.Entity.UID;
                                    Bot.ArenaStatistic.Name = Bot.Entity.Name;
                                    Bot.ArenaStatistic.Level = Bot.Entity.Level;
                                    Bot.ArenaStatistic.Class = Bot.Entity.Class;
                                    Bot.ArenaStatistic.Model = Bot.Entity.Mesh;
                                    Bot.ArenaStatistic.ArenaPoints = ArenaTable.ArenaPointFill(Bot.Entity.Level);
                                    Bot.ArenaStatistic.LastArenaPointFill = DateTime.Now;
                                    ArenaTable.InsertArenaStatistic(Bot);
                                    Bot.ArenaStatistic.Status = 0;
                                    Arena.ArenaStatistics.Add(Bot.Entity.UID, Bot.ArenaStatistic);
                                }
                                else
                                {
                                    Bot.ArenaStatistic.Level = Bot.Entity.Level;
                                    Bot.ArenaStatistic.Class = Bot.Entity.Class;
                                    Bot.ArenaStatistic.Model = Bot.Entity.Mesh;
                                    if (DateTime.Now.DayOfYear != Bot.ArenaStatistic.LastArenaPointFill.DayOfYear)
                                    {
                                        Bot.ArenaStatistic.LastSeasonArenaPoints = Bot.ArenaStatistic.ArenaPoints;
                                        Bot.ArenaStatistic.LastSeasonWin = Bot.ArenaStatistic.TodayWin;
                                        Bot.ArenaStatistic.LastSeasonLose = Bot.ArenaStatistic.TodayBattles - Bot.ArenaStatistic.TodayWin;
                                        Bot.ArenaStatistic.ArenaPoints = ArenaTable.ArenaPointFill(Bot.Entity.Level);
                                        Bot.ArenaStatistic.LastArenaPointFill = DateTime.Now;
                                        Bot.ArenaStatistic.TodayWin = 0;
                                        Bot.ArenaStatistic.TodayBattles = 0;
                                        Arena.Sort();
                                        Arena.YesterdaySort();
                                    }
                                }
                                TeamArena.ArenaStatistics.TryGetValue(Bot.Entity.UID, out Bot.TeamArenaStatistic);
                                if (Bot.TeamArenaStatistic == null)
                                {
                                    Bot.TeamArenaStatistic = new TeamArenaStatistic(true);
                                    Bot.TeamArenaStatistic.EntityID = Bot.Entity.UID;
                                    Bot.TeamArenaStatistic.Name = Bot.Entity.Name;
                                    Bot.TeamArenaStatistic.Level = Bot.Entity.Level;
                                    Bot.TeamArenaStatistic.Class = Bot.Entity.Class;
                                    Bot.TeamArenaStatistic.Model = Bot.Entity.Mesh;
                                    TeamArenaTable.InsertArenaStatistic(Bot);
                                    Bot.TeamArenaStatistic.Status = 0;
                                    if (TeamArena.ArenaStatistics.ContainsKey(Bot.Entity.UID))
                                    {
                                        TeamArena.ArenaStatistics.Remove(Bot.Entity.UID);
                                    }
                                    TeamArena.ArenaStatistics.Add(Bot.Entity.UID, Bot.TeamArenaStatistic);
                                }
                                else if (Bot.TeamArenaStatistic.EntityID == 0)
                                {
                                    Bot.TeamArenaStatistic = new TeamArenaStatistic(true);
                                    Bot.TeamArenaStatistic.EntityID = Bot.Entity.UID;
                                    Bot.TeamArenaStatistic.Name = Bot.Entity.Name;
                                    Bot.TeamArenaStatistic.Level = Bot.Entity.Level;
                                    Bot.TeamArenaStatistic.Class = Bot.Entity.Class;
                                    Bot.TeamArenaStatistic.Model = Bot.Entity.Mesh;
                                    TeamArenaTable.InsertArenaStatistic(Bot);
                                    Bot.TeamArenaStatistic.Status = 0;
                                    if (TeamArena.ArenaStatistics.ContainsKey(Bot.Entity.UID))
                                    {
                                        TeamArena.ArenaStatistics.Remove(Bot.Entity.UID);
                                    }
                                    TeamArena.ArenaStatistics.Add(Bot.Entity.UID, Bot.TeamArenaStatistic);
                                }
                                else
                                {
                                    Bot.TeamArenaStatistic.Level = Bot.Entity.Level;
                                    Bot.TeamArenaStatistic.Class = Bot.Entity.Class;
                                    Bot.TeamArenaStatistic.Model = Bot.Entity.Mesh;
                                    Bot.TeamArenaStatistic.Name = Bot.Entity.Name;
                                }
                                #region Champion
                                Conquer_Online_Server.Game.Champion.ChampionStats.TryGetValue(Bot.Entity.UID, out Bot.ChampionStats);
                                if (Bot.ChampionStats == null)
                                {
                                    Bot.ChampionStats = new Conquer_Online_Server.Network.GamePackets.ChampionStatistic(true);
                                    Bot.ChampionStats.UID = Bot.Entity.UID;
                                    Bot.ChampionStats.Name = Bot.Entity.Name;
                                    Bot.ChampionStats.Level = Bot.Entity.Level;
                                    Bot.ChampionStats.Class = Bot.Entity.Class;
                                    Bot.ChampionStats.Model = Bot.Entity.Mesh;
                                    Bot.ChampionStats.Points = 0;
                                    Bot.ChampionStats.LastReset = DateTime.Now;
                                    ChampionTable.InsertStatistic(Bot);
                                    if (Conquer_Online_Server.Game.Champion.ChampionStats.ContainsKey(Bot.Entity.UID))
                                        Conquer_Online_Server.Game.Champion.ChampionStats.Remove(Bot.Entity.UID);
                                    Conquer_Online_Server.Game.Champion.ChampionStats.Add(Bot.Entity.UID, Bot.ChampionStats);
                                }
                                else if (Bot.ChampionStats.UID == 0)
                                {
                                    Bot.ChampionStats = new Conquer_Online_Server.Network.GamePackets.ChampionStatistic(true);
                                    Bot.ChampionStats.UID = Bot.Entity.UID;
                                    Bot.ChampionStats.Name = Bot.Entity.Name;
                                    Bot.ChampionStats.Level = Bot.Entity.Level;
                                    Bot.ChampionStats.Class = Bot.Entity.Class;
                                    Bot.ChampionStats.Model = Bot.Entity.Mesh;
                                    Bot.ChampionStats.Points = 0;
                                    Bot.ChampionStats.LastReset = DateTime.Now;
                                    ArenaTable.InsertArenaStatistic(Bot);
                                    Bot.ArenaStatistic.Status = Conquer_Online_Server.Network.GamePackets.ArenaStatistic.NotSignedUp;
                                    if (Conquer_Online_Server.Game.Champion.ChampionStats.ContainsKey(Bot.Entity.UID))
                                        Conquer_Online_Server.Game.Champion.ChampionStats.Remove(Bot.Entity.UID);
                                    Conquer_Online_Server.Game.Champion.ChampionStats.Add(Bot.Entity.UID, Bot.ChampionStats);
                                }
                                else
                                {
                                    Bot.ChampionStats.Level = Bot.Entity.Level;
                                    Bot.ChampionStats.Class = Bot.Entity.Class;
                                    Bot.ChampionStats.Model = Bot.Entity.Mesh;
                                    Bot.ChampionStats.Name = Bot.Entity.Name;
                                    if (Bot.ChampionStats.LastReset.DayOfYear != DateTime.Now.DayOfYear)
                                        ChampionTable.Reset(Bot.ChampionStats);
                                }
                                Conquer_Online_Server.Game.Champion.Clear(Bot);
                                #endregion
                                #endregion Arena
                                //   Conquer_Online_Server.Database.EntityTable.LoadAchievement(Bot);
                                Bot.Entity.MyAchievement = new Conquer_Online_Server.Game.Achievement(client.Entity);
                                Bot.ChiData = (ChiTable.AllData[Bot.Entity.UID] = new ChiTable.ChiData() { UID = Bot.Entity.UID, Name = Bot.Entity.Name, Powers = Bot.ChiPowers });

                                Bot.Enemy = new SafeDictionary<uint, Conquer_Online_Server.Game.ConquerStructures.Society.Enemy>(50);

                                Bot.Apprentices = new SafeDictionary<uint, Conquer_Online_Server.Game.ConquerStructures.Society.Apprentice>();

                                VariableVault variables;
                                Conquer_Online_Server.Database.EntityVariableTable.Load(Bot.Account.EntityID, out variables);
                                Bot.Variables = variables;

                                if (Bot.BackupArmorLook != 0)
                                    Bot.SetNewArmorLook(Bot.BackupArmorLook);
                                else
                                    Bot.SetNewArmorLook(Bot.ArmorLook);
                                Bot.SetNewHeadgearLook(Bot.HeadgearLook);
                                Bot.BackupArmorLook = 0;

                                Bot.LoadData(true);

                                if (Bot.Entity.GuildID != 0)
                                    Bot.Entity.GuildBattlePower = Bot.Guild.GetSharedBattlepower(Bot.Entity.GuildRank);

                                Bot.ReviewMentor();

                                //  Conquer_Online_Server.Network.PacketHandler.LoginMessages(Bot);

                                foreach (ConquerItem item in client.Equipment.Objects)
                                    if (item != null)
                                    {
                                        if (Conquer_Online_Server.Database.ConquerItemInformation.BaseInformations.ContainsKey(item.ID))
                                        {
                                            item.Send(Bot);
                                            ItemUsage usage = new ItemUsage(true) { ID = ItemUsage.EquipItem };
                                            usage.UID = item.UID;
                                            usage.dwParam = item.Position;
                                            Bot.Send(usage);
                                            Bot.LoadItemStats();
                                            Bot.CalculateStatBonus();
                                            Bot.CalculateHPBonus();
                                        }
                                    }

                                Bot.LoadItemStats();
                                Bot.CalculateStatBonus();
                                Bot.CalculateHPBonus();
                                Bot.Equipment.UpdateEntityPacket();
                                ClientEquip equips = new ClientEquip();
                                equips.DoEquips(Bot);
                                Bot.Send(equips);
                                Bot.Equipment.UpdateEntityPacket();

                                Bot.Entity.SendUpdates = true;
                                Bot.Entity.UpdateEffects(true);

                                if (Bot.Entity.Mana < 800)
                                    Bot.Entity.Mana = Bot.Entity.MaxMana = 800;

                                if (this.Bot.Spells == null)
                                    this.Bot.Spells = new SafeDictionary<ushort, ISkill>(0x2710);

                                ISkill xx = new Spell(true)
                                {
                                    ID = 1000,
                                    Level = 4
                                };
                                Bot.Spells.Add(xx.ID, xx);
                                xx = new Spell(true)
                                {
                                    ID = 1001,
                                    Level = 3
                                };
                                Bot.Spells.Add(xx.ID, xx);
                                foreach (ISkill s in Bot.Spells.Values)
                                {
                                    if (Conquer_Online_Server.Database.SpellTable.SpellInformations[s.ID][s.Level].CanKill == true)
                                    {
                                        skills skill = new skills()
                                        {
                                            ID = s.ID,
                                            Level = s.Level
                                        };
                                        Count_AISkills++;
                                        AISkills.Add(Count_AISkills, skill);
                                    }
                                }
                                Bot.LoggedIn = true;

                                Bot.Screen.FullWipe();
                                Bot.Screen.Reload(null);
                                Program.World.Register(Bot);
                                client.Map.AddAI(Bot.Entity);
                                client.SendScreenSpawn(Bot.Entity, true);

                                try
                                {
                                    if (client.Spells != null)
                                    {
                                        foreach (ISkill s in client.Spells.Values)
                                        {
                                            if (s.ID != 0)
                                                if (s.Level >= 0)
                                                    if (Conquer_Online_Server.Database.SpellTable.SpellInformations.ContainsKey(s.ID))
                                                        if (Conquer_Online_Server.Database.SpellTable.SpellInformations[s.ID][s.Level] != null)
                                                        {
                                                            if (Conquer_Online_Server.Database.SpellTable.SpellInformations[s.ID][s.Level].CanKill == true && Conquer_Online_Server.Database.SpellTable.SpellInformations[s.ID][s.Level].NeedXP != 1)
                                                            {
                                                                ISkill x = new Spell(true)
                                                                {
                                                                    ID = s.ID,
                                                                    Level = s.Level
                                                                };
                                                                Bot.Spells.Add(x.ID, x);
                                                                skills skill = new skills()
                                                                {
                                                                    ID = s.ID,
                                                                    Level = s.Level
                                                                };
                                                                Count_AISkills++;
                                                                AISkills.Add(Count_AISkills, skill);
                                                            }
                                                        }
                                        }
                                    }
                                }
                                catch (Exception exception2)
                                {
                                    Exception exception = exception2;
                                    Program.SaveException(exception);
                                    client.Send(new Message("Impossible to Adding some skills to  this AI", System.Drawing.Color.BurlyWood, 0x7dc));
                                }
                            }
                        }
                        break;
                    #endregion
                }

            }
            catch (Exception exception2)
            {
                Exception exception = exception2;
                Program.SaveException(exception);
                client.Send(new Message("Impossible to Summon this AI", System.Drawing.Color.BurlyWood, 0x7dc));
            }

        }

        #region Jump Bot
        public DateTime LastBotJump = DateTime.Now;
        public DateTime SitAt = DateTime.Now;
        public DateTime LastBotAttack = DateTime.Now;
        // private Map dynamicMap;
        public uint Player1Damage;
        public uint Player2Damage;
        public int attackSpeed
        {
            get
            {
                var time = 1500;
                return time;
            }
        }
        public uint LastWalk
        {
            get;
            set;
        }
        public void HandleJump()
        {
            Jump_Action();
        }

        public void BeginJumpBot(GameState target)
        {
            if (Threads.BotThread.Bots.ContainsKey(Bot.Entity.UID))
                Threads.BotThread.Bots.TryRemove(Bot.Entity.UID, out target.AIBot);

            Threads.BotThread.Bots.TryAdd(Bot.Entity.UID, this);

            Target = target.Entity;
        }

        public void StopJumpBot()
        {
            AIBot rBot;
            Threads.BotThread.Bots.TryRemove(Bot.Entity.UID, out rBot);
        }
        int num = 0;
        private void Jump_Action()
        {

            if (Target == null)
            { Bot.SendScreen(new Message("Idle Mode | No Target!", Target.Name, Bot.Entity.Name, System.Drawing.Color.White, Message.Talk), true); return; }
            if (Bot.Map.ID != Target.Owner.Map.ID)
            {
                Dispose();
                return;
            }
            if (Time32.Now >= Bot.Entity.DeathStamp.AddSeconds(18) && Bot.Entity.Dead)
            {
                Bot.Entity.BringToLife();
                SpellUse use = new SpellUse(true)
                {
                    Attacker = Bot.Entity.UID,
                    SpellID = 0x44c,
                    X = Bot.Entity.X,
                    Y = Bot.Entity.Y
                };
                Bot.SendScreen(use, true);
                Bot.SendScreen(new Message("Reviving!", Target.Name, "Bot", System.Drawing.Color.White, Message.Talk), true);
                num = 0;
                return;
            }
            if ((Bot.Entity.Dead) && (num == 0))
            {
                Bot.SendScreen(new Message("Reviving in 10 seconds!", Target.Name, "Bot", System.Drawing.Color.White, Message.Talk), true);
                Bot.Entity.Die(Bot.Entity);
                Target.Owner.Time(10);
                num = 1;
                Target.Owner.MessageBox("Wanna Quit? || Yes To Telport , No To Stay.",
                            p =>
                            {
                                p.Entity.Teleport(1002, 429, 378);
                              //  if (p.AIBot != null)
                               //     p.AIBot.Dispose();
                            }, null);
                return;
            }

            bool CanFB = true;
            Bot.Entity.Action = 0;
            uint num9 = 0;
            skills spell = null;
            if (Bot.Entity.Action != Enums.ConquerAction.Sit)
            {
                if (Count_AISkills >= 2)
                {
                    num9 = (uint)Kernel.Random.Next(1, (int)Math.Max(1, Count_AISkills));
                    spell = AISkills[num9];
                }
                else
                {
                    skills xx = new skills()
                    {
                        ID = 1000,
                        Level = 4
                    };
                    spell = xx;

                }
            }

            if (Target.Dead && Bot.Entity.Action != Enums.ConquerAction.Cool && Time32.Now >= Bot.CoolStamp.AddSeconds(10))
            {
                SitAt = DateTime.Now;
                Bot.Entity.Action = Enums.ConquerAction.Cool;
                Conquer_Online_Server.Network.GamePackets.Data generalData = new Conquer_Online_Server.Network.GamePackets.Data(true);
                generalData.UID = Bot.Entity.UID;
                generalData.dwParam = Bot.Entity.Action;
                generalData.dwParam |= (uint)((Bot.Entity.Class * 0x10000) + 0x1000000);
                Bot.Entity.SendScreen(generalData);
                Bot.CoolStamp = Time32.Now;

                Bot.SendScreenSpawn(Bot.Entity, true);
                Target.Owner.SendScreenSpawn(Bot.Entity, true);
                Bot.SendScreen(new Message("Die Noob :P , HHHHHHHHHHH!", Target.Name, "Bot", System.Drawing.Color.White, Message.Talk), true);
                return;
            }

            if (Bot.Entity.Stamina < SpellTable.SpellInformations[spell.ID][spell.Level].UseStamina && Bot.Entity.Action != Enums.ConquerAction.Sit)
            {
                SitAt = DateTime.Now;
                Bot.Entity.Action = Enums.ConquerAction.Sit;
                Conquer_Online_Server.Network.GamePackets.Data buffer = new Conquer_Online_Server.Network.GamePackets.Data(true)
                {
                    UID = Bot.Entity.UID,
                    dwParam = Bot.Entity.Action
                };
                Bot.Entity.SendScreen(buffer);

                Bot.SendScreenSpawn(Bot.Entity, true);
                Target.Owner.SendScreenSpawn(Bot.Entity, true);
                return;
            }

            if (DateTime.Now >= LastBotJump.AddMilliseconds(JumpSpeed))
            {
                if (Target == null || Bot.Entity.Dead || Target.Dead)
                    return;

                if (Kernel.GetDistance(Bot.Entity.X, Bot.Entity.Y, Target.X, Target.Y) > 15)
                {
                    Enums.ConquerAngle angle = Kernel.GetFacing(Kernel.GetAnglex(Bot.Entity.X, Bot.Entity.Y, Target.X, Target.Y));
                    ushort size = (ushort)Kernel.GetDistance(Target.X, Target.Y, Bot.Entity.X, Bot.Entity.Y);
                    size /= 3;
                    CanFB = false;
                    LastBotJump = DateTime.Now;
                    Jump(size, angle);
                }
                else
                {
                    ushort size = (ushort)ProjectX_V3_Lib.ThreadSafe.RandomGenerator.Generator.Next(10);
                    Enums.ConquerAngle angle = (Enums.ConquerAngle)ProjectX_V3_Lib.ThreadSafe.RandomGenerator.Generator.Next(8);
                    LastBotJump = DateTime.Now;
                    Jump(size, angle);
                }
                if (!Bot.Attackable)
                    Bot.Attackable = true;
            }            

            if (DateTime.Now >= LastBotAttack.AddMilliseconds(AttackSpeed) && Bot.Entity.Stamina > SpellTable.SpellInformations[spell.ID][spell.Level].UseStamina)
            {
                if (Target == null || Bot.Entity.Dead || Target.Dead)
                    return;
                if (Kernel.ChanceSuccess(ShootChance) && CanFB)
                {
                    if (Kernel.GetDistance(Bot.Entity.X, Bot.Entity.Y, Target.X, Target.Y) <= SpellTable.SpellInformations[spell.ID][spell.Level].Distance)
                    {
                        #region fb / ss
                        Shoot(Accuracy, spell);
                        #endregion
                    }
                    

                }
            }
            #region Remove unused skill
           /* if ((SpellTable.SpellInformations[spell.ID][spell.Level] != null) && (SpellTable.SpellInformations[spell.ID][spell.Level].OnlyWithThisWeaponSubtype != 0))
            {
                if (!this.Bot.Equipment.Free((byte)4))
                {
                    uint num21 = this.Bot.Equipment.Objects[3].ID / 0x3e8;
                    if (!this.Bot.Equipment.Free((byte)5) && (this.Bot.Equipment.Objects[4] != null))
                    {
                        uint num22 = this.Bot.Equipment.Objects[4].ID / 0x3e8;
                        if ((num21 != SpellTable.SpellInformations[spell.ID][spell.Level].OnlyWithThisWeaponSubtype) && (num22 != SpellTable.SpellInformations[spell.ID][spell.Level].OnlyWithThisWeaponSubtype))
                        {
                            AISkills.Remove(spell.ID);
                            Count_AISkills--;
                            return;
                        }
                    }
                    else if (num21 != SpellTable.SpellInformations[spell.ID][spell.Level].OnlyWithThisWeaponSubtype)
                    {
                        AISkills.Remove(spell.ID);
                        Count_AISkills--;
                        return;
                    }
                }
                else
                {
                    AISkills.Remove(spell.ID);
                    Count_AISkills--;
                    return;
                }
            }
            else
            {
                if (SpellTable.SpellInformations[spell.ID][spell.Level].OnlyWithThisWeaponSubtype != 0)
                {
                    uint firstwepsubtype, secondwepsubtype;

                    if (!Bot.Equipment.Free(24))
                    {
                        firstwepsubtype = Bot.Equipment.Objects[23].ID / 1000;
                        if (!Bot.Equipment.Free(24) && Bot.Equipment.Objects[24] != null)
                        {
                            secondwepsubtype = Bot.Equipment.Objects[24].ID / 1000;
                            if (firstwepsubtype != SpellTable.SpellInformations[spell.ID][spell.Level].OnlyWithThisWeaponSubtype)
                            {
                                if (secondwepsubtype != SpellTable.SpellInformations[spell.ID][spell.Level].OnlyWithThisWeaponSubtype)
                                {
                                    AISkills.Remove(spell.ID);
                                    Count_AISkills--;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            if (firstwepsubtype != SpellTable.SpellInformations[spell.ID][spell.Level].OnlyWithThisWeaponSubtype)
                            {
                                AISkills.Remove(spell.ID);
                                Count_AISkills--;
                                return;
                            }
                        }
                    }
                    else
                    {
                        AISkills.Remove(spell.ID);
                        Count_AISkills--;
                        return;
                    }
                }

            }*/
            #endregion Remove unused skill

        }

        public void JumpOver(ushort x, ushort y)
        {
            Enums.ConquerAngle angle = Kernel.GetFacing(Kernel.GetAnglex(Bot.Entity.X, Bot.Entity.Y, Target.X, Target.Y));
            ushort size = (ushort)Kernel.GetDistance(x, y, Bot.Entity.X, Bot.Entity.Y);
            size *= 2;
            if (size > 18)
                size = 18;
            Jump(size, angle);
        }
        public void Jump(ushort size, Enums.ConquerAngle angle)
        {
            Bot.Entity.Move(angle);
            Jump(Bot.Entity.X, Bot.Entity.Y);
        }
        private void Jump(ushort x, ushort y)
        {
            if (MaxX > 0)
            {
                if (x > MaxX)
                    return;
                if (y > MaxY)
                    return;
                if (x < MinX)
                    return;
                if (y < MinY)
                    return;
            }

            Conquer_Online_Server.Network.GamePackets.Data buffer = new Conquer_Online_Server.Network.GamePackets.Data(true)
            {
                ID = Conquer_Online_Server.Network.GamePackets.Data.Jump,
                dwParam = (uint)((Bot.Entity.Y << 0x10) | Bot.Entity.X),
                wParam1 = Bot.Entity.X,
                wParam2 = Bot.Entity.Y,
                UID = Bot.Entity.UID
            };
            Bot.Entity.SendScreen(buffer);

            Bot.Entity.X = x;
            Bot.Entity.Y = y;
        }
        #endregion

        public void Die()
        {
            Bot.XPCount = 0;
            Bot.Entity.Hitpoints = 0;
            Bot.Entity.Stamina = 150;
            Bot.Entity.DeathStamp = Time32.Now;
            Bot.Entity.ToxicFogLeft = 0;
            _String str = new _String(true);
            str.UID = Bot.Entity.UID;
            str.TextsCount = 1;
            str.Type = _String.Effect;
            str.Texts.Add("endureXPdeath");
            Bot.Entity.SendScreen(str);
            if (Bot.Entity.EntityFlag == EntityFlag.Player)
            {
                if (Constants.PKFreeMaps.Contains(Bot.Entity.MapID))                    
                Bot.Entity.AddFlag(Update.Flags.Dead);
                Bot.Entity.RemoveFlag(Update.Flags.Fly);
                Bot.Entity.RemoveFlag(Update.Flags.Ride);
                Bot.Entity.RemoveFlag(Update.Flags.Cyclone);
                Bot.Entity.RemoveFlag(Update.Flags.Superman);
                Bot.Entity.RemoveFlag(Update.Flags.FatalStrike);
                Bot.Entity.RemoveFlag(Update.Flags.FlashingName);
                Bot.Entity.RemoveFlag(Update.Flags.ShurikenVortex);
                Bot.Entity.RemoveFlag2(Update.Flags2.Oblivion);

                Attack attackx = new Attack(true);
                attackx.AttackType = Attack.Kill;
                attackx.X = Target.X;
                attackx.Y = Target.Y;
                attackx.Attacked = Bot.Entity.UID;
                Bot.SendScreen(attackx, true);

                if (Bot.Entity.Body % 10 < 3)
                    Bot.Entity.TransformationID = 99;
                else
                    Bot.Entity.TransformationID = 98;

                Bot.Send(new MapStatus() { BaseID = Bot.Map.BaseID, ID = Bot.Map.ID, Status = MapsTable.MapInformations[Bot.Map.ID].Status });

            }
            else
            {
                Kernel.Maps[Bot.Entity.MapID].Floor[Bot.Entity.X, Bot.Entity.Y, Bot.Entity.MapObjType, Bot.Entity] = true;
            }
        }

        public void Shoot(int accu, skills spell)
        {
            if (Bot.Entity.Dead || Target.Dead)
                return;

            var interact = new Attack(true);
            interact.Effect1 = Attack.AttackEffects1.None;
            interact.AttackType = Attack.Magic;
            interact.MagicType = spell.ID;
            interact.Attacker = Bot.Entity.UID;
            interact.Attacked = Target.UID;
            interact.MagicLevel = spell.Level;            
            interact.Decoded = true;

            if (Kernel.ChanceSuccess(accu))
            {
                interact.X = Target.X;
                interact.Y = Target.Y;
            }
            else
            {
                interact.X = (ushort)(Target.X + 1);
                interact.Y = (ushort)(Target.Y + 1);
            }
            LastBotAttack = DateTime.Now;
            Bot.Entity.AttackPacket = interact;
            new Conquer_Online_Server.Game.Attacking.Handle(interact, Bot.Entity, Target);
        }       

        public void Dispose()
        {  
            if (Bot == null)
                return;
            if (Bot.Map == null)
                return;
            StopJumpBot();
            Program.World.Unregister(Bot);
            Target.Owner.AIBot = null;
            Target.Owner.aisummoned = false;
            Bot.Map.RemoveAI(Bot.Entity);
            //  Bot.SendScreenSpawn(null, false);
            Bot = null;
        }

        private ushort DestinationX;
        private ushort DestinationY;
        private Threads.ActionThread.ThreadAction CurrentJumpAction;

        public void JumpToDestination(ushort X, ushort Y)
        {
            DestinationX = X;
            DestinationY = Y;
            CurrentJumpAction = Threads.ActionThread.AddAction(() =>
            {
                Jump();
            }, 500);
        }
        public void Jump()
        {
            if (Kernel.GetDistance(Bot.Entity.X, Bot.Entity.Y, DestinationX, DestinationY) <= 5)
            {
                Threads.ActionThread.Actions.TryRemove(CurrentJumpAction.ActionID, out CurrentJumpAction);
                CurrentJumpAction = null;
                return;
            }

            Enums.ConquerAngle angle = Kernel.GetFacing(Kernel.GetAnglex(Bot.Entity.X, Bot.Entity.Y, DestinationX, DestinationY));

            Bot.Entity.Move(angle);

            ushort x = Bot.Entity.X;
            ushort y = Bot.Entity.Y;

            Conquer_Online_Server.Network.GamePackets.Data buffer = new Conquer_Online_Server.Network.GamePackets.Data(true)
            {
                ID = 0x89,
                dwParam = (uint)((Bot.Entity.Y << 0x10) | Bot.Entity.X),
                wParam1 = Bot.Entity.X,
                wParam2 = Bot.Entity.Y,
                wParam5 = uint.MaxValue,
                UID = Bot.Entity.UID
            };
            Bot.Entity.SendScreen(buffer);

            Bot.Entity.X = x;
            Bot.Entity.Y = y;

        }

        public void Teleport(ushort MapID, ushort X, ushort Y)
        {

            if (MapID == Bot.Entity.MapID)
            {
                Bot.Entity.Teleport(X, Y);
            }
            else
            {
                Bot.Entity.X = X;
                Bot.Entity.Y = Y;
                Bot.Entity.PreviousMapID = Bot.Entity.MapID;
                Bot.Entity.MapID = MapID;
                Conquer_Online_Server.Network.GamePackets.Data data2 = new Conquer_Online_Server.Network.GamePackets.Data(true)
                {
                    UID = Bot.Entity.UID,
                    ID = 0x56,
                    dwParam = MapsTable.MapInformations[(ushort)MapID].BaseID,
                    wParam1 = X,
                    wParam2 = Y
                };
                Bot.Entity.Owner.Send(data2);
                MapStatus status = new MapStatus
                {
                    BaseID = Bot.Entity.Owner.Map.BaseID,
                    ID = Bot.Entity.Owner.Map.ID,
                    Status = MapsTable.MapInformations[(ushort)Bot.Entity.Owner.Map.ID].Status,
                    Weather = MapsTable.MapInformations[(ushort)Bot.Entity.Owner.Map.ID].Weather
                };
                Bot.Entity.Owner.Send(status);
                Conquer_Online_Server.Network.GamePackets.Weather weather = new Conquer_Online_Server.Network.GamePackets.Weather(true)
                {
                    WeatherType = (uint)Program.WeatherType,
                    Intensity = 100,
                    Appearence = 2,
                    Direction = 4
                };
                Bot.Entity.Owner.Send(weather);
                Bot.Entity.Owner.Entity.Action = 0;
                Bot.Entity.Owner.ReviveStamp = Time32.Now;
                Bot.Entity.Owner.Attackable = false;
                if (!Bot.Equipment.Free((byte)12) && ((Bot.Map.ID == 0x40c) && (Bot.Equipment.TryGetItem((byte)12).Plus < 6)))
                {
                    Bot.Entity.RemoveFlag(0x4000000000000L);
                }                
            }
        }


    }
}
