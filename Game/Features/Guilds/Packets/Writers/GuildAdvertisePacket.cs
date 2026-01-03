using MTA.Game.Features.Guilds.Models;
using MTA.Network;

namespace MTA.Game.Features.Guilds.Packets.Writers;

/// <summary>
///     Constructs packet 2226 for guild advertisement listings, displaying guild information for players browsing available guilds.
/// </summary>
public class GuildAdvertisePacket : Writer {
    private readonly byte[] _packet;
    private ushort _position = 24;

    public GuildAdvertisePacket(ushort counts = 0) {
        _packet = new byte[36 + counts * 344];
        WriteUInt16((ushort)(_packet.Length - 8), 0, _packet);
        WriteUInt16(2226, 2, _packet);
        WriteUInt16(counts, 8, _packet);
    }

    public ushort AtCount {
        get => BitConverter.ToUInt16(_packet, 4);
        set => WriteUInt16(value, 4, _packet);
    }

    public ushort AllRegistered {
        get => BitConverter.ToUInt16(_packet, 12);
        set => WriteUInt16(value, 12, _packet);
    }

    public ushort PacketNo {
        get => BitConverter.ToUInt16(_packet, 16);
        set => WriteUInt16(value, 16, _packet);
    }

    /// <summary>
    ///     Adds guild information to the advertisement packet, including name, leader, level, member count, and recruitment settings.
    /// </summary>
    public void Append(Guild guild) {
        WriteUInt32(guild.Id, _position, _packet);
        _position += 4;
        WriteString(guild.AdvertiseRecruit.Bulletin, _position, _packet);
        _position += 255; //9
        WriteString(guild.Name, _position, _packet);
        _position += 36;
        WriteString(guild.LeaderName, _position, _packet);
        _position += 17;
        WriteUInt32(guild.Level, _position, _packet);
        _position += 4;
        WriteUInt32((ushort)guild.MemberCount, _position, _packet);
        _position += 4;
        WriteUInt64(guild.SilverFund, _position, _packet);
        _position += 8;
        WriteByte((byte)(guild.AdvertiseRecruit.AutoJoin ? 1 : 0), _position, _packet);
        _position += 2;
        WriteUInt16((ushort)guild.AdvertiseRecruit.NotAllowFlag, _position, _packet);
        _position += 14; //20, era 14
    }

    /// <summary>
    ///     Returns the complete packet byte array ready to be sent to the client.
    /// </summary>
    public byte[] ToArray() {
        return _packet;
    }
}
