using System;
using System.Text;
namespace MTA.Network.AuthPackets
{
    public class Forward : Interfaces.IPacket
    {
        public enum ForwardType : uint { Ready = 2,Ready2 = 3, InvalidInfo = 1, Banned = 25 }
        byte[] Buffer;
        public Forward()
        {
            Buffer = new byte[52];
            Network.Writer.WriteUInt16(52, 0, Buffer);
            Network.Writer.WriteUInt16(1055, 2, Buffer);
        }
        public uint Identifier
        {
            get
            {             
                return BitConverter.ToUInt32(Buffer, 4);
            }
            set
            {
                Network.Writer.WriteUInt32(value, 4, Buffer);
            }
        }
        public ForwardType Type
        {
            get
            {
                return (ForwardType)(uint)BitConverter.ToUInt32(Buffer, 8);
            }
            set
            {
                Network.Writer.WriteUInt32((uint)value, 8, Buffer);
            }
        }
        public string IP
        {
            get
            {
                return Program.Encoding.GetString(Buffer, 20, 16);
            }
            set
            {
                Network.Writer.WriteString(value, 20, Buffer);
            }
        }
        public ushort Port
        {
            get
            {
                return BitConverter.ToUInt16(Buffer, 12);
            }
            set
            {
                Network.Writer.WriteUInt16(value, 12, Buffer);
            }
        }
        public void Deserialize(byte[] buffer)
        {
            //no implementation
        }
        public byte[] ToArray()
        {
            return Buffer;
        }
        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }
    }
}