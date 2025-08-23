using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class GuildCommand : Writer, Interfaces.IPacket
    {
        public const uint
                    JoinRequest = 1,
                    InviteRequest = 2,
                    Quit = 3,
                    Info = 6,
                    Allied = 7,
                    Neutral1 = 8,
                    Enemied = 9,
                    Neutral2 = 10,
                    DonateSilvers = 11,
                    Refresh = 12,
                    Disband = 19,
                    DonateConquerPoints = 20,
                    ChangeGuildRequirements = 24,
                    GuildRequirements = 25,
                    Bulletin = 27,
                    Promote = 28,
                    Discharge = 30,
                    PromoteInfo = 38,
                    RequestPromote = 37;


        private byte[] Buffer;
        public GuildCommand(bool Create)
        {
            if (Create)
            {
                Buffer = new byte[8 + 28];
                WriteUInt16(28, 0, Buffer);
                WriteUInt16(1107, 2, Buffer);
            }
        }
        public GuildCommand(uint leng)
        {
            Buffer = new byte[8 + 28 + leng];
            WriteUInt16((ushort)(Buffer.Length - 8), 0, Buffer);
            WriteUInt16(1107, 2, Buffer);
        }
        private string CreatePromotionString(StringBuilder builder, Game.Enums.GuildMemberRank rank, int occupants,
          int maxOccupants, int extraBattlePower, int conquerPoints)
        {
            builder.Remove(0, builder.Length);
            builder.Append((int)rank);
            builder.Append(" ");
            builder.Append(occupants);
            builder.Append(" ");
            builder.Append(maxOccupants);
            builder.Append(" ");
            builder.Append(extraBattlePower);
            builder.Append(" ");
            builder.Append(conquerPoints);
            builder.Append(" ");
            return builder.ToString();
        }

        public void SendPromote(Client.GameState client, ushort typ)
        {
            if (client == null) return;
            if (client.AsMember == null) return;
            List<string> list = new List<string>();
            StringBuilder builder = new StringBuilder();
            #region Guild Leader
            if (client.AsMember.Rank == Game.Enums.GuildMemberRank.GuildLeader)
            {
                list.Add(CreatePromotionString(builder, Game.Enums.GuildMemberRank.GuildLeader, 1, 1, (int)client.Guild.GetMemberPotency(Game.Enums.GuildMemberRank.GuildLeader), 0));
                //  list.Add(CreatePromotionString(builder, Game.Enums.GuildMemberRank.Aide, (int)client.Guild.RanksCounts[(ushort)Game.Enums.GuildMemberRank.Aide], 6, (int)client.Guild.GetMemberPotency(Game.Enums.GuildMemberRank.Aide), 0));
                list.Add(CreatePromotionString(builder, Game.Enums.GuildMemberRank.DeputyLeader, (int)(int)client.Guild.RanksCounts[(ushort)Game.Enums.GuildMemberRank.DeputyLeader], 7, (int)client.Guild.GetMemberPotency(Game.Enums.GuildMemberRank.DeputyLeader), 0));
                list.Add(CreatePromotionString(builder, Game.Enums.GuildMemberRank.Steward, (int)(int)client.Guild.RanksCounts[(ushort)Game.Enums.GuildMemberRank.Steward], 3, (int)client.Guild.GetMemberPotency(Game.Enums.GuildMemberRank.Steward), 0));
                list.Add(CreatePromotionString(builder, Game.Enums.GuildMemberRank.Follower, (int)(int)client.Guild.RanksCounts[(ushort)Game.Enums.GuildMemberRank.Follower], 10, (int)client.Guild.GetMemberPotency(Game.Enums.GuildMemberRank.Follower), 0));
                list.Add(CreatePromotionString(builder, Game.Enums.GuildMemberRank.Member, (int)client.Guild.RanksCounts[(ushort)Game.Enums.GuildMemberRank.Member], (int)300, (int)client.Guild.GetMemberPotency(Game.Enums.GuildMemberRank.Member), 0));
            }
            #endregion
            #region Leader's Spouse
            if (client.AsMember.Rank == Game.Enums.GuildMemberRank.LeaderSpouse)
            {
                //  list.Add(CreatePromotionString(builder, Game.Enums.GuildMemberRank.DeputyLeader, (int)(int)client.Guild.RanksCounts[(ushort)Game.Enums.GuildMemberRank.DeputyLeader], 4, (int)client.Guild.GetMemberPotency(Game.Enums.GuildMemberRank.DeputyLeader), 0));
                //  list.Add(CreatePromotionString(builder, Game.Enums.GuildMemberRank.Steward, (int)(int)client.Guild.RanksCounts[(ushort)Game.Enums.GuildMemberRank.Steward], 3, (int)client.Guild.GetMemberPotency(Game.Enums.GuildMemberRank.Steward), 0));
                //    list.Add(CreatePromotionString(builder, Game.Enums.GuildMemberRank.Follower, (int)(int)client.Guild.RanksCounts[(ushort)Game.Enums.GuildMemberRank.Follower], 10, (int)client.Guild.GetMemberPotency(Game.Enums.GuildMemberRank.Follower), 0));
                // list.Add(CreatePromotionString(builder, Game.Enums.GuildMemberRank.Member, (int)(int)client.Guild.RanksCounts[(ushort)Game.Enums.GuildMemberRank.Member], (int)300, (int)client.Guild.GetMemberPotency(Game.Enums.GuildMemberRank.Member), 0));
            }
            #endregion
            #region Manager
            if (client.AsMember.Rank == Game.Enums.GuildMemberRank.Manager || client.AsMember.Rank == Game.Enums.GuildMemberRank.HonoraryManager)
            {
                // list.Add(CreatePromotionString(builder, Game.Enums.GuildMemberRank.Aide, (int)client.Guild.RanksCounts[(ushort)Game.Enums.GuildMemberRank.Aide], 6, (int)client.Guild.GetMemberPotency(Game.Enums.GuildMemberRank.Aide), 0));
            }
            #endregion
            if (client.AsMember.Rank == Game.Enums.GuildMemberRank.DeputyLeader)
            {
                // list.Add(CreatePromotionString(builder, Game.Enums.GuildMemberRank.Aide, (int)client.Guild.RanksCounts[(ushort)Game.Enums.GuildMemberRank.Aide], 6, (int)client.Guild.GetMemberPotency(Game.Enums.GuildMemberRank.Aide), 0));
                // list.Add(CreatePromotionString(builder, Game.Enums.GuildMemberRank.Steward, (int)(int)client.Guild.RanksCounts[(ushort)Game.Enums.GuildMemberRank.Steward], 3, (int)client.Guild.GetMemberPotency(Game.Enums.GuildMemberRank.Steward), 0));
                //    list.Add(CreatePromotionString(builder, Game.Enums.GuildMemberRank.Follower, (int)(int)client.Guild.RanksCounts[(ushort)Game.Enums.GuildMemberRank.Follower], 10, (int)client.Guild.GetMemberPotency(Game.Enums.GuildMemberRank.Follower), 0));
            }
            int extraLength = 0;
            foreach (var str in list) extraLength += str.Length + 1;
            byte[] packet = new byte[28 + 8 + extraLength];
            WriteUInt16((ushort)(packet.Length - 8), 0, packet);
            WriteUInt16(1107, 2, packet);
            WriteByte((byte)typ, 4, packet);
            WriteStringList(list, 24, packet);
            client.Send(packet);
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
        /// <summary>
        /// Level offset for GuildRequirements
        /// </summary>
        public uint dwParam2
        {
            get { return BitConverter.ToUInt32(Buffer, 12); }
            set { WriteUInt32(value, 12, Buffer); }
        }

        /// <summary>
        /// Reborn offset for GuildRequirements
        /// </summary>
        public uint dwParam3
        {
            get { return BitConverter.ToUInt32(Buffer, 16); }
            set { WriteUInt32(value, 16, Buffer); }
        }

        /// <summary>
        /// Class offset for GuildRequirements
        /// </summary>
        public uint dwParam4
        {
            get { return BitConverter.ToUInt32(Buffer, 20); }
            set { WriteUInt32(value, 20, Buffer); }
        }
        /// <summary>
        /// offset buletin
        /// </summary>
        public string Str_
        {
            set
            {
                WriteByte(1, 24, Buffer);
                WriteByte((byte)(value.Length), 25, Buffer);
                WriteString(value, 26, Buffer);
            }
        }
        public void Deserialize(byte[] Data)
        {
            Buffer = Data;
        }

        public byte[] ToArray()
        {
            return Buffer;
        }
        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }
    }
}

