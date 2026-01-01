using MTA.Game.Features.Guilds;
using MTA.Network;

namespace MTA.Game.Features.Guilds.Packets {
    public class GuildRanks : Writer {
        byte[] packet;
        ushort Position = 12;

        public GuildRanks(ushort lenghts_count = 0) {
            packet = new byte[(ushort)(24 + lenghts_count * 68)];
            WriteUInt16((ushort)(packet.Length - 8), 0, packet);
            WriteUInt16(2101, 2, packet);
            WriteUInt16(lenghts_count, 6, packet); //counts
            WriteUInt16(20, 8, packet); //registred count(top 20 members)
        }

        public GuildRanks(byte[] buffer) {
            packet = buffer;
        }

        public ushort Rank {
            get => BitConverter.ToUInt16(packet, 4);
            set => WriteUInt16(value, 4, packet);
        }

        public ushort Page {
            get => BitConverter.ToUInt16(packet, 10);
            set => WriteUInt16(value, 10, packet);
        }

        public byte[] ToArray() {
            return packet;
        }

        public void Aprend(Guild.Member member, ulong Donation) {
            WriteUInt32(member.Id, Position, packet);
            Position += 4;
            WriteUInt32((ushort)member.Rank, Position, packet);
            var move_pos = (ushort)(4 * Rank);
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