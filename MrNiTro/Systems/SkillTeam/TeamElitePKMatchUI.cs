using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Client;
using MTA.Game.Features.Tournaments;

namespace MTA.Network.GamePackets
{
    public class TeamElitePKMatchUI : Writer, Interfaces.IPacket
    {
        public const byte
             BeginMatch = 2,
             Effect = 3,
             EndMatch = 4,
             Information = 7,
             Reward = 8;

        public const uint
            Effect_Win = 1,
            Effect_Lose = 0;

        byte[] Buffer;

        public TeamElitePKMatchUI(TeamElitePk.GamePackets ID)
        {
            {
                Buffer = new byte[52 + 8];
                WriteUInt16(52, 0, Buffer);
                if (ID == TeamElitePk.GamePackets.SkillElitePkBrackets)
                    WriteUshort((ushort)TeamElitePk.GamePackets.SkillElitePKMatchUI, 2, Buffer);
                else
                    WriteUshort((ushort)TeamElitePk.GamePackets.TeamElitePKMatchUI, 2, Buffer);
            }
        }

        public uint Type
        {
            get { return BitConverter.ToUInt32(Buffer, 4); }
            set { WriteUInt32(value, 4, Buffer); }
        }

        public uint dwParam
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32(value, 8, Buffer); }
        }

        public uint OpponentUID
        {
            get { return BitConverter.ToUInt32(Buffer, 12); }
            set { WriteUInt32(value, 12, Buffer); }
        }

        public string OpponentName
        {
            get { return Program.Encoding.GetString(Buffer, 16, 16).Trim(); }
            set { WriteString(value, 16, Buffer); }
        }

        public uint TimeLeft
        {
            get { return BitConverter.ToUInt32(Buffer, 44); }
            set { WriteUInt32(value, 44, Buffer); }
        }

        public void Append(Game.ConquerStructures.Team opponent)
        {
            OpponentUID = opponent.Lider.Entity.UID;
            if (opponent.EliteFighterStats != null)
                OpponentName = opponent.EliteFighterStats.Name;
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
