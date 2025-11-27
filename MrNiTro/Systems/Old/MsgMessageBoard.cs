// * Created by CptSky
// * Copyright © 2011
// * COPS v6 Emulator - Project

using System;
using System.Runtime.InteropServices;
using System.Text;
using MTA;
using MTA.Network.GamePackets;
using MTA.Network;
using MTA.Client;

namespace MTA.Network
{
    public unsafe class MsgMessageBoard
    {
        public const Int16 Id = 1111;
        public partial class Msg
        {
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct MsgHeader
            {
                public Int16 Length;
                public Int16 Type;
            }
        }
        public enum Action
        {
            None = 0,
            Del = 1,			        // to server					// no return
            GetList = 2,		    	// to server: index(first index)
            List = 3,		        	// to client: index(first index), name, words, time...
            GetWords = 4,	    		// to server: index(for get)	// return by MsgTalk
        };

        public enum Channel
        {
            None = 0,
            MsgTrade = 2201,
            MsgFriend = 2202,
            MsgTeam = 2203,
            MsgSyn = 2204,
            MsgOther = 2205,
            MsgSystem = 2206,
        };

        public struct MsgInfo
        {
            public MsgMessageBoard.Msg.MsgHeader Header;
            public UInt16 Index;
            public UInt16 Channel;
            public Byte Action;
            public String Param;
        };

        /*  public static Byte[] Create(UInt16 Index, uint Channel, String[] Params, Action Action)
        {
            try
            {
                Int32 StrLength = 0;
                if (Params != null)
                {
                    for (Int32 i = 0; i < Params.Length; i++)
                    {
                        if (Params[i] == null || Params[i].Length > 255)
                            return null;

                        StrLength += Params[i].Length + 1;
                    }
                }

                Byte[] Out = new Byte[14 + StrLength + 8];
                fixed (Byte* p = Out)
                {
                    *((Int16*)(p + 0)) = (Int16)(Out.Length - 8);
                    *((Int16*)(p + 2)) = (Int16)Id;
                    *((UInt16*)(p + 8)) = (UInt16)Index; 
                    *((UInt16*)(p + 10)) = (UInt16)Channel;
                    *((Byte*)(p + 12)) = (Byte)Action;

                    if (Params != null)
                    {
                        *((Byte*)(p + 13)) = (Byte)Params.Length;

                        Int32 Pos = 14;
                        for (Int32 x = 0; x < Params.Length; x++)
                        {
                            *((Byte*)(p + Pos)) = (Byte)Params[x].Length;
                            for (Byte i = 0; i < (Byte)Params[x].Length; i++)
                                *((Byte*)(p + Pos + 1 + i)) = (Byte)Params[x][i];
                            Pos += Params[x].Length + 1;
                        }
                    }
                }
                return Out;
            }
            catch (Exception Exc) { MTA.Console.WriteLine(Exc); return null; }
        }*/
        public static void Create(GameState client, UInt16 Index, ushort Channel, String[] Params, Action Action)
        {
            try
            {
                int StrLength = 0;
                if (Params != null)
                {
                    for (int i = 0; i < Params.Length; i++)
                    {
                        if (Params[i] == null || Params[i].Length > 255)
                            return;

                        StrLength += Params[i].Length + 1;
                    }
                }

                byte[] Out = new byte[14 + StrLength + 8];
                Writer.WriteUInt16((ushort)(Out.Length - 8), 0, Out);
                Writer.WriteUInt16(1111, 2, Out);
                Writer.WriteUInt16(Index, 8, Out);
                Writer.WriteUInt16(Channel, 10, Out);
                Writer.WriteByte((byte)Action, 12, Out);
                if (Params != null)
                {
                    int Pos = 14;
                    Writer.WriteByte((byte)Params.Length, 13, Out);
                    for (int x = 0; x < Params.Length; x++)
                    {
                        Writer.WriteByte((byte)Params[x].Length, Pos, Out);
                        for (byte i = 0; i < (byte)Params[x].Length; i++)
                            Writer.WriteByte((byte)Params[x][i], (Pos + 1 + i), Out);
                        Pos += Params[x].Length + 1;
                    }
                }
                client.Send(Out);
                return;

            }
            catch (Exception Exc) { MTA.Console.WriteLine(Exc); return; }
        }
        public static Encoding Encoding = Encoding.GetEncoding("iso-8859-1");
        public static void Process(MTA.Client.GameState Client, Byte[] Buffer)
        {
            try
            {
                ushort MsgLength = MTA.BitConverter.ToUInt16(Buffer, 0);
                ushort MsgId = MTA.BitConverter.ToUInt16(Buffer, 2);
                ushort Index = MTA.BitConverter.ToUInt16(Buffer, 8);
                ushort Channel = MTA.BitConverter.ToUInt16(Buffer, 10);
                Action Action = (Action)Buffer[12];
                string Param = null;
                Param = Program.Encoding.GetString(Buffer, 15, Buffer[14]);
                //if (Buffer[13] == 3)
                // {
                // Param = Encoding.GetString(Buffer, 15, Buffer[14]);
                //  }

                //  Int16 MsgLength = (Int16)((Buffer[0x01] << 8) + Buffer[0x00]);
                //  Int16 MsgId = (Int16)((Buffer[0x03] << 8) + Buffer[0x02]);
                //  UInt16 Index = (UInt16)((Buffer[0x05 + 4] << 8) + Buffer[0x04 + 4]);
                //  Channel Channel = (Channel)((Buffer[0x07 + 4] << 8) + Buffer[0x06 + 4]);
                //  Action Action = (Action)(Buffer[0x08 + 4]);
                //  String Param = null;
                //  if (Buffer[0x09 + 4] == 3)
                //      Param = Encoding.GetString(Buffer, 0x0B + 4, Buffer[0x0A + 4]);

                //  MTA.Game.Entity Player = Client.User;

                switch (Action)
                {
                    case Action.Del:
                        {
                            if (Param != Client.Entity.Name) // || GM/PM
                                return;

                            Message.MessageBoard.MessageInfo Info =
                               Message.MessageBoard.GetMsgInfoByAuthor(Param, (ushort)Channel);

                            Message.MessageBoard.Delete(Info, (ushort)Channel);
                            return;
                        }
                    case Action.GetList:
                        {
                            String[] List = Message.MessageBoard.GetList(Index, (ushort)Channel);
                            MsgMessageBoard.Create(Client, Index, Channel, List, Action.List);
                            return;
                        }
                    case Action.GetWords:
                        {
                            String Words = Message.MessageBoard.GetWords(Param, (UInt16)Channel);
                            //   Player.Send(MsgTalk.Create(Param, Player.Name, Words, Channel, 0xFFFFFF));
                            Client.Send(new MTA.Network.GamePackets.Message(Words, Client.Entity.Name, Param, System.Drawing.Color.White, Channel));
                            return;
                        }
                    default:
                        {
                            System.Console.WriteLine("Msg[{0}], Action[{1}] not implemented yet!", MsgId, (Int16)Action);
                            return;
                        }
                }
            }
            catch (Exception Exc) { MTA.Console.WriteLine(Exc); }
        }
    }
}
