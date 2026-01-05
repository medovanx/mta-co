using System.Collections.Generic;
using System.IO;
using MTA.Database;
using MTA.Game.Features.House.Database.Models;
using MTA.Game.Features.House.Database.Schema;
using MTA.Network.GamePackets;

namespace MTA.Game.Features.House.Database.Mappers;

/// <summary>
///     Mapper functions that convert MySqlReader to strongly-typed models.
///     All database column name references use HouseSchema constants.
/// </summary>
public static class HouseMappers {
    /// <summary>
    ///     Maps a MySqlReader to a HouseInfo from the `house` table, deserializing the furniture blob
    /// </summary>
    public static HouseInfo MapHouse(MySqlReader reader) {
        var uid = reader.ReadUInt32(HouseSchema.House.Uid);
        var name = reader.ReadString(HouseSchema.House.Name);
        var id = reader.ReadUInt16(HouseSchema.House.Id);
        var mapType = reader.ReadUInt16(HouseSchema.House.MapType);
        var level = reader.ReadUInt16(HouseSchema.House.Level);
        var furnitureBlob = reader.ReadBlob(HouseSchema.House.Furniture);
        var furniture = DeserializeFurniture(furnitureBlob, id);

        return new HouseInfo {
            Uid = uid,
            Name = name,
            Id = id,
            MapType = mapType,
            Level = level,
            Furniture = furniture
        };
    }

    /// <summary>
    ///     Deserializes furniture blob data into a dictionary of SobNpcSpawn objects
    /// </summary>
    private static Dictionary<uint, SobNpcSpawn> DeserializeFurniture(byte[] data, ushort mapId) {
        var furniture = new Dictionary<uint, SobNpcSpawn>();
        if (data.Length == 0) return furniture;

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        int count = reader.ReadByte();
        for (uint x = 0; x < count; x++) {
            var furnitureItem = ReadItem(reader);
            if (furnitureItem.Mesh / 10 == 820)
                furnitureItem.Type = (Enums.NpcType)2;
            else
                furnitureItem.Type = (Enums.NpcType)26;

            furnitureItem.MapID = mapId;
            furniture.TryAdd(furnitureItem.UID, furnitureItem);
        }

        return furniture;
    }

    /// <summary>
    ///     Reads a SobNpcSpawn item from a BinaryReader
    /// </summary>
    private static SobNpcSpawn ReadItem(BinaryReader reader) {
        SobNpcSpawn furnitureItem = new() {
            UID = reader.ReadUInt32(),
            Mesh = reader.ReadUInt16(),
            X = reader.ReadUInt16(),
            Y = reader.ReadUInt16()
        };
        return furnitureItem;
    }
}