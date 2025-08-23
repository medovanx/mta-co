using System;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class NpcRequest : Writer, Interfaces.IPacket
    {
        private byte[] Buffer;

        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }
        public NpcRequest(ushort mode = 0)
        {
            if (mode != 0)
            {
                Buffer = new byte[28];
                WriteUInt16((ushort)20, 0, Buffer);
                WriteUInt16(2031, 2, Buffer);
                WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, Buffer);
                Mode = mode;// for apprend mouse npc
            }
        }
        public ushort Mesh { get { return BitConverter.ToUInt16(Buffer, 12); } set { WriteUInt16(value, 12, Buffer); } }
        public ushort Mode { get { return BitConverter.ToUInt16(Buffer, 16); } set { WriteUInt16(value, 16, Buffer); } }
        public Game.Enums.NpcType NpcTyp { get { return (Game.Enums.NpcType)BitConverter.ToUInt16(Buffer, 18); } set { WriteUInt16((ushort)value, 18, Buffer); } }
      
        public byte[] ToArray()
        {
            return Buffer;
        }

        public uint NpcID
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32(value, 8, Buffer); }
        }

        
        public byte OptionID
        {
            get { return Buffer[14]; }
            set { Buffer[14] = value; }
        }

        public byte InteractType
        {
            get { return Buffer[15]; }
        }

        public string Input
        {
            get { return Program.Encoding.GetString(Buffer, 18, Buffer[17]); }
        }

        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }
    }
}
