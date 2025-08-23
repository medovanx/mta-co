using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Game.Features;
using MTA.Network;

namespace MTA.Network.GamePackets
{
    public class SendFlower : Writer
    {
        public const uint FlowerSender = 2;
        public const uint Flower = 3;
        private byte[] Packet;
        public uint Typing
        {
            get
            {
                return BitConverter.ToUInt32(this.Packet, 4);
            }
            set
            {
                Writer.WriteUInt32(value, 4, this.Packet);
            }
        }
        public string SenderName
        {
            get
            {
                return Encoding.ASCII.GetString(this.Packet, 16, 16);
            }
            set
            {
                Writer.WriteString(value, 16, this.Packet);
            }
        }
        public string ReceiverName
        {
            get
            {
                return Encoding.ASCII.GetString(this.Packet, 32, 16);
            }
            set
            {
                Writer.WriteString(value, 32, this.Packet);
            }
        }
        public uint Amount
        {
            get
            {
                return BitConverter.ToUInt32(this.Packet, 48);
            }
            set
            {
                Writer.WriteUInt32(value, 48, this.Packet);
            }
        }
        public uint FType
        {
            get
            {
                return BitConverter.ToUInt32(this.Packet, 52);
            }
            set
            {
                Writer.WriteUInt32(value, 52, this.Packet);
            }
        }
        public uint Effect
        {
            get
            {
                return BitConverter.ToUInt32(this.Packet, 56);
            }
            set
            {
                Writer.WriteUInt32(value, 56, this.Packet);
            }
        }
        public byte[] ToArray()
        {
            return this.Packet;
        }
        public SendFlower()
        {
            this.Packet = new byte[68];
            Writer.WriteUInt16(60, 0, this.Packet);
            Writer.WriteUInt16(1150, 2, this.Packet);
        }
        public void Apprend(Flowers flowers)
        {
            Writer.WriteUInt32(flowers.RedRoses, 16, this.Packet);
            Writer.WriteUInt32(flowers.RedRoses2day, 20, this.Packet);
            Writer.WriteUInt32(flowers.Lilies, 24, this.Packet);
            Writer.WriteUInt32(flowers.Lilies2day, 28, this.Packet);
            Writer.WriteUInt32(flowers.Orchads, 32, this.Packet);
            Writer.WriteUInt32(flowers.Orchads2day, 36, this.Packet);
            Writer.WriteUInt32(flowers.Tulips, 40, this.Packet);
            Writer.WriteUInt32(flowers.Tulips2day, 44, this.Packet);
        }
    }
}
