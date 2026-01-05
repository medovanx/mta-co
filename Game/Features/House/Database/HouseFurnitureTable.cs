using System.Collections.Generic;
using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Game.Features.House.Database.Mappers;
using MTA.Game.Features.House.Database.Schema;
using MTA.Network.GamePackets;
using HouseInfo = MTA.Game.Features.House.Database.Models.HouseInfo;

namespace MTA.Game.Features.House.Database;

public static class HouseFurnitureTable {
    /// <summary>
    ///     Loads furniture from the house_furniture table for a specific house
    /// </summary>
    public static Dictionary<uint, SobNpcSpawn> LoadFurniture(uint houseUid, ushort mapId) {
        var furniture = new Dictionary<uint, SobNpcSpawn>();
        using var cmd = new MySqlCommand(MySqlCommandType.SELECT)
            .Select(HouseFurnitureSchema.Tables.HouseFurnitureTable)
            .Where(HouseFurnitureSchema.HouseFurniture.HouseUid, houseUid);
        using var reader = new MySqlReader(cmd);
        while (reader.Read()) {
            var record = HouseFurnitureMappers.MapHouseFurniture(reader);
            var furnitureItem = new SobNpcSpawn {
                UID = record.FurnitureUid,
                Mesh = record.Mesh,
                X = record.X,
                Y = record.Y,
                Type = (Enums.NpcType)record.Type,
                MapID = mapId
            };
            furniture.TryAdd(record.FurnitureUid, furnitureItem);
        }

        return furniture;
    }

    /// <summary>
    ///     Saves all furniture for a house to the database (synchronizes in-memory furniture with database)
    /// </summary>
    public static void SaveFurniture(GameState client, HouseInfo info) {
        if (info.Furniture == null) return;

        // Get current furniture from database
        var dbFurniture = new HashSet<uint>();
        using (var cmd = new MySqlCommand(MySqlCommandType.SELECT)
                   .Select(HouseFurnitureSchema.Tables.HouseFurnitureTable)
                   .Where(HouseFurnitureSchema.HouseFurniture.HouseUid, client.Entity.UID)) {
            using var reader = new MySqlReader(cmd);
            while (reader.Read()) {
                var record = HouseFurnitureMappers.MapHouseFurniture(reader);
                dbFurniture.Add(record.FurnitureUid);
            }
        }

        // Add or update furniture that exists in memory
        foreach (var furnitureItem in info.Furniture.Values) {
            var type = (byte)(furnitureItem.Mesh / 10 == 820 ? 2 : 26);
            if (dbFurniture.Contains(furnitureItem.UID))
                // Update existing furniture
                new MySqlCommand(MySqlCommandType.UPDATE)
                    .Update(HouseFurnitureSchema.Tables.HouseFurnitureTable)
                    .Set(HouseFurnitureSchema.HouseFurniture.Mesh, furnitureItem.Mesh)
                    .Set(HouseFurnitureSchema.HouseFurniture.X, furnitureItem.X)
                    .Set(HouseFurnitureSchema.HouseFurniture.Y, furnitureItem.Y)
                    .Set(HouseFurnitureSchema.HouseFurniture.Type, type)
                    .Where(HouseFurnitureSchema.HouseFurniture.HouseUid, client.Entity.UID)
                    .Where(HouseFurnitureSchema.HouseFurniture.FurnitureUid, furnitureItem.UID)
                    .Execute();
            else
                // Insert new furniture
                AddFurniture(client.Entity.UID, furnitureItem, type);
        }

        // Remove furniture that no longer exists in memory
        foreach (var furnitureUid in dbFurniture.Where(furnitureUid => !info.Furniture.ContainsKey(furnitureUid))) {
            RemoveFurniture(client.Entity.UID, furnitureUid);
        }
    }

    /// <summary>
    ///     Adds a single furniture piece to the database
    /// </summary>
    public static void AddFurniture(uint houseUid, SobNpcSpawn furnitureItem, byte type) {
        new MySqlCommand(MySqlCommandType.INSERT)
            .Insert(HouseFurnitureSchema.Tables.HouseFurnitureTable)
            .Insert(HouseFurnitureSchema.HouseFurniture.HouseUid, houseUid)
            .Insert(HouseFurnitureSchema.HouseFurniture.FurnitureUid, furnitureItem.UID)
            .Insert(HouseFurnitureSchema.HouseFurniture.Mesh, furnitureItem.Mesh)
            .Insert(HouseFurnitureSchema.HouseFurniture.X, furnitureItem.X)
            .Insert(HouseFurnitureSchema.HouseFurniture.Y, furnitureItem.Y)
            .Insert(HouseFurnitureSchema.HouseFurniture.Type, type)
            .Execute();
    }

    /// <summary>
    ///     Removes a single furniture piece from the database
    /// </summary>
    public static void RemoveFurniture(uint houseUid, uint furnitureUid) {
        new MySqlCommand(MySqlCommandType.DELETE)
            .Delete(HouseFurnitureSchema.Tables.HouseFurnitureTable, HouseFurnitureSchema.HouseFurniture.HouseUid,
                houseUid)
            .Where(HouseFurnitureSchema.HouseFurniture.FurnitureUid, furnitureUid)
            .Execute();
    }

    /// <summary>
    ///     Updates the position of a furniture piece in the database
    /// </summary>
    public static void UpdateFurniturePosition(uint houseUid, uint furnitureUid, ushort x, ushort y) {
        new MySqlCommand(MySqlCommandType.UPDATE)
            .Update(HouseFurnitureSchema.Tables.HouseFurnitureTable)
            .Set(HouseFurnitureSchema.HouseFurniture.X, x)
            .Set(HouseFurnitureSchema.HouseFurniture.Y, y)
            .Where(HouseFurnitureSchema.HouseFurniture.HouseUid, houseUid)
            .Where(HouseFurnitureSchema.HouseFurniture.FurnitureUid, furnitureUid)
            .Execute();
    }
}