using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conquer_Online_Server.Network.GamePackets
{
    public enum SpinToWinRequestType : byte
    {
        Start = 0,
        End = 2
    }

    public enum SpinToWinNPC : uint
    {
        SilverOnearmedBandit = 9817,
        CPOnearmedBanditI = 9826,
        CPOnearmedBanditII = 9827,
        CPOnearmedBanditIII = 9828
    }
    public class SpinToWinPacketRequest : Writer, Interfaces.IPacket
    {
        byte[] buffer;

        public SpinToWinPacketRequest(byte[] buf)
        {
            this.buffer = buf;
        }

        public SpinToWinRequestType RequestType
        {
            get
            {
                return (SpinToWinRequestType)buffer[4];
            }
            set
            {
                WriteByte((byte)value, 4, buffer);
            }
        }
        public byte MoneyType
        {
            get
            {
                return buffer[5];
            }
            set
            {
                WriteByte(value, 5, buffer);
            }
        }
        public SpinToWinNPC NPCID
        {
            get
            {
                return (SpinToWinNPC)BitConverter.ToUInt32(buffer, 8);
            }
            set
            {
                WriteUInt32((uint)value, 8, buffer);
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