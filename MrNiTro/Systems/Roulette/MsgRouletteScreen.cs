using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using MTA.Network;

namespace MTA.MaTrix.Roulette
{

    public class MsgRouletteScreen : Interfaces.IPacket
    {
       
        public ushort Length
        {
            set
            {
                packet = new byte[value + 8];
                Writer.WriteUshort((ushort)(packet.Length - 8), 0, packet);

            }
        }
        
        public ushort PacketID
        {
            set
            {
                Writer.WriteUshort(value, 2, packet);
            }
        }      
        
        public byte UnKnow
        {
            set
            {
                Writer.Byte((byte)value, 4, packet);
            }
        }
        
        public uint UID
        {
            set
            {
                Writer.WriteUint((uint)value, 5, packet);
            }
        }  
      
        public static unsafe MsgRouletteScreen Create()
        {
            MsgRouletteScreen ptr = new MsgRouletteScreen();
            ptr.Length = 9;
            ptr.UnKnow = 1;
            ptr.PacketID = GamePackets.MsgRouletteScreen;           
            return ptr;
        }

        public byte[] packet;
        public byte[] ToArray()
        {
            return packet;
        }
        public void Send(Client.GameState client)
        {
            client.Send(ToArray());
        }



        public void Deserialize(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}
