namespace MTA.Game.Features.Guilds.Database.Models;

/// <summary>
///     Represents a row from the `guild_relations` table
/// </summary>
public sealed class GuildRelationRecord {
    public uint GuildId { get; init; }
    public uint RelatedGuildId { get; init; }
    public byte RelationType { get; init; } // 0 = enemy, 1 = ally
}