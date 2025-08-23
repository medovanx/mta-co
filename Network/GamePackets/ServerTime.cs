using System;

namespace MTA.Network.GamePackets
{
    public class ServerTime : Writer, Interfaces.IPacket
    {
        public ServerTime()
        {
            Buffer = new byte[40 + 8];
            WriteUInt16(40, 0, Buffer);
            WriteUInt16(1033, 2, Buffer);
            WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, Buffer);
        }
        byte[] Buffer;
        public uint Year
        {
            get { return BitConverter.ToUInt32(Buffer, 12); }
            set { Writer.WriteUInt32(value - 1900, 12, Buffer); }
        }

        public uint Month
        {
            get { return BitConverter.ToUInt32(Buffer, 16); }
            set { Writer.WriteUInt32(value - 1, 16, Buffer); }
        }

        public uint DayOfYear
        {
            get { return BitConverter.ToUInt32(Buffer, 20); }
            set { Writer.WriteUInt32(value, 20, Buffer); }
        }
        public uint DayOfMonth
        {
            get { return BitConverter.ToUInt32(Buffer, 24); }
            set { Writer.WriteUInt32(value, 24, Buffer); }
        }
        public uint Hour
        {
            get { return BitConverter.ToUInt32(Buffer, 28); }
            set { Writer.WriteUInt32(value, 28, Buffer); }
        }
        public uint Minute
        {
            get { return BitConverter.ToUInt32(Buffer, 32); }
            set { Writer.WriteUInt32(value, 32, Buffer); }
        }
        public uint Second
        {
            get { return BitConverter.ToUInt32(Buffer, 36); }
            set { Writer.WriteUInt32(value, 36, Buffer); }
        }
        public void Deserialize(byte[] buffer)
        {
            this.Buffer = buffer;
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
