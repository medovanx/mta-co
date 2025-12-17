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
            this.packet = new byte[(int)(38 + entry * 3)];
            WriteUInt16((ushort)(this.packet.Length - 8), 0, this.packet);
            WriteUInt16(2320, 2, this.packet);
            WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, this.packet);
            if (entry != 0) {
                this.Count = entry;
            }
        }

        public ushort ID {
            get { return BitConverter.ToUInt16(this.packet, 8); }
            set { WriteUInt16(value, 8, this.packet); }
        }

        public byte Class {
            get { return this.packet[10]; }
            set { WriteByte(value, 10, this.packet); }
        }

        public byte Level {
            get { return this.packet[11]; }
            set { WriteByte(value, 11, this.packet); }
        }

        public ushort Study {
            get { return BitConverter.ToUInt16(this.packet, 10); }
            set { WriteUInt16(value, 10, this.packet); }
        }

        public ushort StudyReceive {
            get { return BitConverter.ToUInt16(this.packet, 18); }
            set { WriteUInt16(value, 18, this.packet); }
        }

        public ushort Count {
            get { return BitConverter.ToUInt16(this.packet, 26); }
            set { WriteUInt16(value, 26, this.packet); }
        }

        public void Apprend(byte ID, byte Pharse, byte Level) {
            if ((this.packet.Length - 8) >= this.Position + 3) {
                WriteByte(ID, Position, this.packet);
                Position++;
                WriteByte(Pharse, Position, this.packet);
                Position++;
                WriteByte(Level, Position, this.packet);
                Position++;
            }
        }

        public byte[] ToArray() {
            return this.packet;
        }
    }
}