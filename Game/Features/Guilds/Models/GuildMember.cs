using MTA.Client;
using MTA.Game.ConquerStructures;
using MTA.Game.Features.Guilds.Constants;

namespace MTA.Game.Features.Guilds.Models;

public class GuildMember(uint guildId) {
    public uint ArsenalDonation;
    public byte Class;
    public uint CtfCpsReward;
    public uint CtfSilverReward;
    public uint Exploits = 0;
    public uint GuideDonation;
    public uint ExploitsRank;
    public ulong LastLogin = 0;
    public uint Lilies;
    public uint Mesh;
    public uint Orchids;
    public uint PkDonation;
    public uint Roses;
    public string Spouse = string.Empty;
    public uint Tulips;
    public uint VirtuePoints;
    public uint WarScore;
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public GameState? Client => Kernel.TryGetPlayer(Id, out var client) ? client : null;
    public ulong SilverDonation { get; set; }
    public ulong ConquerPointDonation { get; set; }
    public uint GuildId { get; set; } = guildId;
    public Guild Guild => Kernel.Guilds[GuildId];
    public MemberRank Rank { get; set; }
    public byte Level { get; set; }
    public NobilityRank NobilityRank { get; set; }
    public byte Gender { get; set; }

    public uint TotalDonation =>
        (uint)(Lilies + Orchids + Tulips + Roses + ConquerPointDonation + VirtuePoints +
               (uint)SilverDonation + ArsenalDonation + PkDonation);
}
