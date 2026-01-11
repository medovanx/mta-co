using System.Text;
using MTA.Network;

namespace MTA.Game.Features.Flowers.Packets.Writers;

public class SendFlower : Writer {
    public const uint FlowerSender = 2;
    public const uint Flower = 3;
    private readonly byte[] _packet;

    public SendFlower() {
        _packet = new byte[68];
        WriteUInt16(60, 0, _packet);
        WriteUInt16(1150, 2, _packet);
    }

    public uint Typing {
        get => BitConverter.ToUInt32(_packet, 4);
        set => WriteUInt32(value, 4, _packet);
    }

    public string SenderName {
        get => Encoding.ASCII.GetString(_packet, 16, 16);
        set => WriteString(value, 16, _packet);
    }

    public string ReceiverName {
        get => Encoding.ASCII.GetString(_packet, 32, 16);
        set => WriteString(value, 32, _packet);
    }

    public uint Amount {
        get => BitConverter.ToUInt32(_packet, 48);
        set => WriteUInt32(value, 48, _packet);
    }

    public uint FType {
        get => BitConverter.ToUInt32(_packet, 52);
        set => WriteUInt32(value, 52, _packet);
    }

    public uint Effect {
        get => BitConverter.ToUInt32(_packet, 56);
        set => WriteUInt32(value, 56, _packet);
    }

    public byte[] ToArray() {
        return _packet;
    }

    public void Append(Flowers flowers) {
        WriteUInt32(flowers.RedRoses, 16, _packet);
        WriteUInt32(flowers.RedRosesToday, 20, _packet);
        WriteUInt32(flowers.Lilies, 24, _packet);
        WriteUInt32(flowers.Lilies2day, 28, _packet);
        WriteUInt32(flowers.Orchids, 32, _packet);
        WriteUInt32(flowers.OrchidsToday, 36, _packet);
        WriteUInt32(flowers.Tulips, 40, _packet);
        WriteUInt32(flowers.TulipsToday, 44, _packet);
    }
}