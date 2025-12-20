using System.Text;
using MTA.Client;
using MTA.Interfaces;

namespace MTA.Network.GamePackets {
    public class NameChange : Writer, IPacket {
        public enum NameChangeAction : ushort {
            DialogInfo = 3,
            FreeChange = 4,
            NameTaken = 2,
            Request = 0,
            Success = 1
        }

        private NameChangeAction _Action;
        public ushort _EditAllowed = 1;
        public ushort _EditCount;
        private string _name;
        private byte[] Buffer;

        public NameChange(bool Create) {
            if (Create) {
                Buffer = new byte[0x22];
                WriteUInt16((ushort)Buffer.Length, 0, Buffer);
                WriteUInt16(0x820, 2, Buffer);
            }
        }

        public NameChangeAction Action {
            get { return _Action; }
            set { _Action = (NameChangeAction)Buffer[4]; }
        }

        public ushort EditAllowed {
            get { return _EditAllowed; }
            set { _EditAllowed = BitConverter.ToUInt16(Buffer, 8); }
        }

        public ushort EditCount {
            get { return _EditCount; }
            set { _EditCount = BitConverter.ToUInt16(Buffer, 6); }
        }

        public string Name {
            get { return _name; }
            set { _name = Encoding.ASCII.GetString(Buffer, 10, 0x10).TrimEnd(new char[1]); }
        }

        public void Deserialize(byte[] buffer) {
            Buffer = buffer;
        }

        public void Send(GameState client) {
            client.Send(Buffer);
        }

        public byte[] ToArray() {
            return Buffer;
        }
    }
}