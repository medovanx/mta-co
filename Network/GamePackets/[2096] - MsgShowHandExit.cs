using System;
using System.Collections.Generic;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class MsgShowHandExit : Network.Writer
    {
        byte[] Buffer;

        public MsgShowHandExit(bool Create)
        {
            if (Create)
            {
                Buffer = new byte[16 + 8];
                Writer.Write(Buffer.Length - 8, 0, Buffer);
                Writer.Write(2096, 2, Buffer);
            }
        }
        public uint Type
        {
            get { return BitConverter.ToUInt32(Buffer, 4); }
            set { Write(value, 4, Buffer); }
        }
        public uint Type2
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { Write(value, 8, Buffer); }
        }
        public uint UID
        {
            get { return BitConverter.ToUInt32(Buffer, 12); }
            set { Write(value, 12, Buffer); }
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
            var T = client.Entity.PokerTable;
            if (T == null) return;

            if (T.Players.ContainsKey(client.Entity.UID) && T.Pot > 1)
                T.PlayerMove(new MsgShowHandCallAction(true) { Type = Game.Enums.PokerCallTypes.Fold }, client.Entity.UID);
            T.RemovePlayer(client.Entity.UID);
            client.Send(packet);
        }
    }
}
