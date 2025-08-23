using System;
using System.Drawing;
using MTA.Game;
using System.Collections.Generic;
using System.IO;
namespace MTA.Network.GamePackets
{
    public class ConquerItem : Writer, Interfaces.IPacket
    {
        public const ushort
               Inventory = 0,
               Head = 1,
               Necklace = 2,
               Armor = 3,
               RightWeapon = 4,
               LeftWeapon = 5,
               Ring = 6,
               Bottle = 7,
               Boots = 8,
               Garment = 9,
               Fan = 10,
               Tower = 11,
               Steed = 12,
               SteedCrop = 18,
               RightWeaponAccessory = 15,
               LeftWeaponAccessory = 16,
               SteedArmor = 17,
               Remove = 255,
               Wing = 19,
               AlternateHead = 21,
               AlternateNecklace = 22,
               AlternateArmor = 23,
               AlternateRightWeapon = 24,
               AlternateLeftWeapon = 25,
               AlternateRing = 26,
               AlternateBottle = 27,
               AlternateBoots = 28,
               AlternateGarment = 29;

        public const uint GoldPrize = 2100075;
        public bool IsWorn = false;
        public byte[] Buffer;
        public bool InWardrobe;
        public static Counter ItemUID = new Counter(0);

        private UInt32 mRefineItem = 0;
        private DateTime mRefineryTime;
        private ulong suspiciousStart = 0, unlockEnd = 0;
        private bool unlocking = false;
        private uint warehouse = 0;
        public Dictionary<uint, string> Agate_map { get; set; }
        public DateTime RefineryStarted { get; set; }
        public ConquerItem(bool Create)
        {
            if (Create)
            {
                Buffer = new byte[136 + 8];
                WriteUInt16(136, 0, Buffer);
                WriteUInt16(1008, 2, Buffer);
                Mode = MTA.Game.Enums.ItemMode.Default;
                this.Agate_map = new Dictionary<uint, string>(10);
                StatsLoaded = false;
                //  TimeLeftInMinutes = uint.MaxValue;
            }
        }
        public byte Days
        {
            get;
            set;
        }
        public DateTime DayStamp
        {
            get;
            set;
        }

        public uint UID
        {
            get { return BitConverter.ToUInt32(Buffer, 4); }
            set { WriteUInt32(value, 4, Buffer); }
        }
        public uint ID
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set
            {
                if (value == 0 && ID != 0)
                    throw new Exception("Invalid ID for an Item. Please check the stack trace to find the cause.");
                WriteUInt32(value, 8, Buffer);
            }
        }
        public void SetID(uint ID)
        {
            WriteUInt32(ID, 8, Buffer);
        }
        public ushort Durability
        {
            get { return BitConverter.ToUInt16(Buffer, 12); }
            set { WriteUInt16(value, 12, Buffer); }
        }
        public ushort MaximDurability
        {
            get { return BitConverter.ToUInt16(Buffer, 14); }
            set { WriteUInt16(value, 14, Buffer); }
        }
        public Enums.ItemMode Mode
        {
            get { return (Enums.ItemMode)BitConverter.ToUInt16(Buffer, 16); }
            set { WriteUInt16((ushort)value, 16, Buffer); }
        }

        public byte Perfectionlevel
        {

            get { return Buffer[72]; }
            set
            {
                if (value > 54) value = 54;
                Buffer[72] = value;
            }
        }
        public byte SocketOne2
        {
            get { return Buffer[24]; }
        }
        public byte SocketTwo2
        {
            get { return Buffer[25]; }
        }
        public ushort PerfectionProgress
        {
            get { return BitConverter.ToUInt16(Buffer, 76); }
            set { WriteUInt16(value, 76, Buffer); }
        }
        public uint OwnerID
        {
            get { return BitConverter.ToUInt32(Buffer, 80); }
            set { WriteUInt32(value, 80, Buffer); }
        }
        public string OwnerName
        {
            get { return System.Text.Encoding.Default.GetString(Buffer, 84, 16); }
            set { Writer.Write(value, 84, Buffer); }
        }
        public string Signature
        {
            get { return System.Text.Encoding.Default.GetString(Buffer, 100, 32); }
            set { Writer.Write(value, 100, Buffer); }
        }
        public ushort Position
        {
            get { return BitConverter.ToUInt16(Buffer, 18); }
            set { WriteUInt16(value, 18, Buffer); }
        }
        public uint Warehouse
        {
            get { return warehouse; }
            set { warehouse = value; }
        }
        public uint SocketProgress
        {
            get { return BitConverter.ToUInt32(Buffer, 20); }
            set { WriteUInt32(value, 20, Buffer); }
        }
        public Enums.Gem SocketOne
        {
            get { return (Enums.Gem)Buffer[24]; }
            set { Buffer[24] = (byte)value; }
        }
        public Enums.Gem SocketTwo
        {
            get { return (Enums.Gem)Buffer[25]; }
            set { Buffer[25] = (byte)value; }
        }
        public Enums.ItemEffect Effect
        {
            get { return (Enums.ItemEffect)BitConverter.ToUInt16(Buffer, 28); }
            set { WriteUInt16((ushort)value, 28, Buffer); }
        }
        public byte Plus
        {

            get { return Buffer[33]; }
            set
            {
                if (value > 12) value = 12;
                Buffer[33] = value;
            }
        }
        public byte Bless
        {
            get { return Buffer[34]; }
            set { Buffer[34] = value; }
        }
        public bool Bound
        {
            get { return Buffer[35] == 0 ? false : true; }
            set { Buffer[35] = (byte)(value ? 1 : 0); }
        }
        public byte Enchant
        {
            get { return Buffer[36]; }
            set { Buffer[36] = value; }
        }

        public byte NextRed
        {
            get { return Buffer[34]; }
            set { Buffer[34] = value; }
        }
        public byte NextBlue
        {
            get { return Buffer[36]; }
            set { Buffer[36] = value; }
        }
        public byte NextGreen
        {
            get { return Buffer[40]; }
            set { Buffer[40] = value; }
        }

        public bool Suspicious
        {
            get { return Buffer[44] == 0 ? false : true; }
            set { Buffer[44] = (byte)(value ? 1 : 0); }
        }

        public byte Lock
        {
            get { return Buffer[46]; }
            set { Buffer[46] = value; }
        }

        public Enums.Color Color
        {
            get { return (Enums.Color)BitConverter.ToUInt32(Buffer, 48); }
            set { WriteUInt32((uint)value, 48, Buffer); }
        }
        public uint PlusProgress
        {
            get { return BitConverter.ToUInt32(Buffer, 52); }
            set { WriteUInt32(value, 52, Buffer); }
        }
        public bool Inscribed
        {
            get
            {
                return (BitConverter.ToUInt16(this.Buffer, 56) == 1);
            }
            set
            {
                Writer.WriteUInt16(value ? ((byte)1) : ((byte)0), 56, this.Buffer);
            }
        }
        public uint TimeLeftInMinutes
        {
            get { return BitConverter.ToUInt32(Buffer, 60); }
            set { WriteUInt32(value, 60, Buffer); }
        }
        public ushort StackSize
        {
            get { return BitConverter.ToUInt16(Buffer, 68); }
            set { WriteUInt16(value, 68, Buffer); }
        }
        public ushort MaxStackSize
        {
            get;
            set;
        }
        public DateTime SuspiciousStart
        {
            get { return DateTime.FromBinary((long)suspiciousStart); }
            set { suspiciousStart = (ulong)value.Ticks; }
        }
        public DateTime UnlockEnd
        {
            get { return DateTime.FromBinary((long)unlockEnd); }
            set { unlockEnd = (ulong)value.Ticks; }
        }

        public bool Unlocking
        {
            get { return unlocking; }
            set { unlocking = value; }
        }
        public bool MobDropped
        {
            get;
            set;
        }
        public bool StatsLoaded
        {
            get;
            set;
        }
        public string Agate
        {
            get;
            set;
        }

        public byte[] ToArray()
        {
            return Buffer;
        }

        public void Deserialize(byte[] buffer)
        {
            throw new NotImplementedException();
        }
        //public Double GemBonus(Byte type)
        //{
        //    Double bonus = 0;
        //    if (SocketOne != Enums.Gem.NoSocket)
        //    {
        //       if ((Int32)SocketOne / 10 == type)
        //            bonus += ItemSocket.GetGemBonus((Byte)SocketOne);
        //        if (SocketTwo != Enums.Gem.NoSocket)
        //        {
        //            if ((Int32)SocketTwo / 10 == type)
        //                bonus += ItemSocket.GetGemBonus((Byte)SocketTwo);
        //        }
        //    }
        //    return bonus;
        //}
        public override string ToString()
        {
            return ID.ToString() + "#"
                + Durability.ToString() + "#"
                + MaximDurability.ToString() + "#"
                + Position.ToString() + "#"
                + SocketProgress.ToString() + "#"
                + ((byte)SocketOne).ToString() + "#"
                + ((byte)SocketTwo).ToString() + "#"
                + ((ushort)Effect).ToString() + "#"
                + Plus.ToString() + "#"
                + Bless.ToString() + "#"
                + (Bound ? "1" : "0") + "#"
                + Enchant.ToString() + "#"
                + (Suspicious ? "1" : "0") + "#"
                + Lock.ToString() + "#"
                + (Unlocking ? "1" : "0") + "#"
                + PlusProgress.ToString() + "#"
                + (Inscribed ? "1" : "0") + "#"
                + suspiciousStart.ToString() + "#"
                + Days.ToString() + "#"
                + DayStamp.ToString() + "#"
                + unlockEnd.ToString();
        }
        public string ToLog()
        {
            return "UID: " + UID.ToString() + " | "
                + "ID: " + ID.ToString() + " | "
                + "Plus: " + Plus.ToString();
        }
        public ItemAdding.Purification_ Purification
        {
            get;
            set;
        }
        public ItemAdding.Refinery_ ExtraEffect
        {
            get;
            set;
        }
        public UInt32 RefineItem
        {
            get { return mRefineItem; }
            set { mRefineItem = value; }
        }
        public void SendExtras(Client.GameState client)
        {
            if (client == null)
                return;

            if (RefineItem != 0)
            {
                Refinery.RefineryItem rI = RefineStats;
                if (rI != null)
                {
                    client.Send(new Game_ItemSoul()
                    {
                        ID = rI.Identifier,
                        Identifier = UID,
                        Level = rI.Level,
                        Mode = Game_ItemSoul.Types.Refine,
                        Percent = rI.Percent,
                        Type = 1,
                        Time = (UInt32)(RefineryTime.Subtract(DateTime.Now).TotalSeconds)
                    });
                }
            }

            ItemAdding add = new ItemAdding(true);
            if (Purification.Available)
                add.Append(Purification);
            if (ExtraEffect.Available)
                add.Append(ExtraEffect);
            if (Purification.Available || ExtraEffect.Available)
                client.Send(add);

            if (Lock == 2)
            {
                ItemLock itemLock = new ItemLock(true);
                itemLock.UID = UID;
                itemLock.ID = ItemLock.UnlockDate;
                itemLock.dwParam = (uint)(UnlockEnd.Year * 10000 + UnlockEnd.Month * 100 + UnlockEnd.Day);
                client.Send(itemLock);
            }
        }
        public void SendAgate(Client.GameState client)
        {
            byte[] buffer = new byte[(40 + (0x30 * this.Agate_map.Count)) + 0x30];
            Writer.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
            Writer.WriteUInt16(0x83e, 2, buffer);
            Writer.WriteUInt32(this.UID, 8, buffer);
            Writer.WriteUInt32((byte)this.Agate_map.Count, 12, buffer);
            Writer.WriteUInt32((byte)this.Agate_map.Count, 0x10, buffer);
            Writer.WriteUInt32(this.Durability, 0x18, buffer);
            Writer.WriteUInt32((byte)this.Agate_map.Count, 0x1c, buffer);
            if (this.Agate_map.Count > 0)
            {
                int offset = 0x20;
                for (uint i = 0; i < this.Agate_map.Count; i++)
                {
                    Writer.WriteUInt32(i, offset, buffer);
                    offset += 4;
                    Writer.WriteUInt16(ushort.Parse(this.Agate_map[i].Split(new char[] { '~' })[0].ToString()), offset, buffer);
                    offset += 4;
                    Writer.WriteUInt16(ushort.Parse(this.Agate_map[i].Split(new char[] { '~' })[1].ToString()), offset, buffer);
                    offset += 4;
                    Writer.WriteUInt16(ushort.Parse(this.Agate_map[i].Split(new char[] { '~' })[2].ToString()), offset, buffer);
                    offset += 0x24;
                }
            }
            client.Send(buffer);
        }
        public void Send(Client.GameState client)
        {
            if (client == null)
                return;

            if (ID == 300000)
            {
                uint G = SocketProgress & 0xFF;
                uint B = (SocketProgress >> 8) & 0xFF;
                uint R = (SocketProgress >> 16) & 0xFF;

                if (NextRed == 0 && NextBlue == 0 && NextGreen == 0)
                {
                    NextRed = (byte)R;
                    NextBlue = (byte)B;
                    NextGreen = (byte)G;
                    Database.ConquerItemTable.UpdateNextSteedColor(this);
                }
            }

            if (Days > 0)
            {
                if (DateTime.Now >= this.DayStamp.AddDays(Days))
                {
                    Database.ConquerItemTable.DeleteItem(this.UID);
                    Database.ConquerItemTable.RemoveItem(this.UID);
                    client.Send(new MTA.Network.GamePackets.Message("Your Item Has Expired", System.Drawing.Color.Red, Message.TopLeft));
                }
                TimeSpan Remain = DayStamp.AddDays(Days) - DateTime.Now;
                TimeLeftInMinutes = (uint)Remain.TotalSeconds;
            }

            client.Send(Buffer);

            ItemAdding add = new ItemAdding(true);
            if (Purification.Available)
                add.Append(Purification);
            if (ExtraEffect.Available)
                add.Append(ExtraEffect);

            if (Purification.Available || ExtraEffect.Available)
                client.Send(add);



            if (Lock == 2 && (Mode == Enums.ItemMode.Default || Mode == Enums.ItemMode.Update))
            {
                ItemLock itemLock = new ItemLock(true);
                itemLock.UID = UID;
                itemLock.ID = ItemLock.UnlockDate;
                itemLock.dwParam = (uint)(UnlockEnd.Year * 10000 + UnlockEnd.Month * 100 + UnlockEnd.Day);
                client.Send(itemLock);
            }
            Mode = Enums.ItemMode.Default;
            //   client.Send(Buffer);
        }
        public static Boolean isRune(UInt32 itemid)
        {
            if (itemid >= 729960 && itemid <= 729970)
                return true;
            return false;
        }
        public ushort Vigor
        {
            get;
            set;
        }
        public short BattlePower
        {
            get
            {
                short potBase = 0;
                byte Quality = (byte)(ID % 10);
                if (Quality >= 5)
                    potBase += (byte)(Quality - 5);
                potBase += Plus;
                if (SocketOne != Enums.Gem.NoSocket) potBase++;
                if (SocketTwo != Enums.Gem.NoSocket) potBase++;
                if (((byte)SocketOne) % 10 == 3) potBase++;
                if (((byte)SocketTwo) % 10 == 3) potBase++;

                if (ID / 1000 == 421 || PacketHandler.IsTwoHand(ID))
                    potBase *= 2;
                return potBase;
            }
        }
        public override int GetHashCode()
        {
            return (int)this.UID;
        }
        public override bool Equals(object obj)
        {
            return (obj as ConquerItem).UID == GetHashCode();
        }
        public Refinery.RefineryItem RefineStats
        {
            get
            {
                Refinery.RefineryItem i = null;
                Kernel.DatabaseRefinery.TryGetValue(RefineItem, out i);
                return i;
            }
        }
        public DateTime RefineryTime
        {
            get { return mRefineryTime; }
            set { mRefineryTime = value; }
        }
        public bool IsTwoHander()
        {
            ItemTypes item_type = (ItemTypes)GetItemType();
            bool check = ((UInt16)item_type >= 500 && (UInt16)item_type <= 580);
            if (check)
            {
                check = (item_type != ItemTypes.ShieldID);
                if (check)
                {
                    check = (item_type != ItemTypes.NinjaSwordID);
                    if (check)
                    {
                        check = (item_type != ItemTypes.MonkBeadsID);
                    }
                }
            }
            return check;
        }
        public ItemTypes GetItemType()
        {
            return (ItemTypes)(this.ID / 1000);
        }
        public static void CheckItemExtra(ConquerItem i, Client.GameState c)
        {
            if (i.RefineryTime.Ticks != 0)
            {
                if (DateTime.Now > i.RefineryTime)
                {
                    i.RefineItem = 0;
                    i.RefineryTime = new DateTime(0);

                    Game_ItemSoul expire = new Game_ItemSoul()
                    {
                        Identifier = i.UID
                    };
                    expire.Expired(c);
                    i.Send(c);
                    c.Send(PacketHandler.WindowStats(c));

                    Database.ConquerItemTable.UpdateRefineryItem(i);
                    Database.ConquerItemTable.UpdateRefineryTime(i);

                    if (!c.Equipment.Free(i.Position))
                    {
                        //c.UnloadItemStats(i, true);
                        c.LoadItemStats();
                    }
                }
            }
        }
        public enum ItemTypes : ushort
        {
            BowID = 500,
            FrankoID = 1050,
            ShieldID = 900,
            BackswordID = 421,
            BladeID = 410,
            SwordID = 420,
            NinjaSwordID = 601,
            MonkBeadsID = 610,
            FanID = 201,
            TowerID = 202,
            CropID = 203,
            GarmentID_1 = 181,
            GarmentID_2 = 182,
            BottleID = 2100,
            GemID = 700,
            PickaxeID = 562,
            RingID = 150,
            BootID = 160,
            SteedArmorID = 200,

            NecklaceID = 120,
            BraceletID = 152,
            BagID = 121,
            RightAccessory1 = 350,
            RightAccessory2 = 360,
            RightAccessory3 = 370,
            LeftAccessory = 380
        }

    }

}