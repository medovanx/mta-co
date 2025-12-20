using System.Text;
using MTA.Game.Features.Flowers;

namespace MTA.Network.GamePackets {
    public class SendFlower : Writer {
        public const uint FlowerSender = 2;
        public const uint Flower = 3;
        private byte[] Packet;

        public SendFlower() {
            Packet = new byte[68];
            WriteUInt16(60, 0, Packet);
            WriteUInt16(1150, 2, Packet);
        }

        public uint Typing {
            get { return BitConverter.ToUInt32(Packet, 4); }
            set { WriteUInt32(value, 4, Packet); }
        }

        public string SenderName {
            get { return Encoding.ASCII.GetString(Packet, 16, 16); }
            set { WriteString(value, 16, Packet); }
        }

        public string ReceiverName {
            get { return Encoding.ASCII.GetString(Packet, 32, 16); }
            set { WriteString(value, 32, Packet); }
        }

        public uint Amount {
            get { return BitConverter.ToUInt32(Packet, 48); }
            set { WriteUInt32(value, 48, Packet); }
        }

        public uint FType {
            get { return BitConverter.ToUInt32(Packet, 52); }
            set { WriteUInt32(value, 52, Packet); }
        }

        public uint Effect {
            get { return BitConverter.ToUInt32(Packet, 56); }
            set { WriteUInt32(value, 56, Packet); }
        }

        public byte[] ToArray() {
            return Packet;
        }

        public void Apprend(Flowers flowers) {
            WriteUInt32(flowers.RedRoses, 16, Packet);
            WriteUInt32(flowers.RedRoses2day, 20, Packet);
            WriteUInt32(flowers.Lilies, 24, Packet);
            WriteUInt32(flowers.Lilies2day, 28, Packet);
            WriteUInt32(flowers.Orchads, 32, Packet);
            WriteUInt32(flowers.Orchads2day, 36, Packet);
            WriteUInt32(flowers.Tulips, 40, Packet);
            WriteUInt32(flowers.Tulips2day, 44, Packet);
        }
    }
}