using System;
using System.IO;
using MTA.Database;
using MTA.Network;

namespace MTA.Game.Features.Flowers;

public class FlowerSpawn : Writer {
    private readonly byte[] _buffer;

    public FlowerSpawn(string type, string name, string flowers, string uid, uint flowerId) {
        var send = type + " " + flowers + " " + uid + " " + uid + " " + name + " " + name + "";
        _buffer
            = new byte[88]; //18
        WriteUInt16(80, 0, _buffer);
        WriteUInt16(1151, 2, _buffer);

        _buffer[4] = 2;
        WriteUInt32(flowerId, 8, _buffer);
        _buffer[16] = 1;
        _buffer[24] = 1;
        _buffer[32] = 1;

        WriteUInt32(uint.Parse(uid), 40, _buffer);
        WriteUInt32(uint.Parse(uid), 44, _buffer);
        //Buffer[17] = 1;//13
        // Buffer[18] = (byte)(send.Length & 255);
        for (var i = 0; i < send.Length; i++) {
            try {
                _buffer[48 + i] = Convert.ToByte(send[i]);
                _buffer[48 + i + 16] = Convert.ToByte(send[i]);
            }
            catch { }
        }
    }

    public byte[] ThePacket() {
        return _buffer;
    }
}

public class FlowerRankLegacy : Writer {
    private readonly byte[] _buffer;

    public FlowerRankLegacy(uint uid) {
        var packetLength = 72;
        uint charAmount = 0;
        uint place = 1;
        var playerNames = new string[10];
        var playerFlowers = new uint[10];
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);
        var cmd = new MySqlCommand(MySqlCommandType.SELECT);
        cmd.Select("flowers").Order("redroses DESC");
        var r = new MySqlReader(cmd);
        while (r.Read()) {
            var redroses = r.ReadInt32("redroses");
            if (redroses != 0) {
                var charuid = r.ReadUInt32("id");
                packetLength += r.ReadString("name").Length * 2 + 36;
                playerNames[charAmount] = r.ReadString("name");
                playerFlowers[charAmount] = r.ReadUInt32("redroses");
                charAmount++;
            }
        }

        _buffer = new byte[packetLength];
        WriteUInt16(72, 0, _buffer);
        WriteUInt16(1150, 2, _buffer);

        WriteUInt32(1, 4, _buffer);
        WriteUInt32(uid, 8, _buffer);
        WriteUInt32(0, 12, _buffer);
        WriteUInt32(charAmount, 16, _buffer);
        var position = 20;
        for (var x = 1; x < charAmount; x++) {
            WriteUInt32(0, position, _buffer);
            position += 4;
            WriteUInt32(place, position, _buffer);
            place++;
            WriteUInt32(0, position, _buffer);
            position += 4;
            WriteUInt32(playerFlowers[1], position, _buffer);
            position += 4;
            WriteUInt32(0, position, _buffer);
            position += 4;
            WriteUInt32(2301694, position, _buffer);
            position += 4;
            WriteUInt32(2301694, position, _buffer);
            position += 4;
            WriteString(playerNames[x], position, _buffer);
            position += 16;
            WriteUInt32(0, position, _buffer);
            position += 4;
            WriteString(playerNames[x], position, _buffer);
        }

        WriteString("TQServer", position, _buffer);
    }

    public byte[] ThePacket() {
        return _buffer;
    }
}