// ReSharper disable InconsistentNaming

namespace MTA.Game.Features.House.Database.Models;

/// <summary>
///     Represents a row from the `house_furniture` table
/// </summary>
public sealed class HouseFurnitureRecord {
    public uint HouseUid { get; init; }
    public uint FurnitureUid { get; init; }
    public ushort Mesh { get; init; }
    public ushort X { get; init; }
    public ushort Y { get; init; }
    public byte Type { get; init; }
}