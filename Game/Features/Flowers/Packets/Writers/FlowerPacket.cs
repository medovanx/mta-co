using System;
using MTA.Game.Constants;
using MTA.Interfaces;
using MTA.Network;

namespace MTA.Game.Features.Flowers.Packets.Writers;

using Client = Client;
using Flowers = Flowers;

public class FlowerPacket : Writer, IPacket {
    public const ushort Lilies = 1;
    public const ushort Orchids = 2;
    public const ushort RedRoses = 0;
    public const ushort Tulips = 3;

    private byte[] _buffer;

    public FlowerPacket(Flowers clientFlowers, Client.GameState client) {
        _buffer = new byte[0x44];
        WriteUInt16(60, 0, _buffer);
        WriteUInt16(0x47e, 2, _buffer);
        WriteUInt32(2, 4, _buffer);
        if (BodyTypes.IsBoy(client.Entity.Body) || client.Entity.Body == 2003 ||
            client.Entity.Body == 2004) {
            if (DateTime.Now >= client.Entity.Flowers.LastFlowerSent.AddDays(1)) {
                WriteUInt32(30, 16, _buffer);
            }
            else {
                WriteUInt32(0, 4, _buffer);
                WriteUInt32(client.Entity.UID, 8, _buffer);
                WriteUInt32(0, 16, _buffer);
            }
        }
        else {
            if (clientFlowers != null) {
                WriteUInt32(clientFlowers.RedRoses, 16, _buffer);
                WriteUInt32(clientFlowers.RedRosesToday, 20, _buffer);
                WriteUInt32(clientFlowers.Lilies, 24, _buffer);
                WriteUInt32(clientFlowers.LiliesToday, 28, _buffer);
                WriteUInt32(clientFlowers.Orchids, 32, _buffer);
                WriteUInt32(clientFlowers.OrchidsToday, 36, _buffer);
                WriteUInt32(clientFlowers.Tulips, 40, _buffer);
                WriteUInt32(clientFlowers.TulipsToday, 44, _buffer);
            }
        }
    }

    public FlowerPacket(bool create) {
        if (!create) return;
        _buffer = new byte[68];
        WriteUInt16(60, 0, _buffer);
        WriteUInt16(1150, 2, _buffer);
    }

    public uint Type {
        get => System.BitConverter.ToUInt32(_buffer, 4);
        set => WriteUInt32(value, 4, _buffer);
    }

    public uint F {
        get => System.BitConverter.ToUInt32(_buffer, 16);
        set => WriteUInt32(value, 16, _buffer);
    }

    public uint Amount {
        get => System.BitConverter.ToUInt32(_buffer, 20);
        set => WriteUInt32(value, 20, _buffer);
    }

    public FlowerType FlowerType => (FlowerType)System.BitConverter.ToUInt32(_buffer, 0x18);

    public uint ItemUid {
        get => System.BitConverter.ToUInt32(_buffer, 12);
        set => WriteUInt32(value, 12, _buffer);
    }

    public string ReceiverName {
        get => System.BitConverter.ToString(_buffer, 32, 16);
        set => WriteString(value, 32, _buffer);
    }

    public uint SendAmount {
        get => System.BitConverter.ToUInt32(_buffer, 48);
        set => WriteUInt32(value, 48, _buffer);
    }

    public uint Remove {
        get => System.BitConverter.ToUInt32(_buffer, 56);
        set => WriteUInt32(value, 56, _buffer);
    }

    public string SenderName {
        get => System.BitConverter.ToString(_buffer, 16, 16);
        set => WriteString(value, 16, _buffer);
    }

    public FlowerType SendFlowerType {
        get => (FlowerType)System.BitConverter.ToUInt32(_buffer, 0x34);
        set => WriteUInt32((uint)value, 52, _buffer);
    }

    public uint Uid1 {
        get => System.BitConverter.ToUInt32(_buffer, 8);
        set => WriteUInt32(value, 8, _buffer);
    }

    public uint Uid2 {
        get => System.BitConverter.ToUInt32(_buffer, 10);
        set => WriteUInt32(value, 10, _buffer);
    }

    public void Deserialize(byte[] buffer) {
        _buffer = buffer;
    }

    public void Send(Client.GameState client) {
        client.Send(_buffer);
    }

    public byte[] ToArray() {
        return _buffer;
    }
}