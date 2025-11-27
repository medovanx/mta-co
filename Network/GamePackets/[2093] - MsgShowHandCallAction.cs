using System;
using System.Collections.Generic;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class MsgShowHandCallAction : Writer, Interfaces.IPacket
    {
        byte[] Buffer;

        public MsgShowHandCallAction(bool Create)
        {
            if (Create)
            {
                Buffer = new byte[28 + 8];
                Writer.Write(Buffer.Length - 8, 0, Buffer);
                Writer.Write(2093, 2, Buffer);
            }
        }
        public Game.Enums.PokerCallTypes Type
        {
            get { return (Game.Enums.PokerCallTypes)BitConverter.ToUInt16(Buffer, 6); }
            set { Write((ushort)value, 6, Buffer); }
        }
        public ulong Bet
        {
            get { return BitConverter.ToUInt64(Buffer, 8); }
            set { Write(value, 8, Buffer); }
        }
        public ulong LastBet
        {
            get { return BitConverter.ToUInt64(Buffer, 16); }
            set { Write(value, 16, Buffer); }
        }
        public uint UID
        {
            get { return BitConverter.ToUInt32(Buffer, 24); }
            set { Write(value, 24, Buffer); }
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
        public static void Handle(Client.GameState client, byte[] packet)
        {
            MsgShowHandCallAction msg = new MsgShowHandCallAction(true);
            msg.Deserialize(packet);
            switch (msg.Type)
            {
                default:
                    {
                        var T = client.Entity.PokerTable;
                        if (T != null)
                            T.PlayerMove(msg, client.Entity.UID);
                        break;
                    }
            }
        }
    }
}
