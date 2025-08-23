using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Conquer_Online_Server.Network;

namespace COServer.Network.GamePackets
{
    public class EventAlert
    {
        private byte[] mData;
        public EventAlert()
        {
            mData = new byte[28];
            Writer.WriteUInt16(20, 0, mData);
            Writer.WriteUInt16((ushort)1126, 2, mData);
        }
        public EventAlert(byte[] d)
        {
            mData = new byte[d.Length];
            d.CopyTo(mData, 0);
        }
        public static implicit operator byte[](EventAlert d)
        {
            return d.mData;
        }
        public uint Countdown
        {
            get
            {
                return BitConverter.ToUInt32(mData, 16);
            }
            set
            {
                Writer.WriteUInt32(value, 16, mData);
            }
        }
        public uint StrResID
        {
            get
            {
                return BitConverter.ToUInt32(mData, 8);
            }
            set
            {
                Writer.WriteUInt32(value, 8, mData);
            }
        }
        public uint UK12
        {
            get
            {
                return BitConverter.ToUInt32(mData, 12);
            }
            set
            {
                Writer.WriteUInt32(value, 12, mData);
            }
        }
    }
}