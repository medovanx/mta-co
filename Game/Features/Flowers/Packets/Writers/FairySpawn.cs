using MTA.Client;
using MTA.Interfaces;
using MTA.Network;

namespace MTA.Game.Features.Flowers.Packets.Writers {
    public class FairySpawn : Writer, IPacket {
        private byte[] _buf;

        public FairySpawn(bool create, byte[]? buf) {
            if (!create) {
                this._buf = buf;
                return;
            }
            this._buf = new byte[28];
            WriteUInt16(20, 0, this._buf);
            WriteUInt16(2070, 2, this._buf);
        }

        public uint SType {
            get => BitConverter.ToUInt32(_buf, 4);
            init => WriteUInt32(value, 4, _buf);
        }

        public uint Unknown {
            get => BitConverter.ToUInt32(_buf, 8);
            set => WriteUInt32(value, 8, _buf);
        }

        public uint FairyType {
            get => BitConverter.ToUInt32(_buf, 12);
            init => WriteUInt32(value, 12, _buf);
        }

        public uint Uid {
            get => BitConverter.ToUInt32(_buf, 16);
            set => WriteUInt32(value, 16, _buf);
        }

        public void Send(GameState client) {
            client.Send(_buf);
        }

        public byte[] ToArray() {
            return _buf;
        }

        public void Deserialize(byte[] buffer) {
            _buf = buffer;
        }
    }
}