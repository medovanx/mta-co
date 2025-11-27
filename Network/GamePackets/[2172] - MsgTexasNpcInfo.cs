using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class MsgTexasNpcInfo : Writer, Interfaces.IPacket
    {
        private byte[] Buffer;
        public MsgTexasNpcInfo(int count)
        {
            Buffer = new byte[52 + 8 + (count * 6)];
            Write((ushort)(Buffer.Length - 8), 0, Buffer);
            Write(2172, 2, Buffer);
            PlayersCount = (byte)count;
        }
        //Table UID
        public uint UID
        {
            get { return BitConverter.ToUInt32(Buffer, 4); }
            set { Write(value, 4, Buffer); }
        }
        //Table X coord
        public ushort X
        {
            get { return BitConverter.ToUInt16(Buffer, 16); }
            set { Write(value, 16, Buffer); }
        }
        //Table Y Coord
        public ushort Y
        {
            get { return BitConverter.ToUInt16(Buffer, 18); }
            set { Write(value, 18, Buffer); }
        }
        //Table Mesh
        public uint Mesh
        {
            get { return BitConverter.ToUInt32(Buffer, 20); }
            set { Write(value, 20, Buffer); }
        }
        //table nomber
        public byte Number
        {
            get { return Buffer[26]; }
            set { Buffer[26] = value; }
        }
        public bool TableType
        {
            get { return Buffer[27] == 1; }
            set { Buffer[27] = (byte)(value ? 1 : 0); }
        }
        //Limited=0    Unlimited=1
        public bool Unlimited
        {
            get { return Buffer[30] == 1; }
            set { Buffer[30] = (byte)(value ? 1 : 0); }
        }
        //1=Silver 0=CPs
        public byte BetType
        {
            get { return Buffer[34]; }
            set { Buffer[34] = value; }
        }
        public uint MinLimit
        {
            get { return BitConverter.ToUInt32(Buffer, 38); }
            set { Write(value, 38, Buffer); }
        }
        public byte State
        {
            get { return Buffer[42]; }
            set { Buffer[42] = value; }
        }
        public ulong Pot
        {
            get { return BitConverter.ToUInt64(Buffer, 43); }
            set { Write(value, 43, Buffer); }
        }
        public byte PlayersCount
        {
            get { return Buffer[51]; }
            set { Buffer[51] = value; }
        }
        int offset = 52;
        public void Append(Game.ConquerStructures.PokerPlayer _PokerPlayer)
        {
            Write(_PokerPlayer.UID, offset, Buffer);
            offset += 4;
            Buffer[offset] = _PokerPlayer.Seat;
            offset += 1;
            Buffer[offset] = (byte)(_PokerPlayer.Online ? 1 : 0);
            offset += 1;
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
