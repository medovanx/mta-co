using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using Warehouse = MTA.Game.ConquerStructures.Warehouse;

namespace MTA.Game.Features.House {
    public static class House {
        public class HouseInfo {
            public uint Uid;
            public string? Name;
            public ushort Id;
            public ushort Maptype;
            public ushort Level;
            public Dictionary<uint, SobNpcSpawn>? Furniture;
            public Warehouse? Warehouse;
        }

        public static SafeDictionary<uint, HouseInfo> Houses = [];

        public static void LoadHouses() {
            try {
                MySqlCommand command = new(MySqlCommandType.SELECT);
                command.Select("house");
                MySqlReader reader = new(command);
                while (reader.Read()) {
                    HouseInfo info = new() {
                        Uid = reader.ReadUInt32("UID"),
                        Name = reader.ReadString("Name"),
                        Id = reader.ReadUInt16("ID"),
                        Maptype = reader.ReadUInt16("maptype"),
                        Level = reader.ReadUInt16("level"),
                        Furniture = []
                    };
                    var data = reader.ReadBlob("Furnitures");
                    if (data.Length > 0) {
                        using var stream = new MemoryStream(data);
                        using var r = new BinaryReader(stream);
                        int count = r.ReadByte();
                        for (uint x = 0; x < count; x++) {
                            var @base = ReadItem(r);
                            if ((@base.Mesh / 10) == 820) {
                                @base.Type = (Enums.NpcType)2;
                                info.Warehouse = new Warehouse(null, (Warehouse.WarehouseID)@base.UID);
                                var items = LoadItems(@base.UID);
                                foreach (var item in items.Values.Where(item =>
                                             !info.Warehouse.ContainsUID(item.UID))) {
                                    info.Warehouse.Add2(item, null);
                                }
                            }
                            else {
                                @base.Type = (Enums.NpcType)26;
                            }

                            @base.MapID = info.Id;
                            info.Furniture.TryAdd(@base.UID, @base);
                        }
                    }

                    if (!Houses.ContainsKey(info.Uid))
                        Houses.Add(info.Uid, info);
                    _ = new Map(info.Id, info.Maptype, Kernel.Maps[info.Maptype].Path);
                }
            }
            catch (Exception exception) {
                Console.WriteLine(exception);
                Program.SaveException(exception);
            }
        }

        ///////////////////////////////////////////////////
        public static void WriteItem(BinaryWriter writer, SobNpcSpawn @base) {
            writer.Write(@base.UID);
            writer.Write(@base.Mesh);
            writer.Write(@base.X);
            writer.Write(@base.Y);
        }

        public static SobNpcSpawn ReadItem(BinaryReader reader) {
            SobNpcSpawn @base = new() {
                UID = reader.ReadUInt32(), //8
                Mesh = reader.ReadUInt16(), //8
                X = reader.ReadUInt16(), //10
                Y = reader.ReadUInt16() //12
            };
            return @base;
        }

        ///////////////////////////////////////////////////  
        public static void SaveFurniture(GameState client) {
            if (!Houses.TryGetValue(client.Entity.UID, out var info))
                return;
            MemoryStream stream = new();
            BinaryWriter writer = new(stream);
            writer.Write(value: (byte)(info.Furniture?.Count ?? 0));
            if (info.Furniture != null)
                foreach (var fur in info.Furniture.Values) {
                    WriteItem(writer, fur);
                }

            var sql = "UPDATE `house` SET Furnitures=@Furnitures where UID = " + client.Entity.UID + " ;";
            var rawData = stream.ToArray();
            using var conn = DataHolder.MySqlConnection;
            conn.Open();
            using var cmd = new MySql.Data.MySqlClient.MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@Furnitures", rawData);
            cmd.ExecuteNonQuery();
        }

        ///////////////////////////////////////////////////
        public static void CreateHouse(GameState client) {
            HouseInfo info = new() {
                Uid = client.Entity.UID,
                Name = client.Entity.Name,
                Id = (ushort)client.Entity.UID,
                Maptype = 1098,
                Level = 1,
                Furniture = []
            };
            if (!Houses.ContainsKey(info.Uid))
                Houses.Add(info.Uid, info);
            _ = new Map(info.Id, info.Maptype, Kernel.Maps[info.Maptype].Path);

            MySqlCommand command = new(MySqlCommandType.INSERT);
            command.Insert("house").Insert("UID", client.Entity.UID)
                .Insert("maptype", info.Maptype).Insert("level", info.Level)
                .Insert("Name", client.Entity.Name).Insert("ID", (ushort)client.Entity.UID);
            command.Execute();
        }

        public static void UpgradeHouse(GameState client, byte level) {
            ushort @base = level switch {
                1 => 1099,
                2 => 2080,
                3 => 1765,
                4 => 3024,
                _ => 1098
            };

            level++;
            if (level > 5)
                return;

            new MySqlCommand(MySqlCommandType.UPDATE).Update("house")
                .Set("Name", client.Entity.Name).Set("ID", (ushort)client.Entity.UID)
                .Set("maptype", @base).Set("level", level).Where("UID", client.Entity.UID).Execute();
            if (Kernel.Maps.ContainsKey((ushort)client.Entity.UID)) {
                Kernel.Maps.Remove((ushort)client.Entity.UID);
                _ = new Map((ushort)client.Entity.UID, @base, Kernel.Maps[@base].Path);
            }

            if (!Houses.ContainsKey(client.Entity.UID)) return;
            Houses[client.Entity.UID].Maptype = @base;
            Houses[client.Entity.UID].Level = level;
            //     Houses[client.Entity.UID].Furnitures = new Dictionary<uint, SobNpcSpawn>();
            SaveFurniture(client);
        }

        public static void DowngradeHouse(GameState client, byte currentLevel) {
            if (currentLevel <= 1)
                return; // Cannot downgrade below level 1

            var newLevel = (byte)(currentLevel - 1);

            ushort @base = newLevel switch {
                // Determine maptype based on the new level
                1 => 1098,
                2 => 1099,
                3 => 2080,
                4 => 1765,
                _ => 1098
            };

            new MySqlCommand(MySqlCommandType.UPDATE).Update("house")
                .Set("Name", client.Entity.Name).Set("ID", (ushort)client.Entity.UID)
                .Set("maptype", @base).Set("level", newLevel).Where("UID", client.Entity.UID).Execute();

            if (Kernel.Maps.ContainsKey((ushort)client.Entity.UID)) {
                Kernel.Maps.Remove((ushort)client.Entity.UID);
                _ = new Map((ushort)client.Entity.UID, @base, Kernel.Maps[@base].Path);
            }

            if (!Houses.TryGetValue(client.Entity.UID, out var value)) return;
            value.Maptype = @base;
            value.Level = newLevel;
            SaveFurniture(client);
        }

        public static void Teleport(GameState client, HouseInfo info) {
            client.Entity.AdvancedTeleport(true);
            var (x, y) = Kernel.Maps[info.Maptype].RandomCoordinates();
            if (client.Entity.EntityFlag == EntityFlag.Player)
                if (client.InQualifier())
                    if (client.InQualifier())
                        if (client.Entity.MapID != 700 && client.Entity.MapID < 11000)
                            client.EndQualifier();

            client.Entity.X = x;
            client.Entity.Y = y;
            client.Entity.PX = 0;
            client.Entity.PY = 0;
            client.Entity.PreviousMapID = client.Entity.MapID;
            client.Entity.MapID = info.Id;

            Data data = new(true) {
                UID = client.Entity.UID,
                ID = Data.Teleport,
                dwParam = info.Maptype,
                wParam1 = x,
                wParam2 = y
            };
            client.Send(data);
            client.Send(new MapStatus() { BaseID = info.Maptype, ID = info.Id });
            client.Entity.AdvancedTeleport(true);
        }

        public static void HouseWarehouse(GameState client, Network.GamePackets.Warehouse? warehousePacket = null) {
            if (!Houses.TryGetValue(client.Entity.UID, out var info)) return;
            var itemBox = info.Furniture?.Values.FirstOrDefault(xx => (xx.Mesh / 10) == 820);
            if (itemBox == null) return;
            if (client.Warehouses.ContainsKey((Warehouse.WarehouseID)itemBox.UID)) return;
            info.Warehouse ??= new Warehouse(null, (Warehouse.WarehouseID)itemBox.UID);
            client.Warehouses.Add((Warehouse.WarehouseID)itemBox.UID, info.Warehouse);
        }

        public static SafeDictionary<uint, ConquerItem> LoadItems(uint warehouse) {
            SafeDictionary<uint, ConquerItem> items = [];
            using var mySqlCommand =
                new MySqlCommand(MySqlCommandType.SELECT).Select("items").Where("Warehouse", warehouse);
            using var mySqlReader = new MySqlReader(mySqlCommand);
            while (mySqlReader.Read()) {
                var item = ConquerItemTable.deserialzeItem(mySqlReader);
                if (!items.ContainsKey(item.UID))
                    items.Add(item.UID, item);
            }

            return items;
        }

        private static ConquerItem DeserialzeItem(MySqlReader reader) {
            ConquerItem item = new(true) {
                ID = reader.ReadUInt32("Id"),
                UID = reader.ReadUInt32("Uid"),
                //item.Durability = reader.ReadUInt16("Durability");
                MaximDurability = reader.ReadUInt16("MaximDurability")
            };
            item.Durability = item.MaximDurability;
            item.Position = reader.ReadUInt16("Position");
            item.Agate = reader.ReadString("Agate");
            item.SocketProgress = reader.ReadUInt32("SocketProgress");
            item.PlusProgress = reader.ReadUInt32("PlusProgress");
            item.SocketOne = (Enums.Gem)reader.ReadUInt16("SocketOne");
            item.SocketTwo = (Enums.Gem)reader.ReadUInt16("SocketTwo");
            item.Effect = (Enums.ItemEffect)reader.ReadUInt16("Effect");
            item.Mode = Enums.ItemMode.Default;
            item.Plus = reader.ReadByte("Plus");
            item.Bless = reader.ReadByte("Bless");
            item.Bound = reader.ReadBoolean("Bound");
            item.Enchant = reader.ReadByte("Enchant");
            item.Lock = reader.ReadByte("Locked");
            item.UnlockEnd = DateTime.FromBinary(reader.ReadInt64("UnlockEnd"));
            item.Suspicious = reader.ReadBoolean("Suspicious");
            item.SuspiciousStart = DateTime.FromBinary(reader.ReadInt64("SuspiciousStart"));
            item.Color = (Enums.Color)reader.ReadUInt32("Color");
            item.Warehouse = reader.ReadUInt16("Warehouse");
            item.StackSize = reader.ReadUInt16("StackSize");
            item.RefineItem = reader.ReadUInt32("RefineryItem");
            var rTime = reader.ReadInt64("RefineryTime");

            if (item.ID == 300000) {
                var nextSteedColor = reader.ReadUInt32("NextSteedColor");
                item.NextGreen = (byte)(nextSteedColor & 0xFF);
                item.NextBlue = (byte)((nextSteedColor >> 8) & 0xFF);
                item.NextRed = (byte)((nextSteedColor >> 16) & 0xFF);
            }

            if (item.RefineItem > 0 && rTime != 0) {
                item.RefineryTime = DateTime.FromBinary(rTime);
                if (DateTime.Now > item.RefineryTime) {
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

        public static HouseInfo? SpouseHouse(string spouseName) {
            return Houses.Values.FirstOrDefault(house => house.Name == spouseName);
        }

        public static bool SpouseWarehouse(GameState client, Network.GamePackets.Warehouse warehousePacket) {
            HouseWarehouse(client, warehousePacket);
            var info = SpouseHouse(client.Entity.Spouse);
            if (info == null || client.Entity.MapID == client.Entity.UID)
                info = Houses[client.Entity.UID];
            if (client.Entity.MapID != info.Id) return false;
            switch (warehousePacket.Type) {
                case Network.GamePackets.Warehouse.Entire: {
                    var wh = info.Warehouse;
                    if (wh == null) return true;
                    byte count = 0;
                    warehousePacket.Count = 1;
                    warehousePacket.Type = Network.GamePackets.Warehouse.AddItem;
                    for (; count < wh.Count; count++) {
                        warehousePacket.Append(wh.Objects[count]);
                        client.Send(warehousePacket);
                        var add = new ItemAdding(true);
                        if (wh.Objects[count].Purification.Available)
                            add.Append(wh.Objects[count].Purification);
                        if (wh.Objects[count].ExtraEffect.Available)
                            add.Append(wh.Objects[count].ExtraEffect);
                        if (wh.Objects[count].Purification.Available || wh.Objects[count].ExtraEffect.Available)
                            client.Send(add);
                    }

                    return true;
                }
                case Network.GamePackets.Warehouse.AddItem: {
                    var wh = info.Warehouse;
                    if (wh == null) return true;
                    if (client.Inventory.TryGetItem(warehousePacket.UID, out var item)) {
                        switch (item.ID) {
                            case >= 729960 and <= 729970:
                            case 729611 or 729612 or 729613 or 729614 or 729703:
                                return true;
                        }

                        if (!ConquerItem.isRune(item.UID)) {
                            if (wh.Add2(item, client)) {
                                warehousePacket.UID = 0;
                                warehousePacket.Count = 1;
                                warehousePacket.Append(item);
                                client.Send(warehousePacket);

                                var add = new ItemAdding(true);
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
                        else {
                            client.Send(new Message("You can not store Flame Stone Rune's in Warehouse",
                                System.Drawing.Color.Red, Message.TopLeft));
                        }
                    }

                    break;
                }
                case Network.GamePackets.Warehouse.RemoveItem: {
                    if (!client.Partners.ContainsKey(info.Uid) && client.Entity.UID != info.Uid) {
                        client.Send(new Message("Sorry you cant, You Should be a Trade Partner.",
                            Message.TopLeft));
                        return true;
                    }

                    var wh = info.Warehouse;
                    if (wh == null) return true;
                    if (wh.ContainsUID(warehousePacket.UID))
                        if (wh.Remove2(warehousePacket.UID, client)) {
                            info.Warehouse = wh;
                            client.Send(warehousePacket);
                            return true;
                        }

                    break;
                }
            }

            return false;
        }

        public static SobNpcSpawn? CheckItemBox(GameState client, HouseInfo info) {
            return info.Furniture?.Values.FirstOrDefault(xx => (xx.Mesh / 10) == 820);
        }

        public static void Move(GameState client, SobNpcSpawn sobNpc, HouseInfo info) {
            client.MessageBox("Do u Want To change its place?", (p) => {
                info.Furniture?.Remove(sobNpc.UID);
                p.Screen.FullWipe();
                p.Screen.Reload();
                NpcRequest req2 = new(5) {
                    Mesh = sobNpc.Mesh,
                    NpcTyp = sobNpc.Type
                };
                p.Send(req2);
            });
        }
    }
}