using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conquer_Online_Server.Network.GamePackets
{
    public enum SpinToWinIcon : byte
    {
        None,
        Meteor,
        Blade,
        DoubleBlades,
        DoubleBladesInCoin,
        ExpBall,
        DragonBall,

    }
    public class SpinToWinResponse : Writer, Interfaces.IPacket
    {
        byte[] buffer;

        public SpinToWinResponse(SpinToWinNPC npc)
        {
            buffer = new byte[24];
            WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
            WriteUInt16(0x548, 2, buffer);
            WriteUInt32((uint)npc, 12, buffer);
        }
        public SpinToWinResponse(byte[] buffer)
        {
            this.buffer = buffer;
        }

        public byte[] Generate()
        {
            WriteByte((byte)Conquer_Online_Server.ServerBase.Kernel.Random.Next(0, 6), 5, buffer);
            WriteByte((byte)Conquer_Online_Server.ServerBase.Kernel.Random.Next(0, 6), 6, buffer);
            WriteByte((byte)Conquer_Online_Server.ServerBase.Kernel.Random.Next(0, 6), 7, buffer);
            return buffer;
        }
        public byte[] End()
        {
            WriteByte(1, 4, buffer);
            return buffer;
        }

        public SpinToWinIcon Icon1
        {
            get
            {
                return (SpinToWinIcon)buffer[5];
            }
            set
            {
                WriteByte((byte)value, 5, buffer);
            }
        }
        public SpinToWinIcon Icon2
        {
            get
            {
                return (SpinToWinIcon)buffer[6];
            }
            set
            {
                WriteByte((byte)value, 6, buffer);
            }
        }
        public SpinToWinIcon Icon3
        {
            get
            {
                return (SpinToWinIcon)buffer[7];
            }
            set
            {
                WriteByte((byte)value, 7, buffer);
            }
        }
        public SpinToWinNPC NpcUId
        {
            get
            {
                return (SpinToWinNPC)BitConverter.ToUInt32(buffer, 12);
            }
            set
            {
                WriteUInt32((uint)value, 12, buffer);
            }
        }

        public byte[] ToArray()
        {
            return buffer;
        }

        public void Deserialize(byte[] buffer)
        {
            this.buffer = buffer;
        }

        public void Send(Client.GameClient client)
        {
            client.Send(buffer);
        }
    }
}
