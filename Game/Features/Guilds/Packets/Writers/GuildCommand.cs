// ReSharper disable InconsistentNaming

using System.Linq;
using System.Text;
using MTA.Client;
using MTA.Game.Features.Guilds.Constants;
using MTA.Game.Features.Guilds.Services;
using MTA.Interfaces;
using MTA.Network;

namespace MTA.Game.Features.Guilds.Packets.Writers;

/// <summary>
///     Packet class for guild command operations (packet 1107), handling all guild-related actions such as promotions, donations, and relationships.
/// </summary>
public class GuildCommand : Writer, IPacket {
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
        DischargeDeputyLeader = 30,
        DischargeAide = 33,
        PromoteWithCP = 34,
        DischargeRank = 36,
        PromoteInfo = 38,
        RequestPromote = 37,
        LeaderAbsenceDonation = 45,
        AddToBlacklist = 48,
        RemoveFromBlacklist = 50;

    private byte[] _buffer;

    public GuildCommand(bool create) {
        _buffer = new byte[8 + 28];
        if (!create) return;
        WriteUInt16(28, 0, _buffer);
        WriteUInt16((ushort)Game.Constants.Packets.MsgSyndicate, 2, _buffer);
    }

    public GuildCommand(uint length) {
        _buffer = new byte[8 + 28 + length];
        WriteUInt16((ushort)(_buffer.Length - 8), 0, _buffer);
        WriteUInt16((ushort)Game.Constants.Packets.MsgSyndicate, 2, _buffer);
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
    ///     Level offset for GuildRequirements
    /// </summary>
    public uint DwParam2 {
        get => BitConverter.ToUInt32(_buffer, 12);
        init => WriteUInt32(value, 12, _buffer);
    }

    /// <summary>
    ///     Reborn offset for GuildRequirements
    /// </summary>
    public uint DwParam3 {
        get => BitConverter.ToUInt32(_buffer, 16);
        init => WriteUInt32(value, 16, _buffer);
    }

    /// <summary>
    ///     Class offset for GuildRequirements
    /// </summary>
    public uint DwParam4 {
        get => BitConverter.ToUInt32(_buffer, 20);
        init => WriteUInt32(value, 20, _buffer);
    }

    /// <summary>
    ///     offset bulletin
    /// </summary>
    public string Str {
        set {
            WriteByte(1, 24, _buffer);
            WriteByte((byte)value.Length, 25, _buffer);
            WriteString(value, 26, _buffer);
        }
    }

    /// <summary>
    ///     Parses incoming command packet from the client.
    /// </summary>
    public void Deserialize(byte[] data) {
        _buffer = data;
    }

    /// <summary>
    ///     Returns the command packet byte array.
    /// </summary>
    public byte[] ToArray() {
        return _buffer;
    }

    /// <summary>
    ///     Sends the command packet to the client.
    /// </summary>
    public void Send(GameState client) {
        client.Send(_buffer);
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

    /// <summary>
    ///     Sends promotion options to the client based on the member's current rank, including available ranks, current counts, limits, and CP costs.
    /// </summary>
    public void SendPromote(GameState client, ushort typ) {
        if (client.AsMember == null || client.Guild == null) return;

        var builder = new StringBuilder();
        var promotionOptions = GuildPromotionOptions.GetPromotionOptions(client.AsMember.Rank);

        var list = promotionOptions.Select(option => {
            var currentCount = client.Guild.RanksCounts[(ushort)option.Rank];
            var maxLimit = GuildPromotionOptions.GetMaxLimit(option, client.Guild.Level);
            var potency = (int)client.Guild.GetMemberPotency(option.Rank);

            return CreatePromotionString(builder, option.Rank, currentCount, maxLimit, potency,
                option.ConquerPointsCost);
        }).ToList();

        var extraLength = list.Sum(str => str.Length + 1);
        var packet = new byte[28 + 8 + extraLength];
        WriteUInt16((ushort)(packet.Length - 8), 0, packet);
        WriteUInt16((ushort)Game.Constants.Packets.MsgSyndicate, 2, packet);
        WriteByte((byte)typ, 4, packet);
        WriteStringList(list, 24, packet);
        client.Send(packet);
    }
}