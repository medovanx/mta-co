using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Game.Features.Guilds.Constants;
using MTA.Network;

namespace MTA.Game.Features.Guilds.Packets {
    public class GuildCommand : Writer, Interfaces.IPacket {
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
            RequestPromote = 37,
            AddToBlacklist = 48,
            RemoveFromBlacklist = 50;


        private byte[] _buffer;

        public GuildCommand(bool create) {
            _buffer = new byte[8 + 28];
            if (!create) return;
            WriteUInt16(28, 0, _buffer);
            WriteUInt16(1107, 2, _buffer);
        }

        public GuildCommand(uint leng) {
            _buffer = new byte[8 + 28 + leng];
            WriteUInt16((ushort)(_buffer.Length - 8), 0, _buffer);
            WriteUInt16(1107, 2, _buffer);
        }

        private string CreatePromotionString(StringBuilder builder, MemberRank rank, int occupants,
            int maxOccupants, int extraBattlePower, int conquerPoints) {
            builder.Remove(0, builder.Length);
            builder.Append((int)rank);
            builder.Append(' ');
            builder.Append(occupants);
            builder.Append(' ');
            builder.Append(maxOccupants);
            builder.Append(' ');
            builder.Append(extraBattlePower);
            builder.Append(' ');
            builder.Append(conquerPoints);
            builder.Append(' ');
            return builder.ToString();
        }

        public void SendPromote(Client.GameState client, ushort typ) {
            if (client.AsMember == null) return;
            List<string> list = [];
            var builder = new StringBuilder();

            #region Guild Leader

            if (client.AsMember.Rank == MemberRank.GuildLeader) {
                list.Add(CreatePromotionString(builder, MemberRank.GuildLeader, 1, 1,
                    (int)client.Guild!.GetMemberPotency(MemberRank.GuildLeader), 0));
                //  list.Add(CreatePromotionString(builder, Game.GuildMemberRank.Aide, (int)client.Guild.RanksCounts[(ushort)Game.GuildMemberRank.Aide], 6, (int)client.Guild.GetMemberPotency(Game.GuildMemberRank.Aide), 0));
                list.Add(CreatePromotionString(builder, MemberRank.DeputyLeader,
                    client.Guild.RanksCounts[(ushort)MemberRank.DeputyLeader],
                    client.Guild.GetMaxDeputyLeaders(),
                    (int)client.Guild.GetMemberPotency(MemberRank.DeputyLeader), 0));
                list.Add(CreatePromotionString(builder, MemberRank.Steward,
                    client.Guild.RanksCounts[(ushort)MemberRank.Steward], 3,
                    (int)client.Guild.GetMemberPotency(MemberRank.Steward), 0));
                list.Add(CreatePromotionString(builder, MemberRank.Follower,
                    client.Guild.RanksCounts[(ushort)MemberRank.Follower], 10,
                    (int)client.Guild.GetMemberPotency(MemberRank.Follower), 0));
                list.Add(CreatePromotionString(builder, MemberRank.Member,
                    client.Guild.RanksCounts[(ushort)MemberRank.Member], 300,
                    (int)client.Guild.GetMemberPotency(MemberRank.Member), 0));
            }

            #endregion

            #region Leader's Spouse

            switch (client.AsMember.Rank) {
                case MemberRank.LeaderSpouse:
                    //  list.Add(CreatePromotionString(builder, Game.GuildMemberRank.DeputyLeader, (int)(int)client.Guild.RanksCounts[(ushort)Game.GuildMemberRank.DeputyLeader], 4, (int)client.Guild.GetMemberPotency(Game.GuildMemberRank.DeputyLeader), 0));
                    //  list.Add(CreatePromotionString(builder, Game.GuildMemberRank.Steward, (int)(int)client.Guild.RanksCounts[(ushort)Game.GuildMemberRank.Steward], 3, (int)client.Guild.GetMemberPotency(Game.GuildMemberRank.Steward), 0));
                    //    list.Add(CreatePromotionString(builder, Game.GuildMemberRank.Follower, (int)(int)client.Guild.RanksCounts[(ushort)Game.GuildMemberRank.Follower], 10, (int)client.Guild.GetMemberPotency(Game.GuildMemberRank.Follower), 0));
                    // list.Add(CreatePromotionString(builder, Game.GuildMemberRank.Member, (int)(int)client.Guild.RanksCounts[(ushort)Game.GuildMemberRank.Member], (int)300, (int)client.Guild.GetMemberPotency(Game.GuildMemberRank.Member), 0));
                    break;
                case MemberRank.Manager:
                case MemberRank.HonoraryManager:
                    // list.Add(CreatePromotionString(builder, Game.GuildMemberRank.Aide, (int)client.Guild.RanksCounts[(ushort)Game.GuildMemberRank.Aide], 6, (int)client.Guild.GetMemberPotency(Game.GuildMemberRank.Aide), 0));
                    break;
                case MemberRank.DeputyLeader:
                    // list.Add(CreatePromotionString(builder, Game.GuildMemberRank.Aide, (int)client.Guild.RanksCounts[(ushort)Game.GuildMemberRank.Aide], 6, (int)client.Guild.GetMemberPotency(Game.GuildMemberRank.Aide), 0));
                    // list.Add(CreatePromotionString(builder, Game.GuildMemberRank.Steward, (int)(int)client.Guild.RanksCounts[(ushort)Game.GuildMemberRank.Steward], 3, (int)client.Guild.GetMemberPotency(Game.GuildMemberRank.Steward), 0));
                    //    list.Add(CreatePromotionString(builder, Game.GuildMemberRank.Follower, (int)(int)client.Guild.RanksCounts[(ushort)Game.GuildMemberRank.Follower], 10, (int)client.Guild.GetMemberPotency(Game.GuildMemberRank.Follower), 0));
                    break;
            }

            #endregion

            var extraLength = list.Sum(str => str.Length + 1);
            var packet = new byte[28 + 8 + extraLength];
            WriteUInt16((ushort)(packet.Length - 8), 0, packet);
            WriteUInt16(1107, 2, packet);
            WriteByte((byte)typ, 4, packet);
            WriteStringList(list, 24, packet);
            client.Send(packet);
        }

        public uint Type {
            get => BitConverter.ToUInt32(_buffer, 4);
            set => WriteUInt32(value, 4, _buffer);
        }

        public uint DwParam {
            get => BitConverter.ToUInt32(_buffer, 8);
            set => WriteUInt32(value, 8, _buffer);
        }

        /// <summary>
        /// Level offset for GuildRequirements
        /// </summary>
        public uint DwParam2 {
            get => BitConverter.ToUInt32(_buffer, 12);
            init => WriteUInt32(value, 12, _buffer);
        }

        /// <summary>
        /// Reborn offset for GuildRequirements
        /// </summary>
        public uint DwParam3 {
            get => BitConverter.ToUInt32(_buffer, 16);
            init => WriteUInt32(value, 16, _buffer);
        }

        /// <summary>
        /// Class offset for GuildRequirements
        /// </summary>
        public uint DwParam4 {
            get => BitConverter.ToUInt32(_buffer, 20);
            init => WriteUInt32(value, 20, _buffer);
        }

        /// <summary>
        /// offset buletin
        /// </summary>
        public string Str {
            set {
                WriteByte(1, 24, _buffer);
                WriteByte((byte)(value.Length), 25, _buffer);
                WriteString(value, 26, _buffer);
            }
        }

        public void Deserialize(byte[] data) {
            _buffer = data;
        }

        public byte[] ToArray() {
            return _buffer;
        }

        public void Send(Client.GameState client) {
            client.Send(_buffer);
        }
    }
}