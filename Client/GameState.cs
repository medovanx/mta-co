using System;
using System.Linq;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using MTA.Network.Cryptography;
using MTA.Network.GamePackets;
using MTA.Network.Sockets;
using MTA.Network;
using MTA.Interfaces;
using MTA.Game.ConquerStructures;
using System.Drawing;
using MTA.Game;
using MTA.Database;
using Albetros.Core;
using System.Diagnostics;
using Microsoft.Win32;
using MTA.MaTrix;
//using MTA.iPiraTe.JiangHu;


namespace MTA.Client
{
    public class GameState
    {
        public uint ClaimedElitePk
        {
            get { return this["ClaimedElitePk"]; }
            set { this["ClaimedElitePk"] = value; }
        }
        public uint ClaimedTeampk
        {
            get { return this["ClaimedTeampk"]; }
            set { this["ClaimedTeampk"] = value; }
        }
        public uint ClaimedSkillTeam
        {
            get { return this["ClaimedSkillTeam"]; }
            set { this["ClaimedSkillTeam"] = value; }
        }
        public void GetLanguages(string language)
        {
            switch (language)
            {
                case "En":
                    Language = Languages.English;
                    break;
                case "Ar":
                    Language = Languages.Arabic;
                    break;
            }

        }
        public string LanguageToString()
        {
            switch (Language)
            {
                case Languages.English:
                    return "en";
                case Languages.Arabic:
                    return "ar";
            }
            return "en";

        }
        public Languages Language = Languages.English;  
        public Time32 bodeSHStamp;
        public bool FTbode = false;
      //  public Game.InnerPower InnerPower;
        public bool InWareHouse()
        {
            foreach (var wh in Warehouses.Values)
            {
                if (wh.Count > 0)
                    return true;
            }
            return false;
        }
        public void BlessTouch(GameState client)
        {
           
            if (!client.Spells.ContainsKey(12390))
                return;

            if (client.Weapons != null)
                if (client.Weapons.Item2 != null)
                    if (client.Weapons.Item2.ID / 1000 != 619)
                        return;

            var spell2 = SpellTable.GetSpell(client.Spells[12390].ID, client.Spells[12390].Level);
            if (Kernel.Rate((double)spell2.Percent))
            {

                var spell = Database.SpellTable.GetSpell(1095, 4);
                Entity.AddFlag(Update.Flags.Stigma);
                Entity.StigmaStamp = Time32.Now;
                Entity.StigmaIncrease = spell.PowerPercent;
                Entity.StigmaTime = (byte)spell.Duration;
                if (Entity.EntityFlag == EntityFlag.Player)
                    Send(Constants.Stigma(spell.PowerPercent, spell.Duration));

                spell = Database.SpellTable.GetSpell(1090, 4);
                Entity.ShieldTime = 0;
                Entity.ShieldStamp = Time32.Now;
                Entity.MagicShieldStamp = Time32.Now;
                Entity.MagicShieldTime = 0;

                Entity.AddFlag(Update.Flags.MagicShield);
                Entity.MagicShieldStamp = Time32.Now;
                Entity.MagicShieldIncrease = 1.1f;//spell.PowerPercent;
                Entity.MagicShieldTime = (byte)spell.Duration;
                if (Entity.EntityFlag == EntityFlag.Player)
                    Send(Constants.Shield(spell.PowerPercent, spell.Duration));

                spell = Database.SpellTable.GetSpell(1085, 4);
                Entity.AccuracyStamp = Time32.Now;
                Entity.StarOfAccuracyStamp = Time32.Now;
                Entity.StarOfAccuracyTime = 0;
                Entity.AccuracyTime = 0;

                Entity.AddFlag(Update.Flags.StarOfAccuracy);
                Entity.StarOfAccuracyStamp = Time32.Now;
                Entity.StarOfAccuracyTime = (byte)spell.Duration;
                if (Entity.EntityFlag == EntityFlag.Player)
                    Send(Constants.Accuracy(spell.Duration));

                client.IncreaseSpellExperience(100, 12390);
            }
        }

        public void BreakTouch(GameState client)
        {           
            if (!client.Spells.ContainsKey(12400))
                return;

            if (client.Weapons != null)
                if (client.Weapons.Item2 != null)
                    if (client.Weapons.Item2.ID / 1000 != 619)
                        return;

            var spell = SpellTable.GetSpell(client.Spells[12400].ID, client.Spells[12400].Level);
            if (MyMath.Success((double)30))
            {
                if (Entity.ContainsFlag3(Update.Flags3.lianhuaran04))
                {
                    SpellUse suse = new SpellUse(true);
                    suse.Attacker = Entity.UID;
                    suse.SpellID = spell.ID;
                    suse.SpellLevel = spell.Level;

                    var array = Game.Attacking.Handle.PlayerinRange(Entity, Entity).ToArray();
                    foreach (var target in array)
                    {                       
                        var attacked = target.Entity;
                        if (attacked.UID == client.Entity.UID)
                            continue;
                        if (Game.Attacking.Handle.CanAttack(client.Entity, attacked, spell, true))
                        {
                            var attack = new Attack(true);
                            attack.Attacker = client.Entity.UID;
                            attack.Attacked = attacked.UID;
                          
                            uint damage = Game.Attacking.Calculate.Magic(client.Entity, attacked, ref attack);

                            attack.Damage = damage;
                            suse.Effect1 = attack.Effect1;
                            suse.Effect1 = attack.Effect1;

                            Game.Attacking.Handle.ReceiveAttack(client.Entity, attacked, attack, ref damage, spell);
                            suse.AddTarget(attacked, damage, attack);
                        }
                    }
                    client.SendScreen(suse, true);

                    Entity.RemoveFlag3(Update.Flags3.lianhuaran01);
                    Entity.RemoveFlag3(Update.Flags3.lianhuaran02);
                    Entity.RemoveFlag3(Update.Flags3.lianhuaran03);
                    Entity.RemoveFlag3(Update.Flags3.lianhuaran04);
                }
                else if (Entity.ContainsFlag3(Update.Flags3.lianhuaran03))
                {
                    Entity.AddFlag3(Update.Flags3.lianhuaran04);
                    Entity.RemoveFlag3(Update.Flags3.lianhuaran03);
                    Entity.lianhuaranPercent = 0.5f;
                }
                else if (Entity.ContainsFlag3(Update.Flags3.lianhuaran02))
                {
                    Entity.AddFlag3(Update.Flags3.lianhuaran03);
                    Entity.RemoveFlag3(Update.Flags3.lianhuaran02);
                    Entity.lianhuaranPercent = 0.4f;
                }
                else if (Entity.ContainsFlag3(Update.Flags3.lianhuaran01))
                {
                    Entity.AddFlag3(Update.Flags3.lianhuaran02);
                    Entity.RemoveFlag3(Update.Flags3.lianhuaran01);
                    Entity.lianhuaranPercent = 0.3f;
                }
                else if (!Entity.ContainsFlag3(Update.Flags3.lianhuaran01))
                {
                    Entity.AddFlag3(Update.Flags3.lianhuaran01);
                    Entity.lianhuaranPercent = 0.1f;
                }
                Entity.lianhuaranStamp = Time32.Now;
                Entity.lianhuaranLeft = 20;
                
                client.IncreaseSpellExperience(100, 12400);               
            }
        }
        public uint SashSlots
        {
            get { return this["SashSlots"]; }
            set
            {
                this["SashSlots"] = value;
                if (Entity.EntityFlag == EntityFlag.Player)
                {
                    Entity.Update(Update.Sash, value, false);
                    Entity.Update(Update.AvailableSlots, 200, false);
                }
            }
        }
        
        public SafeDictionary<MTA.Network.GamePackets.Update.AuraType, MaTrix.Auras> Auras = new SafeDictionary<Update.AuraType, MaTrix.Auras>(8);
        public GameState[] MonksInTeam()
        {
            return Team.Teammates.Where(x => x.Entity.Aura_isActive).ToArray();
        }
        public void CheckTeamAura()
        {
            if (Team != null)
            {
                var monks = MonksInTeam();
                if (monks != null)
                {
                    foreach (var monk in monks)
                    {
                        Update.AuraType aura = Update.AuraType.TyrantAura;
                        switch (monk.Entity.Aura_actType)
                        {
                            case Update.Flags2.EarthAura: aura = Update.AuraType.EarthAura; break;
                            case Update.Flags2.FireAura: aura = Update.AuraType.FireAura; break;
                            case Update.Flags2.WaterAura: aura = Update.AuraType.WaterAura; break;
                            case Update.Flags2.WoodAura: aura = Update.AuraType.WoodAura; break;
                            case Update.Flags2.MetalAura: aura = Update.AuraType.MetalAura; break;
                            case Update.Flags2.FendAura: aura = Update.AuraType.FendAura; break;
                            case Update.Flags2.TyrantAura: aura = Update.AuraType.TyrantAura; break;
                        }
                        if (!Auras.ContainsKey(aura))
                        {
                            if (this.Entity.UID != monk.Entity.UID && Kernel.GetDistance(this.Entity.X, this.Entity.Y, monk.Entity.X, monk.Entity.Y) <= Constants.pScreenDistance)
                            {
                                MaTrix.Auras Aura = new MaTrix.Auras();
                                Aura.TeamAuraOwner = monk;
                                Aura.TeamAuraStatusFlag = monk.Entity.Aura_actType;
                                Aura.TeamAuraPower = monk.Entity.Aura_actPower;
                                Aura.TeamAuraLevel = monk.Entity.Aura_actLevel;
                                Aura.aura = aura;
                                if (!Auras.ContainsKey(Aura.aura))
                                {
                                    Auras.Add(Aura.aura, Aura);
                                    this.Entity.AddFlag2(Aura.TeamAuraStatusFlag);
                                    new Update(true).Aura(this.Entity, Update.AuraDataTypes.Add, aura, Aura.TeamAuraLevel, Aura.TeamAuraPower);
                                    this.doAuraBonuses(Aura.TeamAuraStatusFlag, Aura.TeamAuraPower, 1);                                }
                               
                            }
                        }
                    }
                }
            }
            foreach (var Aura in Auras.Values.ToArray())
            {
                var pthis = Aura.TeamAuraOwner;
                if (pthis == null)
                {
                    new Update(true).Aura(this.Entity, Update.AuraDataTypes.Remove, Aura.aura, Aura.TeamAuraLevel, Aura.TeamAuraPower);
                    //this.removeAuraBonuses(this.TeamAuraStatusFlag, this.TeamAuraPower, 1);
                    this.removeAuraBonuses(Aura.TeamAuraStatusFlag, Aura.TeamAuraPower, 1);
                    this.Entity.RemoveFlag2(Aura.TeamAuraStatusFlag);
                    Auras.Remove(Aura.aura);
                }
                else
                {
                    if (!pthis.Entity.Aura_isActive || !pthis.Socket.Alive || pthis.Entity.Dead || pthis.Entity.MapID != this.Entity.MapID || pthis.Entity.Aura_actType != Aura.TeamAuraStatusFlag)
                    {
                        new Update(true).Aura(this.Entity, Update.AuraDataTypes.Remove, Aura.aura, Aura.TeamAuraLevel, Aura.TeamAuraPower);
                        //this.removeAuraBonuses(this.TeamAuraStatusFlag, this.TeamAuraPower, 1);
                        this.removeAuraBonuses(Aura.TeamAuraStatusFlag, Aura.TeamAuraPower, 1);
                        this.Entity.RemoveFlag2(Aura.TeamAuraStatusFlag);
                        Auras.Remove(Aura.aura);
                    }
                    else
                    {
                        if (this.Team == null || (pthis.Team == null || (pthis.Team != null && !pthis.Team.IsTeammate(this.Entity.UID))) || this.Entity.Dead || Kernel.GetDistance(this.Entity.X, this.Entity.Y, pthis.Entity.X, pthis.Entity.Y) > Constants.pScreenDistance)
                        {
                            new Update(true).Aura(this.Entity, Update.AuraDataTypes.Remove, Aura.aura, Aura.TeamAuraLevel, Aura.TeamAuraPower);
                            this.removeAuraBonuses(Aura.TeamAuraStatusFlag, Aura.TeamAuraPower, 1);
                            this.Entity.RemoveFlag2(Aura.TeamAuraStatusFlag);
                            Auras.Remove(Aura.aura);
                        }
                    }
                }
            }

        
        }
        public Franko.ProgressBar ProgressBar;
        public bool TransferedPlayer;
        public void ChangeName(GameState client)
        {
            client.OnDisconnect = p =>
            {
                #region ChangeName progress
                string name200 = p.Entity.Name;
                string newname = p.NewName;
                uint uid = p.Entity.UID;
                if (newname != "")
                {
                    MTA.Console.WriteLine("Change Name In Progress");
                    if (newname != "")
                    {
                        Database.MySqlCommand cmdupdate = null;
                        cmdupdate = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE);
                        cmdupdate.Update("apprentice").Set("MentorName", newname).Where("MentorID", uid).Execute();

                        cmdupdate = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE);
                        cmdupdate.Update("apprentice").Set("ApprenticeName", newname).Where("ApprenticeID", uid).Execute();

                        cmdupdate = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE);
                        cmdupdate.Update("arena").Set("EntityName", newname).Where("EntityID", uid).Execute();

                        cmdupdate = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE);
                        cmdupdate.Update("claimitems").Set("OwnerName", newname).Where("OwnerUID", uid).Execute();

                        cmdupdate = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE);
                        cmdupdate.Update("claimitems").Set("GainerName", newname).Where("GainerUID", uid).Execute();

                        cmdupdate = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE);
                        cmdupdate.Update("detaineditems").Set("OwnerName", newname).Where("OwnerUID", uid).Execute();

                        cmdupdate = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE);
                        cmdupdate.Update("detaineditems").Set("GainerName", newname).Where("GainerUID", uid).Execute();

                        cmdupdate = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE);
                        cmdupdate.Update("enemy").Set("EnemyName", newname).Where("EnemyID", uid).Execute();

                        cmdupdate = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE);
                        cmdupdate.Update("friends").Set("FriendName", newname).Where("FriendID", uid).Execute();

                        cmdupdate = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE);
                        cmdupdate.Update("guilds").Set("LeaderName", newname).Where("LeaderName", name200).Execute();

                        cmdupdate = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE);
                        cmdupdate.Update("clans").Set("LeaderName", newname).Where("LeaderName", name200).Execute();

                        if (p.Entity.MyJiang != null)
                        {
                            p.Entity.MyJiang.OwnName = newname;
                            Game.JiangHu.JiangHuClients[p.Entity.UID] = p.Entity.MyJiang;
                        }
                        if (p.Entity.MyFlowers != null)
                            p.Entity.MyFlowers.Name = newname;

                        cmdupdate = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE);
                        cmdupdate.Update("nobility").Set("EntityName", newname).Where("EntityUID", uid).Execute();

                        cmdupdate = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE);
                        cmdupdate.Update("partners").Set("PartnerName", newname).Where("PartnerID", uid).Execute();

                        cmdupdate = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE);
                        cmdupdate.Update("chi").Set("name", newname).Where("uid", uid).Execute();

                        cmdupdate = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE);
                        cmdupdate.Update("teamarena").Set("EntityName", newname).Where("EntityID", uid).Execute();

                        cmdupdate = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE);
                        cmdupdate.Update("entities").Set("name", newname).Set("namechange", "").Where("UID", uid).Execute();
                        Console.WriteLine(" -[" + name200 + "] : -[" + newname + "]");


                        if (Game.ConquerStructures.Nobility.Board.ContainsKey(p.Entity.UID))
                        {
                            Game.ConquerStructures.Nobility.Board[p.Entity.UID].Name = p.NewName;
                        }
                        if (Arena.ArenaStatistics.ContainsKey(p.Entity.UID))
                        {
                            Arena.ArenaStatistics[p.Entity.UID].Name = p.NewName;
                        }
                        if (p.Entity.GetClan != null)
                        {
                            if (p.Entity.GetClan.LeaderName == name200)
                            {
                                Kernel.Clans[p.Entity.ClanId].LeaderName = p.NewName;
                            }

                            Kernel.Clans[p.Entity.ClanId].Members[p.Entity.UID].Name = p.NewName;
                        }
                        if (p.Guild != null)
                        {
                            if (p.Guild.LeaderName == name200)
                            {
                                Kernel.Guilds[p.Guild.ID].LeaderName = p.NewName;
                            }

                            Kernel.Guilds[p.Guild.ID].Members[p.Entity.UID].Name = p.NewName;
                        }

                    }
                }
                #endregion ChangeName progressa
            };
            client.Disconnect();
        }
        public string Country { get; set; }

        public DateTime timerattack = new DateTime();
        public GameState LobbyPlayWith;        
        public bool LobbySignup
        {
            get { return this["LobbySignup"]; }
            set
            {
                this["LobbySignup"] = value;
            }
        }
        public ushort SuperPotion
        {
            get { return this["SuperPotion"]; }
            set
            {
                this["SuperPotion"] = value;
                if (Entity.FullyLoaded)
                    if (Entity.EntityFlag == EntityFlag.Player)
                    {
                        if (this != null)
                        {
                            Entity.Update(Network.GamePackets.Update.DoubleExpTimer, Entity.DoubleExperienceTime, 500, false);
                        }
                    }
            }
        }
        public MaTrix.Lobby.QualifierGroup LobbyGroup;
        public MaTrix.Pet Pet;
        public MaTrix.Quests Quests;
        public MaTrix.AI AI;
        public SafeDictionary<uint, MaTrix.Inbox.PrizeInfo> Prizes = new SafeDictionary<uint, MaTrix.Inbox.PrizeInfo>(1000);
        public string NewName = "";        
        public int PingCount { get; set; }
        public byte Claimeds
        {
            get { return this["Claimeds"]; }
            set
            {
                this["Claimeds"] = value;
            }
        }
       
        public bool JiangActive = false;   
        public bool StudyToday
        {
            get { return this["StudyToday"]; }
            set
            {
                this["StudyToday"] = value;
            }
        }
        public uint UsedCourses
        {
            get { return this["UsedCourses"]; }
            set
            {
                this["UsedCourses"] = value;
            }
        }
        public DateTime ResetUsedCourses
        {
            get { return this["ResetUsedCourses"]; }
            set
            {
                this["ResetUsedCourses"] = value;
            }
        }
        
        public bool JoinedDBMap
        {
            get { return this["JoinedDBMap"]; }
            set
            {
                this["JoinedDBMap"] = value;
            }
        }
        public DateTime inDBmap
        {
            get { return this["inDBmap"]; }
            set
            {
                this["inDBmap"] = value;
            }
        }
        public uint Appearance
        {
            get { return this["Appearance"]; }
            set
            {
                this["Appearance"] = value;
            }
        }
        public bool _voted;
        public bool Voted
        {
            get 
            { 
                return _voted; 
            }
            set
            {
                _voted = value;              
                new Database.MySqlCommand(MTA.Database.MySqlCommandType.UPDATE)
                    .Update("entities").Set("VotePoint", value).Where("UID", Entity.UID).Execute();
        
            }
        }
        public DateTime VoteStamp
        {
            get { return this["VoteStamp"]; }
            set
            {
                this["VoteStamp"] = value;
            }
        }
        public uint namechanges
        {
            get { return this["namechanges"]; }
            set
            {
                this["namechanges"] = value;
            }
        }
        public DateTime matrixtime
        {
            get { return this["matrixtime"]; }
            set
            {
                this["matrixtime"] = value;
            }
        }
    
        public ulong Donationx
        {
            get { return this["Donationx"]; }
            set
            {
                this["Donationx"] = value;
            }
        }
       
        public string Command = "";
        public bool OnDonation
        {
            get { return this["ondonation"]; }
            set
            {
                this["ondonation"] = value;
            }
        }
        public bool endarena = false;
        public bool endteam = false;
        public IDisposable[] TimerSubscriptions;
        public object TimerSyncRoot, ItemSyncRoot;
        public Time32 LastVIPTeleport, LastVIPTeamTeleport;
        public bool AlternateEquipment;
        private ClientWrapper _socket;
        public Database.AccountTable Account;
        public GameCryptography Cryptography;
        public DHKeyExchange.ServerKeyExchange DHKeyExchange;
        public bool Exchange = true;
        public ConcurrentPacketQueue Queue;
        public PacketFilter PacketFilter;
        public Time32 CantAttack = Time32.Now;
        public bool Filtering = false;
        public Network.GamePackets.Interaction Interaction;
        public int quarantineKill = 0;
        public int quarantineDeath = 0;
        public int TopDlClaim = 0;
        public int TopGlClaim = 0;
        public uint uniquepoints = 0;
        public Action<GameState> OnDisconnect;
        public int apprtnum = 0;
        public Game.Enums.Color staticArmorColor;
        public bool JustCreated = false;
        public Timer Timer;
        public MTA.Game.Features.SpiritBeadQuest SpiritBeadQ;
        #region Network

        public GameState(ClientWrapper socket)
        {
            Fake = socket == null;
            if (Fake) socket = new ClientWrapper() { Alive = true };
            Queue = new ConcurrentPacketQueue();
            PacketFilter = new PacketFilter() { { 10010, 10 }, { 10005, 7 }, { 2064, 4 }, { 2032, 3 }, { 1027, 2 } };
            Attackable = false;
            Action = 0;
            _socket = socket;

            Cryptography = new GameCryptography(Program.Encoding.GetBytes(Constants.GameCryptographyKey));
            if (Program.TestingMode)
                Cryptography = new GameCryptography(Program.Encoding.GetBytes(Constants.GameCryptographyKey));
            DHKeyExchange = new Network.GamePackets.DHKeyExchange.ServerKeyExchange();
            SpiritBeadQ = new Game.Features.SpiritBeadQuest(this);
            ChiPowers = new List<ChiPowerStructure>();
            Retretead_ChiPowers = new ChiPowerStructure[4];
            //JiangPowers = new List<JiangPowerStructure>();
        }
        public bool Ninja()
        {
            if (Entity.EntityFlag == Game.EntityFlag.Player)
            {
                if (Entity.Class >= 50 && Entity.Class <= 55)
                    return true;
                else
                    return false;
            }
            return false;
        }
        public void ReadyToPlay()
        {
            try
            {
                Weapons = new Tuple<ConquerItem, ConquerItem>(null, null);
                ItemSyncRoot = new object();
                Screen = new Game.Screen(this);
              //  if (!Program.ServerTransfer)
                {
                    Pet = new MaTrix.Pet(this);
                    AI = new MaTrix.AI(this);
                }
                Inventory = new Game.ConquerStructures.Inventory(this);
                Equipment = new Game.ConquerStructures.Equipment(this);
                WarehouseOpen = false;
                WarehouseOpenTries = 0;
                TempPassword = "";
                ArsenalDonations = new uint[10];
                if (Account != null)
                {
                    Warehouses = new SafeDictionary<Game.ConquerStructures.Warehouse.WarehouseID, Game.ConquerStructures.Warehouse>(20);
                    Warehouses.Add((MTA.Game.ConquerStructures.Warehouse.WarehouseID)this.Account.EntityID, new Game.ConquerStructures.Warehouse(this, (MTA.Game.ConquerStructures.Warehouse.WarehouseID)this.Account.EntityID, 200));
                    Warehouses.Add(MTA.Game.ConquerStructures.Warehouse.WarehouseID.TwinCity, new Game.ConquerStructures.Warehouse(this, MTA.Game.ConquerStructures.Warehouse.WarehouseID.TwinCity));
                    Warehouses.Add(MTA.Game.ConquerStructures.Warehouse.WarehouseID.PhoenixCity, new Game.ConquerStructures.Warehouse(this, MTA.Game.ConquerStructures.Warehouse.WarehouseID.PhoenixCity));
                    Warehouses.Add(MTA.Game.ConquerStructures.Warehouse.WarehouseID.ApeCity, new Game.ConquerStructures.Warehouse(this, MTA.Game.ConquerStructures.Warehouse.WarehouseID.ApeCity));
                    Warehouses.Add(MTA.Game.ConquerStructures.Warehouse.WarehouseID.DesertCity, new Game.ConquerStructures.Warehouse(this, MTA.Game.ConquerStructures.Warehouse.WarehouseID.DesertCity));
                    Warehouses.Add(MTA.Game.ConquerStructures.Warehouse.WarehouseID.BirdCity, new Game.ConquerStructures.Warehouse(this, MTA.Game.ConquerStructures.Warehouse.WarehouseID.BirdCity));
                    Warehouses.Add(MTA.Game.ConquerStructures.Warehouse.WarehouseID.StoneCity, new Game.ConquerStructures.Warehouse(this, MTA.Game.ConquerStructures.Warehouse.WarehouseID.StoneCity));
                    Warehouses.Add(MTA.Game.ConquerStructures.Warehouse.WarehouseID.Market, new Game.ConquerStructures.Warehouse(this, MTA.Game.ConquerStructures.Warehouse.WarehouseID.Market));
                    Warehouses.Add(MTA.Game.ConquerStructures.Warehouse.WarehouseID.Poker, new Game.ConquerStructures.Warehouse(this, MTA.Game.ConquerStructures.Warehouse.WarehouseID.Poker));
                    
                    if (Account != null)
                    {
                        if (!Warehouses.ContainsKey((MTA.Game.ConquerStructures.Warehouse.WarehouseID)Account.EntityID))
                            Warehouses.Add((Game.ConquerStructures.Warehouse.WarehouseID)Account.EntityID, new Game.ConquerStructures.Warehouse(this, (Game.ConquerStructures.Warehouse.WarehouseID)Account.EntityID));
                    }
                }
                Trade = new Game.ConquerStructures.Trade();
                ArenaStatistic = new ArenaStatistic(true);
                Prayers = new List<GameState>();
                map = null;
                SpiritBeadQ = new Game.Features.SpiritBeadQuest(this);
                Quests = new MaTrix.Quests(this);
                //JiangHuStatus = new JiangHuStatus();
            }
            catch (Exception e)
            {
                Program.SaveException(e);
            }
        }
        public void Send(byte[] buffer)
        {
            if (Fake) return;
            if (!_socket.Alive) return;
            ushort length = BitConverter.ToUInt16(buffer, 0);
            if (length >= 1024 && buffer.Length > length)
            {
                //Console.WriteLine(Environment.StackTrace);
                return;
            }
            byte[] _buffer = new byte[buffer.Length];
            if (length == 0)
                Writer.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
            Buffer.BlockCopy(buffer, 0, _buffer, 0, buffer.Length);
            Network.Writer.WriteString(Constants.ServerKey, _buffer.Length - 8, _buffer);
            try
            {
                lock (_socket)
                {
                    if (!_socket.Alive) return;
                    lock (Cryptography)
                    {
                        Cryptography.Encrypt(_buffer, _buffer.Length);
                        _socket.Send(_buffer);
                    }
                }
            }
            catch (Exception)
            {
                _socket.Alive = false;
                Disconnect();
            }
        }
        private void EndSend(IAsyncResult res)
        {
            try
            {
                _socket.Socket.EndSend(res);
            }
            catch
            {
                _socket.Alive = false;
                Disconnect();
            }
        }
        public void Send(Interfaces.IPacket buffer)
        {
            Send(buffer.ToArray());
        }
        public void SendScreenSpawn(Interfaces.IMapObject obj, bool self)
        {
            try
            {
                foreach (Interfaces.IMapObject _obj in Screen.Objects)
                {
                    if (_obj == null)
                        continue;
                    if (_obj.UID != Entity.UID)
                    {
                        if (_obj.MapObjType == Game.MapObjectType.Player)
                        {
                            GameState client = _obj.Owner as GameState;
                            obj.SendSpawn(client, false);
                        }
                    }
                }
                if (self)
                    obj.SendSpawn(this);
            }
            catch (Exception e)
            {
                Program.SaveException(e);
            }
        }
        public void RemoveScreenSpawn(Interfaces.IMapObject obj, bool self)
        {
            try
            {
                if (Screen == null) return;
                if (Screen.Objects == null) return;
                foreach (Interfaces.IMapObject _obj in Screen.Objects)
                {
                    if (_obj == null) continue;
                    if (obj == null) continue;
                    if (_obj.UID != Entity.UID)
                    {
                        if (_obj.MapObjType == Game.MapObjectType.Player)
                        {
                            GameState client = _obj.Owner as GameState;
                            client.Screen.Remove(obj);
                        }
                    }
                }
                if (self)
                    Screen.Remove(obj);
            }
            catch (Exception e)
            {
                Program.SaveException(e);
            }
        }
        public void SendScreen(byte[] buffer, bool self = true)
        {
            try
            {
                foreach (Interfaces.IMapObject obj in Screen.Objects)
                {
                    if (obj == null) continue;
                    if (obj.UID != Entity.UID)
                    {
                        if (obj.MapObjType == Game.MapObjectType.Player)
                        {
                            GameState client = obj.Owner as GameState;
                            if (WatchingGroup != null && client.WatchingGroup == null)
                                continue;
                            client.Send(buffer);
                        }
                    }
                }
                if (self)
                    Send(buffer);
            }
            catch (Exception e)
            {
                Program.SaveException(e);
            }
        }
        public void SendScreen(Interfaces.IPacket buffer, bool self = true, bool message = false)
        {
            if (Screen == null) return;
            if (Screen.Objects == null) return;
            foreach (Interfaces.IMapObject obj in Screen.Objects)
            {
                if (obj == null)
                    continue;
                if (obj.MapObjType == Game.MapObjectType.Player)
                {
                    GameState client = obj.Owner as GameState;
                    if (message && client.Entity.BlackList.Contains(Entity.Name)) continue;
                    if (client.Entity.UID != Entity.UID)
                        client.Send(buffer);
                }
            }
            if (self)
                Send(buffer);
        }
        public void Disconnect(bool save = true)
        {
            if (Fake) return;
            if (Screen != null) Screen.DisposeTimers();
            PacketHandler.RemoveTPA(this);
            Program.World.Unregister(this);
            if (OnDisconnect != null) OnDisconnect(this);
            if (_socket.Connector != null)
            {
               
                _socket.Disconnect();
                ShutDown();
            }
        }

        private void ShutDown()
        {

            if (Socket.Connector == null) return;
            Socket.Connector = null;
            if (this.Entity != null)
            {
                try
                {
                    if (this.Entity.JustCreated) return;
                    #region Poker
                    if (Entity.PokerTable != null)
                    {
                        var T = Entity.PokerTable;
                        if (T != null)
                            if (T.Players.ContainsKey(Entity.UID) && T.Pot > 1)
                            {
                                T.StopMoveCountDown();
                                T.RemovePlayer(Entity.UID);
                            }
                            else
                                T.RemovePlayer(Entity.UID);
                    }
                    #endregion
                    Time32 now = Time32.Now;
                    Kernel.DisconnectPool.Add(this.Entity.UID, this);
                    RemoveScreenSpawn(this.Entity, false);
                    if (Entity != null && Entity.WTitles != null)
                        Entity.WTitles.Update(); 
                    using (var conn = Database.DataHolder.MySqlConnection)
                    {
                        conn.Open();
                        Database.EntityTable.UpdateOnlineStatus(this, false, conn);
                        InnerPowerTable.Save();
                        Database.EntityTable.SaveEntity(this, conn);   
                        if (!TransferedPlayer)
                            Database.EntityVariableTable.Save(this, conn);
                        Database.SkillTable.SaveProficiencies(this, conn);
                        Database.SkillTable.SaveSpells(this, conn);
                        if (!TransferedPlayer)
                        {
                            Database.ArenaTable.SaveArenaStatistics(this.ArenaStatistic, conn);
                            Database.TeamArenaTable.SaveArenaStatistics(this.TeamArenaStatistic, conn);
                            Database.ChampionTable.SaveStatistics(this.ChampionStats, conn);
                        }
                    }
                    foreach (var kerO in Entity.StorageItems)
                    {
                        ConquerItemTable.UpdateWardrobe(true, kerO.Key);
                    }
                    Kernel.GamePool.Remove(this.Entity.UID);

   

                    if (Booth != null)
                        Booth.Remove();

                    if (Entity.MyClones.Count > 0)
                    {
                        foreach (var item in Entity.MyClones.Values)
                        {                            
                            Data data = new Data(true);
                            data.UID = item.UID;
                            data.ID = Network.GamePackets.Data.RemoveEntity;
                            item.MonsterInfo.SendScreen(data);
                        }
                        Entity.MyClones.Clear();
                    }
                    if (Quests != null)
                        Quests.Save();

                    if (Pet != null)
                        Pet.ClearAll();
                    if (QualifierGroup != null)
                        QualifierGroup.End(this);
                    if (TeamQualifierGroup != null)
                        TeamQualifierGroup.CheckEnd(this, true);
                    if (Entity.CLanArenaBattleFight != null)
                        Entity.CLanArenaBattleFight.CheakToEnd(this, true);
                    if (Entity.GuildArenaBattleFight != null)
                        Entity.GuildArenaBattleFight.CheakToEnd(this, true);
                    if (ChampionGroup != null)
                        ChampionGroup.End(this);
                    if (Challenge != null)
                        Challenge.End(this);

                   

                    Game.Arena.Clear(this);
                    Game.TeamArena.Clear(this);
                    Game.Champion.Clear(this);

                    RemoveScreenSpawn(this.Entity, false);
                    
                    #region Friend/TradePartner/Apprentice
                    Message msg = new Message("Your friend, " + Entity.Name + ", has logged off.", System.Drawing.Color.Red, Message.TopLeft);
                    if (Friends == null)
                        Friends = new SafeDictionary<uint, MTA.Game.ConquerStructures.Society.Friend>(100);
                    foreach (Game.ConquerStructures.Society.Friend friend in Friends.Values)
                    {
                        if (friend.IsOnline)
                        {
                            var packet = new MTA.Network.GamePackets.KnownPersons(true)
                            {
                                UID = Entity.UID,
                                Type = Network.GamePackets.KnownPersons.RemovePerson,
                                Name = Entity.Name,
                                Online = false
                            };
                            friend.Client.Send(packet);
                            packet.Type = Network.GamePackets.KnownPersons.AddFriend;
                            if (friend != null)
                            {
                                if (friend.Client != null)
                                {
                                    friend.Client.Send(packet);
                                    friend.Client.Send(msg);
                                }
                            }
                        }
                    }
                    Message msg2 = new Message("Your partner, " + Entity.Name + ", has logged off.", System.Drawing.Color.Red, Message.TopLeft);

                    if (Partners != null)
                    {
                        foreach (Game.ConquerStructures.Society.TradePartner partner in Partners.Values)
                        {
                            if (partner.IsOnline)
                            {
                                var packet = new TradePartner(true)
                                {
                                    UID = Entity.UID,
                                    Type = TradePartner.BreakPartnership,
                                    Name = Entity.Name,
                                    HoursLeft = (int)(new TimeSpan(partner.ProbationStartedOn.AddDays(3).Ticks).TotalHours - new TimeSpan(DateTime.Now.Ticks).TotalHours),
                                    Online = false
                                };
                                partner.Client.Send(packet);
                                packet.Type = TradePartner.AddPartner;
                                if (partner != null)
                                {
                                    if (partner.Client != null)
                                    {
                                        partner.Client.Send(packet);
                                        partner.Client.Send(msg2);
                                    }
                                }
                            }
                        }
                    }
                    MentorInformation Information = new MentorInformation(true);
                    Information.Mentor_Type = 1;
                    Information.Mentor_ID = Entity.UID;
                    Information.Mentor_Level = Entity.Level;
                    Information.Mentor_Class = Entity.Class;
                    Information.Mentor_PkPoints = Entity.PKPoints;
                    Information.Mentor_Mesh = Entity.Mesh;
                    Information.Mentor_Online = false;
                    Information.String_Count = 3;
                    Information.Mentor_Name = Entity.Name;
                    Information.Mentor_Spouse_Name = Entity.Spouse;
                    if (Apprentices == null) Apprentices = new SafeDictionary<uint, Game.ConquerStructures.Society.Apprentice>();
                    foreach (var appr in Apprentices.Values)
                    {
                        if (appr.IsOnline)
                        {
                            Information.Apprentice_ID = appr.ID;
                            Information.Enrole_Date = appr.EnroleDate;
                            Information.Apprentice_Name = appr.Name;
                            appr.Client.Send(Information);
                            appr.Client.ReviewMentor();
                        }
                    }
                    if (Mentor != null)
                    {
                        if (Mentor.IsOnline)
                        {
                            ApprenticeInformation AppInfo = new ApprenticeInformation();
                            AppInfo.Apprentice_ID = Entity.UID;
                            AppInfo.Apprentice_Level = Entity.Level;
                            AppInfo.Apprentice_Name = Entity.Name;
                            AppInfo.Apprentice_Online = false;
                            AppInfo.Apprentice_Spouse_Name = Entity.Spouse;
                            AppInfo.Enrole_date = Mentor.EnroleDate;
                            AppInfo.Mentor_ID = Mentor.Client.Entity.UID;
                            AppInfo.Mentor_Mesh = Mentor.Client.Entity.Mesh;
                            AppInfo.Mentor_Name = Mentor.Client.Entity.Name;
                            AppInfo.Type = 2;
                            Mentor.Client.Send(AppInfo);
                        }
                    }

                    #endregion
                    #region Team
                   /* if (Team != null)
                    {
                        if (Team.TeamLeader)
                        {
                            Network.GamePackets.Team team = new Network.GamePackets.Team();
                            team.UID = Account.EntityID;
                            team.Type = Network.GamePackets.Team.Dismiss;
                            foreach (Client.GameState Teammate in Team.Teammates)
                            {
                                if (Teammate != null)
                                {
                                    if (Teammate.Entity.UID != Account.EntityID)
                                    {
                                        Teammate.Send(team);
                                        Teammate.Team = null;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Network.GamePackets.Team team = new Network.GamePackets.Team();
                            team.UID = Account.EntityID;
                            team.Type = Network.GamePackets.Team.ExitTeam;
                            foreach (Client.GameState Teammate in Team.Teammates)
                            {
                                if (Teammate != null)
                                {
                                    if (Teammate.Entity.UID != Account.EntityID)
                                    {
                                        Teammate.Send(team);
                                        Teammate.Team.Remove(this);
                                    }
                                }
                            }
                        }
                    }*/
                    #endregion
                    if (Team != null)
                    {
                        Team.Remove(this, true);
                    }
                    foreach (var item in Entity.StorageItems.Values)
                    {
                        if (!item.InWardrobe)
                        {
                            item.InWardrobe = true;
                            Database.ConquerItemTable.UpdateWardrobe(item.InWardrobe, item.UID);
                        }
                    }
                }
                catch (Exception e)
                {
                    Program.SaveException(e);
                }
                finally
                {
                    Kernel.DisconnectPool.Remove(this.Entity.UID);
                    Console.WriteLine(this.Entity.Name + " logged out. IP: " + this.Account.IP + "  ");
                    new Database.MySqlCommand(Database.MySqlCommandType.UPDATE).Update("configuration").Set("LastPlayer", Entity.Name).Set("login", "has logged off").Execute();
                }
            }
        }

        public ClientWrapper Socket { get { return _socket; } }
        public string IP
        {
            get
            {
                return _socket.IP;
            }
        }
        #endregion

        #region Game

        public Database.ChiTable.ChiData ChiData;
        public List<ChiPowerStructure> ChiPowers;
        public ChiPowerStructure[] Retretead_ChiPowers;
        public uint ChiPoints = 0;

        //public List<JiangPowerStructure> JiangPowers;
        //public JiangHuStatus JiangHuStatus;
        //public JiangHu JiangHu;
        public SafeDictionary<uint, DetainedItem> ClaimableItem = new SafeDictionary<uint, DetainedItem>(1000),
                                                  DeatinedItem = new SafeDictionary<uint, DetainedItem>(1000);

        public bool DoSetOffline = true;

        public ushort OnlineTrainingPoints = 0;
        public Time32 LastTrainingPointsUp, LastTreasurePoints = Time32.Now.AddMinutes(1);

        public List<string> GuildNamesSpawned = new List<string>();

        public byte KylinUpgradeCount = 0;

        public ulong OblivionExperience = 0;
        public byte OblivionKills = 0;

        public int PremShopType = 0;
        public DateTime VIPDate;
        public DateTime LastVote;
        public uint VIPDays;
        public uint DonationPoints;
        //  public uint VotePoints;
        #region Colo
        public static uint ScreenColor = 0;
        #region Night Color

        public void Night()
        {
            ScreenColor = 5855577;

            Network.GamePackets.ScreenColor Packet = new Network.GamePackets.ScreenColor(true);
            Packet.UID = this.Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values)
            {
                pclient.Send(Packet);
            }
        }
        public void Night1()
        {
            ScreenColor = 3358767;

            Network.GamePackets.ScreenColor Packet = new Network.GamePackets.ScreenColor(true);
            Packet.UID = this.Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values)
            {
                pclient.Send(Packet);
            }
        }
        public void Night2()
        {
            ScreenColor = 97358;

            Network.GamePackets.ScreenColor Packet = new Network.GamePackets.ScreenColor(true);
            Packet.UID = this.Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values)
            {
                pclient.Send(Packet);
            }
        }

        #endregion
        #region Blue Color

        public void Blue()
        {
            ScreenColor = 69852;

            Network.GamePackets.ScreenColor Packet = new Network.GamePackets.ScreenColor(true);
            Packet.UID = this.Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values)
            {
                pclient.Send(Packet);
            }
        }
        public void Blue1()
        {
            ScreenColor = 4532453;

            Network.GamePackets.ScreenColor Packet = new Network.GamePackets.ScreenColor(true);
            Packet.UID = this.Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values)
            {
                pclient.Send(Packet);
            }
        }
        public void Blue2()
        {
            ScreenColor = 684533;

            Network.GamePackets.ScreenColor Packet = new Network.GamePackets.ScreenColor(true);
            Packet.UID = this.Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values)
            {
                pclient.Send(Packet);
            }
        }

        #endregion
        #region Green Color

        public void Green()
        {
            ScreenColor = 838915;

            Network.GamePackets.ScreenColor Packet = new Network.GamePackets.ScreenColor(true);
            Packet.UID = this.Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values)
            {
                pclient.Send(Packet);
            }
        }
        public void Green1()
        {
            ScreenColor = 824383;

            Network.GamePackets.ScreenColor Packet = new Network.GamePackets.ScreenColor(true);
            Packet.UID = this.Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values)
            {
                pclient.Send(Packet);
            }
        }
        public void Green2()
        {
            ScreenColor = 456828;

            Network.GamePackets.ScreenColor Packet = new Network.GamePackets.ScreenColor(true);
            Packet.UID = this.Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values)
            {
                pclient.Send(Packet);
            }
        }
        public void Green3()
        {
            ScreenColor = 5547633;

            Network.GamePackets.ScreenColor Packet = new Network.GamePackets.ScreenColor(true);
            Packet.UID = this.Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values)
            {
                pclient.Send(Packet);
            }
        }
        public void Green4()
        {
            ScreenColor = 453450;

            Network.GamePackets.ScreenColor Packet = new Network.GamePackets.ScreenColor(true);
            Packet.UID = this.Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values)
            {
                pclient.Send(Packet);
            }
        }

        #endregion
        #region Day Color

        public void Day()
        {
            ScreenColor = 0;

            Network.GamePackets.ScreenColor Packet = new Network.GamePackets.ScreenColor(true);
            Packet.UID = this.Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values)
            {
                pclient.Send(Packet);
            }
        }

        #endregion
        #endregion
        public Time32 ScreenReloadTime;
        public int MillisecondsScreenReload;
        public bool Reloaded = false;
        public Interfaces.IPacket ReloadWith;

        public ushort VendingDisguise;
        //public uint BlessTime;
        public uint BlessTime
        {
            get { return this["BlessTime"]; }
            set { this["BlessTime"] = value; }
        }
        public DateTime BlessStamp
        {
            get { return this["BlessStamp"]; }
            set { this["BlessStamp"] = value; }
        }
        public DateTime DoubleExperienceStamp
        {
            get { return this["DoubleExperienceStamp"]; }
            set { this["DoubleExperienceStamp"] = value; }
        }
        public int speedHackSuspiction = 0;
        public Time32 LastPingT;
        public uint LastPingStamp = 0;
       // public Game.Entity Companion;

        public List<GameState> Prayers;
        public GameState PrayLead;

        public DateTime ChatBanTime;
        public uint ChatBanLasts;
        public bool ChatBanned;

        public uint BackupArmorLook
        {
            get { return this["bkparmorlook"]; }
            set { this["bkparmorlook"] = value; }
        }
        public uint ArmorLook
        {
            get { return this["armorlook"]; }
            set { this["armorlook"] = value; }
        }
        public uint WeaponLook
        {
            get { return this["weaponlook"]; }
            set { this["weaponlook"] = value; }
        }
        public uint WeaponLook2
        {
            get { return this["weaponlook2"]; }
            set { this["weaponlook2"] = value; }
        }
        public uint HeadgearLook
        {
            get { return this["headgearlook"]; }
            set { this["headgearlook"] = value; }
        }

        public bool ValidArmorLook(uint id)
        {
            if (id == 0) return false;
            
            var soulInfo = Database.AddingInformationTable.SoulGearItems[id];
            if (id >= 800000 && id < 900000)
            {
                if (soulInfo.ItemIdentifier < 100)
                    if (soulInfo.ItemIdentifier != ConquerItem.Armor)
                        return false;
                    else { }
                else
                    if (Network.PacketHandler.ItemPosition((uint)(soulInfo.ItemIdentifier * 1000)) != ConquerItem.Armor)
                        return false;
            }
            else
                if (Network.PacketHandler.ItemPosition(id) != ConquerItem.Armor)
                    return false;
            return true;
        }
        public bool ValidHeadgearLook(uint id)
        {
            if (id == 0) return false;
           
            var soulInfo = Database.AddingInformationTable.SoulGearItems[id];
            if (id >= 800000 && id < 900000)
            {
                if (soulInfo.ItemIdentifier < 100)
                    if (soulInfo.ItemIdentifier != ConquerItem.Head)
                        return false;
                    else { }
                else
                    if (Network.PacketHandler.ItemPosition((uint)(soulInfo.ItemIdentifier * 1000)) != ConquerItem.Head)
                        return false;
            }
            else
                if (Network.PacketHandler.ItemPosition(id) != ConquerItem.Head)
                    return false;
            return true;
        }
        public bool ValidWeaponLook(uint id)
        {
            if (id == 0) return false;
                if (Network.PacketHandler.ItemPosition(id) != ConquerItem.RightWeapon)
                    return false;
            return true;
        }
        public bool ValidWeaponLook2(uint id)
        {
            if (id == 0) return false;
            if (Network.PacketHandler.ItemPosition(id) != ConquerItem.RightWeapon)
            {
                if (Network.PacketHandler.ItemPosition(id) != ConquerItem.LeftWeapon)
                    return false;
            }
            else
            {
                if (Network.PacketHandler.IsTwoHand(id))
                    return false;
            }            
            return true;
        }

        public ConquerItemBaseInformation CheckLook(string name, ushort pos, out int minDist)
        {
            minDist = int.MaxValue;
            Database.ConquerItemBaseInformation CIBI = null;
            Game.Enums.ItemQuality Quality = Game.Enums.ItemQuality.Fixed;
            var itemx = Equipment.TryGetItem((byte)pos);
            if (itemx != null)
                Quality = (Enums.ItemQuality)(itemx.ID % 10);          
            
            foreach (var item in Database.ConquerItemInformation.BaseInformations.Values)
            {
                if (pos == ConquerItem.Armor)
                {
                    if (ValidArmorLook(item.ID))
                    {
                        int dist = name.LevenshteinDistance(item.LowerName);
                        if (minDist > dist && Quality == (Game.Enums.ItemQuality)(item.ID % 10))
                        {
                            CIBI = item;
                            minDist = dist;
                        }
                    }
                }
                else if (pos == ConquerItem.Head)
                {
                    if (ValidHeadgearLook(item.ID))
                    {
                        int dist = name.LevenshteinDistance(item.LowerName);
                        if (minDist > dist && Quality == (Game.Enums.ItemQuality)(item.ID % 10))
                        {
                            CIBI = item;
                            minDist = dist;
                        }
                    }
                }
                else if (pos == ConquerItem.LeftWeapon)
                {
                    if (ValidWeaponLook2(item.ID))
                    {
                        int dist = name.LevenshteinDistance(item.LowerName);
                        if (minDist > dist && !PacketHandler.IsTwoHand(item.ID) && Quality == (Game.Enums.ItemQuality)(item.ID % 10))
                        {
                            CIBI = item;
                            minDist = dist;
                        }
                    }
                }
                else if (pos == ConquerItem.RightWeapon)
                {
                    if (ValidWeaponLook(item.ID))
                    {
                        //if (PacketHandler.IsTwoHand(itemx.ID))
                        //{
                        //    int dist = name.LevenshteinDistance(item.LowerName);
                        //    if (minDist > dist && PacketHandler.IsTwoHand(item.ID) && Quality == (Game.Enums.ItemQuality)(item.ID % 10))
                        //    {
                        //        CIBI = item;
                        //        minDist = dist;
                        //    }
                        //}
                        //else
                        {
                            int dist = name.LevenshteinDistance(item.LowerName);
                            if (minDist > dist && !PacketHandler.IsTwoHand(item.ID) && Quality == (Game.Enums.ItemQuality)(item.ID % 10))
                            {
                                CIBI = item;
                                minDist = dist;
                            }
                        }

                    }
                }
            }
            return CIBI;
        }

        public void SetNewArmorLook(uint id, bool change = true)
        {
            if (change)
                ArmorLook = id;
            if (!ValidArmorLook(id)) return;
            int min = 0;
            id = CheckLook(Database.ConquerItemInformation.BaseInformations[id].LowerName, ConquerItem.Armor, out min).ID;

            var item = Equipment.TryGetItem(ConquerItem.Armor);
            var iu = new Network.GamePackets.ItemUsage(true);
            iu.UID = uint.MaxValue - 1;
            iu.dwParam = 13;
            iu.ID = Network.GamePackets.ItemUsage.UnequipItem;
            Send(iu);
            iu = new Network.GamePackets.ItemUsage(true);
            iu.UID = uint.MaxValue - 1;
            iu.ID = Network.GamePackets.ItemUsage.RemoveInventory;
            Send(iu);

            ConquerItem fakeItem = new Network.GamePackets.ConquerItem(true);
            fakeItem.ID = id;
            if (item != null)
            {
                fakeItem.Bless = item.Bless;
                fakeItem.Bound = item.Bound;
                fakeItem.Color = item.Color;
                fakeItem.Effect = item.Effect;
                fakeItem.Enchant = item.Enchant;
                fakeItem.Plus = item.Plus;
                fakeItem.SocketOne = item.SocketOne;
                fakeItem.SocketTwo = item.SocketTwo;
            }

            fakeItem.Durability = 1;
            fakeItem.MaximDurability = 1;
            fakeItem.Color = Game.Enums.Color.Black;
            fakeItem.UID = uint.MaxValue - 1;
            fakeItem.Position = 13;
            Send(fakeItem);
            fakeItem.Mode = Enums.ItemMode.Update;
            Send(fakeItem);
            ClientEquip eqs = new ClientEquip();
            eqs.DoEquips(this);
            Send(eqs);
            Equipment.UpdateEntityPacket();
        }
        public void SetNewHeadgearLook(uint id, bool change = true)
        {
            if (change)
                HeadgearLook = id;
            if (!ValidHeadgearLook(id)) return;
            int min = 0;
            id = CheckLook(Database.ConquerItemInformation.BaseInformations[id].LowerName, ConquerItem.Head, out min).ID;

            var item = Equipment.TryGetItem(ConquerItem.Head);
            var iu = new Network.GamePackets.ItemUsage(true);
            iu.UID = uint.MaxValue - 2;
            iu.dwParam = 14;
            iu.ID = Network.GamePackets.ItemUsage.UnequipItem;
            Send(iu);
            iu = new Network.GamePackets.ItemUsage(true);
            iu.UID = uint.MaxValue - 2;
            iu.ID = Network.GamePackets.ItemUsage.RemoveInventory;
            Send(iu);

            ConquerItem fakeItem = new Network.GamePackets.ConquerItem(true);
            fakeItem.ID = id;
            if (item != null)
            {
                fakeItem.Bless = item.Bless;
                fakeItem.Bound = item.Bound;
                fakeItem.Color = item.Color;
                fakeItem.Effect = item.Effect;
                fakeItem.Enchant = item.Enchant;
                fakeItem.Plus = item.Plus;
                fakeItem.SocketOne = item.SocketOne;
                fakeItem.SocketTwo = item.SocketTwo;
            }

            fakeItem.Durability = 1;
            fakeItem.MaximDurability = 1;
            fakeItem.Color = Game.Enums.Color.Black;
            fakeItem.UID = uint.MaxValue - 2;
            fakeItem.Position = 14;
            Send(fakeItem);
            fakeItem.Mode = Enums.ItemMode.Update;
            Send(fakeItem);
            ClientEquip eqs = new ClientEquip();
            eqs.DoEquips(this);
            Send(eqs);
            Equipment.UpdateEntityPacket();
        }
        public void SetNewWeaponLook(uint id, bool change = true)
        {
            if (change)
                WeaponLook = id;
            if (!ValidWeaponLook(id)) return;
            int min = 0;
           
            var item = Equipment.TryGetItem(ConquerItem.RightWeapon);
            var iu = new Network.GamePackets.ItemUsage(true);
            iu.UID = uint.MaxValue - 3;
            iu.dwParam = ConquerItem.RightWeaponAccessory;
            iu.ID = Network.GamePackets.ItemUsage.UnequipItem;
            Send(iu);
            iu = new Network.GamePackets.ItemUsage(true);
            iu.UID = uint.MaxValue - 3;
            iu.ID = Network.GamePackets.ItemUsage.RemoveInventory;
            Send(iu);

            id = CheckLook(Database.ConquerItemInformation.BaseInformations[id].LowerName, ConquerItem.RightWeapon, out min).ID;
          
            ConquerItem fakeItem = new Network.GamePackets.ConquerItem(true);
            fakeItem.ID = id;
            if (item != null)
            {
                fakeItem.Bless = item.Bless;
                fakeItem.Bound = item.Bound;
                fakeItem.Color = item.Color;
                fakeItem.Effect = item.Effect;
                fakeItem.Enchant = item.Enchant;
                fakeItem.Plus = item.Plus;
                fakeItem.SocketOne = item.SocketOne;
                fakeItem.SocketTwo = item.SocketTwo;
                fakeItem.Lock = 1;
            }
            fakeItem.Durability = 1;
            fakeItem.MaximDurability = 1;         
            fakeItem.UID = uint.MaxValue - 3;
            fakeItem.Position = ConquerItem.RightWeaponAccessory;
            Send(fakeItem);
            fakeItem.Mode = Enums.ItemMode.Update;
            Send(fakeItem);
            ClientEquip eqs = new ClientEquip();            
            eqs.DoEquips(this);
            Send(eqs);
            Equipment.UpdateEntityPacket();
            
        }
        public void SetNewWeaponLook2(uint id, bool change = true)
        {
            if (change)
                WeaponLook2 = id;
            if (!ValidWeaponLook2(id)) return;
            int min = 0;

            var item = Equipment.TryGetItem(ConquerItem.LeftWeapon);
            var iu = new Network.GamePackets.ItemUsage(true);
            iu.UID = uint.MaxValue - 4;
            iu.dwParam = ConquerItem.LeftWeaponAccessory;
            iu.ID = Network.GamePackets.ItemUsage.UnequipItem;
            Send(iu);
            iu = new Network.GamePackets.ItemUsage(true);
            iu.UID = uint.MaxValue - 4;
            iu.ID = Network.GamePackets.ItemUsage.RemoveInventory;
            Send(iu);

            id = CheckLook(Database.ConquerItemInformation.BaseInformations[id].LowerName, ConquerItem.LeftWeapon, out min).ID;

            ConquerItem fakeItem = new Network.GamePackets.ConquerItem(true);
            fakeItem.ID = id;
            if (item != null)
            {
                fakeItem.Bless = item.Bless;
                fakeItem.Bound = item.Bound;
                fakeItem.Color = item.Color;
                fakeItem.Effect = item.Effect;
                fakeItem.Enchant = item.Enchant;
                fakeItem.Plus = item.Plus;
                fakeItem.SocketOne = item.SocketOne;
                fakeItem.SocketTwo = item.SocketTwo;
                fakeItem.Lock = 1;
            }

            fakeItem.Durability = 1;
            fakeItem.MaximDurability = 1;
            fakeItem.UID = uint.MaxValue - 4;
            fakeItem.Position = ConquerItem.LeftWeaponAccessory;
            Send(fakeItem);
            fakeItem.Mode = Enums.ItemMode.Update;
            Send(fakeItem);
            ClientEquip eqs = new ClientEquip();            
            eqs.DoEquips(this);
            Send(eqs);
            Equipment.UpdateEntityPacket();

        }

        public byte JewelarLauKind, JewelarLauGems;
        public uint VirtuePoints;
        public DateTime LastLotteryEntry;
        public byte LotteryEntries;
        public bool InLottery;
        public DateTime OfflineTGEnterTime;
        public bool Mining = false;
        public Time32 MiningStamp;
        public ushort Vigor
        {
            get
            {
                if (Equipment != null)
                    if (!Equipment.Free(12))
                        return Equipment.TryGetItem((byte)12).Vigor;
                return 65535;
            }
            set
            {
                if (!Equipment.Free(12))
                    Equipment.TryGetItem((byte)12).Vigor = value;
            }
        }
        ushort _Maxvigor;
        public ushort MaxVigor
        {
            get
            {
                return _Maxvigor;
            }
            set
            {
                _Maxvigor = value;
            }

        }      
           

        public bool HeadgearClaim, NecklaceClaim, ArmorClaim, WeaponClaim, RingClaim, BootsClaim, TowerClaim, FanClaim;
        public string PromoteItemNameNeed
        {
            get
            {
                if (Entity.Class % 10 == 0)
                    return " nothing but";
                if (Entity.Class % 10 == 1)
                    //   if (Entity.Class / 10 == 4)
                    //      return " five Euxenite Ores and";
                    //    else
                    return " nothing but";
                if (Entity.Class % 10 == 2)
                    return " one Emerald and";
                if (Entity.Class % 10 == 3)
                    return " one Meteor and";
                if (Entity.Class % 10 == 4)
                    return " one MoonBox and";
                return " nothing but";
            }
        }
        public byte PromoteItemCountNeed
        {
            get
            {
                if (Entity.Class % 10 == 0)
                    return 0;
                if (Entity.Class % 10 == 1)
                    //  if (Entity.Class / 10 == 4)
                    //       return 5;
                    //   else
                    return 0;
                if (Entity.Class % 10 == 2)
                    return 1;
                if (Entity.Class % 10 == 3)
                    return 1;
                if (Entity.Class % 10 == 4)
                    return 1;
                return 0;
            }
        }
        public uint PromoteItemNeed
        {
            get
            {
                if (Entity.Class % 10 == 0)
                    return 0;
                if (Entity.Class % 10 == 1)
                    //   if (Entity.Class / 10 == 4)
                    //       return 1072031;
                    //   else
                    return 0;
                if (Entity.Class % 10 == 2)
                    return 1080001;
                if (Entity.Class % 10 == 3)
                    return 1088001;
                if (Entity.Class % 10 == 4)
                    return 721020;
                return 0;
            }
        }
        public uint PromoteItemGain
        {
            get
            {
                if (Entity.Class % 10 == 0)
                    return 0;
                if (Entity.Class % 10 == 1)
                    //   if (Entity.Class / 10 == 4)
                    //       return 500067;
                    //    else
                    return 0;
                if (Entity.Class % 10 == 2)
                    return 0;
                if (Entity.Class % 10 == 3)
                    return 700031;
                if (Entity.Class % 10 == 4)
                    return 1088000;
                return 0;
            }
        }
        public uint PromoteLevelNeed
        {
            get
            {
                if (Entity.Class % 10 == 0)
                    return 15;
                if (Entity.Class % 10 == 1)
                    return 40;
                if (Entity.Class % 10 == 2)
                    return 70;
                if (Entity.Class % 10 == 3)
                    return 100;
                if (Entity.Class % 10 == 4)
                    return 110;
                return 0;
            }
        }
        public byte SelectedItem, UpdateType;
        public ushort UplevelProficiency;
        public UInt32 GuildJoinTarget = 0;
        public uint OnHoldGuildJoin = 0;
        public uint elitepoints = 0;
        public bool Effect = false;
        public bool Effect1 = false;
        public bool Effect10 = false;
        public bool Effect8 = false;
        public bool Effect9 = false;
        public bool Effect11 = false;
        public bool Effect4 = false;
        public bool Effect3 = false;
        public uint eliterank = 0;
        public bool SentRequest = false;
        public Game.ConquerStructures.Society.Guild Guild;
        public Game.ConquerStructures.Society.Guild.Member AsMember;
        public uint Arsenal_Donation = 0;
        public Game.ConquerStructures.Booth Booth;


        public bool RaceExcitement, RaceDecelerated, RaceGuard, RaceDizzy, RaceFrightened;
        public Time32 RaceExcitementStamp, GuardStamp, DizzyStamp, FrightenStamp, ExtraVigorStamp, DecelerateStamp;
        public uint RaceExcitementAmount, RaceExtraVigor;
        public GameCharacterUpdates SpeedChange;
        public void ApplyRacePotion(Enums.RaceItemType type, uint target)
        {
            switch (type)
            {
                case Enums.RaceItemType.FrozenTrap:
                    {
                        if (target != uint.MaxValue)
                        {
                            if (Map.Floor[Entity.X, Entity.Y, MapObjectType.StaticEntity])
                            {
                                StaticEntity item = new StaticEntity((uint)(Entity.X * 1000 + Entity.Y), Entity.X, Entity.Y, (ushort)Map.ID);
                                item.DoFrozenTrap(Entity.UID);
                                Map.AddStaticEntity(item);
                                Kernel.SendSpawn(item);
                            }
                        }
                        else
                        {
                            Entity.FrozenStamp = Time32.Now;
                            Entity.FrozenTime = 5;
                            GameCharacterUpdates update = new GameCharacterUpdates(true);
                            update.UID = Entity.UID;
                            update.Add(GameCharacterUpdates.Freeze, 0, 4);
                            SendScreen(update, true);
                            Entity.AddFlag(Update.Flags.Freeze);
                        }
                        break;
                    }
                case Enums.RaceItemType.RestorePotion:
                    {
                        Vigor += 2000;
                        if (Vigor > MaxVigor)
                            Vigor = MaxVigor;
                        Send(new Vigor(true) { Amount = Vigor });
                        break;
                    }
                case Enums.RaceItemType.ExcitementPotion:
                    {
                        if (RaceExcitement && RaceExcitementAmount > 50)
                            return;

                        if (RaceDecelerated)
                        {
                            RaceDecelerated = false;

                            var upd = new GameCharacterUpdates(true);
                            upd.UID = Entity.UID;
                            upd.Remove(GameCharacterUpdates.Decelerated);
                            SendScreen(upd, true);
                        }
                        RaceExcitementStamp = Time32.Now;
                        RaceExcitement = true;
                        {
                            var upd = new GameCharacterUpdates(true);
                            upd.UID = Entity.UID;
                            upd.Add(GameCharacterUpdates.Accelerated, 50, 15, 25);
                            SendScreen(upd, true);
                            SpeedChange = upd;
                        }
                        RaceExcitementAmount = 50;
                        Entity.AddFlag(Update.Flags.OrangeSparkles);
                        break;
                    }
                case Enums.RaceItemType.SuperExcitementPotion:
                    {
                        if (RaceDecelerated)
                        {
                            RaceDecelerated = false;

                            var upd = new GameCharacterUpdates(true);
                            upd.UID = Entity.UID;
                            upd.Remove(GameCharacterUpdates.Decelerated);
                            SendScreen(upd, true);
                        }
                        RaceExcitementAmount = 200;
                        RaceExcitementStamp = Time32.Now;
                        RaceExcitement = true;
                        this.Entity.AddFlag(Update.Flags.SpeedIncreased);
                        {
                            var upd = new GameCharacterUpdates(true);
                            upd.UID = Entity.UID;
                            upd.Add(GameCharacterUpdates.Accelerated, 200, 15, 100);
                            SendScreen(upd, true);
                            SpeedChange = upd;
                        }
                        Entity.AddFlag(Update.Flags.OrangeSparkles);
                        break;
                    }
                case Enums.RaceItemType.GuardPotion:
                    {
                        RaceGuard = true;
                        GuardStamp = Time32.Now;
                        Entity.AddFlag(Update.Flags.DivineShield);
                        DizzyStamp = DizzyStamp.AddSeconds(-100);
                        FrightenStamp = FrightenStamp.AddSeconds(-100);
                        var upd = new GameCharacterUpdates(true);
                        upd.UID = Entity.UID;
                        upd.Add(GameCharacterUpdates.DivineShield, 0, 10);
                        SendScreen(upd, true);
                        break;
                    }
                case Enums.RaceItemType.DizzyHammer:
                    {
                        Entity Target;
                        if (Screen.TryGetValue(target, out Target))
                        {
                            var Owner = Target.Owner;
                            if (Owner != null)
                            {
                                if (!Owner.RaceGuard && !Owner.RaceFrightened)
                                {
                                    Owner.DizzyStamp = Time32.Now;
                                    Owner.RaceDizzy = true;
                                    Owner.Entity.AddFlag(Update.Flags.Dizzy);
                                    {
                                        var upd = new GameCharacterUpdates(true);
                                        upd.UID = Entity.UID;
                                        upd.Add(GameCharacterUpdates.Dizzy, 0, 5);
                                        Owner.SendScreen(upd, true);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case Enums.RaceItemType.ScreamBomb:
                    {
                        SendScreen(new SpellUse(true)
                        {
                            Attacker = Entity.UID,
                            SpellID = 9989,
                            SpellLevel = 0,
                            X = Entity.X,
                            Y = Entity.Y
                        }.AddTarget(Entity, 0, null), true);
                        foreach (var obj in Screen.SelectWhere<Entity>(MapObjectType.Player,
                            (o) => Kernel.GetDistance(o.X, o.Y, Entity.X, Entity.Y) <= 10))
                        {
                            var Owner = obj.Owner;
                            if (!Owner.RaceGuard && !Owner.RaceDizzy)
                            {
                                Owner.RaceFrightened = true;
                                Owner.FrightenStamp = Time32.Now;
                                Owner.Entity.AddFlag(Update.Flags.Frightened);
                                {
                                    var upd = new GameCharacterUpdates(true);
                                    upd.UID = Owner.Entity.UID;
                                    upd.Add(GameCharacterUpdates.Flustered, 0, 20);
                                    Owner.SendScreen(upd, true);
                                }
                            }
                        }
                        break;
                    }
                case Enums.RaceItemType.SpiritPotion:
                    {
                        ExtraVigorStamp = Time32.Now;
                        RaceExtraVigor = 2000;
                        break;
                    }
                case Enums.RaceItemType.ChaosBomb:
                    {
                        SendScreen(new SpellUse(true)
                        {
                            Attacker = Entity.UID,
                            SpellID = 9989,
                            SpellLevel = 0,
                            X = Entity.X,
                            Y = Entity.Y
                        }.AddTarget(Entity, 0, null), true);
                        foreach (var obj in this.Screen.SelectWhere<Entity>(MapObjectType.Player,
                               (o) => Kernel.GetDistance(o.X, o.Y, Entity.X, Entity.Y) <= 10))
                        {
                            var Owner = obj.Owner;
                            if (!Owner.RaceGuard)
                            {
                                Owner.FrightenStamp = Time32.Now;
                                Owner.DizzyStamp = Owner.DizzyStamp.AddSeconds(-1000);

                                Owner.Entity.AddFlag(Update.Flags.Confused);
                                {
                                    var upd = new GameCharacterUpdates(true);
                                    upd.UID = Owner.Entity.UID;
                                    upd.Add(GameCharacterUpdates.Flustered, 0, 15);
                                    Owner.SendScreen(upd, true);
                                }
                            }
                        }
                        break;
                    }
                case Enums.RaceItemType.SluggishPotion:
                    {
                        SendScreen(new SpellUse(true)
                        {
                            Attacker = Entity.UID,
                            SpellID = 9989,
                            SpellLevel = 0,
                            X = Entity.X,
                            Y = Entity.Y
                        }.AddTarget(Entity, 0, null), true);
                        foreach (var obj in this.Screen.SelectWhere<Entity>(MapObjectType.Player,
                              o => Kernel.GetDistance(o.X, o.Y, Entity.X, Entity.Y) <= 10))
                        {
                            var Owner = obj.Owner;
                            if (!Owner.RaceGuard)
                            {
                                Owner.RaceDecelerated = true;
                                Owner.DecelerateStamp = Time32.Now;
                                if (Owner.RaceExcitement)
                                {
                                    Owner.RaceExcitement = false;

                                    var upd = new GameCharacterUpdates(true);
                                    upd.UID = Owner.Entity.UID;
                                    upd.Remove(GameCharacterUpdates.Accelerated);
                                    Owner.SendScreen(upd, true);
                                }
                                Owner.Entity.AddFlag(Update.Flags.PurpleSparkles);
                                {
                                    var upd = new GameCharacterUpdates(true);
                                    upd.UID = Owner.Entity.UID;
                                    unchecked { upd.Add(GameCharacterUpdates.Decelerated, 50, 10, (uint)(0 - 25)); }
                                    Owner.SendScreen(upd, true);
                                    Owner.SpeedChange = upd;
                                }
                            }
                        }
                        break;
                    }
                case Enums.RaceItemType.TransformItem:
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (Potions[i] != null)
                            {
                                if (Potions[i].Type != Enums.RaceItemType.TransformItem)
                                {
                                    Send(new RacePotion(true)
                                    {
                                        Amount = 0,
                                        Location = i + 1,
                                        PotionType = Potions[i].Type
                                    });
                                    Potions[i] = null;
                                }
                            }
                        }
                        //for (int i = 0; i < 5; i++)
                        {
                            int i = 0;
                            if (Potions[i] == null)
                            {
                                int val = (int)Enums.RaceItemType.TransformItem;
                                while (val == (int)Enums.RaceItemType.TransformItem)
                                    val = Kernel.Random.Next((int)Enums.RaceItemType.ChaosBomb, (int)Enums.RaceItemType.SuperExcitementPotion);
                                Potions[i] = new UsableRacePotion();
                                Potions[i].Count = 1;
                                Potions[i].Type = (Enums.RaceItemType)val;
                                Send(new RacePotion(true)
                                {
                                    Amount = 1,
                                    Location = i + 1,
                                    PotionType = Potions[i].Type
                                });
                            }
                        }
                        break;
                    }
            }
        }


        public void ReviewMentor()
        {
            #region NotMentor
            uint nowBP = 0;
            if (Mentor != null)
            {
                if (Mentor.IsOnline)
                {
                    nowBP = Entity.BattlePowerFrom(Mentor.Client.Entity);
                }
            }
            if (nowBP > 200) nowBP = 0;
            if (nowBP < 0) nowBP = 0;
            if (Entity.MentorBattlePower != nowBP)
            {
                Entity.MentorBattlePower = nowBP;
                if (Mentor != null)
                {
                    if (Mentor.IsOnline)
                    {
                        MentorInformation Information = new MentorInformation(true);
                        Information.Mentor_Type = 1;
                        Information.Mentor_ID = Mentor.Client.Entity.UID;
                        Information.Apprentice_ID = Entity.UID;
                        Information.Enrole_Date = Mentor.EnroleDate;
                        Information.Mentor_Level = Mentor.Client.Entity.Level;
                        Information.Mentor_Class = Mentor.Client.Entity.Class;
                        Information.Mentor_PkPoints = Mentor.Client.Entity.PKPoints;
                        Information.Mentor_Mesh = Mentor.Client.Entity.Mesh;
                        Information.Mentor_Online = true;
                        Information.Shared_Battle_Power = nowBP;
                        Information.String_Count = 3;
                        Information.Mentor_Name = Mentor.Client.Entity.Name;
                        Information.Apprentice_Name = Entity.Name;
                        Information.Mentor_Spouse_Name = Mentor.Client.Entity.Spouse;
                        Send(Information);
                    }
                }
            }
            #endregion
            #region Mentor
            if (Apprentices == null) Apprentices = new SafeDictionary<uint, Game.ConquerStructures.Society.Apprentice>();
            foreach (var appr in Apprentices.Values)
            {
                if (appr.IsOnline)
                {
                    uint nowBPs = 0;
                    nowBPs = appr.Client.Entity.BattlePowerFrom(Entity);
                    if (appr.Client.Entity.MentorBattlePower != nowBPs)
                    {
                        appr.Client.Entity.MentorBattlePower = nowBPs;
                        MentorInformation Information = new MentorInformation(true);
                        Information.Mentor_Type = 1;
                        Information.Mentor_ID = Entity.UID;
                        Information.Apprentice_ID = appr.Client.Entity.UID;
                        Information.Enrole_Date = appr.EnroleDate;
                        Information.Mentor_Level = Entity.Level;
                        Information.Mentor_Class = Entity.Class;
                        Information.Mentor_PkPoints = Entity.PKPoints;
                        Information.Mentor_Mesh = Entity.Mesh;
                        Information.Mentor_Online = true;
                        Information.Shared_Battle_Power = nowBPs;
                        Information.String_Count = 3;
                        Information.Mentor_Name = Entity.Name;
                        Information.Apprentice_Name = appr.Client.Entity.Name;
                        Information.Mentor_Spouse_Name = Entity.Spouse;
                        appr.Client.Send(Information);
                    }
                }
            }
            #endregion
        }
        public void AddQuarantineKill()
        {
            quarantineKill++;
            UpdateQuarantineScore();
        }
        public void AddGl()
        {
            TopGlClaim++;
            return;
        }
        public void AddDl()
        {
            TopDlClaim++;
            return;
        }
        public void AddQuarantineDeath()
        {
            quarantineDeath++;
            UpdateQuarantineScore();
        }
        public void UpdateQuarantineScore()
        {
            string[] scores = new string[3];
            scores[0] = "Black team: " + MTA.Game.Quarantine.BlackScore.ToString() + " wins";
            scores[1] = "White team: " + MTA.Game.Quarantine.WhiteScore.ToString() + " wins";
            scores[2] = "Your score: " + quarantineKill + " kills, " + quarantineDeath + " death";
            for (int i = 0; i < scores.Length; i++)
            {
                Message msg = new Message(scores[i], System.Drawing.Color.Red, i == 0 ? Message.FirstRightCorner : Message.ContinueRightCorner);
                Send(msg);
            }
        }
        public void KillTerrorist()
        {
            foreach (Client.GameState Terrorist in Program.Values)
            {
                if (Terrorist.Entity.KillTheTerrorist_IsTerrorist == true && Terrorist.Entity.MapID == 1801)
                    MTA.Kernel.SendWorldMessage(new MTA.Network.GamePackets.Message("Terrorist: " + Terrorist.Entity.Name + " ", System.Drawing.Color.Black, MTA.Network.GamePackets.Message.FirstRightCorner), Program.Values);
            }
        }
        public void AddBless(uint value)
        {
            Entity.HeavenBlessing += value;
            Entity.Update(Network.GamePackets._String.Effect, "bless", true);
            if (Mentor != null)
            {
                if (Mentor.IsOnline)
                {
                    Mentor.Client.PrizeHeavenBlessing += (ushort)(value / 10 / 60 / 60);
                    AsApprentice = Mentor.Client.Apprentices[Entity.UID];
                }
                if (AsApprentice != null)
                {
                    AsApprentice.Actual_HeavenBlessing += (ushort)(value / 10 / 60 / 60);
                    AsApprentice.Total_HeavenBlessing += (ushort)(value / 10 / 60 / 60);
                    if (Time32.Now > LastMentorSave.AddSeconds(5))
                    {
                        Database.KnownPersons.SaveApprenticeInfo(AsApprentice);
                        LastMentorSave = Time32.Now;
                    }
                }
            }
        }
        public ulong PrizeExperience;
        public ushort PrizeHeavenBlessing;
        public ushort PrizePlusStone;

        public uint MentorApprenticeRequest;
        public uint TradePartnerRequest;

        public object[] OnMessageBoxEventParams;
        public Action<GameState> MessageOK;
        public Action<GameState> MessageCancel;

        public bool JustLoggedOn = true;
        public Time32 ReviveStamp = Time32.Now;
        public bool Attackable;
        public int KillCount = 0, KC2 = 0;
        public Game.ConquerStructures.NobilityInformation NobilityInformation;
        public Game.Entity Entity;
        public Game.Screen Screen;
        public Time32 LastPing = Time32.Now;
        public static ushort NpcTestType = 0;
        public byte TinterItemSelect = 0;
        public DateTime LastDragonBallUse, LastResetTime;
        public byte Action = 0;
        public bool CheerSent = false;
        public Game.Arena.QualifierList.QualifierGroup WatchingGroup;
        public Game.Arena.QualifierList.QualifierGroup QualifierGroup;
        public Game.Champion.QualifierList.QualifierGroup ChampionGroup;
        public Network.GamePackets.ArenaStatistic ArenaStatistic;

        public Game.TeamArena.QualifierList.QualifierGroup TeamWatchingGroup;
        public Game.TeamArena.QualifierList.QualifierGroup TeamQualifierGroup;
        public Network.GamePackets.TeamArenaStatistic TeamArenaStatistic;
        public uint ArenaPoints
        {
            get
            {
                return ArenaStatistic.ArenaPoints;
            }
            set
            {
                ArenaStatistic.ArenaPoints =
                    TeamArenaStatistic.ArenaPoints =
                    value;
            }
        }
        private byte xpCount;
        public byte XPCount
        {
            get { return xpCount; }
            set
            {
                xpCount = value;
                if (xpCount >= 100) xpCount = 100;

                Update update = new Update(true);
                update.UID = Entity.UID;
                update.Append(Update.XPCircle, xpCount);
                update.Send(this);
            }
        }
        public Time32 XPCountStamp = Time32.Now;
        public Time32 XPListStamp = Time32.Now;

        public MTA.Game.ConquerStructures.Trade Trade;
        public byte ExpBalls = 0;
        public ulong MoneySave = 0;
        public uint ActiveNpc = 0;
        public string WarehousePW1, TempPassword;
        public uint WarehousePW;
        public bool WarehouseOpen;
        public Time32 CoolStamp;
        public sbyte WarehouseOpenTries;
        public ushort InputLength;
        public MTA.Game.ConquerStructures.Society.Mentor Mentor;
        public MTA.Game.ConquerStructures.Society.Apprentice AsApprentice;
        public SafeDictionary<ushort, Interfaces.ISkill> RemoveSpells = new SafeDictionary<ushort, Interfaces.ISkill>();
        public SafeDictionary<ushort, Interfaces.IProf> Proficiencies;
        public SafeDictionary<ushort, Interfaces.ISkill> Spells;
        public SafeDictionary<uint, MTA.Game.ConquerStructures.Society.Friend> Friends;
        public SafeDictionary<uint, MTA.Game.ConquerStructures.Society.Enemy> Enemy;
        public SafeDictionary<uint, MTA.Game.ConquerStructures.Society.TradePartner> Partners;
        public SafeDictionary<uint, MTA.Game.ConquerStructures.Society.Apprentice> Apprentices;
        public Game.ConquerStructures.Inventory Inventory;
        public Game.ConquerStructures.Equipment Equipment;
        public SafeDictionary<Game.ConquerStructures.Warehouse.WarehouseID, Game.ConquerStructures.Warehouse> Warehouses;
        public Game.ConquerStructures.Team Team;
        public Time32 lastClientJumpTime = Time32.Now;
        public Time32 lastJumpTime = Time32.Now;
        public int LastJumpTime = 0;
        public short lastJumpDistance = 0;
        public bool DoubleExpToday = false;

        private Game.Map map;
        public Game.Map Map
        {
            get
            {
                if (map == null)
                {
                    Kernel.Maps.TryGetValue(Entity.MapID, out map);
                    /*if (map == null) 
                        Entity.MapID = 1005;*/
                    if (map == null)
                        return (map = new Game.Map(Entity.MapID, Database.MapsTable.MapInformations[Entity.MapID].BaseID, Database.DMaps.MapPaths[Database.MapsTable.MapInformations[Entity.MapID].BaseID]));
                }
                else
                {
                    if (map.ID != Entity.MapID)
                    {
                        Kernel.Maps.TryGetValue(Entity.MapID, out map);
                        /*if (map == null) 
                            Entity.MapID = 1005;*/
                        if (map == null)
                            return (map = new Game.Map(Entity.MapID, Database.MapsTable.MapInformations[Entity.MapID].BaseID, Database.DMaps.MapPaths[Database.MapsTable.MapInformations[Entity.MapID].BaseID]));
                    }
                    if (Entity.MapID == 1004 || Entity.MapID == 1458 || Entity.MapID == 1459 || Entity.MapID == 1460 || Entity.MapID == 16414 || Entity.MapID == 1507 || Entity.MapID == 3990 || Entity.MapID == 3995)
                        if (Entity.ContainsFlag(Network.GamePackets.Update.Flags.Ride)) { Entity.RemoveFlag(Network.GamePackets.Update.Flags.Ride); }

                }
                return map;
            }
        }  

        public uint ExpBall
        {
            get
            {
                ulong exp = Database.DataHolder.LevelExperience(Entity.Level);
                return (uint)(exp * 13000 / (ulong)((Entity.Level * Entity.Level * Entity.Level / 12) + 1));
            }
        }

        public bool AddProficiency(Interfaces.IProf proficiency)
        {
            if (Proficiencies.ContainsKey(proficiency.ID))
            {
                Proficiencies[proficiency.ID].Level = proficiency.Level;
                Proficiencies[proficiency.ID].Experience = proficiency.Experience;
                proficiency.Send(this);
                //Database.SkillTable.SaveProficiencies(this, proficiency.ID);//Samak
                return false;
            }
            else
            {
                Proficiencies.Add(proficiency.ID, proficiency);
                proficiency.NeededExperience = Database.DataHolder.ProficiencyLevelExperience(proficiency.Level);
                proficiency.Send(this);
                Database.SkillTable.SaveProficiencies(this);//Samak
                return true;
            }
        }

        public bool AddSpell(Interfaces.ISkill spell)
        {
            if (Spells.ContainsKey(spell.ID))
            {
                if (Spells[spell.ID].Level < spell.Level)
                {
                    Spells[spell.ID].Level = spell.Level;
                    Spells[spell.ID].Experience = spell.Experience;
                    if (spell.ID != 3060)
                    {
                        spell.Send(this);
                        //Database.SkillTable.SaveSpells(this, spell.ID);
                    }
                }
                return false;
            }
            else
            {
                if (spell.ID == 1045 || spell.ID == 1046)
                {
                    //if (Proficiencies.ContainsKey(Database.SpellTable.SpellInformations[spell.ID][spell.Level].WeaponSubtype))
                    //{
                    //    if (Proficiencies[Database.SpellTable.SpellInformations[spell.ID][spell.Level].WeaponSubtype].Level < 5)
                    //        return false;
                    //}
                    //else
                    //    return false;
                }
                Spells.Add(spell.ID, spell);
                if (spell.ID != 3060)
                {
                    spell.Send(this);
                    //      Database.SkillTable.SaveSpells(this, spell.ID);//Samak
                }
                Database.SkillTable.SaveSpells(this);//Samak

                return true;
            }
        }       
        public bool RemoveSpell(Interfaces.ISkill spell)
        {
            if (Spells.ContainsKey(spell.ID))
            {
                Spells.Remove(spell.ID);
                Network.GamePackets.Data data = new Data(true);
                data.UID = Entity.UID;
                data.dwParam = spell.ID;
                data.ID = 109;
                Send(data);
                Database.SkillTable.DeleteSpell(this, spell.ID);
                return true;
            }
            return false;
        }
        public bool WentToComplete = false;
        public byte SelectedGem = 0;
        public Time32 LastMentorSave = Time32.Now;
        public void IncreaseExperience(ulong experience, bool addMultiple)
        {
            if (Entity.Dead) return;
            byte level = Entity.Level;
            ulong _experience = Entity.Experience;
            ulong prExperienece = experience;
            if (addMultiple)
            {
                if (Entity.VIPLevel > 0)
                    experience *= Entity.VIPLevel;
                experience *= Constants.ExtraExperienceRate;
                if (Entity.HeavenBlessing > 0)
                    experience += (uint)(experience * 20 / 100);
                if (Entity.Reborn >= 2)
                    experience /= 3;
                if (Entity.DoubleExperienceTime > 0 && this.SuperPotion > 0)
                    experience *= (uint)(this.SuperPotion);

                if (Guild != null)
                {
                    if (Guild.Level > 0)
                    {
                        experience += (ushort)(experience * Guild.Level / 100);
                    }
                }
                prExperienece = experience + (ulong)(experience * ((float)Entity.BattlePower / 100));
                _experience += prExperienece;

                _experience += (uint)(_experience * (uint)Entity.Gems[3] / 100);

            }
            else
                _experience += experience;

            if (Entity.Level < 140 && Entity.Auto == true)
            {
                Entity.autohuntxp += (_experience / 16);
                return;
            }
            else if (Entity.Level == 140 && Entity.Auto == true)
            {
                Entity.autohuntxp = 0;
                return;
            }
            if (Entity.Level < 140)
            {
                while (_experience >= Database.DataHolder.LevelExperience(level) && level < 140)
                {
                    _experience -= Database.DataHolder.LevelExperience(level);
                    level++;
                    if (Entity.Reborn == 1)
                    {
                        if (level >= 130 && Entity.FirstRebornLevel > 130 && level < Entity.FirstRebornLevel)
                            level = Entity.FirstRebornLevel;
                    }
                    else if (Entity.Reborn == 2)
                    {
                        if (level >= 130 && Entity.SecondRebornLevel > 130 && level < Entity.SecondRebornLevel)
                            level = Entity.SecondRebornLevel;
                    }
                    if (Entity.Class >= 10 && Entity.Class <= 15)
                        if (!Spells.ContainsKey(1110))
                            AddSpell(new Network.GamePackets.Spell(true) { ID = 1110 });
                    if (Entity.Class >= 20 && Entity.Class <= 25)
                        if (!Spells.ContainsKey(1020))
                            AddSpell(new Network.GamePackets.Spell(true) { ID = 1020 });
                    if (Entity.Class >= 40 && Entity.Class <= 45)
                        if (!Spells.ContainsKey(8002))
                            AddSpell(new Network.GamePackets.Spell(true) { ID = 8002 });
                    if (Entity.Class >= 50 && Entity.Class <= 55)
                        if (!Spells.ContainsKey(6011))
                            AddSpell(new Network.GamePackets.Spell(true) { ID = 6011 });
                    if (Entity.Class >= 60 && Entity.Class <= 65)
                        if (!Spells.ContainsKey(10490))
                            AddSpell(new Network.GamePackets.Spell(true) { ID = 10490 });
                    if (Mentor != null)
                    {
                        if (Mentor.IsOnline)
                        {
                            uint exExp = (uint)(level * 2);
                            Mentor.Client.PrizeExperience += exExp;
                            AsApprentice = Mentor.Client.Apprentices[Entity.UID];
                            if (AsApprentice != null)
                            {
                                AsApprentice.Actual_Experience += exExp;
                                AsApprentice.Total_Experience += exExp;
                            }
                            if (Mentor.Client.PrizeExperience > 50 * 606)
                                Mentor.Client.PrizeExperience = 50 * 606;
                        }
                    }
                    if (level == 70)
                    {
                        if (ArenaStatistic == null || ArenaStatistic.EntityID == 0)
                        {
                            ArenaStatistic = new MTA.Network.GamePackets.ArenaStatistic(true);
                            ArenaStatistic.EntityID = Entity.UID;
                            ArenaStatistic.Name = Entity.Name;
                            ArenaStatistic.Level = Entity.Level;
                            ArenaStatistic.Class = Entity.Class;
                            ArenaStatistic.Model = Entity.Mesh;
                            ArenaPoints = Database.ArenaTable.ArenaPointFill(Entity.Level);
                            ArenaStatistic.LastArenaPointFill = DateTime.Now;
                            Database.ArenaTable.InsertArenaStatistic(this);
                            ArenaStatistic.Status = Network.GamePackets.ArenaStatistic.NotSignedUp;
                            Game.Arena.ArenaStatistics.Add(Entity.UID, ArenaStatistic);
                        }
                    }
                    if (Entity.Reborn == 0)
                    {
                        if (level <= 120)
                        {
                            Database.DataHolder.GetStats(Entity.Class, level, this);
                            CalculateStatBonus();
                            CalculateHPBonus();
                            GemAlgorithm();
                        }
                        else
                            Entity.Atributes += 3;
                    }
                    else
                    {
                        Entity.Atributes += 3;
                    }
                }
                if (Entity.Level != level)
                {
                    if (Team != null)
                    {
                        if (Team.LowestLevelsUID == Entity.UID)
                        {
                            Team.LowestLevel = 0;
                            Team.LowestLevelsUID = 0;
                            Team.SearchForLowest();
                        }
                    }
                    Entity.Level = level;
                    Entity.Hitpoints = Entity.MaxHitpoints;
                    Entity.Mana = Entity.MaxMana;
                    if (Entity.Level > 130)
                        Database.EntityTable.UpdateLevel(Entity.Owner);
                    if (Entity.Reborn == 2)
                        Network.PacketHandler.ReincarnationHash(Entity.Owner);
                }
                if (Entity.Experience != _experience)
                    Entity.Experience = _experience;
            }
        }

        public void IncreaseSpellExperience(uint experience, ushort id)
        {
            if (Spells.ContainsKey(id))
            {
                switch (id)
                {
                    case 1290:
                    case 5030:
                    case 7030:
                        experience = 100; break;
                }
                experience *= Constants.ExtraSpellRate;
                experience += (uint)(experience * Entity.Gems[6] / 100);
                if (Map.BaseID == 1039)
                    experience /= 40;
                Interfaces.ISkill spell = Spells[id];
                if (spell == null)
                    return;
                if (Entity.VIPLevel > 0)
                {
                    experience *= 5;
                }
                Database.SpellInformation spellInfo = Database.SpellTable.SpellInformations[spell.ID][spell.Level];
                if (spellInfo != null)
                {
                    if (spellInfo.NeedExperience != 0 && Entity.Level >= spellInfo.NeedLevel)
                    {
                        spell.Experience += experience;
                        bool leveled = false;
                        if (spell.Experience >= spellInfo.NeedExperience)
                        {
                            spell.Experience = 0;
                            spell.Level++;
                            leveled = true;
                            Send(Constants.SpellLeveled);
                        }
                        if (leveled)
                        {
                            spell.Send(this);
                            Database.SkillTable.SaveSpells(this);//Samak
                        }
                        else
                        {
                            Network.GamePackets.SkillExperience update = new SkillExperience(true);
                            update.AppendSpell(spell.ID, spell.Experience);
                            update.Send(this);
                            //Database.SkillTable.SaveSpells(this, spell.ID);//Samak Mohsen told men that no excperince any more after fixDatabase.EntityTable.UpdateSkillExp(this, spell.ID, experience);
                            Database.EntityTable.UpdateSkillExp(this, spell.ID, experience);
                        }
                    }
                }
            }
        }

        public void IncreaseProficiencyExperience(uint experience, ushort id)
        {
            if (Proficiencies.ContainsKey(id))
            {
                Interfaces.IProf proficiency = Proficiencies[id];
                experience *= Constants.ExtraProficiencyRate;
                experience += (uint)(experience * Entity.Gems[5] / 100);
                if (Map.BaseID == 1039)
                    experience /= 40;
                if (Entity.VIPLevel > 0)
                {
                    experience *= 5;
                }
                proficiency.Experience += experience;
                if (proficiency.Level < 20)
                {
                    bool leveled = false;
                    while (proficiency.Experience >= Database.DataHolder.ProficiencyLevelExperience(proficiency.Level))
                    {
                        proficiency.Experience -= Database.DataHolder.ProficiencyLevelExperience(proficiency.Level);
                        proficiency.Level++;
                        if (proficiency.Level == 20)
                        {
                            proficiency.Experience = 0;
                            proficiency.Send(this);
                            Send(Constants.ProficiencyLeveled);
                            return;
                        }
                        proficiency.NeededExperience = Database.DataHolder.ProficiencyLevelExperience(proficiency.Level);
                        leveled = true;
                        Send(Constants.ProficiencyLeveled);
                    }
                    if (leveled)
                    {
                        proficiency.Send(this);
                        //   Database.SkillTable.SaveProficiencies(this, proficiency.ID);//Samak
                    }
                    else
                    {
                        Network.GamePackets.SkillExperience update = new SkillExperience(true);
                        update.AppendProficiency(proficiency.ID, proficiency.Experience, Database.DataHolder.ProficiencyLevelExperience(proficiency.Level));
                        update.Send(this);
                    }
                    //Database.SkillTable.SaveProficiencies(this, proficiency.ID);//Samak XXXX
                }

            }
            else
            {
                AddProficiency(new Network.GamePackets.Proficiency(true) { ID = id });
            }
        }

        public byte ExtraAtributePoints(byte level, byte mClass)
        {
            if (mClass == 135)
            {
                if (level <= 110)
                    return 0;
                switch (level)
                {
                    case 112: return 1;
                    case 114: return 3;
                    case 116: return 6;
                    case 118: return 10;
                    case 120: return 15;
                    case 121: return 15;
                    case 122: return 21;
                    case 123: return 21;
                    case 124: return 28;
                    case 125: return 28;
                    case 126: return 36;
                    case 127: return 36;
                    case 128: return 45;
                    case 129: return 45;
                    default:
                        return 55;
                }
            }
            else
            {
                if (level <= 120)
                    return 0;
                switch (level)
                {
                    case 121: return 1;
                    case 122: return 3;
                    case 123: return 6;
                    case 124: return 10;
                    case 125: return 15;
                    case 126: return 21;
                    case 127: return 28;
                    case 128: return 36;
                    case 129: return 45;
                    default:
                        return 55;
                }
            }
        }
        public static ISkill LearnableSpell(ushort spellid)
        {
            ISkill spell = new Spell(true);
            spell.ID = spellid;
            return spell;
        }
        public bool Reborn(byte toClass)
        {
            #region Items

            if (Inventory.Count > 37) return false;
            switch (toClass)
            {
                case 11:
                case 21:
                case 51:
                case 61:
                case 71:
                    {
                        Inventory.Add(410077, Game.Enums.ItemEffect.Poison);
                        break;
                    }
                case 41:
                    {
                        Inventory.Add(500057, Game.Enums.ItemEffect.Shield);
                        break;
                    }
                case 132:
                case 142:
                    {
                        if (toClass == 132)
                            Inventory.Add(421077, Game.Enums.ItemEffect.MP);
                        else
                            Inventory.Add(421077, Game.Enums.ItemEffect.HP);
                        break;
                    }
            }

            #region Low level items

            for (byte i = 1; i < 9; i++)
            {
                if (i != 7)
                {
                    ConquerItem item = Equipment.TryGetItem(i);
                    if (item != null && item.ID != 0)
                    {
                        try
                        {
                            //UnloadItemStats(item, false);
                            Database.ConquerItemInformation cii =
                                new MTA.Database.ConquerItemInformation(item.ID, item.Plus);
                            item.ID =
                                cii.LowestID(
                                    Network.PacketHandler.ItemMinLevel(Network.PacketHandler.ItemPosition(item.ID)));
                            item.Mode = MTA.Game.Enums.ItemMode.Update;
                            item.Send(this);
                            LoadItemStats();
                            Database.ConquerItemTable.UpdateItemID(item, this);
                        }
                        catch
                        {
                            Console.WriteLine("Reborn item problem: " + item.ID);
                        }
                    }
                }
            }
            ConquerItem hand = Equipment.TryGetItem(5);
            if (hand != null)
            {
                Equipment.Remove(5);
                CalculateStatBonus();
                CalculateHPBonus();
            }
            hand = Equipment.TryGetItem(25);
            if (hand != null)
            {
                Equipment.Remove(25);
                CalculateStatBonus();
                CalculateHPBonus();
            }
            LoadItemStats();
            SendScreen(Entity.SpawnPacket, false);

            #endregion

            #endregion

            if (Entity.Reborn == 0)
            {
                Entity.FirstRebornClass = Entity.Class;
                Entity.FirstRebornLevel = Entity.Level;
                Entity.Atributes =
                    (ushort)(ExtraAtributePoints(Entity.FirstRebornClass, Entity.FirstRebornLevel) + 52);
            }
            else
            {
                Entity.SecondRebornClass = Entity.Class;
                Entity.SecondRebornLevel = Entity.Level;
                Entity.Atributes =
                    (ushort)(ExtraAtributePoints(Entity.FirstRebornClass, Entity.FirstRebornLevel) +
                              ExtraAtributePoints(Entity.SecondRebornClass, Entity.SecondRebornLevel) + 62);
            }
            byte PreviousClass = Entity.Class;
            Entity.Reborn++;
            Entity.Class = toClass;
            Entity.Level = 15;
            Entity.Experience = 0;

            #region Spells
            Interfaces.ISkill[] spells = Spells.Values.ToArray();
            foreach (Interfaces.ISkill spell in spells)
            {
                spell.PreviousLevel = spell.Level;
                spell.Level = 0;
                spell.Experience = 0;
                #region KungFuKing
                if (PreviousClass == 85)
                {
                    if (Entity.Class != 81)
                    {
                        switch (spell.ID)
                        {
                            case 12120:
                            case 12130:
                            case 12140:
                            case 12160:
                            case 12170:
                            case 12200:
                            case 12240:
                            case 12350:
                            case 12270:
                            case 12280:
                            case 12290:
                            case 12300:
                            case 12320:
                            case 12330:
                            case 12340:
                                RemoveSpell(spell);
                                break;
                        }
                    }
                }
                #endregion
                #region Pirate

                if (PreviousClass == 75)
                {
                    if (Entity.Class != 71)
                    {
                        switch (spell.ID)
                        {
                            case 11110:
                            case 11040:
                            case 11050:
                            case 11060:
                            case 11100:
                            case 11120:
                            case 11130:
                            case 11030:
                                RemoveSpell(spell);
                                break;
                        }
                    }
                }

                #endregion
                #region Monk

                if (PreviousClass == 65)
                {
                    if (Entity.Class != 61)
                    {
                        switch (spell.ID)
                        {
                            case 10490:
                            case 12580:
                            case 12590:
                            case 12600:
                            case 12570:
                            case 12560:
                            case 12550:
                            case 10395:
                            case 10430:
                            case 10410:
                            case 10415:
                            case 10381:
                            case 10425:
                                RemoveSpell(spell);
                                break;
                        }
                    }
                }

                #endregion
                #region WindWalker
                if (PreviousClass == 165)
                {
                    if (Entity.Class != 161)
                    {
                        switch (spell.ID)
                        {
                            case 12840:
                            case 12850:
                            case 12860:
                            case 12870:
                            case 12890:
                            case 12930:
                            case 12940:
                            case 12950:
                            case 12960:
                            case 12970:
                            case 12980:
                            case 12990:
                            case 13000:
                            case 13020:
                            case 13030:
                            case 13090:
                            case 13190:
                            case 13070:
                            case 13080:

                                RemoveSpell(spell);
                                break;
                        }
                    }
                }
                #endregion
                #region Warrior

                if (PreviousClass == 25)
                {
                    if (Entity.Class != 21)
                    {
                        switch (spell.ID)
                        {
                            case 1025:
                            case 12700:
                            case 12690:
                            case 12770:
                            case 12680:
                            case 11160:
                            case 11200:
                                if (Entity.Class != 21 && Entity.Class != 132)
                                    RemoveSpell(spell);
                                break;
                        }
                    }
                }

                #endregion
                #region Ninja

                if (toClass != 51)
                {
                    switch (spell.ID)
                    {
                        case 6010:
                        case 6005:
                        case 6022:
                        case 6000:
                        case 6011:
                        case 6017:
                        case 11170:
                        case 11180:
                        case 11230:
                            RemoveSpell(spell);
                            break;
                    }
                }

                #endregion
                #region Trojan

                if (toClass != 11)
                {
                    switch (spell.ID)
                    {
                        case 1115:
                        case 1130:
                            RemoveSpell(spell);
                            break;
                    }
                }

                #endregion
                #region Archer

                if (toClass != 41)
                {
                    switch (spell.ID)
                    {
                        case 8001:
                        //RapidFire//
                        case 8000:
                        case 8003:
                        //Intensify//
                        case 9000:
                        case 8002:
                        //ArrowRain//
                        case 8030:
                        //ScatterFire//
                        case 8010:
                        case 8031:
                        //Fly//
                        case 8020:
                        //KineticSpark//
                        case 11590:
                        //DaggerStorm//
                        case 11600:
                        //BladeFlurry//
                        case 11610:
                        //PathOfShadow//
                        case 11620:
                        case 11650:
                        case 11660:
                        case 11670:
                            RemoveSpell(spell);
                            break;
                    }
                }

                #endregion
                #region WaterTaoist

                if (PreviousClass == 135)
                {
                    if (toClass != 132)
                    {
                        switch (spell.ID)
                        {
                            case 1000:
                            case 1001:
                            case 1010:
                            case 1125:
                            case 1100:
                            case 8030:
                                RemoveSpell(spell);
                                break;
                            case 1050:
                            case 1175:
                            case 1170:
                                if (toClass != 142)
                                    RemoveSpell(spell);
                                break;
                        }
                    }
                }

                #endregion
                #region FireTaoist

                if (PreviousClass == 145)
                {
                    if (toClass != 142)
                    {
                        switch (spell.ID)
                        {
                            case 1000:
                            case 1001:
                            case 1150:
                            case 1180:
                            case 1120:
                            case 1002:
                            case 1160:
                            case 1165:
                                RemoveSpell(spell);
                                break;
                        }
                    }
                }

                #endregion
                if (Spells.ContainsKey(spell.ID))
                    if (spell.ID != (ushort)Game.Enums.SkillIDs.Reflect)
                        spell.Send(this);
            }
            #endregion

            #region Proficiencies

            foreach (Interfaces.IProf proficiency in Proficiencies.Values)
            {
                proficiency.PreviousLevel = proficiency.Level;
                proficiency.Level = 0;
                proficiency.Experience = 0;
                proficiency.Send(this);
            }

            #endregion

     


                #region Adding earned skills

                if (Entity.Reborn == 2)
                AddSpell(new Spell(true) { ID = 9876 });
            if (toClass == 51 && PreviousClass == 55 && Entity.Reborn == 1)
                AddSpell(new Spell(true) { ID = 6002 });
            if (toClass == 81 && PreviousClass == 85 && Entity.Reborn == 1)
                AddSpell(new Spell(true) { ID = 12280 });
            if (Entity.FirstRebornClass == 85 && Entity.SecondRebornClass == 85 && Entity.Class == 81 &&
                Entity.Reborn == 2)
                AddSpell(new Spell(true) { ID = 12300 });
            if (Entity.FirstRebornClass == 15 && Entity.SecondRebornClass == 15 && Entity.Class == 11 &&
                Entity.Reborn == 2)
                AddSpell(new Spell(true) { ID = 10315 });
            if (Entity.FirstRebornClass == 25 && Entity.SecondRebornClass == 25 && Entity.Class == 21 &&
                Entity.Reborn == 2)
                AddSpell(new Spell(true) { ID = 10311 });
            if (Entity.FirstRebornClass == 45 && Entity.SecondRebornClass == 45 && Entity.Class == 41 &&
                Entity.Reborn == 2)
                AddSpell(new Spell(true) { ID = 10313 });
            if (Entity.FirstRebornClass == 55 && Entity.SecondRebornClass == 55 && Entity.Class == 51 &&
                Entity.Reborn == 2)
                AddSpell(new Spell(true) { ID = 6003 });
            if (Entity.FirstRebornClass == 65 && Entity.SecondRebornClass == 65 && Entity.Class == 61 &&
                Entity.Reborn == 2)
                AddSpell(new Spell(true) { ID = 10405 });
            if (Entity.FirstRebornClass == 135 && Entity.SecondRebornClass == 135 && Entity.Class == 132 &&
                Entity.Reborn == 2)
                AddSpell(new Spell(true) { ID = 30000 });
            if (Entity.FirstRebornClass == 145 && Entity.SecondRebornClass == 145 && Entity.Class == 142 &&
                Entity.Reborn == 2)
                AddSpell(new Spell(true) { ID = 10310 });
            if (Entity.Reborn == 1)
            {
                if (Entity.FirstRebornClass == 75 && Entity.Class == 71)
                {
                    AddSpell(new Spell(true) { ID = 3050 });
                }
                if (Entity.FirstRebornClass == 15 && Entity.Class == 11)
                {
                    AddSpell(new Spell(true) { ID = 3050 });
                }
                else if (Entity.FirstRebornClass == 25 && Entity.Class == 21)
                {
                    AddSpell(new Spell(true) { ID = 3060 });
                }
                else if (Entity.FirstRebornClass == 145 && Entity.Class == 142)
                {
                    AddSpell(new Spell(true) { ID = 3080 });
                }
                else if (Entity.FirstRebornClass == 135 && Entity.Class == 132)
                {
                    AddSpell(new Spell(true) { ID = 3090 });
                }
            }
            if (Entity.Reborn == 2)
            {
                if (Entity.SecondRebornClass == 75 && Entity.Class == 71)
                {
                    AddSpell(new Spell(true) { ID = 3050 });
                }
                if (Entity.SecondRebornClass == 15 && Entity.Class == 11)
                {
                    AddSpell(new Spell(true) { ID = 3050 });
                }
                else if (Entity.SecondRebornClass == 25)
                {
                    AddSpell(new Spell(true) { ID = 3060 });
                }
                else if (Entity.SecondRebornClass == 145 && Entity.Class == 142)
                {
                    AddSpell(new Spell(true) { ID = 3080 });
                }
                else if (Entity.SecondRebornClass == 135 && Entity.Class == 132)
                {
                    AddSpell(new Spell(true) { ID = 3090 });
                }
            }

            #endregion

            #region Remove extra skills      
            if (Entity.Reborn == 2)
            {
                #region Pison Star Del
                if (Entity.SecondRebornClass == 55 && Entity.Class == 41)
                {
                    RemoveSpell(new Spell(false) { ID = 6002 });
                  //   RemoveSpell(new Spell(false) { ID = 8001 });
                }
                if (Entity.SecondRebornClass == 55 && Entity.Class == 81)
                {
                    RemoveSpell(new Spell(false) { ID = 6002 });
                    RemoveSpell(new Spell(false) { ID = 8001 });
                }
                if (Entity.SecondRebornClass == 55 && Entity.Class == 11)
                {
                    RemoveSpell(new Spell(false) { ID = 6002 });
                    RemoveSpell(new Spell(false) { ID = 8001 });
                }
                if (Entity.SecondRebornClass == 55 && Entity.Class == 71)
                {
                    RemoveSpell(new Spell(false) { ID = 6002 });
                    RemoveSpell(new Spell(false) { ID = 8001 });
                }
                if (Entity.SecondRebornClass == 55 && Entity.Class == 61)
                {
                    RemoveSpell(new Spell(false) { ID = 6002 });
                    RemoveSpell(new Spell(false) { ID = 8001 });
                }
                if (Entity.SecondRebornClass == 55 && Entity.Class == 21)
                {
                    RemoveSpell(new Spell(false) { ID = 6002 });
                    RemoveSpell(new Spell(false) { ID = 8001 });
                }
                else if (Entity.SecondRebornClass == 55 && Entity.Class == 142)
                {
                    RemoveSpell(new Spell(false) { ID = 6002 });
                    RemoveSpell(new Spell(false) { ID = 8001 });
                }
                else if (Entity.SecondRebornClass == 55 && Entity.Class == 132)
                {
                    RemoveSpell(new Spell(false) { ID = 6002 });
                    RemoveSpell(new Spell(false) { ID = 8001 });
                }

                #endregion
            }

            #endregion

            #region The View Of Wepeon
            ClientEquip eqs = new ClientEquip();
            eqs.DoEquips(this);
            Send(eqs);
            Equipment.UpdateEntityPacket();
            #endregion  
            Database.DataHolder.GetStats(Entity.Class, Entity.Level, this);
            CalculateStatBonus();
            CalculateHPBonus();
            GemAlgorithm();
            using (var conn = Database.DataHolder.MySqlConnection)
            {
                conn.Open();
                Database.EntityTable.SaveEntity(this, conn);
                //Samak Database.SkillTable.SaveSpells(this, conn);
                //Samak Database.SkillTable.SaveProficiencies(this, conn);
                Database.SkillTable.SaveSpells(this);
                Database.SkillTable.SaveProficiencies(this);
            }

            Kernel.SendWorldMessage(
                new Message("" + Entity.Name + " has got " + Entity.Reborn + " reborns. Congratulations!",
                    System.Drawing.Color.White, Message.Center), Program.Values);
            return true;
        }
        #region Items
        //private int StatHP;
        //public uint[] ArsenalDonations;
        //public uint GetArsenalDonation()
        //{
        //    uint val = 0;
        //    foreach (var Uint in ArsenalDonations)
        //        val += Uint;
        //    using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
        //        cmd.Update("entities").Set("GuildArsenalDonation", (uint)val).Where("UID", this.Entity.UID)
        //            .Execute();
        //    return val;
        //}
        //public void CalculateHPBonus()
        //{
        //    //  if ((int)Account.State >= 3) return;
        //    switch (Entity.Class)
        //    {
        //        case 11: Entity.MaxHitpoints = (uint)(StatHP * 1.05F); break;
        //        case 12: Entity.MaxHitpoints = (uint)(StatHP * 1.08F); break;
        //        case 13: Entity.MaxHitpoints = (uint)(StatHP * 1.10F); break;
        //        case 14: Entity.MaxHitpoints = (uint)(StatHP * 1.12F); break;
        //        case 15: Entity.MaxHitpoints = (uint)(StatHP * 1.15F); break;
        //        default: Entity.MaxHitpoints = (uint)StatHP; break;
        //    }
        //    Entity.MaxHitpoints += Entity.ItemHP;
        //    var plus = Entity.SubClasses.Classes.SingleOrDefault(x => x.Value.ID == 9);
        //    if (plus.Value != null && Entity.SubClass == 9)
        //        Entity.MaxHitpoints += (uint)(plus.Value.Level * 100);
        //    Entity.Hitpoints = Math.Min(Entity.Hitpoints, Entity.MaxHitpoints);
        //}
        //public void CalculateStatBonus()
        //{
        //    byte ManaBoost = 5;
        //    const byte HitpointBoost = 24;
        //    sbyte Class = (sbyte)(Entity.Class / 10);
        //    if (Class == 13 || Class == 14)
        //        ManaBoost += (byte)(5 * (Entity.Class - (Class * 10)));
        //    StatHP = (ushort)((Entity.Strength * 3) +
        //                             (Entity.Agility * 3) +
        //                             (Entity.Spirit * 3) +
        //                             (Entity.Vitality * HitpointBoost));
        //    Entity.MaxMana = (ushort)((Entity.Spirit * ManaBoost) + Entity.ItemMP);
        //    Entity.Mana = Math.Min(Entity.Mana, Entity.MaxMana);
        //}
        //public void SendStatMessage()
        //{
        //    this.ReviewMentor();
        //    Network.GamePackets.Message Msg = new MTA.Network.GamePackets.Message(" Your status has been changed", System.Drawing.Color.DarkGoldenrod
        //        , Network.GamePackets.Message.TopLeft);
        //    Msg.__Message = string.Format(Msg.__Message,
        //        new object[] { Entity.MinAttack, Entity.MaxAttack, Entity.MagicAttack, Entity.Defence, (Entity.MagicDefence + Entity.MagicDefence), Entity.Dodge, Entity.PhysicalDamageDecrease, Entity.MagicDamageDecrease, Entity.PhysicalDamageIncrease, Entity.MagicDamageIncrease, Entity.Hitpoints, Entity.MaxHitpoints, Entity.Mana, Entity.MaxMana, Entity.BattlePower });
        //    this.Send(Msg);
        //}

        //private bool AreStatsLoadable(ConquerItem item)
        //{
        //    if (!AlternateEquipment)
        //        if (item.Position > 20)
        //            return false;
        //    if (AlternateEquipment)
        //        if (item.Position < 20)
        //            if (!Equipment.Free((byte)(20 + item.Position)))
        //                return false;

        //    int Position = item.Position;
        //    if (item.Position > 20) Position -= 20;

        //    if (Position == ConquerItem.LeftWeapon || Position == ConquerItem.RightWeapon)
        //        return false;

        //    return true;
        //}

        //private Tuple<ConquerItem, ConquerItem> ComputeWeapons()
        //{
        //    if (!AlternateEquipment)
        //    {
        //        return new Tuple<ConquerItem, ConquerItem>(
        //            Equipment.TryGetItem(ConquerItem.RightWeapon),
        //            Equipment.TryGetItem(ConquerItem.LeftWeapon));
        //    }
        //    else
        //    {
        //        if (Equipment.Free(ConquerItem.AlternateRightWeapon))
        //        {
        //            return new Tuple<ConquerItem, ConquerItem>(
        //                Equipment.TryGetItem(ConquerItem.RightWeapon),
        //                Equipment.TryGetItem(ConquerItem.LeftWeapon));
        //        }
        //        else
        //        {
        //            if (Equipment.Free(ConquerItem.RightWeapon))
        //            {
        //                return new Tuple<ConquerItem, ConquerItem>(
        //                    Equipment.TryGetItem(ConquerItem.AlternateRightWeapon),
        //                    Equipment.TryGetItem(ConquerItem.AlternateLeftWeapon));
        //            }
        //            else
        //            {
        //                if (!Equipment.Free(ConquerItem.AlternateLeftWeapon))
        //                {
        //                    return new Tuple<ConquerItem, ConquerItem>(
        //                        Equipment.TryGetItem(ConquerItem.AlternateRightWeapon),
        //                        Equipment.TryGetItem(ConquerItem.AlternateLeftWeapon));
        //                }
        //                else
        //                {
        //                    if (Equipment.Free(ConquerItem.LeftWeapon))
        //                    {
        //                        return new Tuple<ConquerItem, ConquerItem>(
        //                            Equipment.TryGetItem(ConquerItem.AlternateRightWeapon),
        //                            null);
        //                    }
        //                    else
        //                    {
        //                        ConquerItem aRight = Equipment.TryGetItem(ConquerItem.AlternateRightWeapon),
        //                                     nLeft = Equipment.TryGetItem(ConquerItem.LeftWeapon);
        //                        if (PacketHandler.IsTwoHand(aRight.ID))
        //                        {
        //                            if (PacketHandler.IsFranko(nLeft.ID))
        //                            {
        //                                if (PacketHandler.IsBow(aRight.ID))
        //                                {
        //                                    return new Tuple<ConquerItem,
        //                                        ConquerItem>(aRight, nLeft);
        //                                }
        //                                else
        //                                {
        //                                    return new Tuple<ConquerItem,
        //                                        ConquerItem>(aRight, null);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (PacketHandler.IsShield(nLeft.ID))
        //                                {
        //                                    if (!Spells.ContainsKey(10311))//Perseverance
        //                                    {
        //                                        Send(new Message("You need to know Perseverance (Pure Warrior skill) to be able to wear 2-handed weapon and shield.", System.Drawing.Color.Red, Message.Talk));
        //                                        return new Tuple<ConquerItem,
        //                                            ConquerItem>(aRight, null);
        //                                    }
        //                                    else
        //                                    {
        //                                        return new Tuple<ConquerItem,
        //                                            ConquerItem>(aRight, nLeft);
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    return new Tuple<ConquerItem,
        //                                        ConquerItem>(aRight, null);
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (!PacketHandler.IsTwoHand(nLeft.ID))
        //                            {
        //                                return new Tuple<ConquerItem,
        //                                    ConquerItem>(aRight, nLeft);
        //                            }
        //                            else
        //                            {
        //                                return new Tuple<ConquerItem,
        //                                    ConquerItem>(aRight, null);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //public int[][] ChampionAllowedStats = new int[][]
        //{
        //    new int[] {1, 0, 0, 0, 0, 0, 0, 30, 0, 0 },
        //    new int[] {2, 0, 0, 0, 0, 0, 0, 40, 1, 1 },
        //    new int[] {3, 1, 0, 0, 0, 0, 50, 50, 2, 3 },
        //    new int[] {4, 3, 1, 1, 0, 0, 100, 60, 5, 4 },
        //    new int[] {5, 5, 1, 1, 1, 0, 150, 70, 7, 5 },
        //    new int[] {6, 5, 1, 1, 1, 0, 200, 80, 9, 7 },
        //    new int[] {12, 7, 2, 2, 1, 1, 255, 100, 12, 9 }
        //};
        //public bool DoChampStats { get { return ChampionGroup != null; } }
        //private int _accuracy;
        //public int Accuracy
        //{
        //    get { return _accuracy; }
        //}
        //public void LoadItemStats()
        //{
        //    uint bStats = Entity.Hitpoints;
        //    for (int i = 0; i < 30; i++)
        //        if (Equipment.Objects[i] != null)
        //            Equipment.Objects[i].IsWorn = false;

        //    #region Set Every Variable to Zero
        //    Entity.Defence = 0;
        //    Entity.MagicDefence = 0;
        //    Entity.MagicDefencePercent = 0;
        //    Entity.BaseMagicAttack = 0;
        //    Entity.BaseMagicDefence =
        //    Entity.BaseMaxAttack =
        //    Entity.BaseMinAttack =
        //    Entity.PhysicalDamageDecrease =
        //    Entity.PhysicalDamageIncrease =
        //    Entity.MagicDamageDecrease =
        //    Entity.MagicDamageIncrease = 0;
        //    Entity.ItemHP = 0;
        //    Entity.ItemHP =
        //    Entity.ItemMP =
        //    Entity.AttackRange = (byte)0;
        //    Entity.Dodge = 0;
        //    Entity.MinAttack = 0;
        //    Entity.MaxAttack = 0;
        //    Entity.Defence = 0;
        //    Entity.SuperItemBless = 0;
        //    Entity.MagicDefence = 0;
        //    Entity.Dodge = 0;
        //    Entity.BaseMagicAttack = 0;
        //    Entity.WoodResistance = 0;
        //    Entity.FireResistance = 0;
        //    Entity.WaterResistance = 0;
        //    Entity.EarthResistance = 0;
        //    Entity.Breaktrough = 0;
        //    Entity.WearsGoldPrize = false;
        //    Entity.CriticalStrike = 0;
        //    Entity.Immunity = 0;
        //    Entity.Penetration = 0;
        //    Entity.Counteraction = 0;
        //    Entity.Block = 0;
        //    _accuracy = 0;
        //    Entity.Detoxication = 0;
        //    Entity.Intensification = 0;
        //    Entity.Penetration = 0;
        //    Entity.SkillCStrike = 0;
        //    Entity.MaxAttack = 0;
        //    Entity.MinAttack = 0;
        //    Entity.PhysicalDamageDecrease = 0;
        //    Entity.MagicDamageDecrease = 0;
        //    Entity.MagicDamageIncrease = 0;
        //    Entity.PhysicalDamageIncrease = 0;
        //    Entity.MagicDefencePercent = 0;
        //    Entity.ItemHP = 0;
        //    Entity.ItemMP = 0;
        //    Entity.ItemBless = 0;
        //    Entity.AttackRange = 1;
        //    Entity.BaseMinAttack = 0;
        //    Entity.BaseMaxAttack = 0;
        //    Entity.BaseMagicDefence = 0;
        //    Entity.BaseDefence = 0;
        //    Entity.MagicDamageIncrease = 0;
        //    Entity.Gems = new double[10];
        //    #endregion

        //    foreach (ConquerItem i in Equipment.Objects)
        //    {
        //        if (i == null) continue;
        //        if (i.Durability == 0) continue;
        //        if (!AreStatsLoadable(i)) continue;
        //        loadItemStats(i);
        //    }

        //    Weapons = ComputeWeapons();
        //    if (Weapons == null) Weapons = new Tuple<ConquerItem, ConquerItem>(null, null);
        //    if (Weapons.Item1 != null)
        //    {
        //        loadItemStats(Weapons.Item1);
        //        if (Weapons.Item2 != null)
        //        {
        //            if (!Weapons.Item1.IsTwoHander())
        //                loadItemStats(Weapons.Item2);
        //            else
        //                if (PacketHandler.IsFranko(Weapons.Item2.ID) || (Entity.Class >= 20 && Entity.Class <= 25))
        //                    loadItemStats(Weapons.Item2);
        //        }
        //    }

        //    #region Subclasses
        //    foreach (var c in Entity.SubClasses.Classes)
        //    {
        //        int lvl = c.Value.Level;
        //        if (DoChampStats) lvl = Math.Min(lvl, ChampionAllowedStats[ChampionStats.Grade][9]);
        //        switch ((MTA.Game.ClassID)c.Value.ID)
        //        {
        //            case Game.ClassID.MartialArtist:
        //                {
        //                    Entity.CriticalStrike += (ushort)(Entity.CriticalStrike - (Entity.CriticalStrike * (lvl / 15)));
        //                    break;
        //                }
        //            case Game.ClassID.Warlock:
        //                {
        //                    Entity.SkillCStrike += (ushort)(Entity.SkillCStrike - (Entity.SkillCStrike * (lvl / 15)));
        //                    break;
        //                }
        //            case Game.ClassID.ChiMaster:
        //                {
        //                    Entity.Immunity += (ushort)(Entity.Immunity - (Entity.Immunity * (lvl / 15)));
        //                    break;
        //                }
        //            case Game.ClassID.Sage:
        //                {
        //                    Entity.Penetration += (ushort)(Entity.Penetration - (Entity.Penetration * (lvl / 15)));
        //                    break;
        //                }
        //            case Game.ClassID.Apothecary:
        //                {
        //                    //double per = lvl * 8 / 10;
        //                    Entity.Detoxication += (ushort)(Entity.Detoxication - (Entity.Detoxication * (lvl / 15)));//per));
        //                    break;
        //                }
        //            case Game.ClassID.Performer:
        //                {
        //                    int per = lvl * 100;
        //                    Entity.BaseMaxAttack += (uint)per / 2;
        //                    Entity.BaseMinAttack += (uint)per / 2;
        //                    Entity.BaseMagicAttack += (uint)per;
        //                    break;
        //                }
        //        }
        //    }
        //    #endregion
        //    #region Chi
        //    uint percentage = 100;
        //    if (DoChampStats)
        //        percentage = (uint)ChampionAllowedStats[ChampionStats.Grade][7];
        //    foreach (var chiPower in ChiPowers)
        //    {
        //        foreach (var attribute in chiPower.Attributes)
        //        {
        //            switch (attribute.Type)
        //            {
        //                case Game.Enums.ChiAttribute.CriticalStrike:
        //                    Entity.CriticalStrike += (int)((ushort)(attribute.Value * 10) * percentage / 100);
        //                    break;
        //                case Game.Enums.ChiAttribute.Counteraction:
        //                    Entity.Counteraction += (ushort)(attribute.Value * percentage / 100);
        //                    break;
        //                case Game.Enums.ChiAttribute.AddAttack:
        //                    Entity.BaseMinAttack += attribute.Value * percentage / 100;
        //                    Entity.BaseMaxAttack += attribute.Value * percentage / 100;
        //                    break;
        //                case Game.Enums.ChiAttribute.AddMagicAttack:
        //                    Entity.BaseMagicAttack += attribute.Value * percentage / 100;
        //                    break;
        //                case Game.Enums.ChiAttribute.AddMagicDefense:
        //                    Entity.BaseMagicDefence += attribute.Value * percentage / 100;
        //                    break;
        //                case Game.Enums.ChiAttribute.Breakthrough:
        //                    Entity.Breaktrough += (ushort)(attribute.Value * percentage / 100);
        //                    break;
        //                case Game.Enums.ChiAttribute.HPAdd:
        //                    Entity.ItemHP += attribute.Value * percentage / 100;
        //                    break;
        //                case Game.Enums.ChiAttribute.Immunity:
        //                    Entity.Immunity += (int)((ushort)(attribute.Value * 10) * percentage / 100);
        //                    break;
        //                case Game.Enums.ChiAttribute.MagicDamageDecrease:
        //                    Entity.MagicDamageDecrease += (ushort)(attribute.Value * percentage / 100);
        //                    break;
        //                case Game.Enums.ChiAttribute.MagicDamageIncrease:
        //                    Entity.MagicDamageIncrease += (ushort)(attribute.Value * percentage / 100);
        //                    break;
        //                case Game.Enums.ChiAttribute.PhysicalDamageDecrease:
        //                    Entity.PhysicalDamageDecrease += (ushort)(attribute.Value * percentage / 100);
        //                    break;
        //                case Game.Enums.ChiAttribute.PhysicalDamageIncrease:
        //                    Entity.PhysicalDamageIncrease += (ushort)(attribute.Value * percentage / 100);
        //                    break;
        //                case Game.Enums.ChiAttribute.SkillCriticalStrike:
        //                    Entity.SkillCStrike += (int)((ushort)(attribute.Value * 10) * percentage / 100);
        //                    break;
        //            }
        //        }
        //    }
        //    #region Dragon Ranking
        //    if (ChiData.DragonRank <= 50 && ChiPowers.Count > 0)
        //    {
        //        if (ChiData.DragonRank <= 3)
        //        {
        //            Entity.ItemHP += 5000;
        //            Entity.BaseMagicDefence += 300;
        //            Entity.PhysicalDamageDecrease += 1000;
        //            Entity.MagicDamageDecrease += 300;
        //        }
        //        else if (ChiData.DragonRank <= 15)
        //        {
        //            Entity.ItemHP += (uint)(3000 - (ChiData.DragonRank - 4) * 90);
        //            Entity.BaseMagicDefence += (uint)(250 - (ChiData.DragonRank - 4) * 9);
        //            Entity.PhysicalDamageDecrease += (ushort)(600 - (ChiData.DragonRank - 4) * 18);
        //            Entity.MagicDamageDecrease += (ushort)(200 - (ChiData.DragonRank - 4) * 4);
        //        }
        //        else if (ChiData.DragonRank <= 50)
        //        {
        //            Entity.ItemHP += 1500;
        //            Entity.BaseMagicDefence += 100;
        //            Entity.PhysicalDamageDecrease += 300;
        //            Entity.MagicDamageDecrease += 100;
        //        }
        //    }
        //    #endregion
        //    #region Phoenix Ranking
        //    if (ChiData.PhoenixRank <= 50 && ChiPowers.Count > 1)
        //    {
        //        if (ChiData.PhoenixRank <= 3)
        //        {
        //            Entity.BaseMinAttack += 3000;
        //            Entity.BaseMaxAttack += 3000;
        //            Entity.BaseMagicAttack += 3000;
        //            Entity.PhysicalDamageIncrease += 1000;
        //            Entity.MagicDamageIncrease += 300;
        //        }
        //        else if (ChiData.PhoenixRank <= 15)
        //        {
        //            Entity.BaseMinAttack += (uint)(2000 - (ChiData.PhoenixRank - 4) * 45);
        //            Entity.BaseMaxAttack += (uint)(2000 - (ChiData.PhoenixRank - 4) * 45);
        //            Entity.BaseMagicAttack += (uint)(2000 - (ChiData.PhoenixRank - 4) * 45);
        //            Entity.PhysicalDamageIncrease += (ushort)(600 - (ChiData.PhoenixRank - 4) * 18);
        //            Entity.MagicDamageIncrease += (ushort)(200 - (ChiData.PhoenixRank - 4) * 4);
        //        }
        //        else if (ChiData.PhoenixRank <= 50)
        //        {
        //            Entity.BaseMinAttack += 1000;
        //            Entity.BaseMaxAttack += 1000;
        //            Entity.BaseMagicAttack += 1000;
        //            Entity.PhysicalDamageIncrease += 300;
        //            Entity.MagicDamageIncrease += 100;
        //        }
        //    }
        //    #endregion
        //    #region Tiger Ranking
        //    if (ChiData.TigerRank <= 50 && ChiPowers.Count > 2)
        //    {
        //        if (ChiData.TigerRank <= 3)
        //        {
        //            Entity.CriticalStrike += 1500;
        //            Entity.SkillCStrike += 1500;
        //            Entity.Immunity += 800;
        //        }
        //        else if (ChiData.TigerRank <= 15)
        //        {
        //            Entity.CriticalStrike += (ushort)(1100 - (ChiData.TigerRank - 4) * 10);
        //            Entity.SkillCStrike += (ushort)(1100 - (ChiData.TigerRank - 4) * 10);
        //            Entity.Immunity += 500;
        //        }
        //        else if (ChiData.TigerRank <= 50)
        //        {
        //            Entity.CriticalStrike += 500;
        //            Entity.SkillCStrike += 500;
        //            Entity.Immunity += 200;
        //        }
        //    }
        //    #endregion
        //    #region Turtle Ranking
        //    if (ChiData.TurtleRank <= 50 && ChiPowers.Count > 3)
        //    {
        //        if (ChiData.TurtleRank <= 3)
        //        {
        //            Entity.Breaktrough += 150;
        //            Entity.Counteraction += 150;
        //            Entity.Immunity += 800;
        //        }
        //        else if (ChiData.TurtleRank <= 15)
        //        {
        //            Entity.Breaktrough += (ushort)(110 - (ChiData.TurtleRank - 4) * 1);
        //            Entity.Counteraction += (ushort)(110 - (ChiData.TurtleRank - 4) * 1);
        //            Entity.Immunity += 500;
        //        }
        //        else if (ChiData.TurtleRank <= 50)
        //        {
        //            Entity.Breaktrough += 50;
        //            Entity.Counteraction += 50;
        //            Entity.Immunity += 200;
        //        }
        //    }
        //    #endregion
        //    #endregion

        //    if (Entity.MyJiang != null)
        //    {
        //        Entity.MyJiang.CreateStatusAtributes(Entity);
        //    }
            
        //    if (Entity.Aura_isActive)
        //        doAuraBonuses(Entity.Aura_actType, Entity.Aura_actPower, 1);
        //    else
        //        removeAuraBonuses(Entity.Aura_actType, Entity.Aura_actPower, 1);
        //    if (TeamAura)
        //        doAuraBonuses(TeamAuraStatusFlag, TeamAuraPower, 1);
        //    else
        //        removeAuraBonuses(TeamAuraStatusFlag, TeamAuraPower, 1);
        //    if (Entity.Class >= 60 && Entity.Class <= 65)
        //        Entity.AttackRange += 2;

        //    /*if (Entity.CriticalStrike > 9000)
        //        Entity.CriticalStrike = 9000;*/

        //    Entity.Hitpoints = bStats;
        //    CalculateStatBonus();
        //    CalculateHPBonus();
        //    ReviewMentor();
        //    GemAlgorithm();
        //    Entity.TrojanBP = (uint)Entity.BattlePower;
        //}

        //public void doAuraBonuses(ulong type, uint power, int i)
        //{
        //    switch (type)
        //    {
        //        case Update.Flags2.EarthAura:
        //            {
        //                Entity.EarthResistance += (int)power * i;
        //                break;
        //            }
        //        case Update.Flags2.FireAura:
        //            {
        //                Entity.FireResistance += (int)power * i;
        //                break;
        //            }
        //        case Update.Flags2.MetalAura:
        //            {
        //                Entity.MetalResistance += (int)power * i;
        //                break;
        //            }
        //        case Update.Flags2.WoodAura:
        //            {
        //                Entity.WoodResistance += (int)power * i;
        //                break;
        //            }
        //        case Update.Flags2.WaterAura:
        //            {
        //                Entity.WaterResistance += (int)power * i;
        //                break;
        //            }
        //        case Update.Flags2.TyrantAura:
        //            {
        //                Entity.CriticalStrike += (int)power * i * 100;
        //                Entity.SkillCStrike += (int)power * i * 100;
        //                if (Entity.CriticalStrike > 120000) Entity.CriticalStrike = 120000;
        //                if (Entity.SkillCStrike > 120000) Entity.SkillCStrike = 120000;
        //                if (Entity.CriticalStrike < 0) Entity.CriticalStrike = 0;
        //                if (Entity.SkillCStrike < 0) Entity.SkillCStrike = 0;
        //                break;
        //            }
        //        case Update.Flags2.FendAura:
        //            {
        //                Entity.Immunity += (int)power * i * 100;
        //                break;
        //            }
        //    }
        //}

        //public void removeAuraBonuses(ulong type, uint power, int i)
        //{
        //    switch (type)
        //    {
        //        case Update.Flags2.EarthAura:
        //            {
        //                Entity.EarthResistance -= (int)power * i;
        //                break;
        //            }
        //        case Update.Flags2.FireAura:
        //            {
        //                Entity.FireResistance -= (int)power * i;
        //                break;
        //            }
        //        case Update.Flags2.MetalAura:
        //            {
        //                Entity.MetalResistance -= (int)power * i;
        //                break;
        //            }
        //        case Update.Flags2.WoodAura:
        //            {
        //                Entity.WoodResistance -= (int)power * i;
        //                break;
        //            }
        //        case Update.Flags2.WaterAura:
        //            {
        //                Entity.WaterResistance -= (int)power * i;
        //                break;
        //            }
        //        case Update.Flags2.TyrantAura:
        //            {
        //                Entity.CriticalStrike -= (int)power * i * 100;
        //                Entity.SkillCStrike -= (int)power * i * 100;
        //                if (Entity.CriticalStrike > 120000) Entity.CriticalStrike = 120000;
        //                if (Entity.SkillCStrike > 120000) Entity.SkillCStrike = 120000;
        //                if (Entity.CriticalStrike < 0) Entity.CriticalStrike = 0;
        //                if (Entity.SkillCStrike < 0) Entity.SkillCStrike = 0;
        //                break;
        //            }
        //        case Update.Flags2.FendAura:
        //            {
        //                Entity.Immunity -= (int)power * i * 100;
        //                break;
        //            }
        //    }
        //}

        //private void loadItemStats(ConquerItem item)
        //{
        //    if (item.ID == ConquerItem.GoldPrize) Entity.WearsGoldPrize = true;
        //    int position = item.Position;
        //    bool isOver = false;
        //    if (isOver = (position > 20))
        //        position -= 20;
        //    item.IsWorn = true;
        //    if (!isOver)
        //    {
        //        if (position == ConquerItem.Garment || position == ConquerItem.Tower || position == ConquerItem.Fan || position == ConquerItem.RightWeaponAccessory || position == ConquerItem.LeftWeaponAccessory)
        //            Entity.SuperItemBless += item.Bless;
        //        if (position == ConquerItem.SteedArmor || position == ConquerItem.LeftWeaponAccessory || position == ConquerItem.RightWeaponAccessory) return;
        //    }
        //    int plus = item.Plus;
        //    if (DoChampStats)
        //        plus = Math.Min(item.Plus, ChampionAllowedStats[ChampionStats.Grade][0]);
        //    Database.ConquerItemInformation dbi = new Database.ConquerItemInformation(item.ID, item.Plus);
        //    if (dbi != null)
        //    {
        //        #region Give Stats.

        //        if (DoChampStats && ChampionAllowedStats[ChampionStats.Grade][5] == 1 || !DoChampStats)
        //        {
        //            if (item.Purification.PurificationItemID != 0)
        //            {
        //                Database.ConquerItemInformation soulDB = new Database.ConquerItemInformation(item.Purification.PurificationItemID, 0);
        //                /*if (position == ConquerItem.LeftWeapon)
        //                {
        //                    Entity.BaseMinAttack += (uint)(soulDB.BaseInformation.MinAttack / 2);
        //                    Entity.BaseMaxAttack += (uint)(soulDB.BaseInformation.MaxAttack / 2);
        //                }
        //                else
        //                {
        //                    Entity.BaseMinAttack += soulDB.BaseInformation.MinAttack;
        //                    Entity.BaseMaxAttack += soulDB.BaseInformation.MaxAttack;
        //                }*/
        //                Entity.BaseMinAttack += soulDB.BaseInformation.MinAttack;
        //                Entity.BaseMaxAttack += soulDB.BaseInformation.MaxAttack;
        //                Entity.ItemHP += soulDB.BaseInformation.ItemHP;
        //                Entity.BaseDefence += soulDB.BaseInformation.PhysicalDefence;
        //                Entity.MagicDefence += soulDB.BaseInformation.MagicDefence;
        //                Entity.Dodge += soulDB.BaseInformation.Dodge;
        //                Entity.Owner._accuracy += soulDB.BaseInformation.Accuracy;
        //                Entity.BaseMagicAttack += soulDB.BaseInformation.MagicAttack;
        //                Entity.WoodResistance += soulDB.BaseInformation.WoodResist;
        //                Entity.FireResistance += soulDB.BaseInformation.FireResist;
        //                Entity.WaterResistance += soulDB.BaseInformation.WaterResist;
        //                Entity.EarthResistance += soulDB.BaseInformation.EarthResist;
        //                Entity.Breaktrough += soulDB.BaseInformation.BreakThrough;
        //                Entity.CriticalStrike += soulDB.BaseInformation.CriticalStrike;
        //                Entity.Immunity += soulDB.BaseInformation.Immunity;
        //                Entity.Penetration += soulDB.BaseInformation.Penetration;
        //                Entity.Counteraction += soulDB.BaseInformation.CounterAction;
        //                Entity.Block += soulDB.BaseInformation.Block;
        //            }
        //        }
        //        if (DoChampStats && ChampionAllowedStats[ChampionStats.Grade][4] == 1 || !DoChampStats)
        //        {
        //            Refinery.RefineryItem refine = null;
        //            if (item.ExtraEffect.Available)
        //            {
        //                if (Kernel.DatabaseRefinery.TryGetValue(item.ExtraEffect.EffectID, out refine))
        //                {
        //                    if (refine != null)
        //                    {
        //                        switch (refine.Type)
        //                        {
        //                            case Refinery.RefineryItem.RefineryType.Block:
        //                                Entity.Block += (UInt16)(refine.Percent * 100);
        //                                break;
        //                            case Refinery.RefineryItem.RefineryType.BreakThrough:
        //                                Entity.Breaktrough += (UInt16)((refine.Percent * 10));
        //                                break;
        //                            case Refinery.RefineryItem.RefineryType.Counteraction:
        //                                Entity.Counteraction += (UInt16)(refine.Percent * 10);
        //                                break;
        //                            case Refinery.RefineryItem.RefineryType.Critical:
        //                                Entity.CriticalStrike += (UInt16)((refine.Percent * 100));
        //                                break;
        //                            case Refinery.RefineryItem.RefineryType.Detoxication:
        //                                Entity.Detoxication += (UInt16)(refine.Percent);
        //                                break;
        //                            case Refinery.RefineryItem.RefineryType.Immunity:
        //                                Entity.Immunity += (UInt16)(refine.Percent * 100);
        //                                break;
        //                            case Refinery.RefineryItem.RefineryType.Intensification:
        //                                Entity.Intensification += (UInt16)(refine.Percent);
        //                                break;
        //                            case Refinery.RefineryItem.RefineryType.Penetration:
        //                                Entity.Penetration += (UInt16)(refine.Percent * 100);
        //                                break;
        //                            case Refinery.RefineryItem.RefineryType.SCritical:
        //                                Entity.SkillCStrike += (UInt16)(refine.Percent * 100);
        //                                break;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        if (position == ConquerItem.Tower)
        //        {
        //            Entity.PhysicalDamageDecrease += dbi.BaseInformation.PhysicalDefence;
        //            Entity.MagicDamageDecrease += dbi.BaseInformation.MagicDefence;
        //        }
        //        else
        //        {
        //            Entity.BaseDefence += dbi.BaseInformation.PhysicalDefence;
        //            Entity.MagicDefencePercent += dbi.BaseInformation.MagicDefence;
        //            Entity.Dodge += (byte)dbi.BaseInformation.Dodge;
        //            if (position != ConquerItem.Fan)
        //                Entity.BaseMagicAttack += dbi.BaseInformation.MagicAttack;
        //        }
        //        Entity.ItemHP += dbi.BaseInformation.ItemHP;
        //        Entity.ItemMP += dbi.BaseInformation.ItemMP;
        //        if (item.Position != ConquerItem.Steed)
        //        {
        //            if (DoChampStats)
        //                Entity.ItemBless += (ushort)Math.Min(item.Bless, ChampionAllowedStats[ChampionStats.Grade][1]);
        //            else
        //                Entity.ItemBless += item.Bless;
        //        }
        //        if (position == ConquerItem.RightWeapon)
        //        {
        //            Entity.AttackRange += dbi.BaseInformation.AttackRange;
        //            if (Network.PacketHandler.IsTwoHand(dbi.BaseInformation.ID))
        //                Entity.AttackRange += 4;
        //            else
        //                Entity.AttackRange += 3;
        //        }
        //        /*if (position == ConquerItem.LeftWeapon)
        //        {
        //            Entity.BaseMinAttack += (uint)(dbi.BaseInformation.MinAttack / 2);
        //            Entity.BaseMaxAttack += (uint)(dbi.BaseInformation.MaxAttack / 2);
        //        }*/
        //        if (position == ConquerItem.LeftWeapon)
        //        {
        //            Entity.BaseMinAttack += (uint)(dbi.BaseInformation.MinAttack * 0.5F);
        //            Entity.BaseMaxAttack += (uint)(dbi.BaseInformation.MaxAttack * 0.5F);
        //        }
        //        else if (position == ConquerItem.Fan)
        //        {
        //            Entity.PhysicalDamageIncrease += dbi.BaseInformation.MinAttack;
        //            Entity.MagicDamageIncrease += dbi.BaseInformation.MagicAttack;
        //        }
        //        else
        //        {
        //            Entity.BaseMinAttack += dbi.BaseInformation.MinAttack;
        //            Entity.BaseMaxAttack += dbi.BaseInformation.MaxAttack;
        //        }
        //        if (item.Plus != 0)
        //        {
        //            if (position == ConquerItem.Tower)
        //            {
        //                Entity.PhysicalDamageDecrease += dbi.PlusInformation.PhysicalDefence;
        //                Entity.MagicDamageDecrease += (ushort)dbi.PlusInformation.MagicDefence;
        //            }
        //            else if (position == ConquerItem.Fan)
        //            {
        //                Entity.PhysicalDamageIncrease += (ushort)dbi.PlusInformation.MinAttack;
        //                Entity.MagicDamageIncrease += (ushort)dbi.PlusInformation.MagicAttack;
        //            }
        //            else
        //            {
        //                if (position == ConquerItem.Steed)
        //                    Entity.ExtraVigor += dbi.PlusInformation.Agility;
        //                Entity.BaseMinAttack += dbi.PlusInformation.MinAttack;
        //                Entity.BaseMaxAttack += dbi.PlusInformation.MaxAttack;
        //                Entity.BaseMagicAttack += dbi.PlusInformation.MagicAttack;
        //                Entity.BaseDefence += dbi.PlusInformation.PhysicalDefence;
        //                Entity.MagicDefence += dbi.PlusInformation.MagicDefence;
        //                Entity.ItemHP += dbi.PlusInformation.ItemHP;
        //                if (position == ConquerItem.Boots)
        //                    Entity.Dodge += (byte)dbi.PlusInformation.Dodge;
        //            }
        //        }
        //        if (position == ConquerItem.Garment)
        //        {
        //            if (item.ID == 187425)
        //            {
        //                Entity.BaseDefence += 400;
        //                Entity.BaseMagicDefence += 2;
        //            }
        //            else if (item.ID == 187415)
        //            {
        //                Entity.BaseDefence += 600;
        //                Entity.BaseMagicDefence += 3;
        //            }
        //            else if (item.ID == 187405)
        //            {
        //                Entity.BaseDefence += 800;
        //                Entity.BaseMagicDefence += 4;
        //            }
        //        }
        //        byte socketone = (byte)item.SocketOne;
        //        byte sockettwo = (byte)item.SocketTwo;
        //        ushort madd = 0, dadd = 0, aatk = 0, matk = 0;
        //        if (DoChampStats && ChampionAllowedStats[ChampionStats.Grade][2] >= 1 || !DoChampStats)
        //        {
        //            switch (socketone)
        //            {
        //                case 1: Entity.Gems[0] += 5; break;
        //                case 2: Entity.Gems[0] += 10; break;
        //                case 3: Entity.Gems[0] += 15; break;

        //                case 11: Entity.Gems[1] += .05; break;
        //                case 12: Entity.Gems[1] += .10; break;
        //                case 13: Entity.Gems[1] += .15; break;

        //                case 31: Entity.Gems[3] += 10; break;
        //                case 32: Entity.Gems[3] += 15; break;
        //                case 33: Entity.Gems[3] += 25; break;

        //                case 51: Entity.Gems[5] += 30; break;
        //                case 52: Entity.Gems[5] += 50; break;
        //                case 53: Entity.Gems[5] += 100; break;

        //                case 61: Entity.Gems[6] += 15; break;
        //                case 62: Entity.Gems[6] += 30; break;
        //                case 63: Entity.Gems[6] += 50; break;

        //                case 71: Entity.Gems[7] += .2; break;
        //                case 72: Entity.Gems[7] += .4; break;
        //                case 73: Entity.Gems[7] += .6; break;

        //                case 101: aatk = matk += 100; break;
        //                case 102: aatk = matk += 300; break;
        //                case 103: aatk = matk += 500; break;

        //                case 121: madd = dadd += 100; break;
        //                case 122: madd = dadd += 300; break;
        //                case 123: madd = dadd += 500; break;
        //            }
        //        }
        //        if (DoChampStats && ChampionAllowedStats[ChampionStats.Grade][2] >= 2 || !DoChampStats)
        //        {
        //            switch (sockettwo)
        //            {
        //                case 1: Entity.Gems[0] += 5; break;
        //                case 2: Entity.Gems[0] += 10; break;
        //                case 3: Entity.Gems[0] += 15; break;

        //                case 11: Entity.Gems[1] += .05; break;
        //                case 12: Entity.Gems[1] += .10; break;
        //                case 13: Entity.Gems[1] += .15; break;

        //                case 31: Entity.Gems[3] += 10; break;
        //                case 32: Entity.Gems[3] += 15; break;
        //                case 33: Entity.Gems[3] += 25; break;

        //                case 51: Entity.Gems[5] += 30; break;
        //                case 52: Entity.Gems[5] += 50; break;
        //                case 53: Entity.Gems[5] += 100; break;

        //                case 61: Entity.Gems[6] += 15; break;
        //                case 62: Entity.Gems[6] += 30; break;
        //                case 63: Entity.Gems[6] += 50; break;

        //                case 71: Entity.Gems[7] += 2; break;
        //                case 72: Entity.Gems[7] += 4; break;
        //                case 73: Entity.Gems[7] += 6; break;

        //                case 101: aatk = matk += 100; break;
        //                case 102: aatk = matk += 300; break;
        //                case 103: aatk = matk += 500; break;

        //                case 121: madd = dadd += 100; break;
        //                case 122: madd = dadd += 300; break;
        //                case 123: madd = dadd += 500; break;
        //            }
        //        }
        //        Entity.PhysicalDamageDecrease += dadd;
        //        Entity.MagicDamageDecrease += madd;
        //        Entity.PhysicalDamageIncrease += aatk;
        //        Entity.MagicDamageIncrease += matk;
        //        if (item.Position != ConquerItem.Steed)
        //            if (!DoChampStats)
        //                Entity.ItemHP += item.Enchant;
        //            else
        //                Entity.ItemHP += (uint)Math.Min(item.Enchant, ChampionAllowedStats[ChampionStats.Grade][6]);

        //        #endregion
        //    }
        //}
        //public void GemAlgorithm()
        //{
        //    Entity.MaxAttack = Entity.Strength + Entity.BaseMaxAttack;
        //    Entity.MinAttack = Entity.Strength + Entity.BaseMinAttack;
        //    Entity.MagicAttack = Entity.BaseMagicAttack;
        //    if (Entity.Gems[0] != 0)
        //    {
        //        Entity.MagicAttack += (uint)Math.Floor(Entity.MagicAttack * (double)(Entity.Gems[0] * 0.01));
        //    }
        //    if (Entity.Gems[1] != 0)
        //    {
        //        Entity.MaxAttack += (uint)Math.Floor(Entity.MaxAttack * (double)(Entity.Gems[1] * 0.003));
        //        Entity.MinAttack += (uint)Math.Floor(Entity.MinAttack * (double)(Entity.Gems[1] * 0.003));
        //    }
        //}
        //public void GemAlgorithm()
        //{
        //    Entity.MaxAttack = Entity.BaseMaxAttack + Entity.Strength;
        //    Entity.MinAttack = Entity.BaseMinAttack + Entity.Strength;
        //    Entity.MagicAttack = Entity.BaseMagicAttack;
        //}

        #endregion

        public int Accuracy
        {
            get
            {
                if (Entity.EntityFlag == Game.EntityFlag.Monster)
                    return 0;
                int _accuracy = 0;
                foreach (ConquerItem i in Equipment.Objects)
                {
                    if (i == null)
                    {
                        continue;
                    }
                    if (i.Position == Network.GamePackets.ConquerItem.LeftWeapon ||
                        i.Position == Network.GamePackets.ConquerItem.RightWeapon)
                    {
                        Database.ConquerItemInformation dbi = new Database.ConquerItemInformation(i.ID, i.Plus);
                        if (dbi != null)
                        {
                            _accuracy += dbi.PlusInformation.Agility;
                        }
                    }
                }
                return _accuracy;
            }
        }
        public ushort AgilityItem
        {
            get
            {
                if (Entity.EntityFlag == Game.EntityFlag.Monster)
                    return 0;
                ushort _AgilityItem = 0;
                foreach (ConquerItem i in Equipment.Objects)
                {
                    if (i == null)
                    {
                        continue;
                    }
                    Database.ConquerItemInformation dbi = new Database.ConquerItemInformation(i.ID, i.Plus);
                    if (dbi != null)
                    {
                        _AgilityItem += dbi.BaseInformation.Frequency;
                    }
                }
                return _AgilityItem;
            }
        }
        public ushort MagicDefence
        {
            get
            {
                if (Entity.EntityFlag == Game.EntityFlag.Monster)
                    return 0;
                ushort _MagicDefence = 0;
                foreach (ConquerItem i in Equipment.Objects)
                {
                    if (i == null)
                    {
                        continue;
                    }
                    if (i.Position == Network.GamePackets.ConquerItem.Armor ||
                        i.Position == Network.GamePackets.ConquerItem.Necklace ||
                        i.Position == Network.GamePackets.ConquerItem.Head)
                    {
                        Database.ConquerItemInformation dbi = new Database.ConquerItemInformation(i.ID, i.Plus);
                        if (dbi != null)
                        {
                            _MagicDefence += dbi.BaseInformation.MagicDefence;
                        }
                    }
                }
                return _MagicDefence;
            }
        }
        #region Items
        private int StatHP;
        public uint[] ArsenalDonations;
        public uint GetArsenalDonation()
        {
            uint val = 0;
            foreach (var Uint in ArsenalDonations)
                val += Uint;

            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update("entities").Set("GuildArsenalDonation", (uint)val).Where("UID", this.Entity.UID)
                    .Execute();
            if (AsMember != null)
                AsMember.ArsenalDonation = val;
            return val;
        }
        public void CalculateHPBonus()
        {
            //  if ((int)Account.State >= 3) return;
            switch (Entity.Class)
            {
                case 11: Entity.MaxHitpoints = (uint)(StatHP * 1.05F); break;
                case 12: Entity.MaxHitpoints = (uint)(StatHP * 1.08F); break;
                case 13: Entity.MaxHitpoints = (uint)(StatHP * 1.10F); break;
                case 14: Entity.MaxHitpoints = (uint)(StatHP * 1.12F); break;
                case 15: Entity.MaxHitpoints = (uint)(StatHP * 1.15F); break;
                default: Entity.MaxHitpoints = (uint)StatHP; break;
            }
            Entity.MaxHitpoints += Entity.ItemHP;
            Entity.MaxHitpoints += Entity.Intensification;          
            Entity.Hitpoints = Math.Min(Entity.Hitpoints, Entity.MaxHitpoints);
        }
        public void CalculateStatBonus()
        {
            byte ManaBoost = 5;
            const byte HitpointBoost = 24;
            sbyte Class = (sbyte)(Entity.Class / 10);
            if (Class == 13 || Class == 14)
                ManaBoost += (byte)(5 * (Entity.Class - (Class * 10)));
            StatHP = (ushort)((Entity.Strength * 3) +
                                     (Entity.Agility * 3) +
                                     (Entity.Spirit * 3) +
                                     (Entity.Vitality * HitpointBoost));
            Entity.MaxMana = (ushort)((Entity.Spirit * ManaBoost) + Entity.ItemMP);
            Entity.Mana = Math.Min(Entity.Mana, Entity.MaxMana);
        }
        public void SendStatMessage()
        {
            this.ReviewMentor();
            Network.GamePackets.Message Msg = new MTA.Network.GamePackets.Message(" Your status has been changed", System.Drawing.Color.DarkGoldenrod
                , Network.GamePackets.Message.TopLeft);
            Msg.__Message = string.Format(Msg.__Message,
                new object[] { Entity.MinAttack, Entity.MaxAttack, Entity.MagicAttack, Entity.Defence, (Entity.MagicDefence + Entity.MagicDefence), Entity.Dodge, Entity.PhysicalDamageDecrease, Entity.MagicDamageDecrease, Entity.PhysicalDamageIncrease, Entity.MagicDamageIncrease, Entity.Hitpoints, Entity.MaxHitpoints, Entity.Mana, Entity.MaxMana, Entity.BattlePower });
            this.Send(Msg);
        }

        private bool AreStatsLoadable(ConquerItem item)
        {
            if (!AlternateEquipment)
                if (item.Position > 20)
                    return false;
            if (AlternateEquipment)
                if (item.Position < 20)
                    if (!Equipment.Free((byte)(20 + item.Position)))
                        return false;

            int Position = item.Position;
            if (item.Position > 20) Position -= 20;

            if (Position == ConquerItem.LeftWeapon || Position == ConquerItem.RightWeapon)
                return false;

            return true;
        }

        private Tuple<ConquerItem, ConquerItem> ComputeWeapons()
        {
            if (!AlternateEquipment)
            {
                return new Tuple<ConquerItem, ConquerItem>(
                    Equipment.TryGetItem(ConquerItem.RightWeapon),
                    Equipment.TryGetItem(ConquerItem.LeftWeapon));
            }
            else
            {
                if (Equipment.Free(ConquerItem.AlternateRightWeapon))
                {
                    return new Tuple<ConquerItem, ConquerItem>(
                        Equipment.TryGetItem(ConquerItem.RightWeapon),
                        Equipment.TryGetItem(ConquerItem.LeftWeapon));
                }
                else
                {
                    if (Equipment.Free(ConquerItem.RightWeapon))
                    {
                        return new Tuple<ConquerItem, ConquerItem>(
                            Equipment.TryGetItem(ConquerItem.AlternateRightWeapon),
                            Equipment.TryGetItem(ConquerItem.AlternateLeftWeapon));
                    }
                    else
                    {
                        if (!Equipment.Free(ConquerItem.AlternateLeftWeapon))
                        {
                            return new Tuple<ConquerItem, ConquerItem>(
                                Equipment.TryGetItem(ConquerItem.AlternateRightWeapon),
                                Equipment.TryGetItem(ConquerItem.AlternateLeftWeapon));
                        }
                        else
                        {
                            if (Equipment.Free(ConquerItem.LeftWeapon))
                            {
                                return new Tuple<ConquerItem, ConquerItem>(
                                    Equipment.TryGetItem(ConquerItem.AlternateRightWeapon),
                                    null);
                            }
                            else
                            {
                                ConquerItem aRight = Equipment.TryGetItem(ConquerItem.AlternateRightWeapon),
                                             nLeft = Equipment.TryGetItem(ConquerItem.LeftWeapon);
                                if (PacketHandler.IsTwoHand(aRight.ID))
                                {
                                    if (PacketHandler.IsFranko(nLeft.ID))
                                    {
                                        if (PacketHandler.IsBow(aRight.ID))
                                        {
                                            return new Tuple<ConquerItem,
                                                ConquerItem>(aRight, nLeft);
                                        }
                                        else
                                        {
                                            return new Tuple<ConquerItem,
                                                ConquerItem>(aRight, null);
                                        }
                                    }
                                    else
                                    {
                                        if (PacketHandler.IsShield(nLeft.ID))
                                        {
                                            if (!Spells.ContainsKey(10311))//Perseverance
                                            {
                                                Send(new Message("You need to know Perseverance (Pure Warrior skill) to be able to wear 2-handed weapon and shield.", System.Drawing.Color.Red, Message.Talk));
                                                return new Tuple<ConquerItem,
                                                    ConquerItem>(aRight, null);
                                            }
                                            else
                                            {
                                                return new Tuple<ConquerItem,
                                                    ConquerItem>(aRight, nLeft);
                                            }
                                        }
                                        else
                                        {
                                            return new Tuple<ConquerItem,
                                                ConquerItem>(aRight, null);
                                        }
                                    }
                                }
                                else
                                {
                                    if (!PacketHandler.IsTwoHand(nLeft.ID))
                                    {
                                        return new Tuple<ConquerItem,
                                            ConquerItem>(aRight, nLeft);
                                    }
                                    else
                                    {
                                        return new Tuple<ConquerItem,
                                            ConquerItem>(aRight, null);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public int[][] ChampionAllowedStats = new int[][]
        {
            new int[] {1, 0, 0, 0, 0, 0, 0, 30, 0, 0 },
            new int[] {2, 0, 0, 0, 0, 0, 0, 40, 1, 1 },
            new int[] {3, 1, 0, 0, 0, 0, 50, 50, 2, 3 },
            new int[] {4, 3, 1, 1, 0, 0, 100, 60, 5, 4 },
            new int[] {5, 5, 1, 1, 1, 0, 150, 70, 7, 5 },
            new int[] {6, 5, 1, 1, 1, 0, 200, 80, 9, 7 },
            new int[] {12, 7, 2, 2, 1, 1, 255, 100, 12, 9 }
        };
        public bool DoChampStats { get { return ChampionGroup != null; } }
        //private int _accuracy;
        //public int Accuracy
        //{
        //    get { return _accuracy; }
        //}
        public void LoadItemStats()
        {
            uint bStats = Entity.Hitpoints;
            for (int i = 0; i < 29; i++)
                if (Equipment.Objects[i] != null)
                    Equipment.Objects[i].IsWorn = false;
            //if (Team != null)
            //    Team.GetClanShareBp(this);
            //CalculateStatBonus();
            #region Hack Points
            var Asheetos = Entity.Agility + Entity.Strength + Entity.Spirit + Entity.Vitality + Entity.Atributes;
            if (Asheetos > 538)
            {
                Entity.Agility = 0;
                Entity.Strength = 0;
                Entity.Spirit = 0;
                Entity.Vitality = 0;
                Entity.Atributes = 538;
                MTA.Database.EntityTable.SaveEntity(this);
                Console.WriteLine("" + Entity.Name + " Hack Points!");
                Disconnect();
            }
            #endregion
            #region Set Every Variable to Zero        
            Entity.Defence = 0;
            Entity.MagicDefence = 0;
            Entity.MagicDefencePercent = 0;
            Entity.BaseMagicAttack = 0;
            Entity.BaseMagicDefence =
            Entity.BaseMaxAttack =
            Entity.BaseMinAttack =
            Entity.PhysicalDamageDecrease =
            Entity.PhysicalDamageIncrease =
            Entity.MagicDamageDecrease =
            Entity.MagicDamageIncrease = 0;
            Entity.ItemHP = 0;
            Entity.PerfectionLevel = 0;  
            Entity.ItemHP =
            Entity.ItemMP =
            Entity.AttackRange = (byte)0;
            Entity.Dodge = 0;
            Entity.MinAttack = 0;
            Entity.MaxAttack = 0;
            Entity.Defence = 0;
            Entity.SuperItemBless = 0;
            Entity.MagicDefence = 0;
            Entity.Dodge = 0;
            Entity.BaseMagicAttack = 0;
            Entity.WoodResistance = 0;
            Entity.FireResistance = 0;
            Entity.WaterResistance = 0;
            Entity.EarthResistance = 0;
            Entity.MetalResistance = 0;
            Entity.Breaktrough = 0;
            Entity.WearsGoldPrize = false;
            Entity.CriticalStrike = 0;
            Entity.Immunity = 0;
            Entity.Penetration = 0;
            Entity.Counteraction = 0;
            Entity.Block = 0;
            // _accuracy = 0;
            Entity.Detoxication = 0;
            Entity.Intensification = 0;
            Entity.Penetration = 0;
            Entity.SkillCStrike = 0;
            Entity.MaxAttack = 0;
            Entity.MinAttack = 0;
            Entity.PhysicalDamageDecrease = 0;
            Entity.MagicDamageDecrease = 0;
            Entity.MagicDamageIncrease = 0;
            Entity.PhysicalDamageIncrease = 0;
            Entity.MagicDefencePercent = 0;
            Entity.ItemHP = 0;
            Entity.ItemMP = 0;
            Entity.ItemBless = 1.0;
            Entity.AttackRange = 1;
            Entity.BaseMinAttack = 0;
            Entity.BaseMaxAttack = 0;
            Entity.BaseMagicDefence = 0;
            Entity.BaseDefence = 0;
            Entity.MagicDamageIncrease = 0;
            Entity.Gems = new int[GemTypes.Last];
            Entity.Weight = 0;
            Entity.Accuracy = 0;
            #endregion
foreach (ConquerItem i in Equipment.Objects)
            {
                if (i == null) continue;
                if (i.Durability == 0) continue;
                if (!AreStatsLoadable(i)) continue;
                loadItemStats(i);
            }

            Weapons = ComputeWeapons();
            if (Weapons == null) Weapons = new Tuple<ConquerItem, ConquerItem>(null, null);
            if (Weapons.Item1 != null)
            {
                loadItemStats(Weapons.Item1);
                if (Weapons.Item2 != null)
                {
                    if (!Weapons.Item1.IsTwoHander())
                        loadItemStats(Weapons.Item2);
                    else
                        if (PacketHandler.IsFranko(Weapons.Item2.ID) || (Entity.Class >= 20 && Entity.Class <= 25))
                            loadItemStats(Weapons.Item2);
                }
            }

            if (Entity.SubClasses != null)
                Entity.SubClasses.UpgradeStatus(this, false);

            #region Chi
            uint percentage = 100;
            if (DoChampStats)
                percentage = (uint)ChampionAllowedStats[ChampionStats.Grade][7];
            foreach (var chiPower in ChiPowers)
            {
                foreach (var attribute in chiPower.Attributes)
                {
                    switch (attribute.Type)
                    {
                        case Game.Enums.ChiAttribute.CriticalStrike:
                            Entity.CriticalStrike += (int)((ushort)(attribute.Value * 10) * percentage / 100);
                            break;
                        case Game.Enums.ChiAttribute.Counteraction:
                            Entity.Counteraction += (ushort)(attribute.Value * percentage / 100);
                            break;
                        case Game.Enums.ChiAttribute.AddAttack:
                            Entity.BaseMinAttack += attribute.Value * percentage / 100;
                            Entity.BaseMaxAttack += attribute.Value * percentage / 100;
                            break;
                        case Game.Enums.ChiAttribute.AddMagicAttack:
                            Entity.BaseMagicAttack += attribute.Value * percentage / 100;
                            break;
                        case Game.Enums.ChiAttribute.AddMagicDefense:
                            Entity.BaseMagicDefence += attribute.Value * percentage / 100;
                            break;
                        case Game.Enums.ChiAttribute.Breakthrough:
                            Entity.Breaktrough += (ushort)(attribute.Value * percentage / 100);
                            break;
                        case Game.Enums.ChiAttribute.HPAdd:
                            Entity.ItemHP += attribute.Value * percentage / 100;
                            break;
                        case Game.Enums.ChiAttribute.Immunity:
                            Entity.Immunity += (int)((ushort)(attribute.Value * 10) * percentage / 100);
                            break;
                        case Game.Enums.ChiAttribute.MagicDamageDecrease:
                            Entity.MagicDamageDecrease += (ushort)(attribute.Value * percentage / 100);
                            break;
                        case Game.Enums.ChiAttribute.MagicDamageIncrease:
                            Entity.MagicDamageIncrease += (ushort)(attribute.Value * percentage / 100);
                            break;
                        case Game.Enums.ChiAttribute.PhysicalDamageDecrease:
                            Entity.PhysicalDamageDecrease += (ushort)(attribute.Value * percentage / 100);
                            break;
                        case Game.Enums.ChiAttribute.PhysicalDamageIncrease:
                            Entity.PhysicalDamageIncrease += 1;
                            break;
                        case Game.Enums.ChiAttribute.SkillCriticalStrike:
                            Entity.SkillCStrike += (int)((ushort)(attribute.Value * 10) * percentage / 100);
                            break;
                    }
                }
            }
            #region Dragon Ranking
            if (ChiData.DragonRank <= 3000 && ChiPowers.Count > 0)
            {
                Entity.ItemHP += 5000;
                Entity.BaseMagicDefence += 300;
                Entity.PhysicalDamageDecrease += 1000;
                Entity.MagicDamageDecrease += 300;
            }
            #endregion
            #region Phoenix Ranking
            if (ChiData.PhoenixRank <= 3000 && ChiPowers.Count > 1)
            {
                Entity.BaseMinAttack += 3000;
                Entity.BaseMaxAttack += 3000;
                Entity.BaseMagicAttack += 3000;
                Entity.PhysicalDamageIncrease += 1;
                Entity.MagicDamageIncrease += 300;
            }
            #endregion
            #region Tiger Ranking
            if (ChiData.TigerRank <= 3000 && ChiPowers.Count > 2)
            {
                Entity.CriticalStrike += 1500;
                Entity.SkillCStrike += 1500;
                Entity.Immunity += 800;
            }
            #endregion
            #region Turtle Ranking
            if (ChiData.TurtleRank <= 3000 && ChiPowers.Count > 3)
            {
                Entity.Breaktrough += 150;
                Entity.Counteraction += 150;
                Entity.Immunity += 800;
            }
            #endregion
            #endregion
            #region Inner
            if (Entity.InnerPower != null)
            {
                Entity.InnerPower.UpdateStatus();
                Entity.Defence += (ushort)Entity.InnerPower.Defence;
                Entity.CriticalStrike += (int)Entity.InnerPower.CriticalStrike;
                Entity.SkillCStrike += (int)Entity.InnerPower.SkillCriticalStrike;
                Entity.Immunity += (int)Entity.InnerPower.Immunity;
                Entity.Breaktrough += (ushort)Entity.InnerPower.Breakthrough;
                Entity.Counteraction += (ushort)Entity.InnerPower.Counteraction;
                Entity.ItemHP += Entity.InnerPower.MaxLife;
                Entity.BaseMaxAttack += Entity.InnerPower.AddAttack;
                Entity.BaseMinAttack += Entity.InnerPower.AddAttack;
                Entity.BaseMagicAttack += Entity.InnerPower.AddMagicAttack;
                Entity.BaseMagicDefence += Entity.InnerPower.AddMagicDefense;
                Entity.PhysicalDamageIncrease += (ushort)Entity.InnerPower.FinalAttack;
                Entity.PhysicalDamageDecrease += (ushort)Entity.InnerPower.FinalDefense;
                Entity.MagicDamageIncrease += (ushort)Entity.InnerPower.FinalMagicAttack;
                Entity.MagicDamageDecrease += (ushort)Entity.InnerPower.FinalMagicDefense;
            }
            #endregion
            #region Vip 6

            if (this.Entity.VIPLevel == 6)
            {
                Entity expr_1951 = this.Entity;
                expr_1951.BaseMinAttack += 2000;
                Entity expr_1952 = this.Entity;
                expr_1951.BaseMaxAttack += 2000;
                this.Entity.ItemHP += 2000u;
                this.Entity.CriticalStrike += 400;
                this.Entity.Immunity += 400;
                Entity expr_1950 = this.Entity;
                expr_1950.Defence += 2000;
            }
            #endregion
            if (Entity.MyJiang != null)
            {
                Entity.MyJiang.CreateStatusAtributes(Entity);
            }
            if (Entity.Aura_isActive)
                doAuraBonuses(Entity.Aura_actType, Entity.Aura_actPower, 1);
            else
                removeAuraBonuses(Entity.Aura_actType, Entity.Aura_actPower, 1);
            //if (TeamAura)
            //    doAuraBonuses(TeamAuraStatusFlag, TeamAuraPower, 1);
            //else
            //    removeAuraBonuses(TeamAuraStatusFlag, TeamAuraPower, 1);
            foreach (var Aura in Auras.Values)
            {
                doAuraBonuses(Aura.TeamAuraStatusFlag, Aura.TeamAuraPower, 1);
            }
            if (Entity.Class >= 60 && Entity.Class <= 65)
                Entity.AttackRange += 2;
          
            
            /*if (Entity.CriticalStrike > 9000)
                Entity.CriticalStrike = 9000;*/
                      
            CalculateStatBonus();
            CalculateHPBonus();
            ReviewMentor();
            GemAlgorithm();
            Entity.TrojanBP = (uint)Entity.BattlePower;
            Entity.Hitpoints = bStats;
            /*
            if (Team != null)
                Team.GetClanShareBp(this);
            */
            CalculateStatBonus();
            PacketHandler.WindowStats(this);
        }

        public void doAuraBonuses(ulong type, uint power, int i)
        {
            switch (type)
            {
                case (ulong)Update.Flags2.EarthAura: Entity.EarthResistance += (int)power * i; break;
                case (ulong)Update.Flags2.FireAura: Entity.FireResistance += (int)power * i; break;
                case (ulong)Update.Flags2.MetalAura: Entity.MetalResistance += (int)power * i; break;
                case (ulong)Update.Flags2.WoodAura: Entity.WoodResistance += (int)power * i; break;
                case (ulong)Update.Flags2.WaterAura: Entity.WaterResistance += (int)power * i; break;
                case (ulong)Update.Flags2.TyrantAura:
                    {
                        Entity.CriticalStrike += (int)power * i * 100;
                        Entity.SkillCStrike += (int)power * i * 100;
                        if (Entity.CriticalStrike > 120000) Entity.CriticalStrike = 120000;
                        if (Entity.SkillCStrike > 120000) Entity.SkillCStrike = 120000;
                        if (Entity.CriticalStrike < 0) Entity.CriticalStrike = 0;
                        if (Entity.SkillCStrike < 0) Entity.SkillCStrike = 0;
                        break;
                    }
                case (ulong)Update.Flags2.FendAura: Entity.Immunity += (int)power * i * 100; break;
            }
        }
        public void removeAuraBonuses(ulong type, uint power, int i)
        {
            switch (type)
            {
                case (ulong)Update.Flags2.EarthAura: Entity.EarthResistance -= (int)power * i; break;
                case (ulong)Update.Flags2.FireAura: Entity.FireResistance -= (int)power * i; break;
                case (ulong)Update.Flags2.MetalAura: Entity.MetalResistance -= (int)power * i; break;
                case (ulong)Update.Flags2.WoodAura: Entity.WoodResistance -= (int)power * i; break;
                case (ulong)Update.Flags2.WaterAura: Entity.WaterResistance -= (int)power * i; break;
                case (ulong)Update.Flags2.TyrantAura:
                    {
                        Entity.CriticalStrike -= (int)power * i * 100;
                        Entity.SkillCStrike -= (int)power * i * 100;
                        if (Entity.CriticalStrike > 120000) Entity.CriticalStrike = 120000;
                        if (Entity.SkillCStrike > 120000) Entity.SkillCStrike = 120000;
                        if (Entity.CriticalStrike < 0) Entity.CriticalStrike = 0;
                        if (Entity.SkillCStrike < 0) Entity.SkillCStrike = 0;
                        break;
                    }
                case (ulong)Update.Flags2.FendAura: Entity.Immunity -= (int)power * i * 100; break;
            }
        }
        private void CalculateVigor(ConquerItem item, Database.ConquerItemInformation dbi)
        {
            if (!Equipment.Free(12))
            {
                if (!this.Entity.ContainsFlag2(Update.Flags.Ride))
                {
                    this.Vigor = 0;
                    this.MaxVigor = 0;
                    MaxVigor += dbi.PlusInformation.Agility;
                    MaxVigor += 30;
                    if (!Equipment.Free(ConquerItem.SteedCrop))
                    {
                        if (Equipment.Objects[17] != null)
                        {
                            if (Equipment.Objects[17].ID % 10 == 9)
                            {
                                MaxVigor += 1000;
                            }
                            else if (Equipment.Objects[17].ID % 10 == 8)
                            {
                                MaxVigor += 700;
                            }
                            else if (Equipment.Objects[17].ID % 10 == 7)
                            {
                                MaxVigor += 500;
                            }
                            else if (Equipment.Objects[18].ID % 10 == 6)
                            {
                                MaxVigor += 300;
                            }
                            else if (Equipment.Objects[18].ID % 10 == 5)
                            {
                                MaxVigor += 100;
                            }
                        }
                    }
                    Vigor = MaxVigor;
                }
            }
        }  
        private void loadItemStats(ConquerItem item)
        {
            if (item.ID == ConquerItem.GoldPrize) Entity.WearsGoldPrize = true;
            int position = item.Position;
            bool isOver = false;
            if (isOver = (position > 20))
                position -= 20;
            item.IsWorn = true;
            //if (!isOver)
            //{
            // //   if (position == ConquerItem.Garment || position == ConquerItem.Tower || position == ConquerItem.Fan || position == ConquerItem.RightWeaponAccessory || position == ConquerItem.LeftWeaponAccessory)
            //    //    Entity.SuperItemBless += item.Bless;
            //    if (position == ConquerItem.SteedArmor || position == ConquerItem.LeftWeaponAccessory || position == ConquerItem.RightWeaponAccessory) return;
            //}
            int plus = item.Plus;
            if (DoChampStats)
                plus = Math.Min(item.Plus, ChampionAllowedStats[ChampionStats.Grade][0]);
            Database.ConquerItemInformation dbi = new Database.ConquerItemInformation(item.ID, item.Plus);
            if (dbi != null)
            {
                #region Star
                Entity.PerfectionLevel += item.Perfectionlevel;

                if (item.Perfectionlevel > 3 && item.Perfectionlevel < 7)
                {
                    Entity.BaseMinAttack += 100 / 12;
                    Entity.BaseMaxAttack += 100 / 12;
                    Entity.BaseMagicAttack += 300 / 12;
                }
                if (item.Perfectionlevel > 7 && item.Perfectionlevel < 10)
                {
                    Entity.BaseMinAttack += 100 / 12;
                    Entity.BaseMaxAttack += 100 / 12;
                    Entity.BaseDefence += 100 / 12;
                    Entity.BaseMagicAttack += 300 / 12;
                    Entity.MagicDefence += 100 / 12;
                }
                if (item.Perfectionlevel > 10 && item.Perfectionlevel < 14)
                {
                    Entity.BaseMinAttack += 300 / 12;
                    Entity.BaseMaxAttack += 300 / 12;
                    Entity.BaseDefence += 300 / 12;
                    Entity.BaseMagicAttack += 600 / 12;
                    Entity.MagicDefence += 150 / 12;
                }
                if (item.Perfectionlevel > 14 && item.Perfectionlevel < 17)
                {
                    Entity.BaseMinAttack += 500 / 12;
                    Entity.BaseMaxAttack += 500 / 12;
                    Entity.BaseDefence += 500 / 12;
                    Entity.BaseMagicAttack += 1000 / 12;
                    Entity.MagicDefence += 250 / 12;
                }
                if (item.Perfectionlevel > 17 && item.Perfectionlevel < 25)
                {
                    Entity.BaseMinAttack += 800 / 12;
                    Entity.BaseMaxAttack += 800 / 12;
                    Entity.BaseDefence += 1200 / 12;
                    Entity.BaseMagicAttack += 1500 / 12;
                    Entity.MagicDefence += 500 / 12;
                }
                if (item.Perfectionlevel > 25 && item.Perfectionlevel < 28)
                {
                    Entity.BaseMinAttack += 1200 / 12;
                    Entity.BaseMaxAttack += 1200 / 12;
                    Entity.BaseDefence += 1200 / 12;
                    Entity.BaseMagicAttack += 2000 / 12;
                    Entity.MagicDefence += 500 / 12;
                }
                if (item.Perfectionlevel > 28 && item.Perfectionlevel < 32)
                {
                    Entity.BaseMinAttack += 1600 / 12;
                    Entity.BaseMaxAttack += 1600 / 12;
                    Entity.BaseDefence += 1600 / 12;
                    Entity.BaseMagicAttack += 2500 / 12;
                    Entity.MagicDefence += 625 / 12;
                }
                if (item.Perfectionlevel > 32 && item.Perfectionlevel < 55)
                {
                    Entity.BaseMinAttack += 3000 / 12;
                    Entity.BaseMaxAttack += 3000 / 12;
                    Entity.BaseDefence += 3000 / 12;
                    Entity.BaseMagicAttack += 4000 / 12;
                    Entity.MagicDefence += 1000 / 12;
                }
                #endregion 
                #region Give Stats.
                #region Garment
                if (position == ConquerItem.Garment)
                {
                    if (item.ID == 188925)
                    {
                        Entity.CriticalStrike += 100;
                        Entity.SkillCStrike += 100;
                        Entity.Immunity += 100;
                    }
                    else if (item.ID == 187425)
                    {
                        Entity.BaseDefence += 400;
                        Entity.BaseMagicDefence += 2;
                    }
                    else if (item.ID == 187415)
                    {
                        Entity.BaseDefence += 600;
                        Entity.BaseMagicDefence += 3;
                    }
                    else if (item.ID == 187405)
                    {
                        Entity.BaseDefence += 800;
                        Entity.BaseMagicDefence += 4;
                    }
                    else if (item.ID == 188935)
                    {
                        Entity.CriticalStrike += 200;
                        Entity.SkillCStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 188945)
                    {
                        Entity.CriticalStrike += 300;
                        Entity.SkillCStrike += 300;
                        Entity.Immunity += 300;
                    }
                    else if (item.ID == 188955)
                    {
                        Entity.CriticalStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 192745)
                    {
                        Entity.CriticalStrike += 100;
                        Entity.SkillCStrike += 100;
                        Entity.Immunity += 100;
                    }
                    else if (item.ID == 192755)
                    {
                        Entity.CriticalStrike += 200;
                        Entity.SkillCStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 192765)
                    {
                        Entity.CriticalStrike += 300;
                        Entity.SkillCStrike += 300;
                        Entity.Immunity += 300;
                    }
                    else if (item.ID == 192775)
                    {
                        Entity.CriticalStrike += 400;
                        Entity.SkillCStrike += 400;
                        Entity.Immunity += 400;
                    }
                    else if (item.ID == 192805)
                    {
                        Entity.CriticalStrike += 100;
                        Entity.Immunity += 100;
                    }
                    else if (item.ID == 192815)
                    {
                        Entity.CriticalStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 192825)
                    {
                        Entity.CriticalStrike += 300;
                        Entity.Immunity += 300;
                    }
                    else if (item.ID == 192935)
                    {
                        Entity.CriticalStrike += 200;
                        Entity.SkillCStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 192925)
                    {
                        Entity.CriticalStrike += 200;
                        Entity.SkillCStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 192895)
                    {
                        Entity.CriticalStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 188845)
                    {
                        Entity.CriticalStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 188755)
                    {
                        Entity.CriticalStrike += 200;
                        Entity.SkillCStrike += 200;
                    }
                    else if (item.ID == 188515)
                    {
                        Entity.CriticalStrike += 400;
                        Entity.SkillCStrike += 400;
                        Entity.Immunity += 400;
                    }
                    else if (item.ID == 187875)
                    {
                        Entity.CriticalStrike += 100;
                    }
                    else if (item.ID == 187885)
                    {
                        Entity.SkillCStrike += 100;
                    }
                    else if (item.ID == 187865)
                    {
                        Entity.SkillCStrike += 200;
                    }
                    else if (item.ID == 187855)
                    {
                        Entity.CriticalStrike += 200;
                    }
                    else if (item.ID == 187795)
                    {
                        Entity.CriticalStrike += 300;
                        Entity.SkillCStrike += 300;
                        Entity.Immunity += 300;
                    }
                    else if (item.ID == 187785)
                    {
                        Entity.CriticalStrike += 200;
                        Entity.SkillCStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 187775)
                    {
                        Entity.CriticalStrike += 100;
                        Entity.SkillCStrike += 100;
                        Entity.Immunity += 100;
                    }
                }
                #endregion
                #region MountArmor
                if (position == ConquerItem.SteedArmor)
                {
                    if (item.ID == 200221)
                    {
                        Entity.CriticalStrike += 300;
                        Entity.SkillCStrike += 300;
                        Entity.Immunity += 300;
                    }
                    else if (item.ID == 200480)
                    {
                        Entity.CriticalStrike = 200;
                        Entity.SkillCStrike = 200;
                        Entity.Immunity = 200;
                    }
                    else if (item.ID == 200021)
                    {
                        Entity.CriticalStrike = 100;
                        Entity.SkillCStrike = 50;
                        Entity.Immunity = 100;
                    }
                    else if (item.ID == 200022)
                    {
                        Entity.CriticalStrike = 200;
                        Entity.SkillCStrike = 100;
                        Entity.Immunity = 200;
                    }
                    else if (item.ID == 200220)
                    {
                        Entity.CriticalStrike += 200;
                        Entity.SkillCStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 200486)
                    {
                        Entity.CriticalStrike += 100;
                        Entity.Immunity += 100;
                    }
                    else if (item.ID == 200485)
                    {
                        Entity.CriticalStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 200479)
                    {
                        Entity.CriticalStrike += 100;
                        Entity.SkillCStrike += 100;
                    }
                    else if (item.ID == 200478)
                    {
                        Entity.CriticalStrike += 100;
                        Entity.SkillCStrike += 100;
                        Entity.Immunity += 100;
                    }
                    else if (item.ID == 200477)
                    {
                        Entity.CriticalStrike += 200;
                        Entity.SkillCStrike += 200;
                    }
                    else if (item.ID == 200475)
                    {
                        Entity.CriticalStrike += 200;
                        Entity.SkillCStrike += 200;
                        Entity.Immunity += 200;
                    }
                }
                #endregion
                #region Cups State
                if (position == ConquerItem.Bottle)
                {
                    if (item.ID == 2100075)
                    {
                        Entity.Breaktrough += 30;
                        Entity.Counteraction += 30;
                        Entity.CriticalStrike += 300;
                        Entity.Immunity += 300;
                    }
                }
                #endregion
                #region soul stats

                if (DoChampStats && ChampionAllowedStats[ChampionStats.Grade][5] == 1 || !DoChampStats)
                {
                    if (item.Purification.PurificationItemID != 0)
                    {
                        Database.ConquerItemInformation soulDB = new Database.ConquerItemInformation(item.Purification.PurificationItemID, 0);
                        if (position == ConquerItem.LeftWeapon)
                        {
                            Entity.BaseMinAttack += (uint)(soulDB.BaseInformation.MinAttack / 2);
                            Entity.BaseMaxAttack += (uint)(soulDB.BaseInformation.MaxAttack / 2);
                        }
                        else
                        {
                            Entity.BaseMinAttack += soulDB.BaseInformation.MinAttack;
                            Entity.BaseMaxAttack += soulDB.BaseInformation.MaxAttack;
                        }
                        //  Entity.BaseMinAttack += soulDB.BaseInformation.MinAttack;
                        //  Entity.BaseMaxAttack += soulDB.BaseInformation.MaxAttack;
                        Entity.ItemHP += soulDB.BaseInformation.ItemHP;
                        Entity.BaseDefence += soulDB.BaseInformation.PhysicalDefence;
                        Entity.MagicDefence += soulDB.BaseInformation.MagicDefence;
                        Entity.Dodge += soulDB.BaseInformation.Dodge;
                        Entity.Accuracy += soulDB.BaseInformation.Accuracy;
                        Entity.BaseMagicAttack += soulDB.BaseInformation.MagicAttack;
                        Entity.MetalResistance += soulDB.BaseInformation.MetalResist;
                        Entity.WoodResistance += soulDB.BaseInformation.WoodResist;
                        Entity.FireResistance += soulDB.BaseInformation.FireResist;
                        Entity.WaterResistance += soulDB.BaseInformation.WaterResist;
                        Entity.EarthResistance += soulDB.BaseInformation.EarthResist;
                        Entity.Breaktrough += soulDB.BaseInformation.BreakThrough;
                        Entity.CriticalStrike += soulDB.BaseInformation.CriticalStrike;
                        Entity.Immunity += soulDB.BaseInformation.Immunity;
                        Entity.Penetration += soulDB.BaseInformation.Penetration;
                        Entity.Counteraction += soulDB.BaseInformation.CounterAction;
                        Entity.Block += soulDB.BaseInformation.Block;

                        Entity.Weight += soulDB.BaseInformation.Weight;
                    }
                } 
                #endregion
                #region Refinery stats
                if (DoChampStats && ChampionAllowedStats[ChampionStats.Grade][4] == 1 || !DoChampStats)
                {
                    Refinery.RefineryItem refine = null;
                    if (item.ExtraEffect.Available)
                    {
                        if (Kernel.DatabaseRefinery.TryGetValue(item.ExtraEffect.EffectID, out refine))
                        {
                            if (refine != null)
                            {
                                switch (refine.Type)
                                {
                                    case Refinery.RefineryItem.RefineryType.Block:
                                        Entity.Block += (UInt16)(refine.Percent * 100);
                                        break;
                                    case Refinery.RefineryItem.RefineryType.BreakThrough:
                                        Entity.Breaktrough += (UInt16)((refine.Percent * 10));
                                        break;
                                    case Refinery.RefineryItem.RefineryType.Counteraction:
                                        Entity.Counteraction += (UInt16)(refine.Percent * 10);
                                        break;
                                    case Refinery.RefineryItem.RefineryType.Critical:
                                        Entity.CriticalStrike += (UInt16)((refine.Percent * 100));
                                        break;
                                    case Refinery.RefineryItem.RefineryType.Detoxication:
                                        Entity.Detoxication += (UInt16)(refine.Percent);
                                        break;
                                    case Refinery.RefineryItem.RefineryType.Immunity:
                                        Entity.Immunity += (UInt16)(refine.Percent * 100);
                                        break;
                                    case Refinery.RefineryItem.RefineryType.Intensification:
                                        Entity.Intensification += (UInt16)(refine.Percent);
                                        break;
                                    case Refinery.RefineryItem.RefineryType.Penetration:
                                        Entity.Penetration += (UInt16)(refine.Percent * 100);
                                        break;
                                    case Refinery.RefineryItem.RefineryType.SCritical:
                                        Entity.SkillCStrike += (UInt16)(refine.Percent * 100);
                                        break;
                                }
                            }
                        }
                    }
                }
                #endregion
                
                if (position == ConquerItem.Tower)
                {
                    Entity.PhysicalDamageDecrease += dbi.BaseInformation.PhysicalDefence;
                    Entity.MagicDamageDecrease += dbi.BaseInformation.MagicDefence;
                }
                else if (position == ConquerItem.Fan)
                {
                    Entity.PhysicalDamageIncrease += dbi.BaseInformation.MaxAttack;
                    Entity.MagicDamageIncrease += dbi.BaseInformation.MagicAttack;
                } 
                else
                {
                    if (position == ConquerItem.LeftWeapon)
                    {
                        Entity.BaseMinAttack += (uint)dbi.BaseInformation.MinAttack / 2;
                        Entity.BaseMaxAttack += (uint)dbi.BaseInformation.MaxAttack / 2;                       

                    }
                    else
                    {
                        if (position == ConquerItem.RightWeapon)
                        {
                            Entity.AttackRange += dbi.BaseInformation.AttackRange;
                            if (Network.PacketHandler.IsTwoHand(dbi.BaseInformation.ID))
                                Entity.AttackRange += 4;
                            else
                                Entity.AttackRange += 3;
                        }
                        Entity.BaseMinAttack += dbi.BaseInformation.MinAttack;
                        Entity.BaseMaxAttack += dbi.BaseInformation.MaxAttack;
                    }

                    Entity.BaseDefence += dbi.BaseInformation.PhysicalDefence;
                    Entity.BaseMagicAttack += dbi.BaseInformation.MagicAttack;
                }

                if (position == ConquerItem.Steed)
                {
                    CalculateVigor(item, dbi);
                }  
                Entity.ItemHP += dbi.BaseInformation.ItemHP;
                Entity.ItemMP += dbi.BaseInformation.ItemMP;
                Entity.Dodge += dbi.BaseInformation.Dodge;
                Entity.Dexterity += dbi.BaseInformation.Frequency;
                Entity.Weight += dbi.BaseInformation.Weight;
                if (item.Position != ConquerItem.Steed)
                {
                    if (DoChampStats)
                        Entity.ItemBless -= (ushort)Math.Min(item.Bless / 100, ChampionAllowedStats[ChampionStats.Grade][1]);
                    else
                        Entity.ItemBless -= ((double)item.Bless / 100);
                }
               

                var gem = (int)item.SocketOne;
                if (gem != 0 && gem != 255)
                    Entity.Gems[gem / 10] += GemTypes.Effects[gem / 10][gem % 10];

                gem = (int)item.SocketTwo;
                if (gem != 0 && gem != 255)
                    Entity.Gems[gem / 10] += GemTypes.Effects[gem / 10][gem % 10];

                if (item.Plus > 0)
                {
                    var add = dbi.PlusInformation;
                    Entity.BaseMinAttack += add.MinAttack;
                    Entity.BaseMaxAttack += add.MaxAttack;
                    Entity.BaseMagicAttack += add.MagicAttack;
                    Entity.BaseDefence += add.PhysicalDefence;
                    Entity.Dodge += add.Dodge;
                    Entity.Dexterity += add.Agility;
                    Entity.MagicDefence += add.MagicDefence;
                    Entity.ItemHP += add.ItemHP;
                }
                Entity.ItemHP += item.Enchant;
                var per = 1;
                var per2 = 1;
              //  if (item.Position == ConquerItem.Garment || item.Position == ConquerItem.Bottle || item.Position == ConquerItem.SteedArmor)
              //      per = per2 = 1;                
                Entity.CriticalStrike += (int)dbi.BaseInformation.CriticalStrike / per;
                Entity.SkillCStrike += (int)dbi.BaseInformation.SkillCriticalStrike / per;
                Entity.Immunity += (int)dbi.BaseInformation.Immunity / per;
                Entity.Penetration += (int)dbi.BaseInformation.Penetration / per;
                Entity.Block += (int)dbi.BaseInformation.Block / per;
                Entity.Breaktrough += (int)dbi.BaseInformation.BreakThrough / per2;
                Entity.Counteraction += (int)dbi.BaseInformation.CounterAction / per2;
                Entity.MetalResistance += dbi.BaseInformation.MetalResist;
                Entity.WoodResistance += dbi.BaseInformation.WoodResist;
                Entity.WaterResistance += dbi.BaseInformation.WaterResist;
                Entity.FireResistance += dbi.BaseInformation.FireResist;
                Entity.EarthResistance += dbi.BaseInformation.EarthResist;

                #endregion
            }
        }        
        public void GemAlgorithm()
        {

            Entity.MaxAttack = Entity.BaseMaxAttack + Entity.Strength;
            Entity.MinAttack = Entity.BaseMinAttack + Entity.Strength;           
            Entity.MagicAttack = Entity.BaseMagicAttack;

        }

        #endregion
        #endregion

        #region Matrix
               
        public byte Circle_Level;
        public string circle_Effect;
        public Point Circle_Center;
        public void Summon2()
        {
            try
            {
                List<System.Drawing.Point> DestructionAreas = new List<System.Drawing.Point>();               
                for (int i = 0; i < 360; i++)
                {
                    ushort x = (ushort)(Circle_Center.X + (Circle_Level * Math.Cos(i)));
                    ushort y = (ushort)(Circle_Center.Y + (Circle_Level * Math.Sin(i)));
                    System.Drawing.Point p = new System.Drawing.Point((int)x, (int)y);
                    if (!DestructionAreas.Contains(p))
                        DestructionAreas.Add(p);
                }
                foreach (System.Drawing.Point p in DestructionAreas)
                {
                    _String str = new _String(true);
                    str.TextsCount = 1;
                    str.PositionX = (ushort)p.X;
                    str.PositionY = (ushort)p.Y;
                    str.Type = _String.MapEffect;
                    str.Texts.Add(circle_Effect);
                    SendScreen(str, true);

                    
                    var spell = Database.SpellTable.GetSpell(11600, this);

                    var attack = new Attack(true);
                    attack.Attacker = Entity.UID;
                    attack.AttackType = Attack.Melee;

                    foreach (var obj1 in Screen.Objects)
                    {
                        if (Kernel.GetDistance(obj1.X, obj1.Y, (ushort)p.X, (ushort)p.Y) <= 3)
                        {
                            if (obj1.MapObjType == MapObjectType.Monster || obj1.MapObjType == MapObjectType.Player)
                            {
                                var attacked = obj1 as Entity;
                                if (MTA.Game.Attacking.Handle.CanAttack(Entity, attacked, spell, false))
                                {
                                    uint damage = Game.Attacking.Calculate.Melee(Entity, attacked, spell, ref attack);

                                    attack.Damage = damage;
                                    attack.Attacked = attacked.UID;
                                    attack.X = attacked.X;
                                    attack.Y = attacked.Y;

                                    MTA.Game.Attacking.Handle.ReceiveAttack(Entity, attacked, attack, ref damage, spell);
                                }
                            }
                            else if (obj1.MapObjType == MapObjectType.SobNpc)
                            {
                                var attacked = obj1 as SobNpcSpawn;
                                if (MTA.Game.Attacking.Handle.CanAttack(Entity, attacked, spell))
                                {
                                    uint damage = Game.Attacking.Calculate.Melee(Entity, attacked, ref attack);
                                    attack.Damage = damage;
                                    attack.Attacked = attacked.UID;
                                    attack.X = attacked.X;
                                    attack.Y = attacked.Y;

                                   MTA.Game.Attacking.Handle.ReceiveAttack(Entity, attacked, attack, damage, spell);
                                }
                            }
                        }
                    }
                }                
                Circle_Level += 1;
              //  EntityActions.RemoveAction(ProjectX_V3_Game.Entities.DelayedActionType.Summon);
              //  EntityActions.AddAction(ProjectX_V3_Game.Entities.DelayedActionType.Summon, Summon2, 1500);                                 
                               
            }
            catch { }
        }        
        public static GameState CharacterFromName(string name)
        {
            foreach (GameState c in Kernel.GamePool.Values)
                if (c.Entity.Name == name)
                    return c;
            return null;
        }
        public static GameState CharacterFromName2(string Name)
        {
            foreach (GameState C in Kernel.GamePool.Values)
                if (C.Entity.Name == Name)
                    return C;
            return null;
        }
        #region New acc Reg.
        public string accountname;
        public string accountpass1;
        public string accountpass2;
        public string accountEmail;
        #endregion
           
       
        public bool ItemGive = false;       
        public bool IsFairy = false;
        public uint FairyType = 0;
        public uint SType = 0;

        #endregion


        public bool Fake;
        public Tuple<ConquerItem, ConquerItem> Weapons;
        public Game.Enums.PkMode PrevPK;
        public InnerPower InnerPower;
        public int TeamCheerFor;
        public int ArenaState = 0;
        public QuizShow.QuizClient Quiz;
        public uint InteractionEffect;
        public Game.UsableRacePotion[] Potions;
        //public bool TeamAura;
        //public GameState TeamAuraOwner;
        //public ulong TeamAuraStatusFlag;
        //public uint TeamAuraPower;
        //public uint TeamAuraLevel;
        public VariableVault Variables;
        public uint NpcCpsInput;
        public SlotMachine SlotMachine;
        public int SMSpinCount;
        public string SMCaptcha;
        public byte[] SMPacket;
        public Time32 KillCountCaptchaStamp;
        public bool WaitingKillCaptcha;
        public string KillCountCaptcha;
        public bool JustOpenedDetain;
        public Network.GamePackets.Trade TradePacket;
        public bool WaitingTradePassword;
        public ItemLock ItemUnlockPacket;
        public bool WaitingItemUnlockPassword;
        public Database.ConquerItemBaseInformation NewLookArmorInfo;
        public Database.ConquerItemBaseInformation NewLookHeadgearInfo;
        public Database.ConquerItemBaseInformation NewLookWeapon;
        public Database.ConquerItemBaseInformation NewLookWeapon2;  

        public Time32 LastAttack, LastMove;

        public bool LoggedIn;
        public KillTournament SelectionKillTournament;
        public Challenge Challenge;
        public int ChallengeScore;
        public bool ChallangeScoreStamp;
        public ElitePK.FighterStats ElitePKStats;
        public ElitePK.Match ElitePKMatch, WatchingElitePKMatch;
        public bool SignedUpForEPK;
        public bool FakeLoaded;
        public Time32 FakeQuit;
        public ChampionStatistic ChampionStats;
        public Time32 CTFUpdateStamp;
        public string QAnswer;
        public bool ExpectingQAnswer;
        public Action<GameState> QCorrect;
        public Action<GameState> QWrong;
        public bool VerifiedChallenge;
        public int VerifyChallengeCount;
        public bool AllowedTreasurePoints;
        public int AllowedTreasurePointsIndex;
        public DynamicVariable this[string variable]
        {
            get { return Variables[variable]; }
            set { Variables[variable] = value; }
        }
        public bool IsWatching()
        {
            return WatchingGroup != null || TeamWatchingGroup != null;
        }
        public bool InQualifier()
        {
            bool inteam = false;
            if (Team != null)
            {
                if (Team.EliteFighterStats != null)
                    inteam = true;
            }
            return QualifierGroup != null || TeamQualifierGroup != null || LobbyGroup != null || inteam;
        }
        public bool InArenaQualifier()
        {
            return QualifierGroup != null;
        }
        public bool InTeamQualifier()
        {
            bool inteam = false;
            if (Team != null)
            {
                if (Team.EliteMatch != null)
                    if (Team.EliteMatch.Map != null)
                        if (Team.EliteMatch.Map.ID == Entity.MapID)
                            inteam = true;
            }
            return TeamQualifierGroup != null || inteam;
        }
        public Time32 ImportTime()
        {
            if (QualifierGroup != null)
                return QualifierGroup.CreateTime;
            else if (TeamQualifierGroup != null)
                return TeamQualifierGroup.ImportTime;
            else if (LobbyGroup != null)
                return LobbyGroup.ImportTime;
            if (Team != null)
            {
                if (Team.EliteMatch != null)
                    return Team.EliteMatch.ImportTime;
            }
            return Time32.Now;
        }
       // public void UpdateQualifier( long damage, bool toxicfog = false)
        public void UpdateQualifier(GameState client, GameState target, long damage, bool toxicfog = false)
        {
            if (LobbyGroup != null)
            {
                LobbyGroup.UpdateDamage(LobbyGroup.OppositeClient(this), (uint)damage);
            }
            else if (ChampionGroup != null)
            {
                ChampionGroup.UpdateDamage(ChampionGroup.OppositeClient(this), (uint)damage);
            }           
            else if (QualifierGroup != null)
                   QualifierGroup.UpdateDamage(client, (uint)damage);
                else if (TeamQualifierGroup != null)
                {
                    if (client == null)
                        TeamQualifierGroup.UpdateDamage(target, (uint)damage, true);
                    else
                        TeamQualifierGroup.UpdateDamage(client, (uint)damage);
                } 
            else if (toxicfog)
            {
                if (ElitePKMatch != null)
                {
                    var opponent = ElitePKMatch.targetOf(this);
                    if (opponent != null)
                        opponent.ElitePKStats.Points += (uint)damage;
                    ElitePKMatch.Update();
                }
               else if (Team != null)
               {
                   if (Team.EliteMatch != null)
                   {
                       var opponent = Team.EliteMatch.targetOfWin(this.Team);
                       if (opponent != null)
                       {
                           opponent.Points += (uint)damage;
                           opponent.Team.SendMesageTeam(opponent.Team.EliteMatch.CreateUpdate().ToArray(), 0);
                       } 
                       Team.SendMesageTeam(Team.EliteMatch.CreateUpdate().ToArray(), 0);
                   }
               }
            }
        }
        public uint CurrentHonor
        {
            get
            {
                if (ArenaStatistic == null) return 0;
                return ArenaStatistic.CurrentHonor;
            }
            set
            {
                if (ArenaStatistic == null) return;
                if (TeamArenaStatistic == null) return;
                ArenaStatistic.CurrentHonor =
                    TeamArenaStatistic.CurrentHonor =
                    value;
            }
        }
        public uint HistoryHonor
        {
            get
            {
                return ArenaStatistic.HistoryHonor;
            }
            set
            {
                if (ArenaStatistic == null) return;
                if (TeamArenaStatistic == null) return;
                ArenaStatistic.HistoryHonor =
                    TeamArenaStatistic.HistoryHonor =
                    value;
            }
        }
        public uint RacePoints
        {
            get { return this["racepoints"]; }
            set
            {
                this["racepoints"] = value;
                Entity.Update(Update.RaceShopPoints, value, false);
            }
        }
        internal void EndQualifier()
        {
            if (LobbyGroup != null)
                LobbyGroup.End(this);

            if (ChampionGroup != null)
                ChampionGroup.End(this);

            if (QualifierGroup != null)
                QualifierGroup.End(this);

            if (TeamQualifierGroup != null)
                TeamQualifierGroup.CheckEnd(this);
            if (Entity.CLanArenaBattle != null)
                Entity.CLanArenaBattle.CheakToEnd(this);
            if (Entity.GuildArenaBattle != null)
                Entity.GuildArenaBattle.CheakToEnd(this);
        }

        internal void Send(string msg, uint type = Message.Talk)
        {
            Send(new Message(msg, type));
        }

        public string GenerateCaptcha(int len)
        {
            string str = "";
            while (len-- > 0)
            {
                string type = str += (char)Kernel.Random.Next('0', '9');
                /*int type = Kernel.Random.Next(0, 3);
                if (type == 0) str += (char)Kernel.Random.Next('0', '9');
                else if (type == 1) str += (char)Kernel.Random.Next('a', 'z');
                else str += (char)Kernel.Random.Next('A', 'Z');*/
            }
            return str;
        }

        public void MessageBox(string text, Action<GameState> msg_ok = null, Action<GameState> msg_cancel = null, uint time = 0, Game.Languages language = Game.Languages.English, bool egbary = false)
        {
            if (!egbary)
            {
                if (Entity.MapID == 6000 || Entity.MapID == 6001 || Entity.MapID == 6002 ||
                    Entity.MapID == 6003 || Entity.MapID == 6004 || Entity.MapID == 1038 ||
                    Entity.PokerTableUID > 0 || Entity.InJail() ||
                     PlayRouletteUID > 0) return;
            }
            if (InQualifier() || (Challenge != null && Challenge.Inside))
                return;
            if (language != Language)
                return;
            MessageOK = msg_ok;
            MessageCancel = msg_cancel;
            NpcReply msg = new NpcReply(NpcReply.MessageBox, text);
            Send(msg);
            if (time != 0)
                Time(time);
        }

        public void Time(uint time)
        {
            Send(new Data(true) { UID = Entity.UID, dwParam = time, ID = Data.CountDown });
        }

        public bool Online
        {
            get
            {
                return Socket.Connector != null;
            }
        }

        internal void LoadData(bool loadFake = false)
        {
        //    Database.KissSystemTable.Kisses(this);
            Database.PkExpelTable.Load(this);
            Database.ConquerItemTable.LoadItems(this);
        //    Database.FlowerSystemTable.Flowers(this);
           
            if (!loadFake)
            {
                Database.ClaimItemTable.LoadClaimableItems(this);
                Database.DetainedItemTable.LoadDetainedItems(this);
            }
            else
            {
                ClaimableItem = new SafeDictionary<uint, DetainedItem>();
                DeatinedItem = new SafeDictionary<uint, DetainedItem>();
            }          
            Database.SubClassTable.Load(this.Entity);
            if (!loadFake)
            {
                using (var conn = Database.DataHolder.MySqlConnection)
                {
                    conn.Open();
                    Database.SkillTable.LoadProficiencies(this, conn);
                    Database.SkillTable.LoadSpells(this, conn);
                }
                Database.KnownPersons.LoadPartner(this);
                Database.KnownPersons.LoadEnemy(this);
                Database.KnownPersons.LoaderFriends(this);
                Database.KnownPersons.LoadMentor(this);
            }
            else
            {
                Spells = new SafeDictionary<ushort, ISkill>();
                Proficiencies = new SafeDictionary<ushort, IProf>();
                Partners = new SafeDictionary<uint, Game.ConquerStructures.Society.TradePartner>();
                Enemy = new SafeDictionary<uint, Game.ConquerStructures.Society.Enemy>();
                Friends = new SafeDictionary<uint, Game.ConquerStructures.Society.Friend>();
                Apprentices = new SafeDictionary<uint, Game.ConquerStructures.Society.Apprentice>();
            }
            Database.ChiTable.Load(this);
            MaTrix.Inbox.Load(this);      
 
            Quests.Load();
     
          //  Database.BigBOSRewardDataBase.LoadReward(this);
        }

        public void FakeLoad(uint UID, bool enterserver = true)
        {
            if (!Kernel.GamePool.ContainsKey(UID))
            {
                ReadyToPlay();                
                this.Account = new Database.AccountTable(null);
                this.Account.EntityID = UID;
                if (Database.EntityTable.LoadEntity(this))
                {
                    if (this.Entity.FullyLoaded)
                    {
                        VariableVault variables;
                        Database.EntityVariableTable.Load(this.Entity.UID, out variables);
                        this.Variables = variables;
                                               
                        if (this.BackupArmorLook != 0)
                            this.SetNewArmorLook(this.BackupArmorLook);
                        else
                            this.SetNewArmorLook(this.ArmorLook);
                        this.SetNewHeadgearLook(this.HeadgearLook);
                        this.BackupArmorLook = 0;

                        this.LoadData(enterserver);

                        if (this.Entity.GuildID != 0)
                            this.Entity.GuildBattlePower = this.Guild.GetSharedBattlepower(this.Entity.GuildRank);

                        this.ReviewMentor();

                        Entity.NobilityRank = NobilityInformation.Rank;

                        if (enterserver)
                        {
                            Network.PacketHandler.LoginMessages(this);

                            Program.World.Register(this);
                            Kernel.GamePool.Add(Entity.UID, this);
                        }
                        FakeLoaded = true;
                       
                    }
                }
            }
        }
        public void FakeLoad2(uint UID, string Name = "")
        {
            if (Name == "")
                Name = "MaTrix[" + UID + "]";
            if (!Kernel.GamePool.ContainsKey(UID))
            {
                this.ReadyToPlay();
                this.Account = new Database.AccountTable(null);
                this.Account.EntityID = UID;
                this.Entity = new Entity(EntityFlag.Player, false);
                this.Entity.Owner = this;
                this.Entity.Name = Name;
                this.Entity.UID = UID;
                this.Entity.Vitality = 537;               
                this.Entity.Face = 37;
                this.Entity.Body = 1003;
                this.Entity.HairStyle = 630;
                this.Entity.Level = 140;
                this.Entity.Class = 15;
                this.Entity.Reborn = 2;
                this.Entity.MaxHitpoints = 20000;
                this.Entity.Hitpoints = this.Entity.MaxHitpoints;
                this.Entity.Mana = 800;
                
                this.Variables = new VariableVault();
                this.Friends = new SafeDictionary<uint, Game.ConquerStructures.Society.Friend>();
                this.Enemy = new SafeDictionary<uint, Game.ConquerStructures.Society.Enemy>();
                this.ChiData = new ChiTable.ChiData();
                this.ChiPowers = new List<ChiPowerStructure>();
              

                this.NobilityInformation = new MTA.Game.ConquerStructures.NobilityInformation();
                this.NobilityInformation.EntityUID = this.Entity.UID;
                this.NobilityInformation.Name = this.Entity.Name;
                this.NobilityInformation.Donation = 0;
                this.NobilityInformation.Rank = Game.ConquerStructures.NobilityRank.Serf;
                this.NobilityInformation.Position = -1;
                this.NobilityInformation.Gender = 1;
                this.NobilityInformation.Mesh = this.Entity.Mesh;
                if (this.Entity.Body % 10 >= 3)
                    this.NobilityInformation.Gender = 0;

                this.TeamArenaStatistic = new MTA.Network.GamePackets.TeamArenaStatistic(true);
                this.TeamArenaStatistic.EntityID = this.Entity.UID;
                this.TeamArenaStatistic.Name = this.Entity.Name;
                this.TeamArenaStatistic.Level = this.Entity.Level;
                this.TeamArenaStatistic.Class = this.Entity.Class;
                this.TeamArenaStatistic.Model = this.Entity.Mesh;
                this.TeamArenaStatistic.Status = Network.GamePackets.TeamArenaStatistic.NotSignedUp;

                this.ArenaStatistic = new MTA.Network.GamePackets.ArenaStatistic(true);
                this.ArenaStatistic.EntityID = this.Entity.UID;
                this.ArenaStatistic.Name = this.Entity.Name;
                this.ArenaStatistic.Level = this.Entity.Level;
                this.ArenaStatistic.Class = this.Entity.Class;
                this.ArenaStatistic.Model = this.Entity.Mesh;
                this.ArenaPoints = ArenaTable.ArenaPointFill(this.Entity.Level);
                this.ArenaStatistic.LastArenaPointFill = DateTime.Now;
                this.ArenaStatistic.Status = Network.GamePackets.ArenaStatistic.NotSignedUp;

                this.ChampionStats = new MTA.Network.GamePackets.ChampionStatistic(true);
                this.ChampionStats.UID = this.Entity.UID;
                this.ChampionStats.Name = this.Entity.Name;
                this.ChampionStats.Level = this.Entity.Level;
                this.ChampionStats.Class = this.Entity.Class;
                this.ChampionStats.Model = this.Entity.Mesh;
                this.ChampionStats.Points = 0;
                this.ChampionStats.LastReset = DateTime.Now;
                this.ChiPowers = new List<ChiPowerStructure>();
                this.Retretead_ChiPowers = new ChiPowerStructure[4];
                this.ChiData = new ChiTable.ChiData() { Name = this.Entity.Name, UID = this.Entity.UID, Powers = this.ChiPowers };
               
                this.Entity.Stamina = 150;

                this.Spells = new SafeDictionary<ushort, Interfaces.ISkill>();
                this.Proficiencies = new SafeDictionary<ushort, Interfaces.IProf>();

                Network.PacketHandler.LoginMessages(this);

                Program.World.Register(this);
                Kernel.GamePool.Add(Entity.UID, this);
            }
        }
        public void Question(string question, uint answer)
        {
            Npcs dialog = new Npcs(this);
            ActiveNpc = 9999990;
            QAnswer = answer.ToString();
            ExpectingQAnswer = true;
            dialog.Text(question);
            dialog.Input("Answer:", 1, (byte)QAnswer.Length);
            dialog.Option("No thank you.", 255);
            dialog.Send();
        }

        public void FakeLoadx(uint UID)
        {
            if (!Kernel.GamePool.ContainsKey(UID))
            {
                ReadyToPlay();
                this.Account = new Database.AccountTable(null);
                this.Account.EntityID = UID;
                //   if (Database.EntityTable.LoadEntity(this))
                {
                    #region Load Entity
                    MTA.Database.MySqlCommand command = new MTA.Database.MySqlCommand(MySqlCommandType.SELECT);
                    command.Select("bots").Where("BotID", (long)UID);
                    MySqlReader reader = new MySqlReader(command);
                    if (!reader.Read())
                    {
                        return;
                    }
                    this.Entity = new MTA.Game.Entity(EntityFlag.Player, false);
                    this.Entity.Name = reader.ReadString("BotName");
                    this.Entity.Owner = this;
                    this.Entity.UID = UID;
                    this.Entity.Body = reader.ReadUInt16("BotBody");
                    this.Entity.Face = reader.ReadUInt16("BotFace");
                    this.Entity.HairStyle = reader.ReadUInt16("BotHairStyle");
                    this.Entity.Level = reader.ReadByte("BotLevel");
                    this.Entity.Class = reader.ReadByte("BotClass");
                    this.Entity.Reborn = reader.ReadByte("BotReborns");
                    this.Entity.Titles = new System.Collections.Concurrent.ConcurrentDictionary<TitlePacket.Titles, DateTime>();
                    this.Entity.MyTitle = (TitlePacket.Titles)reader.ReadUInt32("BotTitle");
                    this.Entity.MapID = reader.ReadUInt16("BotMap");
                    if (this.VendingDisguise == 0)
                        this.VendingDisguise = 0xdf;
                    this.Entity.X = reader.ReadUInt16("BotMapx");
                    this.Entity.Y = reader.ReadUInt16("BotMapy");
                    uint WeaponR = reader.ReadUInt32("BotWeaponR");
                    uint WeaponL = reader.ReadUInt32("BotWeaponL");
                    uint Armor = reader.ReadUInt32("BotArmor");
                    uint Head = reader.ReadUInt32("BotHead");
                    uint Garment = reader.ReadUInt32("BotGarment");
                    
                    string hawkmessage = reader.ReadString("BotMessage");
                    Entity.MyAchievement = new Game.Achievement(Entity);

                    int count = reader.ReadInt32("BItemCount");
                    string[] itemCost = reader.ReadString("BItemCost").Split(new string[] { "~", "@@", " " }, StringSplitOptions.RemoveEmptyEntries);
                    string[] itemID = reader.ReadString("BItemID").Split(new string[] { "~", "@@", " " }, StringSplitOptions.RemoveEmptyEntries);
                    string[] itemPlus = reader.ReadString("BItemPlus").Split(new string[] { "~", "@@", " " }, StringSplitOptions.RemoveEmptyEntries);
                    string[] itemEnchant = reader.ReadString("BItemEnchant").Split(new string[] { "~", "@@", " " }, StringSplitOptions.RemoveEmptyEntries);
                    string[] itemBless = reader.ReadString("BItemBless").Split(new string[] { "~", "@@", " " }, StringSplitOptions.RemoveEmptyEntries);
                    string[] itemSocketOne = reader.ReadString("BItemSoc1").Split(new string[] { "~", "@@", " " }, StringSplitOptions.RemoveEmptyEntries);
                    string[] itemSocketTwo = reader.ReadString("BItemSoc2").Split(new string[] { "~", "@@", " " }, StringSplitOptions.RemoveEmptyEntries);

                    this.ElitePKStats = new ElitePK.FighterStats(this.Entity.UID, this.Entity.Name, this.Entity.Mesh);
                    if (!MTA.Game.ConquerStructures.Nobility.Board.TryGetValue(this.Entity.UID, out this.NobilityInformation))
                    {
                        this.NobilityInformation = new NobilityInformation();
                        this.NobilityInformation.EntityUID = this.Entity.UID;
                        this.NobilityInformation.Name = this.Entity.Name;
                        this.NobilityInformation.Donation = 0L;
                        this.NobilityInformation.Rank = NobilityRank.Serf;
                        this.NobilityInformation.Position = -1;
                        this.NobilityInformation.Gender = 1;
                        this.NobilityInformation.Mesh = this.Entity.Mesh;
                        if ((this.Entity.Body % 10) >= 3)
                        {
                            this.NobilityInformation.Gender = 0;
                        }
                    }
                    else
                    {
                        this.Entity.NobilityRank = this.NobilityInformation.Rank;
                    }
                    Arena.ArenaStatistics.TryGetValue(this.Entity.UID, out this.ArenaStatistic);
                    if ((this.ArenaStatistic == null) || (this.ArenaStatistic.EntityID == 0))
                    {
                        this.ArenaStatistic = new ArenaStatistic(true);
                        this.ArenaStatistic.EntityID = this.Entity.UID;
                        this.ArenaStatistic.Name = this.Entity.Name;
                        this.ArenaStatistic.Level = this.Entity.Level;
                        this.ArenaStatistic.Class = this.Entity.Class;
                        this.ArenaStatistic.Model = this.Entity.Mesh;
                        this.ArenaStatistic.ArenaPoints = ArenaTable.ArenaPointFill(this.Entity.Level);
                        this.ArenaStatistic.LastArenaPointFill = DateTime.Now;
                        ArenaTable.InsertArenaStatistic(this);
                        this.ArenaStatistic.Status = 0;
                        Arena.ArenaStatistics.Add(this.Entity.UID, this.ArenaStatistic);
                    }
                    else
                    {
                        this.ArenaStatistic.Level = this.Entity.Level;
                        this.ArenaStatistic.Class = this.Entity.Class;
                        this.ArenaStatistic.Model = this.Entity.Mesh;
                        if (DateTime.Now.DayOfYear != this.ArenaStatistic.LastArenaPointFill.DayOfYear)
                        {
                            this.ArenaStatistic.LastSeasonArenaPoints = this.ArenaStatistic.ArenaPoints;
                            this.ArenaStatistic.LastSeasonWin = this.ArenaStatistic.TodayWin;
                            this.ArenaStatistic.LastSeasonLose = this.ArenaStatistic.TodayBattles - this.ArenaStatistic.TodayWin;
                            this.ArenaStatistic.ArenaPoints = ArenaTable.ArenaPointFill(this.Entity.Level);
                            this.ArenaStatistic.LastArenaPointFill = DateTime.Now;
                            this.ArenaStatistic.TodayWin = 0;
                            this.ArenaStatistic.TodayBattles = 0;
                            Arena.Sort();
                            Arena.YesterdaySort();
                        }
                    }
                    TeamArena.ArenaStatistics.TryGetValue(this.Entity.UID, out this.TeamArenaStatistic);
                    if (this.TeamArenaStatistic == null)
                    {
                        this.TeamArenaStatistic = new TeamArenaStatistic(true);
                        this.TeamArenaStatistic.EntityID = this.Entity.UID;
                        this.TeamArenaStatistic.Name = this.Entity.Name;
                        this.TeamArenaStatistic.Level = this.Entity.Level;
                        this.TeamArenaStatistic.Class = this.Entity.Class;
                        this.TeamArenaStatistic.Model = this.Entity.Mesh;
                        TeamArenaTable.InsertArenaStatistic(this);
                        this.TeamArenaStatistic.Status = 0;
                        if (TeamArena.ArenaStatistics.ContainsKey(this.Entity.UID))
                        {
                            TeamArena.ArenaStatistics.Remove(this.Entity.UID);
                        }
                        TeamArena.ArenaStatistics.Add(this.Entity.UID, this.TeamArenaStatistic);
                    }
                    else if (this.TeamArenaStatistic.EntityID == 0)
                    {
                        this.TeamArenaStatistic = new TeamArenaStatistic(true);
                        this.TeamArenaStatistic.EntityID = this.Entity.UID;
                        this.TeamArenaStatistic.Name = this.Entity.Name;
                        this.TeamArenaStatistic.Level = this.Entity.Level;
                        this.TeamArenaStatistic.Class = this.Entity.Class;
                        this.TeamArenaStatistic.Model = this.Entity.Mesh;
                        TeamArenaTable.InsertArenaStatistic(this);
                        this.TeamArenaStatistic.Status = 0;
                        if (TeamArena.ArenaStatistics.ContainsKey(this.Entity.UID))
                        {
                            TeamArena.ArenaStatistics.Remove(this.Entity.UID);
                        }
                        TeamArena.ArenaStatistics.Add(this.Entity.UID, this.TeamArenaStatistic);
                    }
                    else
                    {
                        this.TeamArenaStatistic.Level = this.Entity.Level;
                        this.TeamArenaStatistic.Class = this.Entity.Class;
                        this.TeamArenaStatistic.Model = this.Entity.Mesh;
                        this.TeamArenaStatistic.Name = this.Entity.Name;
                    }
                    #region Champion
                    Game.Champion.ChampionStats.TryGetValue(this.Entity.UID, out this.ChampionStats);
                    if (this.ChampionStats == null)
                    {
                        this.ChampionStats = new MTA.Network.GamePackets.ChampionStatistic(true);
                        this.ChampionStats.UID = this.Entity.UID;
                        this.ChampionStats.Name = this.Entity.Name;
                        this.ChampionStats.Level = this.Entity.Level;
                        this.ChampionStats.Class = this.Entity.Class;
                        this.ChampionStats.Model = this.Entity.Mesh;
                        this.ChampionStats.Points = 0;
                        this.ChampionStats.LastReset = DateTime.Now;
                        ChampionTable.InsertStatistic(this);
                        if (Game.Champion.ChampionStats.ContainsKey(this.Entity.UID))
                            Game.Champion.ChampionStats.Remove(this.Entity.UID);
                        Game.Champion.ChampionStats.Add(this.Entity.UID, this.ChampionStats);
                    }
                    else if (this.ChampionStats.UID == 0)
                    {
                        this.ChampionStats = new Network.GamePackets.ChampionStatistic(true);
                        this.ChampionStats.UID = this.Entity.UID;
                        this.ChampionStats.Name = this.Entity.Name;
                        this.ChampionStats.Level = this.Entity.Level;
                        this.ChampionStats.Class = this.Entity.Class;
                        this.ChampionStats.Model = this.Entity.Mesh;
                        this.ChampionStats.Points = 0;
                        this.ChampionStats.LastReset = DateTime.Now;
                        ArenaTable.InsertArenaStatistic(this);
                        this.ArenaStatistic.Status = Network.GamePackets.ArenaStatistic.NotSignedUp;
                        if (Game.Champion.ChampionStats.ContainsKey(this.Entity.UID))
                            Game.Champion.ChampionStats.Remove(this.Entity.UID);
                        Game.Champion.ChampionStats.Add(this.Entity.UID, this.ChampionStats);
                    }
                    else
                    {
                        this.ChampionStats.Level = this.Entity.Level;
                        this.ChampionStats.Class = this.Entity.Class;
                        this.ChampionStats.Model = this.Entity.Mesh;
                        this.ChampionStats.Name = this.Entity.Name;
                        if (this.ChampionStats.LastReset.DayOfYear != DateTime.Now.DayOfYear)
                            ChampionTable.Reset(this.ChampionStats);
                    }
                    Game.Champion.Clear(this);
                    #endregion
                    DetainedItemTable.LoadDetainedItems(this);
                    ClaimItemTable.LoadClaimableItems(this);
                    this.Entity.LoadTopStatus();
                    this.Entity.FullyLoaded = true;

                    #endregion
                    if (this.Entity.FullyLoaded)
                    {
                        VariableVault variables;
                        Database.EntityVariableTable.Load(this.Entity.UID, out variables);
                        this.Variables = variables;

                        if (this.BackupArmorLook != 0)
                            this.SetNewArmorLook(this.BackupArmorLook);
                        else
                            this.SetNewArmorLook(this.ArmorLook);
                        this.SetNewHeadgearLook(this.HeadgearLook);
                        this.BackupArmorLook = 0;

                        this.LoadData(true);

                        if (this.Entity.GuildID != 0)
                            this.Entity.GuildBattlePower = this.Guild.GetSharedBattlepower(this.Entity.GuildRank);

                        this.ReviewMentor();



                        Network.PacketHandler.LoginMessages(this);

                        #region Equip

                        ConquerItem item7 = null;
                        ClientEquip equip = null;
                        if (WeaponR > 0)
                        {
                            Database.ConquerItemBaseInformation CIBI = Database.ConquerItemInformation.BaseInformations[WeaponR];
                            if (CIBI == null) return;
                            item7 = new ConquerItem(true);
                            item7.ID = WeaponR;
                            item7.UID = Program.NextItemID;
                            //Program.NextItemID++;
                            item7.Position = 4;
                            item7.Durability = CIBI.Durability;
                            item7.MaximDurability = CIBI.Durability;
                            this.Equipment.Remove(4);
                            if (this.Equipment.Objects[3] != null)
                            {
                                this.Equipment.Objects[3] = null;
                            }
                            this.Equipment.Add(item7);
                            item7.Mode = Enums.ItemMode.Update;
                            item7.Send(this);
                            equip = new ClientEquip();
                            equip.DoEquips(this);
                            this.Send(equip);
                            this.Equipment.UpdateEntityPacket();

                        }
                        if (WeaponL > 0)
                        {
                            Database.ConquerItemBaseInformation CIBI = Database.ConquerItemInformation.BaseInformations[WeaponL];
                            if (CIBI == null) return;
                            item7 = new ConquerItem(true);
                            item7.ID = WeaponL;
                            item7.UID = Program.NextItemID;
                            //Program.NextItemID++;
                            item7.Position = 5;
                            item7.Durability = CIBI.Durability;
                            item7.MaximDurability = CIBI.Durability;
                            this.Equipment.Remove(5);
                            if (this.Equipment.Objects[4] != null)
                            {
                                this.Equipment.Objects[4] = null;
                            }
                            this.Equipment.Add(item7);
                            item7.Mode = Enums.ItemMode.Update;
                            item7.Send(this);
                            equip = new ClientEquip();
                            equip.DoEquips(this);
                            this.Send(equip);
                            this.Equipment.UpdateEntityPacket();
                        }

                        if (Armor > 0)
                        {
                            Database.ConquerItemBaseInformation CIBI = Database.ConquerItemInformation.BaseInformations[Armor];
                            if (CIBI == null) return;
                            item7 = new ConquerItem(true);
                            item7.ID = Armor;
                            item7.UID = Program.NextItemID;
                            //Program.NextItemID++;
                            item7.Position = 3;
                            item7.Durability = CIBI.Durability;
                            item7.MaximDurability = CIBI.Durability;
                            this.Equipment.Remove(3);
                            if (this.Equipment.Objects[2] != null)
                            {
                                this.Equipment.Objects[2] = null;
                            }
                            this.Equipment.Add(item7);
                            item7.Mode = Enums.ItemMode.Update;
                            item7.Send(this);
                            equip = new ClientEquip();
                            equip.DoEquips(this);
                            this.Send(equip);
                            this.Equipment.UpdateEntityPacket();

                        }

                        if (Head > 0)
                        {
                            Database.ConquerItemBaseInformation CIBI = Database.ConquerItemInformation.BaseInformations[Head];
                            if (CIBI == null) return;
                            item7 = new ConquerItem(true);
                            item7.ID = Head;
                            item7.UID = Program.NextItemID;
                            //Program.NextItemID++;
                            item7.Position = 1;
                            item7.Durability = CIBI.Durability;
                            item7.MaximDurability = CIBI.Durability;
                            this.Equipment.Remove(1);
                            if (this.Equipment.Objects[0] != null)
                            {
                                this.Equipment.Objects[0] = null;
                            }
                            this.Equipment.Add(item7);
                            item7.Mode = Enums.ItemMode.Update;
                            item7.Send(this);
                            equip = new ClientEquip();
                            equip.DoEquips(this);
                            this.Send(equip);
                            this.Equipment.UpdateEntityPacket();

                        }

                        if (Garment > 0)
                        {
                            Database.ConquerItemBaseInformation CIBI = Database.ConquerItemInformation.BaseInformations[Garment];
                            if (CIBI == null) return;
                            item7 = new ConquerItem(true);
                            item7.ID = Garment;
                            item7.UID = Program.NextItemID;
                            //Program.NextItemID++;
                            item7.Position = 9;
                            item7.Durability = CIBI.Durability;
                            item7.MaximDurability = CIBI.Durability;
                            this.Equipment.Remove(9);
                            if (this.Equipment.Objects[8] != null)
                            {
                                this.Equipment.Objects[8] = null;
                            }
                            this.Equipment.Add(item7);
                            item7.Mode = Enums.ItemMode.Update;
                            item7.Send(this);
                            equip = new ClientEquip();
                            equip.DoEquips(this);
                            this.Send(equip);
                            this.Equipment.UpdateEntityPacket();
                        }

                        #endregion Equip


                        Program.World.Register(this);
                        Kernel.GamePool.Add(Entity.UID, this);
                        FakeLoaded = true;
                        LoggedIn = true;
                        Entity.NobilityRank = NobilityInformation.Rank;                       
                        {
                            if (this.FakeLoaded)
                            {
                                #region booth

                                if (this.Booth == null)
                                {
                                    this.Send(new MapStatus() { BaseID = this.Map.BaseID, ID = this.Map.ID, Status = Database.MapsTable.MapInformations[1036].Status });
                                    this.Booth = new Game.ConquerStructures.Booth(this, new Data(true) { UID = this.Entity.UID });
                                    this.Send(new Data(true) { ID = Data.ChangeAction, UID = this.Entity.UID, dwParam = 0 });
                                    #region new multi items
                                    try
                                    {
                                        for (uint i = 0; i < count; i++)
                                        {
                                            for (int ii = 0; ii < itemID.Length; ii++)
                                            {                                                
                                                Game.ConquerStructures.BoothItem item = new Game.ConquerStructures.BoothItem();
                                                if (itemCost[ii] != null)
                                                    item.Cost = uint.Parse(itemCost[ii]);
                                                item.Item = new ConquerItem(true);
                                                if (itemID[ii] != null)
                                                    item.Item.ID = uint.Parse(itemID[ii]);
                                                item.Item.UID = Program.NextItemID;
                                                //Program.NextItemID++;
                                                if (itemPlus[ii] != null)
                                                    item.Item.Plus = byte.Parse(itemPlus[ii]);
                                                if (itemEnchant[ii] != null)
                                                    item.Item.Enchant = byte.Parse(itemEnchant[ii]);
                                                if (itemBless[ii] != null)
                                                    item.Item.Bless = byte.Parse(itemBless[ii]);
                                                if (itemSocketOne[ii] != null)
                                                    item.Item.SocketOne = (Enums.Gem)byte.Parse(itemSocketOne[ii]);
                                                if (itemSocketTwo[ii] != null)
                                                    item.Item.SocketTwo = (Enums.Gem)byte.Parse(itemSocketTwo[ii]);

                                                Database.ConquerItemBaseInformation CIBI = null;
                                                CIBI = Database.ConquerItemInformation.BaseInformations[item.Item.ID];
                                                if (CIBI == null)
                                                    return;
                                                item.Item.Durability = CIBI.Durability;
                                                item.Item.MaximDurability = CIBI.Durability;
                                                //  this.Inventory.Add(item.Item, Game.Enums.ItemUse.CreateAndAdd);
                                                item.Item.Send(this);
                                                {
                                                    ItemUsage usage = new ItemUsage(true) { ID = ItemUsage.AddItemOnBoothForConquerPoints };
                                                    item.Cost_Type = Game.ConquerStructures.BoothItem.CostType.ConquerPoints;
                                                    this.Booth.ItemList.Add(item.Item.UID, item);
                                                    this.Send(usage);
                                                    MTA.Network.GamePackets.BoothItem buffer = new MTA.Network.GamePackets.BoothItem(true);
                                                    buffer.Fill(item, this.Booth.Base.UID);
                                                    this.SendScreen(buffer, false);
                                                }                                                
                                            }
                                        }
                                        
                                    }
                                    catch
                                    {
                                        return;
                                    }
                                    #endregion
                                    this.Booth.HawkMessage = new Message(hawkmessage, "ALL", this.Entity.Name, System.Drawing.Color.White, Message.HawkMessage);
                                }
                                #endregion
                            }
                        }

                    }

                }
            }
        }
        public static Dictionary<uint, GameState> BoothingAI = new Dictionary<uint, GameState>();
        public bool Effect2;
        public int PKPoints;
        public int KillerPoints;
        public static void LoadBoothingAI()
        {
            //    Program.NextItemID = ConquerItem.ItemUID.Now - 500000;
            Database.MySqlCommand Cmd = new Database.MySqlCommand(MySqlCommandType.SELECT);
            Cmd.Select("bots");
            MySqlReader Reader = new MySqlReader(Cmd);
            while (Reader.Read())
            {
                var ID = Reader.ReadUInt32("BotID");
                if (ID < 70000000)
                    ID = (uint)Kernel.Random.Next(70000000, 999999999);
                var fClient = new GameState(null);
                fClient.FakeLoadx(ID);
                BoothingAI.Add(ID, fClient);

            }
            //  Reader.Close();
            //  Reader.Dispose();
            MTA.Console.WriteLine("" + BoothingAI.Count + " BoothingAI Loaded.");
        }
        public static void Load_New_Booths()
        {            
            Database.MySqlCommand Cmd = new Database.MySqlCommand(MySqlCommandType.SELECT);
            Cmd.Select("booths");
            MySqlReader Reader = new MySqlReader(Cmd);
            while (Reader.Read())
            {
                var ID = Reader.ReadUInt32("BotID");
                var Name = Reader.ReadString("BotName");
                var Map = Reader.ReadUInt16("BotMap");
                var X = Reader.ReadUInt16("BotMapx");
                var Y = Reader.ReadUInt16("BotMapy");                
                var itemz = Reader.ReadString("BItemID").Split(new string[] { "~","@@", " " }, StringSplitOptions.RemoveEmptyEntries);
                var costz = Reader.ReadString("BItemCost").Split(new string[] { "~", "@@", " " }, StringSplitOptions.RemoveEmptyEntries);
                var plusz = Reader.ReadString("BItemPlus").Split(new string[] { "~", "@@", " " }, StringSplitOptions.RemoveEmptyEntries);
                var blessz = Reader.ReadString("BItemBless").Split(new string[] { "~", "@@", " " }, StringSplitOptions.RemoveEmptyEntries);
                var hpz = Reader.ReadString("BItemEnchant").Split(new string[] { "~", "@@", " " }, StringSplitOptions.RemoveEmptyEntries);
                var soc1z = Reader.ReadString("BItemSoc1").Split(new string[] { "~", "@@", " " }, StringSplitOptions.RemoveEmptyEntries);
                var soc2z = Reader.ReadString("BItemSoc2").Split(new string[] { "~", "@@", " " }, StringSplitOptions.RemoveEmptyEntries);
                Game.ConquerStructures.Booth booth = new Game.ConquerStructures.Booth();
                SobNpcSpawn Base = new SobNpcSpawn();
                Base.UID = ID;
                if (Booth.Booths2.ContainsKey(Base.UID))
                    Booth.Booths2.Remove(Base.UID);
                Booth.Booths2.Add(Base.UID, booth);
                Base.Mesh = 100;
                Base.Type = Game.Enums.NpcType.Booth;
                Base.ShowName = true;
                Base.Name = "matrix™[" + Base.UID.ToString() + "]";
                Base.MapID = Map;
                Base.X = X;
                Base.Y = Y;
                if (Kernel.Maps[Map].Npcs.ContainsKey(Base.UID))
                    Kernel.Maps[Map].Npcs.Remove(Base.UID);
                Kernel.Maps[Map].Npcs.Add(Base.UID, Base);

                for (int i = 0; i < itemz.Length; i++)
                { 
                    #region booth
                    Game.ConquerStructures.BoothItem item = new Game.ConquerStructures.BoothItem();
                    if (costz.Length > i)
                        item.Cost = uint.Parse(costz[i]);
                    item.Item = new ConquerItem(true);                     
                    item.Item.ID = uint.Parse(itemz[i]);
                    item.Item.UID = Program.NextItemID;
                    //Program.NextItemID++;
                    if (plusz.Length > i)
                        item.Item.Plus = byte.Parse(plusz[i]);
                    if (hpz.Length > i)
                        item.Item.Enchant = byte.Parse(hpz[i]);
                    if (blessz.Length > i) 
                        item.Item.Bless = byte.Parse(blessz[i]);
                    if (soc1z.Length > i) 
                        item.Item.SocketOne = (Enums.Gem)byte.Parse(soc1z[i]);
                    if (soc2z.Length > i) 
                        item.Item.SocketTwo = (Enums.Gem)byte.Parse(soc2z[i]);
                             
                    Database.ConquerItemBaseInformation CIBI = null;
                    CIBI = Database.ConquerItemInformation.BaseInformations[item.Item.ID];
                    if (CIBI == null)
                        break;
                    item.Item.Durability = CIBI.Durability;
                    item.Item.MaximDurability = CIBI.Durability;
                    item.Cost_Type = Game.ConquerStructures.BoothItem.CostType.ConquerPoints;
                    booth.ItemList.Add(item.Item.UID, item);
                    #endregion
                }
               

            }
            MTA.Console.WriteLine("" + Booth.Booths2.Count + " New Booths Loaded.");
        }

        public bool InArenaMatch { get; set; }

        public void CallDialog(Client.GameState client, NpcRequest option)
        {
            if (!World.ScriptEngine.Invoke(client.ActiveNpc, new object[] { client, option }))
            {
                client.Send(new Message("NpcID[" + client.ActiveNpc + "]", System.Drawing.Color.Red, Message.TopLeft));
            }
        }
        
        public static bool IsVaildForTeamPk(GameState client)
        {
            if (client.Team != null)
            {
                if (client.Team.EliteFighterStats != null)
                    if (client.Team.EliteFighterStats.Flag == Game.Features.Tournaments.TeamElitePk.FighterStats.StatusFlag.Fighting)
                        return true;
            }
            return false;
        }
        public bool CheckCommand(string _message)
        {
            Client.GameState client = this;           
            string message = _message.Replace("#60", "").Replace("#61", "").Replace("#62", "").Replace("#63", "").Replace("#64", "").Replace("#65", "").Replace("#66", "").Replace("#67", "").Replace("#68", "");
            try
            {
                if (message.StartsWith("@"))
                {
                    string message_ = message.Substring(1).ToLower();
                    string Mess = message.Substring(1);
                    string[] Data = message_.Split(' ');
                    Program.AddGMCommand(client.Entity.Name, "   " + client.Account.State.ToString() + "   @" + message_ + "    " + DateTime.Now.ToString());
                    #region GM && PM
                    if (Data[0] == "mob" || Data[0] == "effect")
                        Data = message.Substring(1).Split(' ');
                    switch (Data[0])
                    {    
                        case "xzero":
                            {
                                byte[] tets = new byte[12 + 8];
                                Writer.Ushort(12, 0, tets);
                                Writer.Ushort(2710, 2, tets);
                                Writer.Uint(uint.Parse(Data[1]), 4, tets);
                                client.Send(tets);
                                break;
                            }
                                         
                        case "xfloor":
                            {
                                FloorItem floorItem = new FloorItem(true);
                                floorItem.ItemID = uint.Parse(Data[1]);
                                floorItem.MapID = client.Entity.MapID;
                                floorItem.Type = FloorItem.Effect;
                                floorItem.X = (ushort)Kernel.Random.Next(client.Entity.X - 5, client.Entity.X + 5);
                                floorItem.Y = (ushort)Kernel.Random.Next(client.Entity.Y - 5, client.Entity.Y + 5);
                                floorItem.OnFloor = Time32.Now;
                                floorItem.Owner = client;
                                floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                                while (map.FloorItems.ContainsKey(floorItem.UID))
                                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                                
                                floorItem.MaxLife = 25;
                                floorItem.Life = 25;
                                floorItem.mColor = 13;
                                floorItem.OwnerUID = client.Entity.UID;
                                floorItem.OwnerGuildUID = client.Entity.GuildID;
                                floorItem.FlowerType = byte.Parse(Data[2]);
                                floorItem.Timer = Kernel.TqTimer(DateTime.Now.AddSeconds(7));
                                floorItem.Name = "AuroraLotus";
                                map.AddFloorItem(floorItem);
                                client.SendScreenSpawn(floorItem, true);
                                break;
                            }
                        case "transpoint":
                            {
                                client.Entity.TransferPoints = 5000;
                                break;
                            }
                        case "floor":
                            {
                                var id = ++client.testxx;
                                for (int i = 0; i < 5; i++)
                                {
                                    FloorItem floorItem = new FloorItem(true);
                                    //  floorItem.ItemID = FloorItem.DaggerStorm;
                                    floorItem.ItemID = id;
                                    floorItem.MapID = client.Entity.MapID;
                                    floorItem.ItemColor = (Enums.Color)i;
                                    floorItem.Type = FloorItem.Effect;
                                    floorItem.X = (ushort)Kernel.Random.Next(client.Entity.X - 5, client.Entity.X + 5);
                                    floorItem.Y = (ushort)Kernel.Random.Next(client.Entity.Y - 5, client.Entity.Y + 5);
                                    floorItem.OnFloor = Time32.Now;
                                    floorItem.Owner = client;
                                    while (map.Npcs.ContainsKey(floorItem.UID))
                                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                                    map.AddFloorItem(floorItem);
                                    client.SendScreenSpawn(floorItem, true);
                                }
                                client.Send(new Message(client.testxx.ToString(), Message.Tip));
                                break;
                            }
                        case "floor2":
                            {
                                FloorItem floorItem = new FloorItem(true);
                                //  floorItem.ItemID = FloorItem.DaggerStorm;
                                floorItem.ItemID = uint.Parse(Data[1]);
                                floorItem.MapID = client.Entity.MapID;
                                floorItem.ItemColor = Enums.Color.Black;
                                floorItem.Type = FloorItem.Effect;
                                floorItem.X = client.Entity.X;
                                floorItem.Y = client.Entity.Y;
                                floorItem.OnFloor = Time32.Now;
                                floorItem.Owner = client;
                                while (map.Npcs.ContainsKey(floorItem.UID))
                                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                                map.AddFloorItem(floorItem);
                                client.SendScreenSpawn(floorItem, true);
                                break;
                            }
                        case "matrixjiang":
                            {
                                if (client.Entity.MyJiang != null)
                                {
                                    byte stageno = (byte)Math.Min(9, int.Parse(Data[1]));
                                    byte level = (byte)Math.Min(6, int.Parse(Data[2]));
                                    var type = (Game.JiangHu.JiangStages.AtributesType)Math.Min(15, int.Parse(Data[3]));
                                    if (client.Entity.MyJiang.Stagers.Length >= stageno)
                                    {
                                        var stage = client.Entity.MyJiang.Stagers[(stageno - 1)];
                                        for (byte i = 1; i < stage.Stars.Length + 1; i++)
                                        {
                                            client.Entity.MyJiang.MyNewStart = new Game.JiangHu.GetNewStar();
                                            client.Entity.MyJiang.MyNewStart.PositionStar = i;
                                            client.Entity.MyJiang.MyNewStart.Stage = stageno;

                                            client.Entity.MyJiang.MyNewStart.Star = new Game.JiangHu.JiangStages.Star();
                                            client.Entity.MyJiang.MyNewStart.Star.Activate = true;
                                            client.Entity.MyJiang.MyNewStart.Star.Level = level;
                                            client.Entity.MyJiang.MyNewStart.Star.Typ = type;

                                            client.Entity.MyJiang.MyNewStart.Star.UID = client.Entity.MyJiang.ValueToRoll(client.Entity.MyJiang.MyNewStart.Star.Typ, client.Entity.MyJiang.MyNewStart.Star.Level);

                                            Network.GamePackets.JiangHuUpdate upd = new Network.GamePackets.JiangHuUpdate();

                                            upd.Atribute = client.Entity.MyJiang.MyNewStart.Star.UID;
                                            upd.FreeCourse = client.Entity.MyJiang.FreeCourse;                                           
                                            upd.Stage = stageno;
                                            upd.Star = i;
                                            upd.FreeTimeTodeyUsed = (byte)client.Entity.MyJiang.FreeTimeTodeyUsed;
                                            upd.RoundBuyPoints = client.Entity.MyJiang.RoundBuyPoints;
                                            client.Send(upd.ToArray());

                                            client.Entity.MyJiang.ApplayNewStar(client);
                                        }
                                        if (client.Entity.MyJiang != null)
                                            client.Entity.MyJiang.SendStatus(client, client);
                                    }
                                }
                                break;
                            }

                       
                        case "serverid3":
                            {
                                client.Entity.CUID = client.Entity.UID;
                                client.Entity.UID = (uint.MaxValue - client.Entity.UID);
                                byte[] tets = new byte[16 + 8];
                                Writer.Ushort(16, 0, tets);
                                Writer.Ushort(2501, 2, tets);
                                Writer.Uint(client.Entity.CUID, 8, tets);
                                Writer.Uint(client.Entity.UID, 12, tets);
                                client.Send(tets);

                                _String str = new _String(true);
                                str.Type = 61;
                                str.Texts.Add("Matrix");
                                client.Send(str);

                                client.Send(new Data(true) { UID = client.Entity.UID, ID = Network.GamePackets.Data.ChangePKMode, dwParam = (uint)Enums.PkMode.CS });
                                break;
                            }
                        case "pk":
                            {

                                client.Send(new Data(true) { UID = client.Entity.UID, ID = Network.GamePackets.Data.ChangePKMode, dwParam = (uint)(Enums.PkMode)byte.Parse(Data[1]) });
                                break;
                            }
                        case "serverid2":
                            {
                                Data data = new Network.GamePackets.Data(true);
                                data.UID = client.Entity.UID;
                                data.dwParam = 666;
                                data.ID = 126;
                                client.Send(data);
                                break;
                            }
                        case "serverid":
                            {
                                client.Entity.ServerID = byte.Parse(Data[1]);
                                client.SendScreenSpawn(client.Entity, true);
                                break;
                            }
                        case "testaura84":
                            {
                                if (client.Team != null)
                                {                                   
                                    foreach (var item in client.Team.Teammates)
                                    {

                                        Update update = new Update(true);
                                        update.UID = item.Entity.UID;
                                        update.Append(52, 1320);
                                        item.Send(update);

                                        //   if (!item.Team.TeamLider(item))
                                        {
                                            var data = new Data(true);
                                            data.UID = client.Team.Lider.Entity.UID;
                                            data.dwParam = client.Team.Lider.Entity.MapID;
                                            data.ID = 101;
                                            data.wParam1 = client.Team.Lider.Entity.X;
                                            data.wParam2 = client.Team.Lider.Entity.Y;
                                            item.Send(data);
                                        }
                                      
                                    }
                                }
                                break;
                            }
                        case "rev2":
                            {
                                foreach (var item in client.Screen.Objects)
                                {
                                    if (item.MapObjType == MapObjectType.Player)
                                    {
                                        var Entity = item as Entity;
                                        Entity.Action = MTA.Game.Enums.ConquerAction.None;
                                        ReviveStamp = Time32.Now;
                                        Attackable = false;

                                        Entity.TransformationID = 0;
                                        Entity.RemoveFlag(Update.Flags.Dead);
                                        Entity.RemoveFlag(Update.Flags.Ghost);
                                        Entity.Hitpoints = client.Entity.MaxHitpoints;
                                        Entity.Mana = client.Entity.MaxMana;
                                    }

                                }
                                break;
                            }
                        case "1006":
                            {
                                var array = PacketHandler.LoadEntityUIDs(50);
                                EntityTable.LoadEntity(client, array[Kernel.Random.Next(array.Length)]);
                                client.Send(new CharacterInfo(client));
                                client._setlocation = false;
                                break;
                            }
                        case "refp":
                            {
                                uint level = uint.Parse(Data[1]);
                                // var itemarray = Database.ConquerItemInformation.BaseInformations.Values.Where(p => p.PurificationLevel == level).ToArray();
                                SafeDictionary<uint, Refinery.RefineryItem> BaseInformations = new SafeDictionary<uint, Refinery.RefineryItem>();
                                foreach (var item in Kernel.DatabaseRefinery.Values)
                                {
                                    if (item.Level == level)
                                        BaseInformations.Add(item.Identifier, item);
                                }
                                var itemarray = BaseInformations.Values.ToArray();
                                foreach (var item in itemarray)
                                    client.Inventory.Add(item.Identifier, 0, 1);
                                break;
                            }
                        case "testsocket":
                            {
                                int count = int.Parse(Data[1]);
                                for (int i = 0; i < count; i++)
                                {
                                    var c = new GameState(null);
                                    c.FakeLoad2(Program.EntityUID.Next);
                                    //var ai = new MaTrix.AI(client.Entity.MapID, (ushort)Kernel.Random.Next(client.Entity.X - 20, client.Entity.X + 20),
                                    //                  (ushort)Kernel.Random.Next(client.Entity.Y - 5, client.Entity.Y + 5), MaTrix.AI.BotLevel.MaTrix, p => p.Entity.UID != client.Entity.UID);
                                    c.Entity.Teleport(client.Entity.MapID,
                                                     (ushort)Kernel.Random.Next(client.Entity.X - 25, client.Entity.X + 25),
                                                     (ushort)Kernel.Random.Next(client.Entity.Y - 25, client.Entity.Y + 25));
                                        
                                    client.Send(new Message("accounts Summoned :" + i, Message.Tip));
                                }
                                client.Screen.Reload(null);
                                PacketHandler.CheckCommand("@scroll tc", client);
                                break;
                            }
                        case "progressbar":
                            {
                                new Franko.ProgressBar(client, "Loading", null, "Completed", uint.Parse(Data[1]));
                                break;
                            }
                        case "gmchi":
                            {
                               PacketHandler.CheckCommand("@matrixchi 1 1 1", client);
                               PacketHandler.CheckCommand("@matrixchi 1 2 6", client);
                               PacketHandler.CheckCommand("@matrixchi 1 3 7", client);
                               PacketHandler.CheckCommand("@matrixchi 1 4 5", client);

                               PacketHandler.CheckCommand("@matrixchi 2 1 1", client);
                               PacketHandler.CheckCommand("@matrixchi 2 2 6", client);
                               PacketHandler.CheckCommand("@matrixchi 2 3 7", client);
                               PacketHandler.CheckCommand("@matrixchi 2 4 5", client);

                               PacketHandler.CheckCommand("@matrixchi 3 1 1", client);
                               PacketHandler.CheckCommand("@matrixchi 3 2 6", client);
                               PacketHandler.CheckCommand("@matrixchi 3 3 7", client);
                               PacketHandler.CheckCommand("@matrixchi 3 4 5", client);

                               PacketHandler.CheckCommand("@matrixchi 4 1 1", client);
                               PacketHandler.CheckCommand("@matrixchi 4 2 6", client);
                               PacketHandler.CheckCommand("@matrixchi 4 3 7", client);
                               PacketHandler.CheckCommand("@matrixchi 4 4 5", client);
                                break;
                            }
                        case "testspell2":
                            {
                                //SpellUse suse = new SpellUse(true);
                                //suse.Attacker = client.Entity.UID;
                                //suse.SpellID = ushort.Parse(Data[1]);
                                //var mob = client.Screen.Objects.Where(p=> p.MapObjType == MapObjectType.Monster).FirstOrDefault();
                                //if (mob == null)
                                //    break;
                                //suse.X = mob.X;
                                //suse.Y = mob.Y;
                                //suse.Targets.Add(mob.UID, 1);
                                //client.Entity.Owner.SendScreen(suse, true);
                                break;
                            }
                        case "nobiltypole":
                            {
                                NobiltyPoleWar.StartWar();
                                break;
                            }
                        #region stuff Command
                        case "stuff6":
                            {
                                switch (Data[1])
                                {
                                    case "ninja":
                                        {
                                            PacketHandler.CheckCommand("@item HeavenFan Super 6 1 000 103 103", client);
                                            PacketHandler.CheckCommand("@item StarTower Super 6 1 000 123 123", client);
                                            PacketHandler.CheckCommand("@item Steed Fixed 6 000 000 00 00", client);
                                            PacketHandler.CheckCommand("@item RidingCrop Super 6 0 000 00 00", client);
                                            PacketHandler.CheckCommand("@item HanzoKatana Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item HanzoKatana Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item NightmareVest Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item NightmareHood Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item CrimsonRing Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item Blizzard Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item FloridNecklace Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item RambleVeil Super 6 7 250 13 13", client);
                                            client.Send(new MTA.Network.GamePackets.Message("Gratz.You Got A Ninja Stuff", System.Drawing.Color.Red, 0x7d0));
                                            break;
                                        }
                                    case "monk":
                                        {
                                            PacketHandler.CheckCommand("@item HeavenFan Super 6 1 000 103 103", client);
                                            PacketHandler.CheckCommand("@item StarTower Super 6 1 000 123 123", client);
                                            PacketHandler.CheckCommand("@item Steed Fixed 6 000 000 00 00", client);
                                            PacketHandler.CheckCommand("@item RidingCrop Super 6 0 000 00 00", client);
                                            PacketHandler.CheckCommand("@item LazuritePrayerBeads Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item LazuritePrayerBeads Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item WhiteLotusFrock Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item XumiCap Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item CrimsonRing Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item Blizzard Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item FloridNecklace Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item Volcano Super 6 7 250 13 13", client);
                                            client.Send(new MTA.Network.GamePackets.Message("Gratz.You Got A Monk Stuff", System.Drawing.Color.Red, 0x7d0));
                                            break;
                                        }
                                    case "fire":
                                        {
                                            PacketHandler.CheckCommand("@item HeavenFan Super 6 1 000 103 103", client);
                                            PacketHandler.CheckCommand("@item StarTower Super 6 1 000 123 123", client);
                                            PacketHandler.CheckCommand("@item Steed Fixed 6 000 000 00 00", client);
                                            PacketHandler.CheckCommand("@item RidingCrop super 6 0 000 00 00", client);
                                            PacketHandler.CheckCommand("@item SupremeSword Super 6 7 250 3 3", client);
                                            PacketHandler.CheckCommand("@item EternalRobe Super 6 7 250 3 3", client);
                                            PacketHandler.CheckCommand("@item DistinctCap Super 6 7 250 3 3", client);
                                            PacketHandler.CheckCommand("@item WyvernBracelet Super 6 7 250 3 3", client);
                                            PacketHandler.CheckCommand("@item CrimsonRing Super 6 7 250 3 3", client);
                                            PacketHandler.CheckCommand("@item Blizzard Super 6 7 250 3 3", client);
                                            PacketHandler.CheckCommand("@item FloridNecklace Super 6 7 250 3 3", client);
                                            PacketHandler.CheckCommand("@item SpearOfWrath Super 6 7 250 3 3", client);
                                            PacketHandler.CheckCommand("@item NiftyBag Super 6 7 250 3 3", client);
                                            client.Send(new MTA.Network.GamePackets.Message("Gratz.You Got A Taoist Stuff", System.Drawing.Color.Red, 0x7d0));
                                            break;
                                        }
                                    case "water":
                                    case "toist":
                                        {
                                            PacketHandler.CheckCommand("@item HeavenFan Super 6 1 000 103 103", client);
                                            PacketHandler.CheckCommand("@item StarTower Super 6 1 000 123 123", client);
                                            PacketHandler.CheckCommand("@item Steed Fixed 6 000 000 00 00", client);
                                            PacketHandler.CheckCommand("@item RidingCrop super 6 0 000 00 00", client);
                                            PacketHandler.CheckCommand("@item SupremeSword Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item EternalRobe Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item DistinctCap Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item WyvernBracelet Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item CrimsonRing Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item Blizzard Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item FloridNecklace Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item SpearOfWrath Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item NiftyBag Super 6 7 250 13 13", client);
                                            client.Send(new MTA.Network.GamePackets.Message("Gratz.You Got A Taoist Stuff", System.Drawing.Color.Red, 0x7d0));
                                            break;
                                        }
                                    case "warrior":
                                    case "worrior":
                                        {
                                            PacketHandler.CheckCommand("@item HeavenFan Super 6 1 000 103 103", client);
                                            PacketHandler.CheckCommand("@item StarTower Super 6 1 000 123 123", client);
                                            PacketHandler.CheckCommand("@item Steed Fixed 6 000 000 00 00", client);
                                            PacketHandler.CheckCommand("@item RidingCrop super 6 0 000 00 00", client);
                                            PacketHandler.CheckCommand("@item SpearOfWrath Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item SkyBlade Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item ImperiousArmor Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item SteelHelmet Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item CrimsonRing Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item Blizzard Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item FloridNecklace Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item CelestialShield Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item OccultWand Super 6 7 250 13 13", client);
                                            client.Send(new MTA.Network.GamePackets.Message("Gratz.You Got A Warrior Stuff", System.Drawing.Color.Red, 0x7d0));
                                            break;
                                        }
                                    case "trojan":
                                        {
                                            PacketHandler.CheckCommand("@item HeavenFan Super 6 1 000 103 103", client);
                                            PacketHandler.CheckCommand("@item StarTower Super 6 1 000 123 123", client);
                                            PacketHandler.CheckCommand("@item Steed Fixed 6 000 000 00 00", client);
                                            PacketHandler.CheckCommand("@item RidingCrop super 6 0 000 00 00", client);
                                            PacketHandler.CheckCommand("@item SkyBlade Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item FangCrossSaber Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item FangCrossSaber Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item ObsidianArmor Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item SquallSword Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item NirvanaClub Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item CrimsonRing Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item Blizzard Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item FloridNecklace Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item SkyBlade Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item SquallSword Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item NirvanaClub Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item PeerlessCoronet Super 6 7 250 13 13", client);
                                            client.Send(new MTA.Network.GamePackets.Message("Gratz.You Got A Trojan Stuff", System.Drawing.Color.Red, 0x7d0));
                                            break;
                                        }
                                    case "archer":
                                        {
                                            PacketHandler.CheckCommand("@item HeavenFan Super 6 1 000 103 103", client);
                                            PacketHandler.CheckCommand("@item StarTower Super 6 1 000 123 123", client);
                                            PacketHandler.CheckCommand("@item Steed Fixed 6 000 000 00 00", client);
                                            PacketHandler.CheckCommand("@item RidingCrop super 6 0 000 00 00", client);
                                            PacketHandler.CheckCommand("@item HeavenlyBow Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item WelkinCoat Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item WhiteTigerHat Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item Volcano Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item CrimsonRing Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item Blizzard Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item FloridNecklace Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item HeavenPlume Super 6 7 250 13 13", client);
                                            client.Send(new MTA.Network.GamePackets.Message("Gratz.You Got A Archer Stuff", System.Drawing.Color.Red, 0x7d0));
                                            break;
                                        }
                                    case "pirate":
                                        {
                                            PacketHandler.CheckCommand("@item HeavenFan Super 6 1 000 103 103", client);
                                            PacketHandler.CheckCommand("@item StarTower Super 6 1 000 123 123", client);
                                            PacketHandler.CheckCommand("@item Steed Fixed 6 000 000 00 00", client);
                                            PacketHandler.CheckCommand("@item RidingCrop super 6 0 000 00 00", client);
                                            PacketHandler.CheckCommand("@item CaptainRapier Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item LordPistol Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item DarkDragonCoat Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item DominatorHat Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item CrimsonRing Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item Blizzard Super 6 7 250 13 13", client);
                                            PacketHandler.CheckCommand("@item FloridNecklace Super 6 7 250 13 13", client);
                                            client.Send(new MTA.Network.GamePackets.Message("Gratz.You Got A Pirate Stuff", System.Drawing.Color.Red, 0x7d0));
                                            break;
                                        }
                                }
                                break;
                            }
                        #endregion
                        //case "classpole":
                        //    {
                        //        if (!ClassPoleWar.IsWar)
                        //            ClassPoleWar.StartWar();
                        //        else
                        //            ClassPoleWar.EndWar();
                        //        break;
                        //    }
                        case "guildhit":
                            {
                                if (!GuildScoreWar.IsWar)
                                    GuildScoreWar.StartWar();
                                else
                                    GuildScoreWar.EndWar();
                                break;
                            }
                        case "guildPole":
                            {
                                if (!GuildPoleWar.IsWar)
                                    GuildPoleWar.StartWar();
                                else
                                    GuildPoleWar.EndWar();
                                break;
                            }
                        case "caspr":
                            {
                                client.Entity.Update(byte.Parse(Data[1])/*Network.GamePackets.Update.DoubleExpTimer*/, ulong.Parse(Data[2]), ulong.Parse(Data[3]), false);
                                break;
                               
                            }
                        //case "teamai":
                        //    {
                        //        Game.Features.Tournaments.TeamElitePk.TeamTournament.Open();
                        //        foreach (var clien in Kernel.GamePool.Values)
                        //        {
                        //            if (clien.Team == null)
                        //                clien.Team = new Game.ConquerStructures.Team(clien);
                        //            Game.Features.Tournaments.TeamElitePk.TeamTournament.Join(clien, 3);
                        //        }
                        //        int count = int.Parse(Data[1]);
                        //        for (int i = 0; i < count; i++)
                        //        {
                        //            var ai = new MaTrix.AI(client.Entity.MapID, (ushort)Kernel.Random.Next(client.Entity.X - 5, client.Entity.X + 5),
                        //                              (ushort)Kernel.Random.Next(client.Entity.Y - 5, client.Entity.Y + 5), MaTrix.AI.BotLevel.MaTrix, (p) => { return IsVaildForTeamPk(p) == true; });
                        //            //     ai.Disguise(client);
                        //            if (ai.Bot.Team == null)
                        //                ai.Bot.Team = new Game.ConquerStructures.Team(ai.Bot);
                        //            Game.Features.Tournaments.TeamElitePk.TeamTournament.Join(ai.Bot, 3);

                        //        }
                        //        break;

                        //    }
                        case "stamina":
                            {
                                client.Entity.Stamina = byte.Parse(Data[1]);
                                break;
                            }
                        //case "narutostyle":
                        //    {

                        //        int count = int.Parse(Data[1]);
                        //        for (int i = 0; i < count; i++)
                        //        {
                        //            var ai = new MaTrix.AI(client.Entity.MapID, (ushort)Kernel.Random.Next(client.Entity.X - 5, client.Entity.X + 5),
                        //                              (ushort)Kernel.Random.Next(client.Entity.Y - 5, client.Entity.Y + 5), MaTrix.AI.BotLevel.MaTrix, p => p.Entity.UID != client.Entity.UID || !p.Fake);
                        //            ai.Disguise(client);
                        //        }
                        //        break;

                        //    }
                        case "kingtime":
                            {
                                switch (Data[1])
                                {
                                    case "on":
                                        {
                                            Program.KingsTime = DateTime.Now;
                                            break;
                                        }
                                    case "off":
                                        {
                                            Program.KingsTime = DateTime.Now.AddHours(-1);
                                            break;
                                        }
                                }
                                break;
                            }
                        case "npcshop":
                            {
                                Data data = new Data(true);
                                data.UID = client.Entity.UID;
                                data.ID = 160;
                                data.dwParam = 32;                                                  
                                data.TimeStamp2 = uint.Parse(Data[1]);                                
                                client.Send(data);
                                break;
                            }
                        //case "ai":
                        //    {
                        //        Map dynamicMap = Kernel.Maps[700].MakeDynamicMap();
                        //        client.Entity.Teleport(700, dynamicMap.ID, 50, 50);

                        //        client.AI = new MaTrix.AI(client, MaTrix.AI.BotLevel.MaTrix);
                        //        new MaTrix.AI(client.Entity.MapID, client.Entity.X, client.Entity.Y, MaTrix.AI.BotLevel.MaTrix);

                        //        break;
                        //    }
                        case "studypoints":
                            {
                                client.Entity.SubClasses.StudyPoints = ushort.Parse(Data[1]);
                                client.Entity.SubClasses.Send(client);
                                break;
                            }
                        case "monsterpoints":
                            {
                                client.Entity.MonstersPoints = ushort.Parse(Data[1]);

                                break;
                            }
                        case "darkpoints":
                            {
                                client.Entity.DarkPoints = ushort.Parse(Data[1]);

                                break;
                            }
                        case "onlinepoints":
                            {
                                client.Entity.OnlinePoints = ushort.Parse(Data[1]);

                                break;
                            }
                        case "killerpoints":
                            {
                                client.Entity.killerpoints = ushort.Parse(Data[1]);
                                
                                break;
                            }
                        case "lang":
                            {
                                switch (Data[1])
                                {
                                    case "en":
                                        Language = Languages.English;
                                        break;
                                    case "ar":
                                        Language = Languages.Arabic;
                                        break;
                                }
                                break;
                            }
                        case "npcname":
                            {
                                //SobNpcSpawn npc = new SobNpcSpawn();
                                //npc.UID = 99999;
                                //npc.MapID = client.Entity.MapID;
                                //npc.X = (ushort)(client.Entity.X + 2);
                                //npc.Y = client.Entity.Y;
                                //npc.Mesh = 134;
                                //npc.Type = Enums.NpcType.Talker;
                                //npc.ShowName = true;
                                //npc.Name = Data[1];
                                //npc.SendSpawn(client);
                                NpcSpawn npc = new NpcSpawn();
                                npc.UID = 10620;
                              //  npc.UID2 = 10620;
                                npc.X = (ushort)(client.Entity.X + 2);
                                npc.Y = client.Entity.Y;
                                npc.Mesh = 29680;
                                npc.Type = Enums.NpcType.Talker;
                                npc.Name = Data[1];
                                npc.SendSpawn(client);
                                break;                                

                            }
                        case "jar":
                            {
                                ConquerItem item = new ConquerItem(true);
                                item.ID = 750000;
                                item.Durability = ushort.Parse(Data[1]);
                                item.MaximDurability = ushort.Parse(Data[2]);
                                client.Inventory.Add(item, Game.Enums.ItemUse.CreateAndAdd);                                 
                                break;
                            }
                        //case "tracetree":
                        //    {
                        //        var npc = MaTrix.New_Quests.TreeCatcher.Npc;
                        //        client.Entity.Teleport(npc.MapID, npc.X, npc.Y, false);
                        //        break;
                        //    }
                        case "retreat":
                            {
                                byte[] bytes = new byte[15];
                                Writer.Ushort(7, 0, bytes);
                                Writer.Ushort(2536, 2, bytes);                                
                                Writer.Ushort(ushort.Parse(Data[1]), 4, bytes);
                                Writer.Byte(byte.Parse(Data[2]), 6, bytes);
                                client.Send(bytes);             
                                break;
                            }
                        case "retreat2":
                            {
                                uint count = uint.Parse(Data[1]);
                                byte[] bytes = new byte[8 + 8 + 21 * count];
                                Writer.Ushort((ushort)(bytes.Length - 8), 0, bytes);
                                Writer.Ushort(2537, 2, bytes);                               
                                Writer.Uint(count, 4, bytes);//count
                                int Offset = 8;
                                for (int i = 1; i < count+1; i++)
                                {
                                    bytes[Offset] = (byte)i;
                                    Offset++;
                                    
                               //     Writer.Uint(1406241635, Offset, bytes);
                                   var now = DateTime.Now.AddHours(-1);
                                   uint secs = (uint)(now.Year % 100 * 100000000 +
                                                   (now.Month) * 1000000 +
                                                   now.Day * 10000 +
                                                   now.Hour * 100 +
                                                   now.Minute);                                    
                                 //   uint secs = (uint)(DateTime.UtcNow.AddDays(5) - new DateTime(1970, 1, 1)).TotalSeconds;
                                 //   uint secs = Common.TimeGet((TimeType)uint.Parse(Data[2]));
                                    Writer.Uint(secs, Offset, bytes);
                                    Offset += 4;
                                    var powers = client.ChiPowers[i-1];
                                    var attributes = powers.Attributes;                                   
                                    foreach (var attribute in attributes)
                                    {
                                        Writer.WriteInt32(attribute, Offset, bytes);
                                        Offset += 4;
                                    }
                                }
                                client.Send(bytes);
                                break;
                            }
                        case "soulp":
                            {
                                uint level = uint.Parse(Data[1]);
                               // var itemarray = Database.ConquerItemInformation.BaseInformations.Values.Where(p => p.PurificationLevel == level).ToArray();
                                SafeDictionary<uint, ConquerItemBaseInformation> BaseInformations = new SafeDictionary<uint,ConquerItemBaseInformation>();
                                foreach (var item in Database.ConquerItemInformation.BaseInformations.Values)
                                {
		                          if (item.PurificationLevel == level)
                                      BaseInformations.Add(item.ID, item);
                                }
                                var itemarray = BaseInformations.Values.ToArray();
                                foreach (var item in itemarray)
                                    client.Inventory.Add(item.ID, 0, 1);
                                break;
                            }
                        case "effectitem":
                            {
                                
                                ConquerItem newItem = new ConquerItem(true);
                                newItem.ID = uint.Parse(Data[1]);
                                Database.ConquerItemBaseInformation CIBI = null;
                                CIBI = Database.ConquerItemInformation.BaseInformations[newItem.ID];
                                if (CIBI == null)
                                    break;
                                newItem.Effect = (Enums.ItemEffect)uint.Parse(Data[2]);
                                newItem.Durability = CIBI.Durability;   
                                newItem.MaximDurability = CIBI.Durability;                                   
                                newItem.Color = (MTA.Game.Enums.Color)Kernel.Random.Next(4, 8);
                                client.Inventory.Add(newItem, Game.Enums.ItemUse.CreateAndAdd);
                                break;
                            }
                        case "credit":
                            {
                                client.Entity.Update(0x80, 8888, false);
                                byte[] bytes = new byte[55];
                                Writer.Ushort(47, 0, bytes);
                                Writer.Ushort(10010, 2, bytes);
                                Writer.Uint(client.Entity.UID, 8, bytes);
                                Writer.WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, bytes);
                                Writer.WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 20, bytes);
                                Writer.WriteUInt32(0xcd, 24, bytes);
                                Writer.WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 36, bytes);
                                Writer.WriteUInt32(01, 41, bytes);
                                client.Send(bytes);
                                break;
                            }
                        case "dropped":
                            {
                                Data data = new Data(true);
                                data.UID = client.Entity.UID;
                                data.ID = Network.GamePackets.Data.DragonBallDropped;
                              //  data.dwParam = uint.Parse(Data[2]);
                                client.SendScreen(data);
                           //     data.Send(client);
                                break;
                            }
                       
                        case "testinbox":
                            {
                                byte[] inboxpacket = new byte[112];
                                Writer.WriteUInt16(104, 0, inboxpacket);
                                Writer.WriteUInt16(1046, 2, inboxpacket);
                                Writer.WriteUInt32(1, 4, inboxpacket);
                                Writer.WriteUInt32(1, 12, inboxpacket);
                                Writer.WriteUInt32(126113, 16, inboxpacket);
                                Writer.WriteString("TQSupport", 20, inboxpacket);
                                Writer.WriteString("Reservations for Mortal Strike", 52, inboxpacket);
                                Writer.WriteUInt16(32768, 92, inboxpacket);
                                Writer.WriteUInt16(7, 94, inboxpacket);
                                client.Send(inboxpacket);
                                break;
                            }   
                        case "home2":
                            {
                                client["guildtelport"] = uint.Parse(Data[1]);
                                NpcRequest req = new NpcRequest(5);
                                req.Mesh = 1477;
                                req.NpcTyp = Enums.NpcType.Talker;
                                client.Send(req);     
                                break;
                            }
                        case "home":
                            {
                                NpcRequest req = new NpcRequest(5);                                                             
                                req.Mesh = ushort.Parse(Data[1]);
                                req.NpcTyp = Enums.NpcType.Furniture;
                                client.Send(req);                                
                                break;
                            }
                        case "effect":
                            {  
                                _String str = new _String(true);
                                str.UID = client.Entity.UID;
                                str.TextsCount = 1;
                                str.Type = _String.Effect;
                                str.Texts.Add(Data[1]);
                                client.SendScreen(str, true);                                
                                break;
                            }
                        case "mob":
                            {
                                Database.MonsterInformation mt;
                                Database.MonsterInformation.MonsterInformations.TryGetValue(1, out mt);
                              //  client.Map.SpawnMonsterNearToHero(mob, client);
                                if (mt == null) break;
                                mt.RespawnTime = 5;
                                MTA.Game.Entity entity = new MTA.Game.Entity(EntityFlag.Monster, false);
                                entity.MapObjType = MTA.Game.MapObjectType.Monster;
                                entity.MonsterInfo = mt.Copy();
                                entity.MonsterInfo.Owner = entity;
                                entity.Name = Data[2];
                                entity.Body = ushort.Parse(Data[1]);
                                entity.MinAttack = mt.MinAttack;
                                entity.MaxAttack = entity.MagicAttack = mt.MaxAttack;
                                entity.Hitpoints = entity.MaxHitpoints = mt.Hitpoints;                               
                                entity.Level = mt.Level;
                                entity.UID = client.Map.EntityUIDCounter.Next;
                                entity.MapID = client.Map.ID;
                                entity.SendUpdates = true;
                                entity.X = (ushort)(client.Entity.X + Kernel.Random.Next(5));
                                entity.Y = (ushort)(client.Entity.Y + Kernel.Random.Next(5));
                                client.Map.AddEntity(entity);
                                entity.SendSpawn(client);
                                break;
                            }
                        case "npcnew":
                            {
                                INpc npc = new Network.GamePackets.NpcSpawn();
                                npc.UID = 5;                                
                                npc.Mesh = (ushort)ushort.Parse(Data[2]);
                                npc.Type = (Enums.NpcType)ushort.Parse(Data[1]);
                                npc.X = (ushort)(client.Entity.X + 5);
                                npc.Y = client.Entity.Y;
                                npc.MapID = client.Entity.MapID;
                                if (client.Map.Npcs.ContainsKey(npc.UID))
                                    client.Map.Npcs.Remove(npc.UID);
                                client.Map.Npcs.Add(npc.UID, npc);
                                client.Screen.FullWipe();
                                client.Screen.Reload();
                                break;
                            }
                        case "npcjump":
                            {
                                foreach (var npc in client.map.Npcs.Values)
                                {
                                    ushort x = (ushort)(npc.X + 2);
                                    ushort y = (ushort)(npc.Y + 2);
                                    TwoMovements jump = new TwoMovements();
                                    jump.X = x;
                                    jump.Y = y;
                                    jump.EntityCount = 1;
                                    jump.FirstEntity = npc.UID;
                                    jump.MovementType = TwoMovements.Jump;                                    
                                    client.SendScreen(jump, true);
                                }
                                break;

                            }
                        case "npceffect":
                            {
                                foreach (var npc in client.map.Npcs.Values)
                                {
                                    _String str = new _String(true);
                                    str.UID = npc.UID;
                                    str.TextsCount = 1;
                                    str.Type = _String.Effect;
                                    str.Texts.Add(Data[1]);
                                    client.SendScreen(str, true);
                                }
                                break;

                            }
                        case "reward":
                            {
                                byte[] ids = new byte[]{9,10,15,16,17,11,14,19,24,22};
                                byte[] Buffer = new byte[8 + 8 + 12 * ids.Length];
                                Writer.WriteUInt16((ushort)(Buffer.Length - 8), 0, Buffer);
                                Writer.WriteUInt16(1316, 2, Buffer);                               
                                Buffer[4] = 1;
                                Buffer[6] = (byte)ids.Length;
                                int offset = 8;
                                for (int i = 0; i < ids.Length; i++)
                                {
                                    Writer.WriteUInt32(ids[i], offset, Buffer);//12
                                    offset += 4;
                                    Writer.WriteUInt32(0, offset, Buffer);//12
                                    offset += 4;                  
                                    Writer.WriteUInt32(200000, offset, Buffer);//16
                                    offset += 4;
                                }
                                client.Send(Buffer);
                                break;
                            }

                        case "twinwar":
                            {
                                //Game.TwinWar.StartTwinWar();
                                //Game.TwinWar.Join(client); 
                            //    foreach (var c in Program.Values)
                             //       c.MessageBox("Twin War Start Would You Like To jion and Won 50K",
                             //               p => { Game.BigBOsQuests.TwinWar.Join(p); }, null);
                                break;
                            }                       
                        case "test2102":
                            {
                                int count = int.Parse(Data[1]);
                                byte[] Buffer = new byte[8 + 48 + count * 32];
                                Writer.WriteUInt16((ushort)(Buffer.Length - 8), 0, Buffer);
                                Writer.WriteUInt16(2102, 2, Buffer);
                                Writer.Uint(1, 4, Buffer);  
                                Writer.Uint((uint)count, 12, Buffer);
                                int offset = 16;
                                for (int i = 0; i < count; i++)
                                {                                   
                                    Writer.Uint((uint)i, offset, Buffer);//level
                                    offset += 4;
                                    Writer.Uint((uint)i, offset, Buffer);//online
                                    offset += 4;
                                    Writer.Uint((uint)i, offset, Buffer);//bp
                                    offset += 4;
                                  //  Writer.Uint((uint)Enums.GuildMemberRank.DeputyLeader, offset, Buffer);//unkown1
                                    offset += 4;
                                    Writer.String("Matrix-"+ i, offset, Buffer);
                                    offset += 16;                                   
                                }
                                client.Send(Buffer);
                                break;                                
                            }   
                        case "blue":
                            {
                                Attack attack = new Attack(true);
                                attack.Attacker = client.Screen.Objects.First().UID;
                                attack.Attacked = client.Entity.UID;
                                attack.X = client.Entity.X;
                                attack.Y = client.Entity.Y;
                                attack.Effect1 = Attack.AttackEffects1.None;
                                attack.Effect1 |= (Attack.AttackEffects1)byte.Parse(Data[1]);
                                attack.AttackType = Attack.Melee;
                                attack.Damage = 500;
                                client.Send(attack);
                                break;
                            }
                        case "xspell":
                            {
                                foreach (var skill in client.Spells.Values)
                                {
                                    Network.GamePackets.Data data = new Data(true);
                                    data.UID = client.Entity.UID;
                                    data.dwParam = client.Spells[skill.ID].ID;
                                    data.ID = 109;
                                    client.Send(data);
                                    var s = new Spell(true)
                                    {
                                        ID = client.Spells[skill.ID].ID,
                                        Level = client.Spells[skill.ID].Level,
                                        PreviousLevel = client.Spells[skill.ID].PreviousLevel,
                                        Experience = 0,
                                        Souls = Spell.Soul_Level.Level_Four,
                                        Available = true
                                    };
                                    skill.Souls = s.Souls;
                                  //  Writer.WriteByte(1, 24, s.ToArray());                                  
                                //    Writer.WriteByte(byte.Parse(Data[1]), byte.Parse(Data[2]), s.ToArray());
                                //    Writer.WriteByte(byte.Parse(Data[1]), 28, s.ToArray());

                                   //uint  _Levelhu = 4;
                                   // uint IntegerFlag = 0;
                                   // if (_Levelhu >= 1)
                                   //     IntegerFlag |= (uint)(1UL << 1);
                                   // if (_Levelhu >= 2)
                                   //     IntegerFlag |= (uint)(1UL << 4);
                                   // if (_Levelhu >= 3)
                                   //     IntegerFlag |= (uint)(1UL << 8);
                                   // if (_Levelhu >= 4)
                                   //     IntegerFlag |= (uint)(1UL << 16);
                                                                
                                    client.Send(s.ToArray());
                                   // client.Spells[skill.ID].Send(client);
                                   
                                }
                                break;
                            }
                        case "inbox2":
                            {
                                int count = int.Parse(Data[1]);
                               for (int i = 0; i < count; i++)
                                {
                                    MaTrix.Inbox.AddPrize(client, "Matrix" + i.ToString(), "Inbox Test" + i.ToString(), "Message" + i.ToString(), 5000000, 5000000, 600, /*p => { p.Entity.Level = 255; p.Entity.ConquerPoints = 0; }*/null);
                                 /*   MaTrix.Inbox.PrizeInfo prize = new MaTrix.Inbox.PrizeInfo()
                                    {
                                        ID = (uint)i,
                                        Time = 600,
                                        Sender = "Matrix" + i.ToString(),
                                        Subject = "Inbox Test" + i.ToString(),
                                        Message = "Message" + i.ToString(),
                                        MessageOrGift = true,
                                        itemprize = p => { p.Entity.Level = 255; p.Entity.ConquerPoints = 0; },
                                        goldprize = 5000000,
                                        cpsprize = 5000000
                                    };
                                    client.Prizes.Add(prize.ID, prize);*/
                                }
                                break;
                            }
                        case "inbox48":
                            {                               
                                string text = Mess.Remove(0, 7);
                                byte[] inbox = new byte[272];
                                Writer.Ushort((ushort)(inbox.Length - 8), 0, inbox);
                                Writer.Ushort(1048, 2, inbox);
                                Writer.Uint(0, 4, inbox);//id    
                                Writer.WriteString(text, 8, inbox);//string
                                client.Send(inbox);
                                break;
                            } 
                        case "inbox46":
                            {
                                uint count = uint.Parse(Data[1]);
                                byte[] inbox = new byte[20 + 92 * count];
                                Writer.Ushort((ushort)(inbox.Length - 8), 0, inbox);
                                Writer.Ushort(1046, 2, inbox);
                                Writer.Uint(count, 4, inbox);//count    
                                Writer.Uint(count, 12, inbox);//count                               
                                int offset = 16;
                                for (uint i = 0; i < count; i++)
                                {                                   
                                    Writer.Uint(i, offset, inbox);//uid 
                                    offset += 4;
                                    Writer.String("Sender Name", offset, inbox);//sender
                                    offset += 32;
                                    Writer.String("Subject", offset, inbox);//Subject
                                    offset += 40;
                                    Writer.Uint(600, offset, inbox);//Time
                                    offset += 12;
                                }
                                client.Send(inbox);
                                break;
                            } 
                        case "testxx":
                            {
                                client.Send(Network.GamePackets.Data.Custom(client.testxx++, client.Entity.UID));
                                client.Send(new Message(client.testxx.ToString(), Message.Tip));
                                break;
                            }
                        case "testxx2":
                            {
                                client.testxx = uint.Parse(Data[1]);
                                break;
                            }
                       
                        #region vend sys.
                        case "nvend":
                            {
                                switch (Data[1])
                                {

                                    case "add":
                                        {
                                            Game.ConquerStructures.Booth booth = new Game.ConquerStructures.Booth();
                                            SobNpcSpawn Base = new SobNpcSpawn();
                                            Base.UID = uint.Parse(Data[2]);
                                            if (Booth.Booths2.ContainsKey(Base.UID))
                                                Booth.Booths2.Remove(Base.UID);
                                            Booth.Booths2.Add(Base.UID, booth);
                                            Base.Mesh = 100;
                                            Base.Type = Game.Enums.NpcType.Booth;
                                            Base.ShowName = true;
                                            Base.Name = "matrix™[" + Base.UID.ToString()+"]";
                                            Base.MapID = client.Entity.MapID;
                                            Base.X = (ushort)(client.Entity.X + 1);
                                            Base.Y = client.Entity.Y;
                                            if (client.Map.Npcs.ContainsKey(Base.UID))
                                                client.Map.Npcs.Remove(Base.UID);
                                            client.Map.Npcs.Add(Base.UID, Base);
                                            client.SendScreenSpawn(Base, true);                                            
                                            break;
                                        }
                                    case "remove":
                                        {
                                            uint UID = uint.Parse(Data[2]);
                                            if (client.Map.Npcs.ContainsKey(UID))
                                                client.Map.Npcs.Remove(UID);
                                           
                                            client.Screen.FullWipe();
                                            client.Screen.Reload();                                            
                                            break;
                                        }                                    
                                    case "clear":
                                        {
                                            Game.ConquerStructures.Booth booth = null;                                          
                                            uint UID = uint.Parse(Data[2]);
                                            Booth.TryGetValue2(UID, out booth);
                                            if (booth == null) break;
                                            booth.ItemList.Clear();
                                            break;
                                        }
                                    case "additem":
                                        {
                                            Game.ConquerStructures.Booth booth = null;
                                            uint UID = uint.Parse(Data[2]);
                                            Booth.TryGetValue2(UID, out booth);
                                            if (booth == null) break;
                                          //  booth.ItemList.Clear();
                                            #region booth
                                            Game.ConquerStructures.BoothItem item = new Game.ConquerStructures.BoothItem();
                                            item.Cost = uint.Parse(Data[3]);
                                            item.Item = new ConquerItem(true);
                                            item.Item.ID = uint.Parse(Data[4]);
                                            item.Item.UID = Program.NextItemID;
                                            //Program.NextItemID++;
                                            if (Data.Length > 5)
                                            {
                                                item.Item.Plus = byte.Parse(Data[5]);
                                                if (Data.Length > 6)
                                                {
                                                    item.Item.Enchant = byte.Parse(Data[6]);
                                                    if (Data.Length > 7)
                                                    {
                                                        item.Item.Bless = byte.Parse(Data[7]);
                                                        if (Data.Length > 9)
                                                        {
                                                            item.Item.SocketOne = (Enums.Gem)byte.Parse(Data[8]);
                                                            item.Item.SocketTwo = (Enums.Gem)byte.Parse(Data[9]);
                                                        }
                                                    }
                                                }
                                            }
                                            Database.ConquerItemBaseInformation CIBI = null;
                                            CIBI = Database.ConquerItemInformation.BaseInformations[item.Item.ID];
                                            if (CIBI == null)
                                                break;
                                            item.Item.Durability = CIBI.Durability;
                                            item.Item.MaximDurability = CIBI.Durability;
                                            item.Cost_Type = Game.ConquerStructures.BoothItem.CostType.ConquerPoints;
                                            booth.ItemList.Add(item.Item.UID, item);
                                            #endregion
                                            break;
                                        }                                   
                                }
                                break;

                            }
                        #endregion
                    }
                    #endregion
                    return true;
                }
                return false;
            }
            catch (Exception e) { Console.WriteLine(e); client.Send(new Message("Impossible to handle this command. Check your syntax.", System.Drawing.Color.BurlyWood, Message.TopLeft)); return false; }
        }

       

        public uint testxx { get; set; }
        public uint testxx2 { get; set; }
        public ConquerItem spwansitem;
        public uint JoinToWar;
        public int oldnflag;
        public int oldflag2;
        public MaTrix.Lobby.MatchType MatchType;
        public uint PlayRouletteUID;
        public uint WatchRoulette;
        public bool _setlocation = true;
        public bool BlockTrade;
        public bool ChatBlock;



        
    }
}
