// ReSharper disable InconsistentNaming

namespace MTA.Game.Features.Kisses.Database.Models;

/// <summary>
///     Represents a row from the `kisses` table
/// </summary>
public sealed class KissRecord {
    public uint EntityId { get; init; }
    public uint Kisses { get; init; }
    public uint KissesToday { get; init; }
    public uint Letters { get; init; }
    public uint LettersToday { get; init; }
    public uint Wine { get; init; }
    public uint WineToday { get; init; }
    public uint Jades { get; init; }
    public uint JadesToday { get; init; }
    public long LastKissesSent { get; init; }
}