// ReSharper disable InconsistentNaming

namespace MTA.Game.Features.Flowers.Database.Models;

/// <summary>
///     Represents a row from the `flowers` table
/// </summary>
public sealed class FlowerRecord {
    public uint EntityId { get; init; }
    public uint RedRoses { get; init; }
    public uint RedRosesToday { get; init; }
    public uint Lilies { get; init; }
    public uint LiliesToday { get; init; }
    public uint Orchids { get; init; }
    public uint OrchidsToday { get; init; }
    public uint Tulips { get; init; }
    public uint TulipsToday { get; init; }
    public long LastFlowerSent { get; init; }
    public uint SendDay { get; init; }
    public uint AFlower { get; init; }
}