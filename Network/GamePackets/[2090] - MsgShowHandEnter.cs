using System;
using System.Collections.Generic;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class MsgShowHandEnter : Writer, Interfaces.IPacket
    {
        byte[] Buffer;

        public MsgShowHandEnter(bool Create)
        {
            if (Create)
            {
                Buffer = new byte[29 + 8];
                Writer.Write(Buffer.Length - 8, 0, Buffer);
                Writer.Write(2090, 2, Buffer);
            }
        }
        public byte Type
        {
            get { return Buffer[4]; }
            set { Buffer[4] = value; }
        }
        public byte State
        {
            get { return Buffer[5]; }
            set { Buffer[5] = value; }
        }
        public byte Seat
        {
            get { return Buffer[7]; }
            set { Buffer[7] = value; }
        }
        public byte TableNumber
        {
            get { return Buffer[9]; }
            set { Buffer[9] = value; }
        }
        public byte TableType2
        {
            get { return Buffer[25]; }
            set { Buffer[25] = value; }
        }
        public uint UID
        {
            get { return BitConverter.ToUInt32(Buffer, 13); }
            set { Write(value, 13, Buffer); }
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
            MsgShowHandEnter msg = new MsgShowHandEnter(true);
            msg.Deserialize(packet);
            switch (msg.Type)
            {
                case 0:
                case 1:
                    {
                        var TableId = client.Entity.PokerTableUID;
                        var NoSeat = msg.Seat;

                        if (Database.PokerTables.Tables.ContainsKey(TableId))
                        {
                            var T = Database.PokerTables.Tables[TableId];
                            msg.TableType2 = (byte)(T.ShowHand ? 2 : 1);
                            T.SitIn(client, (byte)NoSeat);
                            if (T.Players.ContainsKey(client.Entity.UID))
                                if (T.Watchers.ContainsKey(client.Entity.UID))
                                    T.Watchers.Remove(client.Entity.UID);
                            T.UpdateSeats(client, (byte)NoSeat);
                            if (T.m_State == Game.Enums.GameClientEnum.WaitForPlayers)
                                T.TryToBegin();
                        }
                        break;
                    }
            }
        }
    }
}