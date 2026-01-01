using MTA.Game.Features.Guilds.Constants;
using MTA.Network;

namespace MTA.Game.Features.Guilds.Packets;

public class GuildMinDonations : Writer {
    private readonly byte[] _packet;
    private ushort _position = 8;

    public GuildMinDonations(ushort counts = 0) //31
    {
        _packet = new byte[(ushort)(16 + counts * 8)];
        WriteUInt16((ushort)(_packet.Length - 8), 0, _packet);
        WriteUInt16(1061, 2, _packet);
        WriteUInt16(counts, 6, _packet);
    }

    public byte[] ToArray() {
        return _packet;
    }

    private void Aprend(MemberRank rank, uint amount) {
        WriteUInt32((ushort)rank, _position, _packet);
        _position += 4;
        //WriteUint(uint.MaxValue, Position);//for not apprend
        WriteUInt32(amount, _position, _packet);
        _position += 4;
    }

    public void AprendGuild(Guild guild) {
        if (guild.RankArsenalDonations.Length >= 5) {
            var obj = guild.RankArsenalDonations[4];
            Aprend(MemberRank.Manager, obj.ArsenalDonation);
        }
        else {
            Aprend(MemberRank.Manager, 0);
        }

        if (guild.RankArsenalDonations.Length >= 7) {
            var obj = guild.RankArsenalDonations[6];
            Aprend(MemberRank.HonoraryManager, obj.ArsenalDonation);
        }
        else {
            Aprend(MemberRank.HonoraryManager, 0);
        }


        if (guild.RankArsenalDonations.Length >= 8) {
            var obj = guild.RankArsenalDonations[7];
            Aprend(MemberRank.Supervisor, obj.ArsenalDonation);
        }
        else {
            Aprend(MemberRank.Supervisor, 0);
        }

        if (guild.RankArsenalDonations.Length >= 13) {
            var obj = guild.RankArsenalDonations[12];
            Aprend(MemberRank.Steward, obj.ArsenalDonation);
        }
        else {
            Aprend(MemberRank.Steward, 0);
        }

        if (guild.RankArsenalDonations.Length >= 15) {
            var obj = guild.RankArsenalDonations[14];
            Aprend(MemberRank.ArsFollower, obj.ArsenalDonation);
        }
        else {
            Aprend(MemberRank.ArsFollower, 0);
        }


        if (guild.RankCpDonations.Length >= 3) {
            var obj = guild.RankCpDonations[2];
            Aprend(MemberRank.CPSupervisor, (uint)obj.ConquerPointDonation);
        }
        else {
            Aprend(MemberRank.CPSupervisor, 0);
        }

        if (guild.RankCpDonations.Length >= 5) {
            var obj = guild.RankCpDonations[4];
            Aprend(MemberRank.CPAgent, (uint)obj.ConquerPointDonation);
        }
        else {
            Aprend(MemberRank.CPAgent, 0);
        }

        if (guild.RankCpDonations.Length >= 7) {
            var obj = guild.RankCpDonations[6];
            Aprend(MemberRank.CPFollower, (uint)obj.ConquerPointDonation);
        }
        else {
            Aprend(MemberRank.CPFollower, 0);
        }


        if (guild.RankPkDonations.Length >= 3) {
            var obj = guild.RankPkDonations[2];
            Aprend(MemberRank.PKSupervisor, obj.PkDonation);
        }
        else {
            Aprend(MemberRank.PKSupervisor, 0);
        }

        if (guild.RankPkDonations.Length >= 5) {
            var obj = guild.RankPkDonations[4];
            Aprend(MemberRank.PKAgent, obj.PkDonation);
        }
        else {
            Aprend(MemberRank.PKAgent, 0);
        }

        if (guild.RankPkDonations.Length >= 7) {
            var obj = guild.RankPkDonations[6];
            Aprend(MemberRank.PKFollower, obj.PkDonation);
        }
        else {
            Aprend(MemberRank.PKFollower, 0);
        }


        if (guild.RankRoseDonations.Length >= 3) {
            var obj = guild.RankRoseDonations[2];
            Aprend(MemberRank.RoseSupervisor, obj.Roses);
        }
        else {
            Aprend(MemberRank.RoseSupervisor, 0);
        }

        if (guild.RankRoseDonations.Length >= 5) {
            var obj = guild.RankRoseDonations[4];
            Aprend(MemberRank.RoseAgent, obj.Roses);
        }
        else {
            Aprend(MemberRank.RoseAgent, 0);
        }

        if (guild.RankRoseDonations.Length >= 7) {
            var obj = guild.RankRoseDonations[6];
            Aprend(MemberRank.RoseFollower, obj.Roses);
        }
        else {
            Aprend(MemberRank.RoseFollower, 0);
        }


        if (guild.RankLiliesDonations.Length >= 3) {
            var obj = guild.RankLiliesDonations[2];
            Aprend(MemberRank.LilySupervisor, obj.Lilies);
        }
        else {
            Aprend(MemberRank.LilySupervisor, 0);
        }

        if (guild.RankLiliesDonations.Length >= 5) {
            var obj = guild.RankLiliesDonations[4];
            Aprend(MemberRank.LilyAgent, obj.Lilies);
        }
        else {
            Aprend(MemberRank.LilyAgent, 0);
        }

        if (guild.RankLiliesDonations.Length >= 7) {
            var obj = guild.RankLiliesDonations[6];
            Aprend(MemberRank.LilyFollower, obj.Lilies);
        }
        else {
            Aprend(MemberRank.LilyFollower, 0);
        }


        if (guild.RankTulipsDonations.Length >= 3) {
            var obj = guild.RankTulipsDonations[2];
            Aprend(MemberRank.TSupervisor, obj.Tulips);
        }
        else {
            Aprend(MemberRank.TSupervisor, 0);
        }

        if (guild.RankTulipsDonations.Length >= 5) {
            var obj = guild.RankTulipsDonations[4];
            Aprend(MemberRank.TulipAgent, obj.Tulips);
        }
        else {
            Aprend(MemberRank.TulipAgent, 0);
        }

        if (guild.RankTulipsDonations.Length >= 7) {
            var obj = guild.RankTulipsDonations[6];
            Aprend(MemberRank.TulipFollower, obj.Tulips);
        }
        else {
            Aprend(MemberRank.TulipFollower, 0);
        }


        if (guild.RankOrchidsDonations.Length >= 3) {
            var obj = guild.RankOrchidsDonations[2];
            Aprend(MemberRank.OSupervisor, obj.Orchids);
        }
        else {
            Aprend(MemberRank.OSupervisor, 0);
        }

        if (guild.RankOrchidsDonations.Length >= 5) {
            var obj = guild.RankOrchidsDonations[4];
            Aprend(MemberRank.OrchidAgent, obj.Orchids);
        }
        else {
            Aprend(MemberRank.OrchidAgent, 0);
        }

        if (guild.RankOrchidsDonations.Length >= 7) {
            var obj = guild.RankOrchidsDonations[6];
            Aprend(MemberRank.OrchidFollower, obj.Orchids);
        }
        else {
            Aprend(MemberRank.OrchidFollower, 0);
        }


        if (guild.RankTotalDonations.Length >= 2) {
            var obj = guild.RankTotalDonations[1];
            Aprend(MemberRank.HDeputyLeader, obj.TotalDonation);
        }
        else {
            Aprend(MemberRank.HDeputyLeader, 0);
        }

        if (guild.RankTotalDonations.Length >= 4) {
            var obj = guild.RankTotalDonations[3];
            Aprend(MemberRank.HonorarySteward, obj.TotalDonation);
        }
        else {
            Aprend(MemberRank.HonorarySteward, 0);
        }


        if (guild.RankSilversDonations.Length >= 4) {
            var obj = guild.RankSilversDonations[3];
            Aprend(MemberRank.SSupervisor, (uint)obj.SilverDonation);
        }
        else {
            Aprend(MemberRank.SSupervisor, 0);
        }

        if (guild.RankSilversDonations.Length >= 6) {
            var obj = guild.RankSilversDonations[5];
            Aprend(MemberRank.SilverAgent, (uint)obj.SilverDonation);
        }
        else {
            Aprend(MemberRank.SilverAgent, 0);
        }

        if (guild.RankSilversDonations.Length >= 8) {
            var obj = guild.RankSilversDonations[7];
            Aprend(MemberRank.SilverFollower, (uint)obj.SilverDonation);
        }
        else {
            Aprend(MemberRank.SilverFollower, 0);
        }


        if (guild.RankGuideDonations.Length >= 3) {
            var obj = guild.RankGuideDonations[2];
            Aprend(MemberRank.GSupervisor, obj.VirtuePoints);
        }
        else {
            Aprend(MemberRank.GSupervisor, 0);
        }

        if (guild.RankGuideDonations.Length >= 5) {
            var obj = guild.RankGuideDonations[4];
            Aprend(MemberRank.GuideAgent, obj.VirtuePoints);
        }
        else {
            Aprend(MemberRank.GuideAgent, 0);
        }

        if (guild.RankGuideDonations.Length >= 7) {
            var obj = guild.RankGuideDonations[6];
            Aprend(MemberRank.GuideFollower, obj.VirtuePoints);
        }
        else {
            Aprend(MemberRank.GuideFollower, 0);
        }
    }
}