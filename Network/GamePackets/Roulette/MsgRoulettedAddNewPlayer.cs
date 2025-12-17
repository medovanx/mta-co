using System;
using MTA.Client;
using MTA.Interfaces;

namespace MTA.Network.GamePackets.Roulette {
    public class MsgRoulettedAddNewPlayer : IPacket {
        public byte[] packet;

        public ushort Length {
            set {
                packet = new byte[value + 8];
                Writer.WriteUshort((ushort)(packet.Length - 8), 0, packet);
            }
        }

        public ushort PacketID {
            set { Writer.WriteUshort(value, 2, packet); }
        }

        public uint UID {
            set { Writer.WriteUint(value, 4, packet); }
        }

        public uint Mesh {
            set { Writer.WriteUint(value, 8, packet); }
        }

        public MsgRouletteOpenGui.Color Color {
            set { Writer.Byte((byte)value, 12, packet); }
        }

        public string Name {
            set { Writer.WriteString(value, 13, packet); }
        }

        public byte[] ToArray() {
            return packet;
        }

        public void Send(GameState client) {
            client.Send(ToArray());
        }

        public void Deserialize(byte[] buffer) {
            throw new NotImplementedException();
        }

        public static MsgRoulettedAddNewPlayer Create() {
            MsgRoulettedAddNewPlayer ptr = new MsgRoulettedAddNewPlayer();
            ptr.Length = 29;
            ptr.PacketID = GamePackets.MsgRoulettedAddNewPlayer;
            return ptr;
        }
    }
}