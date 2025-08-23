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

namespace MTA.Game
{
    public class GuildScoreWar
    {
        public static SobNpcSpawn Pole;
        public static DateTime WarStart;
        public static Map Map;
        public static bool IsWar = false;
        private IDisposable Subscriber;
        public static uint Winner; 

        public GuildScoreWar()
        {
            Map = Kernel.Maps[1002].MakeDynamicMap();
            Subscriber = World.Subscribe(work, 1000);

        }
        public void work(int time)
        {
            if (IsWar)
                if (DateTime.Now > WarStart.AddMinutes(60))
                    EndWar();

            
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
                if (client.AsMember == null)
                    return;
                var cooord = Map.RandomCoordinates();
                client.Entity.Teleport(Map.ID, cooord.Item1, cooord.Item2);
                              
                client.Send(Pole);                
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
                Map.Npcs.Clear();
                #region StatuesPole
                Pole = new Network.GamePackets.SobNpcSpawn();
                Pole.UID = Map.EntityUIDCounter2.Next;
                Pole.Mesh = 1137;
                Pole.Type = (Enums.NpcType)10;
                Pole.X = 310;
                Pole.Y = 277;
                Pole.ShowName = true;
                Pole.Sort = 17;
                Pole.Hitpoints = 20000000;
                Pole.MaxHitpoints = 20000000;
                Pole.Name = "GuildScoreWar";
                Pole.MapID = Map.ID;
                #endregion               
                Map.AddNpc(Pole);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Program.SaveException(ex);
            }
        }
        public static void EndWar()
        {
            Guild PoleKeeper;
            SortScores(out PoleKeeper);
            if (PoleKeeper != null)
            {
                Kernel.SendWorldMessage(new Message(PoleKeeper.Name + " GuildScoreWar End !", System.Drawing.Color.White, Message.Center), Program.Values);
                Winner = PoleKeeper.ID;
            }
            else
                Kernel.SendWorldMessage(new Message("No Winner at GuildScoreWar and ended!", System.Drawing.Color.Red, Message.Center), Program.Values);
            IsWar = false;
        }
               
        #region Score
        public static SafeDictionary<uint, Guild> Scores = new SafeDictionary<uint, Guild>(100);
       
        public static Time32 ScoreSendStamp;       
        public static void Start()
        {
            Scores = new SafeDictionary<uint, Guild>();
            WarStart = DateTime.Now; 
            IsWar = true;
        }

        public static void AddScore(uint addScore, Guild Guild)
        {
            if (Guild != null)
            {
                Guild.GuildScoreWar += addScore;              
                if (!Scores.ContainsKey(Guild.ID))
                    Scores.Add(Guild.ID, Guild);               
            }
        }

        public static void SendScores()
        {
            if (Scores == null)
                return;
            if (Scores.Count == 0)
                return;

            var scoreMessages = SortScores();
            if (scoreMessages == null)
                return;
            for (int c = 0; c < scoreMessages.Length; c++)
            {
                Message msg = new Message(scoreMessages[c], System.Drawing.Color.Red, c == 0 ? Message.FirstRightCorner : Message.ContinueRightCorner);
                Kernel.SendWorldMessage(msg, Program.Values, Map.ID);
            }

        }
        private static string[] SortScores()
        {

            List<string> ret = new List<string>();
            int Place = 0;

            foreach (var e in Scores.Values.OrderByDescending((p) => p.GuildScoreWar))
            {
                string str = "No  " + (Place + 1).ToString() + ": " + e.Name + "(" + e.GuildScoreWar + ")";
                ret.Add(str);
                Place++;
                if (Place == 4)
                    break;
            }

            return ret.ToArray();
        }
        private static string[] SortScores(out Guild winner)
        {
            winner = null;
            List<string> ret = new List<string>();
            int Place = 0;


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
    
        public static void GetReward(GameState client, uint cps)
        {
            if (client.Entity.GuildRank == (ushort)GuildRank.GuildLeader)
            {
                if (client.Entity.GuildID == Winner)
                {
                    client.Entity.ConquerPoints += 2000000;
                    Winner = 0;
                    MTA.Kernel.SendWorldMessage(new Message("Congratulations! " + client.Entity.Name + " The winner GuildScoreWar Prize [" + cps.ToString() + " ] cps + 100 KPT!", System.Drawing.Color.White, Message.Center), Program.Values);
                    Program.AddWarLog("GuildScoreWar", cps.ToString(), client.Entity.Name);
                }
            }
        }
    }
}
