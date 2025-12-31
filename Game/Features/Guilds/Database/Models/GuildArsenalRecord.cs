namespace MTA.Game.Features.Guilds.Database.Models;

/// <summary>
///     Represents a row from the `guildarsenal` table
/// </summary>
public sealed class GuildArsenalRecord {
    public uint Id { get; init; }

    public byte[] Data { get; init; } = [];
    // DataLength is computed from Data.Length, not stored separately
}