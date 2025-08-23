using System;
using System.Linq;
using MTA.Network.GamePackets;
using System.Collections.Generic;

namespace MTA.Database
{
    public class ConquerItemTable
    {

        #region Blob
        #region Main

        public Network.GamePackets.ConquerItem ReadItem(System.IO.BinaryReader reader)
        {
            Network.GamePackets.ConquerItem item = new ConquerItem(true);
            item.ID = reader.ReadUInt32();//4
            item.UID = reader.ReadUInt32();//8
            item.Durability = reader.ReadUInt16();//10
            item.MaximDurability = reader.ReadUInt16();//12
            item.Position = reader.ReadUInt16();//14
            item.SocketProgress = reader.ReadUInt32();//18
            item.PlusProgress = reader.ReadUInt32();//22
            item.SocketOne = (Game.Enums.Gem)reader.ReadUInt16();//24
            item.SocketTwo = (Game.Enums.Gem)reader.ReadUInt16();//26
            item.Effect = (Game.Enums.ItemEffect)reader.ReadUInt16();//28
            item.Mode = Game.Enums.ItemMode.Default;
            item.Plus = reader.ReadByte();//29
            item.Bless = reader.ReadByte();//30
            item.Bound = reader.ReadBoolean();//31
            item.Enchant = reader.ReadByte();//32
            item.Lock = reader.ReadByte();//33

            item.UnlockEnd = DateTime.FromBinary(reader.ReadInt64());//41
            item.Suspicious = reader.ReadBoolean();//42
            item.SuspiciousStart = DateTime.FromBinary(reader.ReadInt64());//50

            item.Color = (Game.Enums.Color)reader.ReadUInt32();//54
            if ((byte)item.Color > 9 || (byte)item.Color < 2)
                item.Color = (Game.Enums.Color)Kernel.Random.Next(2, 8);
            item.Warehouse = reader.ReadUInt16();//56
            item.StackSize = reader.ReadUInt16();//58


            if (item.Lock == 2)
                if (DateTime.Now >= item.UnlockEnd)
                    item.Lock = 0;

            return item;
        }

        #endregion

        #region other

        public void WriteItem(System.IO.BinaryWriter writer, Network.GamePackets.ConquerItem item)
        {
            writer.Write(item.ID); //= reader.ReadUInt32();
            writer.Write(item.UID);
            writer.Write(item.Durability);
            writer.Write(item.MaximDurability);
            writer.Write(item.Position);
            writer.Write(item.SocketProgress);
            writer.Write(item.PlusProgress);
            writer.Write((ushort)item.SocketOne);
            writer.Write((ushort)item.SocketTwo);
            writer.Write((ushort)item.Effect);
            writer.Write(item.Plus);
            writer.Write(item.Bless);
            writer.Write(item.Bound);
            writer.Write(item.Enchant);
            writer.Write(item.Lock);
            writer.Write(item.UnlockEnd.Ticks);
            writer.Write(item.Suspicious);
            writer.Write(item.SuspiciousStart.Ticks);
            writer.Write((uint)item.Color);
            writer.Write(item.Warehouse);
            writer.Write(item.StackSize);
            // writer.Write((uint)(item.NextGreen | (item.NextBlue << 8) | (item.NextRed << 16)));
        }
        public static byte[] GetItemsAraay(Client.GameState client)
        {
            uint count = (uint)(client.Inventory.Count + client.Equipment.Count);
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream);
            writer.Write(count);
            foreach (var item in client.Inventory.Objects)
                new ConquerItemTable().WriteItem(writer, item);
            foreach (var item in client.Equipment.GetCollection())
                new ConquerItemTable().WriteItem(writer, item);
            return stream.ToArray();
        }
        #endregion

        #endregion
        public static void LoadItems(Client.GameState client)
        {
            client.Entity.StorageItems = new Dictionary<uint, ConquerItem>();
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("items").Where("EntityID", client.Entity.UID))
            using (var reader = new MySqlReader(cmd))
            {
                while (reader.Read())
                {
                    var item = deserialzeItem(reader);
                    if (!ConquerItemInformation.BaseInformations.ContainsKey(item.ID))
                        continue;
                    HandleInscribing(item, client);
                    ItemAddingTable.GetAddingsForItem(item);
                    if ((byte)Network.PacketHandler.GetPositionFromID(item.ID) == ConquerItem.Garment || (byte)Network.PacketHandler.GetPositionFromID(item.UID) == ConquerItem.SteedCrop || (byte)Network.PacketHandler.GetPositionFromID(item.UID) == ConquerItem.SteedArmor || (byte)Network.PacketHandler.GetPositionFromID(item.UID) == ConquerItem.Bottle)
                    {
                        if (item.SocketOne != Game.Enums.Gem.NoSocket || item.SocketTwo != Game.Enums.Gem.NoSocket)
                        {
                            item.SocketOne = Game.Enums.Gem.NoSocket;
                            item.SocketTwo = Game.Enums.Gem.NoSocket;
                        }

                    }
                    #region WareHouse
                    if (item.Warehouse == 0)
                    {
                        switch (item.Position)
                        {
                            case 0:
                                {
                                    if (item.InWardrobe)
                                    {
                                        client.Entity.StorageItems.Add(item.UID, item);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(item, Game.Enums.ItemUse.None); ;
                                    }
                                }
                                break;
                            default:
                                if (item.Position > 29) continue;
                                if (client.Equipment.Free((byte)item.Position))
                                    client.Equipment.Add(item, Game.Enums.ItemUse.None);
                                else
                                {
                                    if (client.Inventory.Count < 50)
                                    {
                                        if (item.InWardrobe)
                                        {

                                        }
                                        else
                                        {
                                            item.Position = 0;
                                            client.Inventory.Add(item, Game.Enums.ItemUse.None);
                                            if (client.Warehouses[MTA.Game.ConquerStructures.Warehouse.WarehouseID.Market].Count < 20)
                                                client.Warehouses[MTA.Game.ConquerStructures.Warehouse.WarehouseID.Market].Add(item);
                                            UpdatePosition(item);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (item != null)
                        {

                            MTA.Game.ConquerStructures.Warehouse.WarehouseID whID = (Game.ConquerStructures.Warehouse.WarehouseID)item.Warehouse;
                            if (client != null)
                            {
                                if (client.Warehouses != null)
                                {
                                    if (client.Warehouses.ContainsKey(whID))
                                    {
                                        client.Warehouses[whID].Add(item);
                                    }
                                    else
                                    {
                                        using (var cmdx = new MySqlCommand(MySqlCommandType.SELECT).Select("items").Where("Uid", (uint)item.Warehouse))
                                        using (var readerx = new MySqlReader(cmdx))
                                            if (readerx.Read())
                                            {
                                                client.Warehouses.Add((MTA.Game.ConquerStructures.Warehouse.WarehouseID)(uint)item.Warehouse, new MTA.Game.ConquerStructures.Warehouse(client, (MTA.Game.ConquerStructures.Warehouse.WarehouseID)(uint)item.Warehouse));
                                                client.Warehouses[(MTA.Game.ConquerStructures.Warehouse.WarehouseID)(uint)whID].Add(item);
                                            }
                                    }
                                }
                            }
                        }
                    }
                    #endregion WareHouse
                    if (item.ID == 720828)
                    {
                        string str = reader.ReadString("agate");
                        uint key = 0;
                        string[] strArray = str.Split(new char[] { '#' });
                        foreach (string str2 in strArray)
                        {
                            if (str2.Length > 6)
                            {
                                item.Agate_map.Add(key, str2);
                                key++;
                            }
                        }
                    }
                }
            }
        }

        public static ConquerItem deserialzeItem(MySqlReader reader)
        {
            ConquerItem item = new Network.GamePackets.ConquerItem(true);
            item.ID = reader.ReadUInt32("Id");
            item.UID = reader.ReadUInt32("Uid");
            item.Durability = reader.ReadUInt16("Durability");
            item.MaximDurability = reader.ReadUInt16("MaximDurability");
            //  item.Durability = item.MaximDurability;
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
            item.Warehouse = reader.ReadUInt32("Warehouse");
            item.Perfectionlevel = reader.ReadByte("Perfectionlevel");
            item.PerfectionProgress = reader.ReadUInt16("PerfectionProgress");
            item.OwnerID = reader.ReadUInt32("OwnerID");
            item.OwnerName = reader.ReadString("OwnerName");
            item.Signature = reader.ReadString("Signature");
            item.StackSize = reader.ReadUInt16("StackSize");
            item.RefineItem = reader.ReadUInt32("RefineryItem");
            item.InWardrobe = reader.ReadBoolean("InWardrobe");
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

        public static void UpdateDurabilityItem2(ConquerItem Item, uint ItemID)
        {
            if (Item != null)
            {
                using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                    cmd.Update("items").Set("Durability", (long)Item.Durability).Where("UID", (long)Item.UID).And("ID", (long)ItemID).Execute();
            }
        }

        public static void SetDurabilityItem0(ConquerItem Item)
        {
            using (MySql.Data.MySqlClient.MySqlConnection connection = DataHolder.MySqlConnection)
            {
                connection.Open();
                using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                    cmd.Update("items").Set("Durability", (long)0L).Where("UID", (long)Item.UID).Execute(connection);
                connection.Close();
            }
        }

        public static void HandleInscribing(ConquerItem item, Client.GameState client, bool detained = false)
        {
            if (client.Entity.GuildID != 0)
            {
                if (client.Guild != null)
                {
                    int itemPosition = Network.PacketHandler.ArsenalPosition(item.ID);
                    if (itemPosition != -1)
                    {
                        var arsenal = client.Guild.Arsenals[itemPosition];
                        if (arsenal.Unlocked)
                        {
                            if (arsenal.ItemDictionary.ContainsKey(item.UID))
                            {
                                var arsenalItem = arsenal.ItemDictionary[item.UID];
                                arsenalItem.Update(item, client);
                                item.Inscribed = true;
                                client.ArsenalDonations[itemPosition] += arsenalItem.DonationWorth;
                            }
                        }
                    }
                }
            }
        }
        public static ConquerItem LoadItem(uint UID)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("items").Where("UID", UID))
            using (var reader = new MySqlReader(cmd))
                if (reader.Read())
                    return deserialzeItem(reader);
            return new ConquerItem(true);
        }

        public static void AddItem(ref ConquerItem Item, Client.GameState client)
        {
            try
            {
                using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("items"))
                    cmd.Insert("ID", Item.ID).Insert("UID", Item.UID)
                   .Insert("Plus", Item.Plus).Insert("Bless", Item.Bless)
                   .Insert("Enchant", Item.Enchant).Insert("SocketOne", (byte)Item.SocketOne)
                   .Insert("SocketTwo", (byte)Item.SocketTwo).Insert("Durability", Item.Durability)
                   .Insert("MaximDurability", Item.MaximDurability).Insert("SocketProgress", Item.SocketProgress)
                   .Insert("PlusProgress", Item.PlusProgress).Insert("Effect", (ushort)Item.Effect)
                   .Insert("Bound", Item.Bound).Insert("DayStamp", Item.DayStamp.Ticks).Insert("Days", Item.Days).Insert("Locked", Item.Lock).Insert("UnlockEnd", Item.UnlockEnd.Ticks)
                   .Insert("Suspicious", Item.Suspicious).Insert("SuspiciousStart", Item.SuspiciousStart.Ticks)
                   .Insert("Color", (ushort)Item.Color).Insert("Position", Item.Position).Insert("StackSize", Item.StackSize)
                   .Insert("RefineryItem", Item.RefineItem).Insert("RefineryTime", Item.RefineryTime.Ticks).Insert("EntityID", client.Entity.UID)
                   .Insert("Warehouse", Item.Warehouse).Insert("NextSteedColor", Item.NextGreen | (Item.NextBlue << 8) | (Item.NextRed << 16))
                   .Insert("Perfectionlevel", Item.Perfectionlevel).Insert("PerfectionProgress", Item.PerfectionProgress)
                   .Insert("OwnerID", Item.OwnerID).Insert("OwnerName", Item.OwnerName ?? "").Insert("Signature", Item.Signature ?? "")
                   .Insert("Agate", Item.Agate ?? "").Insert("InWardrobe", Item.InWardrobe).Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                DeleteItem(Item.UID);
                AddItem(ref Item, client);
            }
        }

        public static void UpdatePerfection(ConquerItem Item)
        {
            UpdateData(Item, "PerfectionProgress", Item.PerfectionProgress);
            UpdateData(Item, "Perfectionlevel", Item.Perfectionlevel);
            UpdateData(Item, "OwnerName", Item.OwnerName);
            UpdateData(Item, "OwnerID", Item.OwnerID);
            UpdateData(Item, "Signature", Item.Signature);
        }
        private static void UpdateData(ConquerItem Item, string column, object value)
        {
            UpdateData(Item.UID, column, value);
        }
        private static void UpdateData(uint UID, string column, object value)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update("items").Set(column, value.ToString())
                    .Where("UID", UID).Execute();
        }
        public static void UpdateBless(ConquerItem Item)
        {
            UpdateData(Item, "Bless", Item.Bless);
        }
        public static void UpdateRefineryItem(ConquerItem Item)
        {
            UpdateData(Item, "RefineryItem", Item.RefineItem);
        }
        public static void UpdateRefineryTime(ConquerItem Item)
        {
            UpdateData(Item, "RefineryTime", Item.RefineryTime.Ticks);
        }
        public static void UpdateItemAgate(ConquerItem Item)
        {
            string agate = "";
            if (Item.ID == 720828)
            {
                foreach (string coord in Item.Agate_map.Values)
                {
                    agate += coord + "#";
                    UpdateData(Item, "agate", agate);
                }
            }
        }
        public static void UpdateWardrobe(bool inWardrobe, uint UID)
        {
            using (MySqlCommand command = new MySqlCommand(MySqlCommandType.UPDATE))
            {
                command.Update("items")
                    .Set("InWardrobe", inWardrobe)
                    .Where("UID", UID).Execute();
            }
        }
        public static void UpdateColor(ConquerItem Item)
        {
            UpdateData(Item, "Color", (uint)Item.Color);
        }
        public static void UpdateStack(ConquerItem Item)
        {
            UpdateData(Item, "StackSize", Item.StackSize);
        }
        public static void UpdateEnchant(ConquerItem Item)
        {
            UpdateData(Item, "Enchant", Item.Enchant);
        }
        public static void UpdateLock(ConquerItem Item)
        {
            UpdateData(Item, "Locked", Item.Lock);
            UpdateData(Item, "UnlockEnd", Item.UnlockEnd.ToBinary());
        }
        public static void UpdateSockets(ConquerItem Item)
        {
            UpdateData(Item, "SocketOne", (byte)Item.SocketOne);
            UpdateData(Item, "SocketTwo", (byte)Item.SocketTwo);
        }
        public static void UpdateSocketProgress(ConquerItem Item)
        {
            UpdateData(Item, "SocketProgress", Item.SocketProgress);
        }
        public static void UpdateNextSteedColor(ConquerItem Item)
        {
            UpdateData(Item, "NextSteedColor", Item.NextGreen | (Item.NextBlue << 8) | (Item.NextRed << 16));
        }
        public static void UpdateDurabilityItem(ConquerItem Item)
        {
        }
        public static void UpdateLocation(ConquerItem Item, Client.GameState client)
        {
            if (IsThere(Item.UID))
            {
                using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                    cmd.Update("items").Set("EntityID", client.Entity.UID)
                        .Set("Position", Item.Position).Set("Warehouse", (uint)Item.Warehouse)
                        .Where("UID", Item.UID).Execute();
            }
            else
            {
                using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("items"))
                    cmd.Insert("ID", Item.ID).Insert("UID", Item.UID)
                        .Insert("Plus", Item.Plus).Insert("Bless", Item.Bless)
                        .Insert("Enchant", Item.Enchant).Insert("SocketOne", (byte)Item.SocketOne)
                        .Insert("SocketTwo", (byte)Item.SocketTwo).Insert("Durability", Item.Durability)
                        .Insert("MaximDurability", Item.MaximDurability).Insert("SocketProgress", Item.SocketProgress)
                        .Insert("PlusProgress", Item.PlusProgress).Insert("Effect", (ushort)Item.Effect)
                        .Insert("Bound", Item.Bound).Insert("DayStamp", Item.DayStamp.ToString()).Insert("Days", Item.Days).Insert("Locked", Item.Lock).Insert("UnlockEnd", Item.UnlockEnd.Ticks)
                        .Insert("Suspicious", Item.Suspicious).Insert("SuspiciousStart", Item.SuspiciousStart.Ticks)
                        .Insert("Color", (ushort)Item.Color).Insert("Position", Item.Position).Insert("StackSize", Item.StackSize)
                        .Insert("RefineryItem", Item.RefineItem).Insert("RefineryTime", Item.RefineryTime.Ticks).Insert("EntityID", client.Entity.UID)
                        .Insert("NextSteedColor", Item.NextGreen | (Item.NextBlue << 8) | (Item.NextRed << 16))
                        .Insert("Perfectionlevel", Item.Perfectionlevel).Insert("PerfectionProgress", Item.PerfectionProgress)
                        .Insert("OwnerID", Item.OwnerID).Insert("OwnerName", Item.OwnerName ?? "").Insert("Signature", Item.Signature ?? "")
                        .Insert("Agate", Item.Agate ?? "").Insert("InWardrobe", Item.InWardrobe)
                        .Execute();
            }
        }
        public static void UpdatePosition(ConquerItem Item)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update("items").Set("Position", Item.Position).Set("Warehouse", Item.Warehouse)
                .Where("UID", Item.UID).Execute();

        }
        public static void UpdatePlus(ConquerItem Item)
        {
            UpdateData(Item, "Plus", Item.Plus);
        }

        public static void UpdateBound(ConquerItem Item)
        {
            UpdateData(Item, "Bound", 0);
        }
        public static void UpdatePlusProgress(ConquerItem Item)
        {
            UpdateData(Item, "PlusProgress", Item.PlusProgress);
        }
        public static void UpdateItemID(ConquerItem Item, Client.GameState client)
        {
            UpdateData(Item, "ID", Item.ID);
        }
        public static void RemoveItem(uint UID)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update("items").Set("EntityID", 0)
                    .Set("Position", 0).Where("UID", UID).Execute();
        }
        public static void DeleteItem(uint UID)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.DELETE))
                cmd.Delete("items", "UID", UID).Execute();
        }
        public static void ClearPosition(uint EntityID, byte position)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update("items").Set("EntityID", 0).Set("Position", 0)
                    .Where("EntityID", EntityID).And("Position", position).Execute();
        }
        public static void RefineryUpdate(ConquerItem Item, Client.GameState client)
        {
        }

        public static void ClearNulledItems()
        {
            Dictionary<uint, int> dict = new Dictionary<uint, int>();
            using (var c = new MySqlCommand(MySqlCommandType.SELECT).Select("detaineditems"))
            using (var r = c.CreateReader())
                while (r.Read())
                    dict[r.ReadUInt32("ItemUID")] = 0;
            //cmd.Where("UID", r.ReadUInt32("ItemUID"));

            using (var c = new MySqlCommand(MySqlCommandType.SELECT).Select("claimitems"))
            using (var r = c.CreateReader())
                while (r.Read())
                    dict[r.ReadUInt32("ItemUID")] = 0;
            //cmd.Where("UID", r.ReadUInt32("ItemUID"));
            var array = dict.Keys.ToArray();
            foreach (var item in array)
                using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("items")
                    .Set("entityid", 1).Where("entityid", 0).And("uid", item))
                    cmd.Execute();

            using (var cmd = new MySqlCommand(MySqlCommandType.DELETE).Delete("items", "entityid", 0))
                cmd.Execute();
            Console.WriteLine("Cleared items with EntityID = 0");
        }

        public static bool IsThere(uint uid)
        {
            MySqlCommand command = new MySqlCommand(MySqlCommandType.SELECT);
            command.Select("items").Where("UID", (long)uid);
            MySqlReader reader = new MySqlReader(command);
            if (reader.Read())
            {
                //  Reader.Close();
                ////  Reader.Dispose();
                return true;
            }
            //  Reader.Close();
            ////  Reader.Dispose();
            return false;
        }

        public static void Update_Free(ConquerItem Item, Client.GameState client)
        {
            MySqlCommand command = new MySqlCommand(MySqlCommandType.UPDATE);
            command.Update("items").Set("EntityID", client.Entity.UID).Set("DayStamp", Item.DayStamp.Ticks).Set("Days", Item.Days).Where("UID", Item.UID).Execute();
        }
    }
}
