using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class MsgShowHandActivePlayer : Writer, Interfaces.IPacket
    {
        private byte[] Buffer;
        public MsgShowHandActivePlayer(bool create)
        {
            if (create)
            {
                Buffer = new byte[28 + 8];
                Write((ushort)(Buffer.Length - 8), 0, Buffer);
                Write(2092, 2, Buffer);
            }
        }
        public ushort TimeDown
        {
            get { return BitConverter.ToUInt16(Buffer, 4); }
            set { Write(value, 4, Buffer); }
        }
        public Game.Enums.PokerCallTypes Type
        {
            get { return (Game.Enums.PokerCallTypes)BitConverter.ToUInt16(Buffer, 6); }
            set { Write((ushort)value, 6, Buffer); }
        }
        public ulong MinRaiseAmount
        {
            get { return BitConverter.ToUInt64(Buffer, 8); }
            set { Write(value, 8, Buffer); }
        }
        public ulong MaxRaiseAmount
        {
            get { return BitConverter.ToUInt64(Buffer, 16); }
            set { Write(value, 16, Buffer); }
        }
        public uint UID
        {
            get { return BitConverter.ToUInt32(Buffer, 24); }
            set { Write(value, 24, Buffer); }
        }
        public byte[] ToArray()
        {
            return Buffer;
        }
        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }
        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }
    }
}