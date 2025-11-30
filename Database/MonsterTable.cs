using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MTA.Network.GamePackets;

namespace MTA.Database
{
    public class MonsterInformation
    {
        #region Special Mobs Matrix
        public uint Type;
        public byte Switch;
        public uint helmet_type;
        public uint armor_type;
        public uint weaponr_type;
        public uint weaponl_type;
        public Time32 Lastpop;
        #endregion
        private struct SpecialItemDrop
        {
            public int ItemID, Rate, Discriminant, Map;
        }
        private static List<SpecialItemDrop> SpecialItemDropList = new List<SpecialItemDrop>();
        public bool IsRespawnAble = true;
        public Game.Entity Owner;
        public uint ExcludeFromSend = 0;
        private bool LabirinthDrop = false;
        public bool Guard, Reviver;
        public uint ID;
        public ushort Mesh;
        public byte Level;
        public string Name;
        public string Name2;
        public uint Hitpoints;
        public ushort Defence;
        public ushort ViewRange;
        public ushort AttackRange;
        public int RespawnTime;
        public uint MinAttack, MaxAttack;
        public byte AttackType;
        public ushort SpellID;
        public uint InSight;
        public bool ISLava = false;
        public bool Boss;
        public bool SuperBoss;
        public Time32 LastMove;
        public int MoveSpeed;
        public int RunSpeed;
        public int HPPotionID, MPPotionID;
        public int AttackSpeed;
        public int MinimumSpeed
        {
            get
            {
                int min = 10000000;
                if (min > MoveSpeed)
                    min = MoveSpeed;
                if (min > RunSpeed)
                    min = RunSpeed;
                if (min > AttackSpeed)
                    min = AttackSpeed;
                return min;
            }
        }
        public uint ExtraExperience;
        public ulong MinMoneyDropAmount;
        public ulong MaxMoneyDropAmount;

        public ushort BoundX, BoundY;
        public ushort BoundCX, BoundCY;

        public void SendScreen(byte[] buffer)
        {
            foreach (Client.GameState client in Program.Values)
            {
                if (client != null)
                {
                    if (client.Entity != null)
                    {
                        if (client.Entity.UID != ExcludeFromSend)
                        {
                            if (Kernel.GetDistance(client.Entity.X, client.Entity.Y, Owner.X, Owner.Y) > 18)
                            {
                                continue;
                            }
                            client.Send(buffer);
                        }
                    }
                }
            }
        }
        public void SendScreen(Interfaces.IPacket buffer)
        {
            SendScreen(buffer.ToArray());
        }
        public void SendScreenSpawn(Interfaces.IMapObject _object)
        {
            foreach (Client.GameState client in Program.Values)
            {
                if (client != null)
                {
                    if (client.Entity != null)
                    {
                        if (client.Entity.UID != ExcludeFromSend)
                        {
                            if (client.Map.ID == Owner.MapID)
                            {
                                if (Kernel.GetDistance(client.Entity.X, client.Entity.Y, Owner.X, Owner.Y) > 25)
                                {
                                    continue;
                                }
                                _object.SendSpawn(client, false);
                            }
                        }
                    }
                }
            }
        }
        public static bool ItemsInInventory = false;

        public void Drop(Game.Entity killer)
        {
            #region Ramadan kareem
            #region V1

            if (Name == "RamadankareemV1")
            {//MenaMaGice
                uint ItemID = 0;
                byte type1 = 2;
                for (int i = 0; i < 1; i++)
                {
                    type1 = (byte)Kernel.Random.Next(1, 2);
                    switch (type1)
                    {
                        case 1:
                            ItemID = 767099;
                            break;
                    }
                    var infos = Database.ConquerItemInformation.BaseInformations[ItemID];
                    ushort X = Owner.X, Y = Owner.Y;
                    Game.Map Map = Kernel.Maps[Owner.MapID];
                    if (Map.SelectCoordonates(ref X, ref Y))
                    {
                        Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                        floorItem.Item = new Network.GamePackets.ConquerItem(true);
                        floorItem.Item.Color = (MTA.Game.Enums.Color)Kernel.Random.Next(4, 8);
                        floorItem.Item.ID = ItemID;
                        floorItem.Item.Plus = floorItem.Item.Plus;
                        floorItem.Item.MaximDurability = infos.Durability;
                        floorItem.Item.Durability = infos.Durability;
                        floorItem.Item.MobDropped = true;
                        floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
                        floorItem.ItemID = ItemID;
                        floorItem.MapID = Owner.MapID;
                        floorItem.MapObjType = Game.MapObjectType.Item;
                        floorItem.X = X;
                        floorItem.Y = Y;
                        floorItem.Type = Network.GamePackets.FloorItem.Drop;
                        floorItem.OnFloor = Time32.Now;
                        floorItem.ItemColor = floorItem.Item.Color;
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        while (Map.Npcs.ContainsKey(floorItem.UID))
                            floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        Map.AddFloorItem(floorItem);
                        SendScreenSpawn(floorItem);
                    }
                }
                return;
            }

            #endregion
            #region V2

            if (Name == "RamadankareemV2")
            {//MenaMaGice
                uint ItemID = 0;
                byte type1 = 2;
                for (int i = 0; i < 1; i++)
                {
                    type1 = (byte)Kernel.Random.Next(1, 2);
                    switch (type1)
                    {
                        case 1:
                            ItemID = 767100;
                            break;
                    }
                    var infos = Database.ConquerItemInformation.BaseInformations[ItemID];
                    ushort X = Owner.X, Y = Owner.Y;
                    Game.Map Map = Kernel.Maps[Owner.MapID];
                    if (Map.SelectCoordonates(ref X, ref Y))
                    {
                        Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                        floorItem.Item = new Network.GamePackets.ConquerItem(true);
                        floorItem.Item.Color = (MTA.Game.Enums.Color)Kernel.Random.Next(4, 8);
                        floorItem.Item.ID = ItemID;
                        floorItem.Item.Plus = floorItem.Item.Plus;
                        floorItem.Item.MaximDurability = infos.Durability;
                        floorItem.Item.Durability = infos.Durability;
                        floorItem.Item.MobDropped = true;
                        floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
                        floorItem.ItemID = ItemID;
                        floorItem.MapID = Owner.MapID;
                        floorItem.MapObjType = Game.MapObjectType.Item;
                        floorItem.X = X;
                        floorItem.Y = Y;
                        floorItem.Type = Network.GamePackets.FloorItem.Drop;
                        floorItem.OnFloor = Time32.Now;
                        floorItem.ItemColor = floorItem.Item.Color;
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        while (Map.Npcs.ContainsKey(floorItem.UID))
                            floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        Map.AddFloorItem(floorItem);
                        SendScreenSpawn(floorItem);
                    }
                }
                return;
            }

            #endregion
            #region V3

            if (Name == "RamadankareemV3")
            {//MenaMaGice
                uint ItemID = 0;
                byte type1 = 2;
                for (int i = 0; i < 1; i++)
                {
                    type1 = (byte)Kernel.Random.Next(1, 2);
                    switch (type1)
                    {
                        case 1:
                            ItemID = 767101;
                            break;
                    }
                    var infos = Database.ConquerItemInformation.BaseInformations[ItemID];
                    ushort X = Owner.X, Y = Owner.Y;
                    Game.Map Map = Kernel.Maps[Owner.MapID];
                    if (Map.SelectCoordonates(ref X, ref Y))
                    {
                        Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                        floorItem.Item = new Network.GamePackets.ConquerItem(true);
                        floorItem.Item.Color = (MTA.Game.Enums.Color)Kernel.Random.Next(4, 8);
                        floorItem.Item.ID = ItemID;
                        floorItem.Item.Plus = floorItem.Item.Plus;
                        floorItem.Item.MaximDurability = infos.Durability;
                        floorItem.Item.Durability = infos.Durability;
                        floorItem.Item.MobDropped = true;
                        floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
                        floorItem.ItemID = ItemID;
                        floorItem.MapID = Owner.MapID;
                        floorItem.MapObjType = Game.MapObjectType.Item;
                        floorItem.X = X;
                        floorItem.Y = Y;
                        floorItem.Type = Network.GamePackets.FloorItem.Drop;
                        floorItem.OnFloor = Time32.Now;
                        floorItem.ItemColor = floorItem.Item.Color;
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        while (Map.Npcs.ContainsKey(floorItem.UID))
                            floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        Map.AddFloorItem(floorItem);
                        SendScreenSpawn(floorItem);
                    }
                }
                return;
            }

            #endregion
            #endregion
            if (killer.EntityFlag == Game.EntityFlag.Monster)
            {
                if (killer.Owner != null)
                    killer = killer.Owner.Entity;
                else
                    return;
            }

            if (MaTrix.MatrixMob.Drop(killer, Owner))
                return;

            #region All Monster Cps Drop !

            if (killer.Name.Contains("Guard"))
            {
            }

            else
            {
                if (killer.ContainsFlag3((ulong)Update.Flags3.AutoHunting))
                {
                    killer.ConquerPoints += 20000;
                    killer.Owner.Send(new Network.GamePackets.Message("Gratz ! You Kill [ " + Owner.Name + " ] And Got [ 20000 Cps - Becouse You Open AutoHunting ] #37 .", System.Drawing.Color.Red, 2005));
                }
                else
                {
                    killer.ConquerPoints += 20000;
                    killer.Owner.Send(new Network.GamePackets.Message("Gratz ! You Kill [ " + Owner.Name + " ] And Got [ 20000 Cps ] #37 .", System.Drawing.Color.Yellow, 2005));
                }
            }
            #endregion 

            #region ThirillingSpook
            if (killer.Owner.Quests.HasQuest(MaTrix.QuestID.Exorcism))
            {
                var quest = killer.Owner.Quests.GetQuest(MaTrix.QuestID.Exorcism);
                if (Name == quest.Mob)
                {
                    if (killer.Owner.Team != null)
                    {
                        foreach (var member in killer.Owner.Team.Teammates)
                        {
                            if (member.Quests.HasQuest(MaTrix.QuestID.Exorcism))
                            {
                                quest = member.Quests.GetQuest(MaTrix.QuestID.Exorcism);
                                uint kills = 1;
                                if (quest.Mob == "ThrillingSpook2")
                                    kills = 2;
                                else if (quest.Mob == "ThrillingSpook3")
                                    kills = 3;

                                if (quest.Kills >= kills)
                                    continue;
                                member.Quests.IncreaseQuestKills(MaTrix.QuestID.Exorcism, kills);
                                Network.GamePackets.NpcReply npc = new Network.GamePackets.NpcReply(6, "Congratulations, You have killed " + quest.Mob + " 1 go to HeavenMaster to get Your reward");
                                npc.OptionID = 255;
                                member.Send(npc.ToArray());

                                Network.GamePackets._String str = new _String(true);
                                str.UID = killer.UID;
                                str.TextsCount = 1;
                                str.Type = _String.Effect;
                                str.Texts.Add("cortege");
                                member.Send(str);
                            }
                        }
                    }
                    else
                    {
                        uint kills = 1;
                        if (quest.Mob == "ThrillingSpook2")
                            kills = 2;
                        else if (quest.Mob == "ThrillingSpook3")
                            kills = 3;

                        if (quest.Kills >= kills)
                            return;
                        killer.Owner.Quests.IncreaseQuestKills(MaTrix.QuestID.Exorcism, kills);
                        Network.GamePackets.NpcReply npc = new Network.GamePackets.NpcReply(6, "Congratulations, You have killed " + quest.Mob + " 1 go to HeavenMaster to get Your reward");
                        npc.OptionID = 255;
                        killer.Owner.Send(npc.ToArray());

                        Network.GamePackets._String str = new _String(true);
                        str.UID = killer.UID;
                        str.TextsCount = 1;
                        str.Type = _String.Effect;
                        str.Texts.Add("cortege");
                        killer.Owner.Send(str);
                    }
                    return;
                }

            }
            #endregion

            #region EveryThingHas
            #region ApeSkin
            {
                if (Name == "ThunderApe" && Kernel.Rate(30))
                {

                    var infos = Database.ConquerItemInformation.BaseInformations[729088];
                    ushort X = Owner.X, Y = Owner.Y;
                    Game.Map Map = Kernel.Maps[Owner.MapID];
                    if (Map.SelectCoordonates(ref X, ref Y))
                    {
                        Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                        floorItem.Item = new Network.GamePackets.ConquerItem(true);
                        floorItem.Item.Color = (Game.Enums.Color)Kernel.Random.Next(4, 8);
                        floorItem.Item.ID = 729088;
                        floorItem.Item.MobDropped = true;
                        floorItem.Item.MaximDurability = infos.Durability;
                        floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
                        floorItem.Item.StackSize = 1;
                        floorItem.Item.MaxStackSize = infos.StackSize;
                        floorItem.ItemID = 729088;
                        floorItem.MapID = Owner.MapID;
                        floorItem.MapObjType = Game.MapObjectType.Item;
                        floorItem.X = X;
                        floorItem.Y = Y;
                        floorItem.Type = Network.GamePackets.FloorItem.Drop;
                        floorItem.OnFloor = Time32.Now;
                        floorItem.Owner = killer.Owner;
                        floorItem.ItemColor = floorItem.Item.Color;
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        while (Map.Npcs.ContainsKey(floorItem.UID))
                            floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        Map.AddFloorItem(floorItem);
                        SendScreenSpawn(floorItem);
                    }
                }
                if (Name == "ThunderApe" && Kernel.Rate(10))
                {

                    var infos = Database.ConquerItemInformation.BaseInformations[729088];
                    ushort X = Owner.X, Y = Owner.Y;
                    Game.Map Map = Kernel.Maps[Owner.MapID];
                    if (Map.SelectCoordonates(ref X, ref Y))
                    {
                        Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                        floorItem.Item = new Network.GamePackets.ConquerItem(true);
                        floorItem.Item.Color = (Game.Enums.Color)Kernel.Random.Next(4, 8);
                        floorItem.Item.ID = 729088;
                        floorItem.Item.MobDropped = true;
                        floorItem.Item.MaximDurability = infos.Durability;
                        floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
                        floorItem.Item.StackSize = 5;
                        floorItem.Item.MaxStackSize = infos.StackSize;
                        floorItem.ItemID = 729088;
                        floorItem.MapID = Owner.MapID;
                        floorItem.MapObjType = Game.MapObjectType.Item;
                        floorItem.X = X;
                        floorItem.Y = Y;
                        floorItem.Type = Network.GamePackets.FloorItem.Drop;
                        floorItem.OnFloor = Time32.Now;
                        floorItem.Owner = killer.Owner;
                        floorItem.ItemColor = floorItem.Item.Color;
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        while (Map.Npcs.ContainsKey(floorItem.UID))
                            floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        Map.AddFloorItem(floorItem);
                        SendScreenSpawn(floorItem);
                    }
                }
            }
            #endregion
            #region SnakeHeart
            {
                if (Name == "Snakeman" && Kernel.Rate(30))
                {

                    var infos = Database.ConquerItemInformation.BaseInformations[729089];
                    ushort X = Owner.X, Y = Owner.Y;
                    Game.Map Map = Kernel.Maps[Owner.MapID];
                    if (Map.SelectCoordonates(ref X, ref Y))
                    {
                        Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                        floorItem.Item = new Network.GamePackets.ConquerItem(true);
                        floorItem.Item.Color = (Game.Enums.Color)Kernel.Random.Next(4, 8);
                        floorItem.Item.ID = 729089;
                        floorItem.Item.MobDropped = true;
                        floorItem.Item.MaximDurability = infos.Durability;
                        floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
                        floorItem.Item.StackSize = 1;
                        floorItem.Item.MaxStackSize = infos.StackSize;
                        floorItem.ItemID = 729089;
                        floorItem.MapID = Owner.MapID;
                        floorItem.MapObjType = Game.MapObjectType.Item;
                        floorItem.X = X;
                        floorItem.Y = Y;
                        floorItem.Type = Network.GamePackets.FloorItem.Drop;
                        floorItem.OnFloor = Time32.Now;
                        floorItem.Owner = killer.Owner;
                        floorItem.ItemColor = floorItem.Item.Color;
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        while (Map.Npcs.ContainsKey(floorItem.UID))
                            floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        Map.AddFloorItem(floorItem);
                        SendScreenSpawn(floorItem);
                    }
                }
                if (Name == "Snakeman" && Kernel.Rate(10))
                {

                    var infos = Database.ConquerItemInformation.BaseInformations[729089];
                    ushort X = Owner.X, Y = Owner.Y;
                    Game.Map Map = Kernel.Maps[Owner.MapID];
                    if (Map.SelectCoordonates(ref X, ref Y))
                    {
                        Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                        floorItem.Item = new Network.GamePackets.ConquerItem(true);
                        floorItem.Item.Color = (Game.Enums.Color)Kernel.Random.Next(4, 8);
                        floorItem.Item.ID = 729089;
                        floorItem.Item.MobDropped = true;
                        floorItem.Item.MaximDurability = infos.Durability;
                        floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
                        floorItem.Item.StackSize = 5;
                        floorItem.Item.MaxStackSize = infos.StackSize;
                        floorItem.ItemID = 729089;
                        floorItem.MapID = Owner.MapID;
                        floorItem.MapObjType = Game.MapObjectType.Item;
                        floorItem.X = X;
                        floorItem.Y = Y;
                        floorItem.Type = Network.GamePackets.FloorItem.Drop;
                        floorItem.OnFloor = Time32.Now;
                        floorItem.Owner = killer.Owner;
                        floorItem.ItemColor = floorItem.Item.Color;
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        while (Map.Npcs.ContainsKey(floorItem.UID))
                            floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        Map.AddFloorItem(floorItem);
                        SendScreenSpawn(floorItem);
                    }
                }
            }
            #endregion
            #region BirdClaw
            {
                if (Name == "Birdman" && Kernel.Rate(30))
                {

                    var infos = Database.ConquerItemInformation.BaseInformations[729094];
                    ushort X = Owner.X, Y = Owner.Y;
                    Game.Map Map = Kernel.Maps[Owner.MapID];
                    if (Map.SelectCoordonates(ref X, ref Y))
                    {
                        Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                        floorItem.Item = new Network.GamePackets.ConquerItem(true);
                        floorItem.Item.Color = (Game.Enums.Color)Kernel.Random.Next(4, 8);
                        floorItem.Item.ID = 729094;
                        floorItem.Item.MobDropped = true;
                        floorItem.Item.MaximDurability = infos.Durability;
                        floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
                        floorItem.Item.StackSize = 1;
                        floorItem.Item.MaxStackSize = infos.StackSize;
                        floorItem.ItemID = 729094;
                        floorItem.MapID = Owner.MapID;
                        floorItem.MapObjType = Game.MapObjectType.Item;
                        floorItem.X = X;
                        floorItem.Y = Y;
                        floorItem.Type = Network.GamePackets.FloorItem.Drop;
                        floorItem.OnFloor = Time32.Now;
                        floorItem.Owner = killer.Owner;
                        floorItem.ItemColor = floorItem.Item.Color;
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        while (Map.Npcs.ContainsKey(floorItem.UID))
                            floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        Map.AddFloorItem(floorItem);
                        SendScreenSpawn(floorItem);
                    }
                }
                if (Name == "Birdman" && Kernel.Rate(10))
                {

                    var infos = Database.ConquerItemInformation.BaseInformations[729094];
                    ushort X = Owner.X, Y = Owner.Y;
                    Game.Map Map = Kernel.Maps[Owner.MapID];
                    if (Map.SelectCoordonates(ref X, ref Y))
                    {
                        Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                        floorItem.Item = new Network.GamePackets.ConquerItem(true);
                        floorItem.Item.Color = (Game.Enums.Color)Kernel.Random.Next(4, 8);
                        floorItem.Item.ID = 729094;
                        floorItem.Item.MobDropped = true;
                        floorItem.Item.MaximDurability = infos.Durability;
                        floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
                        floorItem.Item.StackSize = 5;
                        floorItem.Item.MaxStackSize = infos.StackSize;
                        floorItem.ItemID = 729094;
                        floorItem.MapID = Owner.MapID;
                        floorItem.MapObjType = Game.MapObjectType.Item;
                        floorItem.X = X;
                        floorItem.Y = Y;
                        floorItem.Type = Network.GamePackets.FloorItem.Drop;
                        floorItem.OnFloor = Time32.Now;
                        floorItem.Owner = killer.Owner;
                        floorItem.ItemColor = floorItem.Item.Color;
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        while (Map.Npcs.ContainsKey(floorItem.UID))
                            floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        Map.AddFloorItem(floorItem);
                        SendScreenSpawn(floorItem);
                    }
                }
            }
            #endregion
            #region BatWing
            {
                if (Name == "TombBat" && Kernel.Rate(30))
                {

                    var infos = Database.ConquerItemInformation.BaseInformations[729098];
                    ushort X = Owner.X, Y = Owner.Y;
                    Game.Map Map = Kernel.Maps[Owner.MapID];
                    if (Map.SelectCoordonates(ref X, ref Y))
                    {
                        Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                        floorItem.Item = new Network.GamePackets.ConquerItem(true);
                        floorItem.Item.Color = (Game.Enums.Color)Kernel.Random.Next(4, 8);
                        floorItem.Item.ID = 729098;
                        floorItem.Item.MobDropped = true;
                        floorItem.Item.MaximDurability = infos.Durability;
                        floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
                        floorItem.Item.StackSize = 1;
                        floorItem.Item.MaxStackSize = infos.StackSize;
                        floorItem.ItemID = 729098;
                        floorItem.MapID = Owner.MapID;
                        floorItem.MapObjType = Game.MapObjectType.Item;
                        floorItem.X = X;
                        floorItem.Y = Y;
                        floorItem.Type = Network.GamePackets.FloorItem.Drop;
                        floorItem.OnFloor = Time32.Now;
                        floorItem.Owner = killer.Owner;
                        floorItem.ItemColor = floorItem.Item.Color;
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        while (Map.Npcs.ContainsKey(floorItem.UID))
                            floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        Map.AddFloorItem(floorItem);
                        SendScreenSpawn(floorItem);
                    }
                }
                if (Name == "Tombat" && Kernel.Rate(10))
                {

                    var infos = Database.ConquerItemInformation.BaseInformations[729098];
                    ushort X = Owner.X, Y = Owner.Y;
                    Game.Map Map = Kernel.Maps[Owner.MapID];
                    if (Map.SelectCoordonates(ref X, ref Y))
                    {
                        Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                        floorItem.Item = new Network.GamePackets.ConquerItem(true);
                        floorItem.Item.Color = (Game.Enums.Color)Kernel.Random.Next(4, 8);
                        floorItem.Item.ID = 729098;
                        floorItem.Item.MobDropped = true;
                        floorItem.Item.MaximDurability = infos.Durability;
                        floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
                        floorItem.Item.StackSize = 5;
                        floorItem.Item.MaxStackSize = infos.StackSize;
                        floorItem.ItemID = 729098;
                        floorItem.MapID = Owner.MapID;
                        floorItem.MapObjType = Game.MapObjectType.Item;
                        floorItem.X = X;
                        floorItem.Y = Y;
                        floorItem.Type = Network.GamePackets.FloorItem.Drop;
                        floorItem.OnFloor = Time32.Now;
                        floorItem.Owner = killer.Owner;
                        floorItem.ItemColor = floorItem.Item.Color;
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        while (Map.Npcs.ContainsKey(floorItem.UID))
                            floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        Map.AddFloorItem(floorItem);
                        SendScreenSpawn(floorItem);
                    }
                }
            }
            #endregion
            #endregion
            #region MonstersPoints
            if (Name == "TompBat")
            {
                {
                    killer.MonstersPoints += 1;

                }
            }
            if (Name == "RedDevilL117")
            {
                {
                    killer.MonstersPoints += 1;

                }
            }
            if (Name == "BullMonsterL113")
            {
                {
                    killer.MonstersPoints += 1;

                }
            }
            if (Name == "RedDevil")
            {
                {
                    killer.MonstersPoints += 1;

                }
            }
            if (Name == "BullMonster")
            {
                {
                    killer.MonstersPoints += 1;

                }
            }
            if (Name == "BloodyBatL108")
            {
                {
                    killer.MonstersPoints += 1;

                }
            }
            if (Name == "BloodyBat")
            {
                {
                    killer.MonstersPoints += 1;

                }
            }
            if (Name == "RedDevilL118")
            {
                {
                    killer.MonstersPoints += 1;

                }
            }
            if (Name == "HawKing")
            {
                {
                    killer.MonstersPoints += 1;

                }
            }
            if (Name == "HawkL93")
            {
                {
                    killer.MonstersPoints += 1;

                }
            }
            if (Name == "WingedSnake")
            {
                {
                    killer.MonstersPoints += 1;

                }
            }
            if (Name == "Pheasant")
            {
                {
                    killer.MonstersPoints += 1;

                }
            }
            if (Name == "Birdman")
            {
                {
                    killer.MonstersPoints += 1;

                }
            }

            if (Name == "Bandit")
            {
                {
                    killer.MonstersPoints += 1;
                }
            }

            if (Name == "Macaque")
            {
                {
                    killer.MonstersPoints += 1;
                }
            }

            if (Name == "TombBat")
            {
                {
                    killer.MonstersPoints += 1;
                }
            }
            if (Name == "TeratoDragon")
            {
                {
                    killer.MonstersPoints += 5000000;
                }
            }
            if (Name == "ThrillingSpook")
            {
                {
                    killer.MonstersPoints += 5000000;
                }
            }
            if (Name == "SnowBanshee")
            {
                {
                    killer.MonstersPoints += 5000000;
                }
            }
            if (Name == "SnowBansheeSoul")
            {
                {
                    killer.MonstersPoints += 5000000;
                }
            }
            if (Name == "Piglet")
            {
                {
                    killer.MonstersPoints += 5;
                }
            }
            #endregion    
            #region Captain
            if (Owner.MapID == 3033)
            {
                if (Name == "Captain")
                {
                    {
                        killer.ConquerPoints += 100;
                        killer.Owner.Send(new Message("You received 200 CPs! because " + Name + " In This Quest", System.Drawing.Color.Black, Message.TopLeft));
                    }
                }
            }
            #endregion
            #region Cloud Jar
            if (Name == killer.QuestMob)
            {
                killer.QuestKO++;
                killer.Send(new Message(" " + killer.QuestMob + "s Killed: " + killer.QuestKO + ". Target: 300. From: " + killer.QuestFrom + "Captain.", 2005));
                if (killer.QuestKO >= 300)
                {
                    killer.Send(new Message("You have killed enough monsters for the quest. Go report to the " + killer.QuestFrom + "Captain.", 2005));
                }
            }
            #endregion
            #region DisCity
            if (Name == "Naga")
            {
                {
                    killer.DisKO += 1;
                    killer.Owner.Send(new Message("Congratulations! You have got 1 Kill you have Now " + killer.DisKO + " DisKo Points", System.Drawing.Color.Azure, Message.TopLeft));
                    return;
                }
            }
            if (Name == "Temptress")
            {
                {
                    killer.DisKO += 1;
                    killer.Owner.Send(new Message("Congratulations! You have got 1 Kill you have Now " + killer.DisKO + " DisKo Points", System.Drawing.Color.Azure, Message.TopLeft));
                    return;
                }
            }
            if (Name == "Centicore")
            {
                {
                    killer.DisKO += 1;
                    killer.Owner.Send(new Message("Congratulations! You have got 1 Kill you have Now " + killer.DisKO + " DisKo Points", System.Drawing.Color.Azure, Message.TopLeft));
                    return;
                }
            }
            if (Name == "HellTroll")
            {
                {
                    killer.DisKO += 3;
                    killer.Owner.Send(new Message("Congratulations! You have got 3 Kill you have Now " + killer.DisKO + " DisKo Points", System.Drawing.Color.Azure, Message.TopLeft));
                    return;
                }
            }

            #endregion
            #region SpecialItemDrop
            foreach (SpecialItemDrop sitem in SpecialItemDropList)
            {
                if (sitem.Map != 0 && Owner.MapID != sitem.Map)
                    continue;
                if (Kernel.Rate(sitem.Rate, sitem.Discriminant))
                {
                    if (killer.VIPLevel < 0)
                    {
                        if (killer.Owner.Inventory.Count <= 39)
                        {
                            killer.Owner.Inventory.Add((uint)sitem.ItemID, 0, 1);
                            return;
                        }
                    }
                    if (sitem.ItemID == 0 || !Database.ConquerItemInformation.BaseInformations.ContainsKey((uint)sitem.ItemID))
                        return;
                    var infos = Database.ConquerItemInformation.BaseInformations[(uint)sitem.ItemID];
                    ushort X = Owner.X, Y = Owner.Y;
                    Game.Map Map = Kernel.Maps[Owner.MapID];
                    if (Map.SelectCoordonates(ref X, ref Y))
                    {
                        Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                        floorItem.Item = new Network.GamePackets.ConquerItem(true);
                        floorItem.Item.Color = (MTA.Game.Enums.Color)Kernel.Random.Next(4, 8);
                        floorItem.Item.ID = (uint)sitem.ItemID;
                        floorItem.Item.MaximDurability = infos.Durability;
                        floorItem.Item.MobDropped = true;
                        if (!Network.PacketHandler.IsEquipment(sitem.ItemID) && infos.ConquerPointsWorth == 0)
                        {
                            floorItem.Item.StackSize = 1;
                            floorItem.Item.MaxStackSize = infos.StackSize;
                        }
                        floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
                        floorItem.ItemID = (uint)sitem.ItemID;
                        floorItem.MapID = Owner.MapID;
                        floorItem.MapObjType = Game.MapObjectType.Item;
                        floorItem.X = X;
                        floorItem.Y = Y;
                        floorItem.Type = Network.GamePackets.FloorItem.Drop;
                        floorItem.OnFloor = Time32.Now;
                        floorItem.ItemColor = floorItem.Item.Color;
                        floorItem.Owner = killer.Owner;
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        while (Map.Npcs.ContainsKey(floorItem.UID))
                            floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        Map.AddFloorItem(floorItem);
                        SendScreenSpawn(floorItem);
                        break;
                    }
                }
            }
            #endregion
            #region ChestDemon - Franko
            uint ran1 = (uint)MTA.Kernel.Random.Next(1, 22);
            if (ran1 > 15)
            {
                if (this.Name == "ChestDemon")
                {
                    if (killer.Name.Contains("Guard"))
                        return;

                    uint Uid = 0;
                    Random R = new Random();
                    //int Nr = R.Next(1, 1);
                    switch (((byte)Kernel.Random.Next(1, 8)))
                    {
                        case 1:
                            Uid = 3000625;
                            break;


                        case 2:
                            Uid = 3000626;
                            break;


                        case 3:
                            Uid = 3000627;
                            break;
                    }

                    if (Uid != 0)
                    {
                        ushort X = Owner.X, Y = Owner.Y;
                        Game.Map Map = Kernel.Maps[Owner.MapID];
                        if (Map.SelectCoordonates(ref X, ref Y))
                        {
                            Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                            floorItem.Item = new Network.GamePackets.ConquerItem(true);
                            floorItem.Item.Color = (MTA.Game.Enums.Color)Kernel.Random.Next(4, 8);
                            floorItem.Item.ID = Uid;
                            floorItem.Item.MaximDurability = floorItem.Item.Durability = 65535;
                            floorItem.Item.UID = Program.NextItemID; ;
                            floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
                            floorItem.ItemID = Uid;
                            floorItem.MapID = Owner.MapID;
                            floorItem.MapObjType = Game.MapObjectType.Item;
                            floorItem.X = X;
                            floorItem.Y = Y;
                            floorItem.Type = Network.GamePackets.FloorItem.Drop;
                            floorItem.OnFloor = Time32.Now;
                            floorItem.ItemColor = floorItem.Item.Color;
                            floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                            while (Map.Npcs.ContainsKey(floorItem.UID))
                                floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                            Map.AddFloorItem(floorItem);
                            SendScreenSpawn(floorItem);
                        }
                    }
                }
            }
            #endregion
            #region FuriousDevastato ( FirstStage Monster in TrojanEpicQuest ( Matrix Edition ) )
            if (Name == "FuriousDevastato")
            {
                if (Kernel.Rate(80, 100))
                {
                    if (killer.Name.Contains("Guard"))
                    {
                        return;
                    }

                    uint ItemID = 3003336;
                    var infos = Database.ConquerItemInformation.BaseInformations[ItemID];
                    ushort X = Owner.X, Y = Owner.Y;
                    Game.Map Map = Kernel.Maps[Owner.MapID];
                    if (Map.SelectCoordonates(ref X, ref Y))
                    {
                        Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                        floorItem.Item = new Network.GamePackets.ConquerItem(true);
                        floorItem.Item.Color = (MTA.Game.Enums.Color)Kernel.Random.Next(4, 8);
                        floorItem.Item.ID = ItemID;
                        floorItem.Item.Plus = floorItem.Item.Plus;
                        floorItem.Item.MaximDurability = infos.Durability;
                        floorItem.Item.Durability = infos.Durability;
                        floorItem.Item.MobDropped = true;
                        floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
                        floorItem.ItemID = ItemID;
                        floorItem.MapID = Owner.MapID;
                        floorItem.MapObjType = Game.MapObjectType.Item;
                        floorItem.X = X;
                        floorItem.Y = Y;
                        floorItem.Type = Network.GamePackets.FloorItem.Drop;
                        floorItem.OnFloor = Time32.Now;
                        floorItem.ItemColor = floorItem.Item.Color;
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        while (Map.Npcs.ContainsKey(floorItem.UID))

                            floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;

                        Map.AddFloorItem(floorItem);

                        SendScreenSpawn(floorItem);
                    }
                    killer.Owner.Send(new Network.GamePackets.Message("Wow ! You Killed the " + Name + " and Got SolarScarp ! Keep Going !", System.Drawing.Color.Blue, 2005));
                    return;
                }
            }
            #endregion
            #region AwakeDevastator ( SecondStage Monster in TrojanEpicQuest ( Matrix Edition ) )
            if (Name == "AwakeDevastator")
            {
                if (Kernel.Rate(80, 100))
                {
                    if (killer.Name.Contains("Guard"))
                    {
                        return;
                    }

                    uint ItemID = 3003338;
                    var infos = Database.ConquerItemInformation.BaseInformations[ItemID];
                    ushort X = Owner.X, Y = Owner.Y;
                    Game.Map Map = Kernel.Maps[Owner.MapID];
                    if (Map.SelectCoordonates(ref X, ref Y))
                    {
                        Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                        floorItem.Item = new Network.GamePackets.ConquerItem(true);
                        floorItem.Item.Color = (MTA.Game.Enums.Color)Kernel.Random.Next(4, 8);
                        floorItem.Item.ID = ItemID;
                        floorItem.Item.Plus = floorItem.Item.Plus;
                        floorItem.Item.MaximDurability = infos.Durability;
                        floorItem.Item.Durability = infos.Durability;
                        floorItem.Item.MobDropped = true;
                        floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
                        floorItem.ItemID = ItemID;
                        floorItem.MapID = Owner.MapID;
                        floorItem.MapObjType = Game.MapObjectType.Item;
                        floorItem.X = X;
                        floorItem.Y = Y;
                        floorItem.Type = Network.GamePackets.FloorItem.Drop;
                        floorItem.OnFloor = Time32.Now;
                        floorItem.ItemColor = floorItem.Item.Color;
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        while (Map.Npcs.ContainsKey(floorItem.UID))

                            floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;

                        Map.AddFloorItem(floorItem);

                        SendScreenSpawn(floorItem);
                    }
                    killer.Owner.Send(new Network.GamePackets.Message("Wow ! You Killed the " + Name + " and Got SolarEssence ! Keep Going !", System.Drawing.Color.Blue, 2005));
                    return;
                }
            }
            #endregion
            #region EvilMonkMiseryA ( ThirdStage Monster in TrojanEpicQuest ( Matrix Edition ) )
            if (Name == "EvilMonkMiseryA")
            {
                if (Kernel.Rate(80, 100))
                {
                    if (killer.Name.Contains("Guard"))
                    {
                        return;
                    }

                    uint ItemID = 3003335;
                    var infos = Database.ConquerItemInformation.BaseInformations[ItemID];
                    ushort X = Owner.X, Y = Owner.Y;
                    Game.Map Map = Kernel.Maps[Owner.MapID];
                    if (Map.SelectCoordonates(ref X, ref Y))
                    {
                        Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                        floorItem.Item = new Network.GamePackets.ConquerItem(true);
                        floorItem.Item.Color = (MTA.Game.Enums.Color)Kernel.Random.Next(4, 8);
                        floorItem.Item.ID = ItemID;
                        floorItem.Item.Plus = floorItem.Item.Plus;
                        floorItem.Item.MaximDurability = infos.Durability;
                        floorItem.Item.Durability = infos.Durability;
                        floorItem.Item.MobDropped = true;
                        floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
                        floorItem.ItemID = ItemID;
                        floorItem.MapID = Owner.MapID;
                        floorItem.MapObjType = Game.MapObjectType.Item;
                        floorItem.X = X;
                        floorItem.Y = Y;
                        floorItem.Type = Network.GamePackets.FloorItem.Drop;
                        floorItem.OnFloor = Time32.Now;
                        floorItem.ItemColor = floorItem.Item.Color;
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        while (Map.Npcs.ContainsKey(floorItem.UID))

                            floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;

                        Map.AddFloorItem(floorItem);

                        SendScreenSpawn(floorItem);
                    }
                    killer.Owner.Send(new Network.GamePackets.Message("Wow ! You Killed the " + Name + " and Got SolarBladeRemain ! Keep Going !", System.Drawing.Color.Blue, 2005));
                    return;
                }
            }
            #endregion
            #region GhostReaverAdv ( FourthStage Monster in TrojanEpicQuest ( Matrix Edition ) )
            if (Name == "GhostReaverAdv")
            {
                if (Kernel.Rate(80, 100))
                {
                    if (killer.Name.Contains("Guard"))
                    {
                        return;
                    }

                    uint ItemID = 3003340;
                    var infos = Database.ConquerItemInformation.BaseInformations[ItemID];
                    ushort X = Owner.X, Y = Owner.Y;
                    Game.Map Map = Kernel.Maps[Owner.MapID];
                    if (Map.SelectCoordonates(ref X, ref Y))
                    {
                        Network.GamePackets.FloorItem floorItem = new Network.GamePackets.FloorItem(true);
                        floorItem.Item = new Network.GamePackets.ConquerItem(true);
                        floorItem.Item.Color = (MTA.Game.Enums.Color)Kernel.Random.Next(4, 8);
                        floorItem.Item.ID = ItemID;
                        floorItem.Item.Plus = floorItem.Item.Plus;
                        floorItem.Item.MaximDurability = infos.Durability;
                        floorItem.Item.Durability = infos.Durability;
                        floorItem.Item.MobDropped = true;
                        floorItem.ValueType = Network.GamePackets.FloorItem.FloorValueType.Item;
                        floorItem.ItemID = ItemID;
                        floorItem.MapID = Owner.MapID;
                        floorItem.MapObjType = Game.MapObjectType.Item;
                        floorItem.X = X;
                        floorItem.Y = Y;
                        floorItem.Type = Network.GamePackets.FloorItem.Drop;
                        floorItem.OnFloor = Time32.Now;
                        floorItem.ItemColor = floorItem.Item.Color;
                        floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;
                        while (Map.Npcs.ContainsKey(floorItem.UID))

                            floorItem.UID = Network.GamePackets.FloorItem.FloorUID.Next;

                        Map.AddFloorItem(floorItem);

                        SendScreenSpawn(floorItem);
                    }
                    killer.Owner.Send(new Network.GamePackets.Message("Wow ! You Killed the " + Name + " and Got SolarBlade ! Keep Going !", System.Drawing.Color.Blue, 2005));
                    return;
                }
            }
            #endregion                    
            if (Name.Contains("ChestDemon"))
            {
                killer.ChestDemonkill += 1;
                killer.Owner.Send(new Message("You have killed ChestDemon!", System.Drawing.Color.Azure, Message.Center));
            }

            else
            {



            }
        }
        public const int ReviverID = 9879;

        public static uint GetIDFromName(string Name)
        {
            foreach (var item in MonsterInformations.Values)
            {
                if (item.Name == Name)
                    return item.ID;
            }
            return 0;
        }
        
        public static ushort GetMeshFromName(string Name)
        {
            foreach (var item in MonsterInformations.Values)
            {
                if (item.Name == Name)
                    return item.Mesh;
            }
            return 0;
        }
        
        public static SafeDictionary<uint, MonsterInformation> MonsterInformations = new SafeDictionary<uint, MonsterInformation>(8000);

        public static void Load()
        {
            using (var command = new MySqlCommand(MySqlCommandType.SELECT))
            {
                command.Select("monsterinfos");
                using (var reader = command.CreateReader())
                {
                    while (reader.Read())
                    {
                        MonsterInformation mf = new MonsterInformation();
                        mf.ID = reader.ReadUInt32("id");
                        mf.Name = reader.ReadString("name");
                        //  mf.Name2 = reader.ReadString("name2");
                        mf.Mesh = reader.ReadUInt16("lookface");
                        mf.Level = reader.ReadByte("level");
                        mf.Hitpoints = reader.ReadUInt32("life");
                        mf.Type = reader.ReadUInt32("type");
                        mf.helmet_type = reader.ReadUInt32("helmet_type");
                        mf.armor_type = reader.ReadUInt32("armor_type");
                        mf.weaponr_type = reader.ReadUInt32("weaponr_type");
                        mf.weaponl_type = reader.ReadUInt32("weaponl_type");
                        mf.Boss = reader.ReadBoolean("Boss");
                        mf.SuperBoss = reader.ReadBoolean("SuperBoss");
                        //IniFile IniFile = new IniFile(Constants.MonstersPath);
                        //if (IniFile.ReadString(mf.Name, "MaxLife") != "")
                        //{
                        //    if (uint.Parse(IniFile.ReadString(mf.Name, "MaxLife")) != 0)
                        //    {
                        //        mf.Hitpoints = uint.Parse(IniFile.ReadString(mf.Name, "MaxLife"));
                        //        byte boss = byte.Parse(IniFile.ReadString(mf.Name, "Boss"));
                        //        if (boss == 0)
                        //            mf.Boss = false;
                        //        else mf.Boss = true;
                        //    }
                        //}
                        //                        if (mf.Name == "Clannad" ||
                        //mf.Name == "Btooom" || mf.Name == "Cyclops" ||
                        //mf.Name == "Hades" || mf.Name == "Centar" || mf.Name == "CoinsThief" || mf.Name == "CoinsMaster")
                        //                            mf.Boss = true;
                        mf.Guard = mf.Name.Contains("Guard");
                        mf.Reviver = mf.ID == ReviverID;
                        mf.ViewRange = reader.ReadUInt16("view_range");
                        mf.AttackRange = reader.ReadUInt16("attack_range");
                        mf.Defence = reader.ReadUInt16("defence");
                        mf.AttackType = reader.ReadByte("attack_user");
                        mf.MinAttack = reader.ReadUInt32("attack_min");
                        mf.MaxAttack = reader.ReadUInt32("attack_max");
                        mf.SpellID = reader.ReadUInt16("magic_type");
                        //   mf.Switch = reader.ReadByte("magic_soul");
                        mf.MoveSpeed = reader.ReadInt32("move_speed");
                        mf.RunSpeed = reader.ReadInt32("run_speed");
                        //     mf.OwnItemID = reader.ReadInt32("ownitem");
                        mf.HPPotionID = reader.ReadInt32("drop_hp");
                        mf.MPPotionID = reader.ReadInt32("drop_mp");
                        //   mf.OwnItemRate = reader.ReadInt32("ownitemrate");
                        mf.AttackSpeed = reader.ReadInt32("attack_speed");
                        mf.ExtraExperience = reader.ReadUInt32("extra_exp");
                        ulong MoneyDropAmount = reader.ReadUInt16("level");
                        if (MoneyDropAmount != 0)
                        {
                            mf.MaxMoneyDropAmount = MoneyDropAmount * 25;
                            if (mf.MaxMoneyDropAmount != 0)
                                mf.MinMoneyDropAmount = 1;
                        }
                        if (mf.MoveSpeed <= 500)
                            mf.MoveSpeed += 500;
                        if (mf.AttackSpeed <= 500)
                            mf.AttackSpeed += 500;
                        if (mf.ID == 9003)
                        {
                            var x = mf;
                        }

                        MonsterInformations.Add(mf.ID, mf);
                        byte lvl = mf.Level;
                        if (mf.Name == "Slinger" ||
                            mf.Name == "GoldGhost" ||
                            mf.Name == "AgileRat" ||
                            mf.Name == "Bladeling" ||
                            mf.Name == "BlueBird" ||
                            mf.Name == "BlueFiend" ||
                            mf.Name == "MinotaurL120")
                        {
                            mf.LabirinthDrop = true;
                            lvl = 20;
                        }
                    }
                }
            }
            Console.WriteLine("Monster information loaded.");
            //Console.WriteLine("Monster drops generated.");
        }

        public MonsterInformation Copy()
        {
            MonsterInformation mf = new MonsterInformation();
            mf.ID = this.ID;
            mf.Name = this.Name;
            mf.Name2 = this.Name2;
            mf.Mesh = this.Mesh;
            mf.Level = this.Level;
            mf.Hitpoints = this.Hitpoints;
            mf.ViewRange = this.ViewRange;
            mf.AttackRange = this.AttackRange;
            mf.AttackType = this.AttackType;
            mf.MinAttack = this.MinAttack;
            mf.MaxAttack = this.MaxAttack;
            mf.SpellID = this.SpellID;
            mf.MoveSpeed = this.MoveSpeed;
            mf.RunSpeed = this.RunSpeed;
            mf.AttackSpeed = this.AttackSpeed;
            mf.BoundX = this.BoundX;
            mf.BoundY = this.BoundY;
            mf.BoundCX = this.BoundCX;
            mf.BoundCY = this.BoundCY;
            mf.RespawnTime = this.RespawnTime;
            mf.IsRespawnAble = this.IsRespawnAble;
            mf.ExtraExperience = this.ExtraExperience;
            mf.MaxMoneyDropAmount = this.MaxMoneyDropAmount;
            mf.MinMoneyDropAmount = this.MinMoneyDropAmount;
            // mf.OwnItemID = this.OwnItemID;
            mf.HPPotionID = this.HPPotionID;
            mf.MPPotionID = this.MPPotionID;
            //mf.OwnItemRate = this.OwnItemRate;
            mf.LabirinthDrop = this.LabirinthDrop;
            mf.Boss = this.Boss;
            mf.SuperBoss = this.SuperBoss;
            mf.Guard = this.Guard;
            mf.Defence = this.Defence;
            mf.Reviver = this.Reviver;
            return mf;
        }
    }
}
