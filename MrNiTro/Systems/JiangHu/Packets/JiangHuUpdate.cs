using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class JiangHuUpdate : Writer
    {
        byte[] packet;
        public JiangHuUpdate()
        {
            packet = new byte[27];
            WriteUInt16(19, 0, packet);
            WriteUInt16(2702, 2, packet);

        }
        public uint FreeCourse { get { return BitConverter.ToUInt32(packet, 4); } set { WriteUInt32(value, 4, packet); } }
        public byte Star { get { return packet[10]; } set { WriteByte(value, 10, packet); } }
        public byte Stage { get { return packet[11]; } set { WriteByte(value, 11, packet); } }
        //public byte Talent { get { return ReadByte(14); } set { WriteByte(value, 14, packet); } }
        public ushort Atribute { get { return BitConverter.ToUInt16(packet, 12); } set { WriteUInt16(value, 12, packet); } }
        public byte FreeTimeTodeyUsed { get { return packet[14]; } set { WriteByte(value, 14, packet); } }
        public uint RoundBuyPoints { get { return BitConverter.ToUInt32(packet, 15); } set { WriteUInt32(value, 15, packet); } }

        public byte[] ToArray()
        {
            return packet;
        }
    }
}
