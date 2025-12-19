using System;
using MTA.Client;
using MTA.Game.Features.Kiss;
using MTA.Game.Features.Kisses;
using MTA.Interfaces;

namespace MTA.Network.GamePackets {
    public class KissPacket : Writer, IPacket {
        public const ushort Letters = 5;
        public const ushort Wine = 6;
        public const ushort Kisses = 4;
        public const ushort Jades = 7;
        private byte[] Buffer2;

        public KissPacket(Kisses ClientKisses, GameState client) {
            Buffer2 = new byte[0x44];
            WriteUInt16(60, 0, Buffer2);
            WriteUInt16(0x47e, 2, Buffer2);
            WriteUInt32(3, 4, Buffer2);
            if (client.Entity.Body == 1001 || client.Entity.Body == 1002 || client.Entity.Body == 2001 ||
                client.Entity.Body == 2002) {
                if (client.Entity.Kisses.LastKissesSent.AddDays(1) <= DateTime.Now) {
                    WriteUInt32(1, 16, Buffer2);
                }
                else {
                    WriteUInt32(0, 4, Buffer2);
                    WriteUInt32(client.Entity.UID, 8, Buffer2);
                    WriteUInt32(0, 16, Buffer2);
                }
            }
            else {
                if (ClientKisses != null) {
                    WriteUInt32(ClientKisses.Kisses2, 16, Buffer2);
                    WriteUInt32(ClientKisses.Kisses2day, 20, Buffer2);
                    WriteUInt32(ClientKisses.Letters1, 24, Buffer2);
                    WriteUInt32(ClientKisses.LetterToday1, 28, Buffer2);
                    WriteUInt32(ClientKisses.Wine, 32, Buffer2);
                    WriteUInt32(ClientKisses.Wine2day, 36, Buffer2);
                    WriteUInt32(ClientKisses.Jades, 40, Buffer2);
                    WriteUInt32(ClientKisses.Jades2day, 44, Buffer2);
                }
            }
        }


        public KissPacket(bool Create) {
            if (Create) {
                Buffer2 = new byte[68];
                WriteUInt16(60, 0, Buffer2);
                WriteUInt16(1150, 2, Buffer2);
            }
        }

        public uint sub {
            get { return System.BitConverter.ToUInt32(Buffer2, 4); }
            set { WriteUInt32(value, 4, Buffer2); }
        }

        public uint Amount {
            get { return System.BitConverter.ToUInt32(Buffer2, 20); }
            set { WriteUInt32(value, 20, Buffer2); }
        }

        public KissType KissesType {
            get { return (KissType)System.BitConverter.ToUInt32(Buffer2, 0x18); }
        }

        public uint ItemUID {
            get { return System.BitConverter.ToUInt32(Buffer2, 12); }
            set { WriteUInt32(value, 12, Buffer2); }
        }

        public string ReceiverName {
            get { return System.BitConverter.ToString(Buffer2, 32, 16); }
            set { WriteString(value, 32, Buffer2); }
        }

        public uint SendAmount {
            get { return System.BitConverter.ToUInt32(Buffer2, 48); }
            set { WriteUInt32(value, 48, Buffer2); }
        }

        public string SenderName {
            get { return System.BitConverter.ToString(Buffer2, 16, 16); }
            set { WriteString(value, 16, Buffer2); }
        }

        public KissType SendKissesType {
            get { return (KissType)System.BitConverter.ToUInt32(Buffer2, 0x34); }
            set { WriteUInt32((uint)value + 4, 52, Buffer2); }
        }

        public uint UID1 {
            get { return System.BitConverter.ToUInt32(Buffer2, 8); }
            set { WriteUInt32(value, 8, Buffer2); }
        }

        public uint remove {
            get { return System.BitConverter.ToUInt32(Buffer2, 56); }
            set { WriteUInt32(value, 56, Buffer2); }
        }

        public uint UID2 {
            get { return System.BitConverter.ToUInt32(Buffer2, 10); }
            set { WriteUInt32(value, 10, Buffer2); }
        }

        public void Deserialize(byte[] Buffer2) {
            this.Buffer2 = Buffer2;
        }

        public void Send(GameState Client) {
            Client.Send(Buffer2);
        }

        public byte[] ToArray() {
            return this.Buffer2;
        }
    }
}