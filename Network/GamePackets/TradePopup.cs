using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network.GamePackets
{
    public class PopupLevelBP : Writer, Interfaces.IPacket
    {
        byte[] Buffer;

        public PopupLevelBP()
        {
            Buffer = new byte[40];
            WriteUInt16(32, 0, Buffer);
            WriteUInt16(2071, 2, Buffer);
        }
        public uint Requester
        {
            get { return BitConverter.ToUInt32(Buffer, 4); }
            set { WriteUInt32(value, 4, Buffer); }
        }
        public uint Receiver
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32(value, 8, Buffer); }
        }
        public uint Level
        {
            get { return BitConverter.ToUInt32(Buffer, 12); }
            set { WriteUInt32(value, 12, Buffer); }
        }

        public uint BattlePower
        {
            get { return BitConverter.ToUInt32(Buffer, 16); }
            set { WriteUInt32(value, 16, Buffer); }
        }

        public bool Spouse
        {
            get { return Buffer[20] == 1; }
            set { Buffer[20] = value == true ? (byte)1 : (byte)0; }
        }
        public bool Friend
        {
            get { return Buffer[21] == 1; }
            set { Buffer[21] = value == true ? (byte)1 : (byte)0; }
        }
        public bool TradePartner
        {
            get { return Buffer[22] == 1; }
            set { Buffer[22] = value == true ? (byte)1 : (byte)0; }
        }
        public bool Mentor
        {
            get { return Buffer[23] == 1; }
            set { Buffer[23] = value == true ? (byte)1 : (byte)0; }
        }
        public bool Apprentice
        {
            get { return Buffer[24] == 1; }
            set { Buffer[24] = value == true ? (byte)1 : (byte)0; }
        }
        public bool Teammate
        {
            get { return Buffer[25] == 1; }
            set { Buffer[25] = value == true ? (byte)1 : (byte)0; }
        }
        public bool GuildMember
        {
            get { return Buffer[26] == 1; }
            set { Buffer[26] = value == true ? (byte)1 : (byte)0; }
        }
        public bool Enemy
        {
            get { return Buffer[27] == 1; }
            set { Buffer[27] = value == true ? (byte)1 : (byte)0; }
        }
       
        public uint Test2
        {
            get { return BitConverter.ToUInt32(Buffer, 28); }
            set { WriteUInt32(value, 28, Buffer); }
        }
            
            
       
        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }
        public byte[] ToArray()
        {
            return Buffer;
        }
        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }
    }
}