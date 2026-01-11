using MTA.Database;
using MTA.Game.Features.Flowers.Database.Models;
using MTA.Game.Features.Flowers.Database.Schema;

namespace MTA.Game.Features.Flowers.Database.Mappers;

/// <summary>
///     Mapper functions that convert MySqlReader to strongly-typed models.
///     All database column name references use FlowerSchema constants.
/// </summary>
public static class FlowerMappers {
    /// <summary>
    ///     Maps a MySqlReader to a FlowerRecord from the `flowers` table
    /// </summary>
    public static FlowerRecord MapFlower(MySqlReader reader) {
        return new FlowerRecord {
            EntityId = reader.ReadUInt32(FlowerSchema.Flowers.EntityId),
            RedRoses = reader.ReadUInt32(FlowerSchema.Flowers.RedRoses),
            RedRosesToday = reader.ReadUInt32(FlowerSchema.Flowers.RedRosesToday),
            Lilies = reader.ReadUInt32(FlowerSchema.Flowers.Lilies),
            LiliesToday = reader.ReadUInt32(FlowerSchema.Flowers.LiliesToday),
            Orchids = reader.ReadUInt32(FlowerSchema.Flowers.Orchids),
            OrchidsToday = reader.ReadUInt32(FlowerSchema.Flowers.OrchidsToday),
            Tulips = reader.ReadUInt32(FlowerSchema.Flowers.Tulips),
            TulipsToday = reader.ReadUInt32(FlowerSchema.Flowers.TulipsToday),
            LastFlowerSent = reader.ReadInt64(FlowerSchema.Flowers.LastFlowerSent),
            SendDay = reader.ReadUInt32(FlowerSchema.Flowers.SendDay),
            AFlower = reader.ReadUInt32(FlowerSchema.Flowers.AFlower)
        };
    }
}