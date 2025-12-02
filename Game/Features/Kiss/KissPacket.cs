namespace MTA.Network.GamePackets
{
    using MTA.Game.Features.Flowers;
    using MTA.Interfaces;
    using MTA.Network;
    using MTA.Game.Features.Kisses;
    using MTA.Client;
    using System;

    public class KissPacket : Writer, IPacket
    {

        private byte[] Buffer2;
        public const ushort Letters = 5;
        public const ushort Wine = 6;
        public const ushort Kisses = 4;
        public const ushort Jades = 7;

        public KissPacket(Kisses ClientKisses, Client.GameState client)
        {
            Buffer2 = new byte[0x44];
            Writer.WriteUInt16(60, 0, Buffer2);
            Writer.WriteUInt16(0x47e, 2, Buffer2);
            Writer.WriteUInt32(3, 4, Buffer2);
            if (client.Entity.Body == 1001 || client.Entity.Body == 1002 || client.Entity.Body == 2001 || client.Entity.Body == 2002)
            {
                if (client.Entity.Kisses.LastKissesSent.AddDays(1) <= DateTime.Now)
                {
                    Writer.WriteUInt32(1, 16, Buffer2);
                }
                else
                {
                    Writer.WriteUInt32(0, 4, Buffer2);
                    Writer.WriteUInt32(client.Entity.UID, 8, Buffer2);
                    Writer.WriteUInt32(0, 16, Buffer2);
                }
            }
            else
            {
                if (ClientKisses != null)
                {
                    Writer.WriteUInt32(ClientKisses.Kisses2, 16, Buffer2);
                    Writer.WriteUInt32(ClientKisses.Kisses2day, 20, Buffer2);
                    Writer.WriteUInt32(ClientKisses.Letters1, 24, Buffer2);
                    Writer.WriteUInt32(ClientKisses.LetterToday1, 28, Buffer2);
                    Writer.WriteUInt32(ClientKisses.Wine, 32, Buffer2);
                    Writer.WriteUInt32(ClientKisses.Wine2day, 36, Buffer2);
                    Writer.WriteUInt32(ClientKisses.Jades, 40, Buffer2);
                    Writer.WriteUInt32(ClientKisses.Jades2day, 44, Buffer2);
                }
            }
        }



        public KissPacket(bool Create)
        {
            if (Create)
            {
                Buffer2 = new byte[68];
                Writer.WriteUInt16(60, 0, Buffer2);
                Writer.WriteUInt16(1150, 2, Buffer2);
            }
        }

        public void Deserialize(byte[] Buffer2)
        {
            this.Buffer2 = Buffer2;
        }

        public void Send(Client.GameState Client)
        {
            Client.Send(Buffer2);
        }

        public byte[] ToArray()
        {
            return this.Buffer2;
        }
        public uint sub
        {
            get
            {
                return BitConverter.ToUInt32(Buffer2, 4);
            }
            set
            {
                Writer.WriteUInt32(value, 4, Buffer2);
            }
        }
        public uint Amount
        {
            get
            {
                return BitConverter.ToUInt32(Buffer2, 20);
            }
            set
            {
                Writer.WriteUInt32(value, 20, Buffer2);
            }
        }

        public KissType KissesType
        {
            get
            {
                return (KissType)BitConverter.ToUInt32(Buffer2, 0x18);
            }
        }

        public uint ItemUID
        {
            get
            {
                return BitConverter.ToUInt32(Buffer2, 12);
            }
            set
            {
                Writer.WriteUInt32(value, 12, Buffer2);
            }
        }

        public string ReceiverName
        {
            get
            {
                return BitConverter.ToString(Buffer2, 32, 16);
            }
            set
            {
                Writer.WriteString(value, 32, Buffer2);
            }
        }

        public uint SendAmount
        {
            get
            {
                return BitConverter.ToUInt32(Buffer2, 48);
            }
            set
            {
                Writer.WriteUInt32(value, 48, Buffer2);
            }
        }

        public string SenderName
        {
            get
            {
                return BitConverter.ToString(Buffer2, 16, 16);
            }
            set
            {
                Writer.WriteString(value, 16, Buffer2);
            }
        }

        public KissType SendKissesType
        {
            get
            {
                return (KissType)BitConverter.ToUInt32(Buffer2, 0x34);
            }
            set
            {
                Writer.WriteUInt32((uint)value + 4, 52, Buffer2);
            }
        }

        public uint UID1
        {
            get { return BitConverter.ToUInt32(Buffer2, 8); }
            set { WriteUInt32(value, 8, Buffer2); }
        }
        public uint remove
        {
            get { return BitConverter.ToUInt32(Buffer2, 56); }
            set { WriteUInt32(value, 56, Buffer2); }
        }

        public uint UID2
        {
            get { return BitConverter.ToUInt32(Buffer2, 10); }
            set { WriteUInt32(value, 10, Buffer2); }
        }
    }
}