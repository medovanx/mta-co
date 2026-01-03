using MTA.Client;
using MTA.Game.Features.Guilds.Models;
using MTA.Interfaces;
using MTA.Network;

namespace MTA.Game.Features.Guilds.Packets.Writers;

/// <summary>
///     Constructs packet 2201 for the guild arsenal overview tab, showing all arsenal slots and their shared battle power.
/// </summary>
public class GuildArsenalTabPacket : Writer, IPacket {
    private byte[] _buffer;

    public GuildArsenalTabPacket(bool create) {
        _buffer = new byte[252];
        if (!create) return;
        WriteUInt16(244, 0, _buffer);
        WriteUInt16(2201, 2, _buffer);
    }

    public uint Type {
        get => BitConverter.ToUInt32(_buffer, 4);
        set => WriteUInt32(value, 4, _buffer);
    }

    public uint SharedBattlePower {
        get => BitConverter.ToUInt32(_buffer, 8);
        set => WriteUInt32(value, 8, _buffer);
    }

    public uint DwParam {
        get => BitConverter.ToUInt32(_buffer, 8);
        set => WriteUInt32(value, 8, _buffer);
    }

    public uint HeroDonation {
        get => BitConverter.ToUInt32(_buffer, 12);
        set => WriteUInt32(value, 12, _buffer);
    }

    public uint DwParam2 {
        get => BitConverter.ToUInt32(_buffer, 12);
        set => WriteUInt32(value, 12, _buffer);
    }

    public uint HeroSharedBattlePower {
        get => BitConverter.ToUInt32(_buffer, 16);
        set => WriteUInt32(value, 16, _buffer);
    }

    public uint ArsenalCount {
        get => BitConverter.ToUInt32(_buffer, 20);
        set => WriteUInt32(value, 20, _buffer);
    }

    /// <summary>
    ///     Sends the arsenal tab packet to the client, displaying the complete arsenal overview.
    /// </summary>
    public void Send(GameState client) {
        client.Send(_buffer);
    }

    /// <summary>
    ///     Parses incoming packet data from the client for viewing the arsenal tab.
    /// </summary>
    public void Deserialize(byte[] buffer) {
        _buffer = buffer;
    }

    /// <summary>
    ///     Returns the packet byte array ready to be sent to the client.
    /// </summary>
    public byte[] ToArray() {
        return _buffer;
    }

    /// <summary>
    ///     Adds arsenal slot information to the tab packet, including shared battle power, enhancement, donation, and unlock status.
    /// </summary>
    public void AppendArsenal(Arsenal arsenal) {
        var offset = 28 + 24 * (arsenal.Position - 1);
        if (!arsenal.Unlocked) {
            offset += 16;
        }
        else {
            WriteUInt32(arsenal.SharedBattlePower, offset, _buffer);
            offset += 4;
            WriteUInt32(arsenal.Enhancement, offset, _buffer);
            offset += 4;
            WriteUInt32(arsenal.Donation, offset, _buffer);
            offset += 4;
            WriteInt32(arsenal.EnhancementExpirationDate(), offset, _buffer);
            offset += 4;
        }

        WriteInt32(arsenal.Unlocked ? 1 : 0, offset, _buffer);
        offset += 4;
        WriteUInt32(arsenal.Position, offset, _buffer);
    }
}