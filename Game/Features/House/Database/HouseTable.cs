using System;
using System.Collections.Generic;
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

                // Load furniture from house_furniture table
                info.Furniture = HouseFurnitureTable.LoadFurniture(info.Uid, info.Id);

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
    ///     Creates a new house in the database
    /// </summary>
    public static void Create(GameState client, HouseInfo info) {
        var command = new MySqlCommand(MySqlCommandType.INSERT);
        command.Insert(HouseSchema.Tables.HouseTable)
            .Insert(HouseSchema.House.Uid, client.Entity.UID)
            .Insert(HouseSchema.House.MapType, info.MapType)
            .Insert(HouseSchema.House.Level, info.Level)
            .Insert(HouseSchema.House.Id, (ushort)client.Entity.UID);
        command.Execute();
    }

    /// <summary>
    ///     Updates house information in the database
    /// </summary>
    public static void Update(GameState client, ushort mapType, ushort level) {
        new MySqlCommand(MySqlCommandType.UPDATE)
            .Update(HouseSchema.Tables.HouseTable)
            .Set(HouseSchema.House.Id, (ushort)client.Entity.UID)
            .Set(HouseSchema.House.MapType, mapType)
            .Set(HouseSchema.House.Level, level)
            .Where(HouseSchema.House.Uid, client.Entity.UID)
            .Execute();
    }
}