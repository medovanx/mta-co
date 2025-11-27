using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class MsgShowHandDealtCard : Writer, Interfaces.IPacket
    {
        private byte[] Buffer;
        public MsgShowHandDealtCard(int count)
        {
            Buffer = new byte[44 + 8 + (count * 8)];
            Write((ushort)(Buffer.Length - 8), 0, Buffer);
            Write(2091, 2, Buffer);
            PlayersCount = (ushort)count;
        }
        public ushort Type
        {
            get { return BitConverter.ToUInt16(Buffer, 4); }
            set { Write(value, 4, Buffer); }
        }
        public ushort RoundStage
        {
            get { return BitConverter.ToUInt16(Buffer, 6); }
            set { Write(value, 6, Buffer); }
        }
        public ushort CardsCount
        {
            get { return BitConverter.ToUInt16(Buffer, 8); }
            set { Write(value, 8, Buffer); }
        }
        public ushort PlayersCount
        {
            get { return BitConverter.ToUInt16(Buffer, 30); }
            set { Write(value, 30, Buffer); }
        }
        public uint Delear
        {
            get { return BitConverter.ToUInt32(Buffer, 32); }
            set { Write(value, 32, Buffer); }
        }
        int offset = 10;
        int offset3 = 44;
        public void Append(Game.ConquerStructures.PokerTable T, int count = 10)
        {
            foreach (Game.ConquerStructures.PokerPlayer Pl in T.Players.Values)
            {
                var cards = Pl.Cards.ToArray();
                count = Math.Min(cards.Length, count);
                for (int i = 0; i < count; i++)
                {
                    var _PokerCard = cards[i];

                    Write((ushort)_PokerCard.Value, offset3, Buffer);
                    offset3 += 2;
                    Write((ushort)_PokerCard.Type, offset3, Buffer);
                    offset3 += 2;
                }
                Write(Pl.UID, offset3, Buffer);
                offset3 += 4;
            }
        }
        public void Append(Game.ConquerStructures.PokerPlayer Pl)
        {
            Write(13, offset, Buffer);
            offset += 2;
            Write(4, offset, Buffer);
            offset += 2;
            Write(Pl.UID, offset, Buffer);
            offset += 4;

        }
        public void Append(uint StartingPlayer, uint LastPlayer, uint NextPlayer, params Game.ConquerStructures.PokerCard[] _Cards)
        {
            if (_Cards != null)
            {
                var Cards = _Cards.ToArray();
                for (int i = 0; i < 5; i++)
                {
                    Game.ConquerStructures.PokerCard _PokerCard = null;
                    if (i < Cards.Length)
                        _PokerCard = Cards[i];

                    Append(_PokerCard);
                    offset += 2;
                }
            }
            else
                offset += 10;
            offset += 10;
            offset += 2;
            Write(StartingPlayer, offset, Buffer);
            offset += 4;
            Write(LastPlayer, offset, Buffer);
            offset += 4;
            Write(NextPlayer, offset, Buffer);
            offset += 4;
        }
        public void Append(Game.ConquerStructures.PokerCard _PokerCard)
        {
            if (_PokerCard != null)
            {
                Write((ushort)_PokerCard.Value, offset, Buffer);
                Write((ushort)_PokerCard.Type, offset + 10, Buffer);
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
