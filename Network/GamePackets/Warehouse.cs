using MTA.Database;
using MTA.Game;
using System;
namespace MTA.Network.GamePackets
{
    public class Warehouse : Writer, Interfaces.IPacket
    {
        public static Client.GameState client;
        public const byte Entire = 0, AddItem = 1, RemoveItem = 2;
        private byte[] buffer;
        public Warehouse(bool Create)
        {
            if (Create)
            {
                buffer = new byte[84 + 8];
                WriteUInt16(84, 0, buffer);
                WriteUInt16(1102, 2, buffer);
            }
        }

        public uint NpcID
        {
            get { return BitConverter.ToUInt32(buffer, 4); }
            set { WriteUInt32(value, 4, buffer); }
        }

        public byte Type
        {
            get
            {
                return buffer[12];
            }
            set
            {
                buffer[12] = value;
            }
        }

        public uint Count
        {
            get { return BitConverter.ToUInt32(buffer, 24); }
            set
            {
                if (value > 20)
                    throw new Exception("Invalid Count value.");
                byte[] Buffer = new byte[8 + 84 + (72 * value)];
                WriteUInt16((ushort)(Buffer.Length - 8), 0, Buffer);
                WriteUInt16(1102, 2, Buffer);
                WriteUInt32(NpcID, 4, Buffer);
                WriteUInt32(Type, 12, Buffer);
                Buffer[13] = buffer[13];
                WriteUInt32(value, 24, Buffer);
                buffer = Buffer;
            }
        }

        public uint UID
        {
            get { return BitConverter.ToUInt32(buffer, 20); }
            set { WriteUInt32(value, 20, buffer); }
        }

        public void Append(ConquerItem item)
        {
            WriteUInt32(item.UID, 28, buffer);
            WriteUInt32(item.ID, 32, buffer);
            WriteByte((byte)item.SocketOne, 37, buffer);
            WriteByte((byte)item.SocketTwo, 38, buffer);
            WriteByte(item.Plus, 45, buffer);
            WriteByte(item.Bless, 46, buffer);
            WriteByte((byte)(item.Bound == true ? 1 : 0), 47, buffer);
            WriteUInt16(item.Enchant, 48, buffer);
            WriteUInt16((ushort)item.Effect, 50, buffer);
            WriteByte((byte)(item.Suspicious == true ? 1 : 0), 52, buffer);
            WriteByte(item.Lock, 54, buffer);
            WriteByte((byte)item.Color, 55, buffer);
            WriteUInt32(item.SocketProgress, 56, buffer);
            WriteUInt32(item.PlusProgress, 60, buffer);
            WriteByte((byte)(item.Inscribed == true ? 1 : 0), 64, buffer);
            WriteUInt16((ushort)item.Mode, 76, buffer);
            WriteUInt16((ushort)item.Durability, 78, buffer);
            WriteUInt16((ushort)item.MaximDurability, 80, buffer);
            WriteUInt32((uint)((item.Days * 24) * 60), 72, buffer);
            WriteUInt32(item.TimeLeftInMinutes, 68, buffer);
        }

        public byte[] ToArray()
        {
            return buffer;
        }

        public void Deserialize(byte[] buffer)
        {
            this.buffer = buffer;
        }

        public void Send(Client.GameState client)
        {
            client.Send(buffer);
        }
    }
}