using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class Advertise : Writer
    {
        byte[] packet;
        public Advertise(ushort counts = 0)
        {
            packet = new byte[36 + counts * 344];
            WriteUInt16((ushort)(packet.Length - 8), 0, packet);
            WriteUInt16(2226, 2, packet);
            WriteUInt16(counts, 8, packet);
        }
        public ushort AtCount { get { return BitConverter.ToUInt16(packet, 4); } set { WriteUInt16(value, 4, packet); } }
        public ushort AllRegistred { get { return BitConverter.ToUInt16(packet, 12); } set { WriteUInt16(value, 12, packet); } }
        public ushort PacketNo { get { return BitConverter.ToUInt16(packet, 16); } set { WriteUInt16(value, 16, packet); } }
        ushort Position = 24;
        public void Aprend(Game.ConquerStructures.Society.Guild guild)
        {
            WriteUInt32(guild.ID, Position, packet);
            Position += 4;
            WriteString(guild.AdvertiseRecruit.Buletin, Position, packet);
            Position += 255;//9
            WriteString(guild.Name, Position, packet);
            Position += 36;
            WriteString(guild.LeaderName, Position, packet);
            Position += 17;
            WriteUInt32(guild.Level, Position, packet);
            Position += 4;
            WriteUInt32((ushort)guild.MemberCount, Position, packet);
            Position += 4;
            WriteUInt64(guild.SilverFund, Position, packet);
            Position += 8;
            WriteByte((byte)(guild.AdvertiseRecruit.AutoJoin ? 1 : 0), Position, packet);
            Position += 2;
            WriteUInt16((ushort)guild.AdvertiseRecruit.NotAllowFlag, Position, packet);
            Position += 14;//20, era 14

        }
        public byte[] ToArray()
        {
            return packet;
        }
    }
}
