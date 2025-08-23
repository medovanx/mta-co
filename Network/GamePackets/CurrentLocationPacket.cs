using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class CurrentLocationPacket : Writer, Interfaces.IPacket
    {
        byte[] buffer;

        public CurrentLocationPacket(byte[] buffer)
        {
            this.buffer = buffer;
        }
        public CurrentLocationPacket(uint heroID, MTA.Game.Enums.CountryID countryID)
        {
            buffer = new byte[18];
            WriteUInt16(10, 0, buffer);
            WriteUInt16(0x97E, 2, buffer);
            WriteUInt32(heroID, 4, buffer);
            WriteUInt16((ushort)countryID, 8, buffer);
        }

        public uint HeroID
        {
            get
            {
                return BitConverter.ToUInt32(buffer, 4);
            }
            set
            {
                WriteUInt32(value, 4, buffer);
            }
        }

        public MTA.Game.Enums.CountryID CountryID
        {
            get
            {
                return (MTA.Game.Enums.CountryID)BitConverter.ToUInt32(buffer, 8);
            }
            set
            {
                WriteUInt16((ushort)value, 8, buffer);
            }
        }

        public byte[] ToArray()
        {
            return buffer;
        }

        public void Deserialize(byte[] buffer)
        {
            this.buffer = buffer;
        }

        public void Send(Client.GameState client)
        {
            client.Send(buffer);
        }
    }
}
