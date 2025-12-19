using MTA.Game.Features.Tournaments;

namespace MTA.Network.GamePackets {
    public class SkillEliteSetTeamName : Writer {
        public const byte Apprend = 0, SuccessfulName = 1, RenameWasSuccessfulName = 2, Remove = 3;
        byte[] packet;

        public SkillEliteSetTeamName(TeamElitePk.GamePackets ID) {
            packet = new byte[56];
            WriteUInt16(48, 0, packet);
            if (ID == TeamElitePk.GamePackets.SkillElitePkBrackets)
                WriteUInt16((ushort)TeamElitePk.GamePackets.SkillEliteSetTeamName, 2, packet);
            else
                WriteUInt16((ushort)TeamElitePk.GamePackets.TeamEliteSetTeamName, 2, packet);
        }

        public uint Type {
            get { return BitConverter.ToUInt32(packet, 4); }
            set { WriteUInt32(value, 4, packet); }
        }

        public uint TeamID {
            get { return BitConverter.ToUInt32(packet, 8); }
            set { WriteUInt32(value, 8, packet); }
        }

        public string TeamName //BastA`s.team
        {
            get { return PacketHandler.ReadString(packet, 12, 32); }
            set { WriteString(value, 12, packet); }
        }

        public byte[] ToArray() {
            return packet;
        }
    }
}