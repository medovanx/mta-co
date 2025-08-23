using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Network.GamePackets;
using MTA.Interfaces;

namespace MTA.Game
{
    class jiMMyQuest
    {
        public static ushort X = 311, Y = 268;
        public static ushort X1 = 300, Y1 = 266;
        public static ushort X2 = 299, Y2 = 278;
        public static ushort X3 = 318, Y3 = 278;
        public static ushort X4 = 324, Y4 = 278;
        public static ushort X5 = 319, Y5 = 270;
        public static ushort X6 = 334, Y6 = 278;
        public static ushort X7 = 298, Y7 = 271;
        public static ushort X8 = 324, Y8 = 266;
        public static ushort X9 = 309, Y9 = 255;
        public static ushort X10 = 301, Y10 = 246;
        public static ushort X11 = 301, Y11 = 230;
        public static ushort X12 = 313, Y12 = 238;
        public static ushort X13 = 323, Y13 = 238;
        public static ushort X14 = 330, Y14 = 250;
        public static ushort X15 = 321, Y15 = 255;
        public static ushort X16 = 320, Y16 = 267;
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
                    floorItem.Value = 50000;
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
                    floorItem.Value = 50000;
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
                    floorItem.Value = 50000;
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
                    floorItem.Value = 50000;
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
                    floorItem.Value = 50000;
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
                    floorItem.Value = 50000;
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
                    floorItem.Value = 50000;
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
                ushort X = X7, Y = Y16;
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
                    floorItem.Value = 50000;
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
                ushort X = 9, Y = 9;
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
                    }
                }

            }


        }
        public static void Load10()
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
                    floorItem.Value = 50000;
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
                    floorItem.Value = 50000;
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
                ushort X = X7, Y = Y16;
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
                    floorItem.Value = 50000;
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
                    floorItem.Value = 50000;
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
                    floorItem.Value = 50000;
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
                    floorItem.Value = 50000;
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
                    floorItem.Value = 50000;
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
                        }
                    }

                #endregion
                }
            }
        }


    }

}