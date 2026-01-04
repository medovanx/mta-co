using MTA.Game.Features.Guilds.Constants;
using MTA.Game.Features.Guilds.Models;
using MTA.Network;

namespace MTA.Game.Features.Guilds.Packets.Writers;

/// <summary>
///     Constructs packet 1061 showing minimum donation requirements for each rank, used to display donation thresholds needed for promotion.
/// </summary>
public class GuildMinDonations : Writer {
    private readonly byte[] _packet;
    private ushort _position = 8;

    public GuildMinDonations(ushort counts = 0) //31
    {
        _packet = new byte[(ushort)(16 + counts * 8)];
        WriteUInt16((ushort)(_packet.Length - 8), 0, _packet);
        WriteUInt16((ushort)Game.Constants.Packets.MsgDutyMinContri, 2, _packet);
        WriteUInt16(counts, 6, _packet);
    }

    /// <summary>
    ///     Returns the packet byte array ready to be sent to the client.
    /// </summary>
    public byte[] ToArray() {
        return _packet;
    }

    private void Append(MemberRank rank, uint amount) {
        WriteUInt32((ushort)rank, _position, _packet);
        _position += 4;
        //WriteUint(uint.MaxValue, Position);//for not append
        WriteUInt32(amount, _position, _packet);
        _position += 4;
    }

    public void AppendGuild(Guild guild) {
        if (guild.RankArsenalDonations.Length >= 5) {
            var obj = guild.RankArsenalDonations[4];
            Append(MemberRank.Manager, obj.ArsenalDonation);
        }
        else {
            Append(MemberRank.Manager, 0);
        }

        if (guild.RankArsenalDonations.Length >= 7) {
            var obj = guild.RankArsenalDonations[6];
            Append(MemberRank.HonoraryManager, obj.ArsenalDonation);
        }
        else {
            Append(MemberRank.HonoraryManager, 0);
        }


        if (guild.RankArsenalDonations.Length >= 8) {
            var obj = guild.RankArsenalDonations[7];
            Append(MemberRank.Supervisor, obj.ArsenalDonation);
        }
        else {
            Append(MemberRank.Supervisor, 0);
        }

        if (guild.RankArsenalDonations.Length >= 13) {
            var obj = guild.RankArsenalDonations[12];
            Append(MemberRank.Steward, obj.ArsenalDonation);
        }
        else {
            Append(MemberRank.Steward, 0);
        }

        if (guild.RankArsenalDonations.Length >= 15) {
            var obj = guild.RankArsenalDonations[14];
            Append(MemberRank.ArsFollower, obj.ArsenalDonation);
        }
        else {
            Append(MemberRank.ArsFollower, 0);
        }


        if (guild.RankCpDonations.Length >= 3) {
            var obj = guild.RankCpDonations[2];
            Append(MemberRank.CPSupervisor, (uint)obj.ConquerPointDonation);
        }
        else {
            Append(MemberRank.CPSupervisor, 0);
        }

        if (guild.RankCpDonations.Length >= 5) {
            var obj = guild.RankCpDonations[4];
            Append(MemberRank.CPAgent, (uint)obj.ConquerPointDonation);
        }
        else {
            Append(MemberRank.CPAgent, 0);
        }

        if (guild.RankCpDonations.Length >= 7) {
            var obj = guild.RankCpDonations[6];
            Append(MemberRank.CPFollower, (uint)obj.ConquerPointDonation);
        }
        else {
            Append(MemberRank.CPFollower, 0);
        }


        if (guild.RankPkDonations.Length >= 3) {
            var obj = guild.RankPkDonations[2];
            Append(MemberRank.PKSupervisor, obj.PkDonation);
        }
        else {
            Append(MemberRank.PKSupervisor, 0);
        }

        if (guild.RankPkDonations.Length >= 5) {
            var obj = guild.RankPkDonations[4];
            Append(MemberRank.PKAgent, obj.PkDonation);
        }
        else {
            Append(MemberRank.PKAgent, 0);
        }

        if (guild.RankPkDonations.Length >= 7) {
            var obj = guild.RankPkDonations[6];
            Append(MemberRank.PKFollower, obj.PkDonation);
        }
        else {
            Append(MemberRank.PKFollower, 0);
        }


        if (guild.RankRoseDonations.Length >= 3) {
            var obj = guild.RankRoseDonations[2];
            Append(MemberRank.RoseSupervisor, obj.Roses);
        }
        else {
            Append(MemberRank.RoseSupervisor, 0);
        }

        if (guild.RankRoseDonations.Length >= 5) {
            var obj = guild.RankRoseDonations[4];
            Append(MemberRank.RoseAgent, obj.Roses);
        }
        else {
            Append(MemberRank.RoseAgent, 0);
        }

        if (guild.RankRoseDonations.Length >= 7) {
            var obj = guild.RankRoseDonations[6];
            Append(MemberRank.RoseFollower, obj.Roses);
        }
        else {
            Append(MemberRank.RoseFollower, 0);
        }


        if (guild.RankLiliesDonations.Length >= 3) {
            var obj = guild.RankLiliesDonations[2];
            Append(MemberRank.LilySupervisor, obj.Lilies);
        }
        else {
            Append(MemberRank.LilySupervisor, 0);
        }

        if (guild.RankLiliesDonations.Length >= 5) {
            var obj = guild.RankLiliesDonations[4];
            Append(MemberRank.LilyAgent, obj.Lilies);
        }
        else {
            Append(MemberRank.LilyAgent, 0);
        }

        if (guild.RankLiliesDonations.Length >= 7) {
            var obj = guild.RankLiliesDonations[6];
            Append(MemberRank.LilyFollower, obj.Lilies);
        }
        else {
            Append(MemberRank.LilyFollower, 0);
        }


        if (guild.RankTulipsDonations.Length >= 3) {
            var obj = guild.RankTulipsDonations[2];
            Append(MemberRank.TSupervisor, obj.Tulips);
        }
        else {
            Append(MemberRank.TSupervisor, 0);
        }

        if (guild.RankTulipsDonations.Length >= 5) {
            var obj = guild.RankTulipsDonations[4];
            Append(MemberRank.TulipAgent, obj.Tulips);
        }
        else {
            Append(MemberRank.TulipAgent, 0);
        }

        if (guild.RankTulipsDonations.Length >= 7) {
            var obj = guild.RankTulipsDonations[6];
            Append(MemberRank.TulipFollower, obj.Tulips);
        }
        else {
            Append(MemberRank.TulipFollower, 0);
        }


        if (guild.RankOrchidsDonations.Length >= 3) {
            var obj = guild.RankOrchidsDonations[2];
            Append(MemberRank.OSupervisor, obj.Orchids);
        }
        else {
            Append(MemberRank.OSupervisor, 0);
        }

        if (guild.RankOrchidsDonations.Length >= 5) {
            var obj = guild.RankOrchidsDonations[4];
            Append(MemberRank.OrchidAgent, obj.Orchids);
        }
        else {
            Append(MemberRank.OrchidAgent, 0);
        }

        if (guild.RankOrchidsDonations.Length >= 7) {
            var obj = guild.RankOrchidsDonations[6];
            Append(MemberRank.OrchidFollower, obj.Orchids);
        }
        else {
            Append(MemberRank.OrchidFollower, 0);
        }


        if (guild.RankTotalDonations.Length >= 2) {
            var obj = guild.RankTotalDonations[1];
            Append(MemberRank.HDeputyLeader, obj.TotalDonation);
        }
        else {
            Append(MemberRank.HDeputyLeader, 0);
        }

        if (guild.RankTotalDonations.Length >= 4) {
            var obj = guild.RankTotalDonations[3];
            Append(MemberRank.HonorarySteward, obj.TotalDonation);
        }
        else {
            Append(MemberRank.HonorarySteward, 0);
        }


        if (guild.RankSilversDonations.Length >= 4) {
            var obj = guild.RankSilversDonations[3];
            Append(MemberRank.SSupervisor, (uint)obj.SilverDonation);
        }
        else {
            Append(MemberRank.SSupervisor, 0);
        }

        if (guild.RankSilversDonations.Length >= 6) {
            var obj = guild.RankSilversDonations[5];
            Append(MemberRank.SilverAgent, (uint)obj.SilverDonation);
        }
        else {
            Append(MemberRank.SilverAgent, 0);
        }

        if (guild.RankSilversDonations.Length >= 8) {
            var obj = guild.RankSilversDonations[7];
            Append(MemberRank.SilverFollower, (uint)obj.SilverDonation);
        }
        else {
            Append(MemberRank.SilverFollower, 0);
        }


        if (guild.RankGuideDonations.Length >= 3) {
            var obj = guild.RankGuideDonations[2];
            Append(MemberRank.GSupervisor, obj.VirtuePoints);
        }
        else {
            Append(MemberRank.GSupervisor, 0);
        }

        if (guild.RankGuideDonations.Length >= 5) {
            var obj = guild.RankGuideDonations[4];
            Append(MemberRank.GuideAgent, obj.VirtuePoints);
        }
        else {
            Append(MemberRank.GuideAgent, 0);
        }

        if (guild.RankGuideDonations.Length >= 7) {
            var obj = guild.RankGuideDonations[6];
            Append(MemberRank.GuideFollower, obj.VirtuePoints);
        }
        else {
            Append(MemberRank.GuideFollower, 0);
        }
    }
}