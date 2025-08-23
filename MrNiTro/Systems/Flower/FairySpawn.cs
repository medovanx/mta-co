using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class FairySpawn : Writer, Interfaces.IPacket
    {
        private byte[] buf;
        public FairySpawn(bool create)
        {

            if (create)
            {
                buf = new byte[28];
                WriteUInt16(20, 0, buf);
                WriteUInt16(2070, 2, buf);
            }
        }
        public uint SType
        {
            get
            {
                return BitConverter.ToUInt32(buf, 4);
            }
            set
            {
                WriteUInt32(value, 4, buf);
            }
        }
        public uint Unknown
        {
            get
            {
                return BitConverter.ToUInt32(buf, 8);
            }
            set
            {
                WriteUInt32(value, 8, buf);
            }
        }
        public uint FairyType
        {
            get
            {
                return BitConverter.ToUInt32(buf, 12);
            }
            set
            {
                WriteUInt32(value, 12, buf);
            }
        }
        public uint UID
        {
            get
            {
                return BitConverter.ToUInt32(buf, 16);
            }
            set
            {
                WriteUInt32(value, 16, buf);
            }
        }
        public void Send(Client.GameState client)
        {
            client.Send(buf);
        }

        public byte[] ToArray()
        {
            return buf;
        }

        public void Deserialize(byte[] buffer)
        {
            buf = buffer;
        }
    }
}
