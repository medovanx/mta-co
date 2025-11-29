using MTA.Client;
using MTA.Interfaces;
using System;
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
                return MTA.BitConverter.ToUInt32(this.Buffer, 4);
            }
            set
            {
                Writer.WriteUInt32(value, 4, this.Buffer);
            }
        }
        public uint RankingType
        {
            get
            {
                return MTA.BitConverter.ToUInt32(this.Buffer, 8);
            }
            set
            {
                Writer.WriteUInt32(value, 8, this.Buffer);
            }
        }
        public ushort RegisteredCount
        {
            get
            {
                return MTA.BitConverter.ToUInt16(this.Buffer, 12);
            }
            set
            {
                Writer.WriteUInt16(value, 12, this.Buffer);
            }
        }
        public ushort Page
        {
            get
            {
                return MTA.BitConverter.ToUInt16(this.Buffer, 14);
            }
            set
            {
                Writer.WriteUInt16(value, 14, this.Buffer);
            }
        }
        public uint Count
        {
            get
            {
                return MTA.BitConverter.ToUInt32(this.Buffer, 16);
            }
            set
            {
                Writer.WriteUInt32(value, 16, this.Buffer);
            }
        }
        public GenericRanking(bool Create, uint entries = 1u)
        {
            if (Create)
            {
                this.Buffer = new byte[752];
                Writer.WriteUInt16(744, 0, this.Buffer);
                Writer.WriteUInt16(1151, 2, this.Buffer);
            }
        }
        public void Append(uint rank, uint amount, uint uid, string name)
        {
            int offset = this.current * 72 + 24;
            if (offset + 72 <= this.Buffer.Length)
            {
                this.current++;
                this.Count = (uint)this.current;
                Writer.WriteUInt32(rank, offset, this.Buffer);
                offset += 8;
                Writer.WriteUInt32(amount, offset, this.Buffer);
                offset += 8;
                Writer.WriteUInt32(uid, offset, this.Buffer);
                offset += 4;
                Writer.WriteUInt32(uid, offset, this.Buffer);
                offset += 4;
                Writer.WriteString(name, offset, this.Buffer);
                offset += 16;
                Writer.WriteString(name, offset, this.Buffer);
            }
        }
        public void Append3(uint rank, uint amount, uint uid, string name, byte level = 0, byte Class = 0, uint mesh = 0u, bool toper = false)
        {
            if (!toper)
            {
                int offset = this.current * 72 + 96;
                if (offset + 72 <= this.Buffer.Length)
                {
                    this.current++;
                    this.Count = (uint)this.current;
                    Writer.WriteUInt32(rank, offset, this.Buffer);
                    offset += 8;
                    Writer.WriteUInt32(amount, offset, this.Buffer);
                    offset += 8;
                    Writer.WriteUInt32(uid, offset, this.Buffer);
                    offset += 4;
                    Writer.WriteUInt32(uid, offset, this.Buffer);
                    offset += 4;
                    Writer.WriteString(name, offset, this.Buffer);
                    offset += 16;
                    Writer.WriteString(name, offset, this.Buffer);
                    offset += 16;
                    Writer.WriteUInt32((uint)level, offset, this.Buffer);
                    offset += 4;
                    Writer.WriteUInt32((uint)Class, offset, this.Buffer);
                    offset += 12;
                    Writer.WriteUInt64((ulong)mesh, offset, this.Buffer);
                    offset += 8;
                }
            }
            else
            {
                int offset = 24;
                Writer.WriteUInt32(1u, offset, this.Buffer);
                offset += 8;
                Writer.WriteUInt32(amount, offset, this.Buffer);
                offset += 8;
                Writer.WriteUInt32(80000000u, offset, this.Buffer);
                offset += 4;
                Writer.WriteUInt32(uid, offset, this.Buffer);
                offset += 4;
                Writer.WriteString(name, offset, this.Buffer);
                offset += 16;
                Writer.WriteString(name, offset, this.Buffer);
                offset += 16;
                Writer.WriteUInt32((uint)level, offset, this.Buffer);
                offset += 4;
                Writer.WriteUInt32((uint)Class, offset, this.Buffer);
                offset += 12;
                Writer.WriteUInt64((ulong)mesh, offset, this.Buffer);
                offset += 8;
            }
        }
        public void Append2(uint rank, uint amount, uint uid, string name, byte level, ushort Class, uint mesh)
        {
            int offset = this.current * 72 + 24;
            if (offset + 72 <= this.Buffer.Length)
            {
                this.current++;
                this.Count = (uint)this.current;
                Writer.WriteUInt32(rank, offset, this.Buffer);
                offset += 8;
                Writer.WriteUInt32(amount, offset, this.Buffer);
                offset += 8;
                Writer.WriteUInt32(uid, offset, this.Buffer);
                offset += 4;
                Writer.WriteUInt32(uid, offset, this.Buffer);
                offset += 4;
                Writer.WriteString(name, offset, this.Buffer);
                offset += 16;
                Writer.WriteString(name, offset, this.Buffer);
                offset += 16;
                Writer.WriteUInt32((uint)level, offset, this.Buffer);
                offset += 4;
                Writer.WriteUInt32((uint)Class, offset, this.Buffer);
                offset += 4;
                Writer.WriteUInt32(mesh, offset, this.Buffer);
                offset += 4;
            }
        }
        public void Reset()
        {
            this.current = 0;
        }
        public void Send(GameState client)
        {
            client.Send(this.Buffer);
        }
        public byte[] ToArray()
        {
            return this.Buffer;
        }
        public void Deserialize(byte[] _buffer)
        {
            this.Buffer = _buffer;
            if (this.Count == 0u)
            {
                byte[] buffer = new byte[104];
                this.Buffer.CopyTo(buffer, 0);
                Writer.WriteUInt16(96, 0, buffer);
                this.Buffer = buffer;
            }
        }

        internal void AppendP(uint p, uint p_2, uint p_3, string p_4, byte p_5, byte p_6, uint p_7)
        {
            throw new NotImplementedException();
        }

        internal void Append2(int p, uint p_2, uint p_3, string p_4, byte p_5, byte p_6, uint p_7, bool p_8)
        {
            throw new NotImplementedException();
        }
    }
}