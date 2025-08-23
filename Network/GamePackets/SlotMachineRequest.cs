using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class SlotMachineRequest : Writer, Interfaces.IPacket
    {
        byte[] Buffer;

        public SlotMachineRequest(bool Create = false)
        {
        }

        public Game.SlotMachineSubType Mode
        {
            get { return (Game.SlotMachineSubType)Buffer[4]; }
            set { Buffer[4] = (byte)value; }
        }
        public byte BetMultiplier
        {
            get { return Buffer[5]; }
            set { Buffer[5] = value; }
        }

        public uint NpcID
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32(value, 8, Buffer); }
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
