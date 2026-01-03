using MTA.Client;
using Writer = MTA.Network.Writer;

namespace MTA.Game.Features.Guilds.Packets.Writers;

/// <summary>
///     Constructs packet 1058 that contains guild member donation information. This packet displays
///     the player's current donations including silver, Conquer Points, arsenal, PK, and flowers.
/// </summary>
public class GuildDonationInfoPacket : Writer {
    private readonly byte[] _packet;

    public GuildDonationInfoPacket() {
        _packet = new byte[8 + 48]; // Base packet size
        WriteUInt16(48, 0, _packet);
        WriteUInt16(1058, 2, _packet);
    }

    /// <summary>
    ///     Builds the packet with donation data from the client's guild member record.
    ///     Writes all donation types (silver, CP, arsenal, PK, flowers) to the packet.
    /// </summary>
    /// <param name="client">The client whose donation data will be written to the packet</param>
    public void Build(GameState client) {
        if (client.AsMember == null) return;
        WriteUInt64(client.AsMember.SilverDonation, 8, _packet);
        WriteUInt32((uint)client.AsMember.ConquerPointDonation, 12, _packet);
        WriteUInt32(0, 16, _packet); // history donation
        WriteUInt32(client.AsMember.PkDonation, 20, _packet);
        WriteUInt32(client.AsMember.ArsenalDonation, 24, _packet);
        WriteUInt32(client.AsMember.Roses, 28, _packet);
        WriteUInt32(client.AsMember.Tulips, 32, _packet);
        WriteUInt32(client.AsMember.Lilies, 36, _packet);
        WriteUInt32(client.AsMember.Orchids, 40, _packet);
        WriteUInt32(client.AsMember.Orchids
                    + client.AsMember.Roses
                    + client.AsMember.Tulips
                    + client.AsMember.Lilies, 44, _packet);
    }

    public byte[] ToArray() {
        return _packet;
    }
}