using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Network.GamePackets;
using MTA.Interfaces;

namespace MTA.Game
{
    class CPsEvent
    {
        public static ushort X = 300, Y = 278;
        public static ushort X1 = 300, Y1 = 288;
        public static ushort X2 = 311, Y2 = 288;
        public static ushort X3 = 321, Y3 = 288;
        public static ushort X4 = 321, Y4 = 278;
        public static ushort X5 = 321, Y5 = 267;
        public static ushort X6 = 311, Y6 = 267;
        public static ushort X7 = 310, Y7 = 256;
        public static ushort X8 = 301, Y8 = 256;
        public static ushort X9 = 301, Y9 = 245;
        public static ushort X10 = 301, Y10 = 233;
        public static ushort X11 = 299, Y11 = 267;
        public static ushort X12 = 298, Y12 = 206;
        public static ushort X13 = 300, Y13 = 196;
        public static ushort X14 = 301, Y14 = 182;
        public static ushort X15 = 301, Y15 = 168;
        public static ushort X16 = 300, Y16 = 153;
        public static void Load()
        {
            if (Kernel.Maps.ContainsKey(1002))
            {
                uint ItemID = 720159;
                #region CPBag

                INpc npc = new Network.GamePackets.NpcSpawn();
                npc.UID = 1305;
                npc.Mesh = 13050;
                npc.Type = Enums.NpcType.Talker;
                npc.X = (ushort)(X - 1);
                npc.Y = (ushort)(Y - 1);
                npc.MapID = 1002;


                Game.Map Map = Kernel.Maps[1002];
                if (Map.SelectCoordonates(ref X, ref Y))
                {
                    Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.ConquerPoints;
                    floorItem.Value = 250000;
                    floorItem.ItemID = ItemID;
                    floorItem.MapID = 1002;
                    floorItem.MapObjType = Game.MapObjectType.Item;
                    floorItem.X = X;
                    floorItem.Y = Y;
                    floorItem.Type = Network.GamePackets.FloorItem.Drop;
                    floorItem.OnFloor = Time32.Now;
                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    while (Map.Npcs.ContainsKey(floorItem.UID))
                    {
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    }
                    Map.AddFloorItem(floorItem);
                    foreach (Client.GameState C in Kernel.GamePool.Values)
                    {
                        if (C.Entity.MapID == 1002)
                        {
                            C.SendScreenSpawn(floorItem, true);
                            npc.SendSpawn(C);

                            C.Entity.Update(MTA.Network.GamePackets._String.Effect, "other", true);
                        }
                    }
                }
                #endregion
            }
        }
        public static void Load2()
        {
            if (Kernel.Maps.ContainsKey(1002))
            {
                #region CPBag

                uint ItemID = 720159;
                ushort X = X1, Y = Y1;
                INpc npc = new Network.GamePackets.NpcSpawn();
                npc.UID = 1305;
                npc.Mesh = 13050;
                npc.Type = Enums.NpcType.Talker;
                npc.X = (ushort)(X - 1);
                npc.Y = (ushort)(Y - 1);
                npc.MapID = 1002;


                Game.Map Map = Kernel.Maps[1002];
                if (Map.SelectCoordonates(ref X, ref Y))
                {
                    Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.ConquerPoints;
                    floorItem.Value = 250000;
                    floorItem.ItemID = ItemID;
                    floorItem.MapID = 1002;
                    floorItem.MapObjType = Game.MapObjectType.Item;
                    floorItem.X = X;
                    floorItem.Y = Y;
                    floorItem.Type = Network.GamePackets.FloorItem.Drop;
                    floorItem.OnFloor = Time32.Now;
                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    while (Map.Npcs.ContainsKey(floorItem.UID))
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    Map.AddFloorItem(floorItem);
                    foreach (Client.GameState C in Kernel.GamePool.Values)
                    {
                        if (C.Entity.MapID == 1002)
                        {
                            C.SendScreenSpawn(floorItem, true);
                            npc.SendSpawn(C);

                            C.Entity.Update(MTA.Network.GamePackets._String.Effect, "other", true);
                        }
                    }
                #endregion

                }
            }
        }
        public static void Load3()
        {
            if (Kernel.Maps.ContainsKey(1002))
            {
                #region CPBag

                uint ItemID = 720159;
                ushort X = X2, Y = Y2;
                INpc npc = new Network.GamePackets.NpcSpawn();
                npc.UID = 1305;
                npc.Mesh = 13050;
                npc.Type = Enums.NpcType.Talker;
                npc.X = (ushort)(X - 1);
                npc.Y = (ushort)(Y - 1);
                npc.MapID = 1002;

                Game.Map Map = Kernel.Maps[1002];
                if (Map.SelectCoordonates(ref X, ref Y))
                {
                    Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.ConquerPoints;
                    floorItem.Value = 250000;
                    floorItem.ItemID = ItemID;
                    floorItem.MapID = 1002;
                    floorItem.MapObjType = Game.MapObjectType.Item;
                    floorItem.X = X;
                    floorItem.Y = Y;
                    floorItem.Type = Network.GamePackets.FloorItem.Drop;
                    floorItem.OnFloor = Time32.Now;
                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    while (Map.Npcs.ContainsKey(floorItem.UID))
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    Map.AddFloorItem(floorItem);
                    foreach (Client.GameState C in Kernel.GamePool.Values)
                    {
                        if (C.Entity.MapID == 1002)
                        {
                            C.SendScreenSpawn(floorItem, true);
                            npc.SendSpawn(C);
                            C.Entity.Update(MTA.Network.GamePackets._String.Effect, "other", true);
                        }
                    }

                #endregion
                }
            }
        }
        public static void Load4()
        {
            if (Kernel.Maps.ContainsKey(1002))
            {
                #region CPBag

                uint ItemID = 720159;
                ushort X = X3, Y = Y3;
                INpc npc = new Network.GamePackets.NpcSpawn();
                npc.UID = 1305;
                npc.Mesh = 13050;
                npc.Type = Enums.NpcType.Talker;
                npc.X = (ushort)(X - 1);
                npc.Y = (ushort)(Y - 1);
                npc.MapID = 1002;

                Game.Map Map = Kernel.Maps[1002];
                if (Map.SelectCoordonates(ref X, ref Y))
                {
                    Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.ConquerPoints;
                    floorItem.Value = 250000;
                    floorItem.ItemID = ItemID;
                    floorItem.MapID = 1002;
                    floorItem.MapObjType = Game.MapObjectType.Item;
                    floorItem.X = X;
                    floorItem.Y = Y;
                    floorItem.Type = Network.GamePackets.FloorItem.Drop;
                    floorItem.OnFloor = Time32.Now;
                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    while (Map.Npcs.ContainsKey(floorItem.UID))
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    Map.AddFloorItem(floorItem);
                    foreach (Client.GameState C in Kernel.GamePool.Values)
                    {
                        if (C.Entity.MapID == 1002)
                        {
                            C.SendScreenSpawn(floorItem, true);
                            npc.SendSpawn(C);
                            C.Entity.Update(MTA.Network.GamePackets._String.Effect, "other", true);
                        }
                    }
                #endregion
                    Load5();
                }
            }
        }
        public static void Load5()
        {
            if (Kernel.Maps.ContainsKey(1002))
            {
                #region CPBag

                uint ItemID = 720159;
                ushort X = X4, Y = Y4;
                INpc npc = new Network.GamePackets.NpcSpawn();
                npc.UID = 1305;
                npc.Mesh = 13050;
                npc.Type = Enums.NpcType.Talker;
                npc.X = (ushort)(X - 1);
                npc.Y = (ushort)(Y - 1);
                npc.MapID = 1002;

                Game.Map Map = Kernel.Maps[1002];
                if (Map.SelectCoordonates(ref X, ref Y))
                {
                    Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.ConquerPoints;
                    floorItem.Value = 250000;
                    floorItem.ItemID = ItemID;
                    floorItem.MapID = 1002;
                    floorItem.MapObjType = Game.MapObjectType.Item;
                    floorItem.X = X;
                    floorItem.Y = Y;
                    floorItem.Type = Network.GamePackets.FloorItem.Drop;
                    floorItem.OnFloor = Time32.Now;
                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    while (Map.Npcs.ContainsKey(floorItem.UID))
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    Map.AddFloorItem(floorItem);
                    foreach (Client.GameState C in Kernel.GamePool.Values)
                    {
                        if (C.Entity.MapID == 1002)
                        {
                            C.SendScreenSpawn(floorItem, true);
                            npc.SendSpawn(C);
                            C.Entity.Update(MTA.Network.GamePackets._String.Effect, "other", true);
                        }
                    }
                #endregion
                }
            }
        }
        public static void Load6()
        {
            if (Kernel.Maps.ContainsKey(1002))
            {
                #region CPBag

                uint ItemID = 720159;
                ushort X = X5, Y = Y5;
                INpc npc = new Network.GamePackets.NpcSpawn();
                npc.UID = 1305;
                npc.Mesh = 13050;
                npc.Type = Enums.NpcType.Talker;
                npc.X = (ushort)(X - 1);
                npc.Y = (ushort)(Y - 1);
                npc.MapID = 1002;

                Game.Map Map = Kernel.Maps[1002];
                if (Map.SelectCoordonates(ref X, ref Y))
                {
                    Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.ConquerPoints;
                    floorItem.Value = 250000;
                    floorItem.ItemID = ItemID;
                    floorItem.MapID = 1002;
                    floorItem.MapObjType = Game.MapObjectType.Item;
                    floorItem.X = X;
                    floorItem.Y = Y;
                    floorItem.Type = Network.GamePackets.FloorItem.Drop;
                    floorItem.OnFloor = Time32.Now;
                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    while (Map.Npcs.ContainsKey(floorItem.UID))
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    Map.AddFloorItem(floorItem);
                    foreach (Client.GameState C in Kernel.GamePool.Values)
                    {
                        if (C.Entity.MapID == 1002)
                        {
                            C.SendScreenSpawn(floorItem, true);
                            npc.SendSpawn(C);
                            C.Entity.Update(MTA.Network.GamePackets._String.Effect, "other", true);
                        }
                    }
                #endregion
                }
            }
        }
        public static void Load7()
        {
            if (Kernel.Maps.ContainsKey(1002))
            {
                #region CPBag

                uint ItemID = 720159;
                ushort X = X6, Y = Y6;
                INpc npc = new Network.GamePackets.NpcSpawn();
                npc.UID = 1305;
                npc.Mesh = 13050;
                npc.Type = Enums.NpcType.Talker;
                npc.X = (ushort)(X - 1);
                npc.Y = (ushort)(Y - 1);
                npc.MapID = 1002;

                Game.Map Map = Kernel.Maps[1002];
                if (Map.SelectCoordonates(ref X, ref Y))
                {
                    Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.ConquerPoints;
                    floorItem.Value = 250000;
                    floorItem.ItemID = ItemID;
                    floorItem.MapID = 1002;
                    floorItem.MapObjType = Game.MapObjectType.Item;
                    floorItem.X = X;
                    floorItem.Y = Y;
                    floorItem.Type = Network.GamePackets.FloorItem.Drop;
                    floorItem.OnFloor = Time32.Now;
                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    while (Map.Npcs.ContainsKey(floorItem.UID))
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    Map.AddFloorItem(floorItem);
                    foreach (Client.GameState C in Kernel.GamePool.Values)
                    {
                        if (C.Entity.MapID == 1002)
                        {
                            C.SendScreenSpawn(floorItem, true);
                            npc.SendSpawn(C);
                            C.Entity.Update(MTA.Network.GamePackets._String.Effect, "other", true);
                        }
                    }

                }
                #endregion
            }
        }
        public static void Load8()
        {
            if (Kernel.Maps.ContainsKey(1002))
            {
                #region CPBag

                uint ItemID = 720159;
                ushort X = X7, Y = Y7;
                INpc npc = new Network.GamePackets.NpcSpawn();
                npc.UID = 1305;
                npc.Mesh = 13050;
                npc.Type = Enums.NpcType.Talker;
                npc.X = (ushort)(X - 1);
                npc.Y = (ushort)(Y - 1);
                npc.MapID = 1002;

                Game.Map Map = Kernel.Maps[1002];
                if (Map.SelectCoordonates(ref X, ref Y))
                {
                    Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.ConquerPoints;
                    floorItem.Value = 250000;
                    floorItem.ItemID = ItemID;
                    floorItem.MapID = 1002;
                    floorItem.MapObjType = Game.MapObjectType.Item;
                    floorItem.X = X;
                    floorItem.Y = Y;
                    floorItem.Type = Network.GamePackets.FloorItem.Drop;
                    floorItem.OnFloor = Time32.Now;
                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    while (Map.Npcs.ContainsKey(floorItem.UID))
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    Map.AddFloorItem(floorItem);
                    foreach (Client.GameState C in Kernel.GamePool.Values)
                    {
                        if (C.Entity.MapID == 1002)
                        {
                            C.SendScreenSpawn(floorItem, true);
                            npc.SendSpawn(C);
                            C.Entity.Update(MTA.Network.GamePackets._String.Effect, "other", true);
                        }
                    }

                #endregion
                }
            }
        }
        public static void Load9()
        {
            if (Kernel.Maps.ContainsKey(1002))
            {
                #region CPBag

                uint ItemID = 720159;
                ushort X = X8, Y = Y8;
                INpc npc = new Network.GamePackets.NpcSpawn();
                npc.UID = 1305;
                npc.Mesh = 13050;
                npc.Type = Enums.NpcType.Talker;
                npc.X = (ushort)(X - 1);
                npc.Y = (ushort)(Y - 1);
                npc.MapID = 1002;

                Game.Map Map = Kernel.Maps[1002];
                if (Map.SelectCoordonates(ref X, ref Y))
                {
                    Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.ConquerPoints;
                    floorItem.Value = 250000;
                    floorItem.ItemID = ItemID;
                    floorItem.MapID = 1002;
                    floorItem.MapObjType = Game.MapObjectType.Item;
                    floorItem.X = X;
                    floorItem.Y = Y;
                    floorItem.Type = Network.GamePackets.FloorItem.Drop;
                    floorItem.OnFloor = Time32.Now;
                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    while (Map.Npcs.ContainsKey(floorItem.UID))
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    Map.AddFloorItem(floorItem);
                    foreach (Client.GameState C in Kernel.GamePool.Values)
                    {
                        if (C.Entity.MapID == 1002)
                        {
                            C.SendScreenSpawn(floorItem, true);
                            npc.SendSpawn(C);
                            C.Entity.Update(MTA.Network.GamePackets._String.Effect, "other", true);
                        }
                    }

                #endregion
                }
            }
        }
        public static void Load10()
        {
            if (Kernel.Maps.ContainsKey(1002))
            {
                #region CPBag

                uint ItemID = 720159;
                ushort X = X9, Y = Y9;
                INpc npc = new Network.GamePackets.NpcSpawn();
                npc.UID = 1305;
                npc.Mesh = 13050;
                npc.Type = Enums.NpcType.Talker;
                npc.X = (ushort)(X - 1);
                npc.Y = (ushort)(Y - 1);
                npc.MapID = 1002;

                Game.Map Map = Kernel.Maps[1002];
                if (Map.SelectCoordonates(ref X, ref Y))
                {
                    Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.ConquerPoints;
                    floorItem.Value = 250000;
                    floorItem.ItemID = ItemID;
                    floorItem.MapID = 1002;
                    floorItem.MapObjType = Game.MapObjectType.Item;
                    floorItem.X = X;
                    floorItem.Y = Y;
                    floorItem.Type = Network.GamePackets.FloorItem.Drop;
                    floorItem.OnFloor = Time32.Now;
                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    while (Map.Npcs.ContainsKey(floorItem.UID))
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    Map.AddFloorItem(floorItem);
                    foreach (Client.GameState C in Kernel.GamePool.Values)
                    {
                        if (C.Entity.MapID == 1002)
                        {
                            C.SendScreenSpawn(floorItem, true);
                            npc.SendSpawn(C);
                            C.Entity.Update(MTA.Network.GamePackets._String.Effect, "other", true);
                        }
                    }

                #endregion
                }
            }
        }
        public static void Load11()
        {
            if (Kernel.Maps.ContainsKey(1002))
            {
                #region CPBag

                uint ItemID = 720159;
                ushort X = X10, Y = Y10;
                INpc npc = new Network.GamePackets.NpcSpawn();
                npc.UID = 1305;
                npc.Mesh = 13050;
                npc.Type = Enums.NpcType.Talker;
                npc.X = (ushort)(X - 1);
                npc.Y = (ushort)(Y - 1);
                npc.MapID = 1002;

                Game.Map Map = Kernel.Maps[1002];
                if (Map.SelectCoordonates(ref X, ref Y))
                {
                    Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.ConquerPoints;
                    floorItem.Value = 250000;
                    floorItem.ItemID = ItemID;
                    floorItem.MapID = 1002;
                    floorItem.MapObjType = Game.MapObjectType.Item;
                    floorItem.X = X;
                    floorItem.Y = Y;
                    floorItem.Type = Network.GamePackets.FloorItem.Drop;
                    floorItem.OnFloor = Time32.Now;
                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    while (Map.Npcs.ContainsKey(floorItem.UID))
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    Map.AddFloorItem(floorItem);
                    foreach (Client.GameState C in Kernel.GamePool.Values)
                    {
                        if (C.Entity.MapID == 1002)
                        {
                            C.SendScreenSpawn(floorItem, true);
                            npc.SendSpawn(C);
                            C.Entity.Update(MTA.Network.GamePackets._String.Effect, "other", true);
                        }
                    }

                #endregion
                }
            }
        }
        public static void Load12()
        {
            if (Kernel.Maps.ContainsKey(1002))
            {
                #region CPBag

                uint ItemID = 720159;
                ushort X = X11, Y = Y11;
                INpc npc = new Network.GamePackets.NpcSpawn();
                npc.UID = 1305;
                npc.Mesh = 13050;
                npc.Type = Enums.NpcType.Talker;
                npc.X = (ushort)(X - 1);
                npc.Y = (ushort)(Y - 1);
                npc.MapID = 1002;

                Game.Map Map = Kernel.Maps[1002];
                if (Map.SelectCoordonates(ref X, ref Y))
                {
                    Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.ConquerPoints;
                    floorItem.Value = 250000;
                    floorItem.ItemID = ItemID;
                    floorItem.MapID = 1002;
                    floorItem.MapObjType = Game.MapObjectType.Item;
                    floorItem.X = X;
                    floorItem.Y = Y;
                    floorItem.Type = Network.GamePackets.FloorItem.Drop;
                    floorItem.OnFloor = Time32.Now;
                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    while (Map.Npcs.ContainsKey(floorItem.UID))
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    Map.AddFloorItem(floorItem);
                    foreach (Client.GameState C in Kernel.GamePool.Values)
                    {
                        if (C.Entity.MapID == 1002)
                        {
                            C.SendScreenSpawn(floorItem, true);
                            npc.SendSpawn(C);
                            C.Entity.Update(MTA.Network.GamePackets._String.Effect, "other", true);
                        }
                    }

                #endregion
                }
            }
        }
        public static void Load13()
        {
            if (Kernel.Maps.ContainsKey(1002))
            {
                #region CPBag

                uint ItemID = 720159;
                ushort X = X12, Y = Y12;
                INpc npc = new Network.GamePackets.NpcSpawn();
                npc.UID = 1305;
                npc.Mesh = 13050;
                npc.Type = Enums.NpcType.Talker;
                npc.X = (ushort)(X - 1);
                npc.Y = (ushort)(Y - 1);
                npc.MapID = 1002;

                Game.Map Map = Kernel.Maps[1002];
                if (Map.SelectCoordonates(ref X, ref Y))
                {
                    Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.ConquerPoints;
                    floorItem.Value = 250000;
                    floorItem.ItemID = ItemID;
                    floorItem.MapID = 1002;
                    floorItem.MapObjType = Game.MapObjectType.Item;
                    floorItem.X = X;
                    floorItem.Y = Y;
                    floorItem.Type = Network.GamePackets.FloorItem.Drop;
                    floorItem.OnFloor = Time32.Now;
                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    while (Map.Npcs.ContainsKey(floorItem.UID))
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    Map.AddFloorItem(floorItem);
                    foreach (Client.GameState C in Kernel.GamePool.Values)
                    {
                        if (C.Entity.MapID == 1002)
                        {
                            C.SendScreenSpawn(floorItem, true);
                            npc.SendSpawn(C);
                            C.Entity.Update(MTA.Network.GamePackets._String.Effect, "other", true);
                        }
                    }

                #endregion
                }
            }
        }
        public static void Load14()
        {
            if (Kernel.Maps.ContainsKey(1002))
            {
                #region CPBag

                uint ItemID = 720159;
                ushort X = X13, Y = Y13;
                INpc npc = new Network.GamePackets.NpcSpawn();
                npc.UID = 1305;
                npc.Mesh = 13050;
                npc.Type = Enums.NpcType.Talker;
                npc.X = (ushort)(X - 1);
                npc.Y = (ushort)(Y - 1);
                npc.MapID = 1002;

                Game.Map Map = Kernel.Maps[1002];
                if (Map.SelectCoordonates(ref X, ref Y))
                {
                    Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.ConquerPoints;
                    floorItem.Value = 250000;
                    floorItem.ItemID = ItemID;
                    floorItem.MapID = 1002;
                    floorItem.MapObjType = Game.MapObjectType.Item;
                    floorItem.X = X;
                    floorItem.Y = Y;
                    floorItem.Type = Network.GamePackets.FloorItem.Drop;
                    floorItem.OnFloor = Time32.Now;
                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    while (Map.Npcs.ContainsKey(floorItem.UID))
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    Map.AddFloorItem(floorItem);
                    foreach (Client.GameState C in Kernel.GamePool.Values)
                    {
                        if (C.Entity.MapID == 1002)
                        {
                            C.SendScreenSpawn(floorItem, true);
                            npc.SendSpawn(C);
                            C.Entity.Update(MTA.Network.GamePackets._String.Effect, "other", true);
                        }
                    }

                #endregion
                }
            }
        }
        public static void Load15()
        {
            if (Kernel.Maps.ContainsKey(1002))
            {
                #region CPBag

                uint ItemID = 720159;
                ushort X = X14, Y = Y14;
                INpc npc = new Network.GamePackets.NpcSpawn();
                npc.UID = 1305;
                npc.Mesh = 13050;
                npc.Type = Enums.NpcType.Talker;
                npc.X = (ushort)(X - 1);
                npc.Y = (ushort)(Y - 1);
                npc.MapID = 1002;

                Game.Map Map = Kernel.Maps[1002];
                if (Map.SelectCoordonates(ref X, ref Y))
                {
                    Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.ConquerPoints;
                    floorItem.Value = 250000;
                    floorItem.ItemID = ItemID;
                    floorItem.MapID = 1002;
                    floorItem.MapObjType = Game.MapObjectType.Item;
                    floorItem.X = X;
                    floorItem.Y = Y;
                    floorItem.Type = Network.GamePackets.FloorItem.Drop;
                    floorItem.OnFloor = Time32.Now;
                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    while (Map.Npcs.ContainsKey(floorItem.UID))
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    Map.AddFloorItem(floorItem);
                    foreach (Client.GameState C in Kernel.GamePool.Values)
                    {
                        if (C.Entity.MapID == 1002)
                        {
                            C.SendScreenSpawn(floorItem, true);
                            npc.SendSpawn(C);
                            C.Entity.Update(MTA.Network.GamePackets._String.Effect, "other", true);
                        }
                    }

                #endregion
                }
            }
        }
        public static void Load16()
        {
            if (Kernel.Maps.ContainsKey(1002))
            {
                #region CPBag

                uint ItemID = 720159;
                ushort X = X15, Y = Y15;
                INpc npc = new Network.GamePackets.NpcSpawn();
                npc.UID = 1305;
                npc.Mesh = 13050;
                npc.Type = Enums.NpcType.Talker;
                npc.X = (ushort)(X - 1);
                npc.Y = (ushort)(Y - 1);
                npc.MapID = 1002;

                Game.Map Map = Kernel.Maps[1002];
                if (Map.SelectCoordonates(ref X, ref Y))
                {
                    Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.ConquerPoints;
                    floorItem.Value = 250000;
                    floorItem.ItemID = ItemID;
                    floorItem.MapID = 1002;
                    floorItem.MapObjType = Game.MapObjectType.Item;
                    floorItem.X = X;
                    floorItem.Y = Y;
                    floorItem.Type = Network.GamePackets.FloorItem.Drop;
                    floorItem.OnFloor = Time32.Now;
                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    while (Map.Npcs.ContainsKey(floorItem.UID))
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    Map.AddFloorItem(floorItem);
                    foreach (Client.GameState C in Kernel.GamePool.Values)
                    {
                        if (C.Entity.MapID == 1002)
                        {
                            C.SendScreenSpawn(floorItem, true);
                            npc.SendSpawn(C);
                            C.Entity.Update(MTA.Network.GamePackets._String.Effect, "other", true);
                        }
                    }

                #endregion
                }
            }
        }
        public static void Load17()
        {
            if (Kernel.Maps.ContainsKey(1002))
            {
                #region CPBag

                uint ItemID = 720159;
                ushort X = X16, Y = Y16;
                INpc npc = new Network.GamePackets.NpcSpawn();
                npc.UID = 1305;
                npc.Mesh = 13050;
                npc.Type = Enums.NpcType.Talker;
                npc.X = (ushort)(X - 1);
                npc.Y = (ushort)(Y - 1);
                npc.MapID = 1002;

                Game.Map Map = Kernel.Maps[1002];
                if (Map.SelectCoordonates(ref X, ref Y))
                {
                    Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                    floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.ConquerPoints;
                    floorItem.Value = 250000;
                    floorItem.ItemID = ItemID;
                    floorItem.MapID = 1002;
                    floorItem.MapObjType = Game.MapObjectType.Item;
                    floorItem.X = X;
                    floorItem.Y = Y;
                    floorItem.Type = Network.GamePackets.FloorItem.Drop;
                    floorItem.OnFloor = Time32.Now;
                    floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    while (Map.Npcs.ContainsKey(floorItem.UID))
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                    Map.AddFloorItem(floorItem);
                    foreach (Client.GameState C in Kernel.GamePool.Values)
                    {
                        if (C.Entity.MapID == 1002)
                        {
                            C.SendScreenSpawn(floorItem, true);
                            npc.SendSpawn(C);
                            C.Entity.Update(MTA.Network.GamePackets._String.Effect, "other", true);
                        }
                    }

                #endregion
                }
            }
        }
        public static void Load18()
        {
            if (Kernel.Maps.ContainsKey(1002))
            {
                ushort X = 900, Y = 900;
                INpc npc = new Network.GamePackets.NpcSpawn();
                npc.UID = 1305;
                npc.Mesh = 13050;
                npc.Type = Enums.NpcType.Talker;
                npc.X = (ushort)(X - 1);
                npc.Y = (ushort)(Y - 1);
                npc.MapID = 1002;
                foreach (Client.GameState C in Kernel.GamePool.Values)
                {
                    if (C.Entity.MapID == 1002)
                    {

                        npc.SendSpawn(C);
                        C.Entity.Update(MTA.Network.GamePackets._String.Effect, "other", true);
                    }
                }

            }


        }


    }

}