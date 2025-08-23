using System;
using System.Collections.Generic;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class MsgTexasInteractive : Writer, Interfaces.IPacket
    {
        byte[] Buffer;

        public MsgTexasInteractive(bool Create, byte count = 0)
        {
            if (Create)
            {
                if (count == 0)
                    Buffer = new byte[20 + 8];
                else Buffer = new byte[52 + (count * 6)];
                Writer.Write(Buffer.Length - 8, 0, Buffer);
                Writer.Write(2171, 2, Buffer);
            }
        }
        public byte Type
        {
            get { return Buffer[4]; }
            set { Buffer[4] = value; }
        }
        public uint Req
        {
            get { return BitConverter.ToUInt32(Buffer, 4); }
            set { Write(value, 4, Buffer); }
        }
        public byte Type2
        {
            get { return Buffer[10]; }
            set { Buffer[10] = value; }
        }
        public byte Seat
        {
            get { return Buffer[16]; }
            set { Write(value, 16, Buffer); }
        }
        public uint TableNumber
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { Write(value, 8, Buffer); }
        }
        public uint PlayerUID
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
            MsgTexasInteractive msg = new MsgTexasInteractive(true);
            msg.Deserialize(packet);
            switch (msg.Type)
            {
                case 0://Join Table
                    {
                        if (Database.PokerTables.Tables.ContainsKey(msg.TableNumber))
                        {
                            var T = Database.PokerTables.Tables[msg.TableNumber];
                            if (T.Players.ContainsKey(client.Entity.UID)) T.RemovePlayer(client.Entity.UID);
                            T.SitIn(client, (byte)msg.Seat);
                            client.Send(msg.ToArray());
                            msg.Type2 = (byte)(T.ShowHand ? 2 : 1);
                            T.UpdateSeats(client, (byte)msg.Seat);
                            if (T.m_State == Game.Enums.GameClientEnum.WaitForPlayers)
                                T.TryToBegin();
                        }
                        break;
                    }
                case 1:
                    {
                        if (Database.PokerTables.Tables.ContainsKey(msg.TableNumber))
                        {
                            var T = Database.PokerTables.Tables[msg.TableNumber];
                            if (T.Players.ContainsKey(client.Entity.UID)) T.RemovePlayer(client.Entity.UID);
                            T.SitIn(client, (byte)msg.Seat);
                            client.Send(msg.ToArray());
                            msg.Type2 = (byte)(T.ShowHand ? 2 : 1);
                            T.UpdateSeats(client, (byte)msg.Seat);
                        }
                        break;
                    }
                case 4://Watch
                    {
                        if (Database.PokerTables.Tables.ContainsKey(msg.TableNumber))
                        {
                            var T = Database.PokerTables.Tables[msg.TableNumber];
                            T.SitIn(client, (byte)msg.Seat, false);
                            if (T.Watchers.ContainsKey(client.Entity.UID))
                                T.Watchers[client.Entity.UID].CurrentState = 2;
                            client.Send(msg.ToArray());
                            T.UpdateSeats(client, (byte)msg.Seat);
                            if (T.m_State == Game.Enums.GameClientEnum.WaitForPlayers)
                                T.TryToBegin();
                        }
                        break;
                    }
            }
        }
    }
}