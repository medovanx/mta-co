using MTA.Client;
using MTA.Interfaces;

namespace MTA.Network.GamePackets
{
    public class GenericRanking : Writer, IPacket
    {
        public const uint Ranking = 1u;
        public const uint QueryCount = 2u;
        public const uint InformationRequest = 5u;
        public const uint RoseFairy = 30000002u;
        public const uint LilyFairy = 30000102u;
        public const uint OrchidFairy = 30000202u;
        public const uint TulipFairy = 30000302u;
        public const uint KissFairy = 30000402u;
        public const uint LoveFairy = 30000502u;
        public const uint TineFairy = 30000602u;
        public const uint JadeFairy = 30000702u;
        public const uint Chi = 60000000u;
        public const uint DragonChi = 60000001u;
        public const uint PhoenixChi = 60000002u;
        public const uint TigerChi = 60000003u;
        public const uint TurtleChi = 60000004u;
        public const uint Prestige = 80000000u;
        private byte[] Buffer;
        private int current;
        public uint Mode
        {
            get
            {
                return BitConverter.ToUInt32(Buffer, 4);
            }
            set
            {
                WriteUInt32(value, 4, Buffer);
            }
        }
        public uint RankingType
        {
            get
            {
                return BitConverter.ToUInt32(Buffer, 8);
            }
            set
            {
                WriteUInt32(value, 8, Buffer);
            }
        }
        public ushort RegisteredCount
        {
            get
            {
                return BitConverter.ToUInt16(Buffer, 12);
            }
            set
            {
                WriteUInt16(value, 12, Buffer);
            }
        }
        public ushort Page
        {
            get
            {
                return BitConverter.ToUInt16(Buffer, 14);
            }
            set
            {
                WriteUInt16(value, 14, Buffer);
            }
        }
        public uint Count
        {
            get
            {
                return BitConverter.ToUInt32(Buffer, 16);
            }
            set
            {
                WriteUInt32(value, 16, Buffer);
            }
        }
        public GenericRanking(bool Create, uint entries = 1u)
        {
            if (Create)
            {
                Buffer = new byte[752];
                WriteUInt16(744, 0, Buffer);
                WriteUInt16(1151, 2, Buffer);
            }
        }
        public void Append(uint rank, uint amount, uint uid, string name)
        {
            int offset = current * 72 + 24;
            if (offset + 72 <= Buffer.Length)
            {
                current++;
                Count = (uint)current;
                WriteUInt32(rank, offset, Buffer);
                offset += 8;
                WriteUInt32(amount, offset, Buffer);
                offset += 8;
                WriteUInt32(uid, offset, Buffer);
                offset += 4;
                WriteUInt32(uid, offset, Buffer);
                offset += 4;
                WriteString(name, offset, Buffer);
                offset += 16;
                WriteString(name, offset, Buffer);
            }
        }
        public void Append3(uint rank, uint amount, uint uid, string name, byte level = 0, byte Class = 0, uint mesh = 0u, bool toper = false)
        {
            if (!toper)
            {
                int offset = current * 72 + 96;
                if (offset + 72 <= Buffer.Length)
                {
                    current++;
                    Count = (uint)current;
                    WriteUInt32(rank, offset, Buffer);
                    offset += 8;
                    WriteUInt32(amount, offset, Buffer);
                    offset += 8;
                    WriteUInt32(uid, offset, Buffer);
                    offset += 4;
                    WriteUInt32(uid, offset, Buffer);
                    offset += 4;
                    WriteString(name, offset, Buffer);
                    offset += 16;
                    WriteString(name, offset, Buffer);
                    offset += 16;
                    WriteUInt32(level, offset, Buffer);
                    offset += 4;
                    WriteUInt32(Class, offset, Buffer);
                    offset += 12;
                    WriteUInt64(mesh, offset, Buffer);
                    offset += 8;
                }
            }
            else
            {
                int offset = 24;
                WriteUInt32(1u, offset, Buffer);
                offset += 8;
                WriteUInt32(amount, offset, Buffer);
                offset += 8;
                WriteUInt32(80000000u, offset, Buffer);
                offset += 4;
                WriteUInt32(uid, offset, Buffer);
                offset += 4;
                WriteString(name, offset, Buffer);
                offset += 16;
                WriteString(name, offset, Buffer);
                offset += 16;
                WriteUInt32(level, offset, Buffer);
                offset += 4;
                WriteUInt32(Class, offset, Buffer);
                offset += 12;
                WriteUInt64(mesh, offset, Buffer);
                offset += 8;
            }
        }
        public void Append2(uint rank, uint amount, uint uid, string name, byte level, ushort Class, uint mesh)
        {
            int offset = current * 72 + 24;
            if (offset + 72 <= Buffer.Length)
            {
                current++;
                Count = (uint)current;
                WriteUInt32(rank, offset, Buffer);
                offset += 8;
                WriteUInt32(amount, offset, Buffer);
                offset += 8;
                WriteUInt32(uid, offset, Buffer);
                offset += 4;
                WriteUInt32(uid, offset, Buffer);
                offset += 4;
                WriteString(name, offset, Buffer);
                offset += 16;
                WriteString(name, offset, Buffer);
                offset += 16;
                WriteUInt32(level, offset, Buffer);
                offset += 4;
                WriteUInt32(Class, offset, Buffer);
                offset += 4;
                WriteUInt32(mesh, offset, Buffer);
                offset += 4;
            }
        }
        public void Reset()
        {
            current = 0;
        }
        public void Send(GameState client)
        {
            client.Send(Buffer);
        }
        public byte[] ToArray()
        {
            return Buffer;
        }
        public void Deserialize(byte[] _buffer)
        {
            Buffer = _buffer;
            if (Count == 0u)
            {
                byte[] buffer = new byte[104];
                Buffer.CopyTo(buffer, 0);
                WriteUInt16(96, 0, buffer);
                Buffer = buffer;
            }
        }

        internal void AppendP(uint p, uint p_2, uint p_3, string p_4, byte p_5, byte p_6, uint p_7)
        {
            int offset = current * 72 + 24;
            if (offset + 72 <= Buffer.Length)
            {
                current++;
                Count = (uint)current;
                WriteUInt32(p, offset, Buffer);
                offset += 8;
                WriteUInt32(p_2, offset, Buffer);
                offset += 8;
                WriteUInt32(p_3, offset, Buffer);
                offset += 4;
                WriteUInt32(p_3, offset, Buffer);
                offset += 4;
                WriteString(p_4, offset, Buffer);
                offset += 16;
                WriteString(p_4, offset, Buffer);
                offset += 16;
                WriteUInt32(p_5, offset, Buffer);
                offset += 4;
                WriteUInt32(p_6, offset, Buffer);
                offset += 4;
                WriteUInt32(p_7, offset, Buffer);
                offset += 4;
            }
        }

        internal void Append2(int p, uint p_2, uint p_3, string p_4, byte p_5, byte p_6, uint p_7, bool p_8)
        {
            if (p_8) // Top player - write at fixed offset 24
            {
                int offset = 24;
                WriteUInt32((uint)p, offset, Buffer);
                offset += 8;
                WriteUInt32(p_2, offset, Buffer);
                offset += 8;
                WriteUInt32(p_3, offset, Buffer);
                offset += 4;
                WriteUInt32(p_3, offset, Buffer);
                offset += 4;
                WriteString(p_4, offset, Buffer);
                offset += 16;
                WriteString(p_4, offset, Buffer);
                offset += 16;
                WriteUInt32(p_5, offset, Buffer);
                offset += 4;
                WriteUInt32(p_6, offset, Buffer);
                offset += 4;
                WriteUInt32(p_7, offset, Buffer);
                offset += 4;
            }
            else // Regular list item
            {
                int offset = current * 72 + 24;
                if (offset + 72 <= Buffer.Length)
                {
                    current++;
                    Count = (uint)current;
                    WriteUInt32((uint)p, offset, Buffer);
                    offset += 8;
                    WriteUInt32(p_2, offset, Buffer);
                    offset += 8;
                    WriteUInt32(p_3, offset, Buffer);
                    offset += 4;
                    WriteUInt32(p_3, offset, Buffer);
                    offset += 4;
                    WriteString(p_4, offset, Buffer);
                    offset += 16;
                    WriteString(p_4, offset, Buffer);
                    offset += 16;
                    WriteUInt32(p_5, offset, Buffer);
                    offset += 4;
                    WriteUInt32(p_6, offset, Buffer);
                    offset += 4;
                    WriteUInt32(p_7, offset, Buffer);
                    offset += 4;
                }
            }
        }
    }
}