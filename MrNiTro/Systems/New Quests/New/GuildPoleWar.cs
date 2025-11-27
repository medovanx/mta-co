using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Client;
using MTA.Interfaces;
using MTA.Network.GamePackets;
using System.Drawing;
using MTA.Network;
using MTA.Game.ConquerStructures;
using System.Threading.Generic;
using MTA.Game.ConquerStructures.Society;
using MTA.Game;

namespace MTA.MaTrix
{
    public class GuildPoleWar
    {
        public const int Member = 0,
            // Knight = 1,
            // Baron = 3,
        //Earl = 5,
        //Member = 7,
        DeputyLeader = 1,
        GuildLeader = 2;

        public static SobNpcSpawn[] Poles = new SobNpcSpawn[3];
        public static uint[] PolesWinners = new uint[3];
        public static SafeDictionary<uint, Entity>[] AllScores = new SafeDictionary<uint, Entity>[3];

        public static DateTime WarStart;
        public static Map Map;
        public static bool IsWar = false;      
        private IDisposable Subscriber;

        public GuildPoleWar()
        {
            Map = Kernel.Maps[1002].MakeDynamicMap();
            Subscriber = World.Subscribe(work, 1000);

        }
        public void work(int time)
        {
            if (IsWar)
                if (DateTime.Now > WarStart.AddMinutes(60))
                    EndWar();;
            if (IsWar)
            {
                if (Time32.Now > ScoreSendStamp.AddSeconds(3))
                {
                    ScoreSendStamp = Time32.Now;
                    SendScores();
                }
            }
        }
        public static void Join(Client.GameState client)
        {
            if (IsWar)
            {
                var cooord = Map.RandomCoordinates();
                client.Entity.Teleport(Map.ID, cooord.Item1, cooord.Item2);

                client.OnDisconnect = p =>
                {
                    p.Entity.Teleport(1002, 301, 266);
                    client.Entity.PKMode = Game.Enums.PkMode.PK;
                    client.Send(new Data(true) { UID = client.Entity.UID, ID = Data.ChangePKMode, dwParam = (uint)client.Entity.PKMode });
                };
                client.Entity.OnDeath = p =>
                {
                    if (p.MapID == Map.ID)
                    {
                        var cooordx = Map.RandomCoordinates();
                        p.Teleport(Map.ID, cooordx.Item1, cooordx.Item2);
                        p.BringToLife();
                    }
                };
            }
        }

        public static void StartWar()
        {
            try
            {
                if (IsWar)
                    EndWar();
                IsWar = true;
                WarStart = DateTime.Now;
                if (!Constants.PKFreeMaps.Contains(Map.ID))
                    Constants.PKFreeMaps.Add(Map.ID);
                Poles = new SobNpcSpawn[3];
                PolesWinners = new uint[3];

                Map.Statues.Clear();
                Map.Npcs.Clear();

                Start();


                Summon("GuildLeader", 300, 288, GuildLeader);
                Summon("DeputyLeader", 300, 268, DeputyLeader);
                //Summon("Duke", 320, 288, Member);
                //Summon("Earl", 320, 268, Earl);
                Summon("Member", 310, 277, Member);
  


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Program.SaveException(ex);
            }
        }

        public static bool Attack(uint addScore, Entity entity, SobNpcSpawn Pole)
        {
            if (!IsWar)
                return false;
            int _rank = (int)Member;
            if (entity.GuildRank == (ushort)GuildRank.GuildLeader)
                _rank = GuildLeader;
            else if (entity.GuildRank == (ushort)GuildRank.DeputyLeader)
                _rank = DeputyLeader;
            var pole = Poles[_rank];
            if (pole != null)
            {
                if (pole != Pole)
                     return false;

                if (PolesWinners[_rank] == entity.UID)
                    return false;

                if (addScore > 0)
                    AddScore(addScore, entity, pole);

                return true;
            }
            return false;
        }

        public static void Summon(string Name, ushort X, ushort Y, int rank)
        {

            SobNpcSpawn Base = new SobNpcSpawn();
            Base.UID = Map.EntityUIDCounter2.Next;
            Base.Mesh = 1137;
            Base.Type = (Enums.NpcType)10;
            Base.Sort = 21;
            Base.ShowName = true;
            Base.Name = Name;
            Base.Hitpoints = Base.MaxHitpoints = 20000000;
            Base.MapID = Map.ID;
            Base.X = X;
            Base.Y = Y;
            Poles[rank] = Base;
            if (!Map.Npcs.ContainsKey(Base.UID))
                Map.Npcs.Add(Base.UID, Base);
        }

        #region Score

        public static bool FirstRound = false;
        public static Time32 ScoreSendStamp, LastWin;

        public static void Start()
        {
            AllScores = new SafeDictionary<uint, Entity>[3];
            WarStart = DateTime.Now;
            FirstRound = true;
            foreach (var c in Program.Values)
                c.Entity.WarScore = 0;
            IsWar = true;
        }

        public static void Reset(int _Class)
        {
            var rank = GuildRank.Member;
            if (_Class == DeputyLeader)
                rank = GuildRank.DeputyLeader;
            if (_Class == GuildLeader)
                rank = GuildRank.GuildLeader;            
                foreach (var c in Program.Values.Where(p => p.Entity.GuildRank == (ushort)rank))
                c.Entity.WarScore = 0;
            IsWar = true;
        }

        public static void FinishRound(SobNpcSpawn Pole, int _Class)
        {
            Entity PoleKeeper;
            LastWin = Time32.Now;
            FirstRound = false;
            SortScores(out PoleKeeper, _Class);
            if (PoleKeeper != null)
            {
                Kernel.SendWorldMessage(new Message("" + PoleKeeper.Name + " has won this round [GuildPoleWar]!", System.Drawing.Color.Red, Message.Center), Program.Values);
                Pole.Name = PoleKeeper.Name;
                PolesWinners[_Class] = PoleKeeper.UID;
            }
            Pole.Hitpoints = Pole.MaxHitpoints;
            foreach (var c in Program.Values)
            {
                if (c.Map.ID == Map.ID)
                    Pole.SendSpawn(c);
            }
            Reset(_Class);
        }

        public static void AddScore(uint addScore, Entity entity, SobNpcSpawn Pole)
        {
            if (entity != null)
            {
                int _rank = (int)Member;
                if (entity.GuildRank == (ushort)GuildRank.GuildLeader)
                    _rank = GuildLeader;
                else if (entity.GuildRank == (ushort)GuildRank.DeputyLeader)
                    _rank = DeputyLeader;

                if (Pole.Hitpoints <= addScore)
                    Pole.Hitpoints = 0;
                var Scores = AllScores[(ushort)((int)_rank)];
                if (Scores == null)
                    Scores = AllScores[(ushort)((int)_rank)] = new SafeDictionary<uint, Entity>();

                if (!Scores.ContainsKey(entity.UID))
                    Scores.Add(entity.UID, entity);

                entity.WarScore += addScore;
               
                if ((int)Pole.Hitpoints <= 0)
                {
                    FinishRound(Pole, (int)_rank);
                    return;
                }
            }
        }

        public static void SendScores()
        {
            for (int i = 0; i < AllScores.Length; i++)
            {
                var Scores = AllScores[i];

                if (Scores == null)
                    continue;
                if (Scores.Count == 0)
                    continue;

                Entity CurrentTopLeader;

                var scoreMessages = SortScores(out CurrentTopLeader, i);
                if (scoreMessages == null)
                    continue;
                for (int c = 0; c < scoreMessages.Length; c++)
                {
                    Message msg = new Message(scoreMessages[c], System.Drawing.Color.Red, c == 0 ? Message.FirstRightCorner : Message.ContinueRightCorner);
                    foreach (Client.GameState client in Program.Values)
                    {
                        if (client != null)
                        {
                            if (client.Map.ID == Map.ID)
                            {                              
                                if (i == DeputyLeader)
                                {
                                    if (client.Entity.GuildRank == (ushort)GuildRank.DeputyLeader)
                                        client.Send(msg);
                                }
                                else if (i == GuildLeader)
                                {
                                    if (client.Entity.GuildRank == (ushort)GuildRank.GuildLeader)
                                        client.Send(msg);
                                }
                                else
                                {
                                     if (client.Entity.GuildRank != (ushort)GuildRank.GuildLeader && client.Entity.GuildRank != (ushort)GuildRank.DeputyLeader)
                                         client.Send(msg);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static string[] SortScores(out Entity winner, int i)
        {
            winner = null;
            List<string> ret = new List<string>();
            int Place = 0;

            var Scores = AllScores[i];
            if (Scores == null) return new string[0];
            foreach (var e in Scores.Values.OrderByDescending((p) => p.WarScore))
            {
                if (Place == 0)
                    winner = e;
                string str = "No  " + (Place + 1).ToString() + ": " + e.Name + "(" + e.WarScore + ")";
                ret.Add(str);
                Place++;
                if (Place == 4)
                    break;
            }

            return ret.ToArray();
        }
        #endregion

        public static void EndWar()
        {
            Kernel.SendWorldMessage(new Message("GuildPole war has ended.!", System.Drawing.Color.Red, Message.Center), Program.Values);
            IsWar = false;
            Map.Npcs.Clear();
            foreach (var client in Program.Values)
                if (client.Entity.MapID == Map.ID)
                    client.Entity.Teleport(1002, 301, 266);
        }
        public static void GetReward(GameState c, uint cps)
        {
            for (int i = 0; i < PolesWinners.Length; i++)
            {
                var winner = PolesWinners[i];
                if (c.Entity.UID == winner)
                {
                    c.Entity.ConquerPoints += 1000000;
                    PolesWinners[i] = 0;
                    MTA.Kernel.SendWorldMessage(new Message("Congratulations! " + c.Entity.Name + " The winner GuildPoleWar Prize [" + cps.ToString() + " ] cps + 100 KPT!", System.Drawing.Color.White, Message.Center), Program.Values);
                    Program.AddWarLog("GuildPoleWar", cps.ToString(), c.Entity.Name);
                }
            }
        }
        //public static void GetReward(GameState c, uint cps)
        //{
        //    for (int i = 0; i < PolesWinners.Length; i++)
        //    {
        //        var winner = PolesWinners[i];


        //        if (c.Entity.UID == winner)
        //        {
        //            uint cpss = 0;
        //            if (i == 0)
        //                cpss = cps;
        //            else
        //                cpss += cps / 2;

        //            c.Entity.ConquerPoints += cpss;
        //            PolesWinners[i] = 0;
        //            MTA.Kernel.SendWorldMessage(new Message("Congratulations! " + c.Entity.Name + " The winner GuildPoleWar Prize [ " + cpss.ToString() + " ] cps + 50 KPT!", System.Drawing.Color.White, Message.Center), Program.Values);
        //            Program.AddWarLog("GuildPoleWar", cpss.ToString(), c.Entity.Name);
        //        }
        //    }
        //}
    }
}
