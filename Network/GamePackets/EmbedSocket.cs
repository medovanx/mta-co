using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class EmbedSocket : Writer, Interfaces.IPacket
    {
        public const ushort Add = 0,
                            Remove = 1,
                            SlotOne = 1,
                            SlotTwo = 2;

        byte[] Buffer;

        public EmbedSocket(bool Create)
        {
            if (Create)
            {
                Buffer = null;
            }
        }

        public uint ItemUID
        {
            get { return BitConverter.ToUInt32(Buffer, 12); }
            set { WriteUInt32(value, 12, Buffer); }
        }

        public uint GemUID
        {
            get { return BitConverter.ToUInt32(Buffer, 16); }
            set { WriteUInt32(value, 16, Buffer); }
        }

        public ushort Slot
        {
            get { return BitConverter.ToUInt16(Buffer, 20); }
            set { WriteUInt16(value, 20, Buffer); }
        }

        public ushort Mode
        {
            get { return BitConverter.ToUInt16(Buffer, 22); }
            set { WriteUInt16(value, 22, Buffer); }
        }
        
        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }

        public byte[] ToArray()
        {
            return Buffer;
        }

        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }
    }
}
