using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class GuildMinDonations : Writer
    {
        byte[] packet;
        public byte[] ToArray() { return packet; }
        public GuildMinDonations(ushort counts = 0)//31
        {
            packet = new byte[(ushort)(16 + counts * 8)];
            WriteUInt16((ushort)(packet.Length - 8), 0, packet);
            WriteUInt16(1061, 2, packet);
            WriteUInt16(counts, 6, packet);

        }
        ushort Position = 8;
        public void Aprend(Game.Enums.GuildMemberRank Rank, uint amount)
        {

            WriteUInt32((ushort)Rank, Position, packet); Position += 4;
            //WriteUint(uint.MaxValue, Position);//for not apprend
            WriteUInt32(amount, Position, packet);
            Position += 4;
        }

        public void AprendGuild(Game.ConquerStructures.Society.Guild guild)
        {
            if (guild.RankArsenalDonations.Length >= 5)
            {
                var obj = guild.RankArsenalDonations[4];
                Aprend(Game.Enums.GuildMemberRank.Manager, obj.ArsenalDonation);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.Manager, 0);

            if (guild.RankArsenalDonations.Length >= 7)
            {
                var obj = guild.RankArsenalDonations[6];
                Aprend(Game.Enums.GuildMemberRank.HonoraryManager, obj.ArsenalDonation);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.HonoraryManager, 0);


            if (guild.RankArsenalDonations.Length >= 8)
            {
                var obj = guild.RankArsenalDonations[7];
                Aprend(Game.Enums.GuildMemberRank.Supervisor, obj.ArsenalDonation);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.Supervisor, 0);

            if (guild.RankArsenalDonations.Length >= 13)
            {
                var obj = guild.RankArsenalDonations[12];
                Aprend(Game.Enums.GuildMemberRank.Steward, obj.ArsenalDonation);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.Steward, 0);
            if (guild.RankArsenalDonations.Length >= 15)
            {
                var obj = guild.RankArsenalDonations[14];
                Aprend(Game.Enums.GuildMemberRank.ArsFollower, obj.ArsenalDonation);
            }
            else Aprend(Game.Enums.GuildMemberRank.ArsFollower, 0);



            if (guild.RankCPDonations.Length >= 3)
            {
                var obj = guild.RankCPDonations[2];
                Aprend(Game.Enums.GuildMemberRank.CPSupervisor, (uint)obj.ConquerPointDonation);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.CPSupervisor, 0);
            if (guild.RankCPDonations.Length >= 5)
            {
                var obj = guild.RankCPDonations[4];
                Aprend(Game.Enums.GuildMemberRank.CPAgent, (uint)obj.ConquerPointDonation);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.CPAgent, 0);

            if (guild.RankCPDonations.Length >= 7)
            {
                var obj = guild.RankCPDonations[6];
                Aprend(Game.Enums.GuildMemberRank.CPFollower, (uint)obj.ConquerPointDonation);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.CPFollower, 0);


            if (guild.RankPkDonations.Length >= 3)
            {
                var obj = guild.RankPkDonations[2];
                Aprend(Game.Enums.GuildMemberRank.PKSupervisor, obj.PkDonation);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.PKSupervisor, 0);

            if (guild.RankPkDonations.Length >= 5)
            {
                var obj = guild.RankPkDonations[4];
                Aprend(Game.Enums.GuildMemberRank.PKAgent, obj.PkDonation);
            }
            else Aprend(Game.Enums.GuildMemberRank.PKAgent, 0);

            if (guild.RankPkDonations.Length >= 7)
            {
                var obj = guild.RankPkDonations[6];
                Aprend(Game.Enums.GuildMemberRank.PKFollower, obj.PkDonation);
            }
            else Aprend(Game.Enums.GuildMemberRank.PKFollower, 0);




            if (guild.RankRosseDonations.Length >= 3)
            {
                var obj = guild.RankRosseDonations[2];
                Aprend(Game.Enums.GuildMemberRank.RoseSupervisor, obj.Rouses);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.RoseSupervisor, 0);

            if (guild.RankRosseDonations.Length >= 5)
            {
                var obj = guild.RankRosseDonations[4];
                Aprend(Game.Enums.GuildMemberRank.RoseAgent, obj.Rouses);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.RoseAgent, 0);

            if (guild.RankRosseDonations.Length >= 7)
            {
                var obj = guild.RankRosseDonations[6];
                Aprend(Game.Enums.GuildMemberRank.RoseFollower, obj.Rouses);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.RoseFollower, 0);





            if (guild.RankLiliesDonations.Length >= 3)
            {
                var obj = guild.RankLiliesDonations[2];
                Aprend(Game.Enums.GuildMemberRank.LilySupervisor, obj.Lilies);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.LilySupervisor, 0);
            if (guild.RankLiliesDonations.Length >= 5)
            {
                var obj = guild.RankLiliesDonations[4];
                Aprend(Game.Enums.GuildMemberRank.LilyAgent, obj.Lilies);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.LilyAgent, 0);
            if (guild.RankLiliesDonations.Length >= 7)
            {
                var obj = guild.RankLiliesDonations[6];
                Aprend(Game.Enums.GuildMemberRank.LilyFollower, obj.Lilies);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.LilyFollower, 0);





            if (guild.RankTulipsDonations.Length >= 3)
            {
                var obj = guild.RankTulipsDonations[2];
                Aprend(Game.Enums.GuildMemberRank.TSupervisor, obj.Tulips);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.TSupervisor, 0);

            if (guild.RankTulipsDonations.Length >= 5)
            {
                var obj = guild.RankTulipsDonations[4];
                Aprend(Game.Enums.GuildMemberRank.TulipAgent, obj.Tulips);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.TulipAgent, 0);
            if (guild.RankTulipsDonations.Length >= 7)
            {
                var obj = guild.RankTulipsDonations[6];
                Aprend(Game.Enums.GuildMemberRank.TulipFollower, obj.Tulips);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.TulipFollower, 0);



            if (guild.RankOrchidsDonations.Length >= 3)
            {
                var obj = guild.RankOrchidsDonations[2];
                Aprend(Game.Enums.GuildMemberRank.OSupervisor, obj.Orchids);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.OSupervisor, 0);

            if (guild.RankOrchidsDonations.Length >= 5)
            {
                var obj = guild.RankOrchidsDonations[4];
                Aprend(Game.Enums.GuildMemberRank.OrchidAgent, obj.Orchids);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.OrchidAgent, 0);
            if (guild.RankOrchidsDonations.Length >= 7)
            {
                var obj = guild.RankOrchidsDonations[6];
                Aprend(Game.Enums.GuildMemberRank.OrchidFollower, obj.Orchids);

            }
            else
                Aprend(Game.Enums.GuildMemberRank.OrchidFollower, 0);



            if (guild.RankTotalDonations.Length >= 2)
            {
                var obj = guild.RankTotalDonations[1];
                Aprend(Game.Enums.GuildMemberRank.HDeputyLeader, obj.TotalDonation);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.HDeputyLeader, 0);
            if (guild.RankTotalDonations.Length >= 4)
            {
                var obj = guild.RankTotalDonations[3];
                Aprend(Game.Enums.GuildMemberRank.HonorarySteward, obj.TotalDonation);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.HonorarySteward, 0);





            if (guild.RankSilversDonations.Length >= 4)
            {
                var obj = guild.RankSilversDonations[3];
                Aprend(Game.Enums.GuildMemberRank.SSupervisor, (uint)obj.SilverDonation);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.SSupervisor, 0);
            if (guild.RankSilversDonations.Length >= 6)
            {
                var obj = guild.RankSilversDonations[5];
                Aprend(Game.Enums.GuildMemberRank.SilverAgent, (uint)obj.SilverDonation);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.SilverAgent, 0);

            if (guild.RankSilversDonations.Length >= 8)
            {
                var obj = guild.RankSilversDonations[7];
                Aprend(Game.Enums.GuildMemberRank.SilverFollower, (uint)obj.SilverDonation);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.SilverFollower, 0);


            if (guild.RankGuideDonations.Length >= 3)
            {
                var obj = guild.RankGuideDonations[2];
                Aprend(Game.Enums.GuildMemberRank.GSupervisor, obj.VirtutePointes);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.GSupervisor, 0);

            if (guild.RankGuideDonations.Length >= 5)
            {
                var obj = guild.RankGuideDonations[4];
                Aprend(Game.Enums.GuildMemberRank.GuideAgent, obj.VirtutePointes);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.GuideAgent, 0);

            if (guild.RankGuideDonations.Length >= 7)
            {
                var obj = guild.RankGuideDonations[6];
                Aprend(Game.Enums.GuildMemberRank.GuideFollower, obj.VirtutePointes);
            }
            else
                Aprend(Game.Enums.GuildMemberRank.GuideFollower, 0);
        }
    }
}
