using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    class LotterySystem : Writer, Interfaces.IPacket
    {
        public const ushort Accept = 0, AddJade = 1, Continue = 2, Show = 3;

        byte[] Buffer;
        public LotterySystem(bool Create)
        {
            if (Create)
            {
                Buffer = new byte[28];
                WriteUInt16(20, 0, Buffer);
                WriteUInt16(1314, 2, Buffer);
                WriteByte(3, 4, Buffer);
                WriteByte(3, 10, Buffer);
                WriteByte(2, 5, Buffer);
                WriteByte(2, 6, Buffer);
                //WriteUInt16(3, 12, Buffer);
            }
        }

        public byte Type
        {
            get { return Buffer[4]; }
            set { Buffer[4] = value; }
        }
        public byte SocketGem1
        {
            get { return Buffer[7]; }
            set { Buffer[7] = value; }
        }
        public byte SocketGem2
        {
            get { return Buffer[8]; }
            set { Buffer[8] = value; }
        }
        public byte Plus
        {
            get { return Buffer[9]; }
            set { Buffer[9] = value; }
        }
        public byte ItemColor
        {
            get { return Buffer[10]; }
            set { Buffer[10] = value; }
        }
        public byte JadesAdded
        {
            get { return Buffer[11]; }
            set { Buffer[11] = value; }
        }
        /*public ushort LottoTimes
        {
            get { return BitConverter.ToUInt16(Buffer, 12); }
            set { WriteUInt16(value, 12, Buffer); }
        }*/
        public ushort LottoTimes1
        {
            get { return BitConverter.ToUInt16(Buffer, 13); }
            set { WriteUInt16(value, 13, Buffer); }
        }
        public uint ItemID
        {
            get { return BitConverter.ToUInt32(Buffer, 12); }
            set { WriteUInt32(value, 12, Buffer); }
        }
        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }
        public byte[] ToArray()
        {
            return Buffer;
        }
        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }
    }
}