using System;
using MTA.Client;
using MTA.Interfaces;

namespace MTA.Network.GamePackets.Roulette {
    public class MsgRouletteNoWinner : IPacket {
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

        public byte Number {
            set { Writer.Byte(value, 4, packet); }
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


        public static MsgRouletteNoWinner Create() {
            MsgRouletteNoWinner packet = new MsgRouletteNoWinner();
            packet.Length = 5;
            packet.PacketID = GamePackets.MsgRouletteNoWinner;
            return packet;
        }
    }
}