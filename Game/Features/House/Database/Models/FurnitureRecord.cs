namespace MTA.Game.Features.House.Database.Models;

/// <summary>
///     Represents a row from the `furniture` table
/// </summary>
public sealed class FurnitureRecord {
    public uint NpcId { get; init; }
    public byte Type { get; init; }
    public ushort Mesh { get; init; }
    public ushort Map { get; init; }
    public ushort X { get; init; }
    public ushort Y { get; init; }
    public uint ItemId { get; init; }
    public uint Price { get; init; }
}