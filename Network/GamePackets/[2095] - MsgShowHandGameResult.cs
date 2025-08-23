using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class MsgShowHandGameResult : Writer, Interfaces.IPacket
    {
        private byte[] Buffer;
        public MsgShowHandGameResult(int count)
        {
            Buffer = new byte[8 + 11 + (count * 15)];
            Write((ushort)(Buffer.Length - 8), 0, Buffer);
            Write(2095, 2, Buffer);
            PlayersCount = (ushort)count;
        }
        public ushort Timer
        {
            get { return BitConverter.ToUInt16(Buffer, 4); }
            set { Write(value, 4, Buffer); }
        }
        public ushort PlayersCount
        {
            get { return BitConverter.ToUInt16(Buffer, 6); }
            set { Write(value, 6, Buffer); }
        }
        int offset = 8;
        public void Append(Game.ConquerStructures.PokerTable T, uint WinnerId, ulong MoneyWins)
        {
            Buffer[offset] = 0;
            offset++;
            Buffer[offset] = 2;
            offset++;
            Buffer[offset] = 0;
            offset++;

            Write(WinnerId, offset, Buffer);
            offset += 4;
            Write(MoneyWins, offset, Buffer);
            offset += 8;

            foreach (var Pl in T.Players.Values.ToList())
            {
                if (Pl.UID == WinnerId) continue;
                byte ContinuePlaying = 0;
                if (T.BetType == Game.Enums.PokerBetType.Money)
                    if (Pl.Entity.Money >= T.MinLimit * 10)
                        ContinuePlaying = 0;
                    else ContinuePlaying = 1;
                else if (T.BetType == Game.Enums.PokerBetType.ConquerPoints)
                    if (Pl.Entity.ConquerPoints >= T.MinLimit * 10)
                        ContinuePlaying = 0;
                    else ContinuePlaying = 1;
                if (ContinuePlaying != 0)
                    Pl.CurrentState = 2;
                Buffer[offset] = (byte)(ContinuePlaying == 0 ? 0 : 1);
                offset++;
                Buffer[offset] = Pl.CurrentState;
                offset++;
                Buffer[offset] = 0;
                offset++;

                Write(Pl.UID, offset, Buffer);
                offset += 4;
                Write((ulong)(0 - Pl.TotalBet), offset, Buffer);
                offset += 8;
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
