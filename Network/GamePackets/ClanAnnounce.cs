using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Interfaces;

namespace MTA.Network.GamePackets
{
    public class ClanAnnouncement : Writer, IPacket
    {
        private Byte[] mData;

        public ClanAnnouncement(Clan clan)
        {
            if (clan.Announcement == null) clan.Announcement = "";
            mData = new Byte[85 + clan.Announcement.Length + 8];
            WriteUInt16((UInt16)(mData.Length - 8), 0, mData);
            WriteUInt16(1312, 2, mData);

            WriteByte((Byte)24, 4, mData);
            WriteUInt16((UInt16)clan.ID, 8, mData);
            WriteByte((Byte)1, 16, mData);
            WriteStringWithLength(clan.Announcement, 17, mData);
        }
        public void Send(Client.GameState client)
        {
            client.Send(mData);
        }

        public byte[] ToArray()
        {
            return mData;
        }

        public void Deserialize(byte[] buffer)
        {
            mData = buffer;
        }
    }
}
