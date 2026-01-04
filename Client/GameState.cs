using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using MTA.Database;
using MTA.Franko;
using MTA.Game;
using MTA.Game.Attacking;
using MTA.Game.ConquerStructures;
using MTA.Game.Features.Guilds.Models;
using MTA.Game.ConquerStructures.Society;
using MTA.Game.Features;
using MTA.Game.Features.Tournaments;
using MTA.Interfaces;
using MTA.MaTrix;
using MTA.Network;
using MTA.Network.Cryptography;
using MTA.Network.GamePackets;
using MTA.Game.Constants;
using MTA.Game.Features.Guilds.Constants;
using MTA.Network.Sockets;
using BoothItem = MTA.Game.ConquerStructures.BoothItem;
using KnownPersons = MTA.Database.KnownPersons;
using Team = MTA.Game.ConquerStructures.Team;
using Trade = MTA.Network.GamePackets.Trade;
using TradePartner = MTA.Game.ConquerStructures.Society.TradePartner;
using Warehouse = MTA.Game.ConquerStructures.Warehouse;
using static MTA.Game.Constants.Items.BasicItems;
using Interaction = MTA.Network.GamePackets.Interaction;

namespace MTA.Client {
    public class GameState {
        public static Dictionary<uint, GameState> BoothingAI = new Dictionary<uint, GameState>();
        public bool _setlocation = true;
        private ClientWrapper _socket;

        public bool _voted;
        public AccountTable Account;
        public AI AI;
        public bool AllowedTreasurePoints;
        public int AllowedTreasurePointsIndex;
        public bool AlternateEquipment;
        public int apprtnum = 0;
        public int ArenaState = 0;

        public SafeDictionary<Update.AuraType, Auras> Auras =
            new SafeDictionary<Update.AuraType, Auras>(8);

        public bool BlockTrade;
        public Time32 bodeSHStamp;
        public Time32 CantAttack = Time32.Now;
        public bool ChallangeScoreStamp;
        public Challenge Challenge;
        public int ChallengeScore;
        public ChampionStatistic ChampionStats;
        public bool ChatBlock;

        public string Command = "";
        public GameCryptography Cryptography;
        public Time32 CTFUpdateStamp;
        public DHKeyExchange.ServerKeyExchange DHKeyExchange;
        public bool Effect2;
        public ElitePK.Match ElitePKMatch, WatchingElitePKMatch;
        public ElitePK.FighterStats ElitePKStats;

        public bool endarena = false;
        public bool endteam = false;
        public bool Exchange = true;
        public bool ExpectingQAnswer;


        public bool Fake;
        public bool FakeLoaded;
        public Time32 FakeQuit;
        public bool Filtering = false;
        public bool FTbode = false;
        public Interaction Interaction;
        public uint InteractionEffect;
        public ItemLock ItemUnlockPacket;
        public uint JoinToWar;
        public bool JustCreated = false;
        public bool JustOpenedDetain;
        public string KillCountCaptcha;
        public Time32 KillCountCaptchaStamp;
        public int KillerPoints;

        public Languages Language = Languages.English;

        public Time32 LastAttack, LastMove;
        public Time32 LastVIPTeleport, LastVIPTeamTeleport;

        public Lobby.QualifierGroup LobbyGroup;
        public GameState LobbyPlayWith;

        public bool LoggedIn;
        public Lobby.MatchType MatchType;
        public ConquerItemBaseInformation NewLookArmorInfo;
        public ConquerItemBaseInformation NewLookHeadgearInfo;
        public ConquerItemBaseInformation NewLookWeapon;
        public ConquerItemBaseInformation NewLookWeapon2;

        public string NewName = "";
        public uint NpcCpsInput;
        public int oldflag2;
        public int oldnflag;
        public Action<GameState> OnDisconnect;
        public PacketFilter PacketFilter;
        public Pet Pet;
        public int PKPoints;
        public uint PlayRouletteUID;

        public UsableRacePotion[] Potions;
        public Enums.PkMode PrevPK;

        public SafeDictionary<uint, Inbox.PrizeInfo> Prizes =
            new SafeDictionary<uint, Inbox.PrizeInfo>(1000);

        public ProgressBar ProgressBar;
        public string QAnswer;
        public Action<GameState> QCorrect;
        public int quarantineDeath;
        public int quarantineKill;
        public Quests Quests;
        public ConcurrentPacketQueue Queue;
        public QuizShow.QuizClient Quiz;
        public Action<GameState> QWrong;
        public KillTournament SelectionKillTournament;
        public bool SignedUpForEPK;
        public SlotMachine SlotMachine;
        public string SMCaptcha;
        public byte[] SMPacket;
        public int SMSpinCount;
        public SpiritBeadQuest SpiritBeadQ;
        public ConquerItem spwansitem;
        public Enums.Color staticArmorColor;
        public int TeamCheerFor;
        public Timer Timer;

        public DateTime timerattack = new DateTime();
        public IDisposable[]? TimerSubscriptions;
        public object TimerSyncRoot, ItemSyncRoot;
        public int TopDlClaim;
        public int TopGlClaim;
        public Trade TradePacket;
        public bool TransferedPlayer;
        public uint uniquepoints = 0;

        //public bool TeamAura;
        //public GameState TeamAuraOwner;
        //public ulong TeamAuraStatusFlag;
        //public uint TeamAuraPower;
        //public uint TeamAuraLevel;
        public VariableVault Variables;
        public bool VerifiedChallenge;
        public int VerifyChallengeCount;
        public bool WaitingItemUnlockPassword;
        public bool WaitingKillCaptcha;
        public bool WaitingTradePassword;
        public uint WatchRoulette;
        public Tuple<ConquerItem, ConquerItem> Weapons;

        public uint ClaimedElitePk {
            get { return this["ClaimedElitePk"]; }
            set { this["ClaimedElitePk"] = value; }
        }

        public uint ClaimedTeampk {
            get { return this["ClaimedTeampk"]; }
            set { this["ClaimedTeampk"] = value; }
        }

        public uint ClaimedSkillTeam {
            get { return this["ClaimedSkillTeam"]; }
            set { this["ClaimedSkillTeam"] = value; }
        }

        public uint SashSlots {
            get { return this["SashSlots"]; }
            set {
                this["SashSlots"] = value;
                if (Entity.EntityFlag == EntityFlag.Player) {
                    Entity.Update(Update.Sash, value, false);
                    Entity.Update(Update.AvailableSlots, 200, false);
                }
            }
        }

        public string Country { get; set; }

        public bool LobbySignup {
            get { return this["LobbySignup"]; }
            set { this["LobbySignup"] = value; }
        }

        public ushort SuperPotion {
            get { return this["SuperPotion"]; }
            set {
                this["SuperPotion"] = value;
                if (Entity is { FullyLoaded: true, EntityFlag: EntityFlag.Player }) {
                    if (this != null) {
                        Entity.Update(Update.DoubleExpTimer, Entity.DoubleExperienceTime, 500,
                            false);
                    }
                }
            }
        }

        public int PingCount { get; set; }

        public byte Claimeds {
            get { return this["Claimeds"]; }
            set { this["Claimeds"] = value; }
        }

        public bool StudyToday {
            get { return this["StudyToday"]; }
            set { this["StudyToday"] = value; }
        }

        public uint UsedCourses {
            get { return this["UsedCourses"]; }
            set { this["UsedCourses"] = value; }
        }

        public DateTime ResetUsedCourses {
            get { return this["ResetUsedCourses"]; }
            set { this["ResetUsedCourses"] = value; }
        }

        public bool JoinedDBMap {
            get { return this["JoinedDBMap"]; }
            set { this["JoinedDBMap"] = value; }
        }

        public DateTime inDBmap {
            get { return this["inDBmap"]; }
            set { this["inDBmap"] = value; }
        }

        public uint Appearance {
            get { return this["Appearance"]; }
            set { this["Appearance"] = value; }
        }

        public bool Voted {
            get { return _voted; }
            set {
                _voted = value;
                new MySqlCommand(MySqlCommandType.UPDATE)
                    .Update("entities").Set("VotePoint", value).Where("UID", Entity.UID).Execute();
            }
        }

        public DateTime VoteStamp {
            get { return this["VoteStamp"]; }
            set { this["VoteStamp"] = value; }
        }

        public uint namechanges {
            get { return this["namechanges"]; }
            set { this["namechanges"] = value; }
        }

        public DateTime matrixtime {
            get { return this["matrixtime"]; }
            set { this["matrixtime"] = value; }
        }

        public ulong Donationx {
            get { return this["Donationx"]; }
            set { this["Donationx"] = value; }
        }

        public bool OnDonation {
            get { return this["ondonation"]; }
            set { this["ondonation"] = value; }
        }

        public DynamicVariable this[string variable] {
            get { return Variables[variable]; }
            set { Variables[variable] = value; }
        }

        public uint CurrentHonor {
            get {
                if (ArenaStatistic == null) return 0;
                return ArenaStatistic.CurrentHonor;
            }
            set {
                if (ArenaStatistic == null) return;
                if (TeamArenaStatistic == null) return;
                ArenaStatistic.CurrentHonor =
                    TeamArenaStatistic.CurrentHonor =
                        value;
            }
        }

        public uint HistoryHonor {
            get { return ArenaStatistic.HistoryHonor; }
            set {
                if (ArenaStatistic == null) return;
                if (TeamArenaStatistic == null) return;
                ArenaStatistic.HistoryHonor =
                    TeamArenaStatistic.HistoryHonor =
                        value;
            }
        }

        public uint RacePoints {
            get { return this["racepoints"]; }
            set {
                this["racepoints"] = value;
                Entity.Update(Update.RaceShopPoints, value, false);
            }
        }

        public bool Online {
            get { return Socket.Connector != null; }
        }

        public bool InArenaMatch { get; set; }

        public uint testxx { get; set; }
        public uint testxx2 { get; set; }

        public void GetLanguages(string language) {
            switch (language) {
                case "En":
                    Language = Languages.English;
                    break;
            }
        }

        public string LanguageToString() {
            switch (Language) {
                case Languages.English:
                    return "en";
            }

            return "en";
        }

        public bool InWareHouse() {
            foreach (var wh in Warehouses.Values) {
                if (wh.Count > 0)
                    return true;
            }

            return false;
        }

        public void BlessTouch(GameState client) {
            if (!client.Spells.ContainsKey(12390))
                return;

            if (client.Weapons is { Item2: not null })
                if (client.Weapons.Item2.ID / 1000 != 619)
                    return;

            var spell2 = SpellTable.GetSpell(client.Spells[12390].ID, client.Spells[12390].Level);
            if (Kernel.Rate((double)spell2.Percent)) {
                var spell = SpellTable.GetSpell(1095, 4);
                Entity.AddFlag(Update.Flags.Stigma);
                Entity.StigmaStamp = Time32.Now;
                Entity.StigmaIncrease = spell.PowerPercent;
                Entity.StigmaTime = (byte)spell.Duration;
                if (Entity.EntityFlag == EntityFlag.Player)
                    Send(GameConstants.Stigma(spell.PowerPercent, spell.Duration));

                spell = SpellTable.GetSpell(1090, 4);
                Entity.ShieldTime = 0;
                Entity.ShieldStamp = Time32.Now;
                Entity.MagicShieldStamp = Time32.Now;
                Entity.MagicShieldTime = 0;

                Entity.AddFlag(Update.Flags.MagicShield);
                Entity.MagicShieldStamp = Time32.Now;
                Entity.MagicShieldIncrease = 1.1f; //spell.PowerPercent;
                Entity.MagicShieldTime = (byte)spell.Duration;
                if (Entity.EntityFlag == EntityFlag.Player)
                    Send(GameConstants.Shield(spell.PowerPercent, spell.Duration));

                spell = SpellTable.GetSpell(1085, 4);
                Entity.AccuracyStamp = Time32.Now;
                Entity.StarOfAccuracyStamp = Time32.Now;
                Entity.StarOfAccuracyTime = 0;
                Entity.AccuracyTime = 0;

                Entity.AddFlag(Update.Flags.StarOfAccuracy);
                Entity.StarOfAccuracyStamp = Time32.Now;
                Entity.StarOfAccuracyTime = (byte)spell.Duration;
                if (Entity.EntityFlag == EntityFlag.Player)
                    Send(GameConstants.Accuracy(spell.Duration));

                client.IncreaseSpellExperience(100, 12390);
            }
        }

        public void BreakTouch(GameState client) {
            if (!client.Spells.ContainsKey(12400))
                return;

            if (client.Weapons is { Item2: not null })
                if (client.Weapons.Item2.ID / 1000 != 619)
                    return;

            var spell = SpellTable.GetSpell(client.Spells[12400].ID, client.Spells[12400].Level);
            if (MyMath.Success(30)) {
                if (Entity.ContainsFlag3(Update.Flags3.lianhuaran04)) {
                    SpellUse suse = new SpellUse(true);
                    suse.Attacker = Entity.UID;
                    suse.SpellID = spell.ID;
                    suse.SpellLevel = spell.Level;

                    var array = Handle.PlayerinRange(Entity, Entity).ToArray();
                    foreach (var target in array) {
                        var attacked = target.Entity;
                        if (attacked.UID == client.Entity.UID)
                            continue;
                        if (Handle.CanAttack(client.Entity, attacked, spell, true)) {
                            var attack = new Attack(true);
                            attack.Attacker = client.Entity.UID;
                            attack.Attacked = attacked.UID;

                            uint damage = Calculate.Magic(client.Entity, attacked, ref attack);

                            attack.Damage = damage;
                            suse.Effect1 = attack.Effect1;
                            suse.Effect1 = attack.Effect1;

                            Handle.ReceiveAttack(client.Entity, attacked, attack, ref damage, spell);
                            suse.AddTarget(attacked, damage, attack);
                        }
                    }

                    client.SendScreen(suse);

                    Entity.RemoveFlag3(Update.Flags3.lianhuaran01);
                    Entity.RemoveFlag3(Update.Flags3.lianhuaran02);
                    Entity.RemoveFlag3(Update.Flags3.lianhuaran03);
                    Entity.RemoveFlag3(Update.Flags3.lianhuaran04);
                }
                else if (Entity.ContainsFlag3(Update.Flags3.lianhuaran03)) {
                    Entity.AddFlag3(Update.Flags3.lianhuaran04);
                    Entity.RemoveFlag3(Update.Flags3.lianhuaran03);
                    Entity.lianhuaranPercent = 0.5f;
                }
                else if (Entity.ContainsFlag3(Update.Flags3.lianhuaran02)) {
                    Entity.AddFlag3(Update.Flags3.lianhuaran03);
                    Entity.RemoveFlag3(Update.Flags3.lianhuaran02);
                    Entity.lianhuaranPercent = 0.4f;
                }
                else if (Entity.ContainsFlag3(Update.Flags3.lianhuaran01)) {
                    Entity.AddFlag3(Update.Flags3.lianhuaran02);
                    Entity.RemoveFlag3(Update.Flags3.lianhuaran01);
                    Entity.lianhuaranPercent = 0.3f;
                }
                else if (!Entity.ContainsFlag3(Update.Flags3.lianhuaran01)) {
                    Entity.AddFlag3(Update.Flags3.lianhuaran01);
                    Entity.lianhuaranPercent = 0.1f;
                }

                Entity.lianhuaranStamp = Time32.Now;
                Entity.lianhuaranLeft = 20;

                client.IncreaseSpellExperience(100, 12400);
            }
        }

        public GameState[] MonksInTeam() {
            return Team.Teammates.Where(x => x.Entity.Aura_isActive).ToArray();
        }

        public void CheckTeamAura() {
            if (Team != null) {
                var monks = MonksInTeam();
                if (monks != null) {
                    foreach (var monk in monks) {
                        Update.AuraType aura = Update.AuraType.TyrantAura;
                        switch (monk.Entity.Aura_actType) {
                            case Update.Flags2.EarthAura: aura = Update.AuraType.EarthAura; break;
                            case Update.Flags2.FireAura: aura = Update.AuraType.FireAura; break;
                            case Update.Flags2.WaterAura: aura = Update.AuraType.WaterAura; break;
                            case Update.Flags2.WoodAura: aura = Update.AuraType.WoodAura; break;
                            case Update.Flags2.MetalAura: aura = Update.AuraType.MetalAura; break;
                            case Update.Flags2.FendAura: aura = Update.AuraType.FendAura; break;
                            case Update.Flags2.TyrantAura: aura = Update.AuraType.TyrantAura; break;
                        }

                        if (!Auras.ContainsKey(aura)) {
                            if (Entity.UID != monk.Entity.UID &&
                                Kernel.GetDistance(Entity.X, Entity.Y, monk.Entity.X, monk.Entity.Y) <=
                                GameConstants.playerViewRange) {
                                Auras Aura = new Auras();
                                Aura.TeamAuraOwner = monk;
                                Aura.TeamAuraStatusFlag = monk.Entity.Aura_actType;
                                Aura.TeamAuraPower = monk.Entity.Aura_actPower;
                                Aura.TeamAuraLevel = monk.Entity.Aura_actLevel;
                                Aura.aura = aura;
                                if (!Auras.ContainsKey(Aura.aura)) {
                                    Auras.Add(Aura.aura, Aura);
                                    Entity.AddFlag2(Aura.TeamAuraStatusFlag);
                                    new Update(true).Aura(Entity, Update.AuraDataTypes.Add, aura,
                                        Aura.TeamAuraLevel, Aura.TeamAuraPower);
                                    doAuraBonuses(Aura.TeamAuraStatusFlag, Aura.TeamAuraPower, 1);
                                }
                            }
                        }
                    }
                }
            }

            foreach (var Aura in Auras.Values.ToArray()) {
                var pthis = Aura.TeamAuraOwner;
                if (pthis == null) {
                    new Update(true).Aura(Entity, Update.AuraDataTypes.Remove, Aura.aura, Aura.TeamAuraLevel,
                        Aura.TeamAuraPower);
                    //this.removeAuraBonuses(this.TeamAuraStatusFlag, this.TeamAuraPower, 1);
                    removeAuraBonuses(Aura.TeamAuraStatusFlag, Aura.TeamAuraPower, 1);
                    Entity.RemoveFlag2(Aura.TeamAuraStatusFlag);
                    Auras.Remove(Aura.aura);
                }
                else {
                    if (!pthis.Entity.Aura_isActive || !pthis.Socket.Alive || pthis.Entity.Dead ||
                        pthis.Entity.MapID != Entity.MapID ||
                        pthis.Entity.Aura_actType != Aura.TeamAuraStatusFlag) {
                        new Update(true).Aura(Entity, Update.AuraDataTypes.Remove, Aura.aura, Aura.TeamAuraLevel,
                            Aura.TeamAuraPower);
                        //this.removeAuraBonuses(this.TeamAuraStatusFlag, this.TeamAuraPower, 1);
                        removeAuraBonuses(Aura.TeamAuraStatusFlag, Aura.TeamAuraPower, 1);
                        Entity.RemoveFlag2(Aura.TeamAuraStatusFlag);
                        Auras.Remove(Aura.aura);
                    }
                    else {
                        if (Team == null ||
                            (pthis.Team == null || (pthis.Team != null && !pthis.Team.IsTeammate(Entity.UID))) ||
                            Entity.Dead ||
                            Kernel.GetDistance(Entity.X, Entity.Y, pthis.Entity.X, pthis.Entity.Y) >
                            GameConstants.playerViewRange) {
                            new Update(true).Aura(Entity, Update.AuraDataTypes.Remove, Aura.aura,
                                Aura.TeamAuraLevel, Aura.TeamAuraPower);
                            removeAuraBonuses(Aura.TeamAuraStatusFlag, Aura.TeamAuraPower, 1);
                            Entity.RemoveFlag2(Aura.TeamAuraStatusFlag);
                            Auras.Remove(Aura.aura);
                        }
                    }
                }
            }
        }

        public void ChangeName(GameState client) {
            client.OnDisconnect = p => {
                #region ChangeName progress

                string name200 = p.Entity.Name;
                string newname = p.NewName;
                uint uid = p.Entity.UID;
                if (newname != "") {
                    Console.WriteLine("Change Name In Progress");
                    if (newname != "") {
                        MySqlCommand cmdupdate = null;
                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("guilds").Set("LeaderName", newname).Where("LeaderName", name200).Execute();

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);

                        if (p.Entity.MyFlowers != null)
                            p.Entity.MyFlowers.Name = newname;

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("partners").Set("PartnerName", newname).Where("PartnerID", uid).Execute();

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("teamarena").Set("EntityName", newname).Where("EntityID", uid).Execute();

                        cmdupdate = new MySqlCommand(MySqlCommandType.UPDATE);
                        cmdupdate.Update("entities").Set("name", newname).Set("namechange", "").Where("UID", uid)
                            .Execute();
                        Console.WriteLine(" -[" + name200 + "] : -[" + newname + "]");


                        if (Nobility.Board.ContainsKey(p.Entity.UID)) {
                            Nobility.Board[p.Entity.UID].Name = p.NewName;
                        }

                        if (Arena.ArenaStatistics.ContainsKey(p.Entity.UID)) {
                            Arena.ArenaStatistics[p.Entity.UID].Name = p.NewName;
                        }

                        if (p.Entity.GetClan != null) {
                            if (p.Entity.GetClan.LeaderName == name200) {
                                Kernel.Clans[p.Entity.ClanId].LeaderName = p.NewName;
                            }

                            Kernel.Clans[p.Entity.ClanId].Members[p.Entity.UID].Name = p.NewName;
                        }

                        if (p.Guild != null) {
                            if (p.Guild.LeaderName == name200) {
                                Kernel.Guilds[p.Guild.Id].LeaderName = p.NewName;
                            }

                            Kernel.Guilds[p.Guild.Id].Members[p.Entity.UID].Name = p.NewName;
                        }
                    }
                }

                #endregion ChangeName progressa
            };
            client.Disconnect();
        }

        public bool IsWatching() {
            return WatchingGroup != null || TeamWatchingGroup != null;
        }

        public bool InQualifier() {
            bool inteam = false;
            if (Team is { EliteFighterStats: not null }) inteam = true;

            return QualifierGroup != null || TeamQualifierGroup != null || LobbyGroup != null || inteam;
        }

        public bool InArenaQualifier() {
            return QualifierGroup != null;
        }

        public bool InTeamQualifier() {
            bool inteam = false;
            if (Team is { EliteMatch.Map: not null })
                if (Team.EliteMatch.Map.ID == Entity.MapID)
                    inteam = true;

            return TeamQualifierGroup != null || inteam;
        }

        public Time32 ImportTime() {
            if (QualifierGroup != null)
                return QualifierGroup.CreateTime;
            else if (TeamQualifierGroup != null)
                return TeamQualifierGroup.ImportTime;
            else if (LobbyGroup != null)
                return LobbyGroup.ImportTime;
            if (Team is { EliteMatch: not null }) return Team.EliteMatch.ImportTime;

            return Time32.Now;
        }

        // public void UpdateQualifier( long damage, bool toxicfog = false)
        public void UpdateQualifier(GameState client, GameState target, long damage, bool toxicfog = false) {
            if (LobbyGroup != null) {
                LobbyGroup.UpdateDamage(LobbyGroup.OppositeClient(this), (uint)damage);
            }
            else if (ChampionGroup != null) {
                ChampionGroup.UpdateDamage(ChampionGroup.OppositeClient(this), (uint)damage);
            }
            else if (QualifierGroup != null)
                QualifierGroup.UpdateDamage(client, (uint)damage);
            else if (TeamQualifierGroup != null) {
                if (client == null)
                    TeamQualifierGroup.UpdateDamage(target, (uint)damage, true);
                else
                    TeamQualifierGroup.UpdateDamage(client, (uint)damage);
            }
            else if (toxicfog) {
                if (ElitePKMatch != null) {
                    var opponent = ElitePKMatch.targetOf(this);
                    if (opponent != null)
                        opponent.ElitePKStats.Points += (uint)damage;
                    ElitePKMatch.Update();
                }
                else if (Team is { EliteMatch: not null }) {
                    var opponent = Team.EliteMatch.targetOfWin(Team);
                    if (opponent != null) {
                        opponent.Points += (uint)damage;
                        opponent.Team.SendMesageTeam(opponent.Team.EliteMatch.CreateUpdate().ToArray(), 0);
                    }

                    Team.SendMesageTeam(Team.EliteMatch.CreateUpdate().ToArray(), 0);
                }
            }
        }

        internal void EndQualifier() {
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

        internal void Send(string msg, uint type = Message.Talk) {
            Send(new Message(msg, type));
        }

        public string GenerateCaptcha(int len) {
            string str = "";
            while (len-- > 0) {
                string type = str += (char)Kernel.Random.Next('0', '9');
                /*int type = Kernel.Random.Next(0, 3);
                if (type == 0) str += (char)Kernel.Random.Next('0', '9');
                else if (type == 1) str += (char)Kernel.Random.Next('a', 'z');
                else str += (char)Kernel.Random.Next('A', 'Z');*/
            }

            return str;
        }

        public void MessageBox(string text, Action<GameState>? msg_ok = null, Action<GameState>? msg_cancel = null,
            uint time = 0, Languages language = Languages.English, bool force = false) {
            if (!force) {
                if (Entity.MapID == 6000 || Entity.MapID == 6001 || Entity.MapID == 6002 ||
                    Entity.MapID == 6003 || Entity.MapID == 6004 || Entity.MapID == 1038 ||
                    Entity.PokerTableUID > 0 || Entity.InJail() ||
                    PlayRouletteUID > 0) return;
            }

            if (InQualifier() || Challenge is { Inside: true })
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

        public void Time(uint time) {
            Send(new Data(true) { UID = Entity.UID, dwParam = time, ID = Data.CountDown });
        }

        internal void LoadData(bool loadFake = false) {
            //    Database.KissSystemTable.Kisses(this);
            PkExpelTable.Load(this);
            ConquerItemTable.LoadItems(this);
            //    Database.FlowerSystemTable.Flowers(this);

            if (!loadFake) {
                ClaimItemTable.LoadClaimableItems(this);
                DetainedItemTable.LoadDetainedItems(this);
            }
            else {
                ClaimableItem = new SafeDictionary<uint, DetainedItem>();
                DeatinedItem = new SafeDictionary<uint, DetainedItem>();
            }

            SubClassTable.Load(Entity);
            if (!loadFake) {
                using (var conn = DataHolder.MySqlConnection) {
                    conn.Open();
                    SkillTable.LoadProficiencies(this);
                    SkillTable.LoadSpells(this);
                }

                KnownPersons.LoadPartner(this);
                KnownPersons.LoadEnemy(this);
                KnownPersons.LoaderFriends(this);
                KnownPersons.LoadMentor(this);
            }
            else {
                Spells = new SafeDictionary<ushort, ISkill>();
                Proficiencies = new SafeDictionary<ushort, IProf>();
                Partners = new SafeDictionary<uint, TradePartner>();
                Enemy = new SafeDictionary<uint, Enemy>();
                Friends = new SafeDictionary<uint, Friend>();
                Apprentices = new SafeDictionary<uint, Apprentice>();
            }

            ChiTable.Load(this);
            Inbox.Load(this);

            Quests.Load();

            //  Database.BigBOSRewardDataBase.LoadReward(this);
        }

        public void FakeLoad(uint UID, bool enterserver = true) {
            if (!Kernel.GamePool.ContainsKey(UID)) {
                ReadyToPlay();
                Account = new AccountTable(null);
                Account.EntityID = UID;
                if (EntityTable.LoadEntity(this)) {
                    if (Entity.FullyLoaded) {
                        VariableVault variables;
                        EntityVariableTable.Load(Entity.UID, out variables);
                        Variables = variables;

                        if (BackupArmorLook != 0)
                            SetNewArmorLook(BackupArmorLook);
                        else
                            SetNewArmorLook(ArmorLook);
                        SetNewHeadgearLook(HeadgearLook);
                        BackupArmorLook = 0;

                        LoadData(enterserver);

                        if (Entity.GuildID != 0)
                            Entity.GuildBattlePower = Guild.GetSharedBattlePower((MemberRank)Entity.GuildRank);

                        ReviewMentor();

                        Entity.NobilityRank = NobilityInformation.Rank;

                        if (enterserver) {
                            PacketHandler.LoginMessages(this);

                            Program.World.Register(this);
                            Kernel.GamePool.Add(Entity.UID, this);
                        }

                        FakeLoaded = true;
                    }
                }
            }
        }

        public void FakeLoad2(uint UID, string Name = "") {
            if (Name == "")
                Name = "MaTrix[" + UID + "]";
            if (!Kernel.GamePool.ContainsKey(UID)) {
                ReadyToPlay();
                Account = new AccountTable(null);
                Account.EntityID = UID;
                Entity = new Entity(EntityFlag.Player, false);
                Entity.Owner = this;
                Entity.Name = Name;
                Entity.UID = UID;
                Entity.Vitality = 537;
                Entity.Face = 37;
                Entity.Body = 1003;
                Entity.HairStyle = 630;
                Entity.Level = 140;
                Entity.Class = 15;
                Entity.Reborn = 2;
                Entity.MaxHitpoints = 20000;
                Entity.Hitpoints = Entity.MaxHitpoints;
                Entity.Mana = 800;

                Variables = new VariableVault();
                Friends = new SafeDictionary<uint, Friend>();
                Enemy = new SafeDictionary<uint, Enemy>();
                ChiData = new ChiTable.ChiData();
                ChiPowers = [];


                NobilityInformation = new NobilityInformation();
                NobilityInformation.EntityUID = Entity.UID;
                NobilityInformation.Name = Entity.Name;
                NobilityInformation.Donation = 0;
                NobilityInformation.Rank = NobilityRank.Serf;
                NobilityInformation.Position = -1;
                NobilityInformation.Gender = 1;
                NobilityInformation.Mesh = Entity.Mesh;
                if (Entity.Body % 10 >= 3)
                    NobilityInformation.Gender = 0;

                TeamArenaStatistic = new TeamArenaStatistic(true);
                TeamArenaStatistic.EntityID = Entity.UID;
                TeamArenaStatistic.Name = Entity.Name;
                TeamArenaStatistic.Level = Entity.Level;
                TeamArenaStatistic.Class = Entity.Class;
                TeamArenaStatistic.Model = Entity.Mesh;
                TeamArenaStatistic.Status = TeamArenaStatistic.NotSignedUp;

                ArenaStatistic = new ArenaStatistic(true);
                ArenaStatistic.EntityID = Entity.UID;
                ArenaStatistic.Name = Entity.Name;
                ArenaStatistic.Level = Entity.Level;
                ArenaStatistic.Class = Entity.Class;
                ArenaStatistic.Model = Entity.Mesh;
                ArenaPoints = ArenaTable.ArenaPointFill(Entity.Level);
                ArenaStatistic.LastArenaPointFill = DateTime.Now;
                ArenaStatistic.Status = ArenaStatistic.NotSignedUp;

                ChampionStats = new ChampionStatistic(true);
                ChampionStats.UID = Entity.UID;
                ChampionStats.Name = Entity.Name;
                ChampionStats.Level = Entity.Level;
                ChampionStats.Class = Entity.Class;
                ChampionStats.Model = Entity.Mesh;
                ChampionStats.Points = 0;
                ChampionStats.LastReset = DateTime.Now;
                ChiPowers = [];
                Retretead_ChiPowers = new ChiPowerStructure[4];
                ChiData = new ChiTable.ChiData()
                    { Name = Entity.Name, UID = Entity.UID, Powers = ChiPowers };

                Entity.Stamina = 150;

                Spells = new SafeDictionary<ushort, ISkill>();
                Proficiencies = new SafeDictionary<ushort, IProf>();

                PacketHandler.LoginMessages(this);

                Program.World.Register(this);
                Kernel.GamePool.Add(Entity.UID, this);
            }
        }

        public void Question(string question, uint answer) {
            Npcs dialog = new Npcs(this);
            ActiveNpc = 9999990;
            QAnswer = answer.ToString();
            ExpectingQAnswer = true;
            dialog.Text(question);
            dialog.Input("Answer:", 1, (byte)QAnswer.Length);
            dialog.Option("No thank you.", 255);
            dialog.Send();
        }

        public void FakeLoadx(uint UID) {
            if (!Kernel.GamePool.ContainsKey(UID)) {
                ReadyToPlay();
                Account = new AccountTable(null);
                Account.EntityID = UID;
                //   if (Database.EntityTable.LoadEntity(this))
                {
                    #region Load Entity

                    MySqlCommand command = new MySqlCommand(MySqlCommandType.SELECT);
                    command.Select("bots").Where("BotID", UID);
                    MySqlReader reader = new MySqlReader(command);
                    if (!reader.Read()) {
                        return;
                    }

                    Entity = new Entity(EntityFlag.Player, false);
                    Entity.Name = reader.ReadString("BotName");
                    Entity.Owner = this;
                    Entity.UID = UID;
                    Entity.Body = reader.ReadUInt16("BotBody");
                    Entity.Face = reader.ReadUInt16("BotFace");
                    Entity.HairStyle = reader.ReadUInt16("BotHairStyle");
                    Entity.Level = reader.ReadByte("BotLevel");
                    Entity.Class = reader.ReadByte("BotClass");
                    Entity.Reborn = reader.ReadByte("BotReborns");
                    Entity.Titles =
                        new ConcurrentDictionary<TitlePacket.Titles, DateTime>();
                    Entity.MyTitle = (TitlePacket.Titles)reader.ReadUInt32("BotTitle");
                    Entity.MapID = reader.ReadUInt16("BotMap");
                    if (VendingDisguise == 0)
                        VendingDisguise = 0xdf;
                    Entity.X = reader.ReadUInt16("BotMapx");
                    Entity.Y = reader.ReadUInt16("BotMapy");
                    uint WeaponR = reader.ReadUInt32("BotWeaponR");
                    uint WeaponL = reader.ReadUInt32("BotWeaponL");
                    uint Armor = reader.ReadUInt32("BotArmor");
                    uint Head = reader.ReadUInt32("BotHead");
                    uint Garment = reader.ReadUInt32("BotGarment");

                    string hawkmessage = reader.ReadString("BotMessage");
                    Entity.MyAchievement = new Achievement(Entity);

                    int count = reader.ReadInt32("BItemCount");
                    string[] itemCost = reader.ReadString("BItemCost").Split(["~", "@@", " "],
                        StringSplitOptions.RemoveEmptyEntries);
                    string[] itemID = reader.ReadString("BItemID").Split(["~", "@@", " "],
                        StringSplitOptions.RemoveEmptyEntries);
                    string[] itemPlus = reader.ReadString("BItemPlus").Split(["~", "@@", " "],
                        StringSplitOptions.RemoveEmptyEntries);
                    string[] itemEnchant = reader.ReadString("BItemEnchant").Split(["~", "@@", " "],
                        StringSplitOptions.RemoveEmptyEntries);
                    string[] itemBless = reader.ReadString("BItemBless").Split(["~", "@@", " "],
                        StringSplitOptions.RemoveEmptyEntries);
                    string[] itemSocketOne = reader.ReadString("BItemSoc1").Split(["~", "@@", " "],
                        StringSplitOptions.RemoveEmptyEntries);
                    string[] itemSocketTwo = reader.ReadString("BItemSoc2").Split(["~", "@@", " "],
                        StringSplitOptions.RemoveEmptyEntries);

                    ElitePKStats = new ElitePK.FighterStats(Entity.UID, Entity.Name, Entity.Mesh);
                    if (!Nobility.Board.TryGetValue(Entity.UID,
                            out NobilityInformation)) {
                        NobilityInformation = new NobilityInformation();
                        NobilityInformation.EntityUID = Entity.UID;
                        NobilityInformation.Name = Entity.Name;
                        NobilityInformation.Donation = 0L;
                        NobilityInformation.Rank = NobilityRank.Serf;
                        NobilityInformation.Position = -1;
                        NobilityInformation.Gender = 1;
                        NobilityInformation.Mesh = Entity.Mesh;
                        if ((Entity.Body % 10) >= 3) {
                            NobilityInformation.Gender = 0;
                        }
                    }
                    else {
                        Entity.NobilityRank = NobilityInformation.Rank;
                    }

                    Arena.ArenaStatistics.TryGetValue(Entity.UID, out ArenaStatistic);
                    if ((ArenaStatistic == null) || (ArenaStatistic.EntityID == 0)) {
                        ArenaStatistic = new ArenaStatistic(true);
                        ArenaStatistic.EntityID = Entity.UID;
                        ArenaStatistic.Name = Entity.Name;
                        ArenaStatistic.Level = Entity.Level;
                        ArenaStatistic.Class = Entity.Class;
                        ArenaStatistic.Model = Entity.Mesh;
                        ArenaStatistic.ArenaPoints = ArenaTable.ArenaPointFill(Entity.Level);
                        ArenaStatistic.LastArenaPointFill = DateTime.Now;
                        ArenaTable.InsertArenaStatistic(this);
                        ArenaStatistic.Status = 0;
                        Arena.ArenaStatistics.Add(Entity.UID, ArenaStatistic);
                    }
                    else {
                        ArenaStatistic.Level = Entity.Level;
                        ArenaStatistic.Class = Entity.Class;
                        ArenaStatistic.Model = Entity.Mesh;
                        if (DateTime.Now.DayOfYear != ArenaStatistic.LastArenaPointFill.DayOfYear) {
                            ArenaStatistic.LastSeasonArenaPoints = ArenaStatistic.ArenaPoints;
                            ArenaStatistic.LastSeasonWin = ArenaStatistic.TodayWin;
                            ArenaStatistic.LastSeasonLose =
                                ArenaStatistic.TodayBattles - ArenaStatistic.TodayWin;
                            ArenaStatistic.ArenaPoints = ArenaTable.ArenaPointFill(Entity.Level);
                            ArenaStatistic.LastArenaPointFill = DateTime.Now;
                            ArenaStatistic.TodayWin = 0;
                            ArenaStatistic.TodayBattles = 0;
                            Arena.Sort();
                            Arena.YesterdaySort();
                        }
                    }

                    TeamArena.ArenaStatistics.TryGetValue(Entity.UID, out TeamArenaStatistic);
                    if (TeamArenaStatistic == null) {
                        TeamArenaStatistic = new TeamArenaStatistic(true);
                        TeamArenaStatistic.EntityID = Entity.UID;
                        TeamArenaStatistic.Name = Entity.Name;
                        TeamArenaStatistic.Level = Entity.Level;
                        TeamArenaStatistic.Class = Entity.Class;
                        TeamArenaStatistic.Model = Entity.Mesh;
                        TeamArenaTable.InsertArenaStatistic(this);
                        TeamArenaStatistic.Status = 0;
                        if (TeamArena.ArenaStatistics.ContainsKey(Entity.UID)) {
                            TeamArena.ArenaStatistics.Remove(Entity.UID);
                        }

                        TeamArena.ArenaStatistics.Add(Entity.UID, TeamArenaStatistic);
                    }
                    else if (TeamArenaStatistic.EntityID == 0) {
                        TeamArenaStatistic = new TeamArenaStatistic(true);
                        TeamArenaStatistic.EntityID = Entity.UID;
                        TeamArenaStatistic.Name = Entity.Name;
                        TeamArenaStatistic.Level = Entity.Level;
                        TeamArenaStatistic.Class = Entity.Class;
                        TeamArenaStatistic.Model = Entity.Mesh;
                        TeamArenaTable.InsertArenaStatistic(this);
                        TeamArenaStatistic.Status = 0;
                        if (TeamArena.ArenaStatistics.ContainsKey(Entity.UID)) {
                            TeamArena.ArenaStatistics.Remove(Entity.UID);
                        }

                        TeamArena.ArenaStatistics.Add(Entity.UID, TeamArenaStatistic);
                    }
                    else {
                        TeamArenaStatistic.Level = Entity.Level;
                        TeamArenaStatistic.Class = Entity.Class;
                        TeamArenaStatistic.Model = Entity.Mesh;
                        TeamArenaStatistic.Name = Entity.Name;
                    }

                    #region Champion

                    Champion.ChampionStats.TryGetValue(Entity.UID, out ChampionStats);
                    if (ChampionStats == null) {
                        ChampionStats = new ChampionStatistic(true);
                        ChampionStats.UID = Entity.UID;
                        ChampionStats.Name = Entity.Name;
                        ChampionStats.Level = Entity.Level;
                        ChampionStats.Class = Entity.Class;
                        ChampionStats.Model = Entity.Mesh;
                        ChampionStats.Points = 0;
                        ChampionStats.LastReset = DateTime.Now;
                        ChampionTable.InsertStatistic(this);
                        if (Champion.ChampionStats.ContainsKey(Entity.UID))
                            Champion.ChampionStats.Remove(Entity.UID);
                        Champion.ChampionStats.Add(Entity.UID, ChampionStats);
                    }
                    else if (ChampionStats.UID == 0) {
                        ChampionStats = new ChampionStatistic(true);
                        ChampionStats.UID = Entity.UID;
                        ChampionStats.Name = Entity.Name;
                        ChampionStats.Level = Entity.Level;
                        ChampionStats.Class = Entity.Class;
                        ChampionStats.Model = Entity.Mesh;
                        ChampionStats.Points = 0;
                        ChampionStats.LastReset = DateTime.Now;
                        ArenaTable.InsertArenaStatistic(this);
                        ArenaStatistic.Status = ArenaStatistic.NotSignedUp;
                        if (Champion.ChampionStats.ContainsKey(Entity.UID))
                            Champion.ChampionStats.Remove(Entity.UID);
                        Champion.ChampionStats.Add(Entity.UID, ChampionStats);
                    }
                    else {
                        ChampionStats.Level = Entity.Level;
                        ChampionStats.Class = Entity.Class;
                        ChampionStats.Model = Entity.Mesh;
                        ChampionStats.Name = Entity.Name;
                        if (ChampionStats.LastReset.DayOfYear != DateTime.Now.DayOfYear)
                            ChampionTable.Reset(ChampionStats);
                    }

                    Champion.Clear(this);

                    #endregion

                    DetainedItemTable.LoadDetainedItems(this);
                    ClaimItemTable.LoadClaimableItems(this);
                    Entity.LoadTopStatus();
                    Entity.FullyLoaded = true;

                    #endregion

                    if (Entity.FullyLoaded) {
                        VariableVault variables;
                        EntityVariableTable.Load(Entity.UID, out variables);
                        Variables = variables;

                        if (BackupArmorLook != 0)
                            SetNewArmorLook(BackupArmorLook);
                        else
                            SetNewArmorLook(ArmorLook);
                        SetNewHeadgearLook(HeadgearLook);
                        BackupArmorLook = 0;

                        LoadData(true);

                        if (Entity.GuildID != 0)
                            Entity.GuildBattlePower = Guild.GetSharedBattlePower((MemberRank)Entity.GuildRank);

                        ReviewMentor();


                        PacketHandler.LoginMessages(this);

                        #region Equip

                        ConquerItem item7 = null;
                        ClientEquip equip = null;
                        if (WeaponR > 0) {
                            ConquerItemBaseInformation CIBI =
                                ConquerItemInformation.BaseInformations[WeaponR];
                            if (CIBI == null) return;
                            item7 = new ConquerItem(true);
                            item7.ID = WeaponR;
                            item7.UID = Program.NextItemId;
                            //Program.NextItemID++;
                            item7.Position = 4;
                            item7.Durability = CIBI.Durability;
                            item7.MaximDurability = CIBI.Durability;
                            Equipment.Remove(4);
                            if (Equipment.Objects[3] != null) {
                                Equipment.Objects[3] = null;
                            }

                            Equipment.Add(item7);
                            item7.Mode = Enums.ItemMode.Update;
                            item7.Send(this);
                            equip = new ClientEquip();
                            equip.DoEquips(this);
                            Send(equip);
                            Equipment.UpdateEntityPacket();
                        }

                        if (WeaponL > 0) {
                            ConquerItemBaseInformation CIBI =
                                ConquerItemInformation.BaseInformations[WeaponL];
                            if (CIBI == null) return;
                            item7 = new ConquerItem(true);
                            item7.ID = WeaponL;
                            item7.UID = Program.NextItemId;
                            //Program.NextItemID++;
                            item7.Position = 5;
                            item7.Durability = CIBI.Durability;
                            item7.MaximDurability = CIBI.Durability;
                            Equipment.Remove(5);
                            if (Equipment.Objects[4] != null) {
                                Equipment.Objects[4] = null;
                            }

                            Equipment.Add(item7);
                            item7.Mode = Enums.ItemMode.Update;
                            item7.Send(this);
                            equip = new ClientEquip();
                            equip.DoEquips(this);
                            Send(equip);
                            Equipment.UpdateEntityPacket();
                        }

                        if (Armor > 0) {
                            ConquerItemBaseInformation CIBI =
                                ConquerItemInformation.BaseInformations[Armor];
                            if (CIBI == null) return;
                            item7 = new ConquerItem(true);
                            item7.ID = Armor;
                            item7.UID = Program.NextItemId;
                            //Program.NextItemID++;
                            item7.Position = 3;
                            item7.Durability = CIBI.Durability;
                            item7.MaximDurability = CIBI.Durability;
                            Equipment.Remove(3);
                            if (Equipment.Objects[2] != null) {
                                Equipment.Objects[2] = null;
                            }

                            Equipment.Add(item7);
                            item7.Mode = Enums.ItemMode.Update;
                            item7.Send(this);
                            equip = new ClientEquip();
                            equip.DoEquips(this);
                            Send(equip);
                            Equipment.UpdateEntityPacket();
                        }

                        if (Head > 0) {
                            ConquerItemBaseInformation CIBI =
                                ConquerItemInformation.BaseInformations[Head];
                            if (CIBI == null) return;
                            item7 = new ConquerItem(true);
                            item7.ID = Head;
                            item7.UID = Program.NextItemId;
                            //Program.NextItemID++;
                            item7.Position = 1;
                            item7.Durability = CIBI.Durability;
                            item7.MaximDurability = CIBI.Durability;
                            Equipment.Remove(1);
                            if (Equipment.Objects[0] != null) {
                                Equipment.Objects[0] = null;
                            }

                            Equipment.Add(item7);
                            item7.Mode = Enums.ItemMode.Update;
                            item7.Send(this);
                            equip = new ClientEquip();
                            equip.DoEquips(this);
                            Send(equip);
                            Equipment.UpdateEntityPacket();
                        }

                        if (Garment > 0) {
                            ConquerItemBaseInformation CIBI =
                                ConquerItemInformation.BaseInformations[Garment];
                            if (CIBI == null) return;
                            item7 = new ConquerItem(true);
                            item7.ID = Garment;
                            item7.UID = Program.NextItemId;
                            //Program.NextItemID++;
                            item7.Position = 9;
                            item7.Durability = CIBI.Durability;
                            item7.MaximDurability = CIBI.Durability;
                            Equipment.Remove(9);
                            if (Equipment.Objects[8] != null) {
                                Equipment.Objects[8] = null;
                            }

                            Equipment.Add(item7);
                            item7.Mode = Enums.ItemMode.Update;
                            item7.Send(this);
                            equip = new ClientEquip();
                            equip.DoEquips(this);
                            Send(equip);
                            Equipment.UpdateEntityPacket();
                        }

                        #endregion Equip


                        Program.World.Register(this);
                        Kernel.GamePool.Add(Entity.UID, this);
                        FakeLoaded = true;
                        LoggedIn = true;
                        Entity.NobilityRank = NobilityInformation.Rank;
                        {
                            if (FakeLoaded) {
                                #region booth

                                if (Booth == null) {
                                    Send(new MapStatus() {
                                        BaseID = Map.BaseID,
                                        ID = Map.ID,
                                        Status = MapsTable.MapInformations[1036].Status
                                    });
                                    Booth = new Booth(this,
                                        new Data(true) { UID = Entity.UID });
                                    Send(new Data(true)
                                        { ID = Data.ChangeAction, UID = Entity.UID, dwParam = 0 });

                                    #region new multi items

                                    try {
                                        for (uint i = 0; i < count; i++) {
                                            for (int ii = 0; ii < itemID.Length; ii++) {
                                                BoothItem item =
                                                    new BoothItem();
                                                if (itemCost[ii] != null)
                                                    item.Cost = uint.Parse(itemCost[ii]);
                                                item.Item = new ConquerItem(true);
                                                if (itemID[ii] != null)
                                                    item.Item.ID = uint.Parse(itemID[ii]);
                                                item.Item.UID = Program.NextItemId;
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

                                                ConquerItemBaseInformation CIBI = null;
                                                CIBI = ConquerItemInformation.BaseInformations[item.Item.ID];
                                                if (CIBI == null)
                                                    return;
                                                item.Item.Durability = CIBI.Durability;
                                                item.Item.MaximDurability = CIBI.Durability;
                                                //  this.Inventory.Add(item.Item, Game.Enums.ItemUse.CreateAndAdd);
                                                item.Item.Send(this);
                                                {
                                                    ItemUsage usage = new ItemUsage(true)
                                                        { ID = ItemUsage.AddItemOnBoothForConquerPoints };
                                                    item.Cost_Type = BoothItem.CostType
                                                        .ConquerPoints;
                                                    Booth.ItemList.Add(item.Item.UID, item);
                                                    Send(usage);
                                                    Network.GamePackets.BoothItem buffer =
                                                        new Network.GamePackets.BoothItem(true);
                                                    buffer.Fill(item, Booth.Base.UID);
                                                    SendScreen(buffer, false);
                                                }
                                            }
                                        }
                                    }
                                    catch {
                                        return;
                                    }

                                    #endregion

                                    Booth.HawkMessage = new Message(hawkmessage, "ALL", Entity.Name,
                                        Color.White, Message.HawkMessage);
                                }

                                #endregion
                            }
                        }
                    }
                }
            }
        }

        public static void LoadBoothingAI() {
            //    Program.NextItemID = ConquerItem.ItemUID.Now - 500000;
            MySqlCommand Cmd = new MySqlCommand(MySqlCommandType.SELECT);
            Cmd.Select("bots");
            MySqlReader Reader = new MySqlReader(Cmd);
            while (Reader.Read()) {
                var ID = Reader.ReadUInt32("BotID");
                if (ID < 70000000)
                    ID = (uint)Kernel.Random.Next(70000000, 999999999);
                var fClient = new GameState(null);
                fClient.FakeLoadx(ID);
                BoothingAI.Add(ID, fClient);
            }

            //  Reader.Close();
            //  Reader.Dispose();
            Console.WriteLine("" + BoothingAI.Count + " BoothingAI Loaded.");
        }

        public static void Load_New_Booths() {
            MySqlCommand Cmd = new MySqlCommand(MySqlCommandType.SELECT);
            Cmd.Select("booths");
            MySqlReader Reader = new MySqlReader(Cmd);
            while (Reader.Read()) {
                var ID = Reader.ReadUInt32("BotID");
                var Name = Reader.ReadString("BotName");
                var Map = Reader.ReadUInt16("BotMap");
                var X = Reader.ReadUInt16("BotMapx");
                var Y = Reader.ReadUInt16("BotMapy");
                var itemz = Reader.ReadString("BItemID")
                    .Split(["~", "@@", " "], StringSplitOptions.RemoveEmptyEntries);
                var costz = Reader.ReadString("BItemCost")
                    .Split(["~", "@@", " "], StringSplitOptions.RemoveEmptyEntries);
                var plusz = Reader.ReadString("BItemPlus")
                    .Split(["~", "@@", " "], StringSplitOptions.RemoveEmptyEntries);
                var blessz = Reader.ReadString("BItemBless")
                    .Split(["~", "@@", " "], StringSplitOptions.RemoveEmptyEntries);
                var hpz = Reader.ReadString("BItemEnchant")
                    .Split(["~", "@@", " "], StringSplitOptions.RemoveEmptyEntries);
                var soc1z = Reader.ReadString("BItemSoc1")
                    .Split(["~", "@@", " "], StringSplitOptions.RemoveEmptyEntries);
                var soc2z = Reader.ReadString("BItemSoc2")
                    .Split(["~", "@@", " "], StringSplitOptions.RemoveEmptyEntries);
                Booth booth = new Booth();
                SobNpcSpawn Base = new SobNpcSpawn();
                Base.UID = ID;
                if (Booth.Booths2.ContainsKey(Base.UID))
                    Booth.Booths2.Remove(Base.UID);
                Booth.Booths2.Add(Base.UID, booth);
                Base.Mesh = 100;
                Base.Type = Enums.NpcType.Booth;
                Base.ShowName = true;
                Base.Name = "matrix�[" + Base.UID.ToString() + "]";
                Base.MapID = Map;
                Base.X = X;
                Base.Y = Y;
                if (Kernel.Maps[Map].Npcs.ContainsKey(Base.UID))
                    Kernel.Maps[Map].Npcs.Remove(Base.UID);
                Kernel.Maps[Map].Npcs.Add(Base.UID, Base);

                for (int i = 0; i < itemz.Length; i++) {
                    #region booth

                    BoothItem item = new BoothItem();
                    if (costz.Length > i)
                        item.Cost = uint.Parse(costz[i]);
                    item.Item = new ConquerItem(true);
                    item.Item.ID = uint.Parse(itemz[i]);
                    item.Item.UID = Program.NextItemId;
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

                    ConquerItemBaseInformation CIBI = null;
                    CIBI = ConquerItemInformation.BaseInformations[item.Item.ID];
                    if (CIBI == null)
                        break;
                    item.Item.Durability = CIBI.Durability;
                    item.Item.MaximDurability = CIBI.Durability;
                    item.Cost_Type = BoothItem.CostType.ConquerPoints;
                    booth.ItemList.Add(item.Item.UID, item);

                    #endregion
                }
            }

            Console.WriteLine("" + Booth.Booths2.Count + " New Booths Loaded.");
        }

        public static bool IsVaildForTeamPk(GameState client) {
            if (client.Team is { EliteFighterStats.Flag: TeamElitePk.FighterStats.StatusFlag.Fighting }) return true;

            return false;
        }

        #region Network

        public GameState(ClientWrapper? socket) {
            Fake = socket == null;
            if (Fake) socket = new ClientWrapper() { Alive = true };
            Queue = new ConcurrentPacketQueue();
            PacketFilter = new PacketFilter() { { 10010, 10 }, { 10005, 7 }, { 2064, 4 }, { 2032, 3 }, { 1027, 2 } };
            Attackable = false;
            Action = 0;
            _socket = socket;

            Cryptography = new GameCryptography(Program.Encoding.GetBytes(GameConstants.GameCryptographyKey));
            DHKeyExchange = new DHKeyExchange.ServerKeyExchange();
            SpiritBeadQ = new SpiritBeadQuest(this);
            ChiPowers = [];
            Retretead_ChiPowers = new ChiPowerStructure[4];
        }

        public bool Ninja() {
            if (Entity.EntityFlag == EntityFlag.Player) {
                if (Entity.Class is >= 50 and <= 55)
                    return true;
                else
                    return false;
            }

            return false;
        }

        public void ReadyToPlay() {
            try {
                Weapons = new Tuple<ConquerItem, ConquerItem>(null, null);
                ItemSyncRoot = new object();
                Screen = new Screen(this);
                //  if (!Program.ServerTransfer)
                {
                    Pet = new Pet(this);
                    AI = new AI(this);
                }
                Inventory = new Inventory(this);
                Equipment = new Equipment(this);
                WarehouseOpen = false;
                WarehouseOpenTries = 0;
                TempPassword = "";
                ArsenalDonations = new uint[10];
                if (Account != null) {
                    Warehouses =
                        new SafeDictionary<Warehouse.WarehouseID,
                            Warehouse>(20);
                    Warehouses.Add((Warehouse.WarehouseID)Account.EntityID,
                        new Warehouse(this,
                            (Warehouse.WarehouseID)Account.EntityID, 200));
                    Warehouses.Add(Warehouse.WarehouseID.TwinCity,
                        new Warehouse(this,
                            Warehouse.WarehouseID.TwinCity));
                    Warehouses.Add(Warehouse.WarehouseID.PhoenixCity,
                        new Warehouse(this,
                            Warehouse.WarehouseID.PhoenixCity));
                    Warehouses.Add(Warehouse.WarehouseID.ApeCity,
                        new Warehouse(this,
                            Warehouse.WarehouseID.ApeCity));
                    Warehouses.Add(Warehouse.WarehouseID.DesertCity,
                        new Warehouse(this,
                            Warehouse.WarehouseID.DesertCity));
                    Warehouses.Add(Warehouse.WarehouseID.BirdCity,
                        new Warehouse(this,
                            Warehouse.WarehouseID.BirdCity));
                    Warehouses.Add(Warehouse.WarehouseID.StoneCity,
                        new Warehouse(this,
                            Warehouse.WarehouseID.StoneCity));
                    Warehouses.Add(Warehouse.WarehouseID.Market,
                        new Warehouse(this,
                            Warehouse.WarehouseID.Market));
                    Warehouses.Add(Warehouse.WarehouseID.Poker,
                        new Warehouse(this,
                            Warehouse.WarehouseID.Poker));

                    if (Account != null) {
                        if (!Warehouses.ContainsKey((Warehouse.WarehouseID)Account.EntityID))
                            Warehouses.Add((Warehouse.WarehouseID)Account.EntityID,
                                new Warehouse(this,
                                    (Warehouse.WarehouseID)Account.EntityID));
                    }
                }

                Trade = new Game.ConquerStructures.Trade();
                ArenaStatistic = new ArenaStatistic(true);
                Prayers = [];
                map = null;
                SpiritBeadQ = new SpiritBeadQuest(this);
                Quests = new Quests(this);
            }
            catch (Exception e) {
                Program.SaveException(e);
            }
        }

        public void Send(byte[] buffer) {
            if (Fake) return;
            if (!_socket.Alive) return;
            ushort length = BitConverter.ToUInt16(buffer, 0);
            if (length >= 1024 && buffer.Length > length) {
                //Console.WriteLine(Environment.StackTrace);
                return;
            }

            byte[] _buffer = new byte[buffer.Length];
            if (length == 0)
                Writer.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
            Buffer.BlockCopy(buffer, 0, _buffer, 0, buffer.Length);
            Writer.WriteString(GameConstants.ServerKey, _buffer.Length - 8, _buffer);
            try {
                lock (_socket) {
                    if (!_socket.Alive) return;
                    lock (Cryptography) {
                        Cryptography.Encrypt(_buffer, _buffer.Length);
                        _socket.Send(_buffer);
                    }
                }
            }
            catch (Exception) {
                _socket.Alive = false;
                Disconnect();
            }
        }

        private void EndSend(IAsyncResult res) {
            try {
                _socket.Socket.EndSend(res);
            }
            catch {
                _socket.Alive = false;
                Disconnect();
            }
        }

        public void Send(IPacket buffer) {
            Send(buffer.ToArray());
        }

        public void SendScreenSpawn(IMapObject obj, bool self) {
            try {
                foreach (IMapObject _obj in Screen.Objects) {
                    if (_obj == null)
                        continue;
                    if (_obj.UID != Entity.UID) {
                        if (_obj.MapObjType == MapObjectType.Player) {
                            GameState client = _obj.Owner as GameState;
                            obj.SendSpawn(client, false);
                        }
                    }
                }

                if (self)
                    obj.SendSpawn(this);
            }
            catch (Exception e) {
                Program.SaveException(e);
            }
        }

        public void RemoveScreenSpawn(IMapObject obj, bool self) {
            try {
                if (Screen == null) return;
                if (Screen.Objects == null) return;
                foreach (IMapObject _obj in Screen.Objects) {
                    if (_obj == null) continue;
                    if (obj == null) continue;
                    if (_obj.UID != Entity.UID) {
                        if (_obj.MapObjType == MapObjectType.Player) {
                            GameState client = _obj.Owner as GameState;
                            client.Screen.Remove(obj);
                        }
                    }
                }

                if (self)
                    Screen.Remove(obj);
            }
            catch (Exception e) {
                Program.SaveException(e);
            }
        }

        public void SendScreen(byte[] buffer, bool self = true) {
            try {
                foreach (IMapObject obj in Screen.Objects) {
                    if (obj == null) continue;
                    if (obj.UID != Entity.UID) {
                        if (obj.MapObjType == MapObjectType.Player) {
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
            catch (Exception e) {
                Program.SaveException(e);
            }
        }

        public void SendScreen(IPacket buffer, bool self = true, bool message = false) {
            if (Screen == null) return;
            if (Screen.Objects == null) return;
            foreach (IMapObject obj in Screen.Objects) {
                if (obj == null)
                    continue;
                if (obj.MapObjType == MapObjectType.Player) {
                    GameState client = obj.Owner as GameState;
                    if (message && client.Entity.BlackList.Contains(Entity.Name)) continue;
                    if (client.Entity.UID != Entity.UID)
                        client.Send(buffer);
                }
            }

            if (self)
                Send(buffer);
        }

        public void Disconnect(bool save = true) {
            if (Fake) return;
            if (Screen != null) Screen.DisposeTimers();
            PacketHandler.RemoveTPA(this);
            Program.World.Unregister(this);
            if (OnDisconnect != null) OnDisconnect(this);
            if (_socket.Connector != null) {
                _socket.Disconnect();
                ShutDown();
            }
        }

        private void ShutDown() {
            if (Socket.Connector == null) return;
            Socket.Connector = null;
            if (Entity != null) {
                try {
                    if (Entity.JustCreated) return;

                    #region Poker

                    if (Entity.PokerTable != null) {
                        var T = Entity.PokerTable;
                        if (T != null)
                            if (T.Players.ContainsKey(Entity.UID) && T.Pot > 1) {
                                T.StopMoveCountDown();
                                T.RemovePlayer(Entity.UID);
                            }
                            else
                                T.RemovePlayer(Entity.UID);
                    }

                    #endregion

                    Time32 now = Time32.Now;
                    Kernel.DisconnectPool.Add(Entity.UID, this);
                    RemoveScreenSpawn(Entity, false);
                    if (Entity is { WTitles: not null })
                        Entity.WTitles.Update();
                    using (var conn = DataHolder.MySqlConnection) {
                        conn.Open();
                        EntityTable.UpdateOnlineStatus(this, false, conn);
                        EntityTable.SaveEntity(this, conn);
                        if (!TransferedPlayer)
                            EntityVariableTable.Save(this, conn);
                        SkillTable.SaveProficiencies(this);
                        SkillTable.SaveSpells(this);
                        if (!TransferedPlayer) {
                            ArenaTable.SaveArenaStatistics(ArenaStatistic, conn);
                            TeamArenaTable.SaveArenaStatistics(TeamArenaStatistic, conn);
                            ChampionTable.SaveStatistics(ChampionStats, conn);
                        }
                    }

                    foreach (var kerO in Entity.StorageItems) {
                        ConquerItemTable.UpdateWardrobe(true, kerO.Key);
                    }

                    Kernel.GamePool.Remove(Entity.UID);


                    if (Booth != null)
                        Booth.Remove();

                    if (Entity.MyClones.Count > 0) {
                        foreach (var item in Entity.MyClones.Values) {
                            Data data = new Data(true);
                            data.UID = item.UID;
                            data.ID = Data.RemoveEntity;
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


                    Arena.Clear(this);
                    TeamArena.Clear(this);
                    Champion.Clear(this);

                    RemoveScreenSpawn(Entity, false);

                    #region Friend/TradePartner/Apprentice

                    Message msg = new Message("Your friend, " + Entity.Name + ", has logged off.",
                        Color.Red, Message.TopLeft);
                    if (Friends == null)
                        Friends = new SafeDictionary<uint, Friend>(100);
                    foreach (Friend friend in Friends.Values) {
                        if (friend.IsOnline) {
                            var packet = new Network.GamePackets.KnownPersons(true) {
                                UID = Entity.UID,
                                Type = Network.GamePackets.KnownPersons.RemovePerson,
                                Name = Entity.Name,
                                Online = false
                            };
                            friend.Client.Send(packet);
                            packet.Type = Network.GamePackets.KnownPersons.AddFriend;
                            if (friend is { Client: not null }) {
                                friend.Client.Send(packet);
                                friend.Client.Send(msg);
                            }
                        }
                    }

                    Message msg2 = new Message("Your partner, " + Entity.Name + ", has logged off.",
                        Color.Red, Message.TopLeft);

                    if (Partners != null) {
                        foreach (TradePartner partner in Partners.Values) {
                            if (partner.IsOnline) {
                                var packet = new Network.GamePackets.TradePartner(true) {
                                    UID = Entity.UID,
                                    Type = Network.GamePackets.TradePartner.BreakPartnership,
                                    Name = Entity.Name,
                                    HoursLeft = (int)(new TimeSpan(partner.ProbationStartedOn.AddDays(3).Ticks)
                                        .TotalHours - new TimeSpan(DateTime.Now.Ticks).TotalHours),
                                    Online = false
                                };
                                partner.Client.Send(packet);
                                packet.Type = Network.GamePackets.TradePartner.AddPartner;
                                if (partner is { Client: not null }) {
                                    partner.Client.Send(packet);
                                    partner.Client.Send(msg2);
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
                    if (Apprentices == null)
                        Apprentices = new SafeDictionary<uint, Apprentice>();
                    foreach (var appr in Apprentices.Values) {
                        if (appr.IsOnline) {
                            Information.Apprentice_ID = appr.ID;
                            Information.Enrole_Date = appr.EnroleDate;
                            Information.Apprentice_Name = appr.Name;
                            appr.Client.Send(Information);
                            appr.Client.ReviewMentor();
                        }
                    }

                    if (Mentor is { IsOnline: true }) {
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

                    if (Team != null) {
                        Team.Remove(this);
                    }

                    foreach (var item in Entity.StorageItems.Values) {
                        if (!item.InWardrobe) {
                            item.InWardrobe = true;
                            ConquerItemTable.UpdateWardrobe(item.InWardrobe, item.UID);
                        }
                    }
                }
                catch (Exception e) {
                    Program.SaveException(e);
                }
                finally {
                    Kernel.DisconnectPool.Remove(Entity.UID);
                    Console.WriteLine(Entity.Name + " logged out. IP: " + Account.IP + "  ");
                }
            }
        }

        public ClientWrapper Socket {
            get { return _socket; }
        }

        public string IP {
            get { return _socket.IP; }
        }

        #endregion

        #region Game

        public ChiTable.ChiData ChiData;
        public List<ChiPowerStructure> ChiPowers;
        public ChiPowerStructure[] Retretead_ChiPowers;
        public uint ChiPoints = 0;

        public SafeDictionary<uint, DetainedItem> ClaimableItem = new SafeDictionary<uint, DetainedItem>(1000),
            DeatinedItem = new SafeDictionary<uint, DetainedItem>(1000);

        public bool DoSetOffline = true;

        public ushort OnlineTrainingPoints = 0;
        public Time32 LastTrainingPointsUp, LastTreasurePoints = Time32.Now.AddMinutes(1);

        public List<string> GuildNamesSpawned = [];

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

        public static uint ScreenColor;

        #region Night Color

        public void Night() {
            ScreenColor = 5855577;

            ScreenColor Packet = new ScreenColor(true);
            Packet.UID = Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values) {
                pclient.Send(Packet);
            }
        }

        public void Night1() {
            ScreenColor = 3358767;

            ScreenColor Packet = new ScreenColor(true);
            Packet.UID = Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values) {
                pclient.Send(Packet);
            }
        }

        public void Night2() {
            ScreenColor = 97358;

            ScreenColor Packet = new ScreenColor(true);
            Packet.UID = Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values) {
                pclient.Send(Packet);
            }
        }

        #endregion

        #region Blue Color

        public void Blue() {
            ScreenColor = 69852;

            ScreenColor Packet = new ScreenColor(true);
            Packet.UID = Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values) {
                pclient.Send(Packet);
            }
        }

        public void Blue1() {
            ScreenColor = 4532453;

            ScreenColor Packet = new ScreenColor(true);
            Packet.UID = Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values) {
                pclient.Send(Packet);
            }
        }

        public void Blue2() {
            ScreenColor = 684533;

            ScreenColor Packet = new ScreenColor(true);
            Packet.UID = Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values) {
                pclient.Send(Packet);
            }
        }

        #endregion

        #region Green Color

        public void Green() {
            ScreenColor = 838915;

            ScreenColor Packet = new ScreenColor(true);
            Packet.UID = Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values) {
                pclient.Send(Packet);
            }
        }

        public void Green1() {
            ScreenColor = 824383;

            ScreenColor Packet = new ScreenColor(true);
            Packet.UID = Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values) {
                pclient.Send(Packet);
            }
        }

        public void Green2() {
            ScreenColor = 456828;

            ScreenColor Packet = new ScreenColor(true);
            Packet.UID = Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values) {
                pclient.Send(Packet);
            }
        }

        public void Green3() {
            ScreenColor = 5547633;

            ScreenColor Packet = new ScreenColor(true);
            Packet.UID = Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values) {
                pclient.Send(Packet);
            }
        }

        public void Green4() {
            ScreenColor = 453450;

            ScreenColor Packet = new ScreenColor(true);
            Packet.UID = Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values) {
                pclient.Send(Packet);
            }
        }

        #endregion

        #region Day Color

        public void Day() {
            ScreenColor = 0;

            ScreenColor Packet = new ScreenColor(true);
            Packet.UID = Entity.UID;
            Packet.ID = 104;
            Packet.dwParam = ScreenColor;
            foreach (GameState pclient in Kernel.GamePool.Values) {
                pclient.Send(Packet);
            }
        }

        #endregion

        #endregion

        public Time32 ScreenReloadTime;
        public int MillisecondsScreenReload;
        public bool Reloaded = false;
        public IPacket ReloadWith;

        public ushort VendingDisguise;

        //public uint BlessTime;
        public uint BlessTime {
            get { return this["BlessTime"]; }
            set { this["BlessTime"] = value; }
        }

        public DateTime BlessStamp {
            get { return this["BlessStamp"]; }
            set { this["BlessStamp"] = value; }
        }

        public DateTime DoubleExperienceStamp {
            get { return this["DoubleExperienceStamp"]; }
            set { this["DoubleExperienceStamp"] = value; }
        }

        public int speedHackSuspiction = 0;
        public Time32 LastPingT;

        public uint LastPingStamp = 0;
        // public Game.Entity Companion;

        public List<GameState> Prayers;
        public GameState? PrayLead;

        public DateTime ChatBanTime;
        public uint ChatBanLasts;
        public bool ChatBanned;

        public uint BackupArmorLook {
            get { return this["bkparmorlook"]; }
            set { this["bkparmorlook"] = value; }
        }

        public uint ArmorLook {
            get { return this["armorlook"]; }
            set { this["armorlook"] = value; }
        }

        public uint WeaponLook {
            get { return this["weaponlook"]; }
            set { this["weaponlook"] = value; }
        }

        public uint WeaponLook2 {
            get { return this["weaponlook2"]; }
            set { this["weaponlook2"] = value; }
        }

        public uint HeadgearLook {
            get { return this["headgearlook"]; }
            set { this["headgearlook"] = value; }
        }

        public bool ValidArmorLook(uint id) {
            if (id == 0) return false;

            var soulInfo = AddingInformationTable.SoulGearItems[id];
            if (id is >= 800000 and < 900000) {
                if (soulInfo.ItemIdentifier < 100)
                    if (soulInfo.ItemIdentifier != ConquerItem.Armor)
                        return false;
                    else { }
                else if (PacketHandler.ItemPosition((uint)(soulInfo.ItemIdentifier * 1000)) !=
                         ConquerItem.Armor)
                    return false;
            }
            else if (PacketHandler.ItemPosition(id) != ConquerItem.Armor)
                return false;

            return true;
        }

        public bool ValidHeadgearLook(uint id) {
            if (id == 0) return false;

            var soulInfo = AddingInformationTable.SoulGearItems[id];
            if (id is >= 800000 and < 900000) {
                if (soulInfo.ItemIdentifier < 100)
                    if (soulInfo.ItemIdentifier != ConquerItem.Head)
                        return false;
                    else { }
                else if (PacketHandler.ItemPosition((uint)(soulInfo.ItemIdentifier * 1000)) != ConquerItem.Head)
                    return false;
            }
            else if (PacketHandler.ItemPosition(id) != ConquerItem.Head)
                return false;

            return true;
        }

        public bool ValidWeaponLook(uint id) {
            if (id == 0) return false;
            if (PacketHandler.ItemPosition(id) != ConquerItem.RightWeapon)
                return false;
            return true;
        }

        public bool ValidWeaponLook2(uint id) {
            if (id == 0) return false;
            if (PacketHandler.ItemPosition(id) != ConquerItem.RightWeapon) {
                if (PacketHandler.ItemPosition(id) != ConquerItem.LeftWeapon)
                    return false;
            }
            else {
                if (PacketHandler.IsTwoHand(id))
                    return false;
            }

            return true;
        }

        public ConquerItemBaseInformation CheckLook(string name, ushort pos, out int minDist) {
            minDist = int.MaxValue;
            ConquerItemBaseInformation CIBI = null;
            Enums.ItemQuality Quality = Enums.ItemQuality.Fixed;
            var itemx = Equipment.TryGetItem((byte)pos);
            if (itemx != null)
                Quality = (Enums.ItemQuality)(itemx.ID % 10);

            foreach (var item in ConquerItemInformation.BaseInformations.Values) {
                if (pos == ConquerItem.Armor) {
                    if (ValidArmorLook(item.ID)) {
                        int dist = name.LevenshteinDistance(item.LowerName);
                        if (minDist > dist && Quality == (Enums.ItemQuality)(item.ID % 10)) {
                            CIBI = item;
                            minDist = dist;
                        }
                    }
                }
                else if (pos == ConquerItem.Head) {
                    if (ValidHeadgearLook(item.ID)) {
                        int dist = name.LevenshteinDistance(item.LowerName);
                        if (minDist > dist && Quality == (Enums.ItemQuality)(item.ID % 10)) {
                            CIBI = item;
                            minDist = dist;
                        }
                    }
                }
                else if (pos == ConquerItem.LeftWeapon) {
                    if (ValidWeaponLook2(item.ID)) {
                        int dist = name.LevenshteinDistance(item.LowerName);
                        if (minDist > dist && !PacketHandler.IsTwoHand(item.ID) &&
                            Quality == (Enums.ItemQuality)(item.ID % 10)) {
                            CIBI = item;
                            minDist = dist;
                        }
                    }
                }
                else if (pos == ConquerItem.RightWeapon) {
                    if (ValidWeaponLook(item.ID)) {
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
                            if (minDist > dist && !PacketHandler.IsTwoHand(item.ID) &&
                                Quality == (Enums.ItemQuality)(item.ID % 10)) {
                                CIBI = item;
                                minDist = dist;
                            }
                        }
                    }
                }
            }

            return CIBI;
        }

        public void SetNewArmorLook(uint id, bool change = true) {
            if (change)
                ArmorLook = id;
            if (!ValidArmorLook(id)) return;
            int min = 0;
            id = CheckLook(ConquerItemInformation.BaseInformations[id].LowerName, ConquerItem.Armor, out min)
                .ID;

            var item = Equipment.TryGetItem(ConquerItem.Armor);
            var iu = new ItemUsage(true);
            iu.UID = uint.MaxValue - 1;
            iu.dwParam = 13;
            iu.ID = ItemUsage.UnequipItem;
            Send(iu);
            iu = new ItemUsage(true);
            iu.UID = uint.MaxValue - 1;
            iu.ID = ItemUsage.RemoveInventory;
            Send(iu);

            ConquerItem fakeItem = new ConquerItem(true);
            fakeItem.ID = id;
            if (item != null) {
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
            fakeItem.Color = Enums.Color.Black;
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

        public void SetNewHeadgearLook(uint id, bool change = true) {
            if (change)
                HeadgearLook = id;
            if (!ValidHeadgearLook(id)) return;
            int min = 0;
            id = CheckLook(ConquerItemInformation.BaseInformations[id].LowerName, ConquerItem.Head, out min)
                .ID;

            var item = Equipment.TryGetItem(ConquerItem.Head);
            var iu = new ItemUsage(true);
            iu.UID = uint.MaxValue - 2;
            iu.dwParam = 14;
            iu.ID = ItemUsage.UnequipItem;
            Send(iu);
            iu = new ItemUsage(true);
            iu.UID = uint.MaxValue - 2;
            iu.ID = ItemUsage.RemoveInventory;
            Send(iu);

            ConquerItem fakeItem = new ConquerItem(true);
            fakeItem.ID = id;
            if (item != null) {
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
            fakeItem.Color = Enums.Color.Black;
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

        public void SetNewWeaponLook(uint id, bool change = true) {
            if (change)
                WeaponLook = id;
            if (!ValidWeaponLook(id)) return;
            int min = 0;

            var item = Equipment.TryGetItem(ConquerItem.RightWeapon);
            var iu = new ItemUsage(true);
            iu.UID = uint.MaxValue - 3;
            iu.dwParam = ConquerItem.RightWeaponAccessory;
            iu.ID = ItemUsage.UnequipItem;
            Send(iu);
            iu = new ItemUsage(true);
            iu.UID = uint.MaxValue - 3;
            iu.ID = ItemUsage.RemoveInventory;
            Send(iu);

            id = CheckLook(ConquerItemInformation.BaseInformations[id].LowerName, ConquerItem.RightWeapon,
                out min).ID;

            ConquerItem fakeItem = new ConquerItem(true);
            fakeItem.ID = id;
            if (item != null) {
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

        public void SetNewWeaponLook2(uint id, bool change = true) {
            if (change)
                WeaponLook2 = id;
            if (!ValidWeaponLook2(id)) return;
            int min = 0;

            var item = Equipment.TryGetItem(ConquerItem.LeftWeapon);
            var iu = new ItemUsage(true);
            iu.UID = uint.MaxValue - 4;
            iu.dwParam = ConquerItem.LeftWeaponAccessory;
            iu.ID = ItemUsage.UnequipItem;
            Send(iu);
            iu = new ItemUsage(true);
            iu.UID = uint.MaxValue - 4;
            iu.ID = ItemUsage.RemoveInventory;
            Send(iu);

            id = CheckLook(ConquerItemInformation.BaseInformations[id].LowerName, ConquerItem.LeftWeapon,
                out min).ID;

            ConquerItem fakeItem = new ConquerItem(true);
            fakeItem.ID = id;
            if (item != null) {
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

        public ushort Vigor {
            get {
                if (Equipment != null)
                    if (!Equipment.Free(12))
                        return Equipment.TryGetItem(12).Vigor;
                return 65535;
            }
            set {
                if (!Equipment.Free(12))
                    Equipment.TryGetItem(12).Vigor = value;
            }
        }

        ushort _Maxvigor;

        public ushort MaxVigor {
            get { return _Maxvigor; }
            set { _Maxvigor = value; }
        }


        public bool HeadgearClaim, NecklaceClaim, ArmorClaim, WeaponClaim, RingClaim, BootsClaim, TowerClaim, FanClaim;

        public string PromoteItemNameNeed {
            get {
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

        public byte PromoteItemCountNeed {
            get {
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

        public uint PromoteItemNeed {
            get {
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

        public uint PromoteItemGain {
            get {
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
                    return DragonBall;
                return 0;
            }
        }

        public uint PromoteLevelNeed {
            get {
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
        public Guild? Guild;
        public GuildMember? AsMember;
        public uint Arsenal_Donation = 0;
        public Booth Booth;


        public bool RaceExcitement, RaceDecelerated, RaceGuard, RaceDizzy, RaceFrightened;
        public Time32 RaceExcitementStamp, GuardStamp, DizzyStamp, FrightenStamp, ExtraVigorStamp, DecelerateStamp;
        public uint RaceExcitementAmount, RaceExtraVigor;
        public GameCharacterUpdates? SpeedChange;

        public void ApplyRacePotion(Enums.RaceItemType type, uint target) {
            switch (type) {
                case Enums.RaceItemType.FrozenTrap: {
                    if (target != uint.MaxValue) {
                        if (Map.Floor[Entity.X, Entity.Y, MapObjectType.StaticEntity]) {
                            StaticEntity item = new StaticEntity((uint)(Entity.X * 1000 + Entity.Y), Entity.X, Entity.Y,
                                Map.ID);
                            item.DoFrozenTrap(Entity.UID);
                            Map.AddStaticEntity(item);
                            Kernel.SendSpawn(item);
                        }
                    }
                    else {
                        Entity.FrozenStamp = Time32.Now;
                        Entity.FrozenTime = 5;
                        GameCharacterUpdates update = new GameCharacterUpdates(true);
                        update.UID = Entity.UID;
                        update.Add(GameCharacterUpdates.Freeze, 0, 4);
                        SendScreen(update);
                        Entity.AddFlag(Update.Flags.Freeze);
                    }

                    break;
                }
                case Enums.RaceItemType.RestorePotion: {
                    Vigor += 2000;
                    if (Vigor > MaxVigor)
                        Vigor = MaxVigor;
                    Send(new Vigor(true) { Amount = Vigor });
                    break;
                }
                case Enums.RaceItemType.ExcitementPotion: {
                    if (RaceExcitement && RaceExcitementAmount > 50)
                        return;

                    if (RaceDecelerated) {
                        RaceDecelerated = false;

                        var upd = new GameCharacterUpdates(true);
                        upd.UID = Entity.UID;
                        upd.Remove(GameCharacterUpdates.Decelerated);
                        SendScreen(upd);
                    }

                    RaceExcitementStamp = Time32.Now;
                    RaceExcitement = true;
                    {
                        var upd = new GameCharacterUpdates(true);
                        upd.UID = Entity.UID;
                        upd.Add(GameCharacterUpdates.Accelerated, 50, 15, 25);
                        SendScreen(upd);
                        SpeedChange = upd;
                    }
                    RaceExcitementAmount = 50;
                    Entity.AddFlag(Update.Flags.OrangeSparkles);
                    break;
                }
                case Enums.RaceItemType.SuperExcitementPotion: {
                    if (RaceDecelerated) {
                        RaceDecelerated = false;

                        var upd = new GameCharacterUpdates(true);
                        upd.UID = Entity.UID;
                        upd.Remove(GameCharacterUpdates.Decelerated);
                        SendScreen(upd);
                    }

                    RaceExcitementAmount = 200;
                    RaceExcitementStamp = Time32.Now;
                    RaceExcitement = true;
                    Entity.AddFlag(Update.Flags.SpeedIncreased);
                    {
                        var upd = new GameCharacterUpdates(true);
                        upd.UID = Entity.UID;
                        upd.Add(GameCharacterUpdates.Accelerated, 200, 15, 100);
                        SendScreen(upd);
                        SpeedChange = upd;
                    }
                    Entity.AddFlag(Update.Flags.OrangeSparkles);
                    break;
                }
                case Enums.RaceItemType.GuardPotion: {
                    RaceGuard = true;
                    GuardStamp = Time32.Now;
                    Entity.AddFlag(Update.Flags.DivineShield);
                    DizzyStamp = DizzyStamp.AddSeconds(-100);
                    FrightenStamp = FrightenStamp.AddSeconds(-100);
                    var upd = new GameCharacterUpdates(true);
                    upd.UID = Entity.UID;
                    upd.Add(GameCharacterUpdates.DivineShield, 0, 10);
                    SendScreen(upd);
                    break;
                }
                case Enums.RaceItemType.DizzyHammer: {
                    Entity Target;
                    if (Screen.TryGetValue(target, out Target)) {
                        var Owner = Target.Owner;
                        if (Owner is { RaceGuard: false, RaceFrightened: false }) {
                            Owner.DizzyStamp = Time32.Now;
                            Owner.RaceDizzy = true;
                            Owner.Entity.AddFlag(Update.Flags.Dizzy);
                            {
                                var upd = new GameCharacterUpdates(true);
                                upd.UID = Entity.UID;
                                upd.Add(GameCharacterUpdates.Dizzy, 0, 5);
                                Owner.SendScreen(upd);
                            }
                        }
                    }

                    break;
                }
                case Enums.RaceItemType.ScreamBomb: {
                    SendScreen(new SpellUse(true) {
                        Attacker = Entity.UID,
                        SpellID = 9989,
                        SpellLevel = 0,
                        X = Entity.X,
                        Y = Entity.Y
                    }.AddTarget(Entity, 0, null));
                    foreach (var obj in Screen.SelectWhere<Entity>(MapObjectType.Player,
                                 (o) => Kernel.GetDistance(o.X, o.Y, Entity.X, Entity.Y) <= 10)) {
                        var Owner = obj.Owner;
                        if (Owner is { RaceGuard: false, RaceDizzy: false }) {
                            Owner.RaceFrightened = true;
                            Owner.FrightenStamp = Time32.Now;
                            Owner.Entity.AddFlag(Update.Flags.Frightened);
                            {
                                var upd = new GameCharacterUpdates(true);
                                upd.UID = Owner.Entity.UID;
                                upd.Add(GameCharacterUpdates.Flustered, 0, 20);
                                Owner.SendScreen(upd);
                            }
                        }
                    }

                    break;
                }
                case Enums.RaceItemType.SpiritPotion: {
                    ExtraVigorStamp = Time32.Now;
                    RaceExtraVigor = 2000;
                    break;
                }
                case Enums.RaceItemType.ChaosBomb: {
                    SendScreen(new SpellUse(true) {
                        Attacker = Entity.UID,
                        SpellID = 9989,
                        SpellLevel = 0,
                        X = Entity.X,
                        Y = Entity.Y
                    }.AddTarget(Entity, 0, null));
                    foreach (var obj in Screen.SelectWhere<Entity>(MapObjectType.Player,
                                 (o) => Kernel.GetDistance(o.X, o.Y, Entity.X, Entity.Y) <= 10)) {
                        var Owner = obj.Owner;
                        if (!Owner.RaceGuard) {
                            Owner.FrightenStamp = Time32.Now;
                            Owner.DizzyStamp = Owner.DizzyStamp.AddSeconds(-1000);

                            Owner.Entity.AddFlag(Update.Flags.Confused);
                            {
                                var upd = new GameCharacterUpdates(true);
                                upd.UID = Owner.Entity.UID;
                                upd.Add(GameCharacterUpdates.Flustered, 0, 15);
                                Owner.SendScreen(upd);
                            }
                        }
                    }

                    break;
                }
                case Enums.RaceItemType.SluggishPotion: {
                    SendScreen(new SpellUse(true) {
                        Attacker = Entity.UID,
                        SpellID = 9989,
                        SpellLevel = 0,
                        X = Entity.X,
                        Y = Entity.Y
                    }.AddTarget(Entity, 0, null));
                    foreach (var obj in Screen.SelectWhere<Entity>(MapObjectType.Player,
                                 o => Kernel.GetDistance(o.X, o.Y, Entity.X, Entity.Y) <= 10)) {
                        var Owner = obj.Owner;
                        if (!Owner.RaceGuard) {
                            Owner.RaceDecelerated = true;
                            Owner.DecelerateStamp = Time32.Now;
                            if (Owner.RaceExcitement) {
                                Owner.RaceExcitement = false;

                                var upd = new GameCharacterUpdates(true);
                                upd.UID = Owner.Entity.UID;
                                upd.Remove(GameCharacterUpdates.Accelerated);
                                Owner.SendScreen(upd);
                            }

                            Owner.Entity.AddFlag(Update.Flags.PurpleSparkles);
                            {
                                var upd = new GameCharacterUpdates(true);
                                upd.UID = Owner.Entity.UID;
                                unchecked {
                                    upd.Add(GameCharacterUpdates.Decelerated, 50, 10, (uint)(0 - 25));
                                }

                                Owner.SendScreen(upd);
                                Owner.SpeedChange = upd;
                            }
                        }
                    }

                    break;
                }
                case Enums.RaceItemType.TransformItem: {
                    for (int i = 0; i < 5; i++) {
                        if (Potions[i] != null) {
                            if (Potions[i].Type != Enums.RaceItemType.TransformItem) {
                                Send(new RacePotion(true) {
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
                        if (Potions[i] == null) {
                            int val = (int)Enums.RaceItemType.TransformItem;
                            while (val == (int)Enums.RaceItemType.TransformItem)
                                val = Kernel.Random.Next((int)Enums.RaceItemType.ChaosBomb,
                                    (int)Enums.RaceItemType.SuperExcitementPotion);
                            Potions[i] = new UsableRacePotion();
                            Potions[i].Count = 1;
                            Potions[i].Type = (Enums.RaceItemType)val;
                            Send(new RacePotion(true) {
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


        public void ReviewMentor() {
            #region NotMentor

            uint nowBP = 0;
            if (Mentor is { IsOnline: true }) {
                nowBP = Entity.BattlePowerFrom(Mentor.Client.Entity);
            }

            if (nowBP > 200) nowBP = 0;
            if (nowBP < 0) nowBP = 0;
            if (Entity.MentorBattlePower != nowBP) {
                Entity.MentorBattlePower = nowBP;
                if (Mentor is { IsOnline: true }) {
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

            #endregion

            #region Mentor

            if (Apprentices == null)
                Apprentices = new SafeDictionary<uint, Apprentice>();
            foreach (var appr in Apprentices.Values) {
                if (appr.IsOnline) {
                    uint nowBPs = 0;
                    nowBPs = appr.Client.Entity.BattlePowerFrom(Entity);
                    if (appr.Client.Entity.MentorBattlePower != nowBPs) {
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

        public void AddQuarantineKill() {
            quarantineKill++;
            UpdateQuarantineScore();
        }

        public void AddGl() {
            TopGlClaim++;
            return;
        }

        public void AddDl() {
            TopDlClaim++;
            return;
        }

        public void AddQuarantineDeath() {
            quarantineDeath++;
            UpdateQuarantineScore();
        }

        public void UpdateQuarantineScore() {
            string[] scores = new string[3];
            scores[0] = "Black team: " + Quarantine.BlackScore.ToString() + " wins";
            scores[1] = "White team: " + Quarantine.WhiteScore.ToString() + " wins";
            scores[2] = "Your score: " + quarantineKill + " kills, " + quarantineDeath + " death";
            for (int i = 0; i < scores.Length; i++) {
                Message msg = new Message(scores[i], Color.Red,
                    i == 0 ? Message.FirstRightCorner : Message.ContinueRightCorner);
                Send(msg);
            }
        }

        public void KillTerrorist() {
            foreach (GameState Terrorist in Program.Values) {
                if (Terrorist.Entity is { KillTheTerrorist_IsTerrorist: true, MapID: 1801 })
                    Kernel.SendWorldMessage(
                        new Message("Terrorist: " + Terrorist.Entity.Name + " ",
                            Color.Black, Message.FirstRightCorner),
                        Program.Values);
            }
        }

        public void AddBless(uint value) {
            Entity.HeavenBlessing += value;
            Entity.Update(_String.Effect, "bless", true);
            if (Mentor != null) {
                if (Mentor.IsOnline) {
                    Mentor.Client.PrizeHeavenBlessing += (ushort)(value / 10 / 60 / 60);
                    AsApprentice = Mentor.Client.Apprentices[Entity.UID];
                }

                if (AsApprentice != null) {
                    AsApprentice.Actual_HeavenBlessing += (ushort)(value / 10 / 60 / 60);
                    AsApprentice.Total_HeavenBlessing += (ushort)(value / 10 / 60 / 60);
                    if (Time32.Now > LastMentorSave.AddSeconds(5)) {
                        KnownPersons.SaveApprenticeInfo(AsApprentice);
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
        public NobilityInformation? NobilityInformation;
        public Entity Entity;
        public Screen Screen;
        public Time32 LastPing = Time32.Now;
        public static ushort NpcTestType = 0;
        public byte TinterItemSelect = 0;
        public DateTime LastDragonBallUse, LastResetTime;
        public byte Action;
        public bool CheerSent = false;
        public Arena.QualifierList.QualifierGroup WatchingGroup;
        public Arena.QualifierList.QualifierGroup QualifierGroup;
        public Champion.QualifierList.QualifierGroup ChampionGroup;
        public ArenaStatistic ArenaStatistic;

        public TeamArena.QualifierList.QualifierGroup TeamWatchingGroup;
        public TeamArena.QualifierList.QualifierGroup TeamQualifierGroup;
        public TeamArenaStatistic TeamArenaStatistic;

        public uint ArenaPoints {
            get { return ArenaStatistic.ArenaPoints; }
            set {
                ArenaStatistic.ArenaPoints =
                    TeamArenaStatistic.ArenaPoints =
                        value;
            }
        }

        private byte xpCount;

        public byte XPCount {
            get { return xpCount; }
            set {
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

        public Game.ConquerStructures.Trade Trade;
        public byte ExpBalls = 0;
        public ulong MoneySave = 0;
        public uint ActiveNpc;
        public string WarehousePW1, TempPassword;
        public uint WarehousePW;
        public bool WarehouseOpen;
        public Time32 CoolStamp;
        public sbyte WarehouseOpenTries;
        public ushort InputLength;
        public Mentor Mentor;
        public Apprentice AsApprentice;
        public SafeDictionary<ushort, ISkill> RemoveSpells = new SafeDictionary<ushort, ISkill>();
        public SafeDictionary<ushort, IProf> Proficiencies;
        public SafeDictionary<ushort, ISkill> Spells;
        public SafeDictionary<uint, Friend> Friends;
        public SafeDictionary<uint, Enemy> Enemy;
        public SafeDictionary<uint, TradePartner> Partners;
        public SafeDictionary<uint, Apprentice> Apprentices;
        public Inventory Inventory;
        public Equipment Equipment;

        public SafeDictionary<Warehouse.WarehouseID, Warehouse>
            Warehouses;

        public Team Team;
        public Time32 lastClientJumpTime = Time32.Now;
        public Time32 lastJumpTime = Time32.Now;
        public int LastJumpTime = 0;
        public short lastJumpDistance = 0;
        public bool DoubleExpToday = false;

        private Map map;

        public Map Map {
            get {
                if (map == null) {
                    Kernel.Maps.TryGetValue(Entity.MapID, out map);
                    /*if (map == null)
                        Entity.MapID = 1005;*/
                    if (map == null)
                        return (map = new Map(Entity.MapID,
                            MapsTable.MapInformations[Entity.MapID].BaseID,
                            DMaps.MapPaths[MapsTable.MapInformations[Entity.MapID].BaseID]));
                }
                else {
                    if (map.ID != Entity.MapID) {
                        Kernel.Maps.TryGetValue(Entity.MapID, out map);
                        /*if (map == null)
                            Entity.MapID = 1005;*/
                        if (map == null)
                            return (map = new Map(Entity.MapID,
                                MapsTable.MapInformations[Entity.MapID].BaseID,
                                DMaps.MapPaths[MapsTable.MapInformations[Entity.MapID].BaseID]));
                    }

                    if (Entity.MapID == 1004 || Entity.MapID == 1458 || Entity.MapID == 1459 || Entity.MapID == 1460 ||
                        Entity.MapID == 16414 || Entity.MapID == 1507 || Entity.MapID == 3990 || Entity.MapID == 3995)
                        if (Entity.ContainsFlag(Update.Flags.Ride)) {
                            Entity.RemoveFlag(Update.Flags.Ride);
                        }
                }

                return map;
            }
        }

        public uint ExpBall {
            get {
                ulong exp = DataHolder.LevelExperience(Entity.Level);
                return (uint)(exp * 13000 / (ulong)((Entity.Level * Entity.Level * Entity.Level / 12) + 1));
            }
        }

        public bool AddProficiency(IProf proficiency) {
            if (Proficiencies.ContainsKey(proficiency.ID)) {
                Proficiencies[proficiency.ID].Level = proficiency.Level;
                Proficiencies[proficiency.ID].Experience = proficiency.Experience;
                proficiency.Send(this);
                SkillTable.SaveProficiencies(this);
                return false;
            }
            else {
                Proficiencies.Add(proficiency.ID, proficiency);
                proficiency.NeededExperience = DataHolder.ProficiencyLevelExperience(proficiency.Level);
                proficiency.Send(this);
                SkillTable.SaveProficiencies(this);
                return true;
            }
        }

        public bool AddSpell(ISkill spell) {
            if (Spells.ContainsKey(spell.ID)) {
                if (Spells[spell.ID].Level < spell.Level) {
                    Spells[spell.ID].Level = spell.Level;
                    Spells[spell.ID].Experience = spell.Experience;
                    spell.Send(this);
                }

                return false;
            }
            else {
                Spells.Add(spell.ID, spell);
                Spells[spell.ID].Available = false;
                spell.Send(this);
                SkillTable.SaveSpells(this);
                return true;
            }
        }

        public bool RemoveSpell(ISkill spell) {
            if (Spells.ContainsKey(spell.ID)) {
                Spells.Remove(spell.ID);
                Data data = new Data(true);
                data.UID = Entity.UID;
                data.dwParam = spell.ID;
                data.ID = 109;
                Send(data);
                SkillTable.DeleteSpell(this, spell.ID);
                return true;
            }

            return false;
        }

        public bool WentToComplete = false;
        public byte SelectedGem = 0;
        public Time32 LastMentorSave = Time32.Now;

        public void IncreaseExperience(ulong experience, bool addMultiple) {
            if (Entity.Dead) return;
            byte level = Entity.Level;
            ulong _experience = Entity.Experience;
            ulong prExperienece = experience;
            if (addMultiple) {
                if (Entity.VIPLevel > 0)
                    experience *= Entity.VIPLevel;
                experience *= GameConstants.ExtraExperienceRate;
                if (Entity.HeavenBlessing > 0)
                    experience += (uint)(experience * 20 / 100);
                if (Entity.Reborn >= 2)
                    experience /= 3;
                if (Entity.DoubleExperienceTime > 0 && SuperPotion > 0)
                    experience *= SuperPotion;

                if (Guild is { Level: > 0 }) {
                    experience += (ushort)(experience * Guild.Level / 100);
                }

                prExperienece = experience + (ulong)(experience * ((float)Entity.BattlePower / 100));
                _experience += prExperienece;

                _experience += (uint)(_experience * (uint)Entity.Gems[3] / 100);
            }
            else
                _experience += experience;

            if (Entity is { Level: < 140, Auto: true }) {
                Entity.autohuntxp += (_experience / 16);
                return;
            }
            else if (Entity is { Level: 140, Auto: true }) {
                Entity.autohuntxp = 0;
                return;
            }

            if (Entity.Level < 140) {
                while (_experience >= DataHolder.LevelExperience(level) && level < 140) {
                    _experience -= DataHolder.LevelExperience(level);
                    level++;
                    if (Entity.Reborn == 1) {
                        if (level >= 130 && Entity.FirstRebornLevel > 130 && level < Entity.FirstRebornLevel)
                            level = Entity.FirstRebornLevel;
                    }
                    else if (Entity.Reborn == 2) {
                        if (level >= 130 && Entity.SecondRebornLevel > 130 && level < Entity.SecondRebornLevel)
                            level = Entity.SecondRebornLevel;
                    }

                    if (Entity.Class is >= 10 and <= 15)
                        if (!Spells.ContainsKey(1110))
                            AddSpell(new Spell(true) { ID = 1110 });
                    if (Entity.Class is >= 20 and <= 25)
                        if (!Spells.ContainsKey(1020))
                            AddSpell(new Spell(true) { ID = 1020 });
                    if (Entity.Class is >= 40 and <= 45)
                        if (!Spells.ContainsKey(8002))
                            AddSpell(new Spell(true) { ID = 8002 });
                    if (Entity.Class is >= 50 and <= 55)
                        if (!Spells.ContainsKey(6011))
                            AddSpell(new Spell(true) { ID = 6011 });
                    if (Entity.Class is >= 60 and <= 65)
                        if (!Spells.ContainsKey(10490))
                            AddSpell(new Spell(true) { ID = 10490 });
                    if (Mentor is { IsOnline: true }) {
                        uint exExp = (uint)(level * 2);
                        Mentor.Client.PrizeExperience += exExp;
                        AsApprentice = Mentor.Client.Apprentices[Entity.UID];
                        if (AsApprentice != null) {
                            AsApprentice.Actual_Experience += exExp;
                            AsApprentice.Total_Experience += exExp;
                        }

                        if (Mentor.Client.PrizeExperience > 50 * 606)
                            Mentor.Client.PrizeExperience = 50 * 606;
                    }

                    if (level == 70) {
                        if (ArenaStatistic == null || ArenaStatistic.EntityID == 0) {
                            ArenaStatistic = new ArenaStatistic(true);
                            ArenaStatistic.EntityID = Entity.UID;
                            ArenaStatistic.Name = Entity.Name;
                            ArenaStatistic.Level = Entity.Level;
                            ArenaStatistic.Class = Entity.Class;
                            ArenaStatistic.Model = Entity.Mesh;
                            ArenaPoints = ArenaTable.ArenaPointFill(Entity.Level);
                            ArenaStatistic.LastArenaPointFill = DateTime.Now;
                            ArenaTable.InsertArenaStatistic(this);
                            ArenaStatistic.Status = ArenaStatistic.NotSignedUp;
                            Arena.ArenaStatistics.Add(Entity.UID, ArenaStatistic);
                        }
                    }

                    if (Entity.Reborn == 0) {
                        if (level <= 120) {
                            DataHolder.GetStats(Entity.Class, level, this);
                            CalculateStatBonus();
                            CalculateHPBonus();
                            GemAlgorithm();
                        }
                        else
                            Entity.Atributes += 3;
                    }
                    else {
                        Entity.Atributes += 3;
                    }
                }

                if (Entity.Level != level) {
                    if (Team != null) {
                        if (Team.LowestLevelsUID == Entity.UID) {
                            Team.LowestLevel = 0;
                            Team.LowestLevelsUID = 0;
                            Team.SearchForLowest();
                        }
                    }

                    Entity.Level = level;
                    Entity.Hitpoints = Entity.MaxHitpoints;
                    Entity.Mana = Entity.MaxMana;
                    if (Entity.Level > 130)
                        EntityTable.UpdateLevel(Entity.Owner);
                    if (Entity.Reborn == 2)
                        PacketHandler.ReincarnationHash(Entity.Owner);
                }

                if (Entity.Experience != _experience)
                    Entity.Experience = _experience;
            }
        }

        public void IncreaseSpellExperience(uint experience, ushort id) {
            if (Spells.ContainsKey(id)) {
                switch (id) {
                    case 1290:
                    case 5030:
                    case 7030:
                        experience = 100; break;
                }

                experience *= GameConstants.ExtraSpellRate;
                experience += (uint)(experience * Entity.Gems[6] / 100);
                if (Map.BaseID == 1039)
                    experience /= 40;
                ISkill spell = Spells[id];
                if (spell == null)
                    return;
                if (Entity.VIPLevel > 0) {
                    experience *= 5;
                }

                SpellInformation spellInfo = SpellTable.SpellInformations[spell.ID][spell.Level];
                if (spellInfo != null) {
                    if (spellInfo.NeedExperience != 0 && Entity.Level >= spellInfo.NeedLevel) {
                        spell.Experience += experience;
                        bool leveled = false;
                        if (spell.Experience >= spellInfo.NeedExperience) {
                            spell.Experience = 0;
                            spell.Level++;
                            leveled = true;
                            Send(GameConstants.SpellLeveled);
                        }

                        if (leveled) {
                            spell.Send(this);
                            SkillTable.SaveSpells(this); //Samak
                        }
                        else {
                            SkillExperience update = new SkillExperience(true);
                            update.AppendSpell(spell.ID, spell.Experience);
                            update.Send(this);
                            //Database.SkillTable.SaveSpells(this, spell.ID);//Samak Mohsen told men that no excperince any more after fixDatabase.EntityTable.UpdateSkillExp(this, spell.ID, experience);
                            EntityTable.UpdateSkillExp(this, spell.ID, experience);
                        }
                    }
                }
            }
        }

        public void IncreaseProficiencyExperience(uint experience, ushort id) {
            if (Proficiencies.ContainsKey(id)) {
                IProf proficiency = Proficiencies[id];
                experience *= GameConstants.ExtraProficiencyRate;
                experience += (uint)(experience * Entity.Gems[5] / 100);
                if (Map.BaseID == 1039)
                    experience /= 40;
                if (Entity.VIPLevel > 0) {
                    experience *= 5;
                }

                proficiency.Experience += experience;
                if (proficiency.Level < 20) {
                    bool leveled = false;
                    while (proficiency.Experience >= DataHolder.ProficiencyLevelExperience(proficiency.Level)) {
                        proficiency.Experience -= DataHolder.ProficiencyLevelExperience(proficiency.Level);
                        proficiency.Level++;
                        if (proficiency.Level == 20) {
                            proficiency.Experience = 0;
                            proficiency.Send(this);
                            Send(GameConstants.ProficiencyLeveled);
                            return;
                        }

                        proficiency.NeededExperience =
                            DataHolder.ProficiencyLevelExperience(proficiency.Level);
                        leveled = true;
                        Send(GameConstants.ProficiencyLeveled);
                    }

                    if (leveled) {
                        proficiency.Send(this);
                        //   Database.SkillTable.SaveProficiencies(this, proficiency.ID);//Samak
                    }
                    else {
                        SkillExperience update = new SkillExperience(true);
                        update.AppendProficiency(proficiency.ID, proficiency.Experience,
                            DataHolder.ProficiencyLevelExperience(proficiency.Level));
                        update.Send(this);
                    }
                    //Database.SkillTable.SaveProficiencies(this, proficiency.ID);//Samak XXXX
                }
            }
            else {
                AddProficiency(new Proficiency(true) { ID = id });
            }
        }

        public byte ExtraAtributePoints(byte level, byte mClass) {
            if (mClass == 135) {
                if (level <= 110)
                    return 0;
                switch (level) {
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
            else {
                if (level <= 120)
                    return 0;
                switch (level) {
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

        public static ISkill LearnableSpell(ushort spellid) {
            ISkill spell = new Spell(true);
            spell.ID = spellid;
            return spell;
        }

        public bool Reborn(byte toClass) {
            #region Items

            if (Inventory.Count > 37) return false;
            switch (toClass) {
                case 11:
                case 21:
                case 51:
                case 61:
                case 71: {
                    Inventory.Add(410077, Enums.ItemEffect.Poison);
                    break;
                }
                case 41: {
                    Inventory.Add(500057, Enums.ItemEffect.Shield);
                    break;
                }
                case 132:
                case 142: {
                    if (toClass == 132)
                        Inventory.Add(421077, Enums.ItemEffect.MP);
                    else
                        Inventory.Add(421077, Enums.ItemEffect.HP);
                    break;
                }
            }

            #region Low level items

            for (byte i = 1; i < 9; i++) {
                if (i != 7) {
                    ConquerItem item = Equipment.TryGetItem(i);
                    if (item != null && item.ID != 0) {
                        try {
                            //UnloadItemStats(item, false);
                            ConquerItemInformation cii =
                                new ConquerItemInformation(item.ID, item.Plus);
                            item.ID =
                                cii.LowestID(
                                    PacketHandler.ItemMinLevel(PacketHandler.ItemPosition(item.ID)));
                            item.Mode = Enums.ItemMode.Update;
                            item.Send(this);
                            LoadItemStats();
                            ConquerItemTable.UpdateItemID(item, this);
                        }
                        catch {
                            Console.WriteLine("Reborn item problem: " + item.ID);
                        }
                    }
                }
            }

            ConquerItem hand = Equipment.TryGetItem(5);
            if (hand != null) {
                Equipment.Remove(5);
                CalculateStatBonus();
                CalculateHPBonus();
            }

            hand = Equipment.TryGetItem(25);
            if (hand != null) {
                Equipment.Remove(25);
                CalculateStatBonus();
                CalculateHPBonus();
            }

            LoadItemStats();
            SendScreen(Entity.SpawnPacket, false);

            #endregion

            #endregion

            if (Entity.Reborn == 0) {
                Entity.FirstRebornClass = Entity.Class;
                Entity.FirstRebornLevel = Entity.Level;
                Entity.Atributes =
                    (ushort)(ExtraAtributePoints(Entity.FirstRebornClass, Entity.FirstRebornLevel) + 52);
            }
            else {
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

            ISkill[] spells = Spells.Values.ToArray();
            foreach (ISkill spell in spells) {
                spell.PreviousLevel = spell.Level;
                spell.Level = 0;
                spell.Experience = 0;

                #region KungFuKing

                if (PreviousClass == 85) {
                    if (Entity.Class != 81) {
                        switch (spell.ID) {
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

                if (PreviousClass == 75) {
                    if (Entity.Class != 71) {
                        switch (spell.ID) {
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

                if (PreviousClass == 65) {
                    if (Entity.Class != 61) {
                        switch (spell.ID) {
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

                if (PreviousClass == 165) {
                    if (Entity.Class != 161) {
                        switch (spell.ID) {
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

                if (PreviousClass == 25) {
                    if (Entity.Class != 21) {
                        switch (spell.ID) {
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

                if (toClass != 51) {
                    switch (spell.ID) {
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

                if (toClass != 11) {
                    switch (spell.ID) {
                        case 1115:
                        case 1130:
                            RemoveSpell(spell);
                            break;
                    }
                }

                #endregion

                #region Archer

                if (toClass != 41) {
                    switch (spell.ID) {
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

                if (PreviousClass == 135) {
                    if (toClass != 132) {
                        switch (spell.ID) {
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

                if (PreviousClass == 145) {
                    if (toClass != 142) {
                        switch (spell.ID) {
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
                    if (spell.ID != (ushort)Enums.SkillIDs.Reflect)
                        spell.Send(this);
            }

            #endregion

            #region Proficiencies

            foreach (IProf proficiency in Proficiencies.Values) {
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
            if (Entity is { FirstRebornClass: 85, SecondRebornClass: 85, Class: 81, Reborn: 2 })
                AddSpell(new Spell(true) { ID = 12300 });
            if (Entity is { FirstRebornClass: 15, SecondRebornClass: 15, Class: 11, Reborn: 2 })
                AddSpell(new Spell(true) { ID = 10315 });
            if (Entity is { FirstRebornClass: 25, SecondRebornClass: 25, Class: 21, Reborn: 2 })
                AddSpell(new Spell(true) { ID = 10311 });
            if (Entity is { FirstRebornClass: 45, SecondRebornClass: 45, Class: 41, Reborn: 2 })
                AddSpell(new Spell(true) { ID = 10313 });
            if (Entity is { FirstRebornClass: 55, SecondRebornClass: 55, Class: 51, Reborn: 2 })
                AddSpell(new Spell(true) { ID = 6003 });
            if (Entity is { FirstRebornClass: 65, SecondRebornClass: 65, Class: 61, Reborn: 2 })
                AddSpell(new Spell(true) { ID = 10405 });
            if (Entity is { FirstRebornClass: 135, SecondRebornClass: 135, Class: 132, Reborn: 2 })
                AddSpell(new Spell(true) { ID = 30000 });
            if (Entity is { FirstRebornClass: 145, SecondRebornClass: 145, Class: 142, Reborn: 2 })
                AddSpell(new Spell(true) { ID = 10310 });
            if (Entity.Reborn == 1) {
                if (Entity is { FirstRebornClass: 75, Class: 71 }) {
                    AddSpell(new Spell(true) { ID = 3050 });
                }

                if (Entity is { FirstRebornClass: 15, Class: 11 }) {
                    AddSpell(new Spell(true) { ID = 3050 });
                }
                else if (Entity is { FirstRebornClass: 25, Class: 21 }) {
                    AddSpell(new Spell(true) { ID = 3060 });
                }
                else if (Entity is { FirstRebornClass: 145, Class: 142 }) {
                    AddSpell(new Spell(true) { ID = 3080 });
                }
                else if (Entity is { FirstRebornClass: 135, Class: 132 }) {
                    AddSpell(new Spell(true) { ID = 3090 });
                }
            }

            if (Entity.Reborn == 2) {
                if (Entity is { SecondRebornClass: 75, Class: 71 }) {
                    AddSpell(new Spell(true) { ID = 3050 });
                }

                if (Entity is { SecondRebornClass: 15, Class: 11 }) {
                    AddSpell(new Spell(true) { ID = 3050 });
                }
                else if (Entity.SecondRebornClass == 25) {
                    AddSpell(new Spell(true) { ID = 3060 });
                }
                else if (Entity is { SecondRebornClass: 145, Class: 142 }) {
                    AddSpell(new Spell(true) { ID = 3080 });
                }
                else if (Entity is { SecondRebornClass: 135, Class: 132 }) {
                    AddSpell(new Spell(true) { ID = 3090 });
                }
            }

            #endregion

            #region Remove extra skills

            if (Entity.Reborn == 2) {
                #region Pison Star Del

                if (Entity is { SecondRebornClass: 55, Class: 41 }) {
                    RemoveSpell(new Spell(false) { ID = 6002 });
                    //   RemoveSpell(new Spell(false) { ID = 8001 });
                }

                if (Entity is { SecondRebornClass: 55, Class: 81 }) {
                    RemoveSpell(new Spell(false) { ID = 6002 });
                    RemoveSpell(new Spell(false) { ID = 8001 });
                }

                if (Entity is { SecondRebornClass: 55, Class: 11 }) {
                    RemoveSpell(new Spell(false) { ID = 6002 });
                    RemoveSpell(new Spell(false) { ID = 8001 });
                }

                if (Entity is { SecondRebornClass: 55, Class: 71 }) {
                    RemoveSpell(new Spell(false) { ID = 6002 });
                    RemoveSpell(new Spell(false) { ID = 8001 });
                }

                if (Entity is { SecondRebornClass: 55, Class: 61 }) {
                    RemoveSpell(new Spell(false) { ID = 6002 });
                    RemoveSpell(new Spell(false) { ID = 8001 });
                }

                if (Entity is { SecondRebornClass: 55, Class: 21 }) {
                    RemoveSpell(new Spell(false) { ID = 6002 });
                    RemoveSpell(new Spell(false) { ID = 8001 });
                }
                else if (Entity is { SecondRebornClass: 55, Class: 142 }) {
                    RemoveSpell(new Spell(false) { ID = 6002 });
                    RemoveSpell(new Spell(false) { ID = 8001 });
                }
                else if (Entity is { SecondRebornClass: 55, Class: 132 }) {
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

            DataHolder.GetStats(Entity.Class, Entity.Level, this);
            CalculateStatBonus();
            CalculateHPBonus();
            GemAlgorithm();
            using (var conn = DataHolder.MySqlConnection) {
                conn.Open();
                EntityTable.SaveEntity(this, conn);
                //Samak Database.SkillTable.SaveSpells(this, conn);
                //Samak Database.SkillTable.SaveProficiencies(this, conn);
                SkillTable.SaveSpells(this);
                SkillTable.SaveProficiencies(this);
            }

            Kernel.SendWorldMessage(
                new Message("" + Entity.Name + " has got " + Entity.Reborn + " reborns. Congratulations!",
                    Color.White, Message.Center), Program.Values);
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

        public int Accuracy {
            get {
                if (Entity.EntityFlag == EntityFlag.Monster)
                    return 0;
                int _accuracy = 0;
                foreach (ConquerItem i in Equipment.Objects) {
                    if (i == null) {
                        continue;
                    }

                    if (i.Position == ConquerItem.LeftWeapon ||
                        i.Position == ConquerItem.RightWeapon) {
                        ConquerItemInformation dbi = new ConquerItemInformation(i.ID, i.Plus);
                        if (dbi != null) {
                            _accuracy += dbi.PlusInformation.Agility;
                        }
                    }
                }

                return _accuracy;
            }
        }

        public ushort AgilityItem {
            get {
                if (Entity.EntityFlag == EntityFlag.Monster)
                    return 0;
                ushort _AgilityItem = 0;
                foreach (ConquerItem i in Equipment.Objects) {
                    if (i == null) {
                        continue;
                    }

                    ConquerItemInformation dbi = new ConquerItemInformation(i.ID, i.Plus);
                    if (dbi != null) {
                        _AgilityItem += dbi.BaseInformation.Frequency;
                    }
                }

                return _AgilityItem;
            }
        }

        public ushort MagicDefence {
            get {
                if (Entity.EntityFlag == EntityFlag.Monster)
                    return 0;
                ushort _MagicDefence = 0;
                foreach (ConquerItem i in Equipment.Objects) {
                    if (i == null) {
                        continue;
                    }

                    if (i.Position == ConquerItem.Armor ||
                        i.Position == ConquerItem.Necklace ||
                        i.Position == ConquerItem.Head) {
                        ConquerItemInformation dbi = new ConquerItemInformation(i.ID, i.Plus);
                        if (dbi != null) {
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

        public uint GetArsenalDonation() {
            uint val = 0;
            foreach (var Uint in ArsenalDonations)
                val += Uint;

            if (AsMember != null) {
                AsMember.ArsenalDonation = val;
                Game.Features.Guilds.Database.GuildMemberTable.Save(AsMember);
            }
            return val;
        }

        public void CalculateHPBonus() {
            //  if ((int)Account.State >= 3) return;
            switch (Entity.Class) {
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

        public void CalculateStatBonus() {
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

        public void SendStatMessage() {
            ReviewMentor();
            Message Msg = new Message(" Your status has been changed",
                Color.DarkGoldenrod
                , Message.TopLeft);
            Msg.__Message = string.Format(Msg.__Message,
                new object[] {
                    Entity.MinAttack, Entity.MaxAttack, Entity.MagicAttack, Entity.Defence,
                    (Entity.MagicDefence + Entity.MagicDefence), Entity.Dodge, Entity.PhysicalDamageDecrease,
                    Entity.MagicDamageDecrease, Entity.PhysicalDamageIncrease, Entity.MagicDamageIncrease,
                    Entity.Hitpoints, Entity.MaxHitpoints, Entity.Mana, Entity.MaxMana, Entity.BattlePower
                });
            Send(Msg);
        }

        private bool AreStatsLoadable(ConquerItem item) {
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

        private Tuple<ConquerItem, ConquerItem> ComputeWeapons() {
            if (!AlternateEquipment) {
                return new Tuple<ConquerItem, ConquerItem>(
                    Equipment.TryGetItem(ConquerItem.RightWeapon),
                    Equipment.TryGetItem(ConquerItem.LeftWeapon));
            }
            else {
                if (Equipment.Free(ConquerItem.AlternateRightWeapon)) {
                    return new Tuple<ConquerItem, ConquerItem>(
                        Equipment.TryGetItem(ConquerItem.RightWeapon),
                        Equipment.TryGetItem(ConquerItem.LeftWeapon));
                }
                else {
                    if (Equipment.Free(ConquerItem.RightWeapon)) {
                        return new Tuple<ConquerItem, ConquerItem>(
                            Equipment.TryGetItem(ConquerItem.AlternateRightWeapon),
                            Equipment.TryGetItem(ConquerItem.AlternateLeftWeapon));
                    }
                    else {
                        if (!Equipment.Free(ConquerItem.AlternateLeftWeapon)) {
                            return new Tuple<ConquerItem, ConquerItem>(
                                Equipment.TryGetItem(ConquerItem.AlternateRightWeapon),
                                Equipment.TryGetItem(ConquerItem.AlternateLeftWeapon));
                        }
                        else {
                            if (Equipment.Free(ConquerItem.LeftWeapon)) {
                                return new Tuple<ConquerItem, ConquerItem>(
                                    Equipment.TryGetItem(ConquerItem.AlternateRightWeapon),
                                    null);
                            }
                            else {
                                ConquerItem aRight = Equipment.TryGetItem(ConquerItem.AlternateRightWeapon),
                                    nLeft = Equipment.TryGetItem(ConquerItem.LeftWeapon);
                                if (PacketHandler.IsTwoHand(aRight.ID)) {
                                    if (PacketHandler.IsFranko(nLeft.ID)) {
                                        if (PacketHandler.IsBow(aRight.ID)) {
                                            return new Tuple<ConquerItem,
                                                ConquerItem>(aRight, nLeft);
                                        }
                                        else {
                                            return new Tuple<ConquerItem,
                                                ConquerItem>(aRight, null);
                                        }
                                    }
                                    else {
                                        if (PacketHandler.IsShield(nLeft.ID)) {
                                            if (!Spells.ContainsKey(10311)) //Perseverance
                                            {
                                                Send(new Message(
                                                    "You need to know Perseverance (Pure Warrior skill) to be able to wear 2-handed weapon and shield.",
                                                    Color.Red, Message.Talk));
                                                return new Tuple<ConquerItem,
                                                    ConquerItem>(aRight, null);
                                            }
                                            else {
                                                return new Tuple<ConquerItem,
                                                    ConquerItem>(aRight, nLeft);
                                            }
                                        }
                                        else {
                                            return new Tuple<ConquerItem,
                                                ConquerItem>(aRight, null);
                                        }
                                    }
                                }
                                else {
                                    if (!PacketHandler.IsTwoHand(nLeft.ID)) {
                                        return new Tuple<ConquerItem,
                                            ConquerItem>(aRight, nLeft);
                                    }
                                    else {
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

        public int[][] ChampionAllowedStats = new int[][] {
            [1, 0, 0, 0, 0, 0, 0, 30, 0, 0],
            [2, 0, 0, 0, 0, 0, 0, 40, 1, 1],
            [3, 1, 0, 0, 0, 0, 50, 50, 2, 3],
            [4, 3, 1, 1, 0, 0, 100, 60, 5, 4],
            [5, 5, 1, 1, 1, 0, 150, 70, 7, 5],
            [6, 5, 1, 1, 1, 0, 200, 80, 9, 7],
            [12, 7, 2, 2, 1, 1, 255, 100, 12, 9]
        };

        public bool DoChampStats {
            get { return ChampionGroup != null; }
        }

        //private int _accuracy;
        //public int Accuracy
        //{
        //    get { return _accuracy; }
        //}
        public void LoadItemStats() {
            uint bStats = Entity.Hitpoints;
            for (int i = 0; i < 29; i++)
                if (Equipment.Objects[i] != null)
                    Equipment.Objects[i].IsWorn = false;
            //if (Team != null)
            //    Team.GetClanShareBp(this);
            //CalculateStatBonus();

            #region Hack Points

            var Asheetos = Entity.Agility + Entity.Strength + Entity.Spirit + Entity.Vitality + Entity.Atributes;
            if (Asheetos > 538) {
                Entity.Agility = 0;
                Entity.Strength = 0;
                Entity.Spirit = 0;
                Entity.Vitality = 0;
                Entity.Atributes = 538;
                EntityTable.SaveEntity(this);
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
                    Entity.AttackRange = 0;
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

            foreach (ConquerItem i in Equipment.Objects) {
                if (i == null) continue;
                if (i.Durability == 0) continue;
                if (!AreStatsLoadable(i)) continue;
                loadItemStats(i);
            }

            Weapons = ComputeWeapons();
            if (Weapons == null) Weapons = new Tuple<ConquerItem, ConquerItem>(null, null);
            if (Weapons.Item1 != null) {
                loadItemStats(Weapons.Item1);
                if (Weapons.Item2 != null) {
                    if (!Weapons.Item1.IsTwoHander())
                        loadItemStats(Weapons.Item2);
                    else if (PacketHandler.IsFranko(Weapons.Item2.ID) || Entity.Class is >= 20 and <= 25)
                        loadItemStats(Weapons.Item2);
                }
            }

            if (Entity.SubClasses != null)
                Entity.SubClasses.UpgradeStatus(this, false);

            #region Chi

            uint percentage = 100;
            if (DoChampStats)
                percentage = (uint)ChampionAllowedStats[ChampionStats.Grade][7];
            foreach (var chiPower in ChiPowers) {
                foreach (var attribute in chiPower.Attributes) {
                    switch (attribute.Type) {
                        case Enums.ChiAttribute.PStrike:
                            Entity.CriticalStrike += (int)((ushort)(attribute.Value * 10) * percentage / 100);
                            break;
                        case Enums.ChiAttribute.Counteraction:
                            Entity.Counteraction += (ushort)(attribute.Value * percentage / 100);
                            break;
                        case Enums.ChiAttribute.PAttack:
                            Entity.BaseMinAttack += attribute.Value * percentage / 100;
                            Entity.BaseMaxAttack += attribute.Value * percentage / 100;
                            break;
                        case Enums.ChiAttribute.MAttack:
                            Entity.BaseMagicAttack += attribute.Value * percentage / 100;
                            break;
                        case Enums.ChiAttribute.MDefense:
                            Entity.BaseMagicDefence += attribute.Value * percentage / 100;
                            break;
                        case Enums.ChiAttribute.Break:
                            Entity.Breaktrough += (ushort)(attribute.Value * percentage / 100);
                            break;
                        case Enums.ChiAttribute.MaxHP:
                            Entity.ItemHP += attribute.Value * percentage / 100;
                            break;
                        case Enums.ChiAttribute.Immunity:
                            Entity.Immunity += (int)((ushort)(attribute.Value * 10) * percentage / 100);
                            break;
                        case Enums.ChiAttribute.FinalMDamage:
                            Entity.MagicDamageDecrease += (ushort)(attribute.Value * percentage / 100);
                            break;
                        case Enums.ChiAttribute.FinalMAttack:
                            Entity.MagicDamageIncrease += (ushort)(attribute.Value * percentage / 100);
                            break;
                        case Enums.ChiAttribute.FinalPDamage:
                            Entity.PhysicalDamageDecrease += (ushort)(attribute.Value * percentage / 100);
                            break;
                        case Enums.ChiAttribute.FinalPAttack:
                            Entity.PhysicalDamageIncrease += 1;
                            break;
                        case Enums.ChiAttribute.MStrike:
                            Entity.SkillCStrike += (int)((ushort)(attribute.Value * 10) * percentage / 100);
                            break;
                    }
                }
            }

            #region Dragon Ranking

            if (ChiData.DragonRank <= 3000 && ChiPowers.Count > 0) {
                Entity.ItemHP += 5000;
                Entity.BaseMagicDefence += 300;
                Entity.PhysicalDamageDecrease += 1000;
                Entity.MagicDamageDecrease += 300;
            }

            #endregion

            #region Phoenix Ranking

            if (ChiData.PhoenixRank <= 3000 && ChiPowers.Count > 1) {
                Entity.BaseMinAttack += 3000;
                Entity.BaseMaxAttack += 3000;
                Entity.BaseMagicAttack += 3000;
                Entity.PhysicalDamageIncrease += 1;
                Entity.MagicDamageIncrease += 300;
            }

            #endregion

            #region Tiger Ranking

            if (ChiData.TigerRank <= 3000 && ChiPowers.Count > 2) {
                Entity.CriticalStrike += 1500;
                Entity.SkillCStrike += 1500;
                Entity.Immunity += 800;
            }

            #endregion

            #region Turtle Ranking

            if (ChiData.TurtleRank <= 3000 && ChiPowers.Count > 3) {
                Entity.Breaktrough += 150;
                Entity.Counteraction += 150;
                Entity.Immunity += 800;
            }

            #endregion

            #endregion

            #region Vip 6

            if (Entity.VIPLevel == 6) {
                Entity expr_1951 = Entity;
                expr_1951.BaseMinAttack += 2000;
                Entity expr_1952 = Entity;
                expr_1951.BaseMaxAttack += 2000;
                Entity.ItemHP += 2000u;
                Entity.CriticalStrike += 400;
                Entity.Immunity += 400;
                Entity expr_1950 = Entity;
                expr_1950.Defence += 2000;
            }

            #endregion

            if (Entity.Aura_isActive)
                doAuraBonuses(Entity.Aura_actType, Entity.Aura_actPower, 1);
            else
                removeAuraBonuses(Entity.Aura_actType, Entity.Aura_actPower, 1);
            //if (TeamAura)
            //    doAuraBonuses(TeamAuraStatusFlag, TeamAuraPower, 1);
            //else
            //    removeAuraBonuses(TeamAuraStatusFlag, TeamAuraPower, 1);
            foreach (var Aura in Auras.Values) {
                doAuraBonuses(Aura.TeamAuraStatusFlag, Aura.TeamAuraPower, 1);
            }

            if (Entity.Class is >= 60 and <= 65)
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

        public void doAuraBonuses(ulong type, uint power, int i) {
            switch (type) {
                case Update.Flags2.EarthAura: Entity.EarthResistance += (int)power * i; break;
                case Update.Flags2.FireAura: Entity.FireResistance += (int)power * i; break;
                case Update.Flags2.MetalAura: Entity.MetalResistance += (int)power * i; break;
                case Update.Flags2.WoodAura: Entity.WoodResistance += (int)power * i; break;
                case Update.Flags2.WaterAura: Entity.WaterResistance += (int)power * i; break;
                case Update.Flags2.TyrantAura: {
                    Entity.CriticalStrike += (int)power * i * 100;
                    Entity.SkillCStrike += (int)power * i * 100;
                    if (Entity.CriticalStrike > 120000) Entity.CriticalStrike = 120000;
                    if (Entity.SkillCStrike > 120000) Entity.SkillCStrike = 120000;
                    if (Entity.CriticalStrike < 0) Entity.CriticalStrike = 0;
                    if (Entity.SkillCStrike < 0) Entity.SkillCStrike = 0;
                    break;
                }
                case Update.Flags2.FendAura: Entity.Immunity += (int)power * i * 100; break;
            }
        }

        public void removeAuraBonuses(ulong type, uint power, int i) {
            switch (type) {
                case Update.Flags2.EarthAura: Entity.EarthResistance -= (int)power * i; break;
                case Update.Flags2.FireAura: Entity.FireResistance -= (int)power * i; break;
                case Update.Flags2.MetalAura: Entity.MetalResistance -= (int)power * i; break;
                case Update.Flags2.WoodAura: Entity.WoodResistance -= (int)power * i; break;
                case Update.Flags2.WaterAura: Entity.WaterResistance -= (int)power * i; break;
                case Update.Flags2.TyrantAura: {
                    Entity.CriticalStrike -= (int)power * i * 100;
                    Entity.SkillCStrike -= (int)power * i * 100;
                    if (Entity.CriticalStrike > 120000) Entity.CriticalStrike = 120000;
                    if (Entity.SkillCStrike > 120000) Entity.SkillCStrike = 120000;
                    if (Entity.CriticalStrike < 0) Entity.CriticalStrike = 0;
                    if (Entity.SkillCStrike < 0) Entity.SkillCStrike = 0;
                    break;
                }
                case Update.Flags2.FendAura: Entity.Immunity -= (int)power * i * 100; break;
            }
        }

        private void CalculateVigor(ConquerItem item, ConquerItemInformation dbi) {
            if (!Equipment.Free(12)) {
                if (!Entity.ContainsFlag2(Update.Flags.Ride)) {
                    Vigor = 0;
                    MaxVigor = 0;
                    MaxVigor += dbi.PlusInformation.Agility;
                    MaxVigor += 30;
                    if (!Equipment.Free(ConquerItem.SteedCrop)) {
                        if (Equipment.Objects[17] != null) {
                            if (Equipment.Objects[17].ID % 10 == 9) {
                                MaxVigor += 1000;
                            }
                            else if (Equipment.Objects[17].ID % 10 == 8) {
                                MaxVigor += 700;
                            }
                            else if (Equipment.Objects[17].ID % 10 == 7) {
                                MaxVigor += 500;
                            }
                            else if (Equipment.Objects[18].ID % 10 == 6) {
                                MaxVigor += 300;
                            }
                            else if (Equipment.Objects[18].ID % 10 == 5) {
                                MaxVigor += 100;
                            }
                        }
                    }

                    Vigor = MaxVigor;
                }
            }
        }

        private void loadItemStats(ConquerItem item) {
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
            ConquerItemInformation dbi = new ConquerItemInformation(item.ID, item.Plus);
            if (dbi != null) {
                #region Star

                Entity.PerfectionLevel += item.Perfectionlevel;

                if (item.Perfectionlevel is > 3 and < 7) {
                    Entity.BaseMinAttack += 100 / 12;
                    Entity.BaseMaxAttack += 100 / 12;
                    Entity.BaseMagicAttack += 300 / 12;
                }

                if (item.Perfectionlevel is > 7 and < 10) {
                    Entity.BaseMinAttack += 100 / 12;
                    Entity.BaseMaxAttack += 100 / 12;
                    Entity.BaseDefence += 100 / 12;
                    Entity.BaseMagicAttack += 300 / 12;
                    Entity.MagicDefence += 100 / 12;
                }

                if (item.Perfectionlevel is > 10 and < 14) {
                    Entity.BaseMinAttack += 300 / 12;
                    Entity.BaseMaxAttack += 300 / 12;
                    Entity.BaseDefence += 300 / 12;
                    Entity.BaseMagicAttack += 600 / 12;
                    Entity.MagicDefence += 150 / 12;
                }

                if (item.Perfectionlevel is > 14 and < 17) {
                    Entity.BaseMinAttack += 500 / 12;
                    Entity.BaseMaxAttack += 500 / 12;
                    Entity.BaseDefence += 500 / 12;
                    Entity.BaseMagicAttack += 1000 / 12;
                    Entity.MagicDefence += 250 / 12;
                }

                if (item.Perfectionlevel is > 17 and < 25) {
                    Entity.BaseMinAttack += 800 / 12;
                    Entity.BaseMaxAttack += 800 / 12;
                    Entity.BaseDefence += 1200 / 12;
                    Entity.BaseMagicAttack += 1500 / 12;
                    Entity.MagicDefence += 500 / 12;
                }

                if (item.Perfectionlevel is > 25 and < 28) {
                    Entity.BaseMinAttack += 1200 / 12;
                    Entity.BaseMaxAttack += 1200 / 12;
                    Entity.BaseDefence += 1200 / 12;
                    Entity.BaseMagicAttack += 2000 / 12;
                    Entity.MagicDefence += 500 / 12;
                }

                if (item.Perfectionlevel is > 28 and < 32) {
                    Entity.BaseMinAttack += 1600 / 12;
                    Entity.BaseMaxAttack += 1600 / 12;
                    Entity.BaseDefence += 1600 / 12;
                    Entity.BaseMagicAttack += 2500 / 12;
                    Entity.MagicDefence += 625 / 12;
                }

                if (item.Perfectionlevel is > 32 and < 55) {
                    Entity.BaseMinAttack += 3000 / 12;
                    Entity.BaseMaxAttack += 3000 / 12;
                    Entity.BaseDefence += 3000 / 12;
                    Entity.BaseMagicAttack += 4000 / 12;
                    Entity.MagicDefence += 1000 / 12;
                }

                #endregion

                #region Give Stats.

                #region Garment

                if (position == ConquerItem.Garment) {
                    if (item.ID == 188925) {
                        Entity.CriticalStrike += 100;
                        Entity.SkillCStrike += 100;
                        Entity.Immunity += 100;
                    }
                    else if (item.ID == 187425) {
                        Entity.BaseDefence += 400;
                        Entity.BaseMagicDefence += 2;
                    }
                    else if (item.ID == 187415) {
                        Entity.BaseDefence += 600;
                        Entity.BaseMagicDefence += 3;
                    }
                    else if (item.ID == 187405) {
                        Entity.BaseDefence += 800;
                        Entity.BaseMagicDefence += 4;
                    }
                    else if (item.ID == 188935) {
                        Entity.CriticalStrike += 200;
                        Entity.SkillCStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 188945) {
                        Entity.CriticalStrike += 300;
                        Entity.SkillCStrike += 300;
                        Entity.Immunity += 300;
                    }
                    else if (item.ID == 188955) {
                        Entity.CriticalStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 192745) {
                        Entity.CriticalStrike += 100;
                        Entity.SkillCStrike += 100;
                        Entity.Immunity += 100;
                    }
                    else if (item.ID == 192755) {
                        Entity.CriticalStrike += 200;
                        Entity.SkillCStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 192765) {
                        Entity.CriticalStrike += 300;
                        Entity.SkillCStrike += 300;
                        Entity.Immunity += 300;
                    }
                    else if (item.ID == 192775) {
                        Entity.CriticalStrike += 400;
                        Entity.SkillCStrike += 400;
                        Entity.Immunity += 400;
                    }
                    else if (item.ID == 192805) {
                        Entity.CriticalStrike += 100;
                        Entity.Immunity += 100;
                    }
                    else if (item.ID == 192815) {
                        Entity.CriticalStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 192825) {
                        Entity.CriticalStrike += 300;
                        Entity.Immunity += 300;
                    }
                    else if (item.ID == 192935) {
                        Entity.CriticalStrike += 200;
                        Entity.SkillCStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 192925) {
                        Entity.CriticalStrike += 200;
                        Entity.SkillCStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 192895) {
                        Entity.CriticalStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 188845) {
                        Entity.CriticalStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 188755) {
                        Entity.CriticalStrike += 200;
                        Entity.SkillCStrike += 200;
                    }
                    else if (item.ID == 188515) {
                        Entity.CriticalStrike += 400;
                        Entity.SkillCStrike += 400;
                        Entity.Immunity += 400;
                    }
                    else if (item.ID == 187875) {
                        Entity.CriticalStrike += 100;
                    }
                    else if (item.ID == 187885) {
                        Entity.SkillCStrike += 100;
                    }
                    else if (item.ID == 187865) {
                        Entity.SkillCStrike += 200;
                    }
                    else if (item.ID == 187855) {
                        Entity.CriticalStrike += 200;
                    }
                    else if (item.ID == 187795) {
                        Entity.CriticalStrike += 300;
                        Entity.SkillCStrike += 300;
                        Entity.Immunity += 300;
                    }
                    else if (item.ID == 187785) {
                        Entity.CriticalStrike += 200;
                        Entity.SkillCStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 187775) {
                        Entity.CriticalStrike += 100;
                        Entity.SkillCStrike += 100;
                        Entity.Immunity += 100;
                    }
                }

                #endregion

                #region MountArmor

                if (position == ConquerItem.SteedArmor) {
                    if (item.ID == 200221) {
                        Entity.CriticalStrike += 300;
                        Entity.SkillCStrike += 300;
                        Entity.Immunity += 300;
                    }
                    else if (item.ID == 200480) {
                        Entity.CriticalStrike = 200;
                        Entity.SkillCStrike = 200;
                        Entity.Immunity = 200;
                    }
                    else if (item.ID == 200021) {
                        Entity.CriticalStrike = 100;
                        Entity.SkillCStrike = 50;
                        Entity.Immunity = 100;
                    }
                    else if (item.ID == 200022) {
                        Entity.CriticalStrike = 200;
                        Entity.SkillCStrike = 100;
                        Entity.Immunity = 200;
                    }
                    else if (item.ID == 200220) {
                        Entity.CriticalStrike += 200;
                        Entity.SkillCStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 200486) {
                        Entity.CriticalStrike += 100;
                        Entity.Immunity += 100;
                    }
                    else if (item.ID == 200485) {
                        Entity.CriticalStrike += 200;
                        Entity.Immunity += 200;
                    }
                    else if (item.ID == 200479) {
                        Entity.CriticalStrike += 100;
                        Entity.SkillCStrike += 100;
                    }
                    else if (item.ID == 200478) {
                        Entity.CriticalStrike += 100;
                        Entity.SkillCStrike += 100;
                        Entity.Immunity += 100;
                    }
                    else if (item.ID == 200477) {
                        Entity.CriticalStrike += 200;
                        Entity.SkillCStrike += 200;
                    }
                    else if (item.ID == 200475) {
                        Entity.CriticalStrike += 200;
                        Entity.SkillCStrike += 200;
                        Entity.Immunity += 200;
                    }
                }

                #endregion

                #region Cups State

                if (position == ConquerItem.Bottle) {
                    if (item.ID == 2100075) {
                        Entity.Breaktrough += 30;
                        Entity.Counteraction += 30;
                        Entity.CriticalStrike += 300;
                        Entity.Immunity += 300;
                    }
                }

                #endregion

                #region soul stats

                if (DoChampStats && ChampionAllowedStats[ChampionStats.Grade][5] == 1 || !DoChampStats) {
                    if (item.Purification.PurificationItemID != 0) {
                        ConquerItemInformation soulDB =
                            new ConquerItemInformation(item.Purification.PurificationItemID, 0);
                        if (position == ConquerItem.LeftWeapon) {
                            Entity.BaseMinAttack += (uint)(soulDB.BaseInformation.MinAttack / 2);
                            Entity.BaseMaxAttack += (uint)(soulDB.BaseInformation.MaxAttack / 2);
                        }
                        else {
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

                if (DoChampStats && ChampionAllowedStats[ChampionStats.Grade][4] == 1 || !DoChampStats) {
                    Refinery.RefineryItem refine = null;
                    if (item.ExtraEffect.Available) {
                        if (Kernel.DatabaseRefinery.TryGetValue(item.ExtraEffect.EffectID, out refine)) {
                            if (refine != null) {
                                switch (refine.Type) {
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

                if (position == ConquerItem.Tower) {
                    Entity.PhysicalDamageDecrease += dbi.BaseInformation.PhysicalDefence;
                    Entity.MagicDamageDecrease += dbi.BaseInformation.MagicDefence;
                }
                else if (position == ConquerItem.Fan) {
                    Entity.PhysicalDamageIncrease += dbi.BaseInformation.MaxAttack;
                    Entity.MagicDamageIncrease += dbi.BaseInformation.MagicAttack;
                }
                else {
                    if (position == ConquerItem.LeftWeapon) {
                        Entity.BaseMinAttack += (uint)dbi.BaseInformation.MinAttack / 2;
                        Entity.BaseMaxAttack += (uint)dbi.BaseInformation.MaxAttack / 2;
                    }
                    else {
                        if (position == ConquerItem.RightWeapon) {
                            Entity.AttackRange += dbi.BaseInformation.AttackRange;
                            if (PacketHandler.IsTwoHand(dbi.BaseInformation.ID))
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

                if (position == ConquerItem.Steed) {
                    CalculateVigor(item, dbi);
                }

                Entity.ItemHP += dbi.BaseInformation.ItemHP;
                Entity.ItemMP += dbi.BaseInformation.ItemMP;
                Entity.Dodge += dbi.BaseInformation.Dodge;
                Entity.Dexterity += dbi.BaseInformation.Frequency;
                Entity.Weight += dbi.BaseInformation.Weight;
                if (item.Position != ConquerItem.Steed) {
                    if (DoChampStats)
                        Entity.ItemBless -=
                            (ushort)Math.Min(item.Bless / 100, ChampionAllowedStats[ChampionStats.Grade][1]);
                    else
                        Entity.ItemBless -= ((double)item.Bless / 100);
                }


                var gem = (int)item.SocketOne;
                if (gem != 0 && gem != 255)
                    Entity.Gems[gem / 10] += GemTypes.Effects[gem / 10][gem % 10];

                gem = (int)item.SocketTwo;
                if (gem != 0 && gem != 255)
                    Entity.Gems[gem / 10] += GemTypes.Effects[gem / 10][gem % 10];

                if (item.Plus > 0) {
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
                Entity.CriticalStrike += dbi.BaseInformation.CriticalStrike / per;
                Entity.SkillCStrike += dbi.BaseInformation.SkillCriticalStrike / per;
                Entity.Immunity += dbi.BaseInformation.Immunity / per;
                Entity.Penetration += dbi.BaseInformation.Penetration / per;
                Entity.Block += dbi.BaseInformation.Block / per;
                Entity.Breaktrough += dbi.BaseInformation.BreakThrough / per2;
                Entity.Counteraction += dbi.BaseInformation.CounterAction / per2;
                Entity.MetalResistance += dbi.BaseInformation.MetalResist;
                Entity.WoodResistance += dbi.BaseInformation.WoodResist;
                Entity.WaterResistance += dbi.BaseInformation.WaterResist;
                Entity.FireResistance += dbi.BaseInformation.FireResist;
                Entity.EarthResistance += dbi.BaseInformation.EarthResist;

                #endregion
            }
        }

        public void GemAlgorithm() {
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

        public void Summon2() {
            try {
                List<Point> DestructionAreas = [];
                for (int i = 0; i < 360; i++) {
                    ushort x = (ushort)(Circle_Center.X + (Circle_Level * Math.Cos(i)));
                    ushort y = (ushort)(Circle_Center.Y + (Circle_Level * Math.Sin(i)));
                    Point p = new Point(x, y);
                    if (!DestructionAreas.Contains(p))
                        DestructionAreas.Add(p);
                }

                foreach (Point p in DestructionAreas) {
                    _String str = new _String(true);
                    str.TextsCount = 1;
                    str.PositionX = (ushort)p.X;
                    str.PositionY = (ushort)p.Y;
                    str.Type = _String.MapEffect;
                    str.Texts.Add(circle_Effect);
                    SendScreen(str);


                    var spell = SpellTable.GetSpell(11600, this);

                    var attack = new Attack(true);
                    attack.Attacker = Entity.UID;
                    attack.AttackType = Attack.Melee;

                    foreach (var obj1 in Screen.Objects) {
                        if (Kernel.GetDistance(obj1.X, obj1.Y, (ushort)p.X, (ushort)p.Y) <= 3) {
                            if (obj1.MapObjType == MapObjectType.Monster || obj1.MapObjType == MapObjectType.Player) {
                                var attacked = obj1 as Entity;
                                if (Handle.CanAttack(Entity, attacked, spell, false)) {
                                    uint damage = Calculate.Melee(Entity, attacked, spell, ref attack);

                                    attack.Damage = damage;
                                    attack.Attacked = attacked.UID;
                                    attack.X = attacked.X;
                                    attack.Y = attacked.Y;

                                    Handle.ReceiveAttack(Entity, attacked, attack, ref damage,
                                        spell);
                                }
                            }
                            else if (obj1.MapObjType == MapObjectType.SobNpc) {
                                var attacked = obj1 as SobNpcSpawn;
                                if (Handle.CanAttack(Entity, attacked, spell)) {
                                    uint damage = Calculate.Melee(Entity, attacked, ref attack);
                                    attack.Damage = damage;
                                    attack.Attacked = attacked.UID;
                                    attack.X = attacked.X;
                                    attack.Y = attacked.Y;

                                    Handle.ReceiveAttack(Entity, attacked, attack, damage, spell);
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

        public static GameState CharacterFromName(string name) {
            foreach (GameState c in Kernel.GamePool.Values)
                if (c.Entity.Name == name)
                    return c;
            return null;
        }

        public static GameState CharacterFromName2(string Name) {
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
    }
}
