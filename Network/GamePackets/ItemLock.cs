using System;

namespace MTA.Network.GamePackets
{
    public class ItemLock : Writer, Interfaces.IPacket
    {
        public const uint RequestLock = 0, RequestUnlock = 1, UnlockDate = 2;

        byte[] Buffer;

        public ItemLock(bool Create)
        {
            if (Create)
            {
                Buffer = new byte[24];
                Writer.Write(16, 0, Buffer);
                Writer.Write(2048, 2, Buffer);
            }
        }
        public uint UID
        {
            get { return BitConverter.ToUInt32(Buffer, 4); }
            set { Write(value, 4, Buffer); }
        }
        public uint ID
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { Write(value, 8, Buffer); }
        }
        public uint dwParam
        {
            get { return BitConverter.ToUInt32(Buffer, 12); }
            set { Write(value, 12, Buffer); }
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
