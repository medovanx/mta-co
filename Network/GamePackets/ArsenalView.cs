using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Game.ConquerStructures.Society;

namespace MTA.Network.GamePackets
{
    public class ArsenalView : Writer, Interfaces.IPacket
    {
        public const uint 
            Unlock = 0,
            Inscribe = 1,
            View = 4;

        byte[] Buffer;

        public ArsenalView(bool create, uint itemCount = 0)
        {
            if (create)
            {
                Buffer = new byte[56 + itemCount * 40];
                WriteUInt16((ushort)(48 + itemCount * 40), 0, Buffer);
                WriteUInt16(2202, 2, Buffer);
            }
        }

        public uint Type
        {
            get { return BitConverter.ToUInt32(Buffer, 4); }
            set { WriteUInt32(value, 4, Buffer); }
        }

        public uint BeginAt
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32(value, 8, Buffer); }
        }
        
        public uint EndAt
        {
            get { return BitConverter.ToUInt32(Buffer, 12); }
            set { WriteUInt32(value, 12, Buffer); }
        }

        public uint ArsenalType
        {
            get { return BitConverter.ToUInt32(Buffer, 16); }
            set { WriteUInt32(value, 16, Buffer); }
        }

        public int TotalInscribed
        {
            get { return BitConverter.ToInt32(Buffer, 20); }
            set { WriteInt32(value, 20, Buffer); }
        }

        public uint SharedBattlepower
        {
            get { return BitConverter.ToUInt32(Buffer, 24); }
            set { WriteUInt32(value, 24, Buffer); }
        }

        public uint Enchantment
        {
            get { return BitConverter.ToUInt32(Buffer, 28); }
            set { WriteUInt32(value, 28, Buffer); }
        }

        public int EnchantmentExpirationDate
        {
            get { return BitConverter.ToInt32(Buffer, 32); }
            set { WriteInt32(value, 32, Buffer); }
        }

        public uint Donation
        {
            get { return BitConverter.ToUInt32(Buffer, 36); }
            set { WriteUInt32(value, 36, Buffer); }
        }

        public uint Count
        {
            get { return BitConverter.ToUInt32(Buffer, 40); }
            set { WriteUInt32(value, 40, Buffer); }
        }

        public void AppendItem(Arsenal.ArsenalItem item)
        {
            int offset = (int)(44 + 40 * Count); Count++;
            WriteUInt32(item.UID, offset, Buffer); offset += 4;
            WriteUInt32(item.Rank, offset, Buffer); offset += 4;
            WriteString(item.Owner, offset, Buffer); offset += 16;
            WriteUInt32(item.ID, offset, Buffer); offset += 4;
            Buffer[offset] = (byte)(item.ID % 10); offset++;
            Buffer[offset] = item.Plus; offset++;
            Buffer[offset] = item.SocketOne; offset++;
            Buffer[offset] = item.SocketTwo; offset++;
            WriteUInt32(item.BattlePower, offset, Buffer); offset += 4;
            WriteUInt32(item.DonationWorth, offset, Buffer); offset += 4;
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
