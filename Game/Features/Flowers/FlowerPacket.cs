namespace MTA.Network.GamePackets
{
    using MTA.Game.Features.Flowers;
    using MTA.Interfaces;
    using MTA.Network;
    using MTA.Client;
    using System;

    public class FlowerPacket : Writer, IPacket
    {

        private byte[] Buffer;
        public const ushort Lilies = 1;
        public const ushort Orchids = 2;
        public const ushort RedRoses = 0;
        public const ushort Tulips = 3;

        public FlowerPacket(MTA.Game.Features.Flowers.Flowers ClientFlowers, Client.GameState client)
        {
            Buffer = new byte[0x44];
            Writer.WriteUInt16(60, 0, Buffer);
            Writer.WriteUInt16(0x47e, 2, Buffer);
            Writer.WriteUInt32(2, 4, Buffer);
            if (client.Entity.Body == 1003 || client.Entity.Body == 1004 || client.Entity.Body == 2003 || client.Entity.Body == 2004)
            {
                if (DateTime.Now >= client.Entity.Flowers.LastFlowerSent.AddDays(1))
                {
                    Writer.WriteUInt32(30, 16, Buffer);
                }
                else
                {
                    Writer.WriteUInt32(0, 4, Buffer);
                    Writer.WriteUInt32(client.Entity.UID, 8, Buffer);
                    Writer.WriteUInt32(0, 16, Buffer);
                }

            }
            else
            {

                if (ClientFlowers != null)
                {
                    Writer.WriteUInt32(ClientFlowers.RedRoses, 16, Buffer);
                    Writer.WriteUInt32(ClientFlowers.RedRoses2day, 20, Buffer);
                    Writer.WriteUInt32(ClientFlowers.Lilies, 24, Buffer);
                    Writer.WriteUInt32(ClientFlowers.Lilies2day, 28, Buffer);
                    Writer.WriteUInt32(ClientFlowers.Orchads, 32, Buffer);
                    Writer.WriteUInt32(ClientFlowers.Orchads2day, 36, Buffer);
                    Writer.WriteUInt32(ClientFlowers.Tulips, 40, Buffer);
                    Writer.WriteUInt32(ClientFlowers.Tulips2day, 44, Buffer);
                }
            }

        }
        public FlowerPacket(bool Create)
        {
            if (Create)
            {
                Buffer = new byte[68];
                Writer.WriteUInt16(60, 0, Buffer);
                Writer.WriteUInt16(1150, 2, Buffer);
            }
        }

        public void Deserialize(byte[] buffer)
        {
            this.Buffer = buffer;
        }

        public void Send(Client.GameState Client)
        {
            Client.Send(Buffer);
        }

        public byte[] ToArray()
        {
            return this.Buffer;
        }
        public uint type
        {
            get
            {
                return BitConverter.ToUInt32(Buffer, 4);
            }
            set
            {
                Writer.WriteUInt32(value, 4, Buffer);
            }
        }
        public uint f
        {
            get
            {
                return BitConverter.ToUInt32(Buffer, 16);
            }
            set
            {
                Writer.WriteUInt32(value, 16, Buffer);
            }
        }
        public uint Amount
        {
            get
            {
                return BitConverter.ToUInt32(Buffer, 20);
            }
            set
            {
                Writer.WriteUInt32(value, 20, Buffer);
            }
        }

        public MTA.Game.Features.Flowers.FlowerType FlowerType
        {
            get
            {
                return (MTA.Game.Features.Flowers.FlowerType)BitConverter.ToUInt32(Buffer, 0x18);
            }
        }

        public uint ItemUID
        {
            get
            {
                return BitConverter.ToUInt32(Buffer, 12);
            }
            set
            {
                Writer.WriteUInt32(value, 12, Buffer);
            }
        }

        public string ReceiverName
        {
            get
            {
                return BitConverter.ToString(Buffer, 32, 16);
            }
            set
            {
                Writer.WriteString(value, 32, Buffer);
            }
        }

        public uint SendAmount
        {
            get
            {
                return BitConverter.ToUInt32(Buffer, 48);
            }
            set
            {
                Writer.WriteUInt32(value, 48, Buffer);
            }
        }
        public uint remove
        {
            get { return BitConverter.ToUInt32(Buffer, 56); }
            set { WriteUInt32(value, 56, Buffer); }
        }
        public string SenderName
        {
            get
            {
                return BitConverter.ToString(Buffer, 16, 16);
            }
            set
            {
                Writer.WriteString(value, 16, Buffer);
            }
        }

        public MTA.Game.Features.Flowers.FlowerType SendFlowerType
        {
            get
            {
                return (MTA.Game.Features.Flowers.FlowerType)BitConverter.ToUInt32(Buffer, 0x34);
            }
            set
            {
                Writer.WriteUInt32((uint)value, 52, Buffer);
            }
        }

        public uint UID1
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32(value, 8, Buffer); }
        }

        public uint UID2
        {
            get { return BitConverter.ToUInt32(Buffer, 10); }
            set { WriteUInt32(value, 10, Buffer); }
        }
    }

}