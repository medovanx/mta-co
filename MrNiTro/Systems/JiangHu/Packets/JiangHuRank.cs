using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class JiangHuRank : Writer
    {
        byte[] packet;
        public JiangHuRank(byte entry = 1)
        {
            packet = new byte[16 + entry * 41];
            WriteUInt16((ushort)(packet.Length - 8), 0, packet);
            WriteUInt16(2703, 2, packet);

            WriteByte(entry, 6, packet);//count on page
        }
        public byte Page { get { return packet[4]; } set { packet[4] = value; } }
        public byte MyRank { get { return packet[5]; } set { packet[5] = value; } }
      
        public byte RegisteredCount { get { return packet[7]; } set { WriteByte(value, 7, packet); } }

        private ushort Position = 8;
        public void Appren(byte Rank, uint Inner_Strength, uint Level, string Name, string CustomizedName)
        {
            WriteByte(Rank, Position, packet); Position++;
            WriteUInt32(Inner_Strength, Position, packet); Position += 4;
            WriteUInt32(Level, Position, packet); Position += 4;
            WriteString(Name, Position, packet); Position += 16;
            WriteString(CustomizedName, Position, packet); Position += 16;
        }
        public byte[] ToArray()
        {
            return packet;
        }
    }
}
