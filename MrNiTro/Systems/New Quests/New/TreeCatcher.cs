using Conquer_Online_Server.Client;
using Conquer_Online_Server.Game;
using Conquer_Online_Server.Interfaces;
using Conquer_Online_Server.Network.GamePackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Conquer_Online_Server.MaTrix.New_Quests
{
    public class TreeCatcher
    {
        public static bool IsON = false;
        private IDisposable Subscriber;
        public static Map Map;
        public static INpc Npc;
        public bool Move;
        private static List<ushort> Maps = new List<ushort>()
        {
            (ushort)Enums.Maps.TwinCity,
            (ushort)Enums.Maps.ApeMoutain,
            (ushort)Enums.Maps.PhoenixCastle,
            (ushort)Enums.Maps.DesertCity,
            (ushort)Enums.Maps.BirdIsland,
            (ushort)Enums.Maps.Market
        };
        public TreeCatcher()
        {
            Create();
            Subscriber = World.Subscribe(work, 1000);           
        }
        public void Create()
        {
            IsON = true;
            Map = Kernel.Maps[(ushort)Enums.Maps.TwinCity];
            Npc = new NpcSpawn();
            Npc.UID = Map.EntityUIDCounter2.Next;
            Npc.MapID = (ushort)Enums.Maps.TwinCity;
            var cord = Map.RandomCoordinates();
            Npc.X = cord.Item1;
            Npc.Y = cord.Item2;
            Npc.Type = Enums.NpcType.Talker;
            Npc.Mesh = (ushort)(170 + Kernel.Random.Next(9));
            Map.Npcs.Add(Npc.UID, Npc);
            Console.WriteLine(string.Format("Tree Location : Map {0}, X {1}, Y{2}", ((Enums.Maps)Npc.MapID).ToString(), Npc.X, Npc.Y));   
        }
        public void work(int time)
        {
            var cord = Map.RandomCoordinates();
            if (IsON)
            {
                if (Npc != null && Map != null)
                {                   
                    if (Program.Values.Length > 0)
                    {
                        if (Move)
                            return;  
                      if (Npc.X != cord.Item1 && Npc.Y != cord.Item2)
                      {                                                
                          TryPathFinding(cord);
                         // Kernel.SendWorldMessage(new Message(string.Format("Tree Going To Location : Map {0}, X {1}, Y{2}", ((Enums.Maps)Npc.MapID).ToString(), Npc.X, Npc.Y), System.Drawing.Color.Red, Message.TopLeft), Program.Values);
                      }
                      else
                      {  
                          cord = Map.RandomCoordinates();
                          TryPathFinding(cord);
                         // Kernel.SendWorldMessage(new Message(string.Format("Tree Going To Location : Map {0}, X {1}, Y{2}", ((Enums.Maps)Npc.MapID).ToString(), Npc.X, Npc.Y), System.Drawing.Color.Red, Message.TopLeft), Program.Values);
     
                      }
                    }                       
                }
            }  
        }
        private void TryPathFinding(Tuple<ushort, ushort> cord)
        {
            new Thread(() =>
            {
                Move = true;
                var path = Pathfinding.AStar.Calculate.FindWay(cord.Item1, cord.Item2, Npc.X, Npc.Y, Map);
                if (path != null)
                {
                    for (int i = 5; i < path.Count; i += 5)
                    {
                        TwoMovements jump = new TwoMovements();
                        jump.X = (ushort)path[i].X;
                        jump.Y = (ushort)path[i].Y;
                        jump.EntityCount = 1;
                        jump.FirstEntity = Npc.UID;
                        jump.MovementType = TwoMovements.Jump;
                        Kernel.SendWorldMessage(jump, Program.Values, Map.ID);
                        Npc.X = jump.X;
                        Npc.Y = jump.Y;
                  //      Kernel.SendWorldMessage(new Message(string.Format("Tree at Location : Map {0}, X {1}, Y{2}", ((Enums.Maps)Npc.MapID).ToString(), Npc.X, Npc.Y), System.Drawing.Color.Red, Message.TopLeft), Program.Values);     
                        System.Threading.Thread.Sleep(10000);
                       
                    }
                    TwoMovements jump2 = new TwoMovements();
                    jump2.X = (ushort)path[path.Count - 1].X;
                    jump2.Y = (ushort)path[path.Count - 1].Y;
                    jump2.EntityCount = 1;
                    jump2.FirstEntity = Npc.UID;
                    jump2.MovementType = TwoMovements.Jump;
                    Kernel.SendWorldMessage(jump2, Program.Values, Map.ID);
                    Npc.X = jump2.X;
                    Npc.Y = jump2.Y;
                  //  Kernel.SendWorldMessage(new Message(string.Format("Tree at Location : Map {0}, X {1}, Y{2}", ((Enums.Maps)Npc.MapID).ToString(), Npc.X, Npc.Y), System.Drawing.Color.Red, Message.TopLeft), Program.Values);     
                    Move = false;
                    Thread.CurrentThread.Abort();

                }
            }).Start();
        }
        public static  void Catched(GameState client)
        {
            ReSpwan();
            client.Entity.ConquerPoints += rates.Shit;
            Kernel.SendWorldMessage(new Message("Player "+client.Entity.Name+" Have Catched the Tree and claimed 50k cps", System.Drawing.Color.Red, Message.TopLeft), Program.Values);
            client.Screen.FullWipe();
            client.Screen.Reload(null);
        }
        public static void ReSpwan()
        {
            Map.Npcs.Remove(Npc.UID);
            var mapid = Maps[Kernel.Random.Next(Maps.Count)];
            Map = Kernel.Maps[mapid];
            Npc.MapID = mapid;
            var cord = Map.RandomCoordinates();
            Npc.UID = Map.EntityUIDCounter2.Next;
            Npc.X = cord.Item1;
            Npc.Y = cord.Item2;
            Map.Npcs.Add(Npc.UID, Npc);
            Kernel.SendWorldMessage(new Message(string.Format("Tree Respwan at Location : Map {0}, X {1}, Y{2}", ((Enums.Maps)Npc.MapID).ToString(), Npc.X, Npc.Y), System.Drawing.Color.Red, Message.TopLeft), Program.Values);
        }
    }
}
