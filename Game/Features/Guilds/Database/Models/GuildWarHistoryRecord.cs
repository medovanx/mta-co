using System;
using System.Collections.Generic;

namespace MTA.Game.Features.Guilds.Database.Models;

/// <summary>
///     Represents a row from the `guild_war_history` table
/// </summary>
public sealed class GuildWarHistoryRecord {
    public uint Id { get; init; }
    public uint GuildId { get; init; }
    public uint GuildLeaderEntityId { get; init; }
    public required string GuildLeaderName { get; init; }
    public bool GuildLeaderClaimed { get; init; }
    public List<uint> DeputyClaimedIds { get; init; } = [];
    public DateTime WarEndTime { get; init; }
}