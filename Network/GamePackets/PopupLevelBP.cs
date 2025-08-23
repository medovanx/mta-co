//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Conquer_Online_Server.Network.GamePackets
//{
//    public class PopupLevelBP : Writer, Interfaces.IPacket
//    {
//        byte[] Buffer;

//        public PopupLevelBP()
//        {
//            Buffer = new byte[40];
//            WriteUInt16(32, 0, Buffer);
//            WriteUInt16(2071, 2, Buffer);
//        }
//        public uint Requester
//        {
//            get { return BitConverter.ToUInt32(Buffer, 4); }
//            set { WriteUInt32(value, 4, Buffer); }
//        }
//        public uint Receiver
//        {
//            get { return BitConverter.ToUInt32(Buffer, 8); }
//            set { WriteUInt32(value, 8, Buffer); }
//        }
//        public uint Level
//        {
//            get { return BitConverter.ToUInt32(Buffer, 12); }
//            set { WriteUInt32(value, 12, Buffer); }
//        }

//        public uint BattlePower
//        {
//            get { return BitConverter.ToUInt32(Buffer, 16); }
//            set { WriteUInt32(value, 16, Buffer); }
//        }
//        public byte Marry
//        {
//            get { return Buffer[20]; }
//            set { WriteByte(value, 20, Buffer); }
//        }
//        public byte friend
//        {
//            get { return Buffer[21]; }
//            set { WriteByte(value, 21, Buffer); }
//        }
//        public byte tradepartner
//        {
//            get { return Buffer[22]; }
//            set { WriteByte(value, 22, Buffer); }
//        }
//        public uint Test2
//        {
//            get { return BitConverter.ToUInt32(Buffer, 28); }
//            set { WriteUInt32(value, 28, Buffer); }
//        }
//        public byte menetor
//        {
//            get { return Buffer[23]; }
//            set { WriteByte(value, 23, Buffer); }
//        }
//        public byte apprentice
//        {
//            get { return Buffer[24]; }
//            set { WriteByte(value, 24, Buffer); }
//        }
//        public byte teammate
//        {
//            get { return Buffer[25]; }
//            set { WriteByte(value, 25, Buffer); }
//        }
//        public byte guildmember
//        {
//            get { return Buffer[26]; }
//            set { WriteByte(value, 26, Buffer); }
//        }
//        public byte enemy
//        {
//            get { return Buffer[27]; }
//            set { WriteByte(value, 27, Buffer); }
//        }
//        public void Deserialize(byte[] buffer)
//        {
//            Buffer = buffer;
//        }
//        public byte[] ToArray()
//        {
//            return Buffer;
//        }
//        public void Send(Client.GameState client)
//        {
//            client.Send(Buffer);
//        }
//    }
//}
