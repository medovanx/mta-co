using System.IO;
using MTA.Game.Features.Guilds.Models;

namespace MTA.Game.Features.Guilds.Packets.Writers;

/// <summary>
///     Constructs packet 2102 for guild member list display, showing all guild members with their ranks, levels, and donations.
/// </summary>
public class GuildMemberList {
    public required Guild Guild;
    public ushort PageNumber;
    public ushort Size;
    public ushort SubType;
    public ushort Type;

    public GuildMemberList(byte[] packet, Guild guild) {
        Guild = guild;
        var reader = new BinaryReader(new MemoryStream(packet));
        Size = reader.ReadUInt16();
        Type = reader.ReadUInt16();
        SubType = reader.ReadUInt16();
        PageNumber = reader.ReadUInt16();
    }

    /// <summary>
    ///     Builds the member list packet with all guild members, including their names, ranks, levels, silver donations, and online status.
    /// </summary>
    public byte[] Build() {
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);

        writer.Write((ushort)0);
        writer.Write((ushort)Game.Constants.Packets.MsgSynMemberList);
        writer.Write((ushort)0);
        writer.Write((ushort)1); //page
        writer.Write((ushort)0);
        writer.Write((ushort)0);
        writer.Write((ushort)Guild.Members.Count); //count
        writer.Write((ushort)0);
        foreach (var member in Guild.Members.Values) {
            for (var i = 0; i < 16; i++) //16 offsets
            {
                if (i < member.Name.Length)
                    writer.Write((byte)member.Name[i]);
                else
                    writer.Write((byte)0);
            }

            writer.Write((ushort)member.NobilityRank);
            writer.Write((ushort)0);
            writer.Write((ushort)1);
            writer.Write((ushort)0);
            writer.Write((uint)member.Level);
            writer.Write((uint)member.Rank);
            writer.Write((uint)0);
            writer.Write((uint)member.SilverDonation);
            if (member.Client != null)
                writer.Write((byte)1);
            else
                writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
        }

        var streamLength = (int)stream.Length;
        stream.Position = 0;
        writer.Write((ushort)streamLength);
        stream.Position = stream.Length;
        writer.Write(Program.Encoding.GetBytes("TQServer"));
        stream.Position = 0;
        var buf = new byte[stream.Length];
        stream.ReadExactly(buf, 0, buf.Length);
        writer.Close();
        stream.Close();
        return buf;
    }
}