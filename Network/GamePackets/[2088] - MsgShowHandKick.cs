using System;
using System.Collections.Generic;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class MsgShowHandKick : Writer, Interfaces.IPacket
    {
        byte[] Buffer;

        public MsgShowHandKick(bool Create)
        {
            if (Create)
            {
                Buffer = new byte[28 + 8];
                Writer.Write(Buffer.Length - 8, 0, Buffer);
                Writer.Write(2088, 2, Buffer);
                Buffer[4] = 1;
            }
        }
        public byte Type
        {
            get { return Buffer[4]; }
            set { Buffer[4] = value; }
        }
        public uint PlayerUID2
        {
            get { return BitConverter.ToUInt32(Buffer, 5); }
            set { Write(value, 5, Buffer); }
        }
        public uint PlayerUID
        {
            get { return BitConverter.ToUInt32(Buffer, 9); }
            set { Write(value, 9, Buffer); }
        }
        public uint PlayersCount
        {
            get { return BitConverter.ToUInt32(Buffer, 13); }
            set { Write(value, 13, Buffer); }
        }
        public byte AgreedVoices
        {
            get { return Buffer[17]; }
            set { Buffer[17] = value; }
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
            MsgShowHandKick msg = new MsgShowHandKick(true);
            msg.Deserialize(packet);
            var TableId = client.Entity.PokerTableUID;
            if (Database.PokerTables.Tables.ContainsKey(TableId))
            {
                var T = Database.PokerTables.Tables[TableId];
                if (T.Players.ContainsKey(msg.PlayerUID))
                {
                    var msg2 = new MsgShowHandKick(true) { PlayerUID2 = client.Entity.UID, PlayerUID = msg.PlayerUID, PlayersCount = (ushort)T.Players.Count };
                    foreach (var pl in T.Players.Values)
                    {
                        if (pl.UID == client.Entity.UID)
                            continue;
                        pl.Send(packet);
                        if (msg.AgreedVoices > T.Players.Count / 2)
                        {
                            T.RemovePlayer(pl.UID);
                        }
                    }
                }
            }
        }
    }
}
