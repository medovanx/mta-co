using System.IO;

namespace MTA.Game.Features.Guilds.Packets {
    public class GuildMemberList {
        public ushort Size;
        public ushort Type;
        public ushort SubType;
        public ushort PageNumber;
        public required Guild Guild;

        public GuildMemberList(byte[] packet, Guild guild) {
            Guild = guild;
            var reader = new BinaryReader(new MemoryStream(packet));
            Size = reader.ReadUInt16();
            Type = reader.ReadUInt16();
            SubType = reader.ReadUInt16();
            PageNumber = reader.ReadUInt16();
        }

        public byte[] Build() {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write((ushort)0);
            writer.Write((ushort)2102);
            writer.Write((ushort)0);
            writer.Write((ushort)1); //page
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)Guild.Members.Count); //count
            writer.Write((ushort)0);
            foreach (var m in Guild.Members.Values) {
                for (var i = 0; i < 16; i++) //16 offsets
                {
                    if (i < m.Name.Length) {
                        writer.Write((byte)m.Name[i]);
                    }
                    else
                        writer.Write((byte)0);
                }

                writer.Write((ushort)m.NobilityRank);
                writer.Write((ushort)0);
                writer.Write((ushort)1);
                writer.Write((ushort)0);
                writer.Write((uint)m.Level);
                writer.Write((uint)m.Rank);
                writer.Write((uint)0);
                writer.Write((uint)m.SilverDonation);
                if (m.Client != null) {
                    writer.Write((byte)1);
                }
                else {
                    writer.Write((byte)0);
                }

                writer.Write((byte)0);
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.Write((ushort)0);
            }

            var packetlength = (int)stream.Length;
            stream.Position = 0;
            writer.Write((ushort)packetlength);
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

    public class GuildDonationList {
        public ushort Size;
        public ushort Type;
        private readonly ushort _subType;
        private readonly ushort _pageNumber;
        public required Guild Guild;

        public GuildDonationList(byte[] packet, Guild guild) {
            Guild = guild;
            var reader = new BinaryReader(new MemoryStream(packet));
            Size = reader.ReadUInt16();
            Type = reader.ReadUInt16();
            _subType = reader.ReadUInt16();
            _pageNumber = reader.ReadUInt16();
        }

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
            for (var i = 0; i < 16; i++) //16 offsets
            {
                if (i < "TestName".Length) {
                    writer.Write((byte)"TestName"[i]);
                }
                else
                    writer.Write((byte)0);
            }

            writer.Write((ulong)0);
            writer.Write((uint)130); //level
            writer.Write((uint)1000); //guildrank
            writer.Write((uint)0); //unknown
            writer.Write((uint)10000); //donation
            writer.Write((byte)1); //online-offline
            writer.Write((byte)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            var packetlength = (int)stream.Length;
            stream.Position = 0;
            writer.Write((ushort)packetlength);
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
}