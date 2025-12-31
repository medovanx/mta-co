// ReSharper disable InconsistentNaming

namespace MTA.Game.Features.Guilds.Database.Models;

/// <summary>
///     Represents a row from the `guilds` table
/// </summary>
public sealed class GuildRecord {
    public uint Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Bulletin { get; init; } = string.Empty;
    public ulong SilverFund { get; init; }
    public ulong LeaderID { get; init; }
    public uint Wins { get; init; }
    public uint Loses { get; init; }
    public uint CTFPoints { get; init; }
    public uint CTFReward { get; init; }
    public uint ConquerPointFund { get; init; }
    public uint LevelRequirement { get; init; }
    public uint RebornRequirement { get; init; }
    public uint ClassRequirement { get; init; }
    public string Advertise { get; init; } = string.Empty;
    public string GuildEnroll { get; init; } = string.Empty;
    public string BulletinEnroll { get; init; } = string.Empty;
    public string CTFDonationCps { get; init; } = string.Empty;
    public string CTFDonationSilver { get; init; } = string.Empty;
    public string CTFdonationSilverold { get; init; } = string.Empty;
    public string CTFdonationCpsold { get; init; } = string.Empty;
}