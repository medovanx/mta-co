using System.IO;
using MTA.Game.Features.Guilds.Models;

namespace MTA.Game.Features.Guilds.Packets.Writers;

/// <summary>
///     Builds the packet that displays the guild donation list to clients. This packet shows
///     member donation information including silver, CP, and other contributions. Currently,
///     contains placeholder data structure - the actual member donation data should be populated
///     from the guild's member list.
/// </summary>
public class GuildDonationList {
    private readonly ushort _pageNumber;
    private readonly ushort _subType;
    public required Guild Guild;
    public ushort Size;
    public ushort Type;

    /// <summary>
    ///     Initializes the donation list packet parser from incoming client data.
    /// </summary>
    /// <param name="packet">The incoming packet data</param>
    /// <param name="guild">The guild to build the donation list for</param>
    public GuildDonationList(byte[] packet, Guild guild) {
        Guild = guild;
        var reader = new BinaryReader(new MemoryStream(packet));
        Size = reader.ReadUInt16();
        Type = reader.ReadUInt16();
        _subType = reader.ReadUInt16();
        _pageNumber = reader.ReadUInt16();
    }

    /// <summary>
    ///     Constructs the donation list packet to send to the client. The packet includes the total
    ///     member count and sample member data (level, rank, donation amount, online status).
    ///     Note: Currently uses placeholder data - should be updated to iterate through actual
    ///     guild members and include their real donation statistics.
    /// </summary>
    /// <returns>The complete packet byte array ready to send to the client</returns>
    public byte[] Build() {
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);

        writer.Write((ushort)0);
        writer.Write((ushort)2102);
        writer.Write(_subType);
        writer.Write(_pageNumber); //page
        writer.Write((ushort)0);
        writer.Write((ushort)0);
        writer.Write((ushort)Guild.Members.Count); //count
        writer.Write((ushort)0);
        for (var i = 0; i < 16; i++) {
            if (i < "TestName".Length)
                writer.Write((byte)"TestName"[i]);
            else
                writer.Write((byte)0);
        }

        writer.Write((ulong)0);
        writer.Write((uint)130); //level
        writer.Write((uint)1000); //guild rank
        writer.Write((uint)0); //unknown
        writer.Write((uint)10000); //donation
        writer.Write((byte)1); //online-offline
        writer.Write((byte)0);
        writer.Write((ushort)0);
        writer.Write((ushort)0);
        writer.Write((ushort)0);
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