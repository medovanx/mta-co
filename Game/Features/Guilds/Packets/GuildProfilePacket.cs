namespace MTA.Game.Features.Guilds.Packets {
    using Network;

    public class GuildProfilePacket(byte[] packet) : Writer, Interfaces.IPacket {
        private byte[] _packet = packet;

        public uint Silver {
            get => BitConverter.ToUInt32(_packet, 8);
            set => WriteUInt32(value, 8, _packet);
        }

        public uint Pk {
            get => BitConverter.ToUInt32(_packet, 20);
            set => WriteUInt32(value, 20, _packet);
        }

        public uint Cps {
            get => BitConverter.ToUInt32(_packet, 12);
            set => WriteUInt32(value, 12, _packet);
        }

        public uint Guide {
            get => BitConverter.ToUInt32(_packet, 16);
            set => WriteUInt32(value, 16, _packet);
        }

        public uint Arsenal {
            get => BitConverter.ToUInt32(_packet, 24);
            set => WriteUInt32(value, 24, _packet);
        }

        public uint Rose {
            get => BitConverter.ToUInt32(_packet, 28);
            set => WriteUInt32(value, 28, _packet);
        }

        public uint Orchid {
            get => BitConverter.ToUInt32(_packet, 32);
            set => WriteUInt32(value, 32, _packet);
        }

        public uint Lily {
            get => BitConverter.ToUInt32(_packet, 36);
            set => WriteUInt32(value, 36, _packet);
        }

        public uint Tulip {
            get => BitConverter.ToUInt32(_packet, 40);
            set => WriteUInt32(value, 40, _packet);
        }

        public uint HistorySilvers {
            get => BitConverter.ToUInt32(_packet, 44);
            set => WriteUInt32(value, 44, _packet);
        }

        public uint HistoryCps {
            get => BitConverter.ToUInt32(_packet, 48);
            set => WriteUInt32(value, 48, _packet);
        }

        public uint HistoryGuide {
            get => BitConverter.ToUInt32(_packet, 52);
            set => WriteUInt32(value, 52, _packet);
        }

        public uint HistoryPk {
            get => BitConverter.ToUInt32(_packet, 56);
            set => WriteUInt32(value, 56, _packet);
        }

        public void Send(Client.GameState client) {
            client.Send(_packet);
        }

        public void Deserialize(byte[] data) {
            _packet = data;
        }

        public byte[] ToArray() {
            return _packet;
        }
    }
}