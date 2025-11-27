using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MTA.Network.GamePackets;
using System.Collections.Concurrent;
using MTA.Database;

namespace MTA.Game.ConquerStructures.Society
{
    public class Arsenal
    {
        public class ArsenalItem
        {
            public uint ID, UID, Rank, BattlePower, DonationWorth, OwnerUID;
            public string Owner;
            public byte Plus, SocketOne, SocketTwo;
            public Arsenal Super;

            public ArsenalItem(Arsenal super)
            {
                Super = super;
            }
            public ArsenalItem(Arsenal super, ConquerItem item, Client.GameState client)
            {
                Super = super;
                UID = item.UID;
                Update(item, client);
                OwnerUID = client.Entity.UID;
            }

            public void Update(ConquerItem item, Client.GameState client)
            {
                bool updated = (ID != item.ID || Plus != item.Plus || SocketOne != (byte)item.SocketOne || SocketTwo != (byte)item.SocketTwo);
                ID = item.ID;
                Plus = item.Plus;
                SocketOne = (byte)item.SocketOne;
                SocketTwo = (byte)item.SocketTwo;
                Owner = client.Entity.Name;
                if (updated)
                {
                    Super.Donation -= DonationWorth;
                    BattlePower = CalculateBattlepower(item);
                    DonationWorth = CalculateDonationWorth(item);
                    Super.Donation += DonationWorth;
                }
            }
            public uint CalculateBattlepower(ConquerItem item)
            {
                uint bp = item.Plus + ((item.ID % 10) - 5);
                if (item.SocketOne != Enums.Gem.NoSocket)
                {
                    bp += 1;
                    if (((byte)item.SocketOne) % 10 == 3)
                        bp += 1;
                    if (item.SocketTwo != Enums.Gem.NoSocket)
                    {
                        bp += 1;
                        if (((byte)item.SocketTwo) % 10 == 3)
                            bp += 1;
                    }
                }
                return bp;
            }
            public uint CalculateDonationWorth(ConquerItem item)
            {
                uint worth = 0;
                switch (item.ID % 10)
                {
                    case 8:
                        worth = 1000;
                        break;
                    case 9:
                        worth = 16660;
                        break;
                }
                if (item.SocketOne != Enums.Gem.NoSocket)
                {
                    worth += 33330;
                    if (item.SocketTwo != Enums.Gem.NoSocket)
                        worth += 100000;
                }
                switch (item.Plus)
                {
                    case 1:
                        worth += 90;
                        break;
                    case 2:
                        worth += 490;
                        break;
                    case 3:
                        worth += 1350;
                        break;
                    case 4:
                        worth += 4070;
                        break;
                    case 5:
                        worth += 12340;
                        break;
                    case 6:
                        worth += 37030;
                        break;
                    case 7:
                        worth += 111110;
                        break;
                    case 8:
                        worth += 333330;
                        break;
                    case 9:
                        worth += 1000000;
                        break;
                    case 10:
                        worth += 1033330;
                        break;
                    case 11:
                        worth += 1101230;
                        break;
                    case 12:
                        worth += 1212340;
                        break;

                }
                return worth;
            }

            public void Load(BinaryReader reader)
            {
                UID = reader.ReadUInt32();
                OwnerUID = reader.ReadUInt32();
                ID = reader.ReadUInt32();
                BattlePower = reader.ReadUInt32();
                DonationWorth = reader.ReadUInt32();
                Plus = reader.ReadByte();
                SocketOne = reader.ReadByte();
                SocketTwo = reader.ReadByte();
                Owner = reader.ReadString();
            }
            public void Save(BinaryWriter writer)
            {
                writer.Write(UID);
                writer.Write(OwnerUID);
                writer.Write(ID);
                writer.Write(BattlePower);
                writer.Write(DonationWorth);
                writer.Write(Plus);
                writer.Write(SocketOne);
                writer.Write(SocketTwo);
                writer.Write(Owner);
            }
        }

        public uint SharedBattlePower, Enhancement, _Donation;

        public uint Donation
        {
            get { return _Donation; }
            set
            {
                uint val = TotalSharedBattlePower;
                if (value < 2000000) SharedBattlePower = 0;
                else if (value >= 2000000 && value < 4000000) SharedBattlePower = 1;
                else if (value >= 4000000 && value < 10000000) SharedBattlePower = 2;
                if (value >= 10000000) SharedBattlePower = 3;
                if (val != SharedBattlePower) Super.ArsenalBPChanged = true;
                _Donation = value;
            }
        }
        public uint TotalSharedBattlePower
        {
            get
            {
                return Math.Min(3, SharedBattlePower + Enhancement);
            }
        }

        public byte Position;
        public bool Unlocked;
        public DateTime EnhancementExpDate;
        public ConcurrentDictionary<uint, ArsenalItem> ItemDictionary;
        public List<ArsenalItem> OrderedList;
        public Guild Super;

        public Arsenal(Guild super)
        {
            Unlocked = false;
            ItemDictionary = new ConcurrentDictionary<uint, ArsenalItem>();
            OrderedList = new List<ArsenalItem>();
            Super = super;
        }

        public void OrderList()
        {
            OrderedList = ItemDictionary.Values.OrderByDescending(p => p.BattlePower).ToList();
            uint rank = 0;
            foreach (var item in OrderedList)
            {
                rank++;
                item.Rank = rank;
            }
        }

        public int EnhancementExpirationDate()
        {
            return EnhancementExpDate.Year * 10000 + EnhancementExpDate.Month * 100 + EnhancementExpDate.Day;
        }

        public void AddItem(ConquerItem item, Client.GameState client)
        {
            if (ItemDictionary.ContainsKey(item.UID))
            {
                var aItem = ItemDictionary[item.UID];
                aItem.Update(item, client);
            }
            else
            {
                var aItem = new ArsenalItem(this, item, client);
                ItemDictionary.Add(aItem.UID, aItem);
                if (client != null)
                {
                    client.ArsenalDonations[Position] += aItem.DonationWorth;
                    Donation += aItem.DonationWorth;
                    using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                        cmd.Update("entities").Set("GuildArsenalDonation", (uint)Donation).Where("UID", client.Entity.UID)
                            .Execute();
                }
            }
            OrderList();
        }

        public void RemoveItem(ArsenalItem item, Client.GameState client)
        {
            ItemDictionary.Remove(item.UID);
            if (client != null)
            {
                client.ArsenalDonations[Position] -= item.DonationWorth;
                Donation -= item.DonationWorth;
                using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                    cmd.Update("entities").Set("GuildArsenalDonation", (uint)Donation).Where("UID", client.Entity.UID)
                        .Execute();
            }
            OrderList();
        }
        public void RemoveItem(ConquerItem item, Client.GameState client)
        {
            if (ItemDictionary.ContainsKey(item.UID))
                RemoveItem(ItemDictionary[item.UID], client);
            OrderList();
        }
        public void RemoveInscribedItemsBy(uint uid)
        {
            var array = ItemDictionary.Values.ToArray();
            foreach (var item in array)
            {
                if (item.OwnerUID == uid)
                {
                    RemoveItem(item, null);
                }
            }
            OrderList();
        }
        public void Load(BinaryReader reader)
        {
            if (reader.BaseStream.Length == reader.BaseStream.Position) return;
            Position = reader.ReadByte();
            Unlocked = reader.ReadBoolean();
            Donation = reader.ReadUInt32();
            Enhancement = reader.ReadUInt32();
            EnhancementExpDate = DateTime.FromBinary(reader.ReadInt64());
            int itemCount = reader.ReadInt32();
            for (int i = 0; i < itemCount; i++)
            {
                ArsenalItem item = new ArsenalItem(this);
                item.Load(reader);
                ItemDictionary.Add(item.UID, item);
            }
            OrderList();
            if (Enhancement != 0)
                if (DateTime.Now >= EnhancementExpDate)
                    Enhancement = 0;
        }
        public void Save(BinaryWriter writer)
        {
            writer.Write(Position);
            writer.Write(Unlocked);
            writer.Write(Donation);
            writer.Write(Enhancement);
            writer.Write(EnhancementExpDate.Ticks);
            writer.Write(ItemDictionary.Count);
            foreach (var item in ItemDictionary.Values)
                item.Save(writer);
        }
    }
}
