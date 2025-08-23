using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Database;
using MTA.Network.GamePackets;
using System.IO;
using MTA.Game;
using MTA.Client;

namespace MTA.MaTrix
{
    public class House
    {
        public class HouseInfo
        {
            public uint UID;
            public string Name;
            public ushort ID;
            public ushort maptype;
            public ushort level;
            public Dictionary<uint, SobNpcSpawn> Furnitures;
            public Game.ConquerStructures.Warehouse Warehouse;
        }
        public static SafeDictionary<uint, HouseInfo> Houses = new SafeDictionary<uint, HouseInfo>();        
        public static void LoadHouses()
        {
            try
            {
                MySqlCommand command = new MySqlCommand(MySqlCommandType.SELECT);
                command.Select("house");
                MySqlReader reader = new MySqlReader(command);
                while (reader.Read())
                {
                    HouseInfo info = new HouseInfo();  
                    info.UID = reader.ReadUInt32("UID");
                    info.Name = reader.ReadString("Name");
                    info.ID = reader.ReadUInt16("ID");
                    info.maptype = reader.ReadUInt16("maptype");
                    info.level = reader.ReadUInt16("level");
                    info.Furnitures = new Dictionary<uint, SobNpcSpawn>();
                    byte[] data = reader.ReadBlob("Furnitures");
                    if (data.Length > 0)
                    {
                        using (var stream = new MemoryStream(data))
                        using (var r = new BinaryReader(stream))
                        {
                            int count = r.ReadByte();
                            for (uint x = 0; x < count; x++)
                            {
                                SobNpcSpawn Base = new SobNpcSpawn();
                                Base = ReadItem(r);
                                if ((Base.Mesh / 10) == 820)
                                {
                                    Base.Type = (Enums.NpcType)2;
                                    info.Warehouse = new Game.ConquerStructures.Warehouse(null, (Game.ConquerStructures.Warehouse.WarehouseID)Base.UID);
                                    var items = LOadItems(Base.UID);
                                    foreach (var item in items.Values)
                                    {
                                        if (!info.Warehouse.ContainsUID(item.UID))
                                            info.Warehouse.Add2(item, null);
                                    }
                                }
                                else
                                    Base.Type = (Enums.NpcType)26;
                                Base.MapID = info.ID;
                                if (!info.Furnitures.ContainsKey(Base.UID))
                                    info.Furnitures.Add(Base.UID, Base); 
                              
                            }
                        }
                    }
                    if (!Houses.ContainsKey(info.UID))
                        Houses.Add(info.UID, info);
                    new Map(info.ID, info.maptype, Kernel.Maps[info.maptype].Path);

                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Program.SaveException(exception);
            }
        }
        ///////////////////////////////////////////////////
        public static void WriteItem(BinaryWriter writer, SobNpcSpawn Base)
        {
            writer.Write(Base.UID);
            writer.Write(Base.Mesh);
            writer.Write(Base.X);
            writer.Write(Base.Y);
        }
        public static SobNpcSpawn ReadItem(BinaryReader reader)
        {
            SobNpcSpawn Base = new SobNpcSpawn();
            Base.UID = reader.ReadUInt32();//8
            Base.Mesh = reader.ReadUInt16();//8
            Base.X = reader.ReadUInt16();//10
            Base.Y = reader.ReadUInt16();//12
            return Base;
        }
        ///////////////////////////////////////////////////  
        public static void SaveFurnitures(Client.GameState client)
        {
            if (!Houses.ContainsKey(client.Entity.UID))
                return;
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            var info = Houses[client.Entity.UID];
            writer.Write((byte)info.Furnitures.Count);
            foreach (var fur in info.Furnitures.Values)
                WriteItem(writer, fur);
            string SQL = "UPDATE `house` SET Furnitures=@Furnitures where UID = " + client.Entity.UID + " ;";
            byte[] rawData = stream.ToArray();
            using (var conn = DataHolder.MySqlConnection)
            {
                conn.Open();
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = SQL;
                    cmd.Parameters.AddWithValue("@Furnitures", rawData);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        ///////////////////////////////////////////////////
        public static void createhouse(GameState client)
        {            
            HouseInfo info = new HouseInfo();  
            info.UID = client.Entity.UID;
            info.Name = client.Entity.Name;
            info.ID = (ushort)client.Entity.UID;
            info.maptype = 1098;
            info.level = 1;
            info.Furnitures = new Dictionary<uint, SobNpcSpawn>();
            if (!Houses.ContainsKey(info.UID))
                Houses.Add(info.UID, info);
            new Map(info.ID, info.maptype, Kernel.Maps[info.maptype].Path);

            MySqlCommand command = new MySqlCommand(MySqlCommandType.INSERT);
            command.Insert("house").Insert("UID", client.Entity.UID)
                 .Insert("maptype", info.maptype).Insert("level", info.level)
                .Insert("Name", client.Entity.Name).Insert("ID", (ushort)client.Entity.UID);
            command.Execute();

        }
        public static void UpgradeHouse(GameState client, byte level)
        {
            ushort _base = 1098;
            if (level == 1)
                _base = 1099;
            if (level == 2)
                _base = 2080;
            if (level == 3)
                _base = 1765;
            if (level == 4)
                _base = 3024;
           
            level++;
            if (level > 5)
                return;

            new MySqlCommand(MySqlCommandType.UPDATE).Update("house")
                .Set("Name", client.Entity.Name).Set("ID", (ushort)client.Entity.UID)
                .Set("maptype", _base).Set("level", 0).Where("UID", client.Entity.UID).Execute();
            if (Kernel.Maps.ContainsKey((ushort)client.Entity.UID))
            {
                Kernel.Maps.Remove((ushort)client.Entity.UID);
                new Map((ushort)client.Entity.UID, _base, Kernel.Maps[_base].Path);
            }
            if (Houses.ContainsKey(client.Entity.UID))
            {
                Houses[client.Entity.UID].maptype = _base;
                Houses[client.Entity.UID].level = level;
           //     Houses[client.Entity.UID].Furnitures = new Dictionary<uint, SobNpcSpawn>();
                SaveFurnitures(client);
            }
        }
               
        public static void TelePort(GameState client, HouseInfo info)
        {
            if (client.Entity.EntityFlag == EntityFlag.Player)
            {
                client.Entity.AdvancedTeleport(true);
                ushort MapID;
                ushort X, Y;
                MapID = info.ID;
                var cord = Kernel.Maps[info.maptype].RandomCoordinates();
                X = cord.Item1;
                Y = cord.Item2;
                if (!Houses.ContainsKey(info.UID) && client.QualifierGroup == null)
                {
                    MapID = 1002;
                    X = 432;
                    Y = 378;
                }
                if (client.Entity.EntityFlag == EntityFlag.Player)
                {
                    if (client.InQualifier())
                        if (client.InQualifier())
                            if (client.Entity.MapID != 700 && client.Entity.MapID < 11000)
                            client.EndQualifier();
                }
               
                client.Entity.X = X;
                client.Entity.Y = Y;
                client.Entity.PX = 0;
                client.Entity.PY = 0;
                client.Entity.PreviousMapID = client.Entity.MapID;
                client.Entity.MapID = info.ID;
                
                Network.GamePackets.Data Data = new Network.GamePackets.Data(true);
                Data.UID = client.Entity.UID;
                Data.ID = Network.GamePackets.Data.Teleport;
                Data.dwParam = info.maptype;
                Data.wParam1 = X;
                Data.wParam2 = Y;
                client.Send(Data);
                client.Send(new MapStatus() { BaseID = info.maptype, ID = info.ID });
                client.Entity.AdvancedTeleport(true);
            }
        }
        public static void HouseWarehouse(GameState client, Warehouse warehousepacket = null)
        {
            if (client != null)
            {
                if (Houses.ContainsKey(client.Entity.UID))
                {
                    var info = Houses[client.Entity.UID];
                    var itembox = info.Furnitures.Values.Where(xx => (xx.Mesh / 10) == 820).FirstOrDefault();
                    if (itembox != null)
                    {
                        if (!client.Warehouses.ContainsKey((Game.ConquerStructures.Warehouse.WarehouseID)itembox.UID))
                        {
                            if (info.Warehouse == null)
                                info.Warehouse = new Game.ConquerStructures.Warehouse(null, (Game.ConquerStructures.Warehouse.WarehouseID)itembox.UID);                                 
                            client.Warehouses.Add((Game.ConquerStructures.Warehouse.WarehouseID)itembox.UID, info.Warehouse);
                        }
                    }
                }
            }
        }
        public static SafeDictionary<uint, ConquerItem> LOadItems(uint Warehouse)
        {
            SafeDictionary<uint, ConquerItem> Items = new SafeDictionary<uint, ConquerItem>();
            using (var cmdx = new MySqlCommand(MySqlCommandType.SELECT).Select("items").Where("Warehouse", Warehouse))
            using (var readerx = new MySqlReader(cmdx))
            {
                while (readerx.Read())
                {
                    var item = ConquerItemTable.deserialzeItem(readerx);
                    if (!Items.ContainsKey(item.UID))
                        Items.Add(item.UID, item);
                }
            }
            return Items;
        }

        private static ConquerItem deserialzeItem(MySqlReader reader)
        {
            ConquerItem item = new Network.GamePackets.ConquerItem(true);
            item.ID = reader.ReadUInt32("Id");
            item.UID = reader.ReadUInt32("Uid");
            //item.Durability = reader.ReadUInt16("Durability");
            item.MaximDurability = reader.ReadUInt16("MaximDurability");
            item.Durability = item.MaximDurability;
            item.Position = reader.ReadUInt16("Position");
            item.Agate = reader.ReadString("Agate");
            item.SocketProgress = reader.ReadUInt32("SocketProgress");
            item.PlusProgress = reader.ReadUInt32("PlusProgress");
            item.SocketOne = (Game.Enums.Gem)reader.ReadUInt16("SocketOne");
            item.SocketTwo = (Game.Enums.Gem)reader.ReadUInt16("SocketTwo");
            item.Effect = (Game.Enums.ItemEffect)reader.ReadUInt16("Effect");
            item.Mode = Game.Enums.ItemMode.Default;
            item.Plus = reader.ReadByte("Plus");
            item.Bless = reader.ReadByte("Bless");
            item.Bound = reader.ReadBoolean("Bound");
            item.Enchant = reader.ReadByte("Enchant");
            item.Lock = reader.ReadByte("Locked");
            item.UnlockEnd = DateTime.FromBinary(reader.ReadInt64("UnlockEnd"));
            item.Suspicious = reader.ReadBoolean("Suspicious");
            item.SuspiciousStart = DateTime.FromBinary(reader.ReadInt64("SuspiciousStart"));
            item.Color = (Game.Enums.Color)reader.ReadUInt32("Color");
            item.Warehouse = reader.ReadUInt16("Warehouse");
            item.StackSize = reader.ReadUInt16("StackSize");
            item.RefineItem = reader.ReadUInt32("RefineryItem");
            Int64 rTime = reader.ReadInt64("RefineryTime");

            if (item.ID == 300000)
            {
                uint NextSteedColor = reader.ReadUInt32("NextSteedColor");
                item.NextGreen = (byte)(NextSteedColor & 0xFF);
                item.NextBlue = (byte)((NextSteedColor >> 8) & 0xFF);
                item.NextRed = (byte)((NextSteedColor >> 16) & 0xFF);
            }
            if (item.RefineItem > 0 && rTime != 0)
            {
                item.RefineryTime = DateTime.FromBinary(rTime);
                if (DateTime.Now > item.RefineryTime)
                {
                    item.RefineryTime = new DateTime(0);
                    item.RefineItem = 0;
                }
            }
            if (item.Lock == 2)
                if (DateTime.Now >= item.UnlockEnd)
                    item.Lock = 0;

            item.DayStamp = DateTime.FromBinary(reader.ReadInt64("DayStamp"));
            item.Days = reader.ReadByte("Days");
            return item;
        }

        public static HouseInfo SpouseHouse(string Spousename)
        {
            foreach (var house in Houses.Values)
                if (house.Name == Spousename)
                    return house;
            return null;
        }

        public static bool SpouseWarehouse(GameState client, Warehouse warehousepacket)
        {
            HouseWarehouse(client, warehousepacket);            
            var info = MaTrix.House.SpouseHouse(client.Entity.Spouse);
            if (info == null || client.Entity.MapID == client.Entity.UID)
                info = Houses[client.Entity.UID];
            if (info != null)
            {
                if (client.Entity.MapID == info.ID)
                {
                    switch (warehousepacket.Type)
                    {
                        case Warehouse.Entire:
                            {
                                Game.ConquerStructures.Warehouse wh = info.Warehouse;
                                if (wh == null) return true;
                                byte count = 0;
                                warehousepacket.Count = 1;
                                warehousepacket.Type = Warehouse.AddItem;
                                for (; count < wh.Count; count++)
                                {
                                    warehousepacket.Append(wh.Objects[count]);
                                    client.Send(warehousepacket);
                                    ItemAdding add = new ItemAdding(true);
                                    if (wh.Objects[count].Purification.Available)
                                        add.Append(wh.Objects[count].Purification);
                                    if (wh.Objects[count].ExtraEffect.Available)
                                        add.Append(wh.Objects[count].ExtraEffect);
                                    if (wh.Objects[count].Purification.Available || wh.Objects[count].ExtraEffect.Available)
                                        client.Send(add);

                                }
                                return true;
                            }
                        case Warehouse.AddItem:
                            {
                                Game.ConquerStructures.Warehouse wh = info.Warehouse;
                                if (wh == null) return true;
                                ConquerItem item = null;
                                if (client.Inventory.TryGetItem(warehousepacket.UID, out item))
                                {
                                    if (item.ID >= 729960 && item.ID <= 729970)
                                        return true;
                                    if (item.ID == 729611 || item.ID == 729612 || item.ID == 729613 || item.ID == 729614 || item.ID == 729703)
                                        return true;
                                    if (!ConquerItem.isRune(item.UID))
                                    {
                                        if (wh.Add2(item, client))
                                        {
                                            warehousepacket.UID = 0;
                                            warehousepacket.Count = 1;
                                            warehousepacket.Append(item);
                                            client.Send(warehousepacket);

                                            ItemAdding add = new ItemAdding(true);
                                            if (item.Purification.Available)
                                                add.Append(item.Purification);
                                            if (item.ExtraEffect.Available)
                                                add.Append(item.ExtraEffect);
                                            if (item.Purification.Available || item.ExtraEffect.Available)
                                                client.Send(add);

                                            info.Warehouse = wh;
                                            return true;
                                        }
                                    }
                                    else client.Send(new Message("You can not store Flame Stone Rune's in Warehouse", System.Drawing.Color.Red, Message.TopLeft));
                                }
                                break;
                            }
                        case Warehouse.RemoveItem:
                            {
                                if (!client.Partners.ContainsKey(info.UID) && client.Entity.UID != info.UID)
                                {
                                    client.Send(new Message("Sorry you cant, You Should be a Trade Partner.", Message.TopLeft));
                                    return true;
                                }
                                Game.ConquerStructures.Warehouse wh = info.Warehouse;
                                if (wh == null) return true;
                                if (wh.ContainsUID(warehousepacket.UID))
                                {
                                    if (wh.Remove2(warehousepacket.UID, client))
                                    {
                                        info.Warehouse = wh;
                                        client.Send(warehousepacket);
                                        return true;
                                    }
                                }
                                break;
                            }

                    }
                
                
                }
            }
            return false;
        }

        public static SobNpcSpawn CheckItemBox(GameState client, HouseInfo info)
        {
            return info.Furnitures.Values.Where(xx => (xx.Mesh / 10) == 820).FirstOrDefault();

        }

        public static void Move(GameState client, SobNpcSpawn sobnpc, HouseInfo info)
        {
            client.MessageBox("Do u Want To change its place?", (p) =>
            {
                info.Furnitures.Remove(sobnpc.UID);
                p.Screen.FullWipe();
                p.Screen.Reload();
                NpcRequest req2 = new NpcRequest(5);
                req2.Mesh = sobnpc.Mesh;
                req2.NpcTyp = sobnpc.Type;
                p.Send(req2);
            }, null);
        }
    }
}
