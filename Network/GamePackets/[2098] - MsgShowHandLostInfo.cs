using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class MsgShowHandLostInfo : Writer, Interfaces.IPacket
    {
        private byte[] Buffer;
        public MsgShowHandLostInfo(int count)
        {
            Buffer = new byte[45 + 8 + (count * 11)];
            Write((ushort)(Buffer.Length - 8), 0, Buffer);
            Write(2098, 2, Buffer);
        }
        public byte Type
        {
            get { return Buffer[4]; }
            set { Buffer[4] = value; }
        }
        public byte PlayersCount
        {
            get { return Buffer[6]; }
            set { Buffer[6] = value; }
        }
        public byte CountDown
        {
            get { return Buffer[7]; }
            set { Buffer[7] = value; }
        }
        int offset = 19;
        public void Append(Game.ConquerStructures.PokerTable T)
        {
            var players = T.Players.Values.ToArray();
            for (int i = 0; i < 6; i++)
            {
                if (i < players.Length)
                {
                    var Pl = players[i];
                    Write(Pl.UID, offset, Buffer);
                }
                offset += 4;
            }

            offset += 2;

            foreach (Game.ConquerStructures.PokerPlayer Pl in T.Players.Values)
            {
                Buffer[offset++] = 2;
                Buffer[offset++] = 4;
                Buffer[offset++] = 13;
                Buffer[offset++] = 0;
                Buffer[offset++] = 4;
                Buffer[offset++] = 13;
                Buffer[offset++] = 0;

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