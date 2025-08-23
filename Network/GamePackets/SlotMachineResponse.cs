using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class SlotMachineResponse : Writer, Interfaces.IPacket
    {

        byte[] Buffer;

        public SlotMachineResponse(bool Create = false)
        {
            Buffer = new byte[24];
            WriteUInt16(16, 0, Buffer);
            WriteUInt16(1352, 2, Buffer);
        }

        public Game.SlotMachineSubType Mode
        {
            get { return (Game.SlotMachineSubType)Buffer[4]; }
            set { Buffer[4] = (byte)value; }
        }
        public byte WheelOne
        {
            get { return Buffer[5]; }
            set { Buffer[5] = value; }
        }
        public byte WheelTwo
        {
            get { return Buffer[6]; }
            set { Buffer[6] = value; }
        }
        public byte WheelThree
        {
            get { return Buffer[7]; }
            set { Buffer[7] = value; }
        }
        public uint NpcID
        {
            get { return BitConverter.ToUInt32(Buffer, 16); }
            set { WriteUInt32(value, 16, Buffer); }
        }  

        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }

        public byte[] ToArray()
        {
            return Buffer;
        }

        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }
    }
}
