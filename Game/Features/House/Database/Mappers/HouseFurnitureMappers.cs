using MTA.Database;
using MTA.Game.Features.House.Database.Models;
using MTA.Game.Features.House.Database.Schema;

namespace MTA.Game.Features.House.Database.Mappers;

/// <summary>
///     Mapper functions that convert MySqlReader to strongly-typed models.
///     All database column name references use HouseFurnitureSchema constants.
/// </summary>
public static class HouseFurnitureMappers {
    /// <summary>
    ///     Maps a MySqlReader to a HouseFurnitureRecord from the `house_furniture` table
    /// </summary>
    public static HouseFurnitureRecord MapHouseFurniture(MySqlReader reader) {
        return new HouseFurnitureRecord {
            HouseUid = reader.ReadUInt32(HouseFurnitureSchema.HouseFurniture.HouseUid),
            FurnitureUid = reader.ReadUInt32(HouseFurnitureSchema.HouseFurniture.FurnitureUid),
            Mesh = reader.ReadUInt16(HouseFurnitureSchema.HouseFurniture.Mesh),
            X = reader.ReadUInt16(HouseFurnitureSchema.HouseFurniture.X),
            Y = reader.ReadUInt16(HouseFurnitureSchema.HouseFurniture.Y),
            Type = reader.ReadByte(HouseFurnitureSchema.HouseFurniture.Type)
        };
    }
}