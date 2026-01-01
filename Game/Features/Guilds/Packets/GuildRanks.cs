using MTA.Network;

namespace MTA.Game.Features.Guilds.Packets;

public class GuildRanks : Writer {
    private readonly byte[] _packet;
    private ushort _position = 12;

    public GuildRanks(ushort lengthsCount = 0) {
        _packet = new byte[(ushort)(24 + lengthsCount * 68)];
        WriteUInt16((ushort)(_packet.Length - 8), 0, _packet);
        WriteUInt16(2101, 2, _packet);
        WriteUInt16(lengthsCount, 6, _packet); //counts
        WriteUInt16(20, 8, _packet); //registred count(top 20 members)
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

    public byte[] ToArray() {
        return _packet;
    }

    public void Aprend(GuildMember member, ulong donation) {
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