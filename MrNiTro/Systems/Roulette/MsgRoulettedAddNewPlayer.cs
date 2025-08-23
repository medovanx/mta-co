using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Network;

namespace MTA.MaTrix.Roulette
{
    public class MsgRoulettedAddNewPlayer : Interfaces.IPacket
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
        public uint UID
        {
            set
            {
                Writer.WriteUint(value, 4, packet);
            }
        }
        public uint Mesh
        {
            set
            {
                Writer.WriteUint(value, 8, packet);
            }
        }
        public MsgRouletteOpenGui.Color Color
        {
            set
            {
                Writer.Byte((byte)value, 12, packet);
            }
        }
        public string Name
        {
            set
            {
                Writer.WriteString(value, 13, packet);
            }
        }
        public static MsgRoulettedAddNewPlayer Create()
        {
            MsgRoulettedAddNewPlayer ptr = new MsgRoulettedAddNewPlayer();
            ptr.Length = 29;
            ptr.PacketID = GamePackets.MsgRoulettedAddNewPlayer;           
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
