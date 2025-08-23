/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KinSocket;

namespace MTA.Network.GamePackets
{
    public class SyncPacket
    {
        private byte[] mData = new byte[0x40 + 8 + 4];

        public SyncPacket()
        {
            PacketConstructor.Write((ushort)0x40 + 4, 0, this.mData);
            PacketConstructor.Write((ushort)0x2721, 2, this.mData);
            this.Count = 1;
        }

        public static implicit operator byte[](SyncPacket d)
        {
            return d.mData;
        }

        public uint Count
        {
            get
            {
                return BitConverter.ToUInt32(this.mData, 8 + 4);
            }
            set
            {
                PacketConstructor.Write(value, 8 + 4, this.mData);
            }
        }

        public uint Duration
        {
            get
            {
                return BitConverter.ToUInt32(this.mData, 20 + 4);
            }
            set
            {
                PacketConstructor.Write(value, 20 + 4, this.mData);
            }
        }

        public uint Identifier
        {
            get
            {
                return BitConverter.ToUInt32(this.mData, 4 + 4);
            }
            set
            {
                PacketConstructor.Write(value, 4 + 4, this.mData);
            }
        }

        public uint Level
        {
            get
            {
                return BitConverter.ToUInt32(this.mData, 0x34 + 4);
            }
            set
            {
                PacketConstructor.Write(value, 0x34 + 4, this.mData);
            }
        }

        public uint Multiple
        {
            get
            {
                return BitConverter.ToUInt32(this.mData, 0x1c + 4);
            }
            set
            {
                PacketConstructor.Write(value, 0x1c + 4, this.mData);
            }
        }

        public ulong StatusFlag1
        {
            get
            {
                return BitConverter.ToUInt64(this.mData, 0x10 + 4);
            }
            set
            {
                PacketConstructor.Write(value, 0x10 + 4, this.mData);
            }
        }

        public ulong StatusFlag2
        {
            get
            {
                return BitConverter.ToUInt64(this.mData, 0x18 + 4);
            }
            set
            {
                PacketConstructor.Write(value, 0x18 + 4, this.mData);
            }
        }

        public uint StatusFlag3
        {
            get
            {
                return BitConverter.ToUInt32(this.mData, 0x20 + 4);
            }
            set
            {
                PacketConstructor.Write(value, 0x20 + 4, this.mData);
            }
        }

        public uint StatusFlagOffset
        {
            get
            {
                return BitConverter.ToUInt32(this.mData, 40 + 4);
            }
            set
            {
                PacketConstructor.Write(value, 40 + 4, this.mData);
            }
        }

        public uint Time
        {
            get
            {
                return BitConverter.ToUInt32(this.mData, 0x2c + 4);
            }
            set
            {
                PacketConstructor.Write(value, 0x2c + 4, this.mData);
            }
        }
        public enum SyncType
        {
            Agility = 0x10,
            AttributePoints = 10,
            AzureShield = 0x31,
            Class = 7,
            CP = 0x1d,
            CPB = 0x2d,
            CursedTimer = 20,
            DoubleExpTimer = 0x12,
            EnlightPoints = 0x29,
            Experience = 5,
            FirstRebornClass = 0x33,
            Gold = 4,
            GuildBattlePower = 0x2c,
            HairStyle = 0x1a,
            HeavensBlessing = 0x11,
            HitPoints = 0,
            Level = 12,
            Lookface = 11,
            LuckyTimeTimer = 0x1c,
            ManaPoints = 2,
            MaxHitPoints = 1,
            MaxManaPoints = 3,
            Merchant = 0x26,
            OnlineTraining = 0x1f,
            OtherBonus = 0x24,
            PKPoints = 6,
            QuizPoints = 40,
            Reborn = 0x16,
            SecondRebornClass = 50,
            Spirit = 13,
            Stamina = 8,
            StaticPoints = 0x2a,
            StatusFlag = 0x19,
            Strength = 15,
            VIPLevel = 0x27,
            Vitality = 14,
            WarehouseGold = 9,
            XPCircle = 0x1b
        }
        public SyncType Type
        {
            get
            {
                return (SyncType)BitConverter.ToUInt32(this.mData, 12 + 4);
            }
            set
            {
                PacketConstructor.Write((uint)value, 12 + 4, this.mData);
            }
        }

        public uint Unknown1
        {
            get
            {
                return BitConverter.ToUInt32(this.mData, 0x24 + 4);
            }
            set
            {
                PacketConstructor.Write(value, 0x24 + 4, this.mData);
            }
        }

        public uint Value
        {
            get
            {
                return BitConverter.ToUInt32(this.mData, 0x30 + 4);
            }
            set
            {
                PacketConstructor.Write(value, 0x30 + 4, this.mData);
            }
        }
    }
}*/
