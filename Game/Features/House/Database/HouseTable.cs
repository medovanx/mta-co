using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Game.Features.House.Database.Mappers;
using MTA.Game.Features.House.Database.Models;
using MTA.Game.Features.House.Database.Schema;
using MTA.Network.GamePackets;
using static MTA.Game.Features.House.House;
using Warehouse = MTA.Game.ConquerStructures.Warehouse;

namespace MTA.Game.Features.House.Database;

public static class HouseTable {
    /// <summary>
    ///     Loads all houses from the database and populates the House.Houses dictionary
    /// </summary>
    public static void Load() {
        try {
            using var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select(HouseSchema.Tables.HouseTable);
            using var reader = new MySqlReader(cmd);
            while (reader.Read()) {
                var info = HouseMappers.MapHouse(reader);

                // Initialize warehouse if item box exists
                if (info.Furniture != null) {
                    var itemBox = info.Furniture.Values.FirstOrDefault(xx => xx.Mesh / 10 == 820);
                    if (itemBox != null) {
                        itemBox.Type = (Enums.NpcType)2;
                        info.Warehouse = new Warehouse(null, (Warehouse.WarehouseID)itemBox.UID);
                        var items = LoadWarehouseItems(itemBox.UID);
                        foreach (var item in items.Values.Where(item => !info.Warehouse.ContainsUID(item.UID))) {
                            info.Warehouse.Add2(item, null);
                        }
                    }
                }

                if (!Houses.ContainsKey(info.Uid))
                    Houses.Add(info.Uid, info);
                _ = new Map(info.Id, info.MapType, Kernel.Maps[info.MapType].Path);
            }
        }
        catch (Exception exception) {
            Console.WriteLine(exception);
            Program.SaveException(exception);
        }
    }

    /// <summary>
    ///     Loads items from a warehouse
    /// </summary>
    private static SafeDictionary<uint, ConquerItem> LoadWarehouseItems(uint warehouse) {
        SafeDictionary<uint, ConquerItem> items = [];
        using var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("items").Where("Warehouse", warehouse);
        using var reader = new MySqlReader(cmd);
        while (reader.Read()) {
            var item = ConquerItemTable.deserialzeItem(reader);
            if (!items.ContainsKey(item.UID))
                items.Add(item.UID, item);
        }

        return items;
    }


    /// <summary>
    ///     Saves furniture data to the database as a blob
    /// </summary>
    public static void SaveFurniture(GameState client, HouseInfo info) {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write((byte)(info.Furniture?.Count ?? 0));
        if (info.Furniture != null)
            foreach (var fur in info.Furniture.Values) {
                WriteItem(writer, fur);
            }

        var rawData = stream.ToArray();
        using var conn = DataHolder.MySqlConnection;
        conn.Open();
        using var cmd = new MySql.Data.MySqlClient.MySqlCommand();
        cmd.Connection = conn;
        cmd.CommandText =
            $"UPDATE `{HouseSchema.Tables.HouseTable}` SET {HouseSchema.House.Furniture}=@Furnitures WHERE {HouseSchema.House.Uid} = @Uid";
        cmd.Parameters.AddWithValue("@Furnitures", rawData);
        cmd.Parameters.AddWithValue("@Uid", client.Entity.UID);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    ///     Creates a new house in the database
    /// </summary>
    public static void Create(GameState client, HouseInfo info) {
        var command = new MySqlCommand(MySqlCommandType.INSERT);
        command.Insert(HouseSchema.Tables.HouseTable)
            .Insert(HouseSchema.House.Uid, client.Entity.UID)
            .Insert(HouseSchema.House.MapType, info.MapType)
            .Insert(HouseSchema.House.Level, info.Level)
            .Insert(HouseSchema.House.Name, client.Entity.Name)
            .Insert(HouseSchema.House.Id, (ushort)client.Entity.UID);
        command.Execute();
    }

    /// <summary>
    ///     Updates house information in the database
    /// </summary>
    public static void Update(GameState client, ushort maptype, ushort level) {
        new MySqlCommand(MySqlCommandType.UPDATE)
            .Update(HouseSchema.Tables.HouseTable)
            .Set(HouseSchema.House.Name, client.Entity.Name)
            .Set(HouseSchema.House.Id, (ushort)client.Entity.UID)
            .Set(HouseSchema.House.MapType, maptype)
            .Set(HouseSchema.House.Level, level)
            .Where(HouseSchema.House.Uid, client.Entity.UID)
            .Execute();
    }

    /// <summary>
    ///     Writes a SobNpcSpawn item to a BinaryWriter
    /// </summary>
    private static void WriteItem(BinaryWriter writer, SobNpcSpawn furnitureItem) {
        writer.Write(furnitureItem.UID);
        writer.Write(furnitureItem.Mesh);
        writer.Write(furnitureItem.X);
        writer.Write(furnitureItem.Y);
    }
}