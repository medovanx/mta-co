using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Network;

namespace MTA.MaTrix.Roulette
{
    public class MsgRouletteNoWinner : Interfaces.IPacket
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
        public byte Number
        {
            set
            {
                Writer.Byte(value, 4, packet);
            }
        }


        public static MsgRouletteNoWinner Create()
        {
            MsgRouletteNoWinner packet = new MsgRouletteNoWinner();
            packet.Length = 5;
            packet.PacketID = GamePackets.MsgRouletteNoWinner;
            return packet;
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
