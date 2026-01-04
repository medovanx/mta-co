// ReSharper disable InconsistentNaming

namespace MTA.Game.Features.Guilds.Database.Models;

/// <summary>
///     Represents a row from the `guild_members` table
/// </summary>
public sealed class GuildMemberRecord {
    public uint EntityId { get; init; }
    public uint GuildId { get; init; }
    public ushort Rank { get; init; }
    public ulong SilverDonation { get; init; }
    public ulong ConquerPointDonation { get; init; }
    public uint ArsenalDonation { get; init; }
    public uint Lilies { get; init; }
    public uint Roses { get; init; }
    public uint Orchids { get; init; }
    public uint Tulips { get; init; }
    public uint PkDonation { get; init; }
    public ulong LastLogin { get; init; }
    public uint Exploits { get; init; }
    public uint GuideDonation { get; init; }
    public uint CtfCpsReward { get; init; }
    public uint CtfSilverReward { get; init; }
}