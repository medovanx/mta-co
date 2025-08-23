using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class GuildRanks : Writer
    {
        byte[] packet;
        public byte[] ToArray() { return packet; }
        public GuildRanks(ushort lenghts_count = 0)
        {
            packet = new byte[(ushort)(24 + lenghts_count * 68)];
            WriteUInt16((ushort)(packet.Length - 8), 0, packet);
            WriteUInt16(2101, 2, packet);
            WriteUInt16(lenghts_count, 6, packet);//counts
            WriteUInt16(20, 8, packet);//registred count(top 20 members)
        }
        public GuildRanks(byte[] buffer)
        {
            packet = buffer;
        }
        public ushort Rank { get { return BitConverter.ToUInt16(packet, 4); } set { WriteUInt16(value, 4, packet); } }
       
        public ushort Page { get { return BitConverter.ToUInt16(packet, 10); } set { WriteUInt16(value, 10, packet); } }
        ushort Position = 12;
        public void Aprend(Game.ConquerStructures.Society.Guild.Member member, ulong Donation)
        {
            WriteUInt32(member.ID, Position, packet);
            Position += 4;
            WriteUInt32((ushort)member.Rank, Position, packet);
            ushort move_pos = (ushort)(4 * Rank);
            Position += (ushort)(8 + move_pos);
            WriteUInt64(Donation, Position, packet);
            Position -= (ushort)(8 + move_pos);
            Position += 44;
            WriteUInt64(Donation, Position, packet);
            Position += 4;
            WriteString(member.Name, Position, packet);
            Position += 16;
        }
    }
}
