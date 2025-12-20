using System;
using MTA.Client;
using MTA.Interfaces;

namespace MTA.Network.GamePackets.Roulette {
    public class MsgRouletteScreen : IPacket {
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

        public byte UnKnow {
            set { Writer.Byte(value, 4, packet); }
        }

        public uint UID {
            set { Writer.WriteUint(value, 5, packet); }
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

        public static unsafe MsgRouletteScreen Create() {
            MsgRouletteScreen ptr = new MsgRouletteScreen();
            ptr.Length = 9;
            ptr.UnKnow = 1;
            ptr.PacketID = GamePackets.MsgRouletteScreen;
            return ptr;
        }
    }
}