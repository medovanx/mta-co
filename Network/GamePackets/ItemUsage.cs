using System;

namespace MTA.Network.GamePackets
{
    public class ItemUsage : Writer, Interfaces.IPacket
    {
        public const ushort
            BuyFromNPC = 1,
            SellToNPC = 2,
            RemoveInventory = 3,
            EquipItem = 4,
            Unknown5 = 5,
            UnequipItem = 6,
            FrankoReload = 8,
            ViewWarehouse = 9,
            WarehouseDeposit = 10,
            WarehouseWithdraw = 11,
            Repair = 14,
            VIPRepair = 15,
            DragonBallUpgrade = 19,
            MeteorUpgrade = 20,
            ShowBoothItems = 21,
            AddItemOnBoothForSilvers = 22,
            RemoveItemFromBooth = 23,
            BuyFromBooth = 24,
            UpdateDurability = 25,
            AddItemOnBoothForConquerPoints = 29,
            Ping = 27,
            Enchant = 28,
            RedeemGear = 32,
            ClaimGear = 33,
            SocketTalismanWithItem = 35,
            SocketTalismanWithCPs = 36,
            DropItem = 37,
            DropMoney = 38,
            GemCompose = 39,
            Bless = 40,
            Accessories = 41,
            MainEquipment = 44,
            AlternateEquipment = 45,
            LowerEquipment = 54,
            ToristSuper = 51,
            SocketerMan = 43,
            MergeStackableItems = 48,
            SplitStack = 49;

        byte[] Buffer;
        public ItemUsage(bool Create)
        {
            if (Create)
            {
                Buffer = new byte[112];
                WriteUInt16(104, 0, Buffer);
                WriteUInt16(1009, 2, Buffer);
                WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, Buffer);
            }
        }
        public uint UID
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32(value, 8, Buffer); }
        }
        public uint dwParam
        {
            get { return BitConverter.ToUInt32(Buffer, 12); }
            set { WriteUInt32(value, 12, Buffer); }
        }

        public ushort ID
        {
            get { return BitConverter.ToUInt16(Buffer, 20); }
            set { WriteUInt16(value, 20, Buffer); }
        }

        public uint TimeStamp
        {
            get { return BitConverter.ToUInt32(Buffer, 24); }
            set { WriteUInt32(value, 24, Buffer); }
        }

        public uint dwExtraInfo
        {
            get { return BitConverter.ToUInt32(Buffer, 26); }
            set { WriteUInt32(value, 26, Buffer); }
        }
        public uint dwExtraInfo2
        {
            get { return BitConverter.ToUInt32(Buffer, 88); }
            set { WriteUInt32(value, 88, Buffer); }
        }
        public uint dwExtraInfo3
        {
            get { return BitConverter.ToUInt32(Buffer, 36); }
            set { WriteUInt32(value, 36, Buffer); }
        }

        public uint[] Batch
        {
            get
            {
                uint[] items = new uint[dwExtraInfo];
                for (int i = 0; i < dwExtraInfo; i++)
                {
                    items[i] = BitConverter.ToUInt32(Buffer, 91 + 4 * i);
                }
                return items;
            }
            set
            {
                if (value != null)
                {
                    dwExtraInfo = (uint)value.Length;
                    for (int i = 0; i < dwExtraInfo; i++)
                    {
                        WriteUInt32(value[i], 91 + 4 * i, Buffer);
                    }
                }
            }
        }

        public byte[] ToArray()
        {
            return Buffer;
        }
        public void Deserialize(byte[] buffer)
        {
            Buffer = buffer;
        }
        public void Send(Client.GameState client)
        {
            client.Send(Buffer);
        }
    }
}
