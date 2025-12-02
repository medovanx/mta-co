using System;
using System.Collections.Generic;
using MTA.Client;
using MTA.Network;

namespace MTA.Network.GamePackets
{
    public class MsgGoldLeaguePoint : Writer, Interfaces.IPacket
    {
        public const uint Defult = 8;
        public uint Points;
        public uint Points2;
        private byte[] Buffer;
        public MsgGoldLeaguePoint()
        {
            Buffer = new byte[10 + 8];
            WriteUInt32(10, 0, Buffer);
            WriteUInt32((ushort)2600, 2, Buffer);
        }
        public byte[] ToArray()
        {
            byte[] ptr = CreateProtocolBuffer(Points, Points2);
            byte[] Buffer = new byte[11 + ptr.Length];
            WriteUInt16((ushort)(Buffer.Length - 8), 0, Buffer);
            WriteUInt16(2600, 2, Buffer);
            WriteUInt32(8, 4, Buffer);
            Array.Copy(ptr, 0, Buffer, 4, ptr.Length);
            return Buffer;
        }

        public static byte[] CreateProtocolBuffer(params uint[] values)
        {
            List<byte> ptr = new List<byte>();
            ptr.Add(8);
            for (int x = 0; x < values.Length; x++)
            {
                uint value = values[x];
                while (value > 0x7F)
                {
                    ptr.Add((byte)((value & 0x7F) | 0x80));
                    value >>= 7;
                }
                ptr.Add((byte)(value & 0x7F));
                ptr.Add((byte)(8 * (x + 2)));
                if (x + 1 == values.Length)
                    break;
            }
            return ptr.ToArray();
        }
        public void Deserialize(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public void Send(GameState client)
        {
            client.Send(Buffer);
        }
    }
}

