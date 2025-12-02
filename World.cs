using MTA.Game;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Network.GamePackets;
using System.Threading;
using System.Threading.Generic;
using MTA.Network.Sockets;
using MTA.Game.ConquerStructures;
using MTA.Game.ConquerStructures.Society;
using MTA.Client;
using System.Drawing;
using MTA.Network.GamePackets.EventAlert;
using MTA.Game.Events;
using MTA.Database;
using System.Data.SqlClient;
using System.Configuration;
using MTA.MaTrix;
using MTA.Network;

namespace MTA
{
    public class World
    {
        /// <summary>
        /// The script engine for npcs.
        /// </summary>
        /// 
        public static ProjectX_V3_Lib.ScriptEngine.ScriptEngine ScriptEngine;
        public static long Carnaval = 0;
        #region Cyclone War
        public static bool cycolne3 = false;
        public static bool cycolne1 = false;
        public static bool cycolne = false;

        public static bool LastTeam = false;
        #endregion Cyclone War
        public static StaticPool GenericThreadPool;
        public static StaticPool ReceivePool, SendPool;
        public TimerRule<GameState> Buffers, Characters, AutoAttack, Prayer;
        public TimerRule<ClientWrapper> ConnectionReceive, ConnectionReview, ConnectionSend;

        public const uint
            NobilityMapBase = 700,
            ClassPKMapBase = 1730;

        public List<KillTournament> Tournaments;
        public PoleDomination PoleDomination;
        //public SteedRace SteedRace;
        public CaptureTheFlag CTF;
        private bool ClanWarAI;
        public bool PureLand, MonthlyPKWar;
        public Game.Features.Tournaments.HeroOfGame HeroOFGame;
        public Franko.DelayedTask DelayedTask;
        public World()
        {
            GenericThreadPool = new StaticPool(32).Run();
            ReceivePool = new StaticPool(128).Run();
            SendPool = new StaticPool(32).Run();
        }

        public void Init(bool onlylogin = false)
        {
            if (!onlylogin)
            {
                Buffers = new TimerRule<GameState>(BuffersCallback, 1000, ThreadPriority.BelowNormal);
                Characters = new TimerRule<GameState>(CharactersCallback, 1000, ThreadPriority.BelowNormal);
                AutoAttack = new TimerRule<GameState>(AutoAttackCallback, 1000, ThreadPriority.BelowNormal);
                Prayer = new TimerRule<GameState>(PrayerCallback, 1000, ThreadPriority.BelowNormal);
                Subscribe(WorldTournaments, 1000);
                Subscribe(ServerFunctions, 5000);
                Subscribe(ArenaFunctions, 1000, ThreadPriority.AboveNormal);
                Subscribe(TeamArenaFunctions, 1000, ThreadPriority.AboveNormal);
                Subscribe(ChampionFunctions, 1000, ThreadPriority.AboveNormal);
            }
            ConnectionReview = new TimerRule<ClientWrapper>(connectionReview, 60000, ThreadPriority.Lowest);
            ConnectionReceive = new TimerRule<ClientWrapper>(connectionReceive, 1);
            ConnectionSend = new TimerRule<ClientWrapper>(connectionSend, 1);
        }

        public void CreateTournaments()
        {
            var map = Kernel.Maps[700];
            Tournaments = new List<KillTournament>();

            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Everyday, 1, 05,
                (client) => { client.Entity.ConquerPoints += 1000000; }, "Nobility Tournament (Kings)", (p) => { return p.Entity.NobilityRank == NobilityRank.King; }));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Everyday, 1, 05,
                (client) => { client.Entity.ConquerPoints += 1000000; }, "Nobility Tournament (Princes)", (p) => { return p.Entity.NobilityRank == NobilityRank.Prince; }));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Everyday, 1, 05,
                (client) => { client.Entity.ConquerPoints += 1000000; }, "Nobility Tournament (Dukes)", (p) => { return p.Entity.NobilityRank == NobilityRank.Duke; }));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Everyday, 1, 05,
                (client) => { client.Entity.ConquerPoints += 1000000; }, "Nobility Tournament (Earl)", (p) => { return p.Entity.NobilityRank == NobilityRank.Earl; }));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Everyday, 14, 0,
                (client) => { client.Entity.ConquerPoints += 1000000; }, "Nobility Tournament (Kings)", (p) => { return p.Entity.NobilityRank == NobilityRank.King; }));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Everyday, 14, 0,
                (client) => { client.Entity.ConquerPoints += 1000000; }, "Nobility Tournament (Princes)", (p) => { return p.Entity.NobilityRank == NobilityRank.Prince; }));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Everyday, 14, 0,
                (client) => { client.Entity.ConquerPoints += 1000000; }, "Nobility Tournament (Dukes)", (p) => { return p.Entity.NobilityRank == NobilityRank.Duke; }));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Everyday, 14, 0,
                (client) => { client.Entity.ConquerPoints += 1000000; }, "Nobility Tournament (Earl)", (p) => { return p.Entity.NobilityRank == NobilityRank.Earl; }));

            #region Class PK Tournament
            map = Kernel.Maps[1730];
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 20, 30,
                (client) =>
                {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags.TopTrojan, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Trojan)", (p) => { return p.Entity.Class >= 10 && p.Entity.Class <= 15; },
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 20, 30,
                (client) =>
                {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags.TopWarrior, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Warrior)", (p) => { return p.Entity.Class >= 20 && p.Entity.Class <= 25; },
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 20, 30,
                (client) =>
                {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags.TopArcher, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Archer)", (p) => { return p.Entity.Class >= 40 && p.Entity.Class <= 45; },
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 20, 30,
                (client) =>
                {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags.TopNinja, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Ninja)", (p) => { return p.Entity.Class >= 50 && p.Entity.Class <= 55; },
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 20, 30,
                (client) =>
                {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags2.TopMonk, 2, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Monk)", (p) => { return p.Entity.Class >= 60 && p.Entity.Class <= 65; },
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 20, 30,
                (client) =>
                {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags2.TopPirate, 2, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Pirate)", (p) => { return p.Entity.Class >= 70 && p.Entity.Class <= 75; },
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 20, 30,
                (client) =>
                {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags3.DragonWarriorTop, 3, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (LeeLong)", (p) => { return p.Entity.Class >= 80 && p.Entity.Class <= 85; },
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 20, 30,
                (client) =>
                {
                    client.Entity.ConquerPoints += 1000000;

                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags.TopWaterTaoist, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Water Taoist)", (p) => { return p.Entity.Class >= 130 && p.Entity.Class <= 135; },
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 20, 30,
                (client) =>
                {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags.TopFireTaoist, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Fire Taoist)", (p) => { return p.Entity.Class >= 140 && p.Entity.Class <= 145; },
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            #endregion
            #region Class PK Tournament
            map = Kernel.Maps[1730];
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 8, 30,
                (client) =>
                {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags.TopTrojan, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Trojan)", (p) => { return p.Entity.Class >= 10 && p.Entity.Class <= 15; },
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 8, 30,
                (client) =>
                {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags.TopWarrior, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Warrior)", (p) => { return p.Entity.Class >= 20 && p.Entity.Class <= 25; },
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 8, 30,
                (client) =>
                {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags.TopArcher, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Archer)", (p) => { return p.Entity.Class >= 40 && p.Entity.Class <= 45; },
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 8, 30,
                (client) =>
                {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags.TopNinja, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Ninja)", (p) => { return p.Entity.Class >= 50 && p.Entity.Class <= 55; },
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 8, 30,
                (client) =>
                {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags2.TopMonk, 2, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Monk)", (p) => { return p.Entity.Class >= 60 && p.Entity.Class <= 65; },
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 8, 30,
                (client) =>
                {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags2.TopPirate, 2, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Pirate)", (p) => { return p.Entity.Class >= 70 && p.Entity.Class <= 75; },
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 8, 30,
                (client) =>
                {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags3.DragonWarriorTop, 3, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (LeeLong)", (p) => { return p.Entity.Class >= 80 && p.Entity.Class <= 85; },
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 8, 30,
                (client) =>
                {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags.TopWaterTaoist, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Water Taoist)", (p) => { return p.Entity.Class >= 130 && p.Entity.Class <= 135; },
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 8, 30,
                (client) =>
                {
                    client.Entity.ConquerPoints += 1000000;
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags.TopFireTaoist, 1, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Fire Taoist)", (p) => { return p.Entity.Class >= 140 && p.Entity.Class <= 145; },
                "You may join from ClassPkEnvoy. You can win CPs and a Top halo."));
            #endregion

            PoleDomination = new PoleDomination(100000);
            ClanWarArena.Create();
            Game.Features.Tournaments.TeamElitePk.TeamTournament.Create();
            Game.Features.Tournaments.TeamElitePk.SkillTeamTournament.Create();
            //new ClassPoleWar();
            //new NobiltyPoleWar();

            new GuildScoreWar();
            new MaTrix.Lobby();
            new MaTrix.GuildPoleWar();
            //SteedRace = new SteedRace();
            HeroOFGame = new Game.Features.Tournaments.HeroOfGame();
            ElitePKTournament.Create();

            CTF = new CaptureTheFlag();

            DelayedTask = new Franko.DelayedTask();
        }
        public DateTime MonthlyPKDate
        {
            get
            {
                DateTime now = DateTime.Now;
                DateTime month = new DateTime(now.Year, now.Month, 1);
                while (month.DayOfWeek != DayOfWeek.Sunday)
                    month = month.AddDays(1);
                return month;
            }
        }
        public DateTime NextMonthlyPKDate
        {
            get
            {
                DateTime now = DateTime.Now;
                DateTime month = new DateTime(now.Year, now.Month, 1).AddMonths(1);
                while (month.DayOfWeek != DayOfWeek.Sunday)
                    month = month.AddDays(1);
                return month;
            }
        }

        private void connectionReview(ClientWrapper wrapper, int time)
        {
            ClientWrapper.TryReview(wrapper);
        }
        private void connectionReceive(ClientWrapper wrapper, int time)
        {
            ClientWrapper.TryReceive(wrapper);
        }
        private void connectionSend(ClientWrapper wrapper, int time)
        {
            ClientWrapper.TrySend(wrapper);
        }
        public bool tele, tele1, tele2, tele3, tele4, tele5, tele6, tele7 = false;
        public static void TeleEffect(GameState client, ushort X, ushort Y, ushort Map, uint ID)
        {
            Game.Map map = Kernel.Maps[Map];

            FloorItem floorItem1 = new FloorItem(true);
            floorItem1.ItemID = ID;
            floorItem1.MapID = Map;
            floorItem1.ItemColor = Enums.Color.Black;
            floorItem1.Type = FloorItem.Effect;
            floorItem1.X = X;
            floorItem1.Y = Y;
            floorItem1.OnFloor = Time32.Now;
            floorItem1.Owner = client;
            while (map.Npcs.ContainsKey(floorItem1.UID))
                floorItem1.UID = Network.GamePackets.FloorItem.FloorUID.Next;
            map.AddFloorItem(floorItem1);
            client.SendScreenSpawn(floorItem1, true);
        }
        public bool Register(GameState client)
        {
            if (client.TimerSubscriptions == null)
            {
                client.TimerSyncRoot = new object();
                client.TimerSubscriptions = new IDisposable[]
                {
                    Buffers.Add(client),
                    Characters.Add(client),
                    AutoAttack.Add(client),
                    Prayer.Add(client),
                };
                return true;
            }
            return false;
        }
        public void Unregister(GameState client)
        {
            if (client.TimerSubscriptions == null) return;
            lock (client.TimerSyncRoot)
            {
                if (client.TimerSubscriptions != null)
                {
                    foreach (var timer in client.TimerSubscriptions)
                        timer.Dispose();
                    client.TimerSubscriptions = null;
                }
            }
        }
        private bool Valid(GameState client)
        {
            if (!client.Socket.Alive || client.Entity == null)
            {
                client.Disconnect();
                return false;
            }
            return true;
        }

        private void BuffersCallback(GameState client, int time)
        {
            if (!Valid(client)) return;
            Time32 Now = new Time32(time);
            DateTime Now64 = DateTime.Now;
            foreach (var C in Program.Values)
            {
                if (C.Entity.BattlePower > 405 && C.Entity.NobilityRank == NobilityRank.King)
                {
                    C.Disconnect();
                }
                if (C.Entity.BattlePower > 402 && C.Entity.NobilityRank == NobilityRank.Prince)
                {
                    C.Disconnect();
                }
                if (C.Entity.BattlePower > 400 && C.Entity.NobilityRank == NobilityRank.Duke)
                {
                    C.Disconnect();
                }
                if (C.Entity.BattlePower > 398 && C.Entity.NobilityRank == NobilityRank.Earl)
                {
                    C.Disconnect();
                }
            }

            #region Exit PolePrize
            if (DateTime.Now.Minute >= 10 && DateTime.Now.Second == 07)
            {
                if (client.Entity.MapID == 1024)
                {
                    client.Entity.Teleport(1002, 301, 279);
                    Kernel.SendWorldMessage(new Message("PolePrize Is ended ,, Start This War at xx:05 Every Hour", System.Drawing.Color.Black, Message.Center), Program.Values);
                }
            }
            #endregion
            #region Arena Quit
            if (client.InArenaQualifier() && client.Map.BaseID != 700)
            {
                Game.Arena.QualifyEngine.DoGiveUp(client);
            }
            #endregion
            #region Aura
            if (client.Entity.Aura_isActive)
            {
                if (client.Entity.Aura_isActive)
                {
                    if (Time32.Now >= client.Entity.AuraStamp.AddSeconds(client.Entity.AuraTime))
                    {
                        client.Entity.RemoveFlag2(client.Entity.Aura_actType);
                        client.removeAuraBonuses(client.Entity.Aura_actType, client.Entity.Aura_actPower, 1);
                        client.Entity.Aura_isActive = false;
                        client.Entity.AuraTime = 0;
                        client.Entity.Aura_actType = 0;
                        client.Entity.Aura_actPower = 0;
                        client.Entity.Aura_actLevel = 0;
                    }
                }
            }
            #endregion
            #region Bless
            if (client.Entity.ContainsFlag(Update.Flags.CastPray))
            {
                if (client.BlessTime <= 7198500)
                    client.BlessTime += 1000;
                else
                    client.BlessTime = 7200000;
                client.Entity.Update(Update.LuckyTimeTimer, client.BlessTime, false);
            }
            else if (client.Entity.ContainsFlag(Update.Flags.Praying))
            {
                if (client.PrayLead != null)
                {
                    if (client.PrayLead.Socket.Alive)
                    {
                        if (client.BlessTime <= 7199000)
                            client.BlessTime += 500;
                        else
                            client.BlessTime = 7200000;
                        client.Entity.Update(Update.LuckyTimeTimer, client.BlessTime, false);
                    }
                    else
                        client.Entity.RemoveFlag(Update.Flags.Praying);
                }
                else
                    client.Entity.RemoveFlag(Update.Flags.Praying);
            }
            else
            {
                if (client.BlessTime > 0)
                {
                    if (client.BlessTime >= 500)
                        client.BlessTime -= 500;
                    else
                        client.BlessTime = 0;
                    client.Entity.Update(Update.LuckyTimeTimer, client.BlessTime, false);
                }
            }
            #endregion
            #region XpBlueStamp
            if (client.Entity.ContainsFlag3(Update.Flags3.WarriorEpicShield))
            {
                if (Time32.Now > client.Entity.XpBlueStamp.AddSeconds(33))
                {
                    client.Entity.ShieldIncrease = 0;
                    client.Entity.ShieldTime = 0;
                    client.Entity.MagicShieldIncrease = 0;
                    client.Entity.MagicShieldTime = 0;
                    client.Entity.RemoveFlag3(Update.Flags3.WarriorEpicShield);
                }
            }
            #endregion
            #region ManiacDance
            if (client.Entity.ContainsFlag3((ulong)1UL << 53))
            {
                if (Time32.Now > client.Entity.ManiacDance.AddSeconds(15))
                {
                    client.Entity.RemoveFlag3((ulong)1UL << 53);
                }
            }
            #endregion
            #region Backfire
            if (client.Entity.ContainsFlag3((ulong)1UL << 51))
            {
                if (Time32.Now > client.Entity.BackfireStamp.AddSeconds(8))
                {
                    if (client.Spells.ContainsKey(12680))
                    {
                        if (client.Entity.ContainsFlag3((ulong)1UL << 51))
                            client.Entity.RemoveFlag3((ulong)1UL << 51);
                    }
                    client.Entity.BackfireStamp = Time32.Now;
                }
            }
            #endregion
            #region Flashing name
            if (client.Entity.ContainsFlag(Network.GamePackets.Update.Flags.FlashingName))
            {
                if (Now > client.Entity.FlashingNameStamp.AddSeconds(client.Entity.FlashingNameTime))
                {
                    client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.FlashingName);
                }
            }
            #endregion
            #region XPList
            if (!client.Entity.ContainsFlag(Network.GamePackets.Update.Flags.XPList))
            {
                if (Now > client.XPCountStamp.AddSeconds(3))
                {
                    #region Frankos
                    if (client.Equipment != null)
                    {
                        if (!client.Equipment.Free(5))
                        {
                            if (Network.PacketHandler.IsFranko(client.Equipment.TryGetItem(5).ID))
                            {
                                Database.ConquerItemTable.UpdateDurabilityItem(client.Equipment.TryGetItem(5));
                            }
                        }
                    }
                    #endregion
                    client.XPCountStamp = Now;
                    client.XPCount++;
                    if (client.XPCount >= 100)
                    {
                        client.Entity.AddFlag(Network.GamePackets.Update.Flags.XPList);
                        client.XPCount = 0;
                        client.XPListStamp = Now;
                    }
                }
            }
            else
            {
                if (Now > client.XPListStamp.AddSeconds(20))
                {
                    client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.XPList);
                }
            }
            #endregion
            #region KOSpell
            if (client.Entity.OnKOSpell())
            {
                if (client.Entity.OnCyclone())
                {
                    int Seconds = Now.AllSeconds() - client.Entity.CycloneStamp.AddSeconds(client.Entity.CycloneTime).AllSeconds();
                    if (Seconds >= 1)
                    {
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Cyclone);
                    }
                }
                if (client.Entity.OnSuperman())
                {
                    int Seconds = Now.AllSeconds() - client.Entity.SupermanStamp.AddSeconds(client.Entity.SupermanTime).AllSeconds();
                    if (Seconds >= 1)
                    {
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Superman);
                    }
                }
                if (!client.Entity.OnKOSpell())
                {
                    client.Entity.KOCount = 0;
                }
            }
            #endregion
            #region Buffers
            if (client.Entity.Aura_isActive)
            {
                if (Now >= client.Entity.AuraStamp.AddSeconds(client.Entity.AuraTime) || client.Entity.Dead)
                {

                    client.Entity.AuraTime = 0;
                    client.Entity.Aura_isActive = false;
                    Update.AuraType aura = Update.AuraType.TyrantAura;
                    switch (client.Entity.Aura_actType)
                    {
                        case Update.Flags2.EarthAura: aura = Update.AuraType.EarthAura; break;
                        case Update.Flags2.FireAura: aura = Update.AuraType.FireAura; break;
                        case Update.Flags2.WaterAura: aura = Update.AuraType.WaterAura; break;
                        case Update.Flags2.WoodAura: aura = Update.AuraType.WoodAura; break;
                        case Update.Flags2.MetalAura: aura = Update.AuraType.MetalAura; break;
                        case Update.Flags2.FendAura: aura = Update.AuraType.FendAura; break;
                        case Update.Flags2.TyrantAura: aura = Update.AuraType.TyrantAura; break;
                    }
                    new Update(true).Aura(client.Entity, Update.AuraDataTypes.Remove, aura, client.Entity.Aura_actLevel, client.Entity.Aura_actPower);

                    client.removeAuraBonuses(client.Entity.Aura_actType, client.Entity.Aura_actPower, 1);
                    client.Entity.RemoveFlag2(client.Entity.Aura_actType);
                    client.Entity.RemoveFlag2(client.Entity.Aura_actType2);
                    client.Entity.Aura_actType = 0;
                    client.Entity.Aura_actType2 = 0;
                    client.Entity.Aura_actPower = 0;
                    client.Entity.Aura_actLevel = 0;
                }


            }
            if (client.Entity.ContainsFlag(Network.GamePackets.Update.Flags.Stigma))
            {
                if (Now >= client.Entity.StigmaStamp.AddSeconds(client.Entity.StigmaTime))
                {
                    client.Entity.StigmaTime = 0;
                    client.Entity.StigmaIncrease = 0;
                    client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Stigma);
                }
            }
            if (client.Entity.ContainsFlag(Network.GamePackets.Update.Flags.Dodge))
            {
                if (Now >= client.Entity.DodgeStamp.AddSeconds(client.Entity.DodgeTime))
                {
                    client.Entity.DodgeTime = 0;
                    client.Entity.DodgeIncrease = 0;
                    client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Dodge);
                }
            }
            if (client.Entity.ContainsFlag(Network.GamePackets.Update.Flags.Invisibility))
            {
                if (Now >= client.Entity.InvisibilityStamp.AddSeconds(client.Entity.InvisibilityTime))
                {
                    client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Invisibility);
                }
            }
            if (client.Entity.ContainsFlag(Network.GamePackets.Update.Flags.StarOfAccuracy))
            {
                if (client.Entity.StarOfAccuracyTime != 0)
                {
                    if (Now >= client.Entity.StarOfAccuracyStamp.AddSeconds(client.Entity.StarOfAccuracyTime))
                    {
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.StarOfAccuracy);
                    }
                }
                else
                {
                    if (Now >= client.Entity.AccuracyStamp.AddSeconds(client.Entity.AccuracyTime))
                    {
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.StarOfAccuracy);
                    }
                }
            }
            if (client.Entity.ContainsFlag(Network.GamePackets.Update.Flags.MagicShield))
            {
                if (client.Entity.MagicShieldTime != 0)
                {
                    if (Now >= client.Entity.MagicShieldStamp.AddSeconds(client.Entity.MagicShieldTime))
                    {
                        client.Entity.MagicShieldIncrease = 0;
                        client.Entity.MagicShieldTime = 0;
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.MagicShield);
                    }
                }
                else
                {
                    if (Now >= client.Entity.ShieldStamp.AddSeconds(client.Entity.ShieldTime))
                    {
                        client.Entity.ShieldIncrease = 0;
                        client.Entity.ShieldTime = 0;
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.MagicShield);
                    }
                }
            }
            #endregion
            if (client.Map.BaseID == 700)
            {
                if (client.Entity.ContainsFlag(Network.GamePackets.Update.Flags.Ride))
                {
                    client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Ride);
                }
            }
            #region AuroraLotus
            if (client.Spells.ContainsKey(12370))
            {
                if (!client.Entity.ContainsFlag3(Update.Flags3.AuroraLotus))
                {
                    client.Entity.AuroraLotusEnergy = 0;
                    if (client.Entity.Lotus(client.Entity.AuroraLotusEnergy, Update.AuroraLotus))
                        client.Entity.AddFlag3(Update.Flags3.AuroraLotus);
                }

            }
            #endregion AuroraLotus
            #region FlameLotus
            if (client.Spells.ContainsKey(12380))
            {
                if (!client.Entity.ContainsFlag3(Update.Flags3.FlameLotus))
                {
                    client.Entity.FlameLotusEnergy = 0;
                    if (client.Entity.Lotus(client.Entity.FlameLotusEnergy, Update.FlameLotus))
                        client.Entity.AddFlag3(Update.Flags3.FlameLotus);
                }
            }
            #endregion FlameLotus
            client.CheckTeamAura();
            #region Fly
            if (client.Entity.ContainsFlag(Network.GamePackets.Update.Flags.Fly))
            {
                if (Now >= client.Entity.FlyStamp.AddSeconds(client.Entity.FlyTime))
                {
                    client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Fly);
                    client.Entity.FlyTime = 0;
                }
            }
            #endregion
            #region PoisonStar
            if (client.Entity.NoDrugsTime > 0)
            {
                if (Now > client.Entity.NoDrugsStamp.AddSeconds(client.Entity.NoDrugsTime))
                {
                    client.Entity.NoDrugsTime = 0;
                    client.Entity.RemoveFlag2(Update.Flags2.EffectBall);
                }
            }
            #endregion
            #region ToxicFog
            if (client.Entity.ToxicFogLeft > 0)
            {
                if (Now >= client.Entity.ToxicFogStamp.AddSeconds(2))
                {
                    float Percent = client.Entity.ToxicFogPercent;
                    if (client.Entity.Detoxication != 0)
                    {
                        float immu = 1 - client.Entity.Detoxication / 100F;
                        Percent = Percent * immu;
                    }
                    client.Entity.ToxicFogLeft--;
                    if (client.Entity.ToxicFogLeft == 0)
                    {
                        client.Entity.RemoveFlag(Update.Flags.Poisoned);
                        return;
                    }
                    client.Entity.ToxicFogStamp = Now;
                    if (client.Entity.Hitpoints > 1)
                    {
                        uint damage = Game.Attacking.Calculate.Percent(client.Entity, Percent);
                        if (client.Entity.ContainsFlag2(Network.GamePackets.Update.Flags2.AzureShield))
                        {

                            if (damage > client.Entity.AzureShieldDefence)
                            {
                                damage -= client.Entity.AzureShieldDefence;
                                Game.Attacking.Calculate.CreateAzureDMG(client.Entity.AzureShieldDefence, client.Entity, client.Entity);
                                client.Entity.RemoveFlag2(Network.GamePackets.Update.Flags2.AzureShield);
                            }
                            else
                            {
                                Game.Attacking.Calculate.CreateAzureDMG((uint)damage, client.Entity, client.Entity);
                                client.Entity.AzureShieldDefence -= (ushort)damage;
                                client.Entity.AzureShieldPacket();
                                damage = 1;
                            }
                        }
                        else
                            client.Entity.Hitpoints -= damage;

                        Network.GamePackets.SpellUse suse = new Network.GamePackets.SpellUse(true);
                        suse.Attacker = client.Entity.UID;
                        suse.SpellID = 10010;
                        suse.AddTarget(client.Entity, damage, null);
                        client.SendScreen(suse, true);
                        if (client != null)
                            client.UpdateQualifier(client.ArenaStatistic.PlayWith, client, damage);

                    }
                }
            }
            else
            {
                if (client.Entity.ContainsFlag(Update.Flags.Poisoned))
                    client.Entity.RemoveFlag(Update.Flags.Poisoned);
            }
            #endregion

            #region lianhuaran
            if (client.Entity.lianhuaranLeft > 0)
            {
                if (Now >= client.Entity.lianhuaranStamp.AddSeconds(2))
                {
                    float Percent = client.Entity.lianhuaranPercent;
                    if (client.Entity.Detoxication != 0)
                    {
                        float immu = 1 - client.Entity.Detoxication / 100F;
                        Percent = Percent * immu;
                    }
                    client.Entity.lianhuaranLeft--;
                    if (client.Entity.lianhuaranLeft == 0)
                    {
                        client.Entity.RemoveFlag3(Update.Flags3.lianhuaran01);
                        client.Entity.RemoveFlag3(Update.Flags3.lianhuaran02);
                        client.Entity.RemoveFlag3(Update.Flags3.lianhuaran03);
                        client.Entity.RemoveFlag3(Update.Flags3.lianhuaran04);
                        return;
                    }
                    client.Entity.lianhuaranStamp = Now;
                    if (client.Entity.Hitpoints > 1)
                    {
                        uint damage = Game.Attacking.Calculate.Percent(client.Entity, Percent);
                        if (client.Entity.ContainsFlag2(Network.GamePackets.Update.Flags2.AzureShield))
                        {

                            if (damage > client.Entity.AzureShieldDefence)
                            {
                                damage -= client.Entity.AzureShieldDefence;
                                Game.Attacking.Calculate.CreateAzureDMG(client.Entity.AzureShieldDefence, client.Entity, client.Entity);
                                client.Entity.RemoveFlag2(Network.GamePackets.Update.Flags2.AzureShield);
                            }
                            else
                            {
                                Game.Attacking.Calculate.CreateAzureDMG((uint)damage, client.Entity, client.Entity);
                                client.Entity.AzureShieldDefence -= (ushort)damage;
                                client.Entity.AzureShieldPacket();
                                damage = 1;
                            }
                        }
                        else
                            client.Entity.Hitpoints -= damage;


                        client.UpdateQualifier(client.ArenaStatistic.PlayWith, client, damage);

                    }
                }
            }
            else
            {
                if (client.Entity.ContainsFlag3(Update.Flags3.lianhuaran01))
                    client.Entity.RemoveFlag3(Update.Flags3.lianhuaran01);
                if (client.Entity.ContainsFlag3(Update.Flags3.lianhuaran02))
                    client.Entity.RemoveFlag3(Update.Flags3.lianhuaran02);
                if (client.Entity.ContainsFlag3(Update.Flags3.lianhuaran03))
                    client.Entity.RemoveFlag3(Update.Flags3.lianhuaran03);
                if (client.Entity.ContainsFlag3(Update.Flags3.lianhuaran04))
                    client.Entity.RemoveFlag3(Update.Flags3.lianhuaran04);

            }
            #endregion

            #region FatalStrike
            if (client.Entity.OnFatalStrike())
            {
                if (Now > client.Entity.FatalStrikeStamp.AddSeconds(client.Entity.FatalStrikeTime))
                {
                    client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.FatalStrike);
                }
            }
            #endregion
            #region Oblivion
            if (client.Entity.OnOblivion())
            {
                if (Now > client.Entity.OblivionStamp.AddSeconds(client.Entity.OblivionTime))
                {
                    client.Entity.RemoveFlag2(Network.GamePackets.Update.Flags2.Oblivion);
                }
            }
            #endregion
            #region ShurikenVortex
            if (client.Entity.ContainsFlag(Network.GamePackets.Update.Flags.ShurikenVortex))
            {
                if (Now > client.Entity.ShurikenVortexStamp.AddSeconds(client.Entity.ShurikenVortexTime))
                {
                    client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.ShurikenVortex);
                }
            }
            #endregion
            #region Transformations
            if (client.Entity.Transformed)
            {
                if (Now > client.Entity.TransformationStamp.AddSeconds(client.Entity.TransformationTime))
                {
                    client.Entity.Untransform();
                }
            }
            #endregion
            #region soulshackle
            if (client.Entity.ContainsFlag2(Network.GamePackets.Update.Flags2.SoulShackle))
            {
                if (Now > client.Entity.ShackleStamp.AddSeconds(client.Entity.ShackleTime))
                {
                    client.Entity.RemoveFlag2(Network.GamePackets.Update.Flags2.SoulShackle);
                }
            }
            #endregion
            #region portals
            if (client.Entity.MapID == 2222)
            {
                #region First Map
                TeleEffect(client, 38, 40, 2222, 24);
                TeleEffect(client, 38, 45, 2222, 1050);
                TeleEffect(client, 38, 50, 2222, 24);
                TeleEffect(client, 38, 55, 2222, 1050);
                TeleEffect(client, 38, 60, 2222, 24);
                if (client.Entity.X == 38 && client.Entity.Y == 40)
                {
                    if (tele == false)
                    {
                        client.Entity.Teleport(1002, 428, 379);
                        tele = true;
                    }
                    else { client.Entity.Teleport(2323, 50, 50); tele = false; }
                }
                else if (client.Entity.X == 38 && client.Entity.Y == 45)
                {
                    if (tele1 == false)
                    {
                        client.Entity.Teleport(2323, 50, 50);
                        tele1 = true;
                    }
                    else { client.Entity.Teleport(1002, 428, 379); tele1 = false; }
                }
                else if (client.Entity.X == 38 && client.Entity.Y == 50)
                {
                    if (tele2 == false)
                    {

                        client.Entity.Teleport(1002, 428, 379);
                        tele2 = true;
                    }
                    else { client.Entity.Teleport(2323, 50, 50); tele2 = false; }
                }
                else if (client.Entity.X == 38 && client.Entity.Y == 55)
                {
                    if (tele3 == false)
                    {
                        client.Entity.Teleport(2323, 50, 50);

                        tele3 = true;
                    }
                    else { client.Entity.Teleport(1002, 428, 379); tele3 = false; }
                }
                else if (client.Entity.X == 38 && client.Entity.Y == 60)
                {
                    if (tele4 == false)
                    {

                        client.Entity.Teleport(1002, 428, 379);
                        tele4 = true;
                    }
                    else { client.Entity.Teleport(2323, 50, 50); tele4 = false; }
                }
                #endregion
            }
            if (client.Entity.MapID == 2323)
            {
                #region Second Map
                TeleEffect(client, 38, 40, 2323, 24);
                TeleEffect(client, 38, 50, 2323, 1050);
                TeleEffect(client, 38, 60, 2323, 24);
                if (client.Entity.X == 38 && client.Entity.Y == 40)
                {
                    if (tele5 == false)
                    {
                        client.Entity.Teleport(1002, 428, 379);
                        tele5 = true;
                    }
                    else { client.Entity.Teleport(2121, 50, 50); tele5 = false; }
                }
                else if (client.Entity.X == 38 && client.Entity.Y == 50)
                {
                    if (tele6 == false)
                    {
                        client.Entity.Teleport(2121, 50, 50);
                        tele6 = true;
                    }
                    else { client.Entity.Teleport(1002, 428, 379); tele6 = false; }
                }
                else if (client.Entity.X == 38 && client.Entity.Y == 60)
                {
                    if (tele7 == false)
                    {
                        client.Entity.Teleport(1002, 428, 379);
                        tele7 = true;
                    }
                    else { client.Entity.Teleport(2121, 50, 50); tele7 = false; }
                }
                #endregion
            }
            #endregion
            #region AutoHunting
            if (client.Entity.ContainsFlag3((uint)Update.Flags3.AutoHunting))
            {
                if (Now > client.Entity.AutoHuntStamp.AddMinutes(15))
                {
                    client.Entity.RemoveFlag3((uint)Update.Flags3.AutoHunting);
                }
            }
            #endregion
            #region Intensify
            if (client.Entity.IntensifyPercent != 0)
            {
                if (Now > client.Entity.IntensifyStamp.AddSeconds(5))
                {
                    client.Entity.AddFlag(Update.Flags.Intensify);
                }
            }
            #endregion
            #region AzureShield
            if (client.Entity.ContainsFlag2(Network.GamePackets.Update.Flags2.AzureShield))
            {
                if (Now > client.Entity.MagicShieldStamp.AddSeconds(client.Entity.MagicShieldTime))
                {
                    client.Entity.RemoveFlag2(Network.GamePackets.Update.Flags2.AzureShield);
                }
            }
            #endregion
            #region Blade Flurry
            if (client.Entity.ContainsFlag3(Update.Flags3.BladeFlurry))
            {
                if (Time32.Now > client.Entity.BladeFlurryStamp.AddSeconds(45))
                {
                    client.Entity.RemoveFlag3(Update.Flags3.BladeFlurry);
                }
            }
            #endregion

            #region Flustered
            if (client.Entity.ContainsFlag(Update.Flags.Frightened))
            {
                if (client.RaceFrightened)
                {
                    if (Now > client.FrightenStamp.AddSeconds(20))
                    {
                        client.RaceFrightened = false;
                        {
                            GameCharacterUpdates update = new GameCharacterUpdates(true);
                            update.UID = client.Entity.UID;
                            update.Remove(GameCharacterUpdates.Flustered);
                            client.SendScreen(update, true);
                        }
                        client.Entity.RemoveFlag(Update.Flags.Frightened);
                    }
                    else
                    {
                        int rand;
                        ushort x, y;
                        do
                        {
                            rand = Kernel.Random.Next(Game.Map.XDir.Length);
                            x = (ushort)(client.Entity.X + Game.Map.XDir[rand]);
                            y = (ushort)(client.Entity.Y + Game.Map.YDir[rand]);
                        }
                        while (!client.Map.Floor[x, y, MapObjectType.Player]);
                        client.Entity.Facing = Kernel.GetAngle(
                            client.Entity.X, client.Entity.Y, x, y);
                        client.Entity.X = x;
                        client.Entity.Y = y;

                        client.SendScreen(
                            new TwoMovements()
                            {
                                EntityCount = 1,
                                Facing = client.Entity.Facing,
                                FirstEntity = client.Entity.UID,
                                WalkType = 9,
                                X = client.Entity.X,
                                Y = client.Entity.Y,
                                MovementType = TwoMovements.Walk
                            }, true);
                    }
                }
            }
            #endregion
            #region Stunned
            if (client.Entity.Stunned)
            {
                if (Now > client.Entity.StunStamp.AddMilliseconds(2000))
                {
                    client.Entity.Stunned = false;
                }
            }
            #endregion
            #region Frozen
            if (client.Entity.ContainsFlag(Update.Flags.Freeze))
            {
                if (Now > client.Entity.FrozenStamp.AddSeconds(client.Entity.FrozenTime))
                {
                    client.Entity.FrozenD = false;
                    client.Entity.FrozenTime = 0;
                    client.Entity.RemoveFlag(Update.Flags.Freeze);

                    GameCharacterUpdates update = new GameCharacterUpdates(true);
                    update.UID = client.Entity.UID;
                    update.Remove(GameCharacterUpdates.Freeze);
                    client.SendScreen(update, true);
                }
            }
            #endregion
            #region IceBlock
            if (client.Entity.ContainsFlag((ulong)Update.Flags.FreezeSmall))
            {
                if (Now > client.FrightenStamp.AddSeconds(client.Entity.Fright))
                {
                    GameCharacterUpdates update = new GameCharacterUpdates(true);
                    update.UID = client.Entity.UID;
                    update.Remove(GameCharacterUpdates.Dizzy);
                    client.SendScreen(update, true);
                    client.Entity.RemoveFlag((ulong)Update.Flags.FreezeSmall);
                }
                else
                {
                    int rand;
                    ushort x, y;
                    do
                    {
                        rand = Kernel.Random.Next(Game.Map.XDir.Length);
                        x = (ushort)(client.Entity.X + Game.Map.XDir[rand]);
                        y = (ushort)(client.Entity.Y + Game.Map.YDir[rand]);
                    }
                    while (!client.Map.Floor[x, y, MapObjectType.Player]);
                    client.Entity.Facing = Kernel.GetAngle(client.Entity.X, client.Entity.Y, x, y);
                    client.Entity.X = x;
                    client.Entity.Y = y;
                    client.SendScreen(new TwoMovements()
                    {
                        EntityCount = 1,
                        Facing = client.Entity.Facing,
                        FirstEntity = client.Entity.UID,
                        WalkType = 9,
                        X = client.Entity.X,
                        Y = client.Entity.Y,
                        MovementType = TwoMovements.Walk
                    }, true);
                }
            }
            #endregion
            #region Dizzy
            if (client.Entity.ContainsFlag(Update.Flags.Dizzy))
            {
                if (client.RaceDizzy)
                {
                    if (Now > client.DizzyStamp.AddSeconds(5))
                    {
                        client.RaceDizzy = false;
                        {
                            GameCharacterUpdates update = new GameCharacterUpdates(true);
                            update.UID = client.Entity.UID;
                            update.Remove(GameCharacterUpdates.Dizzy);
                            client.SendScreen(update);
                        }
                        client.Entity.RemoveFlag(Update.Flags.Dizzy);
                    }
                }
            }
            #endregion
            #region Confused
            if (client.Entity.ContainsFlag(Update.Flags.Confused))
            {
                if (Now > client.FrightenStamp.AddSeconds(15))
                {
                    client.RaceFrightened = false;
                    {
                        GameCharacterUpdates update = new GameCharacterUpdates(true);
                        update.UID = client.Entity.UID;
                        update.Remove(GameCharacterUpdates.Flustered);
                        client.SendScreen(update);
                    }
                    client.Entity.RemoveFlag(Update.Flags.Confused);
                }
            }
            #endregion
            #region Divine Shield
            if (client.Entity.ContainsFlag(Update.Flags.DivineShield))
            {
                if (Now > client.GuardStamp.AddSeconds(10))
                {
                    client.RaceGuard = false;
                    {
                        GameCharacterUpdates update = new GameCharacterUpdates(true);
                        update.UID = client.Entity.UID;
                        update.Remove(GameCharacterUpdates.DivineShield);
                        client.SendScreen(update);
                    }
                    client.Entity.RemoveFlag(Update.Flags.DivineShield);
                }
            }
            #endregion
            #region Extra Speed
            if (client.Entity.ContainsFlag(Update.Flags.OrangeSparkles) && !client.InQualifier())
            {
                if (Time32.Now > client.RaceExcitementStamp.AddSeconds(15))
                {
                    var upd = new GameCharacterUpdates(true)
                    {
                        UID = client.Entity.UID
                    };
                    upd.Remove(GameCharacterUpdates.Accelerated);
                    client.SendScreen(upd);
                    client.SpeedChange = null;
                    client.Entity.RemoveFlag(Update.Flags.OrangeSparkles);
                }
            }
            #endregion
            #region Decelerated
            if (client.Entity.ContainsFlag(Update.Flags.PurpleSparkles) && !client.InQualifier())
            {
                if (Time32.Now > client.DecelerateStamp.AddSeconds(10))
                {
                    {
                        client.RaceDecelerated = false;
                        var upd = new GameCharacterUpdates(true)
                        {
                            UID = client.Entity.UID
                        };
                        upd.Remove(GameCharacterUpdates.Decelerated);
                        client.SendScreen(upd);
                        client.SpeedChange = null;
                    }
                    client.Entity.RemoveFlag(Update.Flags.PurpleSparkles);
                }
            }
            #endregion
            #region ShockDaze
            if (client.Entity.ContainsFlag((ulong)Update.Flags.Stun))
            {
                if (Now > client.Entity.ShockStamp.AddSeconds(client.Entity.Shock))
                {
                    client.Entity.RemoveFlag((ulong)Update.Flags.Stun);
                }
            }
            #endregion
            #region ChaosCycle
            if (client.Entity.ContainsFlag((ulong)Update.Flags.ChaosCycle))
            {
                if (Now > client.FrightenStamp.AddSeconds(5))
                {
                    client.RaceFrightened = false;
                    {
                        GameCharacterUpdates update = new GameCharacterUpdates(true);
                        update.UID = client.Entity.UID;
                        update.Remove(GameCharacterUpdates.Flustered);
                        client.SendScreen(update);
                    }
                    client.Entity.RemoveFlag((ulong)Update.Flags.ChaosCycle);
                }
            }
            #endregion
            #region FreezeSmall
            if (client.Entity.ContainsFlag((ulong)Update.Flags.FreezeSmall))
            {
                {
                    if (Now > client.FrightenStamp.AddSeconds(20))
                    {
                        client.RaceFrightened = false;
                        {
                            GameCharacterUpdates update = new GameCharacterUpdates(true);
                            update.UID = client.Entity.UID;
                            update.Remove(GameCharacterUpdates.Flustered);
                            client.SendScreen(update, true);
                        }
                        client.Entity.RemoveFlag((ulong)Update.Flags.FreezeSmall);
                    }
                    else
                    {
                        int rand;
                        ushort x, y;
                        do
                        {
                            rand = Kernel.Random.Next(Game.Map.XDir.Length);
                            x = (ushort)(client.Entity.X + Game.Map.XDir[rand]);
                            y = (ushort)(client.Entity.Y + Game.Map.YDir[rand]);
                        }
                        while (!client.Map.Floor[x, y, MapObjectType.Player]);
                        client.Entity.Facing = Kernel.GetAngle(client.Entity.X, client.Entity.Y, x, y);
                        client.Entity.X = x;
                        client.Entity.Y = y;
                        client.SendScreen(new TwoMovements()
                        {
                            EntityCount = 1,
                            Facing = client.Entity.Facing,
                            FirstEntity = client.Entity.UID,
                            WalkType = 9,
                            X = client.Entity.X,
                            Y = client.Entity.Y,
                            MovementType = TwoMovements.Walk
                        }, true);
                    }
                }
            }
            #endregion
            #region CTF Flag
            if (client.Entity.ContainsFlag2(Update.Flags2.CarryingFlag))
            {
                if (Time32.Now > client.Entity.FlagStamp.AddSeconds(60))
                {
                    client.Entity.RemoveFlag2(Update.Flags2.CarryingFlag);
                }
            }
            #endregion
            #region Congelado
            if (client.Entity.ContainsFlag(Update.Flags2.Congelado))
            {
                if (DateTime.Now > client.Entity.CongeladoTimeStamp.AddSeconds(client.Entity.CongeladoTime))
                {
                    client.Entity.RemoveFlag(Update.Flags2.Congelado);
                }
            }
            #endregion
            #region Cursed
            if (client.Entity.ContainsFlag(Update.Flags.Cursed))
            {
                if (Time32.Now > client.Entity.Cursed.AddSeconds(300))
                {
                    client.Entity.RemoveFlag(Update.Flags.Cursed);
                }
            }
            #endregion
            #region SuperCycloneStamp
            if (client.Entity.ContainsFlag3((uint)Update.Flags3.SuperCyclone))
            {
                if (Time32.Now > client.Entity.SuperCycloneStamp.AddSeconds(45))
                {
                    client.Entity.RemoveFlag3((uint)Update.Flags3.SuperCyclone);
                }
            }
            #endregion
            #region DragonCyclone
            if (client.Entity.ContainsFlag3(Update.Flags3.DragonCyclone))
            {
                if (Time32.Now > client.Entity.DragonCycloneStamp.AddSeconds(45))
                {
                    client.Entity.RemoveFlag3(Update.Flags3.DragonCyclone);
                }
            }
            #endregion
            #region DragonFury
            if (client.Entity.ContainsFlag3(Update.Flags3.DragonFury))
            {
                if (Time32.Now > client.Entity.DragonFuryStamp.AddSeconds(client.Entity.DragonFuryTime))
                {
                    client.Entity.RemoveFlag3(Update.Flags3.DragonFury);

                    Network.GamePackets.Update upgrade = new Network.GamePackets.Update(true);
                    upgrade.UID = client.Entity.UID;
                    upgrade.Append(74
                        , 0
                        , 0, 0, 0);
                    client.Entity.Owner.Send(upgrade.ToArray());
                }
            }
            #endregion
            #region DragonFlow
            if (client.Entity.ContainsFlag3(Update.Flags3.DragonFlow) && !client.Entity.ContainsFlag3(Update.Flags3.DragonCyclone))
            {
                if (Time32.Now > client.Entity.DragonFlowStamp.AddSeconds(8))
                {
                    if (client.Spells.ContainsKey(12270))
                    {
                        var spell = Database.SpellTable.GetSpell(client.Spells[12270].ID, client.Spells[12270].Level);
                        if (spell != null)
                        {
                            int stamina = 100;
                            if (client.Entity.HeavenBlessing > 0)
                                stamina += 50;
                            if (client.Spells != null)
                            {
                                if (client.Spells.ContainsKey(12560))
                                {
                                    var spells = client.Spells[12560];
                                    var skill = Database.SpellTable.SpellInformations[12560][spells.Level];
                                    stamina += (int)skill.Power;
                                }
                            }
                            if (client.Entity.Stamina != stamina)
                            {
                                client.Entity.Stamina += (byte)spell.Power;
                                if (client.Entity.ContainsFlag3(Update.Flags3.DragonCyclone))
                                    if (client.Entity.Stamina != stamina)
                                        client.Entity.Stamina += (byte)spell.Power;
                                _String str = new _String(true);
                                str.UID = client.Entity.UID;
                                str.TextsCount = 1;
                                str.Type = _String.Effect;
                                str.Texts.Add("leedragonblood");
                                client.SendScreen(str, true);
                            }
                        }
                    }
                    client.Entity.DragonFlowStamp = Time32.Now;
                }
            }
            #endregion
            #region DragonSwing
            if (client.Entity.ContainsFlag3(Update.Flags3.DragonSwing))
            {
                if (Time32.Now > client.Entity.DragonSwingStamp.AddSeconds(160))
                {
                    client.Entity.RemoveFlag3(Update.Flags3.DragonSwing);
                    client.Entity.OnDragonSwing = false;
                    Update upgrade = new Update(true);
                    upgrade.UID = client.Entity.UID;
                    upgrade.Append(Update.DragonSwing, 0, 0, 0, 0);
                    client.Entity.Owner.Send(upgrade.ToArray());
                }
            }
            #endregion
            if (client.Entity.race == 1 && World.cycolne1 == true)
            {
                client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Ride);

                client.Entity.CycloneStamp = Time32.Now;
                client.Entity.CycloneTime = 180;
                client.Entity.AddFlag(Update.Flags.Cyclone);
                client.Entity.race = 0;
                Random R = new Random();
                int Nr = R.Next(1, 2);
                if (Nr == 1) client.Entity.Teleport(1645, 309, 238);
                if (Nr == 2) client.Entity.Teleport(1645, 305, 231);

            }
            if (!World.cycolne3 && client.Entity.MapID == 1645)
            {

                client.Entity.Teleport(1002, 435 - 128, 378 - 100);

            }
            if (client.Entity.ContainsFlag(Update.Flags.Ride) && client.Entity.MapID == 1645)
            {
                client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Ride);
            }
        }
        private void CharactersCallback(GameState client, int time)
        {
            #region lacb
            if (client.Entity.lacb >= 10 & client.Entity.lacb <= 300)
            {//MenaMagice 
                client.Entity.Update((byte)Update.mantos, 1, true);
            }
            if (client.Entity.lacb >= 300 & client.Entity.lacb <= 600)
            {
                client.Entity.Update((byte)Update.mantos, 2, true);
            }
            if (client.Entity.lacb >= 600 & client.Entity.lacb <= 900)
            {
                client.Entity.Update((byte)Update.mantos, 3, true);
            }
            if (client.Entity.lacb >= 900 & client.Entity.lacb <= 1300)
            {
                client.Entity.Update((byte)Update.mantos, 4, true);
            }
            if (client.Entity.lacb >= 1300 & client.Entity.lacb <= 1600)
            {
                client.Entity.Update((byte)Update.mantos, 5, true);
            }
            if (client.Entity.lacb >= 1600 & client.Entity.lacb <= 1900)
            {
                client.Entity.Update((byte)Update.mantos, 6, true);
            }
            if (client.Entity.lacb >= 1900 & client.Entity.lacb <= 2200)
            {
                client.Entity.Update((byte)Update.mantos, 7, true);
            }
            if (client.Entity.lacb >= 2200 & client.Entity.lacb <= 2800)
            {
                client.Entity.Update((byte)Update.mantos, 8, true);
            }
            if (client.Entity.lacb >= 2800 & client.Entity.lacb <= 3400)
            {
                client.Entity.Update((byte)Update.mantos, 9, true);
            }
            if (client.Entity.lacb >= 3400 & client.Entity.lacb <= 4200)
            {
                client.Entity.Update((byte)Update.mantos, 10, true);
            }
            if (client.Entity.lacb >= 4200 & client.Entity.lacb <= 5400)
            {
                client.Entity.Update((byte)Update.mantos, 11, true);
            }
            if (client.Entity.lacb >= 5400 & client.Entity.lacb <= 6800)
            {
                client.Entity.Update((byte)Update.mantos, 12, true);
            }
            if (client.Entity.lacb >= 6800)
            {
                client.Entity.Update((byte)Update.mantos, 13, true);
            }
            #endregion

            #region Time Check
            if (DateTime.Now.Second == 00)
            #endregion Time Check
            {
                if (!Valid(client)) return;
                #region Winners for FB and SS
                if (client.Entity.aWinner == true)
                {
                    if (Time32.Now > client.Entity.WinnerWaiting.AddSeconds(1))
                    {
                        switch (client.Entity.MapID)
                        {
                            case 1543://room 1 
                                {
                                    Program.Room1 = false;
                                    break;
                                }
                            case 1544://room 2 
                                {
                                    Program.Room2 = false;
                                    break;
                                }
                            case 1545://room 3 
                                {
                                    Program.Room3 = false;
                                    break;
                                }
                            case 1546://room 4 
                                {
                                    Program.Room4 = false;
                                    break;
                                }
                            case 1547://room 5 
                                {
                                    Program.Room5 = false;
                                    break;
                                }
                            case 1548://room 6 
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
                Time32 Now32 = new Time32(time);
                DateTime Now64 = DateTime.Now;

                if (client.Entity.Titles.Count > 0)
                {
                    foreach (var titles in client.Entity.Titles)
                    {
                        if (Now64 > titles.Value)
                        {
                            client.Entity.RemoveTopStatus((UInt64)titles.Key);
                        }
                    }
                }
                if (client.OnDonation)
                {
                    if (DateTime.Now >= client.matrixtime.AddHours(1.0))
                    {
                        SafeDictionary<uint, Game.ConquerStructures.NobilityInformation> Board = new SafeDictionary<uint, Game.ConquerStructures.NobilityInformation>(10000);
                        client.NobilityInformation.Donation -= client.Donationx;
                        Board.Add(client.Entity.UID, client.NobilityInformation);
                        Database.NobilityTable.UpdateNobilityInformation(client.NobilityInformation);
                        Game.ConquerStructures.Nobility.Sort(client.Entity.UID);
                        client.OnDonation = false;
                    }
                }

                if ((client.Entity.attributes9 == true) && (DateTime.Now > client.Entity.attributestime9.AddSeconds(80.0)) && client.Entity.StartTimer)
                {
                    client.Entity.MaxAttack -= 3000;
                    client.Entity.MinAttack -= 3000;
                    client.Entity.MaxHitpoints -= 3000;
                    client.Entity.Hitpoints -= 3000;
                    client.Entity.MagicAttack -= 3000;
                    client.Entity.attributes9 = false;
                }
                if ((client.Entity.attributes8 == true) && (DateTime.Now > client.Entity.attributestime8.AddSeconds(80.0)) && client.Entity.StartTimer)
                {
                    client.Entity.attributes8 = false;
                }
                if ((client.Entity.attributes7 == true) && (DateTime.Now > client.Entity.attributestime7.AddSeconds(80.0)) && client.Entity.StartTimer)
                {
                    client.Entity.Breaktrough -= 1500;
                    client.Entity.attributes7 = false;
                }
                if ((client.Entity.attributes6 == true) && (DateTime.Now > client.Entity.attributestime6.AddSeconds(80.0)) && client.Entity.StartTimer)
                {
                    client.Entity.CriticalStrike -= 15000;
                    client.Entity.SkillCStrike -= 15000;
                    client.Entity.attributes6 = false;
                }
                if ((client.Entity.attributes5 == true) && (DateTime.Now > client.Entity.attributestime5.AddSeconds(80.0)) && client.Entity.StartTimer)
                {
                    client.Entity.Counteraction -= 1500;
                    client.Entity.attributes5 = false;
                }
                if ((client.Entity.attributes4 == true) && (DateTime.Now > client.Entity.attributestime4.AddSeconds(80.0)) && client.Entity.StartTimer)
                {
                    client.Entity.Immunity -= 15000;
                    client.Entity.attributes4 = false;
                }
                if ((client.Entity.attributes3 == true) && (DateTime.Now > client.Entity.attributestime3.AddSeconds(80.0)) && client.Entity.StartTimer)
                {
                    client.Entity.PhysicalDamageIncrease -= 3000;
                    client.Entity.attributes3 = false;
                }
                if ((client.Entity.attributes2 == true) && (DateTime.Now > client.Entity.attributestime2.AddSeconds(80.0)) && client.Entity.StartTimer)
                {
                    client.Entity.MagicDamageIncrease -= 3000;
                    client.Entity.attributes2 = false;
                }
                if ((client.Entity.attributes1 == true) && (DateTime.Now > client.Entity.attributestime1.AddSeconds(80.0)) && client.Entity.StartTimer)
                {
                    client.Entity.PhysicalDamageDecrease -= 3000;
                    client.Entity.attributes1 = false;
                }
                if ((client.Entity.attributes == true) && (DateTime.Now > client.Entity.attributestime.AddSeconds(80.0)) && client.Entity.StartTimer)
                {
                    client.Entity.MagicDamageDecrease -= 3000;
                    client.Entity.attributes = false;
                }




                #region Training points
                if (client.Entity.HeavenBlessing > 0 && !client.Entity.Dead)
                {
                    if (Now32 > client.LastTrainingPointsUp.AddMinutes(10))
                    {
                        client.OnlineTrainingPoints += 10;
                        if (client.OnlineTrainingPoints >= 30)
                        {
                            client.OnlineTrainingPoints -= 30;
                            client.IncreaseExperience(client.ExpBall / 100, false);
                        }
                        client.LastTrainingPointsUp = Now32;
                        client.Entity.Update(Network.GamePackets.Update.OnlineTraining, client.OnlineTrainingPoints, false);
                    }
                }
                #endregion
                #region Extra treasure points
                if (client.AllowedTreasurePoints)
                {
                    if (Now32 > client.LastTreasurePoints.AddMinutes(1))
                    {
                        client.Entity.TreasuerPoints++;
                        client.LastTreasurePoints = Time32.Now;
                    }
                }
                #endregion
                #region Minning
                if (client.Mining && !client.Entity.Dead)
                {
                    if (Now32 >= client.MiningStamp.AddSeconds(2))
                    {
                        client.MiningStamp = Now32;
                        Game.ConquerStructures.Mining.Mine(client);
                    }
                }
                #endregion
                #region Class Fix With Auto Skill
                #region Trojan
                if (client.Entity.Class == 16)
                {
                    client.Entity.Class -= 1;
                }
                #endregion
                #region Warrior
                if (client.Entity.Class == 26)
                {
                    client.Entity.Class -= 1;
                }
                #endregion
                #region Archer
                if (client.Entity.Class == 46)
                {
                    client.Entity.Class -= 1;
                }
                #endregion
                #region Ninja
                if (client.Entity.Class == 56)
                {
                    client.Entity.Class -= 1;
                }
                #endregion
                #region Monk
                if (client.Entity.Class == 66)
                {
                    client.Entity.Class -= 1;
                }
                #endregion
                #region Pirate
                if (client.Entity.Class == 76)
                {
                    client.Entity.Class -= 1;
                }
                #endregion
                #region Leelong
                if (client.Entity.Class == 86)
                {
                    client.Entity.Class -= 1;
                }
                #endregion
                #region Toaist
                if (client.Entity.Class == 103)
                {
                    client.Entity.Class -= 1;
                }
                #endregion
                #region Water
                if (client.Entity.Class == 136)
                {
                    client.Entity.Class -= 1;
                }
                #endregion
                #region Fire
                if (client.Entity.Class == 146)
                {
                    client.Entity.Class -= 1;
                }
                #endregion
                #endregion
                #region MentorPrizeSave
                if (Now32 > client.LastMentorSave.AddSeconds(5))
                {
                    Database.KnownPersons.SaveApprenticeInfo(client.AsApprentice);
                    client.LastMentorSave = Now32;
                }
                #endregion
                #region Attackable
                if (client.JustLoggedOn)
                {
                    client.JustLoggedOn = false;
                    client.ReviveStamp = Now32;
                }
                if (!client.Attackable)
                {
                    if (Now32 > client.ReviveStamp.AddSeconds(5))
                    {
                        client.Attackable = true;
                    }
                }
                #endregion
                #region DoubleExperience
                if (client.Entity.DoubleExperienceTime == 0 && client.SuperPotion > 0)
                {
                    client.SuperPotion = 0;
                }
                if (client.Entity.DoubleExperienceTime > 0)
                {
                    if (Now32 >= client.Entity.DoubleExpStamp.AddMilliseconds(1000))
                    {
                        client.Entity.DoubleExpStamp = Now32;
                        client.Entity.DoubleExperienceTime--;
                    }
                }
                #endregion
                #region HeavenBlessing
                if (client.Entity.HeavenBlessing > 0)
                {
                    if (Now32 > client.Entity.HeavenBlessingStamp.AddMilliseconds(1000))
                    {
                        client.Entity.HeavenBlessingStamp = Now32;
                        client.Entity.HeavenBlessing--;
                    }
                }
                #endregion
                #region Enlightment
                if (client.Entity.EnlightmentTime > 0)
                {
                    if (Now32 >= client.Entity.EnlightmentStamp.AddMinutes(1))
                    {
                        client.Entity.EnlightmentStamp = Now32;
                        client.Entity.EnlightmentTime--;
                        if (client.Entity.EnlightmentTime % 10 == 0 && client.Entity.EnlightmentTime > 0)
                            client.IncreaseExperience(Game.Attacking.Calculate.Percent((int)client.ExpBall, .10F), false);
                    }
                }
                #endregion
                #region starTeam
                if (client.Team != null)
                {
                    if (client.Entity.MapID == client.Team.Lider.Entity.MapID)
                    {
                        Data Data = new Data(true);
                        Data.UID = client.Team.Lider.Entity.UID;
                        Data.dwParam = client.Team.Lider.Entity.MapID;
                        Data.ID = Data.TeamMemberPos;
                        Data.wParam1 = client.Team.Lider.Entity.X;
                        Data.wParam2 = client.Team.Lider.Entity.Y;
                        Data.Send(client);
                    }
                }
                #endregion
                #region PKPoints
                if (Now32 >= client.Entity.PKPointDecreaseStamp.AddMinutes(5))
                {
                    client.Entity.PKPointDecreaseStamp = Now32;
                    if (client.Entity.PKPoints > 0)
                    {
                        client.Entity.PKPoints--;
                    }
                    else
                        client.Entity.PKPoints = 0;
                }
                #endregion
                #region OverHP
                if (client.Entity.FullyLoaded)
                {
                    if (client.Entity.Hitpoints > client.Entity.MaxHitpoints && client.Entity.MaxHitpoints > 1 && !client.Entity.Transformed)
                    {
                        client.Entity.Hitpoints = client.Entity.MaxHitpoints;
                    }
                }
                #endregion
                #region Room
                #region Room
                if (client.Entity.MapID == 1543)
                {
                    if (client.Entity.MapID == 1543)
                    {
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.ShurikenVortex);
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.ReflectMelee);
                    }
                }
                #endregion
                #region Room
                if (client.Entity.MapID == 1544)
                {
                    if (client.Entity.MapID == 1544)
                    {
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.ShurikenVortex);
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.ReflectMelee);
                    }
                }
                #endregion
                #region Room
                if (client.Entity.MapID == 1545)
                {
                    if (client.Entity.MapID == 1545)
                    {
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.ShurikenVortex);
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.ReflectMelee);
                    }
                }
                #endregion
                #region Room
                if (client.Entity.MapID == 1546)
                {
                    if (client.Entity.MapID == 1546)
                    {
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.ShurikenVortex);
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.ReflectMelee);
                    }
                }
                #endregion
                #region Room
                if (client.Entity.MapID == 1547)
                {
                    if (client.Entity.MapID == 1547)
                    {
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.ShurikenVortex);
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.ReflectMelee);
                    }
                }
                #endregion
                #region Room
                if (client.Entity.MapID == 1548)
                {
                    if (client.Entity.MapID == 1548)
                    {
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.ShurikenVortex);
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.ReflectMelee);
                    }
                }
                #endregion
                #endregion

                #region Die Delay
                if (client.Entity.Hitpoints == 0 && client.Entity.ContainsFlag(Network.GamePackets.Update.Flags.Dead) && !client.Entity.ContainsFlag(Network.GamePackets.Update.Flags.Ghost))
                {
                    if (Now32 > client.Entity.DeathStamp.AddSeconds(2))
                    {
                        client.Entity.AddFlag(Network.GamePackets.Update.Flags.Ghost);
                        if (client.Entity.Body % 10 < 3)
                            client.Entity.TransformationID = 99;
                        else
                            client.Entity.TransformationID = 98;

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
                    if (Now32 > client.Entity.ChainboltStamp.AddSeconds(client.Entity.ChainboltTime))
                        client.Entity.RemoveFlag2(Update.Flags2.ChainBoltActive);
                #endregion

                if (client.Entity.HasMagicDefender && Now32 >= client.Entity.MagicDefenderStamp.AddSeconds(client.Entity.MagicDefenderSecs))
                {
                    client.Entity.RemoveMagicDefender();
                }
                if (Now32 >= client.Entity.BlackbeardsRageStamp.AddSeconds(60))
                {
                    client.Entity.RemoveFlag2(MTA.Network.GamePackets.Update.Flags2.BlackbeardsRage);
                }
                if (Now32 >= client.Entity.CannonBarrageStamp.AddSeconds(60))
                {
                    client.Entity.RemoveFlag2(MTA.Network.GamePackets.Update.Flags2.CannonBarrage);
                }
                if (Now32 >= client.Entity.FatigueStamp.AddSeconds(client.Entity.FatigueSecs))
                {
                    client.Entity.RemoveFlag2(MTA.Network.GamePackets.Update.Flags2.Fatigue);
                    client.Entity.IsDefensiveStance = false;
                }
                if (Now32 > client.Entity.GuildRequest.AddSeconds(30))
                {
                    client.GuildJoinTarget = 0;
                }


                #region Equipment
                if (client.Entity.MapID == 1036)
                {
                    if (Kernel.GetDistance(client.Entity.X, client.Entity.Y, 184, 205) < 17 && !client.Effect)
                    {
                        client.Effect = true;
                        if (client.Entity.MapID == 1036)
                        {
                            Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                            floorItem.ItemID = 812;
                            floorItem.MapID = 1036;
                            floorItem.X = 184;
                            floorItem.Y = 205;
                            floorItem.Type = Network.GamePackets.FloorItem.Effect;
                            client.Send(floorItem);
                        }
                    }
                    else
                    {
                        if (Kernel.GetDistance(client.Entity.X, client.Entity.Y, 184, 205) > 17)
                        {
                            client.Effect = false;
                        }
                    }
                }
                #endregion
                #region Team Qualifier
                if ((Now64.Hour == 11 || Now64.Hour == 19) && Now64.Minute == 19 && Now64.Second == 2)
                {
                    client.MessageBox("TeamArena has started! It will open for two hours! Would you like to sign up?",
                        (p) => { TeamArena.QualifyEngine.DoSignup(p); },
                        (p) => { p.Send("You can still join from the team arena interface!"); });
                }
                #endregion
                #region Weekly PK
                if (Now64.Second <= 2 && Now64.DayOfWeek == DayOfWeek.Saturday && Now64.Hour == 20 && Now64.Minute == 00)
                {
                    client.MessageBox("Weekly PK has begun! Would you like to join? Prize [ TOP And Cps]",
                          (p) => { p.Entity.Teleport(1002, 327, 194); }, null, 60);
                }
                #endregion
                #region Night
                if (rates.Night == 1)
                {
                    if (client.Entity.MapID == 701)
                    {
                        Random disco = new Random();
                        uint discocolor = (uint)disco.Next(50000, 999999999);
                        Network.GamePackets.Data datas = new Network.GamePackets.Data(true);
                        datas.UID = client.Entity.UID;
                        datas.ID = 104;
                        datas.dwParam = discocolor;
                        client.Send(datas);
                    }
                    else
                    {
                        if (DateTime.Now.Minute >= 40 && DateTime.Now.Minute <= 45)
                        {
                            Network.GamePackets.Data datas = new Network.GamePackets.Data(true);
                            datas.UID = client.Entity.UID;
                            datas.ID = 104;
                            datas.dwParam = 5855577;
                            client.Send(datas);
                        }
                        else
                        {
                            Network.GamePackets.Data datas = new Network.GamePackets.Data(true);
                            datas.UID = client.Entity.UID;
                            datas.ID = 104;
                            datas.dwParam = 0;
                            client.Send(datas);
                        }
                    }
                }

                #endregion
            }
        }
        private void AutoAttackCallback(GameState client, int time)
        {
            if (!Valid(client)) return;
            Time32 Now = new Time32(time);
            if (client.Entity.AttackPacket != null || client.Entity.VortexPacket != null)
            {
                try
                {
                    if (client.Entity.ContainsFlag(Network.GamePackets.Update.Flags.ShurikenVortex))
                    {
                        if (client.Entity.VortexPacket != null && client.Entity.VortexPacket.ToArray() != null)
                        {
                            if (Now > client.Entity.VortexAttackStamp.AddMilliseconds(1400))
                            {
                                client.Entity.VortexAttackStamp = Now;
                                client.Entity.VortexPacket.AttackType = Attack.Magic;
                                new Game.Attacking.Handle(client.Entity.VortexPacket, client.Entity, null);
                            }
                        }
                    }
                    else
                    {
                        var AttackPacket = client.Entity.AttackPacket;
                        if (AttackPacket != null && AttackPacket.ToArray() != null)
                        {
                            uint AttackType = AttackPacket.AttackType;
                            if (AttackType == Network.GamePackets.Attack.Magic || AttackType == Network.GamePackets.Attack.Melee || AttackType == Network.GamePackets.Attack.Ranged)
                            {
                                if (AttackType == Network.GamePackets.Attack.Magic)
                                {
                                    if (Now > client.Entity.AttackStamp.AddSeconds(1))
                                    {
                                        if (AttackPacket.Damage != 12160 &&
                                            AttackPacket.Damage != 12170 &&
                                            AttackPacket.Damage != 12120 &&
                                            AttackPacket.Damage != 12130 &&
                                            AttackPacket.Damage != 12140 &&
                                            AttackPacket.Damage != 12320 &&
                                            AttackPacket.Damage != 12330 &&
                                            AttackPacket.Damage != 12340 &&
                                            AttackPacket.Damage != 12570 &&
                                            AttackPacket.Damage != 12210)
                                        {
                                            new Game.Attacking.Handle(AttackPacket, client.Entity, null);
                                        }
                                    }
                                }

                                else
                                {
                                    int decrease = 300;
                                    if (client.Entity.OnCyclone())
                                        decrease = 700;
                                    if (client.Entity.OnSuperman())
                                        decrease = 200;
                                    if (Now > client.Entity.AttackStamp.AddMilliseconds((1000 - client.Entity.Agility - decrease) * (int)(AttackType == Network.GamePackets.Attack.Ranged ? 1 : 1)))
                                    {
                                        new Game.Attacking.Handle(AttackPacket, client.Entity, null);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Program.SaveException(e);
                    client.Entity.AttackPacket = null;
                    client.Entity.VortexPacket = null;
                }
            }
        }

        private void PrayerCallback(GameState client, int time)
        {
            if (!Valid(client)) return;
            Time32 Now = new Time32(time);

            if (client.Entity.Reborn > 1)
                return;

            if (!client.Entity.ContainsFlag(Network.GamePackets.Update.Flags.Praying))
            {
                foreach (Interfaces.IMapObject ClientObj in client.Screen.Objects)
                {
                    if (ClientObj != null)
                    {
                        if (ClientObj.MapObjType == Game.MapObjectType.Player)
                        {
                            var Client = ClientObj.Owner;
                            if (Client.Entity.ContainsFlag(Network.GamePackets.Update.Flags.CastPray))
                            {
                                if (Kernel.GetDistance(client.Entity.X, client.Entity.Y, ClientObj.X, ClientObj.Y) <= 3)
                                {
                                    client.Entity.AddFlag(Network.GamePackets.Update.Flags.Praying);
                                    client.PrayLead = Client;
                                    client.Entity.Action = Client.Entity.Action;
                                    Client.Prayers.Add(client);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (client.PrayLead != null)
                {
                    if (Kernel.GetDistance(client.Entity.X, client.Entity.Y, client.PrayLead.Entity.X, client.PrayLead.Entity.Y) > 4)
                    {
                        client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Praying);
                        client.PrayLead.Prayers.Remove(client);
                        client.PrayLead = null;
                    }
                }
            }
        }

        private void WorldTournaments(int time)
        {
            Time32 Now = new Time32(time);
            DateTime Now64 = DateTime.Now;
            /*#region Server Auto Restart
            if (Now64.Hour == 12 && Now64.Minute >= 00 && Now64.Minute <= 10 && Now64.Second == 01)
            {
                foreach (Client.GameState Server in Kernel.GamePool.Values)
                {
                    Program.mess--;
                    MTA.Network.GamePackets.Message FiveMinute = new Network.GamePackets.Message("The server will be brought down for maintenance in "+Program.mess+" Minutes. Please exit the game now.", System.Drawing.Color.Red, Network.GamePackets.Message.Center);
                    Server.Send(FiveMinute);
                    if (Program.mess == 0 || Program.mess == 1)
                    {
                        Program.CommandsAI("@restart");
                        return;
                    }
                }
            }
            #endregion*/
            #region CP Castle Event
            #region CP Castle Event
            if (DateTime.Now.Hour == 13 && DateTime.Now.Minute == 50 && DateTime.Now.Second == 0 ||
                DateTime.Now.Hour == 19 && DateTime.Now.Minute == 50 && DateTime.Now.Second == 0)
            {
                foreach (var client in Program.Values)
                {
                    client.Send(new Message("Hurry! CP Castle Event It Will Begin 10 Minutes After Getting Ready", Message.System));
                }
            }
            if (DateTime.Now.Hour == 13 && DateTime.Now.Minute == 55 && DateTime.Now.Second == 0 ||
                DateTime.Now.Hour == 19 && DateTime.Now.Minute == 55 && DateTime.Now.Second == 0)
            {
                foreach (var client in Program.Values)
                {
                    client.Send(new Message("CP Castle Event It Will Begin 5 Minutes After Getting Ready", Message.System));
                }
            }
            if (DateTime.Now.Hour == 13 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 50 ||
                DateTime.Now.Hour == 19 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 50)
            {
                foreach (var client in Program.Values)
                {
                    client.Send(new Message("CP Castle Event It Will Begin 10 Second After Getting Ready", Message.System));
                }
            }
            if (DateTime.Now.Hour == 14 && DateTime.Now.Minute == 00 && DateTime.Now.Second == 0 ||
                DateTime.Now.Hour == 20 && DateTime.Now.Minute == 00 && DateTime.Now.Second == 0)
            {
                foreach (var client in Program.Values)
                {
                    Kernel.SendWorldMessage(new Message("Hurry! CP Castle Event Then He Began To Log on Now Quickly", Color.White, Message.Service));
                }
            }
            if (DateTime.Now.Hour == 14 && DateTime.Now.Minute == 20 && DateTime.Now.Second == 0 ||
                DateTime.Now.Hour == 20 && DateTime.Now.Minute == 20 && DateTime.Now.Second == 0)
            {
                foreach (var client in Program.Values)
                {
                    Kernel.SendWorldMessage(new Message("CP Castle Event Will End After 10 Minutes. Hurry To Get Reward", Color.White, Message.System));
                }
            }
            if (DateTime.Now.Hour == 14 && DateTime.Now.Minute == 25 && DateTime.Now.Second == 0 ||
                DateTime.Now.Hour == 20 && DateTime.Now.Minute == 25 && DateTime.Now.Second == 0)
            {
                foreach (var client in Program.Values)
                {
                    Kernel.SendWorldMessage(new Message("CP Castle Event Will End After 5 Minutes. Hurry To Get Reward", Color.White, Message.System));
                }
            }
            if (DateTime.Now.Hour == 14 && DateTime.Now.Minute == 30 && DateTime.Now.Second == 0 ||
                DateTime.Now.Hour == 20 && DateTime.Now.Minute == 30 && DateTime.Now.Second == 0)
            {
                foreach (var client in Program.Values)
                {
                    Kernel.SendWorldMessage(new Message("CP Castle Event I Ended up Next Time", Color.White, Message.System));
                    if (client.Entity.MapID == 3030 ||
                        client.Entity.MapID == 3031 ||
                        client.Entity.MapID == 3032 ||
                        client.Entity.MapID == 3033)
                    {
                        client.Entity.Teleport(1002, 300, 280);
                    }
                }
            }
            #endregion
            #region CP Castle Event(AutoInvite)
            if (DateTime.Now.Hour == 14 && DateTime.Now.Minute == 00 && DateTime.Now.Second == 0 ||
                DateTime.Now.Hour == 20 && DateTime.Now.Minute == 00 && DateTime.Now.Second == 0)
            {
                Kernel.SendWorldMessage(new Message("CP Castle War began !", Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("CP Castle began! Would you Like to join ...?",
                       p => { p.Entity.Teleport(1002, 288, 280); }, null, 60);
            }
            #endregion
            #endregion

            HeroOFGame.CheakUp();
            if (Matrix_Times.Start.SkillTeam && !Game.Features.Tournaments.TeamElitePk.SkillTeamTournament.Opened)
            {
                Game.Features.Tournaments.TeamElitePk.SkillTeamTournament.Open();
                foreach (Client.GameState client in Kernel.GamePool.Values)
                {
                    client.ClaimedSkillTeam = 0;
                    if (client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Entity.Dead)
                    {

                        EventAlert alert = new EventAlert
                        {
                            StrResID = 10541,
                            Countdown = 60,
                            UK12 = 1
                        };
                        client.Entity.StrResID = 10541;
                        client.Send(alert);
                    }
                }
            }
            if (Matrix_Times.Start.TeamPK && !Game.Features.Tournaments.TeamElitePk.TeamTournament.Opened)
            {
                Game.Features.Tournaments.TeamElitePk.TeamTournament.Open();
                foreach (Client.GameState client in Kernel.GamePool.Values)
                {
                    client.ClaimedTeampk = 0;
                    if (client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Entity.Dead)
                    {

                        EventAlert alert = new EventAlert
                        {
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
            if (DateTime.Now.DayOfWeek == DayOfWeek.Friday && DateTime.Now.Hour == 19 && DateTime.Now.Minute == 30 && DateTime.Now.Second == 1)
            {
                Kernel.SendWorldMessage(new Network.GamePackets.Message("Couples PkWar has started! You have 5 minute to signup go to TC CouplesPkGuide in TwinCity!", System.Drawing.Color.White, Network.GamePackets.Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    if (client.Entity.Spouse != "None")
                        client.MessageBox("CouplesPk War has started! Would you like to join? [Prize: " + 5000000 + " CPs]",
                            p => { p.Entity.Teleport(1002, 275, 187); }, null);
            }
            #endregion
            #region cycolne race
            if (DateTime.Now.Minute == 57 && DateTime.Now.Second == 1)
            {
                World.cycolne3 = true;
                Game.Entity.Speed = 0;
                foreach (var client in Program.Values)
                    client.MessageBox("Cycolne Race Start U Like To Join And Get " + 100000 + " CPS ",
                             p => { p.Entity.Teleport(1002, 308, 235); }, null);
            }
            if (DateTime.Now.Minute == 58 && DateTime.Now.Second == 1)
            {
                World.cycolne1 = true;
            }
            if (DateTime.Now.Minute == 59 && cycolne3)
            {
                cycolne3 = false;
                World.cycolne1 = false;

            }
            #endregion
            //

            #region Elite GW
            {
                foreach (var client in Program.Values)
                    if (client.Entity.MapID == 6000 || client.Entity.MapID == 6001 || client.Entity.MapID == 6002 || client.Entity.MapID == 6003 || client.Entity.MapID == 6004)
                        return;
                if (!Game.EliteGuildWar.IsWar)
                {
                    if (Now64.Minute == 15 && Now64.Second == 01)
                    {
                        Game.EliteGuildWar.Start();
                        foreach (var client in Program.Values)
                            if (client.Entity.GuildID != 0)
                                client.MessageBox("Elite GuildWar has begun! Would you like to join?",
                                    p => { p.Entity.Teleport(1002, 286, 158); }, null);
                    }
                }
                if (Game.EliteGuildWar.IsWar)
                {
                    if (Time32.Now > Game.EliteGuildWar.ScoreSendStamp.AddSeconds(3))
                    {
                        Game.EliteGuildWar.ScoreSendStamp = Time32.Now;
                        Game.EliteGuildWar.SendScores();
                    }
                    if (Now64.Minute == 25 && Now64.Second <= 02)
                    {
                        Kernel.SendWorldMessage(new Network.GamePackets.Message("5 Minutes left till Elite GuildWar End Hurry kick other Guild's Ass!.", System.Drawing.Color.White, Network.GamePackets.Message.Center), Program.Values);
                    }
                }

                if (Game.EliteGuildWar.IsWar)
                {
                    if (Now64.Minute == 29 && Now64.Second == 58)
                    {
                        Game.EliteGuildWar.End();
                        {
                            Kernel.SendWorldMessage(new Network.GamePackets.Message("Elite Guild War Ended Thanks To MTA.", System.Drawing.Color.White, Network.GamePackets.Message.Center), Program.Values);
                        }
                    }
                }
            }
            #endregion
            #region Clan War
            if ((Now64.Hour == 21 || Now64.Hour == 16) && Now64.Minute == 00 && Now64.Second == 05 && !ClanWar.IsWar)
            {
                Game.ClanWar.Start();
                ClanWarAI = false;
                if (Now64.Hour != 16)
                {
                    ClanWarAI = Now64.Hour != 16;
                    foreach (var client in Program.Values)
                        if (client.Entity.GuildID != 0)
                            client.MessageBox("ClanWar Has Begun! Would You Like To Join This War ...?",
                                p => { p.Entity.Teleport(1002, 284, 146); }, null);
                }
            }
            if (Now64.Hour == 16 && Now64.Minute == 10 && !ClanWarAI)
            {
                ClanWarAI = true;
                foreach (var client in Program.Values)
                    if (client.Entity.GuildID != 0)
                        client.MessageBox("ClanWar Has Begun! Would You Like To Join This War ...?",
                            p => { p.Entity.Teleport(1002, 284, 146); }, null);
            }
            if ((Now64.Hour == 22 || Now64.Hour == 17) && Now64.Minute == 00 && ClanWar.IsWar)
            {
                Game.ClanWar.End();
            }
            if (Game.ClanWar.IsWar)
            {
                if (Time32.Now > Game.ClanWar.ScoreSendStamp.AddSeconds(3))
                {
                    Game.ClanWar.ScoreSendStamp = Time32.Now;
                    Game.ClanWar.SendScores();
                }

            }
            #endregion
            #region Dis City
            if (Now64.DayOfWeek == DayOfWeek.Wednesday || Now64.DayOfWeek == DayOfWeek.Sunday)
            {
                if ((Now64.Hour == 12 || Now64.Hour == 19) && Now64.Minute == 05 && Now64.Second == 2)
                {
                    Kernel.SendWorldMessage(new Network.GamePackets.Message("DisCity signup has been closed. Please try next time!", System.Drawing.Color.White, Network.GamePackets.Message.Center), Program.Values);

                    Game.Features.DisCity.Signup = false;
                }
            }
            #endregion
            #region Class PK
            if (Now64.Hour == 20 && Now64.Minute == 30 && Now64.Second == 0 || Now64.Hour == 8 && Now64.Minute == 30 && Now64.Second == 0)
            {
                Kernel.SendWorldMessage(new Message("Class PK War began! all Go Twin 302, 148", Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values) ;

            }
            #endregion
            #region Monthly PK
            if (Now64.Day <= 7 && Now64.DayOfWeek == DayOfWeek.Sunday)
            {
                if (Now64.Hour == 21 && Now64.Minute >= 50 && Now64.Second == 2)
                {
                    int min = 60 - Now64.Minute;
                    Kernel.SendWorldMessage(new Message("MonthelyPk " + min.ToString() + " Minute!", Color.Red, 2012));
                }
                if (Now64.Hour == 22 && Now64.Minute == 00 && Now64.Second <= 2)
                {
                    MonthlyPKWar = true;
                    foreach (Client.GameState client in Kernel.GamePool.Values)
                    {
                        if (client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Entity.Dead)
                        {
                            EventAlert alert = new EventAlert
                            {
                                StrResID = 10523,
                                Countdown = 60,
                                UK12 = 1
                            };
                            client.Entity.StrResID = 10523;
                            client.Send(alert);
                        }
                    }
                }
                if (Now64.Hour == 22 && Now64.Minute >= 15 && MonthlyPKWar)
                {
                    MonthlyPKWar = false;
                    Kernel.SendWorldMessage(new Message("MonthelyPk Ended!", Color.Red, Message.Center));
                }
            }
            #endregion
            #region LuckyMan
            if (Now64.Minute == 43 && Now64.Second == 5)
            {
                Kernel.SendWorldMessage(new Message("Lucky Man War began !", Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("Lucky Man began! Would you Like to join ...?",
                     p => { p.Entity.Teleport(1002, 288, 360); }, null, 60);
            }
            #endregion
            #region SuperGuildWar
            if (Now64.Hour == 20 && Now64.Minute == 5 && Now64.Second == 0 && (Now64.Day == 1 || Now64.Day == 7 || Now64.Day == 14 || Now64.Day == 21))
            {
                Kernel.SendWorldMessage(new Message("Super Guild War now work will end at [23:00] Server time? !", Color.White, Message.BroadcastMessage), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("Super Guild War now work will end at 23:00 Server time?",
                    p => { p.Entity.Teleport(1038, 348, 337); }, null, 60);
            }
            #endregion


            //     #region uniqureKiller
            //   if ()
            //      if (DateTime.Now.Hour == 18 && DateTime.Now.Minute == 19 && DateTime.Now.Second == 59)
            //      {
            //          Program.uniquepk = true;
            //           Kernel.SendWorldMessage(new Message("UniqueKiller War began!", Color.Red, Message.Center), Program.Values);
            //          foreach (var client in Program.Values)

            //             client.MessageBox("UniqueKiller began! Would you like to join ...?",
            //                 p => { p.Entity.Teleport(1002, 255, 235); }, null, 60);
            //    }
            //    #endregion
            //////////////////////
            #region PoleIslanD

            if (!Game.PoleIslanD.IsWar)
            {
                if (Now64.Hour == 16 && Now64.Minute == 00 && Now64.Second == 35)
                {
                    Game.PoleIslanD.Start();

                    foreach (var client in Program.Values)
                        if (client.Entity.MapID == 6000 || client.Entity.MapID == 6001 || client.Entity.MapID == 6002 || client.Entity.MapID == 6003 || client.Entity.MapID == 6004)
                            return;
                    foreach (var client in Program.Values)
                        if (client.Entity.GuildID != 0)
                            client.MessageBox("PoleIslanD has begun! Would you like to join? ",
                                p => { p.Entity.Teleport(1002, 298, 230); }, null);
                }
            }
            if (Game.PoleIslanD.IsWar)
            {
                if (Time32.Now > Game.PoleIslanD.ScoreSendStamp.AddSeconds(3))
                {
                    Game.PoleIslanD.ScoreSendStamp = Time32.Now;
                    Game.PoleIslanD.SendScores();
                }
                if (Now64.Hour == 16 && Now64.Minute == 50 && Now64.Second <= 2)
                {
                    Kernel.SendWorldMessage(new Network.GamePackets.Message("10 Minutes left till PoleIslanD End Hurry kick other Guild's Ass!.", System.Drawing.Color.White, Network.GamePackets.Message.Center), Program.Values);
                }
            }

            if (Game.PoleIslanD.IsWar)
            {
                if (Now64.Hour == 17 && Now64.Minute == 00 && Now64.Second == 04)
                {
                    Game.PoleIslanD.End();
                    {

                    }
                }
            }
            #endregion
            #region PoleRakion

            if (!Game.PoleRakion.IsWar)
            {
                if (Now64.Hour == 22 && Now64.Minute == 00 && Now64.Second == 35)
                {
                    Game.PoleRakion.Start();

                    foreach (var client in Program.Values)
                        if (client.Entity.MapID == 6000 || client.Entity.MapID == 6001 || client.Entity.MapID == 6002 || client.Entity.MapID == 6003 || client.Entity.MapID == 6004)
                            return;
                    foreach (var client in Program.Values)
                        if (client.Entity.GuildID != 0)
                            client.MessageBox("PoleRakion has begun! Would you like to join? ",
                                p => { p.Entity.Teleport(1002, 249, 215); }, null);
                }
            }
            if (Game.PoleRakion.IsWar)
            {
                if (Time32.Now > Game.PoleRakion.ScoreSendStamp.AddSeconds(3))
                {
                    Game.PoleRakion.ScoreSendStamp = Time32.Now;
                    Game.PoleRakion.SendScores();
                }
                if (Now64.Hour == 22 && Now64.Minute == 50 && Now64.Second <= 2)
                {
                    Kernel.SendWorldMessage(new Network.GamePackets.Message("10 Minutes left till PoleRakion End Hurry kick other Guild's Ass!.", System.Drawing.Color.White, Network.GamePackets.Message.Center), Program.Values);
                }
            }

            if (Game.PoleRakion.IsWar)
            {
                if (Now64.Hour == 23 && Now64.Minute == 00 && Now64.Second == 04)
                {
                    Game.PoleRakion.End();
                    {

                    }
                }
            }
            #endregion
            /////////////////////
            ////////////////////New Quests Adedd By Franko///////////////////
            #region Portals`War
            if (DateTime.Now.Minute == 10 && Now64.Second == 10)
            {
                Kernel.SendWorldMessage(new Message(" PortalsWar Pk, Now Online All Go To Play PK, !", Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(" PortalsWar Pk, Now Online, like to Join? ",
                    p => { p.Entity.Teleport(1002, 291, 360); }, null, 60);
            }
            #endregion
            #region ConquerPK PK
            if (DateTime.Now.Minute == 01 && Now64.Second == 10)
            {
                Kernel.SendWorldMessage(new Message(" ConquerPK Pk, Now Online All Go To Play PK, !", Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(" ConquerPK Pk, Now Online, like to Join? ",
                    p => { p.Entity.Teleport(1002, 274, 360); }, null, 60);
            }
            #endregion
            #region PolePrize
            if (DateTime.Now.Minute == 05 && Now64.Second == 01)
            {
                Kernel.SendWorldMessage(new Message("The War PolePrize Is Started Now ,, End This War at xx:10", Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("PolePrize Start Now Let's go Fast? ",
                    p => { p.Entity.Teleport(1002, 230, 227); }, null, 60);
            }
            #endregion
            #region Ghost PK
            if (DateTime.Now.Minute == 06 && Now64.Second == 10)
            {
                Kernel.SendWorldMessage(new Message(" Ghost Pk, Now Online All Go To Play PK !", Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(" Ghostpk Pk, Now Online, like to Join? ",
                    p => { p.Entity.Teleport(1002, 300, 361); }, null, 60);
            }
            #endregion
            #region StayAlive PK
            if (DateTime.Now.Minute == 16 && Now64.Second == 10)
            {
                Kernel.SendWorldMessage(new Message(" StayAlive Pk, Now Online All Go To Play PK !", Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(" StayAlive Pk, Now Online, like to Join? ",
                    p => { p.Entity.Teleport(1002, 297, 360); }, null, 60);
            }
            #endregion
            #region PrinceWar
            if (DateTime.Now.Minute == 23 && Now64.Second == 1)
            {
                Kernel.SendWorldMessage(new Message("(PrinceWar Pk, Now Online All Go To Play PK", Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(" PrinceWar, Now Online, like to Join? ",
                    p => { p.Entity.Teleport(1002, 274, 362); }, null, 60);
            }
            #endregion
            #region Attackers QuesT
            if (DateTime.Now.Minute == 32 && Now64.Second == 1)
            {
                Kernel.SendWorldMessage(new Message(" Attackers  Pk, Now Online All Go To Play PK !", Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(" Attackers QuesT Pk, Now Online, like to Join?",
                    (p) => { p.Entity.Teleport(1002, 294, 360); }, null, 60);
            }
            #endregion
            #region Rabbit PK
            if (DateTime.Now.Minute == 38 && Now64.Second == 1)
            {
                Kernel.SendWorldMessage(new Message(" Rabbit Pk, Now Online All Go To Play PK !", Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(" Rabbit Pk, Now Online, like to Join? ",
                    p => { p.Entity.Teleport(1002, 277, 360); }, null, 60);
            }
            #endregion
            #region RevengerWar
            if (DateTime.Now.Minute == 47 && Now64.Second == 1)
            {
                Kernel.SendWorldMessage(new Message(" ReVenger, Now Online All Go To Play PK,!", Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(" ReVengerWar Pk, Now Online, like to Join? ",
                    (p) => { p.Entity.Teleport(1002, 279, 360); }, null, 60);
            }
            #endregion
            #region Dead World
            if (DateTime.Now.Minute == 53 && Now64.Second == 1)
            {
                Kernel.SendWorldMessage(new Message(" Dead World Pk, Now Online All Go To Play PK!", Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox(" Dead World Pk, Now Online, like to Join? ",
                    p => { p.Entity.Teleport(1002, 282, 360); }, null, 60);
            }
            #endregion
            #region MemberAlter
            if (DateTime.Now.Minute == 57 && Now64.Second == 10)
            {
                Kernel.SendWorldMessage(new Message(" MemberAlter, Now Online All Go To Play PK,!", Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("War, MemberAlter, Now Online, like to Join? ",
                    p => { p.Entity.Teleport(1002, 285, 360); }, null, 60);
            }
            #endregion
            #region [T]KingDom.GLD
            if (Now64.Second <= 2 && Now64.DayOfWeek == DayOfWeek.Monday && Now64.Hour == 18 && Now64.Minute == 48)
            {
                Kernel.SendWorldMessage(new Message("((#42))War [T]KingDom.GLD, Now Online All Go To Play PK,((#41))--((#50))Monday, 18:48 To 18:55((#50)) !", Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("KingDom.GLD PK, Now Online, like to Join?",
                      p => { p.Entity.Teleport(1002, 255, 235); }, null, 60);
            }
            #endregion
            #region [T]KingDom.DLD
            if (Now64.Second <= 2 && Now64.DayOfWeek == DayOfWeek.Tuesday && Now64.Hour == 18 && Now64.Minute == 48)
            {
                Kernel.SendWorldMessage(new Message("((#42))War [T]KingDom.DLD, Now Online All Go To Play PK,((#41))--((#50))Tuesday, 18:48 To 18:55((#50)) !", Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("KingDom.DLD PK, Now Online, like to Join?",
                        p => { p.Entity.Teleport(1002, 255, 235); }, null, 60);
            }
            #endregion
            /////////////////////////////////////////////
            #region Mr/Ms Conquer
            //   if ()
            if (DateTime.Now.Hour == 19 && DateTime.Now.Minute == 31 && Now64.Second == 15)
            {
                Kernel.SendWorldMessage(new Message("Mr/Ms Conquer War began! Go Twin city ", Color.Red, Message.BroadcastMessage), Program.Values);
                foreach (var client in Program.Values)

                    client.MessageBox("Mr/Ms Conquer  began! Would you like to join Priz ?",
                        p => { p.Entity.Teleport(1002, 288, 192); }, null, 60);
            }
            #endregion
            #region Topguild
            if (Now64.Minute == 35 && Now64.Second == 10)
            {
                Kernel.SendWorldMessage(new Message("Hero Guild War began !", Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("Hero Guild began! Would you like to join ?",
                    p => { p.Entity.Teleport(1002, 313, 143); }, null, 60);
            }
            #endregion
            #region LastTeam Fight
            if (Now64.Minute == 13 && Now64.Second == 10)
            {
                Kernel.SendWorldMessage(new Message("Last Team Fight began !", Color.White, Message.Center), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("Last Team Fight began! Would you like to join ?",
                    p => { p.Entity.Teleport(1002, 289, 143); }, null, 60);
            }
            #endregion
            #region Team & SKill PK
            if (Matrix_Times.Start.TeamPK && !Game.Features.Tournaments.TeamElitePk.TeamTournament.Opened)
            {
                Kernel.SendWorldMessage(new Message("The Team PK Tournament has start at 19:00. Prepare yourself and sign up for it as a team!", Color.White, Message.BroadcastMessage), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("The Team PK Tournament began! Would you like to join Prize [100kk] First Rank?",
                    p => { p.Entity.Teleport(1002, 440, 249); }, null, 60);
            }
            if (Matrix_Times.Start.SkillTeam && !Game.Features.Tournaments.TeamElitePk.SkillTeamTournament.Opened)
            {
                Kernel.SendWorldMessage(new Message("The Skill Team PK Tournament will start at 10:00. Prepare yourself and sign up for it as a team!", Color.White, Message.BroadcastMessage), Program.Values);
                foreach (var client in Program.Values)
                    client.MessageBox("The Skill Team PK Tournament began! Would you like to join, Prize [100kk] First Rank?",
                    p => { p.Entity.Teleport(1002, 445, 242); }, null, 60);
            }
            #endregion
            #region GuildWar
            if (GuildWar.IsWar)
            {
                if (Time32.Now > GuildWar.ScoreSendStamp.AddSeconds(3))
                {
                    GuildWar.ScoreSendStamp = Time32.Now;
                    GuildWar.SendScores();
                }

            }
            if (Now64.Hour >= 20 && Now64.Hour <= 21 && Now64.DayOfWeek == DayOfWeek.Friday)
            {
                if (!GuildWar.IsWar)
                {
                    GuildWar.Start();
                    foreach (Client.GameState client in Kernel.GamePool.Values)
                    {
                        client.Entity.DeputyLeader = 0;
                        if (client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Entity.Dead)
                        {
                            EventAlert alert = new EventAlert
                            {
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
            if (GuildWar.IsWar)
            {
                if (Now64.Hour == 21 && Now64.Second <= 2)
                {
                    GuildWar.Flame10th = false;
                    GuildWar.End();
                }
            }
            #endregion

            #region SuperGuildWar
            if (SuperGuildWar.IsWar)
            {
                if (Time32.Now > SuperGuildWar.ScoreSendStamp.AddSeconds(3))
                {
                    SuperGuildWar.ScoreSendStamp = Time32.Now;
                    SuperGuildWar.SendScores();
                }
            }
            if (Now64.Hour >= 01 && Now64.Hour <= 20 && (Now64.Day == 1 || Now64.Day == 7 || Now64.Day == 14 || Now64.Day == 21))
            {
                if (!SuperGuildWar.IsWar)
                {
                    SuperGuildWar.Start();
                    foreach (var client in Program.Values)
                        if (client.Entity.GuildID != 0)
                            client.MessageBox("Super GuildWar has begun! Would you like to join?", p => { p.Entity.Teleport(1002, 352, 337); }, null);
                }
            }
            if (SuperGuildWar.IsWar)
            {
                if (Now64.Hour == 23 && Now64.Second <= 2)
                {
                    SuperGuildWar.End();
                }
            }
            #endregion

            #region Elite PK Tournament
            if ((Now64.Hour == ElitePK.EventTime) && Now64.Minute >= 55 && !ElitePKTournament.TimersRegistered)
            {
                ElitePK.EventTime = DateTime.Now.Hour;
                ElitePKTournament.RegisterTimers();
                ElitePKBrackets brackets = new ElitePKBrackets(true, 0);
                brackets.Type = ElitePKBrackets.EPK_State;
                brackets.OnGoing = true;
                foreach (var client in Program.Values)
                {
                    client.ClaimedElitePk = 0;
                    client.Send(brackets);
                    foreach (Client.GameState Client in Kernel.GamePool.Values)
                    {
                        if (client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Entity.Dead)
                        {
                            EventAlert alert = new EventAlert
                            {
                                StrResID = 10533,
                                Countdown = 60,
                                UK12 = 1
                            };
                            client.Entity.StrResID = 10533;
                            client.Send(alert);
                        }
                    }
                    #region RemoveTopElite
                    var EliteChampion = Network.GamePackets.TitlePacket.Titles.ElitePKChamption_High;
                    var EliteSecond = Network.GamePackets.TitlePacket.Titles.ElitePK2ndPlace_High;
                    var EliteThird = Network.GamePackets.TitlePacket.Titles.ElitePK3ndPlace_High;
                    var EliteEightChampion = Network.GamePackets.TitlePacket.Titles.ElitePKChamption_Low;
                    var EliteEightSecond = Network.GamePackets.TitlePacket.Titles.ElitePK2ndPlace_Low;
                    var EliteEightThird = Network.GamePackets.TitlePacket.Titles.ElitePK3ndPlace_Low;
                    var EliteEight = Network.GamePackets.TitlePacket.Titles.ElitePKTopEight_Low;
                    if (client.Entity.Titles.ContainsKey(EliteChampion))
                        client.Entity.RemoveTopStatus((ulong)EliteChampion);
                    if (client.Entity.Titles.ContainsKey(EliteSecond))
                        client.Entity.RemoveTopStatus((ulong)EliteSecond);
                    if (client.Entity.Titles.ContainsKey(EliteThird))
                        client.Entity.RemoveTopStatus((ulong)EliteThird);
                    if (client.Entity.Titles.ContainsKey(EliteEightChampion))
                        client.Entity.RemoveTopStatus((ulong)EliteEightChampion);
                    if (client.Entity.Titles.ContainsKey(EliteEightSecond))
                        client.Entity.RemoveTopStatus((ulong)EliteEightSecond);
                    if (client.Entity.Titles.ContainsKey(EliteEightThird))
                        client.Entity.RemoveTopStatus((ulong)EliteEightThird);
                    if (client.Entity.Titles.ContainsKey(EliteEight))
                        client.Entity.RemoveTopStatus((ulong)EliteEight);
                    #endregion
                }
            }
            if (Now64.Hour == ElitePK.EventTime + 1 && Now64.Minute >= 10 && ElitePKTournament.TimersRegistered)
            {
                bool done = true;
                foreach (var epk in ElitePKTournament.Tournaments)
                    if (epk.Players.Count != 0)
                        done = false;
                if (done)
                {
                    ElitePKTournament.TimersRegistered = false;
                    ElitePKBrackets brackets = new ElitePKBrackets(true, 0);
                    brackets.Type = ElitePKBrackets.EPK_State;
                    brackets.OnGoing = false;
                    foreach (var client in Program.Values)
                        client.Send(brackets);
                }
            }
            #endregion

        }
        private void ServerFunctions(int time)
        {
            DateTime LastPerfectionSort = DateTime.Now;
            if (DateTime.Now >= LastPerfectionSort.AddMinutes(10))
            {
                LastPerfectionSort = DateTime.Now;
                new PerfectionScore().GetRankingList();
                new PerfectionRank().UpdateRanking();
            }
            #region New weather
            MTA.Network.GamePackets.Weather weather;
            #region Rain System
            if (DateTime.Now.Minute == 10 && DateTime.Now.Second == 0 || DateTime.Now.Minute == 00 && DateTime.Now.Second == 00)
            {
                foreach (GameState state in Kernel.GamePool.Values)
                {
                    Program.WeatherType = MTA.Network.GamePackets.Weather.Snow;
                    weather = new MTA.Network.GamePackets.Weather(true)
                    {
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
            if (DateTime.Now.Minute == 20 && DateTime.Now.Second == 0 || DateTime.Now.Minute == 00 && DateTime.Now.Second == 00)
            {
                foreach (GameState state in Kernel.GamePool.Values)
                {
                    Program.WeatherType = MTA.Network.GamePackets.Weather.Snow;
                    weather = new MTA.Network.GamePackets.Weather(true)
                    {
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
            if (DateTime.Now.Minute == 30 && DateTime.Now.Second == 0 || DateTime.Now.Minute == 00 && DateTime.Now.Second == 00)
            {
                foreach (GameState state in Kernel.GamePool.Values)
                {
                    Program.WeatherType = MTA.Network.GamePackets.Weather.Snow;
                    weather = new MTA.Network.GamePackets.Weather(true)
                    {
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
            if (DateTime.Now.Minute == 40 && DateTime.Now.Second == 0 || DateTime.Now.Minute == 00 && DateTime.Now.Second == 00)
            {
                foreach (GameState state in Kernel.GamePool.Values)
                {
                    Program.WeatherType = MTA.Network.GamePackets.Weather.Snow;
                    weather = new MTA.Network.GamePackets.Weather(true)
                    {
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
            if (DateTime.Now.Minute == 60 && DateTime.Now.Second == 0 || DateTime.Now.Minute == 00 && DateTime.Now.Second == 00)
            {
                foreach (GameState state in Kernel.GamePool.Values)
                {
                    Program.WeatherType = MTA.Network.GamePackets.Weather.Snow;
                    weather = new MTA.Network.GamePackets.Weather(true)
                    {
                        WeatherType = (uint)Program.WeatherType,
                        Intensity = 255,
                        Appearence = 255,
                        Direction = 255
                    };
                    state.Send(weather);
                }
            }
            #endregion CherryBlossomPetals
            #endregion  New weather

            var kvpArray = Kernel.GamePool.ToArray();
            foreach (var kvp in kvpArray)
                if (kvp.Value == null || kvp.Value.Entity == null)
                    Kernel.GamePool.Remove(kvp.Key);
            Program.Values = Kernel.GamePool.Values.ToArray();

            Console.Title = Constants.ServerName + " - Online : " + Kernel.GamePool.Count + "/" + Program.PlayerCap;

            if (Kernel.GamePool.Count > Program.MaxOn)
            {
                Program.MaxOn = Kernel.GamePool.Count;
            }
            Console.Title = Constants.ServerName + " - Online : " + Kernel.GamePool.Count + "/" + Program.PlayerCap + " (Peak: " + Program.MaxOn + ")";
            new Database.MySqlCommand(Database.MySqlCommandType.UPDATE).Update("configuration").Set("GuildID", Game.ConquerStructures.Society.Guild.GuildCounter.Now).Set("MaxOnline", Program.MaxOn).Set("ItemUID", Program._NextItemID).Where("Server", Constants.ServerName).Execute();
            Database.EntityVariableTable.Save(0, Program.Vars);
            if (Kernel.BlackSpoted.Values.Count > 0)
            {
                foreach (var spot in Kernel.BlackSpoted.Values)
                {
                    if (Time32.Now >= spot.BlackSpotStamp.AddSeconds(spot.BlackSpotStepSecs))
                    {
                        if (spot.Dead && spot.EntityFlag == EntityFlag.Player)
                        {
                            foreach (var h in Program.Values)
                            {
                                h.Send(Program.BlackSpotPacket.ToArray(false, spot.UID));
                            }
                            Kernel.BlackSpoted.Remove(spot.UID);
                            continue;
                        }
                        foreach (var h in Program.Values)
                        {
                            h.Send(Program.BlackSpotPacket.ToArray(false, spot.UID));
                        }
                        spot.IsBlackSpotted = false;
                        Kernel.BlackSpoted.Remove(spot.UID);
                    }
                }
            }
            DateTime Now = DateTime.Now;

            if (Now > Game.ConquerStructures.Broadcast.LastBroadcast.AddMinutes(1))
            {
                if (Game.ConquerStructures.Broadcast.Broadcasts.Count > 0)
                {
                    Game.ConquerStructures.Broadcast.CurrentBroadcast = Game.ConquerStructures.Broadcast.Broadcasts[0];
                    Game.ConquerStructures.Broadcast.Broadcasts.Remove(Game.ConquerStructures.Broadcast.CurrentBroadcast);
                    Game.ConquerStructures.Broadcast.LastBroadcast = Now;
                    Kernel.SendWorldMessage(new Network.GamePackets.Message(Game.ConquerStructures.Broadcast.CurrentBroadcast.Message, "ALLUSERS", Game.ConquerStructures.Broadcast.CurrentBroadcast.EntityName, System.Drawing.Color.Red, Network.GamePackets.Message.BroadcastMessage), Program.Values);
                }
                else
                    Game.ConquerStructures.Broadcast.CurrentBroadcast.EntityID = 1;
            }


            if (Now > Program.LastRandomReset.AddMinutes(30))
            {
                Program.LastRandomReset = Now;
                Kernel.Random = new FastRandom(Program.RandomSeed);
            }
            Program.Today = Now.DayOfWeek;
        }
        private void ArenaFunctions(int time)
        {
            Game.Arena.EngagePlayers();
            Game.Arena.CheckGroups();
            Game.Arena.VerifyAwaitingPeople();
            Game.Arena.Reset();
        }
        private void TeamArenaFunctions(int time)
        {
            Game.TeamArena.PickUpTeams();
            Game.TeamArena.EngagePlayers();
            Game.TeamArena.CheckGroups();
            Game.TeamArena.VerifyAwaitingPeople();
            Game.TeamArena.Reset();
        }
        private void ChampionFunctions(int time)
        {
            Game.Champion.EngagePlayers();
            Game.Champion.CheckGroups();
            Game.Champion.VerifyAwaitingPeople();
            Game.Champion.Reset();
        }

        #region Funcs
        public static void Execute(Action<int> action, int timeOut = 0, ThreadPriority priority = ThreadPriority.Normal)
        {
            GenericThreadPool.Subscribe(new LazyDelegate(action, timeOut, priority));
        }
        public static void Execute<T>(Action<T, int> action, T param, int timeOut = 0, ThreadPriority priority = ThreadPriority.Normal)
        {
            GenericThreadPool.Subscribe<T>(new LazyDelegate<T>(action, timeOut, priority), param);
        }
        public static IDisposable Subscribe(Action<int> action, int period = 1, ThreadPriority priority = ThreadPriority.Normal)
        {
            return GenericThreadPool.Subscribe(new TimerRule(action, period, priority));
        }
        public static IDisposable Subscribe<T>(Action<T, int> action, T param, int timeOut = 0, ThreadPriority priority = ThreadPriority.Normal)
        {
            return GenericThreadPool.Subscribe<T>(new TimerRule<T>(action, timeOut, priority), param);
        }
        public static IDisposable Subscribe<T>(TimerRule<T> rule, T param, StandalonePool pool)
        {
            return pool.Subscribe<T>(rule, param);
        }
        public static IDisposable Subscribe<T>(TimerRule<T> rule, T param, StaticPool pool)
        {
            return pool.Subscribe<T>(rule, param);
        }
        public static IDisposable Subscribe<T>(TimerRule<T> rule, T param)
        {
            return GenericThreadPool.Subscribe<T>(rule, param);
        }
        #endregion

        internal void SendServerMessaj(string p)
        {
            Kernel.SendWorldMessage(new Message(p, System.Drawing.Color.Red, Message.TopLeft), Program.Values);
        }
    }
}
