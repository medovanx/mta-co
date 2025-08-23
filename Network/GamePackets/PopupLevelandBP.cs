using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class PopupLevelandBP : Writer
    {
        public byte[] packet;
        public byte[] ToArray() { return packet; }
        public PopupLevelandBP()
        {
            packet = new byte[40];
            WriteUInt16(0x20, 0, packet);
            WriteUInt16(0x817, 2, packet);
        }
        public uint BattlePower
        {
            get
            {
                return BitConverter.ToUInt32(packet, 0x10);
            }
            set
            {
                WriteUInt32(value, 0x10, packet);
            }
        }

        public uint Level
        {
            get
            {
                return BitConverter.ToUInt32(packet, 12);
            }
            set
            {
                WriteUInt32(value, 12, packet);
            }
        }

        public uint Receiver
        {
            get
            {
                return BitConverter.ToUInt32(packet, 8);
            }
            set
            {
                WriteUInt32(value, 8, packet);
            }
        }

        public uint Requester
        {
            get
            {
                return BitConverter.ToUInt32(packet, 4);
            }
            set
            {
                WriteUInt32(value, 4, packet);
            }
        }
    }
}
