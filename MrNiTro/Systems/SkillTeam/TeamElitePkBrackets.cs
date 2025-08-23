using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Game;
using MTA.Client;
using MTA.Game.Features.Tournaments;

namespace MTA.Network.GamePackets
{
    public class TeamElitePkBrackets : Writer, Interfaces.IPacket
    {
        public const byte
            InitialList = 0,
            StaticUpdate = 1,
            GUIEdit = 2,
            UpdateList = 3,
            RequestInformation = 4,
            StopWagers = 5,
            EPK_State = 6;

        byte[] Buffer;

        public TeamElitePkBrackets(TeamElitePk.GamePackets id, int matches = 0)
        {
            {
                if (matches > 5)
                    return;
                Buffer = new byte[190 + 154 * matches];
                WriteUInt16((ushort)(Buffer.Length - 8), 0, Buffer);
                WriteUInt16((ushort)id, 2, Buffer);
            }
        }

        public byte Type
        {
            get { return Buffer[4]; }
            set { Buffer[4] = value; }
        }

        public byte Page
        {
            get { return Buffer[6]; }
            set { Buffer[6] = value; }
        }

        public byte ListCount
        {
            get { return Buffer[8]; }
            set { Buffer[8] = value; }
        }

        public uint MatchCount
        {
            get { return BitConverter.ToUInt32(Buffer, 10); }
            set { WriteUInt32(value, 10, Buffer); }
        }

        public ushort Group
        {
            get { return BitConverter.ToUInt16(Buffer, 14); }
            set { WriteUInt16(value, 14, Buffer); }
        }

        public ushort GUIType
        {
            get { return BitConverter.ToUInt16(Buffer, 16); }
            set { WriteUInt16(value, 16, Buffer); }
        }

        public ushort TimeLeft
        {
            get { return BitConverter.ToUInt16(Buffer, 18); }
            set { WriteUInt16(value, 18, Buffer); }
        }

        public uint TotalMatchesOnRoom
        {
            get { return BitConverter.ToUInt16(Buffer, 20); }
            set { WriteUInt32(value, 20, Buffer); }
        }

        public bool OnGoing
        {
            get { return Buffer[20] == 1; }
            set { WriteBoolean(value, 20, Buffer); }
        }

        private int offset = 24;

        public void Append(TeamElitePk.Match match)
        {
            try
            {
                if (Type != GUIEdit)
                    WriteUint(match.ID, offset, Buffer);

                offset += 4;
                WriteUshort((ushort)match.MatchStats.Length, offset, Buffer); offset += 2;
                WriteUshort((ushort)match.Index, offset, Buffer); offset += 2;
                if (match.MatchStats.Length == 1)
                    WriteUshort((ushort)TeamElitePk.Match.StatusFlag.OK, offset, Buffer);
                else
                    WriteUshort((ushort)match.Flag, offset, Buffer);

                offset += 2;

                for (int i = 0; i < match.MatchStats.Length; i++)
                {
                    AppendPlayer(match.MatchStats[i], offset);
                    offset += 48;
                }
                if (match.MatchStats.Length == 2) offset += 48;
                if (match.MatchStats.Length == 1) offset += 96;
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        private void AppendPlayer(TeamElitePk.FighterStats stats, int _offset)
        {
            WriteUint(stats.Team.UID, _offset, Buffer); _offset += 4;
            WriteUint(stats.LeaderUID, _offset, Buffer); _offset += 4;
            WriteUint(stats.LeaderMesh, _offset, Buffer); _offset += 4;
            WriteString(stats.Name, _offset, Buffer); _offset += 32;
            WriteUint((uint)stats.Flag, _offset, Buffer); _offset += 4;

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
