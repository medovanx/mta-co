using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Interfaces;
using MTA.Network;
using MTA.Client;

namespace MTA.Network.GamePackets
{
    #region Class Level
    public class SubClassShowFull : Writer, IPacket
    {
        public enum SubProfessionAction
        {
            Switch = 0,
            Unknown1 = 1,
            RequestUpgrade = 2,
            ConfirmUpgrade = 3,
            ConfirmJoin = 4,
            ConfirmPromote = 5,
            QueryInfo = 6,
            SendInfo = 7,
            SetStudyPoint = 8,
            RequestJoin = 9,
            RequestPromote = 10
        }
        public const byte
        SwitchSubClass = 0,
        ActivateSubClass = 1,
        LearnSubClass = 4,
        ShowGUI = 7;
        int num = 4;
        byte[] Buffer;
        public SubClassShowFull(bool Create)
        {
            if (Create)
            {
                Buffer = new byte[8 + 26 + num];
                WriteUInt16(30, 0, Buffer);
                WriteUInt16(2320, 2, Buffer);
            }
        }
        public ushort Study
        {
            get
            {
                return BitConverter.ToUInt16(this.Buffer, 10);
            }
            set
            {
                WriteUInt16(value, 10, this.Buffer);
            }
        }

        public ushort StudyReceive
        {
            get
            {
                return BitConverter.ToUInt16(this.Buffer, 0x12);
            }
            set
            {
                WriteUInt16(value, 0x12, this.Buffer);
            }
        }
        public ushort ID
        {
            get { return BitConverter.ToUInt16(Buffer, 4 + num); }
            set { WriteUInt16(value, 4 + num, Buffer); }
        }

        public byte Class
        {
            get { return Buffer[6 + num]; }
            set { Buffer[6 + num] = value; }
        }

        public byte Level
        {
            get { return Buffer[7 + num]; }
            set { Buffer[7 + num] = value; }
        }

        public void Deserialize(byte[] buffer)
        {
            this.Buffer = buffer;
        }

        public byte[] ToArray()
        {
            return Buffer;
        }

        public void Send(Client.GameState c)
        {
            c.Send(Buffer);
        }
    }
    #endregion
    #region Class Send    
    public class Game_SubClass : IPacket
    {
        private Byte[] mData;
        int num = 4;
        public Game_SubClass()
        {
            this.mData = new Byte[33 + 8];
            Writer.WriteUInt16((UInt16)(mData.Length - 8), 0, mData);
            Writer.WriteUInt16((UInt16)2320, 2, mData);
            Writer.WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, mData);
        }

        public Types Type
        {
            get { return (Types)BitConverter.ToUInt16(mData, 4 + num); }
            set { Writer.WriteUInt16((Byte)value, 4 + num, mData); }
        }
        public ID ClassId
        {
            get { return (ID)mData[6 + num]; }
            set { mData[6 + num] = (Byte)value; }
        }
        public Byte Phase
        {
            get { return mData[7 + num]; }
            set { mData[7 + num] = value; }
        }
        public void Deserialize(byte[] buffer)
        {
            this.mData = buffer;
        }

        public byte[] ToArray()
        {
            return mData;
        }

        public void Send(Client.GameState c)
        {
            c.Send(mData);
        }

        public enum Types : ushort
        {
            Switch = 0,
            Activate = 1,
            Upgrade = 2,
            Learn = 4,
            MartialPromoted = 5,
            Show = 6,
            Info = 7
        }
        public enum ID : byte
        {
            None = 0,
            MartialArtist = 1,
            Warlock = 2,
            ChiMaster = 3,
            Sage = 4,
            Apothecary = 5,
            Performer = 6,
            Wrangler = 9
        }
    }
    public class SubClass : Writer, IPacket
    {
        public const byte
        SwitchSubClass = 0,
        ActivateSubClass = 1,
        ShowSubClasses = 7,
        MartialPromoted = 5,
        LearnSubClass = 4;
        Game.Entity Owner = null;

        byte[] Buffer;
        byte Type;
        public SubClass(Game.Entity E) { Owner = E; Type = 7; }

        public void Deserialize(byte[] buffer)
        {
            this.Buffer = buffer;
        }

        public byte[] ToArray()
        {
            Buffer = new byte[8 + 30 + (Owner.SubClasses.Classes.Count * 3)];
            WriteUInt16((ushort)(Buffer.Length - 8), 0, Buffer);
            WriteUInt16(2320, 2, Buffer);
            WriteUInt16((ushort)Type, 8, Buffer);
            WriteUInt16(Owner.SubClasses.StudyPoints, 10, Buffer);
            WriteUInt16((ushort)Owner.SubClasses.Classes.Count, 26, Buffer);
            int Position = 30;
            if (Owner.SubClasses.Classes.Count > 0)
            {
                Game.SubClass[] Classes = new Game.SubClass[Owner.SubClasses.Classes.Count];
                Owner.SubClasses.Classes.Values.CopyTo(Classes, 0);
                foreach (Game.SubClass Class in Classes)
                {
                    WriteByte(Class.ID, Position, Buffer); Position++;
                    WriteByte(Class.Phase, Position, Buffer); Position++;
                    WriteByte(Class.Level, Position, Buffer); Position++;
                }
            }
            WriteString("TQServer", (Buffer.Length - 8), Buffer);
            return Buffer;
        }

        public void Send(Client.GameState c)
        {
            c.Send(Buffer);
        }
    }
    #endregion
   
}