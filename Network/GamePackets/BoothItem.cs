using System;
using System.Drawing;
using MTA.Game;

namespace MTA.Network.GamePackets
{
    public unsafe class BoothItem2 : Interfaces.IPacket
    {
        private Byte[] mData;
        public void ParseItem(ConquerItem i)
        {
            ItemID = i.ID;
            ItemIdentifier = i.UID;
            Durability = i.Durability;
            MaxDurability = i.MaximDurability;
            SocketProgress = i.SocketProgress;
            SocketOne = i.SocketOne;
            SocketTwo = i.SocketTwo;
            Effect = (UInt16)i.Effect;
            Plus = i.Plus;
            Bless = i.Bless;
            Bound = i.Bound;
            Enchant = i.Enchant;
            Suspicious = i.Suspicious;
            Lock = Convert.ToBoolean(i.Lock);
            Color = (UInt32)i.Color;
            PlusProgress = i.PlusProgress;
            StackSize = i.StackSize;
            TimeLeftInMinutes = i.TimeLeftInMinutes;
            if (i.Perfectionlevel > 0)
            {
                Perfectionlevel = i.Perfectionlevel;
                PerfectionProgress = i.PerfectionProgress;
                OwnerID = i.OwnerID;
                OwnerName = i.OwnerName;
                if (i.Signature.Length > 0)
                {
                    Signature = i.Signature;
                }
            }  
        }
        public BoothItem2()
        {
            mData = new Byte[143 + 8];
            Writer.WriteUInt16(((UInt16)(mData.Length - 8)), 0, mData);
            Writer.WriteUInt16((UInt16)1108, 2, mData);
        }
        public UInt32 Perfectionlevel
        {
            get { return BitConverter.ToUInt32(mData, 82); }
            set { Writer.WriteUInt32(value, 82, mData); }
        }
        public UInt32 PerfectionProgress
        {
            get { return BitConverter.ToUInt32(mData, 86); }
            set { Writer.WriteUInt32(value, 86, mData); }
        }
        public UInt32 OwnerID
        {
            get { return BitConverter.ToUInt32(mData, 90); }
            set { Writer.WriteUInt32(value, 90, mData); }
        }
        public string OwnerName
        {
            get { return System.BitConverter.ToString(mData, 94); }
            set { Writer.WriteString(value, 94, mData); }
        }
        public string Signature
        {
            get { return System.BitConverter.ToString(mData, 110); }
            set { Writer.WriteString(value, 110, mData); }
        } 
        public UInt32 ItemIdentifier
        {
            get { return BitConverter.ToUInt32(mData, 4); }
            set { Writer.WriteUInt32(value, 4, mData); }
        }
        public UInt32 Identifier
        {
            get { return BitConverter.ToUInt32(mData, 8); }
            set { Writer.WriteUInt32(value, 8, mData); }
        }
        public UInt32 Cost
        {
            get { return BitConverter.ToUInt32(mData, 12); }
            set { Writer.WriteUInt32(value, 12, mData); }
        }
        public UInt32 ItemID
        {
            get { return BitConverter.ToUInt32(mData, 20); }
            set { Writer.WriteUInt32(value, 20, mData); }
        }
        public UInt16 Durability
        {
            get { return BitConverter.ToUInt16(mData, 24); }
            set { Writer.WriteUInt16(value, 24, mData); }
        }
        public UInt16 MaxDurability
        {
            get { return BitConverter.ToUInt16(mData, 26); }
            set { Writer.WriteUInt16(value, 26, mData); }
        }
        public CostTypes CostType
        {
            get { return (CostTypes)BitConverter.ToUInt16(mData, 28); }
            set { Writer.WriteUInt16((UInt16)value, 28, mData); }
        }
        public PacketHandler.Positions Position
        {
            get { return (PacketHandler.Positions)mData[30]; }
            set { mData[30] = (Byte)value; }
        }
        public UInt32 SocketProgress
        {
            get { return BitConverter.ToUInt32(mData, 31); }
            set { Writer.WriteUInt32(value, 31, mData); }
        }
        public Enums.Gem SocketOne
        {
            get { return (Enums.Gem)mData[35]; }
            set { mData[35] = (Byte)value; }
        }
        public Enums.Gem SocketTwo
        {
            get { return (Enums.Gem)mData[36]; }
            set { mData[36] = (Byte)value; }
        }
        public UInt16 Effect
        {
            get { return BitConverter.ToUInt16(mData, 41); }
            set { Writer.WriteUInt16((UInt16)value, 41, mData); }
        }
        public Byte Plus
        {
            get { return mData[42]; }
            set { mData[42] = value; }
        }
        public Byte Bless
        {
            get { return mData[43]; }
            set { mData[43] = value; }
        }
        public Boolean Bound
        {
            get { return mData[44] == 0 ? false : true; }
            set { mData[44] = (Byte)(value ? 1 : 0); }
        }
        public Byte Enchant
        {
            get { return mData[45]; }
            set { mData[45] = value; }
        }
        public Boolean Suspicious
        {
            get { return mData[51] == 0 ? false : true; }
            set { mData[51] = (Byte)(value ? 1 : 0); }
        }
        public Boolean Lock
        {
            get { return mData[52] == 0 ? false : true; }
            set { mData[52] = (Byte)(value ? 1 : 0); }
        }
        public UInt32 Color
        {
            get { return BitConverter.ToUInt32(mData, 54); }
            set { Writer.WriteUInt32((UInt32)value, 54, mData); }
        }
        public UInt32 PlusProgress
        {
            get { return BitConverter.ToUInt32(mData, 56); }
            set { Writer.WriteUInt32(value, 56, mData); }
        }
        public uint TimeLeftInMinutes
        {
            get { return BitConverter.ToUInt32(mData, 60); }
            set { Writer.WriteUInt32(value, 60, mData); }
        }
        public UInt16 StackSize
        {
            get { return BitConverter.ToUInt16(mData, 72); }
            set { Writer.WriteUInt16(value, 72, mData); }
        }
        public UInt32 PurificationID
        {
            get { return BitConverter.ToUInt32(mData, 74); }
            set { Writer.WriteUInt32(value, 74, mData); }
        }
        public byte[] ToArray()
        {
            return mData;
        }
        public void Deserialize(byte[] buffer)
        {
            mData = buffer;
        }
        public void Send(Client.GameState client)
        {
            client.Send(mData);
        }
        public enum CostTypes : ushort
        {
            Gold = 1,
            CPs = 3,
            ViewEquip = 4
        }
    }
    public class BoothItem : Writer, Interfaces.IPacket
    {
        byte[] Buffer;
        public BoothItem(bool Create)
        {
            if (Create)
            {
                Buffer = new byte[82 + 8];
                WriteUInt16(((UInt16)(Buffer.Length - 8)), 0, Buffer);
                WriteUInt16(1108, 2, Buffer);
            }
        }
        public uint UID
        {
            get { return BitConverter.ToUInt32(Buffer, 4); }
            set { WriteUInt32(value, 4, Buffer); }
        }
        public uint BoothID
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { WriteUInt32(value, 8, Buffer); }
        }
        public uint Cost
        {
            get { return BitConverter.ToUInt32(Buffer, 12); }
            set { WriteUInt32(value, 12, Buffer); }
        }
        public uint ID
        {
            get { return BitConverter.ToUInt32(Buffer, 20); }
            set { WriteUInt32(value, 20, Buffer); }
        }
        public ushort Durability
        {
            get { return BitConverter.ToUInt16(Buffer, 24); }
            set { WriteUInt16(value, 24, Buffer); }
        }
        public ushort MaximDurability
        {
            get { return BitConverter.ToUInt16(Buffer, 26); }
            set { WriteUInt16(value, 26, Buffer); }
        }
        public uint CostType
        {
            get { return BitConverter.ToUInt32(Buffer, 28); }
            set { WriteUInt32(value, 28, Buffer); }
        }
        public uint SocketProgress
        {
            get { return BitConverter.ToUInt32(Buffer, 31); }
            set { WriteUInt32(value, 31, Buffer); }
        }
        public Enums.Gem SocketOne
        {
            get { return (Enums.Gem)Buffer[35]; }
            set { Buffer[35] = (byte)value; }
        }
        public Enums.Gem SocketTwo
        {
            get { return (Enums.Gem)Buffer[36]; }
            set { Buffer[36] = (byte)value; }
        }
        public Enums.ItemEffect Effect
        {
            get { return (Enums.ItemEffect)BitConverter.ToUInt16(Buffer, 41); }
            set { WriteUInt16((ushort)value, 41, Buffer); }
        }
        public byte Plus
        {
            get { return Buffer[42]; }
            set { Buffer[42] = value; }
        }
        public byte Bless
        {
            get { return Buffer[43]; }
            set { Buffer[43] = value; }
        }
        public bool Bound
        {
            get { return Buffer[44] == 0 ? false : true; }
            set { Buffer[44] = (byte)(value ? 1 : 0); }
        }
        public byte Enchant
        {
            get { return Buffer[45]; }
            set { Buffer[45] = value; }
        }
        public bool Suspicious
        {
            get { return Buffer[51] == 0 ? false : true; }
            set { Buffer[51] = (byte)(value ? 1 : 0); }
        }
        public byte Lock
        {
            get { return Buffer[52]; }
            set { Buffer[52] = value; }
        }
        public Enums.Color Color
        {
            get { return (Enums.Color)BitConverter.ToUInt32(Buffer, 54); }
            set { WriteUInt32((uint)value, 54, Buffer); }
        }
        public uint PlusProgress
        {
            get { return BitConverter.ToUInt32(Buffer, 56); }
            set { WriteUInt32(value, 56, Buffer); }
        }
        public bool Inscribed
        {
            get { return (BitConverter.ToUInt16(this.Buffer, 60) == 1); }
            set { Writer.WriteUInt16(value ? ((byte)1) : ((byte)0), 60, this.Buffer); }
        }
        public uint TimeLeftInMinutes
        {
            get { return BitConverter.ToUInt32(Buffer, 64); }
            set { Writer.WriteUInt32(value, 64, Buffer); }
        }
        public ushort StackSize
        {
            get { return BitConverter.ToUInt16(Buffer, 72); }
            set { WriteUInt16(value, 72, Buffer); }
        }
        public uint PurificationID
        {
            get { return BitConverter.ToUInt32(Buffer, 74); }
            set { WriteUInt32(value, 74, Buffer); }
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
        public override int GetHashCode()
        {
            return (int)this.UID;
        }
        public void Fill(Game.ConquerStructures.BoothItem item, uint boothID)
        {
            UID = item.Item.UID;
            BoothID = boothID;
            Cost = item.Cost;
            ID = item.Item.ID;
            Durability = item.Item.Durability;
            MaximDurability = item.Item.MaximDurability;
            CostType = (byte)item.Cost_Type;
            SocketOne = item.Item.SocketOne;
            SocketTwo = item.Item.SocketTwo;
            Effect = item.Item.Effect;
            Bound = item.Item.Bound;
            Plus = item.Item.Plus;
            Bless = item.Item.Bless;
            Enchant = item.Item.Enchant;
            SocketProgress = item.Item.SocketProgress;
            Color = item.Item.Color;
            PlusProgress = item.Item.PlusProgress;
            StackSize = item.Item.StackSize;
            Inscribed = item.Item.Inscribed;
            TimeLeftInMinutes = item.Item.TimeLeftInMinutes;
        }
        public void Fill(ConquerItem item, uint boothID)
        {
            UID = item.UID;
            BoothID = boothID;
            ID = item.ID;
            Durability = item.Durability;
            MaximDurability = item.MaximDurability;
            Buffer[24] = (byte)4;
            Buffer[26] = (byte)item.Position;
            SocketOne = item.SocketOne;
            SocketTwo = item.SocketTwo;
            Effect = item.Effect;
            Plus = item.Plus;
            Bound = item.Bound;
            Bless = item.Bless;
            Enchant = item.Enchant;
            SocketProgress = item.SocketProgress;
            Color = item.Color;
            PlusProgress = item.PlusProgress;
            StackSize = item.StackSize;
            Inscribed = item.Inscribed;
            TimeLeftInMinutes = item.TimeLeftInMinutes;
        }
    }
}