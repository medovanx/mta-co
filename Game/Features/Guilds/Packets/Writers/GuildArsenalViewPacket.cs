using MTA.Client;
using MTA.Game.Features.Guilds.Models;
using MTA.Interfaces;
using MTA.Network;

namespace MTA.Game.Features.Guilds.Packets.Writers;

/// <summary>
///     Constructs packet 2202 for viewing guild arsenal items, displaying inscribed items with their battle power and donation information.
/// </summary>
public class GuildArsenalViewPacket : Writer, IPacket {
    public const uint
        Unlock = 0,
        Inscribe = 1,
        View = 4;

    private byte[] _buffer;

    public GuildArsenalViewPacket(bool create, uint itemCount = 0) {
        _buffer = new byte[56 + itemCount * 40];
        if (!create) return;
        WriteUInt16((ushort)(48 + itemCount * 40), 0, _buffer);
        WriteUInt16(2202, 2, _buffer);
    }

    public uint Type {
        get => BitConverter.ToUInt32(_buffer, 4);
        set => WriteUInt32(value, 4, _buffer);
    }

    public uint BeginAt {
        get => BitConverter.ToUInt32(_buffer, 8);
        init => WriteUInt32(value, 8, _buffer);
    }

    public uint EndAt {
        get => BitConverter.ToUInt32(_buffer, 12);
        set => WriteUInt32(value, 12, _buffer);
    }

    public uint ArsenalType {
        get => BitConverter.ToUInt32(_buffer, 16);
        set => WriteUInt32(value, 16, _buffer);
    }

    public int TotalInscribed {
        get => BitConverter.ToInt32(_buffer, 20);
        set => WriteInt32(value, 20, _buffer);
    }

    public uint SharedBattlePower {
        get => BitConverter.ToUInt32(_buffer, 24);
        set => WriteUInt32(value, 24, _buffer);
    }

    public uint Enchantment {
        get => BitConverter.ToUInt32(_buffer, 28);
        set => WriteUInt32(value, 28, _buffer);
    }

    public int EnchantmentExpirationDate {
        get => BitConverter.ToInt32(_buffer, 32);
        set => WriteInt32(value, 32, _buffer);
    }

    public uint Donation {
        get => BitConverter.ToUInt32(_buffer, 36);
        set => WriteUInt32(value, 36, _buffer);
    }

    public uint Count {
        get => BitConverter.ToUInt32(_buffer, 40);
        set => WriteUInt32(value, 40, _buffer);
    }

    /// <summary>
    ///     Sends the arsenal view packet to the client, displaying the current page of inscribed items.
    /// </summary>
    public void Send(GameState client) {
        client.Send(_buffer);
    }

    /// <summary>
    ///     Parses incoming packet data from the client for viewing specific arsenal pages.
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
    ///     Adds an inscribed item to the view packet, including its stats, battle power, and donation worth.
    /// </summary>
    public void AppendItem(Arsenal.ArsenalItem item) {
        var offset = (int)(44 + 40 * Count);
        Count++;
        WriteUInt32(item.Uid, offset, _buffer);
        offset += 4;
        WriteUInt32(item.Rank, offset, _buffer);
        offset += 4;
        WriteString(item.Owner, offset, _buffer);
        offset += 16;
        WriteUInt32(item.Id, offset, _buffer);
        offset += 4;
        _buffer[offset] = (byte)(item.Id % 10);
        offset++;
        _buffer[offset] = item.Plus;
        offset++;
        _buffer[offset] = item.SocketOne;
        offset++;
        _buffer[offset] = item.SocketTwo;
        offset++;
        WriteUInt32(item.BattlePower, offset, _buffer);
        offset += 4;
        WriteUInt32(item.DonationWorth, offset, _buffer);
    }
}