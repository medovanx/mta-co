using MTA.Client;
using MTA.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class RacePotion : Writer, Interfaces.IPacket
    {
        private byte[] buffer;
        public RacePotion(bool create)
        {
            if (create)
            {
                buffer = new byte[24];
                WriteUInt16(16, 0, buffer);
                WriteUInt16(2072, 2, buffer);
            }
        }

        public ushort Amount
        {
            get { return BitConverter.ToUInt16(buffer, 4); }
            set { WriteUInt16(value, 4, buffer); }
        }

        public Enums.RaceItemType PotionType
        {
            get { return (Enums.RaceItemType)BitConverter.ToUInt16(buffer, 6); }
            set { WriteUInt16((ushort)value, 6, buffer); }
        }

        public int Location
        {
            get { return BitConverter.ToInt32(buffer, 8); }
            set { WriteInt32(value, 8, buffer); }
        }
        
        public uint dwParam
        {
            get { return BitConverter.ToUInt32(buffer, 12); }
            set { WriteUInt32(value, 12, buffer); }
        }
        
        public void Deserialize(byte[] buffer)
        {
            this.buffer = buffer;
        }

        public void Send(GameState client)
        {
            client.Send(buffer);
        }

        public byte[] ToArray()
        {
            return buffer;
        }
    }
}
