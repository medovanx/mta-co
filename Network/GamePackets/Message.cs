using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

namespace MTA.Network.GamePackets
{
    public class Message : Interfaces.IPacket
    {
        public string _From;
        public string _To;
        public uint ChatType;
        public Color Color;
        public string __Message;
        public uint Mesh;

        public const uint
            CS = 2402,
                    Talk = 2000,
                    Whisper = 2001,
                    Team = 2003,
                    Guild = 2004,
                    Clan = 2006,
                    System = 2007,
                    Friend = 2009,
                    Ally = 2025,  
                    Center = 2011,
                    TopLeft = 2012,
                    Service = 2014,
                    Tip = 2015,
                    World = 2021,
                    Qualifier = 2022,
                    PopUP = 2100,
                    Dialog = 2101,
                    Website = 2105,
                    FirstRightCorner = 2108,
                    ContinueRightCorner = 2109,
                    SystemWhisper = 2110,
                    GuildAnnouncement = 2111,
                    Agate = 2115,
                    ArenaQualifier = 2022,
                    BroadcastMessage = 2500,
                    Monster = 2600,
                    SlideFromRight = 100000,
                    HawkMessage = 2104,
                    SlideFromRightRedVib = 1000000,
                    WhiteVibrate = 10000000;


        public Message(string _Message, uint _ChatType, Game.Languages Language = Game.Languages.English)
        {
            this.Mesh = 0;
            this.__Message = _Message;
            this._To = "ALL";
            this._From = "SYSTEM";
            this.Color = Color.Red;
            this.ChatType = _ChatType;
        }
        public Message(string _Message, Color _Color, uint _ChatType, Game.Languages Language = Game.Languages.English)
        {
            this.Mesh = 0;
            this.__Message = _Message;
            this._To = "ALL";
            this._From = "SYSTEM";
            this.Color = _Color;
            this.ChatType = _ChatType;
        }
        public Message(string _Message, string __To, Color _Color, uint _ChatType, Game.Languages Language = Game.Languages.English)
        {
            this.Mesh = 0;
            this.__Message = _Message;
            this._To = __To;
            this._From = "SYSTEM";
            this.Color = _Color;
            this.ChatType = _ChatType;
        }
        public Message(string _Message, Client.GameState Client, Color _Color, uint _ChatType, Game.Languages Language = Game.Languages.English)
        {
            this.Mesh = 0;
            this.__Message = _Message;
            this._To = Client.Entity.Name;
            this.MessageUID1 = Client.Entity.UID;
            this._From = "SYSTEM";
            this.Color = _Color;
            this.ChatType = _ChatType;
        }
        public Message(string _Message, string __To, string __From, Color _Color, uint _ChatType, Game.Languages Language = Game.Languages.English)
        {
            this.Mesh = 0;
            this.__Message = _Message;
            this._To = __To;
            this._From = __From;
            this.Color = _Color;
            this.ChatType = _ChatType;
        }
        public Message()
        {
            this.Mesh = 0;
        }

        public Game.Languages Language = Game.Languages.English;
        public uint MessageUID1 = 0;
        public uint MessageUID2 = 0;

        public void Deserialize(byte[] buffer)
        {
            Color = Color.FromArgb(BitConverter.ToInt32(buffer, 8));
            ChatType = BitConverter.ToUInt32(buffer, 12);
            MessageUID1 = BitConverter.ToUInt32(buffer, 16);
            MessageUID1 = BitConverter.ToUInt32(buffer, 20);
            Mesh = BitConverter.ToUInt32(buffer, 24);
            _From = Encoding.Default.GetString(buffer, 36, buffer[35]);
            _To = Encoding.Default.GetString(buffer, 37 + _From.Length, buffer[36 + _From.Length]);
            __Message = Encoding.Default.GetString(buffer, (39 + _From.Length) + _To.Length, buffer[(38 + _From.Length) + _To.Length]);
        }  
        public byte[] ToArray()
        {
            byte[] Packet = new byte[(((32 + _From.Length) + _To.Length) + __Message.Length) + 18];
            Writer.WriteUInt16((ushort)(Packet.Length - 8), 0, Packet);
            Writer.WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, Packet);
            Writer.WriteUInt16(1004, 2, Packet);
            Writer.WriteUInt32((uint)Color.ToArgb(), 8, Packet);
            Writer.WriteUInt32(ChatType, 12, Packet);
            Writer.WriteUInt32(MessageUID1, 16, Packet);
            Writer.WriteUInt32(MessageUID1, 20, Packet);
            Writer.WriteUInt32(Mesh, 24, Packet);
            Writer.WriteStringList(new List<string>() { _From, _To, "", __Message }, 34, Packet);
            return Packet;
        }
      
        public void Send(Client.GameState client)
        {
             if (client.Language == Language)
                 client.Send(ToArray());
        }
        public class MessageBoard
        {
            private const Int32 TITLE_SIZE = 44;
            private const Int32 LIST_SIZE = 10;

            private static List<MessageInfo> TradeBoard = new List<MessageInfo>();
            private static List<MessageInfo> FriendBoard = new List<MessageInfo>();
            private static List<MessageInfo> TeamBoard = new List<MessageInfo>();
            private static List<MessageInfo> SynBoard = new List<MessageInfo>();
            private static List<MessageInfo> OtherBoard = new List<MessageInfo>();
            private static List<MessageInfo> SystemBoard = new List<MessageInfo>();

            public struct MessageInfo
            {
                public String Author;
                public String Words;
                public String Date;
            };

            public static void Add(String Author, String Words, ushort Channel)
            {
                MessageInfo Info = new MessageInfo();
                Info.Author = Author;
                Info.Words = Words;
                Info.Date = DateTime.Now.ToString("yyyyMMddHHmmss");

                switch (Channel)
                {
                    case 2201:
                        TradeBoard.Add(Info);
                        break;
                    case 2202:
                        FriendBoard.Add(Info);
                        break;
                    case 2203:
                        TeamBoard.Add(Info);
                        break;
                    case 2204:
                        SynBoard.Add(Info);
                        break;
                    case 2205:
                        OtherBoard.Add(Info);
                        break;
                    case 2206:
                        SystemBoard.Add(Info);
                        break;
                }
            }

            public static void Delete(MessageInfo Message, ushort Channel)
            {
                switch (Channel)
                {
                    case 2201:
                        if (TradeBoard.Contains(Message))
                            TradeBoard.Remove(Message);
                        break;
                    case 2202:
                        if (FriendBoard.Contains(Message))
                            FriendBoard.Remove(Message);
                        break;
                    case 2203:
                        if (TeamBoard.Contains(Message))
                            TeamBoard.Remove(Message);
                        break;
                    case 2204:
                        if (SynBoard.Contains(Message))
                            SynBoard.Remove(Message);
                        break;
                    case 2205:
                        if (OtherBoard.Contains(Message))
                            OtherBoard.Remove(Message);
                        break;
                    case 2206:
                        if (SystemBoard.Contains(Message))
                            SystemBoard.Remove(Message);
                        break;
                }
            }

            public static String[] GetList(UInt16 Index, ushort Channel)
            {
                MessageInfo[] Board = null;
                switch (Channel)
                {
                    case 2201:
                        Board = TradeBoard.ToArray();
                        break;
                    case 2202:
                        Board = FriendBoard.ToArray();
                        break;
                    case 2203:
                        Board = TeamBoard.ToArray();
                        break;
                    case 2204:
                        Board = SynBoard.ToArray();
                        break;
                    case 2205:
                        Board = OtherBoard.ToArray();
                        break;
                    case 2206:
                        Board = SystemBoard.ToArray();
                        break;
                    default:
                        return null;
                }

                if (Board.Length == 0)
                    return null;

                if ((Index / 8 * LIST_SIZE) > Board.Length)
                    return null;

                String[] List = null;

                Int32 Start = (Board.Length - ((Index / 8 * LIST_SIZE) + 1));

                if (Start < LIST_SIZE)
                    List = new String[(Start + 1) * 3];
                else
                    List = new String[LIST_SIZE * 3];

                Int32 End = (Start - (List.Length / 3));

                Int32 x = 0;
                for (Int32 i = Start; i > End; i--)
                {
                    List[x + 0] = Board[i].Author;
                    if (Board[i].Words.Length > TITLE_SIZE)
                        List[x + 1] = Board[i].Words.Remove(TITLE_SIZE, Board[i].Words.Length - TITLE_SIZE);
                    else
                        List[x + 1] = Board[i].Words;
                    List[x + 2] = Board[i].Date;
                    x += 3;
                }
                return List;
            }

            public static String GetWords(String Author, ushort Channel)
            {
                MessageInfo[] Board = null;
                switch (Channel)
                {
                    case 2201:
                        Board = TradeBoard.ToArray();
                        break;
                    case 2202:
                        Board = FriendBoard.ToArray();
                        break;
                    case 2203:
                        Board = TeamBoard.ToArray();
                        break;
                    case 2204:
                        Board = SynBoard.ToArray();
                        break;
                    case 2205:
                        Board = OtherBoard.ToArray();
                        break;
                    case 2206:
                        Board = SystemBoard.ToArray();
                        break;
                    default:
                        return "";
                }

                foreach (MessageInfo Info in Board)
                {
                    if (Info.Author == Author)
                        return Info.Words;
                }
                return "";
            }

            public static MessageInfo GetMsgInfoByAuthor(String Author, ushort Channel)
            {
                MessageInfo[] Board = null;
                switch (Channel)
                {
                    case 2201:
                        Board = TradeBoard.ToArray();
                        break;
                    case 2202:
                        Board = FriendBoard.ToArray();
                        break;
                    case 2203:
                        Board = TeamBoard.ToArray();
                        break;
                    case 2204:
                        Board = SynBoard.ToArray();
                        break;
                    case 2205:
                        Board = OtherBoard.ToArray();
                        break;
                    case 2206:
                        Board = SystemBoard.ToArray();
                        break;
                    default:
                        return new MessageInfo();
                }

                foreach (MessageInfo Info in Board)
                {
                    if (Info.Author == Author)
                        return Info;
                }
                return new MessageInfo();
            }
        }
    }
}
