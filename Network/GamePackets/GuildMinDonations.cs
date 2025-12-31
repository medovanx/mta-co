using MTA.Game;
using MTA.Game.Features.Guilds.Models;

namespace MTA.Network.GamePackets {
    public class GuildMinDonations : Writer {
        byte[] packet;
        ushort Position = 8;

        public GuildMinDonations(ushort counts = 0) //31
        {
            packet = new byte[(ushort)(16 + counts * 8)];
            WriteUInt16((ushort)(packet.Length - 8), 0, packet);
            WriteUInt16(1061, 2, packet);
            WriteUInt16(counts, 6, packet);
        }

        public byte[] ToArray() {
            return packet;
        }

        public void Aprend(Enums.GuildMemberRank Rank, uint amount) {
            WriteUInt32((ushort)Rank, Position, packet);
            Position += 4;
            //WriteUint(uint.MaxValue, Position);//for not apprend
            WriteUInt32(amount, Position, packet);
            Position += 4;
        }

        public void AprendGuild(Guild guild) {
            if (guild.RankArsenalDonations.Length >= 5) {
                var obj = guild.RankArsenalDonations[4];
                Aprend(Enums.GuildMemberRank.Manager, obj.ArsenalDonation);
            }
            else
                Aprend(Enums.GuildMemberRank.Manager, 0);

            if (guild.RankArsenalDonations.Length >= 7) {
                var obj = guild.RankArsenalDonations[6];
                Aprend(Enums.GuildMemberRank.HonoraryManager, obj.ArsenalDonation);
            }
            else
                Aprend(Enums.GuildMemberRank.HonoraryManager, 0);


            if (guild.RankArsenalDonations.Length >= 8) {
                var obj = guild.RankArsenalDonations[7];
                Aprend(Enums.GuildMemberRank.Supervisor, obj.ArsenalDonation);
            }
            else
                Aprend(Enums.GuildMemberRank.Supervisor, 0);

            if (guild.RankArsenalDonations.Length >= 13) {
                var obj = guild.RankArsenalDonations[12];
                Aprend(Enums.GuildMemberRank.Steward, obj.ArsenalDonation);
            }
            else
                Aprend(Enums.GuildMemberRank.Steward, 0);

            if (guild.RankArsenalDonations.Length >= 15) {
                var obj = guild.RankArsenalDonations[14];
                Aprend(Enums.GuildMemberRank.ArsFollower, obj.ArsenalDonation);
            }
            else Aprend(Enums.GuildMemberRank.ArsFollower, 0);


            if (guild.RankCpDonations.Length >= 3) {
                var obj = guild.RankCpDonations[2];
                Aprend(Enums.GuildMemberRank.CPSupervisor, (uint)obj.ConquerPointDonation);
            }
            else
                Aprend(Enums.GuildMemberRank.CPSupervisor, 0);

            if (guild.RankCpDonations.Length >= 5) {
                var obj = guild.RankCpDonations[4];
                Aprend(Enums.GuildMemberRank.CPAgent, (uint)obj.ConquerPointDonation);
            }
            else
                Aprend(Enums.GuildMemberRank.CPAgent, 0);

            if (guild.RankCpDonations.Length >= 7) {
                var obj = guild.RankCpDonations[6];
                Aprend(Enums.GuildMemberRank.CPFollower, (uint)obj.ConquerPointDonation);
            }
            else
                Aprend(Enums.GuildMemberRank.CPFollower, 0);


            if (guild.RankPkDonations.Length >= 3) {
                var obj = guild.RankPkDonations[2];
                Aprend(Enums.GuildMemberRank.PKSupervisor, obj.PkDonation);
            }
            else
                Aprend(Enums.GuildMemberRank.PKSupervisor, 0);

            if (guild.RankPkDonations.Length >= 5) {
                var obj = guild.RankPkDonations[4];
                Aprend(Enums.GuildMemberRank.PKAgent, obj.PkDonation);
            }
            else Aprend(Enums.GuildMemberRank.PKAgent, 0);

            if (guild.RankPkDonations.Length >= 7) {
                var obj = guild.RankPkDonations[6];
                Aprend(Enums.GuildMemberRank.PKFollower, obj.PkDonation);
            }
            else Aprend(Enums.GuildMemberRank.PKFollower, 0);


            if (guild.RankRoseDonations.Length >= 3) {
                var obj = guild.RankRoseDonations[2];
                Aprend(Enums.GuildMemberRank.RoseSupervisor, obj.Roses);
            }
            else
                Aprend(Enums.GuildMemberRank.RoseSupervisor, 0);

            if (guild.RankRoseDonations.Length >= 5) {
                var obj = guild.RankRoseDonations[4];
                Aprend(Enums.GuildMemberRank.RoseAgent, obj.Roses);
            }
            else
                Aprend(Enums.GuildMemberRank.RoseAgent, 0);

            if (guild.RankRoseDonations.Length >= 7) {
                var obj = guild.RankRoseDonations[6];
                Aprend(Enums.GuildMemberRank.RoseFollower, obj.Roses);
            }
            else
                Aprend(Enums.GuildMemberRank.RoseFollower, 0);


            if (guild.RankLiliesDonations.Length >= 3) {
                var obj = guild.RankLiliesDonations[2];
                Aprend(Enums.GuildMemberRank.LilySupervisor, obj.Lilies);
            }
            else
                Aprend(Enums.GuildMemberRank.LilySupervisor, 0);

            if (guild.RankLiliesDonations.Length >= 5) {
                var obj = guild.RankLiliesDonations[4];
                Aprend(Enums.GuildMemberRank.LilyAgent, obj.Lilies);
            }
            else
                Aprend(Enums.GuildMemberRank.LilyAgent, 0);

            if (guild.RankLiliesDonations.Length >= 7) {
                var obj = guild.RankLiliesDonations[6];
                Aprend(Enums.GuildMemberRank.LilyFollower, obj.Lilies);
            }
            else
                Aprend(Enums.GuildMemberRank.LilyFollower, 0);


            if (guild.RankTulipsDonations.Length >= 3) {
                var obj = guild.RankTulipsDonations[2];
                Aprend(Enums.GuildMemberRank.TSupervisor, obj.Tulips);
            }
            else
                Aprend(Enums.GuildMemberRank.TSupervisor, 0);

            if (guild.RankTulipsDonations.Length >= 5) {
                var obj = guild.RankTulipsDonations[4];
                Aprend(Enums.GuildMemberRank.TulipAgent, obj.Tulips);
            }
            else
                Aprend(Enums.GuildMemberRank.TulipAgent, 0);

            if (guild.RankTulipsDonations.Length >= 7) {
                var obj = guild.RankTulipsDonations[6];
                Aprend(Enums.GuildMemberRank.TulipFollower, obj.Tulips);
            }
            else
                Aprend(Enums.GuildMemberRank.TulipFollower, 0);


            if (guild.RankOrchidsDonations.Length >= 3) {
                var obj = guild.RankOrchidsDonations[2];
                Aprend(Enums.GuildMemberRank.OSupervisor, obj.Orchids);
            }
            else
                Aprend(Enums.GuildMemberRank.OSupervisor, 0);

            if (guild.RankOrchidsDonations.Length >= 5) {
                var obj = guild.RankOrchidsDonations[4];
                Aprend(Enums.GuildMemberRank.OrchidAgent, obj.Orchids);
            }
            else
                Aprend(Enums.GuildMemberRank.OrchidAgent, 0);

            if (guild.RankOrchidsDonations.Length >= 7) {
                var obj = guild.RankOrchidsDonations[6];
                Aprend(Enums.GuildMemberRank.OrchidFollower, obj.Orchids);
            }
            else
                Aprend(Enums.GuildMemberRank.OrchidFollower, 0);


            if (guild.RankTotalDonations.Length >= 2) {
                var obj = guild.RankTotalDonations[1];
                Aprend(Enums.GuildMemberRank.HDeputyLeader, obj.TotalDonation);
            }
            else
                Aprend(Enums.GuildMemberRank.HDeputyLeader, 0);

            if (guild.RankTotalDonations.Length >= 4) {
                var obj = guild.RankTotalDonations[3];
                Aprend(Enums.GuildMemberRank.HonorarySteward, obj.TotalDonation);
            }
            else
                Aprend(Enums.GuildMemberRank.HonorarySteward, 0);


            if (guild.RankSilversDonations.Length >= 4) {
                var obj = guild.RankSilversDonations[3];
                Aprend(Enums.GuildMemberRank.SSupervisor, (uint)obj.SilverDonation);
            }
            else
                Aprend(Enums.GuildMemberRank.SSupervisor, 0);

            if (guild.RankSilversDonations.Length >= 6) {
                var obj = guild.RankSilversDonations[5];
                Aprend(Enums.GuildMemberRank.SilverAgent, (uint)obj.SilverDonation);
            }
            else
                Aprend(Enums.GuildMemberRank.SilverAgent, 0);

            if (guild.RankSilversDonations.Length >= 8) {
                var obj = guild.RankSilversDonations[7];
                Aprend(Enums.GuildMemberRank.SilverFollower, (uint)obj.SilverDonation);
            }
            else
                Aprend(Enums.GuildMemberRank.SilverFollower, 0);


            if (guild.RankGuideDonations.Length >= 3) {
                var obj = guild.RankGuideDonations[2];
                Aprend(Enums.GuildMemberRank.GSupervisor, obj.VirtuePoints);
            }
            else
                Aprend(Enums.GuildMemberRank.GSupervisor, 0);

            if (guild.RankGuideDonations.Length >= 5) {
                var obj = guild.RankGuideDonations[4];
                Aprend(Enums.GuildMemberRank.GuideAgent, obj.VirtuePoints);
            }
            else
                Aprend(Enums.GuildMemberRank.GuideAgent, 0);

            if (guild.RankGuideDonations.Length >= 7) {
                var obj = guild.RankGuideDonations[6];
                Aprend(Enums.GuildMemberRank.GuideFollower, obj.VirtuePoints);
            }
            else
                Aprend(Enums.GuildMemberRank.GuideFollower, 0);
        }
    }
}