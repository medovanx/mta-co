using MTA.Database;
using MTA.Game.Features.House.Database.Models;
using MTA.Game.Features.House.Database.Schema;

namespace MTA.Game.Features.House.Database.Mappers;

/// <summary>
///     Mapper functions that convert MySqlReader to strongly-typed models.
///     All database column name references use FurnitureSchema constants.
/// </summary>
public static class FurnitureMappers {
    /// <summary>
    ///     Maps a MySqlReader to a FurnitureRecord from the `furniture` table
    /// </summary>
    public static FurnitureRecord MapFurniture(MySqlReader reader) {
        return new FurnitureRecord {
            NpcId = reader.ReadUInt32(FurnitureSchema.Furniture.NpcId),
            Type = reader.ReadByte(FurnitureSchema.Furniture.Type),
            Mesh = reader.ReadUInt16(FurnitureSchema.Furniture.Mesh),
            Map = reader.ReadUInt16(FurnitureSchema.Furniture.Map),
            X = reader.ReadUInt16(FurnitureSchema.Furniture.X),
            Y = reader.ReadUInt16(FurnitureSchema.Furniture.Y),
            ItemId = reader.ReadUInt32(FurnitureSchema.Furniture.ItemId),
            Price = reader.ReadUInt32(FurnitureSchema.Furniture.Price)
        };
    }
}