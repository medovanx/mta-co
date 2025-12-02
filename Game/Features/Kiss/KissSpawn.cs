using System;
using System.IO;
using System.Text;
using MTA.Game.ConquerStructures.Society;
using MTA.Network;

namespace MTA.Network.GamePackets
{
    public class KissSpawn : Writer
    {
        byte[] Buffer;
        public KissSpawn(string Type, string name, string Kisses, string UID, uint KissID)
        {
            string send = Type + " " + Kisses + " " + UID + " " + UID + " " + name + " " + name + "";
            Buffer
                = new byte[88];//18
            WriteUInt16((byte)(80), 0, Buffer);
            WriteUInt16(1151, 2, Buffer);

            Buffer[4] = 2;
            WriteUInt32(KissID, 8, Buffer);
            Buffer[16] = 1;
            Buffer[32] = 2;

            WriteUInt32(uint.Parse(UID), 40, Buffer);
            WriteUInt32(uint.Parse(UID), 44, Buffer);
            for (int i = 0; i < send.Length; i++)
            {
                try
                {
                    Buffer[48 + i] = Convert.ToByte(send[i]);
                    Buffer[48 + i + 16] = Convert.ToByte(send[i]);

                }
                catch { }
            }
        }
        public byte[] ThePacket()
        {
            return Buffer;
        }
    }
}