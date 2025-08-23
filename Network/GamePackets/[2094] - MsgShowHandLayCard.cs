using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class MsgShowHandLayCard : Writer, Interfaces.IPacket
    {
        private byte[] Buffer;
        public MsgShowHandLayCard(int count)
        {
            Buffer = new byte[8 + 8 + (count * 12)];
            Write((ushort)(Buffer.Length - 8), 0, Buffer);
            Write(2094, 2, Buffer);
            Count = (ushort)count;
        }
        public ushort Count
        {
            get { return BitConverter.ToUInt16(Buffer, 6); }
            set { Write(value, 6, Buffer); }
        }
        int offset = 8;
        public void Append(Game.ConquerStructures.PokerTable T)
        {
            foreach (Game.ConquerStructures.PokerPlayer Pl in T.Players.Values)
            {
                var cards = Pl.Cards.ToArray();
                for (int i = 0; i < cards.Length; i++)
                {
                    var card = cards[i];
                    Write((ushort)card.Value, offset, Buffer);
                    Write((ushort)card.Type, offset + 4, Buffer);
                    offset += 2;
                }
                offset += 4;
                Write(Pl.UID, offset, Buffer);
                offset += 4;
            }
        }
        public byte[] ToArray()
        {
            return Buffer;
        }
        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }
        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }
    }
}