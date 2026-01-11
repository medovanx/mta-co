using MTA.Database;
using MTA.Game.Features.Kisses.Database.Models;
using MTA.Game.Features.Kisses.Database.Schema;

namespace MTA.Game.Features.Kisses.Database.Mappers;

/// <summary>
///     Mapper functions that convert MySqlReader to strongly-typed models.
///     All database column name references use KissSchema constants.
/// </summary>
public static class KissMappers {
    /// <summary>
    ///     Maps a MySqlReader to a KissRecord from the `kisses` table
    /// </summary>
    public static KissRecord MapKiss(MySqlReader reader) {
        return new KissRecord {
            EntityId = reader.ReadUInt32(KissSchema.Kisses.EntityId),
            Kisses = reader.ReadUInt32(KissSchema.Kisses.KissesCount),
            KissesToday = reader.ReadUInt32(KissSchema.Kisses.KissesToday),
            Letters = reader.ReadUInt32(KissSchema.Kisses.Letters),
            LettersToday = reader.ReadUInt32(KissSchema.Kisses.LettersToday),
            Wine = reader.ReadUInt32(KissSchema.Kisses.Wine),
            WineToday = reader.ReadUInt32(KissSchema.Kisses.WineToday),
            Jades = reader.ReadUInt32(KissSchema.Kisses.Jades),
            JadesToday = reader.ReadUInt32(KissSchema.Kisses.JadesToday),
            LastKissesSent = reader.ReadInt64(KissSchema.Kisses.LastKissesSent)
        };
    }
}