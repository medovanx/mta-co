using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Game.ConquerStructures.Society;

namespace MTA.Network.GamePackets
{
    public class ArsenalTab : Writer, Interfaces.IPacket
    {
        byte[] Buffer;
        public ArsenalTab(bool create)
        {
            if (create)
            {
                Buffer = new byte[252];
                WriteUInt16(244, 0, Buffer);
                WriteUInt16(2201, 2, Buffer);
            }
        }

        public uint Type
        {
            get { return BitConverter.ToUInt32(Buffer, 4); }
            set { WriteUInt32(value, 4, Buffer); }
        }

        public uint SharedBattlepower
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32(value, 8, Buffer); }
        }

        public uint dwParam
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32(value, 8, Buffer); }
        }

        public uint HeroDonation
        {
            get { return BitConverter.ToUInt32(Buffer, 12); }
            set { WriteUInt32(value, 12, Buffer); }
        }

        public uint dwParam2
        {
            get { return BitConverter.ToUInt32(Buffer, 12); }
            set { WriteUInt32(value, 12, Buffer); }
        }

        public uint HeroSharedBattlepower
        {
            get { return BitConverter.ToUInt32(Buffer, 16); }
            set { WriteUInt32(value, 16, Buffer); }
        }

        public uint ArsenalCount
        {
            get { return BitConverter.ToUInt32(Buffer, 20); }
            set { WriteUInt32(value, 20, Buffer); }
        }

        public void AppendArsenal(Arsenal arsenal)
        {
            int offset = 28 + 24 * (arsenal.Position - 1);
            if (!arsenal.Unlocked)
            {
                offset += 16;
                WriteInt32(arsenal.Unlocked ? 1 : 0, offset, Buffer); offset += 4;
                WriteUInt32(arsenal.Position, offset, Buffer); offset += 4;
            }
            else
            {
                WriteUInt32(arsenal.SharedBattlePower, offset, Buffer); offset += 4;
                WriteUInt32(arsenal.Enhancement, offset, Buffer); offset += 4;
                WriteUInt32(arsenal.Donation, offset, Buffer); offset += 4;
                WriteInt32(arsenal.EnhancementExpirationDate(), offset, Buffer); offset += 4;
                WriteInt32(arsenal.Unlocked ? 1 : 0, offset, Buffer); offset += 4;
                WriteUInt32(arsenal.Position, offset, Buffer); offset += 4;
            }
        }

        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }

        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }

        public byte[] ToArray()
        {
            return Buffer;
        }
    }
}
