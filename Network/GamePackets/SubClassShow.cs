using System;

namespace MTA.Network.GamePackets {
    public class SubClassShow : Writer {
        public const byte SwitchSubClass = 0;
        public const byte ActivateSubClass = 1;
        public const byte Upgrade = 2;
        public const byte SendUpdate = 3;
        public const byte LearnSubClass = 4;
        public const byte MartialPromoted = 5;
        public const byte Open = 6;
        public const byte ShowGUI = 7;
        public const byte Animation = 8;
        public const byte Join = 9;
        public const byte Pro = 10;

        public static uint[] ItemsPromote = new uint[]
            { 0, 721259, 721261, 711188, 723087, 1088001, 711679, 0, 0, 723903 };

        public static byte[] ItemsCount = new byte[] { 0, 5, 10, 1, 20, 10, 1, 0, 0, 40 };


        private byte[] packet;
        private ushort Position = 30;

        public SubClassShow(ushort entry = 0) {
            packet = new byte[38 + entry * 3];
            WriteUInt16((ushort)(packet.Length - 8), 0, packet);
            WriteUInt16(2320, 2, packet);
            WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, packet);
            if (entry != 0) {
                Count = entry;
            }
        }

        public ushort ID {
            get { return BitConverter.ToUInt16(packet, 8); }
            set { WriteUInt16(value, 8, packet); }
        }

        public byte Class {
            get { return packet[10]; }
            set { WriteByte(value, 10, packet); }
        }

        public byte Level {
            get { return packet[11]; }
            set { WriteByte(value, 11, packet); }
        }

        public ushort Study {
            get { return BitConverter.ToUInt16(packet, 10); }
            set { WriteUInt16(value, 10, packet); }
        }

        public ushort StudyReceive {
            get { return BitConverter.ToUInt16(packet, 18); }
            set { WriteUInt16(value, 18, packet); }
        }

        public ushort Count {
            get { return BitConverter.ToUInt16(packet, 26); }
            set { WriteUInt16(value, 26, packet); }
        }

        public void Apprend(byte ID, byte Pharse, byte Level) {
            if ((packet.Length - 8) >= Position + 3) {
                WriteByte(ID, Position, packet);
                Position++;
                WriteByte(Pharse, Position, packet);
                Position++;
                WriteByte(Level, Position, packet);
                Position++;
            }
        }

        public byte[] ToArray() {
            return packet;
        }
    }
}