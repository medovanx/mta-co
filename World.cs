using MTA.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using MTA.Network.GamePackets;
using System.Threading;
using System.Threading.Generic;
using MTA.Network.Sockets;
using MTA.Game.ConquerStructures;
using MTA.Client;
using System.Drawing;
using MTA.Franko;
using MTA.Game.Features.Tournaments;
using MTA.Game.Npcs.ScriptEngine;
using MTA.Network.GamePackets.EventAlert;

namespace MTA {
    public class World {
        /// <summary>
        /// The script engine for npcs.
        /// </summary>
        /// 
        public static ScriptEngine? ScriptEngine;

        public static long Carnaval = 0;

        #region Cyclone War

        public static bool _cyclone3;
        public static bool _cyclone1;
        public static bool Cyclone = false;

        public static bool LastTeam = false;

        #endregion Cyclone War

        public static StaticPool? GenericThreadPool;
        public static StaticPool? ReceivePool;
        public static StaticPool? SendPool;
        private TimerRule<GameState> _buffers;
        public TimerRule<GameState> Characters, AutoAttack, Prayer;
        public TimerRule<ClientWrapper> ConnectionReceive, ConnectionReview, ConnectionSend;

        public const uint
            NobilityMapBase = 700,
            ClassPkMapBase = 1730;

        public List<KillTournament> Tournaments;

        public PoleDomination PoleDomination;
        public CaptureTheFlag Ctf;
        private bool _clanWarAi;
        public bool PureLand, MonthlyPkWar;
        public HeroOfGame HeroOfGame;
        public DelayedTask DelayedTask;
        public DateTime MonthlyPkDate;
        public DateTime NextMonthlyPkDate;

        public World() {
            GenericThreadPool = new StaticPool().Run();
            ReceivePool = new StaticPool(128).Run();
            SendPool = new StaticPool().Run();
        }

        public World(HeroOfGame heroOfGame, bool monthlyPkWar, TimerRule<GameState> buffers,
            TimerRule<GameState> characters, TimerRule<GameState> autoAttack, TimerRule<GameState> prayer,
            TimerRule<ClientWrapper> connectionReceive, TimerRule<ClientWrapper> connectionReview,
            TimerRule<ClientWrapper> connectionSend, List<KillTournament> tournaments, PoleDomination poleDomination,
            CaptureTheFlag ctf, DelayedTask delayedTask) {
            HeroOfGame = heroOfGame;
            MonthlyPkWar = monthlyPkWar;
            _buffers = buffers;
            Characters = characters;
            AutoAttack = autoAttack;
            Prayer = prayer;
            ConnectionReceive = connectionReceive;
            ConnectionReview = connectionReview;
            ConnectionSend = connectionSend;
            Tournaments = tournaments;
            PoleDomination = poleDomination;
            Ctf = ctf;
            DelayedTask = delayedTask;
            GenericThreadPool = new StaticPool().Run();
            ReceivePool = new StaticPool(128).Run();
            SendPool = new StaticPool().Run();
        }

        public void Init(bool onlylogin = false) {
            if (!onlylogin) {
                // Initialize event system
                Game.Events.EventScheduler.Initialize();

                _buffers = new TimerRule<GameState>(BuffersCallback, 1000, ThreadPriority.BelowNormal);
                Characters = new TimerRule<GameState>(CharactersCallback, 1000, ThreadPriority.BelowNormal);
                AutoAttack = new TimerRule<GameState>(AutoAttackCallback, 1000, ThreadPriority.BelowNormal);
                Prayer = new TimerRule<GameState>(PrayerCallback, 1000, ThreadPriority.BelowNormal);
                Subscribe(WorldTournaments, 1000);
                Subscribe(ServerFunctions, 5000);
                Subscribe(ArenaFunctions, 1000, ThreadPriority.AboveNormal);
                Subscribe(TeamArenaFunctions, 1000, ThreadPriority.AboveNormal);
                Subscribe(ChampionFunctions, 1000, ThreadPriority.AboveNormal);
            }

            ConnectionReview = new TimerRule<ClientWrapper>(ConnectionReviewCallback, 60000, ThreadPriority.Lowest);
            ConnectionReceive = new TimerRule<ClientWrapper>(ConnectionReceiveCallback, 1);
            ConnectionSend = new TimerRule<ClientWrapper>(ConnectionSendCallback, 1);
        }

        public void CreateTournaments() {
            var map = Kernel.Maps[700];
            Tournaments = [
                new KillTournament(map.MakeDynamicMap().ID, WeekDay.Everyday, 1, 05,
                    (client) => { client.Entity.ConquerPoints += 1000000; }, "Nobility Tournament (Kings)",
                    (p) => p.Entity.NobilityRank == NobilityRank.King),

                new KillTournament(map.MakeDynamicMap().ID, WeekDay.Everyday, 1, 05,
                    (client) => { client.Entity.ConquerPoints += 1000000; }, "Nobility Tournament (Princes)",
                    (p) => p.Entity.NobilityRank == NobilityRank.Prince),

                new KillTournament(map.MakeDynamicMap().ID, WeekDay.Everyday, 1, 05,
                    (client) => { client.Entity.ConquerPoints += 1000000; }, "Nobility Tournament (Dukes)",
                    (p) => p.Entity.NobilityRank == NobilityRank.Duke),

                new KillTournament(map.MakeDynamicMap().ID, WeekDay.Everyday, 1, 05,
                    (client) => { client.Entity.ConquerPoints += 1000000; }, "Nobility Tournament (Earl)",
                    (p) => p.Entity.NobilityRank == NobilityRank.Earl),

                new KillTournament(map.MakeDynamicMap().ID, WeekDay.Everyday, 14, 0,
                    (client) => { client.Entity.ConquerPoints += 1000000; }, "Nobility Tournament (Kings)",
                    (p) => p.Entity.NobilityRank == NobilityRank.King),

                new KillTournament(map.MakeDynamicMap().ID, WeekDay.Everyday, 14, 0,
                    (client) => { client.Entity.ConquerPoints += 1000000; }, "Nobility Tournament (Princes)",
                    (p) => p.Entity.NobilityRank == NobilityRank.Prince),

                new KillTournament(map.MakeDynamicMap().ID, WeekDay.Everyday, 14, 0,
                    (client) => { client.Entity.ConquerPoints += 1000000; }, "Nobility Tournament (Dukes)",
                    (p) => p.Entity.NobilityRank == NobilityRank.Duke),

                new KillTournament(map.MakeDynamicMap().ID, WeekDay.Everyday, 14, 0,
                    (client) => { client.Entity.ConquerPoints += 1000000; }, "Nobility Tournament (Earl)",
                    (p) => p.Entity.NobilityRank == NobilityRank.Earl)
            ];

            #region Class PK Tournament

            map = Kernel.Maps[1730];
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 20, 30,
                (client) => {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Update.Flags.TopTrojan, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Trojan)", (p) => p.Entity.Class is >= 10 and <= 15,
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 20, 30,
                (client) => {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Update.Flags.TopWarrior, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Warrior)", (p) => p.Entity.Class is >= 20 and <= 25,
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 20, 30,
                (client) => {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Update.Flags.TopArcher, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Archer)", (p) => p.Entity.Class is >= 40 and <= 45,
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 20, 30,
                (client) => {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Update.Flags.TopNinja, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Ninja)", (p) => p.Entity.Class is >= 50 and <= 55,
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 20, 30,
                (client) => {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Update.Flags2.TopMonk, 2, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Monk)", (p) => p.Entity.Class is >= 60 and <= 65,
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 20, 30,
                (client) => {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Update.Flags2.TopPirate, 2, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Pirate)", (p) => p.Entity.Class is >= 70 and <= 75,
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 20, 30,
                (client) => {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Update.Flags3.DragonWarriorTop, 3, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (LeeLong)", (p) => p.Entity.Class is >= 80 and <= 85,
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 20, 30,
                (client) => {
                    client.Entity.ConquerPoints += 1000000;

                    client.Entity.AddTopStatus(Update.Flags.TopWaterTaoist, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Water Taoist)", (p) => p.Entity.Class is >= 130 and <= 135,
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 20, 30,
                (client) => {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Update.Flags.TopFireTaoist, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Fire Taoist)", (p) => p.Entity.Class is >= 140 and <= 145,
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));

            #endregion

            #region Class PK Tournament

            map = Kernel.Maps[1730];
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 8, 30,
                (client) => {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Update.Flags.TopTrojan, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Trojan)", (p) => p.Entity.Class is >= 10 and <= 15,
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 8, 30,
                (client) => {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Update.Flags.TopWarrior, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Warrior)", (p) => p.Entity.Class is >= 20 and <= 25,
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 8, 30,
                (client) => {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Update.Flags.TopArcher, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Archer)", (p) => p.Entity.Class is >= 40 and <= 45,
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 8, 30,
                (client) => {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Update.Flags.TopNinja, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Ninja)", (p) => p.Entity.Class is >= 50 and <= 55,
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 8, 30,
                (client) => {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Update.Flags2.TopMonk, 2, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Monk)", (p) => p.Entity.Class is >= 60 and <= 65,
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 8, 30,
                (client) => {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Update.Flags2.TopPirate, 2, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Pirate)", (p) => p.Entity.Class is >= 70 and <= 75,
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 8, 30,
                (client) => {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Update.Flags3.DragonWarriorTop, 3, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (LeeLong)", (p) => p.Entity.Class is >= 80 and <= 85,
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 8, 30,
                (client) => {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Update.Flags.TopWaterTaoist, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Water Taoist)", (p) => p.Entity.Class is >= 130 and <= 135,
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 8, 30,
                (client) => {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Update.Flags.TopFireTaoist, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Fire Taoist)", (p) => p.Entity.Class is >= 140 and <= 145,
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));

            #endregion

            PoleDomination = new PoleDomination(100000);
            ClanWarArena.Create();
            TeamElitePk.TeamTournament.Create();
            TeamElitePk.SkillTeamTournament.Create();
            //new ClassPoleWar();
            //new NobilityPoleWar();

            _ = new GuildScoreWar();
            _ = new MaTrix.Lobby();
            _ = new MaTrix.GuildPoleWar();
            HeroOfGame = new HeroOfGame();
            ElitePKTournament.Create();

            Ctf = new CaptureTheFlag();

            DelayedTask = new DelayedTask();
        }

        private void ConnectionReviewCallback(ClientWrapper wrapper, int time) {
            ClientWrapper.TryReview(wrapper);
        }

        private void ConnectionReceiveCallback(ClientWrapper wrapper, int time) {
            ClientWrapper.TryReceive(wrapper);
        }

        private void ConnectionSendCallback(ClientWrapper wrapper, int time) {
            ClientWrapper.TrySend(wrapper);
        }

        public bool Tele, Tele1, Tele2, Tele3, Tele4, Tele5, Tele6, Tele7;

        static World() {
            _cyclone1 = false;
        }

        private static void TeleEffect(GameState client, ushort x, ushort y, ushort mapId, uint id) {
            var map = Kernel.Maps[mapId];

            var floorItem1 = new FloorItem(true) {
                ItemID = id,
                MapID = mapId,
                ItemColor = Enums.Color.Black,
                Type = FloorItem.Effect,
                X = x,
                Y = y,
                OnFloor = Time32.Now,
                Owner = client
            };
            while (map.Npcs.ContainsKey(floorItem1.UID))
                floorItem1.UID = FloorItem.FloorUID.Next;
            map.AddFloorItem(floorItem1);
            client.SendScreenSpawn(floorItem1, true);
        }

        public bool Register(GameState client) {
            if (client.TimerSubscriptions == null) {
                client.TimerSyncRoot = new object();
                client.TimerSubscriptions = [
                    _buffers.Add(client),
                    Characters.Add(client),
                    AutoAttack.Add(client),
                    Prayer.Add(client)
                ];
                return true;
            }

            return false;
        }

        public void Unregister(GameState client) {
            if (client.TimerSubscriptions == null) return;
            lock (client.TimerSyncRoot) {
                if (client.TimerSubscriptions != null) {
                    foreach (var timer in client.TimerSubscriptions)
                        timer.Dispose();
                    client.TimerSubscriptions = null;
                }
            }
        }

        private static bool Valid(GameState client) {
            if (client.Socket.Alive) return true;
            client.Disconnect();
            return false;
        }

        private void BuffersCallback(GameState c, int time) {
            if (!Valid(c)) return;
            var now = new Time32(time);
            foreach (var client in Program.Values) {
                if (client.Entity is { BattlePower: > 405, NobilityRank: NobilityRank.King }) {
                    client.Disconnect();
                }

                if (client.Entity is { BattlePower: > 402, NobilityRank: NobilityRank.Prince }) {
                    client.Disconnect();
                }

                if (client.Entity is { BattlePower: > 400, NobilityRank: NobilityRank.Duke }) {
                    client.Disconnect();
                }

                if (client.Entity is { BattlePower: > 398, NobilityRank: NobilityRank.Earl }) {
                    client.Disconnect();
                }
            }

            #region Exit PolePrize

            if (DateTime.Now.Minute >= 10 && DateTime.Now.Second == 07) {
                if (c.Entity.MapID == 1024) {
                    c.Entity.Teleport(1002, 301, 279);
                    Kernel.SendWorldMessage(
                        new Message("PolePrize Is ended ,, Start This War at xx:05 Every Hour", Color.Black,
                            Message.Center), Program.Values);
                }
            }

            #endregion

            #region Arena Quit

            if (c.InArenaQualifier() && c.Map.BaseID != 700) {
                Arena.QualifyEngine.DoGiveUp(c);
            }

            #endregion

            #region Aura

            if (c.Entity.Aura_isActive) {
                if (c.Entity.Aura_isActive) {
                    if (Time32.Now >= c.Entity.AuraStamp.AddSeconds(c.Entity.AuraTime)) {
                        c.Entity.RemoveFlag2(c.Entity.Aura_actType);
                        c.removeAuraBonuses(c.Entity.Aura_actType, c.Entity.Aura_actPower, 1);
                        c.Entity.Aura_isActive = false;
                        c.Entity.AuraTime = 0;
                        c.Entity.Aura_actType = 0;
                        c.Entity.Aura_actPower = 0;
                        c.Entity.Aura_actLevel = 0;
                    }
                }
            }

            #endregion

            #region Bless

            if (c.Entity.ContainsFlag(Update.Flags.CastPray)) {
                if (c.BlessTime <= 7198500)
                    c.BlessTime += 1000;
                else
                    c.BlessTime = 7200000;
                c.Entity.Update(Update.LuckyTimeTimer, c.BlessTime, false);
            }
            else if (c.Entity.ContainsFlag(Update.Flags.Praying)) {
                if (c.PrayLead != null) {
                    if (c.PrayLead.Socket.Alive) {
                        if (c.BlessTime <= 7199000)
                            c.BlessTime += 500;
                        else
                            c.BlessTime = 7200000;
                        c.Entity.Update(Update.LuckyTimeTimer, c.BlessTime, false);
                    }
                    else
                        c.Entity.RemoveFlag(Update.Flags.Praying);
                }
                else
                    c.Entity.RemoveFlag(Update.Flags.Praying);
            }
            else {
                if (c.BlessTime > 0) {
                    if (c.BlessTime >= 500)
                        c.BlessTime -= 500;
                    else
                        c.BlessTime = 0;
                    c.Entity.Update(Update.LuckyTimeTimer, c.BlessTime, false);
                }
            }

            #endregion

            #region XpBlueStamp

            if (c.Entity.ContainsFlag3(Update.Flags3.WarriorEpicShield)) {
                if (Time32.Now > c.Entity.XpBlueStamp.AddSeconds(33)) {
                    c.Entity.ShieldIncrease = 0;
                    c.Entity.ShieldTime = 0;
                    c.Entity.MagicShieldIncrease = 0;
                    c.Entity.MagicShieldTime = 0;
                    c.Entity.RemoveFlag3(Update.Flags3.WarriorEpicShield);
                }
            }

            #endregion

            #region ManiacDance

            if (c.Entity.ContainsFlag3(1UL << 53)) {
                if (Time32.Now > c.Entity.ManiacDance.AddSeconds(15)) {
                    c.Entity.RemoveFlag3(1UL << 53);
                }
            }

            #endregion

            #region Backfire

            if (c.Entity.ContainsFlag3(1UL << 51)) {
                if (Time32.Now > c.Entity.BackfireStamp.AddSeconds(8)) {
                    if (c.Spells.ContainsKey(12680)) {
                        if (c.Entity.ContainsFlag3(1UL << 51))
                            c.Entity.RemoveFlag3(1UL << 51);
                    }

                    c.Entity.BackfireStamp = Time32.Now;
                }
            }

            #endregion

            #region Flashing name

            if (c.Entity.ContainsFlag(Update.Flags.FlashingName)) {
                if (now > c.Entity.FlashingNameStamp.AddSeconds(c.Entity.FlashingNameTime)) {
                    c.Entity.RemoveFlag(Update.Flags.FlashingName);
                }
            }

            #endregion

            #region XPList

            if (!c.Entity.ContainsFlag(Update.Flags.XPList)) {
                if (now > c.XPCountStamp.AddSeconds(3)) {
                    #region Frankos

                    if (!c.Equipment.Free(5)) {
                        if (Network.PacketHandler.IsFranko(c.Equipment.TryGetItem(5).ID)) {
                            Database.ConquerItemTable.UpdateDurabilityItem(c.Equipment.TryGetItem(5));
                        }
                    }

                    #endregion

                    c.XPCountStamp = now;
                    c.XPCount++;
                    if (c.XPCount >= 100) {
                        c.Entity.AddFlag(Update.Flags.XPList);
                        c.XPCount = 0;
                        c.XPListStamp = now;
                    }
                }
            }
            else {
                if (now > c.XPListStamp.AddSeconds(20)) {
                    c.Entity.RemoveFlag(Update.Flags.XPList);
                }
            }

            #endregion

            #region KOSpell

            if (c.Entity.OnKOSpell()) {
                if (c.Entity.OnCyclone()) {
                    int seconds = now.AllSeconds() -
                                  c.Entity.CycloneStamp.AddSeconds(c.Entity.CycloneTime).AllSeconds();
                    if (seconds >= 1) {
                        c.Entity.RemoveFlag(Update.Flags.Cyclone);
                    }
                }

                if (c.Entity.OnSuperman()) {
                    var seconds = now.AllSeconds() -
                                  c.Entity.SupermanStamp.AddSeconds(c.Entity.SupermanTime).AllSeconds();
                    if (seconds >= 1) {
                        c.Entity.RemoveFlag(Update.Flags.Superman);
                    }
                }

                if (!c.Entity.OnKOSpell()) {
                    c.Entity.KOCount = 0;
                }
            }

            #endregion

            #region Buffers

            if (c.Entity.Aura_isActive) {
                if (now >= c.Entity.AuraStamp.AddSeconds(c.Entity.AuraTime) || c.Entity.Dead) {
                    c.Entity.AuraTime = 0;
                    c.Entity.Aura_isActive = false;
                    Update.AuraType aura = Update.AuraType.TyrantAura;
                    switch (c.Entity.Aura_actType) {
                        case Update.Flags2.EarthAura: aura = Update.AuraType.EarthAura; break;
                        case Update.Flags2.FireAura: aura = Update.AuraType.FireAura; break;
                        case Update.Flags2.WaterAura: aura = Update.AuraType.WaterAura; break;
                        case Update.Flags2.WoodAura: aura = Update.AuraType.WoodAura; break;
                        case Update.Flags2.MetalAura: aura = Update.AuraType.MetalAura; break;
                        case Update.Flags2.FendAura: aura = Update.AuraType.FendAura; break;
                        case Update.Flags2.TyrantAura: aura = Update.AuraType.TyrantAura; break;
                    }

                    new Update(true).Aura(c.Entity, Update.AuraDataTypes.Remove, aura, c.Entity.Aura_actLevel,
                        c.Entity.Aura_actPower);

                    c.removeAuraBonuses(c.Entity.Aura_actType, c.Entity.Aura_actPower, 1);
                    c.Entity.RemoveFlag2(c.Entity.Aura_actType);
                    c.Entity.RemoveFlag2(c.Entity.Aura_actType2);
                    c.Entity.Aura_actType = 0;
                    c.Entity.Aura_actType2 = 0;
                    c.Entity.Aura_actPower = 0;
                    c.Entity.Aura_actLevel = 0;
                }
            }

            if (c.Entity.ContainsFlag(Update.Flags.Stigma)) {
                if (now >= c.Entity.StigmaStamp.AddSeconds(c.Entity.StigmaTime)) {
                    c.Entity.StigmaTime = 0;
                    c.Entity.StigmaIncrease = 0;
                    c.Entity.RemoveFlag(Update.Flags.Stigma);
                }
            }

            if (c.Entity.ContainsFlag(Update.Flags.Dodge)) {
                if (now >= c.Entity.DodgeStamp.AddSeconds(c.Entity.DodgeTime)) {
                    c.Entity.DodgeTime = 0;
                    c.Entity.DodgeIncrease = 0;
                    c.Entity.RemoveFlag(Update.Flags.Dodge);
                }
            }

            if (c.Entity.ContainsFlag(Update.Flags.Invisibility)) {
                if (now >= c.Entity.InvisibilityStamp.AddSeconds(c.Entity.InvisibilityTime)) {
                    c.Entity.RemoveFlag(Update.Flags.Invisibility);
                }
            }

            if (c.Entity.ContainsFlag(Update.Flags.StarOfAccuracy)) {
                if (c.Entity.StarOfAccuracyTime != 0) {
                    if (now >= c.Entity.StarOfAccuracyStamp.AddSeconds(c.Entity.StarOfAccuracyTime)) {
                        c.Entity.RemoveFlag(Update.Flags.StarOfAccuracy);
                    }
                }
                else {
                    if (now >= c.Entity.AccuracyStamp.AddSeconds(c.Entity.AccuracyTime)) {
                        c.Entity.RemoveFlag(Update.Flags.StarOfAccuracy);
                    }
                }
            }

            if (c.Entity.ContainsFlag(Update.Flags.MagicShield)) {
                if (c.Entity.MagicShieldTime != 0) {
                    if (now >= c.Entity.MagicShieldStamp.AddSeconds(c.Entity.MagicShieldTime)) {
                        c.Entity.MagicShieldIncrease = 0;
                        c.Entity.MagicShieldTime = 0;
                        c.Entity.RemoveFlag(Update.Flags.MagicShield);
                    }
                }
                else {
                    if (now >= c.Entity.ShieldStamp.AddSeconds(c.Entity.ShieldTime)) {
                        c.Entity.ShieldIncrease = 0;
                        c.Entity.ShieldTime = 0;
                        c.Entity.RemoveFlag(Update.Flags.MagicShield);
                    }
                }
            }

            #endregion

            if (c.Map.BaseID == 700) {
                if (c.Entity.ContainsFlag(Update.Flags.Ride)) {
                    c.Entity.RemoveFlag(Update.Flags.Ride);
                }
            }

            #region AuroraLotus

            if (c.Spells.ContainsKey(12370)) {
                if (!c.Entity.ContainsFlag3(Update.Flags3.AuroraLotus)) {
                    c.Entity.AuroraLotusEnergy = 0;
                    if (c.Entity.Lotus(c.Entity.AuroraLotusEnergy))
                        c.Entity.AddFlag3(Update.Flags3.AuroraLotus);
                }
            }

            #endregion AuroraLotus

            #region FlameLotus

            if (c.Spells.ContainsKey(12380)) {
                if (!c.Entity.ContainsFlag3(Update.Flags3.FlameLotus)) {
                    c.Entity.FlameLotusEnergy = 0;
                    if (c.Entity.Lotus(c.Entity.FlameLotusEnergy, Update.FlameLotus))
                        c.Entity.AddFlag3(Update.Flags3.FlameLotus);
                }
            }

            #endregion FlameLotus

            c.CheckTeamAura();

            #region Fly

            if (c.Entity.ContainsFlag(Update.Flags.Fly)) {
                if (now >= c.Entity.FlyStamp.AddSeconds(c.Entity.FlyTime)) {
                    c.Entity.RemoveFlag(Update.Flags.Fly);
                    c.Entity.FlyTime = 0;
                }
            }

            #endregion

            #region PoisonStar

            if (c.Entity.NoDrugsTime > 0) {
                if (now > c.Entity.NoDrugsStamp.AddSeconds(c.Entity.NoDrugsTime)) {
                    c.Entity.NoDrugsTime = 0;
                    c.Entity.RemoveFlag2(Update.Flags2.EffectBall);
                }
            }

            #endregion

            #region ToxicFog

            if (c.Entity.ToxicFogLeft > 0) {
                if (now >= c.Entity.ToxicFogStamp.AddSeconds(2)) {
                    float percent = c.Entity.ToxicFogPercent;
                    if (c.Entity.Detoxication != 0) {
                        float immunity = 1 - c.Entity.Detoxication / 100F;
                        percent = percent * immunity;
                    }

                    c.Entity.ToxicFogLeft--;
                    if (c.Entity.ToxicFogLeft == 0) {
                        c.Entity.RemoveFlag(Update.Flags.Poisoned);
                        return;
                    }

                    c.Entity.ToxicFogStamp = now;
                    if (c.Entity.Hitpoints > 1) {
                        uint damage = Game.Attacking.Calculate.Percent(c.Entity, percent);
                        if (c.Entity.ContainsFlag2(Update.Flags2.AzureShield)) {
                            if (damage > c.Entity.AzureShieldDefence) {
                                damage -= c.Entity.AzureShieldDefence;
                                Game.Attacking.Calculate.CreateAzureDMG(c.Entity.AzureShieldDefence, c.Entity,
                                    c.Entity);
                                c.Entity.RemoveFlag2(Update.Flags2.AzureShield);
                            }
                            else {
                                Game.Attacking.Calculate.CreateAzureDMG(damage, c.Entity, c.Entity);
                                c.Entity.AzureShieldDefence -= (ushort)damage;
                                c.Entity.AzureShieldPacket();
                                damage = 1;
                            }
                        }
                        else
                            c.Entity.Hitpoints -= damage;

                        SpellUse suse = new SpellUse(true) {
                            Attacker = c.Entity.UID,
                            SpellID = 10010
                        };
                        suse.AddTarget(c.Entity, damage, null);
                        c.SendScreen(suse);
                        c.UpdateQualifier(c.ArenaStatistic.PlayWith, c, damage);
                    }
                }
            }
            else {
                if (c.Entity.ContainsFlag(Update.Flags.Poisoned))
                    c.Entity.RemoveFlag(Update.Flags.Poisoned);
            }

            #endregion

            #region lianhuaran

            if (c.Entity.lianhuaranLeft > 0) {
                if (now >= c.Entity.lianhuaranStamp.AddSeconds(2)) {
                    float percent = c.Entity.lianhuaranPercent;
                    if (c.Entity.Detoxication != 0) {
                        float immu = 1 - c.Entity.Detoxication / 100F;
                        percent = percent * immu;
                    }

                    c.Entity.lianhuaranLeft--;
                    if (c.Entity.lianhuaranLeft == 0) {
                        c.Entity.RemoveFlag3(Update.Flags3.lianhuaran01);
                        c.Entity.RemoveFlag3(Update.Flags3.lianhuaran02);
                        c.Entity.RemoveFlag3(Update.Flags3.lianhuaran03);
                        c.Entity.RemoveFlag3(Update.Flags3.lianhuaran04);
                        return;
                    }

                    c.Entity.lianhuaranStamp = now;
                    if (c.Entity.Hitpoints > 1) {
                        uint damage = Game.Attacking.Calculate.Percent(c.Entity, percent);
                        if (c.Entity.ContainsFlag2(Update.Flags2.AzureShield)) {
                            if (damage > c.Entity.AzureShieldDefence) {
                                damage -= c.Entity.AzureShieldDefence;
                                Game.Attacking.Calculate.CreateAzureDMG(c.Entity.AzureShieldDefence, c.Entity,
                                    c.Entity);
                                c.Entity.RemoveFlag2(Update.Flags2.AzureShield);
                            }
                            else {
                                Game.Attacking.Calculate.CreateAzureDMG(damage, c.Entity, c.Entity);
                                c.Entity.AzureShieldDefence -= (ushort)damage;
                                c.Entity.AzureShieldPacket();
                                damage = 1;
                            }
                        }
                        else
                            c.Entity.Hitpoints -= damage;


                        c.UpdateQualifier(c.ArenaStatistic.PlayWith, c, damage);
                    }
                }
            }
            else {
                if (c.Entity.ContainsFlag3(Update.Flags3.lianhuaran01))
                    c.Entity.RemoveFlag3(Update.Flags3.lianhuaran01);
                if (c.Entity.ContainsFlag3(Update.Flags3.lianhuaran02))
                    c.Entity.RemoveFlag3(Update.Flags3.lianhuaran02);
                if (c.Entity.ContainsFlag3(Update.Flags3.lianhuaran03))
                    c.Entity.RemoveFlag3(Update.Flags3.lianhuaran03);
                if (c.Entity.ContainsFlag3(Update.Flags3.lianhuaran04))
                    c.Entity.RemoveFlag3(Update.Flags3.lianhuaran04);
            }

            #endregion

            #region FatalStrike

            if (c.Entity.OnFatalStrike()) {
                if (now > c.Entity.FatalStrikeStamp.AddSeconds(c.Entity.FatalStrikeTime)) {
                    c.Entity.RemoveFlag(Update.Flags.FatalStrike);
                }
            }

            #endregion

            #region Oblivion

            if (c.Entity.OnOblivion()) {
                if (now > c.Entity.OblivionStamp.AddSeconds(c.Entity.OblivionTime)) {
                    c.Entity.RemoveFlag2(Update.Flags2.Oblivion);
                }
            }

            #endregion

            #region ShurikenVortex

            if (c.Entity.ContainsFlag(Update.Flags.ShurikenVortex)) {
                if (now > c.Entity.ShurikenVortexStamp.AddSeconds(c.Entity.ShurikenVortexTime)) {
                    c.Entity.RemoveFlag(Update.Flags.ShurikenVortex);
                }
            }

            #endregion

            #region Transformations

            if (c.Entity.Transformed) {
                if (now > c.Entity.TransformationStamp.AddSeconds(c.Entity.TransformationTime)) {
                    c.Entity.Untransform();
                }
            }

            #endregion

            #region soulshackle

            if (c.Entity.ContainsFlag2(Update.Flags2.SoulShackle)) {
                if (now > c.Entity.ShackleStamp.AddSeconds(c.Entity.ShackleTime)) {
                    c.Entity.RemoveFlag2(Update.Flags2.SoulShackle);
                }
            }

            #endregion

            #region portals

            if (c.Entity.MapID == 2222) {
                #region First Map

                TeleEffect(c, 38, 40, 2222, 24);
                TeleEffect(c, 38, 45, 2222, 1050);
                TeleEffect(c, 38, 50, 2222, 24);
                TeleEffect(c, 38, 55, 2222, 1050);
                TeleEffect(c, 38, 60, 2222, 24);
                if (c.Entity is { X: 38, Y: 40 }) {
                    if (!Tele) {
                        c.Entity.Teleport(1002, 428, 379);
                        Tele = true;
                    }
                    else {
                        c.Entity.Teleport(2323, 50, 50);
                        Tele = false;
                    }
                }
                else if (c.Entity is { X: 38, Y: 45 }) {
                    if (!Tele1) {
                        c.Entity.Teleport(2323, 50, 50);
                        Tele1 = true;
                    }
                    else {
                        c.Entity.Teleport(1002, 428, 379);
                        Tele1 = false;
                    }
                }
                else if (c.Entity is { X: 38, Y: 50 }) {
                    if (!Tele2) {
                        c.Entity.Teleport(1002, 428, 379);
                        Tele2 = true;
                    }
                    else {
                        c.Entity.Teleport(2323, 50, 50);
                        Tele2 = false;
                    }
                }
                else if (c.Entity is { X: 38, Y: 55 }) {
                    if (!Tele3) {
                        c.Entity.Teleport(2323, 50, 50);

                        Tele3 = true;
                    }
                    else {
                        c.Entity.Teleport(1002, 428, 379);
                        Tele3 = false;
                    }
                }
                else if (c.Entity is { X: 38, Y: 60 }) {
                    if (!Tele4) {
                        c.Entity.Teleport(1002, 428, 379);
                        Tele4 = true;
                    }
                    else {
                        c.Entity.Teleport(2323, 50, 50);
                        Tele4 = false;
                    }
                }

                #endregion
            }

            if (c.Entity.MapID == 2323) {
                #region Second Map

                TeleEffect(c, 38, 40, 2323, 24);
                TeleEffect(c, 38, 50, 2323, 1050);
                TeleEffect(c, 38, 60, 2323, 24);
                if (c.Entity is { X: 38, Y: 40 }) {
                    if (!Tele5) {
                        c.Entity.Teleport(1002, 428, 379);
                        Tele5 = true;
                    }
                    else {
                        c.Entity.Teleport(2121, 50, 50);
                        Tele5 = false;
                    }
                }
                else if (c.Entity is { X: 38, Y: 50 }) {
                    if (!Tele6) {
                        c.Entity.Teleport(2121, 50, 50);
                        Tele6 = true;
                    }
                    else {
                        c.Entity.Teleport(1002, 428, 379);
                        Tele6 = false;
                    }
                }
                else if (c.Entity is { X: 38, Y: 60 }) {
                    if (!Tele7) {
                        c.Entity.Teleport(1002, 428, 379);
                        Tele7 = true;
                    }
                    else {
                        c.Entity.Teleport(2121, 50, 50);
                        Tele7 = false;
                    }
                }

                #endregion
            }

            #endregion

            #region AutoHunting

            if (c.Entity.ContainsFlag3((uint)Update.Flags3.AutoHunting)) {
                if (now > c.Entity.AutoHuntStamp.AddMinutes(15)) {
                    c.Entity.RemoveFlag3((uint)Update.Flags3.AutoHunting);
                }
            }

            #endregion

            #region Intensify

            if (c.Entity.IntensifyPercent != 0) {
                if (now > c.Entity.IntensifyStamp.AddSeconds(5)) {
                    c.Entity.AddFlag(Update.Flags.Intensify);
                }
            }

            #endregion

            #region AzureShield

            if (c.Entity.ContainsFlag2(Update.Flags2.AzureShield)) {
                if (now > c.Entity.MagicShieldStamp.AddSeconds(c.Entity.MagicShieldTime)) {
                    c.Entity.RemoveFlag2(Update.Flags2.AzureShield);
                }
            }

            #endregion

            #region Blade Flurry

            if (c.Entity.ContainsFlag3(Update.Flags3.BladeFlurry)) {
                if (Time32.Now > c.Entity.BladeFlurryStamp.AddSeconds(45)) {
                    c.Entity.RemoveFlag3(Update.Flags3.BladeFlurry);
                }
            }

            #endregion

            #region Flustered

            if (c.Entity.ContainsFlag(Update.Flags.Frightened)) {
                if (c.RaceFrightened) {
                    if (now > c.FrightenStamp.AddSeconds(20)) {
                        c.RaceFrightened = false;
                        {
                            GameCharacterUpdates update = new GameCharacterUpdates(true) {
                                UID = c.Entity.UID
                            };
                            update.Remove(GameCharacterUpdates.Flustered);
                            c.SendScreen(update);
                        }
                        c.Entity.RemoveFlag(Update.Flags.Frightened);
                    }
                    else {
                        ushort x, y;
                        do {
                            var rand = Kernel.Random.Next(Map.XDir.Length);
                            x = (ushort)(c.Entity.X + Map.XDir[rand]);
                            y = (ushort)(c.Entity.Y + Map.YDir[rand]);
                        } while (!c.Map.Floor[x, y, MapObjectType.Player]);

                        c.Entity.Facing = Kernel.GetAngle(
                            c.Entity.X, c.Entity.Y, x, y);
                        c.Entity.X = x;
                        c.Entity.Y = y;

                        c.SendScreen(
                            new TwoMovements() {
                                EntityCount = 1,
                                Facing = c.Entity.Facing,
                                FirstEntity = c.Entity.UID,
                                WalkType = 9,
                                X = c.Entity.X,
                                Y = c.Entity.Y,
                                MovementType = TwoMovements.Walk
                            });
                    }
                }
            }

            #endregion

            #region Stunned

            if (c.Entity.Stunned) {
                if (now > c.Entity.StunStamp.AddMilliseconds(2000)) {
                    c.Entity.Stunned = false;
                }
            }

            #endregion

            #region Frozen

            if (c.Entity.ContainsFlag(Update.Flags.Freeze)) {
                if (now > c.Entity.FrozenStamp.AddSeconds(c.Entity.FrozenTime)) {
                    c.Entity.FrozenD = false;
                    c.Entity.FrozenTime = 0;
                    c.Entity.RemoveFlag(Update.Flags.Freeze);

                    GameCharacterUpdates update = new GameCharacterUpdates(true) {
                        UID = c.Entity.UID
                    };
                    update.Remove(GameCharacterUpdates.Freeze);
                    c.SendScreen(update);
                }
            }

            #endregion

            #region IceBlock

            if (c.Entity.ContainsFlag(Update.Flags.FreezeSmall)) {
                if (now > c.FrightenStamp.AddSeconds(c.Entity.Fright)) {
                    var update = new GameCharacterUpdates(true) {
                        UID = c.Entity.UID
                    };
                    update.Remove(GameCharacterUpdates.Dizzy);
                    c.SendScreen(update);
                    c.Entity.RemoveFlag(Update.Flags.FreezeSmall);
                }
                else {
                    ushort x, y;
                    do {
                        var rand = Kernel.Random.Next(Map.XDir.Length);
                        x = (ushort)(c.Entity.X + Map.XDir[rand]);
                        y = (ushort)(c.Entity.Y + Map.YDir[rand]);
                    } while (!c.Map.Floor[x, y, MapObjectType.Player]);

                    c.Entity.Facing = Kernel.GetAngle(c.Entity.X, c.Entity.Y, x, y);
                    c.Entity.X = x;
                    c.Entity.Y = y;
                    c.SendScreen(new TwoMovements() {
                        EntityCount = 1,
                        Facing = c.Entity.Facing,
                        FirstEntity = c.Entity.UID,
                        WalkType = 9,
                        X = c.Entity.X,
                        Y = c.Entity.Y,
                        MovementType = TwoMovements.Walk
                    });
                }
            }

            #endregion

            #region Dizzy

            if (c.Entity.ContainsFlag(Update.Flags.Dizzy)) {
                if (c.RaceDizzy) {
                    if (now > c.DizzyStamp.AddSeconds(5)) {
                        c.RaceDizzy = false;
                        {
                            GameCharacterUpdates update = new GameCharacterUpdates(true) {
                                UID = c.Entity.UID
                            };
                            update.Remove(GameCharacterUpdates.Dizzy);
                            c.SendScreen(update);
                        }
                        c.Entity.RemoveFlag(Update.Flags.Dizzy);
                    }
                }
            }

            #endregion

            #region Confused

            if (c.Entity.ContainsFlag(Update.Flags.Confused)) {
                if (now > c.FrightenStamp.AddSeconds(15)) {
                    c.RaceFrightened = false;
                    {
                        GameCharacterUpdates update = new GameCharacterUpdates(true) {
                            UID = c.Entity.UID
                        };
                        update.Remove(GameCharacterUpdates.Flustered);
                        c.SendScreen(update);
                    }
                    c.Entity.RemoveFlag(Update.Flags.Confused);
                }
            }

            #endregion

            #region Divine Shield

            if (c.Entity.ContainsFlag(Update.Flags.DivineShield)) {
                if (now > c.GuardStamp.AddSeconds(10)) {
                    c.RaceGuard = false;
                    {
                        GameCharacterUpdates update = new GameCharacterUpdates(true) {
                            UID = c.Entity.UID
                        };
                        update.Remove(GameCharacterUpdates.DivineShield);
                        c.SendScreen(update);
                    }
                    c.Entity.RemoveFlag(Update.Flags.DivineShield);
                }
            }

            #endregion

            #region Extra Speed

            if (c.Entity.ContainsFlag(Update.Flags.OrangeSparkles) && !c.InQualifier()) {
                if (Time32.Now > c.RaceExcitementStamp.AddSeconds(15)) {
                    var upd = new GameCharacterUpdates(true) {
                        UID = c.Entity.UID
                    };
                    upd.Remove(GameCharacterUpdates.Accelerated);
                    c.SendScreen(upd);
                    c.SpeedChange = null;
                    c.Entity.RemoveFlag(Update.Flags.OrangeSparkles);
                }
            }

            #endregion

            #region Decelerated

            if (c.Entity.ContainsFlag(Update.Flags.PurpleSparkles) && !c.InQualifier()) {
                if (Time32.Now > c.DecelerateStamp.AddSeconds(10)) {
                    {
                        c.RaceDecelerated = false;
                        var upd = new GameCharacterUpdates(true) {
                            UID = c.Entity.UID
                        };
                        upd.Remove(GameCharacterUpdates.Decelerated);
                        c.SendScreen(upd);
                        c.SpeedChange = null;
                    }
                    c.Entity.RemoveFlag(Update.Flags.PurpleSparkles);
                }
            }

            #endregion

            #region ShockDaze

            if (c.Entity.ContainsFlag(Update.Flags.Stun)) {
                if (now > c.Entity.ShockStamp.AddSeconds(c.Entity.Shock)) {
                    c.Entity.RemoveFlag(Update.Flags.Stun);
                }
            }

            #endregion

            #region ChaosCycle

            if (c.Entity.ContainsFlag(Update.Flags.ChaosCycle)) {
                if (now > c.FrightenStamp.AddSeconds(5)) {
                    c.RaceFrightened = false;
                    {
                        GameCharacterUpdates update = new GameCharacterUpdates(true) {
                            UID = c.Entity.UID
                        };
                        update.Remove(GameCharacterUpdates.Flustered);
                        c.SendScreen(update);
                    }
                    c.Entity.RemoveFlag(Update.Flags.ChaosCycle);
                }
            }

            #endregion

            #region FreezeSmall

            if (c.Entity.ContainsFlag(Update.Flags.FreezeSmall)) {
                {
                    if (now > c.FrightenStamp.AddSeconds(20)) {
                        c.RaceFrightened = false;
                        {
                            GameCharacterUpdates update = new GameCharacterUpdates(true) {
                                UID = c.Entity.UID
                            };
                            update.Remove(GameCharacterUpdates.Flustered);
                            c.SendScreen(update);
                        }
                        c.Entity.RemoveFlag(Update.Flags.FreezeSmall);
                    }
                    else {
                        ushort x, y;
                        do {
                            var rand = Kernel.Random.Next(Map.XDir.Length);
                            x = (ushort)(c.Entity.X + Map.XDir[rand]);
                            y = (ushort)(c.Entity.Y + Map.YDir[rand]);
                        } while (!c.Map.Floor[x, y, MapObjectType.Player]);

                        c.Entity.Facing = Kernel.GetAngle(c.Entity.X, c.Entity.Y, x, y);
                        c.Entity.X = x;
                        c.Entity.Y = y;
                        c.SendScreen(new TwoMovements() {
                            EntityCount = 1,
                            Facing = c.Entity.Facing,
                            FirstEntity = c.Entity.UID,
                            WalkType = 9,
                            X = c.Entity.X,
                            Y = c.Entity.Y,
                            MovementType = TwoMovements.Walk
                        });
                    }
                }
            }

            #endregion

            #region CTF Flag

            if (c.Entity.ContainsFlag2(Update.Flags2.CarryingFlag)) {
                if (Time32.Now > c.Entity.FlagStamp.AddSeconds(60)) {
                    c.Entity.RemoveFlag2(Update.Flags2.CarryingFlag);
                }
            }

            #endregion

            #region Congelado

            if (c.Entity.ContainsFlag(Update.Flags2.Congelado)) {
                if (DateTime.Now > c.Entity.CongeladoTimeStamp.AddSeconds(c.Entity.CongeladoTime)) {
                    c.Entity.RemoveFlag(Update.Flags2.Congelado);
                }
            }

            #endregion

            #region Cursed

            if (c.Entity.ContainsFlag(Update.Flags.Cursed)) {
                if (Time32.Now > c.Entity.Cursed.AddSeconds(300)) {
                    c.Entity.RemoveFlag(Update.Flags.Cursed);
                }
            }

            #endregion

            #region SuperCycloneStamp

            if (c.Entity.ContainsFlag3((uint)Update.Flags3.SuperCyclone)) {
                if (Time32.Now > c.Entity.SuperCycloneStamp.AddSeconds(45)) {
                    c.Entity.RemoveFlag3((uint)Update.Flags3.SuperCyclone);
                }
            }

            #endregion

            #region DragonCyclone

            if (c.Entity.ContainsFlag3(Update.Flags3.DragonCyclone)) {
                if (Time32.Now > c.Entity.DragonCycloneStamp.AddSeconds(45)) {
                    c.Entity.RemoveFlag3(Update.Flags3.DragonCyclone);
                }
            }

            #endregion

            #region DragonFury

            if (c.Entity.ContainsFlag3(Update.Flags3.DragonFury)) {
                if (Time32.Now > c.Entity.DragonFuryStamp.AddSeconds(c.Entity.DragonFuryTime)) {
                    c.Entity.RemoveFlag3(Update.Flags3.DragonFury);

                    Update upgrade = new Update(true) {
                        UID = c.Entity.UID
                    };
                    upgrade.Append(74
                        , 0
                        , 0, 0, 0);
                    c.Entity.Owner.Send(upgrade.ToArray());
                }
            }

            #endregion

            #region DragonFlow

            if (c.Entity.ContainsFlag3(Update.Flags3.DragonFlow) &&
                !c.Entity.ContainsFlag3(Update.Flags3.DragonCyclone)) {
                if (Time32.Now > c.Entity.DragonFlowStamp.AddSeconds(8)) {
                    if (c.Spells.TryGetValue(12270, out Interfaces.ISkill? value)) {
                        var spell = Database.SpellTable.GetSpell(value.ID, value.Level);
                        {
                            int stamina = 100;
                            if (c.Entity.HeavenBlessing > 0)
                                stamina += 50;
                            if (c.Spells.ContainsKey(12560)) {
                                var spells = c.Spells[12560];
                                var skill = Database.SpellTable.SpellInformations[12560][spells.Level];
                                stamina += skill.Power;
                            }

                            if (c.Entity.Stamina != stamina) {
                                c.Entity.Stamina += (byte)spell.Power;
                                if (c.Entity.ContainsFlag3(Update.Flags3.DragonCyclone))
                                    if (c.Entity.Stamina != stamina)
                                        c.Entity.Stamina += (byte)spell.Power;
                                _String str = new _String(true) {
                                    UID = c.Entity.UID,
                                    TextsCount = 1,
                                    Type = _String.Effect
                                };
                                str.Texts.Add("leedragonblood");
                                c.SendScreen(str);
                            }
                        }
                    }

                    c.Entity.DragonFlowStamp = Time32.Now;
                }
            }

            #endregion

            #region DragonSwing

            if (c.Entity.ContainsFlag3(Update.Flags3.DragonSwing)) {
                if (Time32.Now > c.Entity.DragonSwingStamp.AddSeconds(160)) {
                    c.Entity.RemoveFlag3(Update.Flags3.DragonSwing);
                    c.Entity.OnDragonSwing = false;
                    Update upgrade = new Update(true) {
                        UID = c.Entity.UID
                    };
                    upgrade.Append(Update.DragonSwing, 0, 0, 0, 0);
                    c.Entity.Owner.Send(upgrade.ToArray());
                }
            }

            #endregion

            if (c.Entity.race == 1 && _cyclone1) {
                c.Entity.RemoveFlag(Update.Flags.Ride);

                c.Entity.CycloneStamp = Time32.Now;
                c.Entity.CycloneTime = 180;
                c.Entity.AddFlag(Update.Flags.Cyclone);
                c.Entity.race = 0;
                var r = new Random();
                var nr = r.Next(1, 2);
                switch (nr) {
                    case 1:
                        c.Entity.Teleport(1645, 309, 238);
                        break;
                    case 2:
                        c.Entity.Teleport(1645, 305, 231);
                        break;
                }
            }

            if (!_cyclone3 && c.Entity.MapID == 1645) {
                c.Entity.Teleport(1002, 435 - 128, 378 - 100);
            }

            if (c.Entity.ContainsFlag(Update.Flags.Ride) && c.Entity.MapID == 1645) {
                c.Entity.RemoveFlag(Update.Flags.Ride);
            }
        }

        private void CharactersCallback(GameState client, int time) {
            #region lacb

            if (client.Entity.lacb >= 10 & client.Entity.lacb <= 300) {
                client.Entity.Update(Update.mantos, 1, true);
            }

            if (client.Entity.lacb >= 300 & client.Entity.lacb <= 600) {
                client.Entity.Update(Update.mantos, 2, true);
            }

            if (client.Entity.lacb >= 600 & client.Entity.lacb <= 900) {
                client.Entity.Update(Update.mantos, 3, true);
            }

            if (client.Entity.lacb >= 900 & client.Entity.lacb <= 1300) {
                client.Entity.Update(Update.mantos, 4, true);
            }

            if (client.Entity.lacb >= 1300 & client.Entity.lacb <= 1600) {
                client.Entity.Update(Update.mantos, 5, true);
            }

            if (client.Entity.lacb >= 1600 & client.Entity.lacb <= 1900) {
                client.Entity.Update(Update.mantos, 6, true);
            }

            if (client.Entity.lacb >= 1900 & client.Entity.lacb <= 2200) {
                client.Entity.Update(Update.mantos, 7, true);
            }

            if (client.Entity.lacb >= 2200 & client.Entity.lacb <= 2800) {
                client.Entity.Update(Update.mantos, 8, true);
            }

            if (client.Entity.lacb >= 2800 & client.Entity.lacb <= 3400) {
                client.Entity.Update(Update.mantos, 9, true);
            }

            if (client.Entity.lacb >= 3400 & client.Entity.lacb <= 4200) {
                client.Entity.Update(Update.mantos, 10, true);
            }

            if (client.Entity.lacb >= 4200 & client.Entity.lacb <= 5400) {
                client.Entity.Update(Update.mantos, 11, true);
            }

            if (client.Entity.lacb >= 5400 & client.Entity.lacb <= 6800) {
                client.Entity.Update(Update.mantos, 12, true);
            }

            if (client.Entity.lacb >= 6800) {
                client.Entity.Update(Update.mantos, 13, true);
            }

            #endregion

            #region Time Check

            if (DateTime.Now.Second == 00)

                #endregion Time Check

            {
                if (!Valid(client)) return;

                #region Winners for FB and SS

                if (client.Entity.aWinner) {
                    if (Time32.Now > client.Entity.WinnerWaiting.AddSeconds(1)) {
                        switch (client.Entity.MapID) {
                            case 1543: //room 1 
                            {
                                Program.Room1 = false;
                                break;
                            }
                            case 1544: //room 2 
                            {
                                Program.Room2 = false;
                                break;
                            }
                            case 1545: //room 3 
                            {
                                Program.Room3 = false;
                                break;
                            }
                            case 1546: //room 4 
                            {
                                Program.Room4 = false;
                                break;
                            }
                            case 1547: //room 5 
                            {
                                Program.Room5 = false;
                                break;
                            }
                            case 1548: //room 6 
                            {
                                Program.Room6 = false;
                                break;
                            }
                        }

                        client.Entity.Teleport(1002, 299, 281);
                        client.Entity.aWinner = false;
                    }
                }

                #endregion

                var now32 = new Time32(time);
                var now64 = DateTime.Now;

                if (!client.Entity.Titles.IsEmpty) {
                    foreach (var titles in client.Entity.Titles) {
                        if (now64 > titles.Value) {
                            client.Entity.RemoveTopStatus((UInt64)titles.Key);
                        }
                    }
                }

                if (client.OnDonation) {
                    if (DateTime.Now >= client.matrixtime.AddHours(1.0)) {
                        SafeDictionary<uint, NobilityInformation> board =
                            new SafeDictionary<uint, NobilityInformation>(10000);
                        client.NobilityInformation.Donation -= client.Donationx;
                        board.Add(client.Entity.UID, client.NobilityInformation);
                        Database.NobilityTable.UpdateNobilityInformation(client.NobilityInformation);
                        Nobility.Sort(client.Entity.UID);
                        client.OnDonation = false;
                    }
                }

                if (client.Entity.attributes9 && (DateTime.Now > client.Entity.attributestime9.AddSeconds(80.0)) &&
                    client.Entity.StartTimer) {
                    client.Entity.MaxAttack -= 3000;
                    client.Entity.MinAttack -= 3000;
                    client.Entity.MaxHitpoints -= 3000;
                    client.Entity.Hitpoints -= 3000;
                    client.Entity.MagicAttack -= 3000;
                    client.Entity.attributes9 = false;
                }

                if (client.Entity.attributes8 && (DateTime.Now > client.Entity.attributestime8.AddSeconds(80.0)) &&
                    client.Entity.StartTimer) {
                    client.Entity.attributes8 = false;
                }

                if (client.Entity.attributes7 && (DateTime.Now > client.Entity.attributestime7.AddSeconds(80.0)) &&
                    client.Entity.StartTimer) {
                    client.Entity.Breaktrough -= 1500;
                    client.Entity.attributes7 = false;
                }

                if (client.Entity.attributes6 && (DateTime.Now > client.Entity.attributestime6.AddSeconds(80.0)) &&
                    client.Entity.StartTimer) {
                    client.Entity.CriticalStrike -= 15000;
                    client.Entity.SkillCStrike -= 15000;
                    client.Entity.attributes6 = false;
                }

                if (client.Entity.attributes5 && (DateTime.Now > client.Entity.attributestime5.AddSeconds(80.0)) &&
                    client.Entity.StartTimer) {
                    client.Entity.Counteraction -= 1500;
                    client.Entity.attributes5 = false;
                }

                if (client.Entity.attributes4 && (DateTime.Now > client.Entity.attributestime4.AddSeconds(80.0)) &&
                    client.Entity.StartTimer) {
                    client.Entity.Immunity -= 15000;
                    client.Entity.attributes4 = false;
                }

                if (client.Entity.attributes3 && (DateTime.Now > client.Entity.attributestime3.AddSeconds(80.0)) &&
                    client.Entity.StartTimer) {
                    client.Entity.PhysicalDamageIncrease -= 3000;
                    client.Entity.attributes3 = false;
                }

                if (client.Entity.attributes2 && (DateTime.Now > client.Entity.attributestime2.AddSeconds(80.0)) &&
                    client.Entity.StartTimer) {
                    client.Entity.MagicDamageIncrease -= 3000;
                    client.Entity.attributes2 = false;
                }

                if (client.Entity.attributes1 && (DateTime.Now > client.Entity.attributestime1.AddSeconds(80.0)) &&
                    client.Entity.StartTimer) {
                    client.Entity.PhysicalDamageDecrease -= 3000;
                    client.Entity.attributes1 = false;
                }

                if (client.Entity.attributes && (DateTime.Now > client.Entity.attributestime.AddSeconds(80.0)) &&
                    client.Entity.StartTimer) {
                    client.Entity.MagicDamageDecrease -= 3000;
                    client.Entity.attributes = false;
                }


                #region Training points

                if (client.Entity is { HeavenBlessing: > 0, Dead: false }) {
                    if (now32 > client.LastTrainingPointsUp.AddMinutes(10)) {
                        client.OnlineTrainingPoints += 10;
                        if (client.OnlineTrainingPoints >= 30) {
                            client.OnlineTrainingPoints -= 30;
                            client.IncreaseExperience(client.ExpBall / 100, false);
                        }

                        client.LastTrainingPointsUp = now32;
                        client.Entity.Update(Update.OnlineTraining, client.OnlineTrainingPoints, false);
                    }
                }

                #endregion

                #region Extra treasure points

                if (client.AllowedTreasurePoints) {
                    if (now32 > client.LastTreasurePoints.AddMinutes(1)) {
                        client.Entity.TreasuerPoints++;
                        client.LastTreasurePoints = Time32.Now;
                    }
                }

                #endregion

                #region Minning

                if (client is { Mining: true, Entity.Dead: false }) {
                    if (now32 >= client.MiningStamp.AddSeconds(2)) {
                        client.MiningStamp = now32;
                        Mining.Mine(client);
                    }
                }

                #endregion

                #region Class Fix With Auto Skill

                #region Trojan

                if (client.Entity.Class == 16) {
                    client.Entity.Class -= 1;
                }

                #endregion

                #region Warrior

                if (client.Entity.Class == 26) {
                    client.Entity.Class -= 1;
                }

                #endregion

                #region Archer

                if (client.Entity.Class == 46) {
                    client.Entity.Class -= 1;
                }

                #endregion

                #region Ninja

                if (client.Entity.Class == 56) {
                    client.Entity.Class -= 1;
                }

                #endregion

                #region Monk

                if (client.Entity.Class == 66) {
                    client.Entity.Class -= 1;
                }

                #endregion

                #region Pirate

                if (client.Entity.Class == 76) {
                    client.Entity.Class -= 1;
                }

                #endregion

                #region Leelong

                if (client.Entity.Class == 86) {
                    client.Entity.Class -= 1;
                }

                #endregion

                #region Toaist

                if (client.Entity.Class == 103) {
                    client.Entity.Class -= 1;
                }

                #endregion

                #region Water

                if (client.Entity.Class == 136) {
                    client.Entity.Class -= 1;
                }

                #endregion

                #region Fire

                if (client.Entity.Class == 146) {
                    client.Entity.Class -= 1;
                }

                #endregion

                #endregion

                #region MentorPrizeSave

                if (now32 > client.LastMentorSave.AddSeconds(5)) {
                    Database.KnownPersons.SaveApprenticeInfo(client.AsApprentice);
                    client.LastMentorSave = now32;
                }

                #endregion

                #region Attackable

                if (client.JustLoggedOn) {
                    client.JustLoggedOn = false;
                    client.ReviveStamp = now32;
                }

                if (!client.Attackable) {
                    if (now32 > client.ReviveStamp.AddSeconds(5)) {
                        client.Attackable = true;
                    }
                }

                #endregion

                #region DoubleExperience

                if (client.Entity.DoubleExperienceTime == 0 && client.SuperPotion > 0) {
                    client.SuperPotion = 0;
                }

                if (client.Entity.DoubleExperienceTime > 0) {
                    if (now32 >= client.Entity.DoubleExpStamp.AddMilliseconds(1000)) {
                        client.Entity.DoubleExpStamp = now32;
                        client.Entity.DoubleExperienceTime--;
                    }
                }

                #endregion

                #region HeavenBlessing

                if (client.Entity.HeavenBlessing > 0) {
                    if (now32 > client.Entity.HeavenBlessingStamp.AddMilliseconds(1000)) {
                        client.Entity.HeavenBlessingStamp = now32;
                        client.Entity.HeavenBlessing--;
                    }
                }

                #endregion

                #region Enlightment

                if (client.Entity.EnlightmentTime > 0) {
                    if (now32 >= client.Entity.EnlightmentStamp.AddMinutes(1)) {
                        client.Entity.EnlightmentStamp = now32;
                        client.Entity.EnlightmentTime--;
                        if (client.Entity.EnlightmentTime % 10 == 0 && client.Entity.EnlightmentTime > 0)
                            client.IncreaseExperience(Game.Attacking.Calculate.Percent((int)client.ExpBall, .10F),
                                false);
                    }
                }

                #endregion

                #region starTeam

                if (client is { Team: not null }) {
                    if (client.Entity.MapID == client.Team.Leader.Entity.MapID) {
                        var data = new Data(true) {
                            UID = client.Team.Leader.Entity.UID,
                            dwParam = client.Team.Leader.Entity.MapID,
                            ID = Data.TeamMemberPos,
                            wParam1 = client.Team.Leader.Entity.X,
                            wParam2 = client.Team.Leader.Entity.Y
                        };
                        data.Send(client);
                    }
                }

                #endregion

                #region PKPoints

                if (now32 >= client.Entity.PKPointDecreaseStamp.AddMinutes(5)) {
                    client.Entity.PKPointDecreaseStamp = now32;
                    if (client.Entity.PKPoints > 0) {
                        client.Entity.PKPoints--;
                    }
                    else
                        client.Entity.PKPoints = 0;
                }

                #endregion

                #region OverHP

                if (client.Entity.FullyLoaded) {
                    if (client.Entity.Hitpoints > client.Entity.MaxHitpoints && client.Entity is
                            { MaxHitpoints: > 1, Transformed: false }) {
                        client.Entity.Hitpoints = client.Entity.MaxHitpoints;
                    }
                }

                #endregion

                #region Room

                #region Room

                if (client.Entity.MapID == 1543) {
                    if (client.Entity.MapID == 1543) {
                        client.Entity.RemoveFlag(Update.Flags.ShurikenVortex);
                        client.Entity.RemoveFlag(Update.Flags.ReflectMelee);
                    }
                }

                #endregion

                #region Room

                if (client.Entity.MapID == 1544) {
                    if (client.Entity.MapID == 1544) {
                        client.Entity.RemoveFlag(Update.Flags.ShurikenVortex);
                        client.Entity.RemoveFlag(Update.Flags.ReflectMelee);
                    }
                }

                #endregion

                #region Room

                if (client.Entity.MapID == 1545) {
                    if (client.Entity.MapID == 1545) {
                        client.Entity.RemoveFlag(Update.Flags.ShurikenVortex);
                        client.Entity.RemoveFlag(Update.Flags.ReflectMelee);
                    }
                }

                #endregion

                #region Room

                if (client.Entity.MapID == 1546) {
                    if (client.Entity.MapID == 1546) {
                        client.Entity.RemoveFlag(Update.Flags.ShurikenVortex);
                        client.Entity.RemoveFlag(Update.Flags.ReflectMelee);
                    }
                }

                #endregion

                #region Room

                if (client.Entity.MapID == 1547) {
                    if (client.Entity.MapID == 1547) {
                        client.Entity.RemoveFlag(Update.Flags.ShurikenVortex);
                        client.Entity.RemoveFlag(Update.Flags.ReflectMelee);
                    }
                }

                #endregion

                #region Room

                if (client.Entity.MapID == 1548) {
                    if (client.Entity.MapID == 1548) {
                        client.Entity.RemoveFlag(Update.Flags.ShurikenVortex);
                        client.Entity.RemoveFlag(Update.Flags.ReflectMelee);
                    }
                }

                #endregion

                #endregion

                #region Die Delay

                if (client.Entity.Hitpoints == 0 && client.Entity.ContainsFlag(Update.Flags.Dead) &&
                    !client.Entity.ContainsFlag(Update.Flags.Ghost)) {
                    if (now32 > client.Entity.DeathStamp.AddSeconds(2)) {
                        client.Entity.AddFlag(Update.Flags.Ghost);
                        client.Entity.TransformationID = client.Entity.Body % 10 < 3 ? (ushort)99 : (ushort)98;

                        client.SendScreenSpawn(client.Entity, true);
                    }
                }

                #endregion

                #region OverVigor

                /* if (client.Entity.FullyLoaded)
            {
                if (client.Vigor > client.Entity.ExtraVigor)
                {
                    client.Vigor = client.Entity.ExtraVigor;
                }
            }*/

                #endregion

                #region ChainBolt

                if (client.Entity.ContainsFlag2(Update.Flags2.ChainBoltActive))
                    if (now32 > client.Entity.ChainboltStamp.AddSeconds(client.Entity.ChainboltTime))
                        client.Entity.RemoveFlag2(Update.Flags2.ChainBoltActive);

                #endregion

                if (client.Entity.HasMagicDefender &&
                    now32 >= client.Entity.MagicDefenderStamp.AddSeconds(client.Entity.MagicDefenderSecs)) {
                    client.Entity.RemoveMagicDefender();
                }

                if (now32 >= client.Entity.BlackbeardsRageStamp.AddSeconds(60)) {
                    client.Entity.RemoveFlag2(Update.Flags2.BlackbeardsRage);
                }

                if (now32 >= client.Entity.CannonBarrageStamp.AddSeconds(60)) {
                    client.Entity.RemoveFlag2(Update.Flags2.CannonBarrage);
                }

                if (now32 >= client.Entity.FatigueStamp.AddSeconds(client.Entity.FatigueSecs)) {
                    client.Entity.RemoveFlag2(Update.Flags2.Fatigue);
                    client.Entity.IsDefensiveStance = false;
                }

                if (now32 > client.Entity.GuildRequest.AddSeconds(30)) {
                    client.GuildJoinTarget = 0;
                }


                #region Equipment

                if (client.Entity.MapID == 1036) {
                    if (Kernel.GetDistance(client.Entity.X, client.Entity.Y, 184, 205) < 17 && !client.Effect) {
                        client.Effect = true;
                        if (client.Entity.MapID == 1036) {
                            FloorItem floorItem = new FloorItem(true) {
                                ItemID = 812,
                                MapID = 1036,
                                X = 184,
                                Y = 205,
                                Type = FloorItem.Effect
                            };
                            client.Send(floorItem);
                        }
                    }
                    else {
                        if (Kernel.GetDistance(client.Entity.X, client.Entity.Y, 184, 205) > 17) {
                            client.Effect = false;
                        }
                    }
                }

                #endregion

                #region Team Qualifier

                if ((now64.Hour == 11 || now64.Hour == 19) && now64 is { Minute: 19, Second: 2 }) {
                    client.MessageBox("TeamArena has started! It will open for two hours! Would you like to sign up?",
                        TeamArena.QualifyEngine.DoSignup,
                        (p) => { p.Send("You can still join from the team arena interface!"); });
                }

                #endregion

                #region Weekly PK

                if (now64 is { Second: <= 2, DayOfWeek: DayOfWeek.Saturday, Hour: 20, Minute: 00 }) {
                    client.MessageBox("Weekly PK has begun! Would you like to join? Prize [ TOP And Cps]",
                        (p) => { p.Entity.Teleport(1002, 327, 194); }, null, 60);
                }

                #endregion

                #region Night

                if (Rates.Night == 1) {
                    if (client.Entity.MapID == 701) {
                        Random disco = new Random();
                        uint discocolor = (uint)disco.Next(50000, 999999999);
                        Data datas = new Data(true) {
                            UID = client.Entity.UID,
                            ID = 104,
                            dwParam = discocolor
                        };
                        client.Send(datas);
                    }
                    else {
                        if (DateTime.Now.Minute >= 40 && DateTime.Now.Minute <= 45) {
                            Data datas = new Data(true) {
                                UID = client.Entity.UID,
                                ID = 104,
                                dwParam = 5855577
                            };
                            client.Send(datas);
                        }
                        else {
                            Data datas = new Data(true) {
                                UID = client.Entity.UID,
                                ID = 104,
                                dwParam = 0
                            };
                            client.Send(datas);
                        }
                    }
                }

                #endregion
            }
        }

        private void AutoAttackCallback(GameState client, int time) {
            if (!Valid(client)) return;
            var now = new Time32(time);
            if (client.Entity.AttackPacket == null && client.Entity.VortexPacket == null) return;
            try {
                if (client.Entity.ContainsFlag(Update.Flags.ShurikenVortex)) {
                    if (client.Entity.VortexPacket?.ToArray() == null) return;
                    if (now <= client.Entity.VortexAttackStamp.AddMilliseconds(1400)) return;
                    client.Entity.VortexAttackStamp = now;
                    client.Entity.VortexPacket.AttackType = Attack.Magic;
                    _ = new Game.Attacking.Handle(client.Entity.VortexPacket, client.Entity, null);
                }
                else {
                    var attackPacket = client.Entity.AttackPacket;
                    attackPacket?.ToArray();
                    if (attackPacket == null) return;
                    var attackType = attackPacket.AttackType;
                    if (attackType != Attack.Magic && attackType != Attack.Melee &&
                        attackType != Attack.Ranged) return;
                    _ = new Game.Attacking.Handle(attackPacket, client.Entity, null);
                    if (attackType == Attack.Magic) {
                        if (now <= client.Entity.AttackStamp.AddSeconds(1)) return;
                        if (attackPacket.Damage != 12160 &&
                            attackPacket.Damage != 12170 &&
                            attackPacket.Damage != 12120 &&
                            attackPacket.Damage != 12130 &&
                            attackPacket.Damage != 12140 &&
                            attackPacket.Damage != 12320 &&
                            attackPacket.Damage != 12330 &&
                            attackPacket.Damage != 12340 &&
                            attackPacket.Damage != 12570 &&
                            attackPacket.Damage != 12210) { }
                    }

                    else {
                        var decrease = 300;
                        if (client.Entity.OnCyclone())
                            decrease = 700;
                        if (client.Entity.OnSuperman())
                            decrease = 200;
                        if (now > client.Entity.AttackStamp.AddMilliseconds(
                                (1000 - client.Entity.Agility - decrease) * (1))) { }
                    }
                }
            }
            catch (Exception e) {
                Program.SaveException(e);
                client.Entity.AttackPacket = null;
                client.Entity.VortexPacket = null;
            }
        }

        private void PrayerCallback(GameState client, int time) {
            if (!Valid(client)) return;

            if (client.Entity.Reborn > 1)
                return;

            if (!client.Entity.ContainsFlag(Update.Flags.Praying)) {
                foreach (var clientObj in client.Screen.Objects) {
                    if (clientObj.MapObjType != MapObjectType.Player) continue;
                    var clientObjOwner = clientObj.Owner;
                    if (!clientObjOwner.Entity.ContainsFlag(Update.Flags.CastPray)) continue;
                    if (Kernel.GetDistance(client.Entity.X, client.Entity.Y, clientObj.X, clientObj.Y) > 3) continue;
                    client.Entity.AddFlag(Update.Flags.Praying);
                    client.PrayLead = clientObjOwner;
                    client.Entity.Action = clientObjOwner.Entity.Action;
                    clientObjOwner.Prayers.Add(client);
                    break;
                }
            }
            else {
                if (client.PrayLead != null && Kernel.GetDistance(client.Entity.X, client.Entity.Y,
                        client.PrayLead.Entity.X,
                        client.PrayLead.Entity.Y) <= 4) return;
                client.Entity.RemoveFlag(Update.Flags.Praying);
                client.PrayLead?.Prayers.Remove(client);
                client.PrayLead = null;
            }
        }

        private void WorldTournaments(int time) {
            DateTime now64 = DateTime.Now;

            #region Event System

            // Update all scheduled events
            Game.Events.EventScheduler.Update(DateTime.Now);

            #endregion

            HeroOfGame.CheakUp();
            if (MatrixTimes.Start.SkillTeam && !TeamElitePk.SkillTeamTournament.Opened) {
                TeamElitePk.SkillTeamTournament.Open();
                foreach (GameState client in Kernel.GamePool.Values) {
                    client.ClaimedSkillTeam = 0;
                    if (client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Entity.Dead) {
                        EventAlert alert = new EventAlert {
                            StrResID = 10541,
                            Countdown = 60,
                            UK12 = 1
                        };
                        client.Entity.StrResID = 10541;
                        client.Send(alert);
                    }
                }
            }

            if (MatrixTimes.Start.TeamPk && !TeamElitePk.TeamTournament.Opened) {
                TeamElitePk.TeamTournament.Open();
                foreach (GameState client in Kernel.GamePool.Values) {
                    client.ClaimedTeampk = 0;
                    if (client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Entity.Dead) {
                        EventAlert alert = new EventAlert {
                            StrResID = 10543,
                            Countdown = 60,
                            UK12 = 1
                        };
                        client.Entity.StrResID = 10543;
                        client.Send(alert);
                    }
                }
            }

            #region Couples PK War

            if (DateTime.Now.DayOfWeek == DayOfWeek.Friday && DateTime.Now.Hour == 19 && DateTime.Now.Minute == 30 &&
                DateTime.Now.Second == 1) {
                Kernel.SendWorldMessage(
                    new Message(
                        "Couples PkWar has started! You have 5 minute to signup go to TC CouplesPkGuide in TwinCity!",
                        Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    if (client.Entity.Spouse != "None")
                        client.MessageBox(
                            "CouplesPk War has started! Would you like to join? [Prize: " + 5000000 + " CPs]",
                            p => { p.Entity.Teleport(1002, 275, 187); });
            }

            #endregion

            #region cycolne race

            if (DateTime.Now.Minute == 57 && DateTime.Now.Second == 1) {
                _cyclone3 = true;
                Entity.Speed = 0;
                foreach (var client in Program.Values)
                    client.MessageBox("Cyclone Race Start U Like To Join And Get " + 100000 + " CPS ",
                        p => { p.Entity.Teleport(1002, 308, 235); });
            }

            if (DateTime.Now.Minute == 58 && DateTime.Now.Second == 1) {
                _cyclone1 = true;
            }

            if (DateTime.Now.Minute == 59 && _cyclone3) {
                _cyclone3 = false;
                _cyclone1 = false;
            }

            #endregion

            //

            #region Elite GW

            {
                foreach (var client in Program.Values)
                    if (client.Entity.MapID == 6000 || client.Entity.MapID == 6001 || client.Entity.MapID == 6002 ||
                        client.Entity.MapID == 6003 || client.Entity.MapID == 6004)
                        return;
                if (!EliteGuildWar.IsWar) {
                    if (now64 is { Minute: 15, Second: 01 }) {
                        EliteGuildWar.Start();
                        foreach (var client in Program.Values)
                            if (client.Entity.GuildID != 0)
                                client.MessageBox("Elite GuildWar has begun! Would you like to join?",
                                    p => { p.Entity.Teleport(1002, 286, 158); });
                    }
                }

                if (EliteGuildWar.IsWar) {
                    if (Time32.Now > EliteGuildWar.ScoreSendStamp.AddSeconds(3)) {
                        EliteGuildWar.ScoreSendStamp = Time32.Now;
                        EliteGuildWar.SendScores();
                    }

                    if (now64 is { Minute: 25, Second: <= 02 }) {
                        Kernel.SendWorldMessage(
                            new Message("5 Minutes left till Elite GuildWar End Hurry kick other Guild's Ass!.",
                                Color.White, Message.Center), Program.Values);
                    }
                }

                if (EliteGuildWar.IsWar) {
                    if (now64 is { Minute: 29, Second: 58 }) {
                        EliteGuildWar.End();
                        {
                            Kernel.SendWorldMessage(
                                new Message("Elite Guild War Ended Thanks To MTA.", Color.White, Message.Center),
                                Program.Values);
                        }
                    }
                }
            }

            #endregion

            #region Clan War

            if ((now64.Hour == 21 || now64.Hour == 16) && now64 is { Minute: 00, Second: 05 } && !ClanWar.IsWar) {
                ClanWar.Start();
                _clanWarAi = false;
                if (now64.Hour != 16) {
                    _clanWarAi = now64.Hour != 16;
                    foreach (var client in Program.Values)
                        if (client.Entity.GuildID != 0)
                            client.MessageBox("ClanWar Has Begun! Would You Like To Join This War ...?",
                                p => { p.Entity.Teleport(1002, 284, 146); });
                }
            }

            if (now64 is { Hour: 16, Minute: 10 } && !_clanWarAi) {
                _clanWarAi = true;
                foreach (var client in Program.Values)
                    if (client.Entity.GuildID != 0)
                        client.MessageBox("ClanWar Has Begun! Would You Like To Join This War ...?",
                            p => { p.Entity.Teleport(1002, 284, 146); });
            }

            if ((now64.Hour == 22 || now64.Hour == 17) && now64.Minute == 00 && ClanWar.IsWar) {
                ClanWar.End();
            }

            if (ClanWar.IsWar) {
                if (Time32.Now > ClanWar.ScoreSendStamp.AddSeconds(3)) {
                    ClanWar.ScoreSendStamp = Time32.Now;
                    ClanWar.SendScores();
                }
            }

            #endregion

            #region Dis City

            if (now64.DayOfWeek == DayOfWeek.Wednesday || now64.DayOfWeek == DayOfWeek.Sunday) {
                if ((now64.Hour == 12 || now64.Hour == 19) && now64 is { Minute: 05, Second: 2 }) {
                    Kernel.SendWorldMessage(
                        new Message("DisCity signup has been closed. Please try next time!", Color.White,
                            Message.Center), Program.Values);

                    Game.Features.DisCity.Signup = false;
                }
            }

            #endregion

            #region Class PK

            if (now64 is { Hour: 20, Minute: 30, Second: 0 } ||
                now64 is { Hour: 8, Minute: 30, Second: 0 }) {
                Kernel.SendWorldMessage(
                    new Message("Class PK War began! all Go Twin 302, 148", Color.White, Message.Center),
                    Program.Values);
            }

            #endregion

            #region Monthly PK

            if (now64 is { Day: <= 7, DayOfWeek: DayOfWeek.Sunday }) {
                if (now64 is { Hour: 21, Minute: >= 50, Second: 2 }) {
                    int min = 60 - now64.Minute;
                    Kernel.SendWorldMessage(new Message("MonthelyPk " + min.ToString() + " Minute!", Color.Red, 2012));
                }

                if (now64 is { Hour: 22, Minute: 00, Second: <= 2 }) {
                    MonthlyPkWar = true;
                    foreach (GameState client in Kernel.GamePool.Values) {
                        if (client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Entity.Dead) {
                            EventAlert alert = new EventAlert {
                                StrResID = 10523,
                                Countdown = 60,
                                UK12 = 1
                            };
                            client.Entity.StrResID = 10523;
                            client.Send(alert);
                        }
                    }
                }

                if (now64 is { Hour: 22, Minute: >= 15 } && MonthlyPkWar) {
                    MonthlyPkWar = false;
                    Kernel.SendWorldMessage(new Message("MonthelyPk Ended!", Color.Red, Message.Center));
                }
            }

            #endregion

            #region LuckyMan

            if (now64 is { Minute: 43, Second: 5 }) {
                Kernel.SendWorldMessage(new Message("Lucky Man War began !", Color.White, Message.Center),
                    Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("Lucky Man began! Would you Like to join ...?",
                        p => { p.Entity.Teleport(1002, 288, 360); }, null, 60);
            }

            #endregion

            #region SuperGuildWar

            if (now64 is { Hour: 20, Minute: 5, Second: 0 } &&
                (now64.Day == 1 || now64.Day == 7 || now64.Day == 14 || now64.Day == 21)) {
                Kernel.SendWorldMessage(
                    new Message("Super Guild War now work will end at [23:00] Server time? !", Color.White,
                        Message.BroadcastMessage), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("Super Guild War now work will end at 23:00 Server time?",
                        p => { p.Entity.Teleport(1038, 348, 337); }, null, 60);
            }

            #endregion


            //     #region uniqureKiller
            //   if ()
            //      if (DateTime.Now.Hour == 18 && DateTime.Now.Minute == 19 && DateTime.Now.Second == 59)
            //      {
            //          Program.UniquePk = true;
            //           Kernel.SendWorldMessage(new Message("UniqueKiller War began!", Color.Red, Message.Center), Program.Values);
            //          foreach (var client in Program.Values)

            //             client.MessageBox("UniqueKiller began! Would you like to join ...?",
            //                 p => { p.Entity.Teleport(1002, 255, 235); }, null, 60);
            //    }
            //    #endregion
            //////////////////////

            #region PoleIslanD

            if (!PoleIslanD.IsWar) {
                if (now64 is { Hour: 16, Minute: 00, Second: 35 }) {
                    PoleIslanD.Start();

                    foreach (var client in Program.Values)
                        if (client.Entity.MapID == 6000 || client.Entity.MapID == 6001 || client.Entity.MapID == 6002 ||
                            client.Entity.MapID == 6003 || client.Entity.MapID == 6004)
                            return;
                    foreach (var client in Program.Values)
                        if (client.Entity.GuildID != 0)
                            client.MessageBox("PoleIslanD has begun! Would you like to join? ",
                                p => { p.Entity.Teleport(1002, 298, 230); });
                }
            }

            if (PoleIslanD.IsWar) {
                if (Time32.Now > PoleIslanD.ScoreSendStamp.AddSeconds(3)) {
                    PoleIslanD.ScoreSendStamp = Time32.Now;
                    PoleIslanD.SendScores();
                }

                if (now64 is { Hour: 16, Minute: 50, Second: <= 2 }) {
                    Kernel.SendWorldMessage(
                        new Message("10 Minutes left till PoleIslanD End Hurry kick other Guild's Ass!.", Color.White,
                            Message.Center), Program.Values);
                }
            }

            if (PoleIslanD.IsWar) {
                if (now64 is { Hour: 17, Minute: 00, Second: 04 }) {
                    PoleIslanD.End();
                    { }
                }
            }

            #endregion

            #region PoleRakion

            if (!PoleRakion.IsWar) {
                if (now64 is { Hour: 22, Minute: 00, Second: 35 }) {
                    PoleRakion.Start();

                    foreach (var client in Program.Values)
                        if (client.Entity.MapID == 6000 || client.Entity.MapID == 6001 || client.Entity.MapID == 6002 ||
                            client.Entity.MapID == 6003 || client.Entity.MapID == 6004)
                            return;
                    foreach (var client in Program.Values)
                        if (client.Entity.GuildID != 0)
                            client.MessageBox("PoleRakion has begun! Would you like to join? ",
                                p => { p.Entity.Teleport(1002, 249, 215); });
                }
            }

            if (PoleRakion.IsWar) {
                if (Time32.Now > PoleRakion.ScoreSendStamp.AddSeconds(3)) {
                    PoleRakion.ScoreSendStamp = Time32.Now;
                    PoleRakion.SendScores();
                }

                if (now64 is { Hour: 22, Minute: 50, Second: <= 2 }) {
                    Kernel.SendWorldMessage(
                        new Message("10 Minutes left till PoleRakion End Hurry kick other Guild's Ass!.", Color.White,
                            Message.Center), Program.Values);
                }
            }

            if (PoleRakion.IsWar) {
                if (now64 is { Hour: 23, Minute: 00, Second: 04 }) {
                    PoleRakion.End();
                    { }
                }
            }

            #endregion

            /////////////////////
            ////////////////////New Quests Adedd By Franko///////////////////

            #region Portals`War

            if (DateTime.Now.Minute == 10 && now64.Second == 10) {
                Kernel.SendWorldMessage(
                    new Message(" PortalsWar Pk, Now Online All Go To Play PK, !", Color.White, Message.Center),
                    Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(" PortalsWar Pk, Now Online, like to Join? ",
                        p => { p.Entity.Teleport(1002, 291, 360); }, null, 60);
            }

            #endregion

            #region ConquerPK PK

            if (DateTime.Now.Minute == 01 && now64.Second == 10) {
                Kernel.SendWorldMessage(
                    new Message(" ConquerPK Pk, Now Online All Go To Play PK, !", Color.White, Message.Center),
                    Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(" ConquerPK Pk, Now Online, like to Join? ",
                        p => { p.Entity.Teleport(1002, 274, 360); }, null, 60);
            }

            #endregion

            #region PolePrize

            if (DateTime.Now.Minute == 05 && now64.Second == 01) {
                Kernel.SendWorldMessage(
                    new Message("The War PolePrize Is Started Now ,, End This War at xx:10", Color.White,
                        Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("PolePrize Start Now Let's go Fast? ",
                        p => { p.Entity.Teleport(1002, 230, 227); }, null, 60);
            }

            #endregion

            #region Ghost PK

            if (DateTime.Now.Minute == 06 && now64.Second == 10) {
                Kernel.SendWorldMessage(
                    new Message(" Ghost Pk, Now Online All Go To Play PK !", Color.White, Message.Center),
                    Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(" Ghostpk Pk, Now Online, like to Join? ",
                        p => { p.Entity.Teleport(1002, 300, 361); }, null, 60);
            }

            #endregion

            #region StayAlive PK

            if (DateTime.Now.Minute == 16 && now64.Second == 10) {
                Kernel.SendWorldMessage(
                    new Message(" StayAlive Pk, Now Online All Go To Play PK !", Color.White, Message.Center),
                    Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(" StayAlive Pk, Now Online, like to Join? ",
                        p => { p.Entity.Teleport(1002, 297, 360); }, null, 60);
            }

            #endregion

            #region PrinceWar

            if (DateTime.Now.Minute == 23 && now64.Second == 1) {
                Kernel.SendWorldMessage(
                    new Message("(PrinceWar Pk, Now Online All Go To Play PK", Color.White, Message.Center),
                    Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(" PrinceWar, Now Online, like to Join? ",
                        p => { p.Entity.Teleport(1002, 274, 362); }, null, 60);
            }

            #endregion

            #region Attackers QuesT

            if (DateTime.Now.Minute == 32 && now64.Second == 1) {
                Kernel.SendWorldMessage(
                    new Message(" Attackers  Pk, Now Online All Go To Play PK !", Color.White, Message.Center),
                    Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(" Attackers QuesT Pk, Now Online, like to Join?",
                        (p) => { p.Entity.Teleport(1002, 294, 360); }, null, 60);
            }

            #endregion

            #region Rabbit PK

            if (DateTime.Now.Minute == 38 && now64.Second == 1) {
                Kernel.SendWorldMessage(
                    new Message(" Rabbit Pk, Now Online All Go To Play PK !", Color.White, Message.Center),
                    Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(" Rabbit Pk, Now Online, like to Join? ",
                        p => { p.Entity.Teleport(1002, 277, 360); }, null, 60);
            }

            #endregion

            #region RevengerWar

            if (DateTime.Now.Minute == 47 && now64.Second == 1) {
                Kernel.SendWorldMessage(
                    new Message(" ReVenger, Now Online All Go To Play PK,!", Color.White, Message.Center),
                    Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(" ReVengerWar Pk, Now Online, like to Join? ",
                        (p) => { p.Entity.Teleport(1002, 279, 360); }, null, 60);
            }

            #endregion

            #region Dead World

            if (DateTime.Now.Minute == 53 && now64.Second == 1) {
                Kernel.SendWorldMessage(
                    new Message(" Dead World Pk, Now Online All Go To Play PK!", Color.White, Message.Center),
                    Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(" Dead World Pk, Now Online, like to Join? ",
                        p => { p.Entity.Teleport(1002, 282, 360); }, null, 60);
            }

            #endregion

            #region MemberAlter

            if (DateTime.Now.Minute == 57 && now64.Second == 10) {
                Kernel.SendWorldMessage(
                    new Message(" MemberAlter, Now Online All Go To Play PK,!", Color.White, Message.Center),
                    Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("War, MemberAlter, Now Online, like to Join? ",
                        p => { p.Entity.Teleport(1002, 285, 360); }, null, 60);
            }

            #endregion

            #region [T]KingDom.GLD

            if (now64 is { Second: <= 2, DayOfWeek: DayOfWeek.Monday, Hour: 18, Minute: 48 }) {
                Kernel.SendWorldMessage(
                    new Message(
                        "((#42))War [T]KingDom.GLD, Now Online All Go To Play PK,((#41))--((#50))Monday, 18:48 To 18:55((#50)) !",
                        Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("KingDom.GLD PK, Now Online, like to Join?",
                        p => { p.Entity.Teleport(1002, 255, 235); }, null, 60);
            }

            #endregion

            #region [T]KingDom.DLD

            if (now64 is { Second: <= 2, DayOfWeek: DayOfWeek.Tuesday, Hour: 18, Minute: 48 }) {
                Kernel.SendWorldMessage(
                    new Message(
                        "((#42))War [T]KingDom.DLD, Now Online All Go To Play PK,((#41))--((#50))Tuesday, 18:48 To 18:55((#50)) !",
                        Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("KingDom.DLD PK, Now Online, like to Join?",
                        p => { p.Entity.Teleport(1002, 255, 235); }, null, 60);
            }

            #endregion

            /////////////////////////////////////////////

            #region Mr/Ms Conquer

            //   if ()
            if (DateTime.Now.Hour == 19 && DateTime.Now.Minute == 31 && now64.Second == 15) {
                Kernel.SendWorldMessage(
                    new Message("Mr/Ms Conquer War began! Go Twin city ", Color.Red, Message.BroadcastMessage),
                    Program.Values);
                foreach (var client in Program.Values)

                    client.MessageBox("Mr/Ms Conquer  began! Would you like to join Priz ?",
                        p => { p.Entity.Teleport(1002, 288, 192); }, null, 60);
            }

            #endregion

            #region Topguild

            if (now64 is { Minute: 35, Second: 10 }) {
                Kernel.SendWorldMessage(new Message("Hero Guild War began !", Color.White, Message.Center),
                    Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("Hero Guild began! Would you like to join ?",
                        p => { p.Entity.Teleport(1002, 313, 143); }, null, 60);
            }

            #endregion

            #region LastTeam Fight

            if (now64 is { Minute: 13, Second: 10 }) {
                Kernel.SendWorldMessage(new Message("Last Team Fight began !", Color.White, Message.Center),
                    Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("Last Team Fight began! Would you like to join ?",
                        p => { p.Entity.Teleport(1002, 289, 143); }, null, 60);
            }

            #endregion

            #region Team & SKill PK

            if (MatrixTimes.Start.TeamPk && !TeamElitePk.TeamTournament.Opened) {
                Kernel.SendWorldMessage(
                    new Message(
                        "The Team PK Tournament has start at 19:00. Prepare yourself and sign up for it as a team!",
                        Color.White, Message.BroadcastMessage), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("The Team PK Tournament began! Would you like to join Prize [100kk] First Rank?",
                        p => { p.Entity.Teleport(1002, 440, 249); }, null, 60);
            }

            if (MatrixTimes.Start.SkillTeam && !TeamElitePk.SkillTeamTournament.Opened) {
                Kernel.SendWorldMessage(
                    new Message(
                        "The Skill Team PK Tournament will start at 10:00. Prepare yourself and sign up for it as a team!",
                        Color.White, Message.BroadcastMessage), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(
                        "The Skill Team PK Tournament began! Would you like to join, Prize [100kk] First Rank?",
                        p => { p.Entity.Teleport(1002, 445, 242); }, null, 60);
            }

            #endregion

            #region GuildWar

            if (GuildWar.IsWar) {
                if (Time32.Now > GuildWar.ScoreSendStamp.AddSeconds(3)) {
                    GuildWar.ScoreSendStamp = Time32.Now;
                    GuildWar.SendScores();
                }
            }

            if (now64.Hour is >= 20 and <= 21 && now64.DayOfWeek == DayOfWeek.Friday) {
                if (!GuildWar.IsWar) {
                    GuildWar.Start();
                    foreach (GameState client in Kernel.GamePool.Values) {
                        client.Entity.DeputyLeader = 0;
                        if (client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Entity.Dead) {
                            EventAlert alert = new EventAlert {
                                StrResID = 10515,
                                Countdown = 60,
                                UK12 = 1
                            };
                            client.Entity.StrResID = 10515;
                            client.Send(alert);
                        }
                    }
                }
            }

            if (GuildWar.IsWar) {
                if (now64 is { Hour: 21, Second: <= 2 }) {
                    GuildWar.Flame10th = false;
                    GuildWar.End();
                }
            }

            #endregion

            #region SuperGuildWar

            if (SuperGuildWar.IsWar) {
                if (Time32.Now > SuperGuildWar.ScoreSendStamp.AddSeconds(3)) {
                    SuperGuildWar.ScoreSendStamp = Time32.Now;
                    SuperGuildWar.SendScores();
                }
            }

            if (now64.Hour is >= 01 and <= 20 &&
                (now64.Day == 1 || now64.Day == 7 || now64.Day == 14 || now64.Day == 21)) {
                if (!SuperGuildWar.IsWar) {
                    SuperGuildWar.Start();
                    foreach (var client in Program.Values)
                        if (client.Entity.GuildID != 0)
                            client.MessageBox("Super GuildWar has begun! Would you like to join?",
                                p => { p.Entity.Teleport(1002, 352, 337); });
                }
            }

            if (SuperGuildWar.IsWar) {
                if (now64 is { Hour: 23, Second: <= 2 }) {
                    SuperGuildWar.End();
                }
            }

            #endregion

            #region Elite PK Tournament

            if ((now64.Hour == ElitePK.EventTime) && now64.Minute >= 55 && !ElitePKTournament.TimersRegistered) {
                ElitePK.EventTime = DateTime.Now.Hour;
                ElitePKTournament.RegisterTimers();
                ElitePKBrackets brackets = new ElitePKBrackets(true) {
                    Type = ElitePKBrackets.EPK_State,
                    OnGoing = true
                };
                foreach (var client in Program.Values) {
                    client.ClaimedElitePk = 0;
                    client.Send(brackets);
                    foreach (var unused in Kernel.GamePool.Values) {
                        if (client.Map.BaseID == 6001 || client.Map.BaseID == 6000 || client.Entity.Dead) continue;
                        var alert = new EventAlert {
                            StrResID = 10533,
                            Countdown = 60,
                            UK12 = 1
                        };
                        client.Entity.StrResID = 10533;
                        client.Send(alert);
                    }

                    #region RemoveTopElite

                    var eliteChampion = TitlePacket.Titles.ElitePKChamption_High;
                    var eliteSecond = TitlePacket.Titles.ElitePK2ndPlace_High;
                    var eliteThird = TitlePacket.Titles.ElitePK3ndPlace_High;
                    var eliteEightChampion = TitlePacket.Titles.ElitePKChamption_Low;
                    var eliteEightSecond = TitlePacket.Titles.ElitePK2ndPlace_Low;
                    var eliteEightThird = TitlePacket.Titles.ElitePK3ndPlace_Low;
                    var eliteEight = TitlePacket.Titles.ElitePKTopEight_Low;
                    if (client.Entity.Titles.ContainsKey(eliteChampion))
                        client.Entity.RemoveTopStatus((ulong)eliteChampion);
                    if (client.Entity.Titles.ContainsKey(eliteSecond))
                        client.Entity.RemoveTopStatus((ulong)eliteSecond);
                    if (client.Entity.Titles.ContainsKey(eliteThird))
                        client.Entity.RemoveTopStatus((ulong)eliteThird);
                    if (client.Entity.Titles.ContainsKey(eliteEightChampion))
                        client.Entity.RemoveTopStatus((ulong)eliteEightChampion);
                    if (client.Entity.Titles.ContainsKey(eliteEightSecond))
                        client.Entity.RemoveTopStatus((ulong)eliteEightSecond);
                    if (client.Entity.Titles.ContainsKey(eliteEightThird))
                        client.Entity.RemoveTopStatus((ulong)eliteEightThird);
                    if (client.Entity.Titles.ContainsKey(eliteEight))
                        client.Entity.RemoveTopStatus((ulong)eliteEight);

                    #endregion
                }
            }

            if (now64.Hour == ElitePK.EventTime + 1 && now64.Minute >= 10 && ElitePKTournament.TimersRegistered) {
                bool done = true;
                foreach (var epk in ElitePKTournament.Tournaments)
                    if (epk.Players.Count != 0)
                        done = false;
                if (done) {
                    ElitePKTournament.TimersRegistered = false;
                    ElitePKBrackets brackets = new ElitePKBrackets(true) {
                        Type = ElitePKBrackets.EPK_State,
                        OnGoing = false
                    };
                    foreach (var client in Program.Values)
                        client.Send(brackets);
                }
            }

            #endregion
        }

        private void ServerFunctions(int time) {
            var lastPerfectionSort = DateTime.Now;
            if (DateTime.Now >= lastPerfectionSort.AddMinutes(10)) {
                new PerfectionScore().GetRankingList();
                new PerfectionRank().UpdateRanking();
            }

            #region New weather

            Network.GamePackets.Weather weather;

            #region Rain System

            if (DateTime.Now.Minute == 10 && DateTime.Now.Second == 0 ||
                DateTime.Now.Minute == 00 && DateTime.Now.Second == 00) {
                foreach (GameState state in Kernel.GamePool.Values) {
                    Program.WeatherType = Network.GamePackets.Weather.Snow;
                    weather = new Network.GamePackets.Weather(true) {
                        WeatherType = (uint)Program.WeatherType,
                        Intensity = 255,
                        Appearence = 255,
                        Direction = 255
                    };
                    state.Send(weather);
                }
            }

            #endregion Rain System

            #region Snow System

            if (DateTime.Now.Minute == 20 && DateTime.Now.Second == 0 ||
                DateTime.Now.Minute == 00 && DateTime.Now.Second == 00) {
                foreach (GameState state in Kernel.GamePool.Values) {
                    Program.WeatherType = Network.GamePackets.Weather.Snow;
                    weather = new Network.GamePackets.Weather(true) {
                        WeatherType = (uint)Program.WeatherType,
                        Intensity = 255,
                        Appearence = 255,
                        Direction = 255
                    };
                    state.Send(weather);
                }
            }

            #endregion Snow System

            #region AutumnLeaves

            if (DateTime.Now.Minute == 30 && DateTime.Now.Second == 0 ||
                DateTime.Now.Minute == 00 && DateTime.Now.Second == 00) {
                foreach (GameState state in Kernel.GamePool.Values) {
                    Program.WeatherType = Network.GamePackets.Weather.Snow;
                    weather = new Network.GamePackets.Weather(true) {
                        WeatherType = (uint)Program.WeatherType,
                        Intensity = 255,
                        Appearence = 255,
                        Direction = 255
                    };
                    state.Send(weather);
                }
            }

            #endregion AutumnLeaves

            #region CherryBlossomPetals

            if (DateTime.Now.Minute == 40 && DateTime.Now.Second == 0 ||
                DateTime.Now.Minute == 00 && DateTime.Now.Second == 00) {
                foreach (GameState state in Kernel.GamePool.Values) {
                    Program.WeatherType = Network.GamePackets.Weather.Snow;
                    weather = new Network.GamePackets.Weather(true) {
                        WeatherType = (uint)Program.WeatherType,
                        Intensity = 255,
                        Appearence = 255,
                        Direction = 255
                    };
                    state.Send(weather);
                }
            }

            #endregion CherryBlossomPetals

            #region BlowingCotten

            if (DateTime.Now.Minute == 60 && DateTime.Now.Second == 0 ||
                DateTime.Now.Minute == 00 && DateTime.Now.Second == 00) {
                foreach (GameState state in Kernel.GamePool.Values) {
                    Program.WeatherType = Network.GamePackets.Weather.Snow;
                    weather = new Network.GamePackets.Weather(true) {
                        WeatherType = (uint)Program.WeatherType,
                        Intensity = 255,
                        Appearence = 255,
                        Direction = 255
                    };
                    state.Send(weather);
                }
            }

            #endregion CherryBlossomPetals

            #endregion New weather

            Kernel.GamePool.ToArray();

            Program.Values = Kernel.GamePool.Values.ToArray();

            Console.Title = Constants.ServerName + " - Online : " + Kernel.GamePool.Count + "/" + Program.PlayerCap;

            if (Kernel.GamePool.Count > Program.MaxOn) {
                Program.MaxOn = Kernel.GamePool.Count;
            }

            Console.Title = Constants.ServerName + " - Online : " + Kernel.GamePool.Count + "/" + Program.PlayerCap +
                            " (Peak: " + Program.MaxOn + ")";
            if (Constants.ServerName != null)
                new Database.MySqlCommand(Database.MySqlCommandType.UPDATE).Update("configuration")
                    .Set("GuildID", Game.ConquerStructures.Society.Guild.GuildCounter.Now)
                    .Set("MaxOnline", Program.MaxOn).Set("ItemUID", Program.NextItemId)
                    .Where("Server", Constants.ServerName).Execute();
            if (Program.Vars != null) Database.EntityVariableTable.Save(0, Program.Vars);
            if (Kernel.BlackSpoted.Values.Count > 0) {
                foreach (var spot in Kernel.BlackSpoted.Values) {
                    if (Time32.Now >= spot.BlackSpotStamp.AddSeconds(spot.BlackSpotStepSecs)) {
                        if (spot is { Dead: true, EntityFlag: EntityFlag.Player }) {
                            foreach (var h in Program.Values) {
                                h.Send(Program.BlackSpotPacket.ToArray(false, spot.UID));
                            }

                            Kernel.BlackSpoted.Remove(spot.UID);
                            continue;
                        }

                        foreach (var h in Program.Values) {
                            h.Send(Program.BlackSpotPacket.ToArray(false, spot.UID));
                        }

                        spot.IsBlackSpotted = false;
                        Kernel.BlackSpoted.Remove(spot.UID);
                    }
                }
            }

            var now = DateTime.Now;

            if (now > Game.ConquerStructures.Broadcast.LastBroadcast.AddMinutes(1)) {
                if (Game.ConquerStructures.Broadcast.Broadcasts.Count > 0) {
                    Game.ConquerStructures.Broadcast.CurrentBroadcast = Game.ConquerStructures.Broadcast.Broadcasts[0];
                    Game.ConquerStructures.Broadcast.Broadcasts.Remove(
                        Game.ConquerStructures.Broadcast.CurrentBroadcast);
                    Game.ConquerStructures.Broadcast.LastBroadcast = now;
                    Kernel.SendWorldMessage(
                        new Message(Game.ConquerStructures.Broadcast.CurrentBroadcast.Message, "ALLUSERS",
                            Game.ConquerStructures.Broadcast.CurrentBroadcast.EntityName, Color.Red,
                            Message.BroadcastMessage), Program.Values);
                }
                else
                    Game.ConquerStructures.Broadcast.CurrentBroadcast.EntityID = 1;
            }


            if (now > Program.LastRandomReset.AddMinutes(30)) {
                Program.LastRandomReset = now;
                Kernel.Random = new FastRandom(Program.RandomSeed);
            }

            Program.Today = now.DayOfWeek;
        }

        private void ArenaFunctions(int time) {
            Arena.EngagePlayers();
            Arena.CheckGroups();
            Arena.VerifyAwaitingPeople();
            Arena.Reset();
        }

        private void TeamArenaFunctions(int time) {
            TeamArena.PickUpTeams();
            TeamArena.EngagePlayers();
            TeamArena.CheckGroups();
            TeamArena.VerifyAwaitingPeople();
            TeamArena.Reset();
        }

        private void ChampionFunctions(int time) {
            Champion.EngagePlayers();
            Champion.CheckGroups();
            Champion.VerifyAwaitingPeople();
            Champion.Reset();
        }

        #region Funcs

        public static void Execute(Action<int> action, int timeOut = 0,
            ThreadPriority priority = ThreadPriority.Normal) {
            GenericThreadPool?.Subscribe(new LazyDelegate(action, timeOut, priority));
        }

        public static void Execute<T>(Action<T, int> action, T param, int timeOut = 0,
            ThreadPriority priority = ThreadPriority.Normal) {
            GenericThreadPool?.Subscribe(new LazyDelegate<T>(action, timeOut, priority), param);
        }

        public static IDisposable? Subscribe(Action<int> action, int period = 1,
            ThreadPriority priority = ThreadPriority.Normal) {
            return GenericThreadPool?.Subscribe(new TimerRule(action, period, priority));
        }

        public static IDisposable? Subscribe<T>(Action<T, int> action, T param, int timeOut = 0,
            ThreadPriority priority = ThreadPriority.Normal) {
            return GenericThreadPool?.Subscribe(new TimerRule<T>(action, timeOut, priority), param);
        }

        public static IDisposable Subscribe<T>(TimerRule<T> rule, T param, StandalonePool pool) {
            return pool.Subscribe(rule, param);
        }

        public static IDisposable Subscribe<T>(TimerRule<T> rule, T param, StaticPool pool) {
            return pool.Subscribe(rule, param);
        }

        public static IDisposable? Subscribe<T>(TimerRule<T> rule, T param) {
            return GenericThreadPool?.Subscribe(rule, param);
        }

        #endregion

        internal static void SendServerMessage(string p) {
            Kernel.SendWorldMessage(new Message(p, Color.Red, Message.TopLeft), Program.Values);
        }
    }
}