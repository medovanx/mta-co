using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Client;
using MTA.Game;
using MTA.Game.Features.Tournaments;

namespace MTA.Network.GamePackets
{
    public class TeamElitePKMatchStats : Writer, Interfaces.IPacket
    {
        byte[] Buffer;

        public TeamElitePKMatchStats(TeamElitePk.GamePackets ID)
        {
            Buffer = new byte[128 + 8];
            WriteUInt16(128, 0, Buffer);
            if (ID == TeamElitePk.GamePackets.SkillElitePkBrackets)
                WriteUshort((ushort)TeamElitePk.GamePackets.SkillElitePKMatchStats, 2, Buffer);
            else
                WriteUshort((ushort)TeamElitePk.GamePackets.TeamElitePKMatchStats, 2, Buffer);
        }

        public void Append(TeamElitePk.Match match)
        {
            ushort offset = 4;
            var array = match.MatchStats;
            AppendPlayer(array[0], offset); offset += 60;//60
            AppendPlayer(array[1], offset);
        }

        public uint val1 = 0;
        public uint val2 = 0;
        public uint val3 = 0;
        public uint val4 = 0;

        private void AppendPlayer(TeamElitePk.FighterStats team, ushort offset)
        {
            WriteUint(team.LeaderUID, offset, Buffer); offset += 4;
            WriteUint(team.Team.UID, offset, Buffer); offset += 4;
            WriteString(team.Name, offset, Buffer); offset += 32;//???
            WriteUint(val1, offset, Buffer);
            offset += 4;
            WriteUint(val2, offset, Buffer);
            offset += 4;
            WriteUint(val3, offset, Buffer);
            offset += 4;
            WriteUint(val4, offset, Buffer);
            offset += 4;
            WriteUint(team.Points, offset, Buffer);
        }

        public void Send(GameState client)
        {
            client.Send(Buffer);
        }

        public byte[] ToArray()
        {
            return Buffer;
        }

        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }
    }
}
