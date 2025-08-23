using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MTA.Network.GamePackets
{   
    public class AutoHunt : Writer, Interfaces.IPacket
    {
        public const ushort
        Icon = 0,
        Start = 1,
        Gui = 2,
        End = 3;
        private byte[] Buffer;
        public AutoHunt(bool Create)
        {
            if (Create)
            {
                Buffer = new byte[38 + 8];
                WriteUInt16(38, 0, Buffer);
                WriteUInt16(1070, 2, Buffer);
            }
        }

        public ushort Type
        {
            get { return BitConverter.ToUInt16(Buffer, 4); }
            set { WriteUInt32(value, 4, Buffer); }
        }
        public ushort Show
        {
            get { return BitConverter.ToUInt16(Buffer, 6); }
            set { WriteUInt32(value, 6, Buffer); }
        }
        public ushort EXP
        {
            get { return BitConverter.ToUInt16(Buffer, 14); }
            set { WriteUInt32(value, 14, Buffer); }
        }

        public void Deserialize(byte[] Data)
        {
            Buffer = Data;
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

