using MTA.Database;
using MTA.Game.Features.House.Database.Models;
using MTA.Game.Features.House.Database.Schema;

namespace MTA.Game.Features.House.Database.Mappers;

/// <summary>
///     Mapper functions that convert MySqlReader to strongly-typed models.
///     All database column name references use HouseSchema constants.
/// </summary>
public static class HouseMappers {
    /// <summary>
    ///     Maps a MySqlReader to a HouseInfo from the `house` table
    /// </summary>
    public static HouseInfo MapHouse(MySqlReader reader) {
        return new HouseInfo {
            Uid = reader.ReadUInt32(HouseSchema.House.Uid),
            Name = reader.ReadString(HouseSchema.House.Name),
            Id = reader.ReadUInt16(HouseSchema.House.Id),
            MapType = reader.ReadUInt16(HouseSchema.House.MapType),
            Level = reader.ReadUInt16(HouseSchema.House.Level),
            Furniture = null // Furniture is loaded separately from house_furniture table
        };
    }
}