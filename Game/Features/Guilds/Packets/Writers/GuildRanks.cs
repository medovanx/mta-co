using MTA.Game.Features.Guilds.Models;
using MTA.Network;

namespace MTA.Game.Features.Guilds.Packets.Writers;

/// <summary>
///     Constructs packet 2101 for guild donation rankings display, showing top members by donation type (Silver, CP, PK, Flowers, etc.).
/// </summary>
public class GuildRanks : Writer {
    private readonly byte[] _packet;
    private ushort _position = 12;

    public GuildRanks(ushort lengthsCount = 0) {
        _packet = new byte[(ushort)(24 + lengthsCount * 68)];
        WriteUInt16((ushort)(_packet.Length - 8), 0, _packet);
        WriteUInt16((ushort)Game.Constants.Packets.MsgFactionRankInfo, 2, _packet);
        WriteUInt16(lengthsCount, 6, _packet); //counts
        WriteUInt16(20, 8, _packet); //registered count(top 20 members)
    }

    public GuildRanks(byte[] buffer) {
        _packet = buffer;
    }

    public ushort Rank {
        get => BitConverter.ToUInt16(_packet, 4);
        init => WriteUInt16(value, 4, _packet);
    }

    public ushort Page {
        get => BitConverter.ToUInt16(_packet, 10);
        set => WriteUInt16(value, 10, _packet);
    }

    /// <summary>
    ///     Returns the packet byte array ready to be sent to the client.
    /// </summary>
    public byte[] ToArray() {
        return _packet;
    }

    /// <summary>
    ///     Adds a member to the ranking list with their donation amount for the specified rank type.
    /// </summary>
    public void Append(GuildMember member, ulong donation) {
        WriteUInt32(member.Id, _position, _packet);
        _position += 4;
        WriteUInt32((ushort)member.Rank, _position, _packet);
        var movePos = (ushort)(4 * Rank);
        _position += (ushort)(8 + movePos);
        WriteUInt64(donation, _position, _packet);
        _position -= (ushort)(8 + movePos);
        _position += 44;
        WriteUInt64(donation, _position, _packet);
        _position += 4;
        WriteString(member.Name, _position, _packet);
        _position += 16;
    }
}