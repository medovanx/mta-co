using System;

namespace MTA.Network.GamePackets
{
    public class Vigor : Writer, Interfaces.IPacket
    {
        byte[] Buffer = null;

        public Vigor(bool Create)
        {
            if (Create)
            {
                Buffer = new byte[48];
                WriteUInt16(40, 0, Buffer);
                WriteUInt16(1033, 2, Buffer);
                WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, Buffer);
                Type = 2;
            }
        }

        public uint Type
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32(value, 8, Buffer); }
        }

        public uint Amount
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
            if (Buffer != null)
                client.Send(Buffer);
        }
    }
}
