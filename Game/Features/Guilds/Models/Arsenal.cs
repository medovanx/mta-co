using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MTA.Client;
using MTA.Game.Features.Guilds.Database;
using MTA.Network.GamePackets;

namespace MTA.Game.Features.Guilds.Models;

/// <summary>
///     Represents a guild arsenal slot for inscribed items, contributing to shared battle power and tracking donations.
/// </summary>
public class Arsenal(Guild super) {
    private uint _donation;
    public DateTime EnhancementExpDate;
    public ConcurrentDictionary<uint, ArsenalItem> ItemDictionary = new();
    public List<ArsenalItem> OrderedList = [];

    public byte Position;

    public uint SharedBattlePower, Enhancement;
    public Guild Super = super;
    public bool Unlocked;

    public uint Donation {
        get => _donation;
        set {
            var val = TotalSharedBattlePower;
            SharedBattlePower = value switch {
                < 2000000 => 0,
                < 4000000 => 1,
                < 10000000 => 2,
                _ => 3
            };
            if (val != SharedBattlePower) Super.ArsenalBpChanged = true;
            _donation = value;
        }
    }

    public uint TotalSharedBattlePower => Math.Min(3, SharedBattlePower + Enhancement);

    /// <summary>
    ///     Sorts items by battle power and assigns ranks, updating the ordered list for display.
    /// </summary>
    public void OrderList() {
        OrderedList = ItemDictionary.Values.OrderByDescending(p => p.BattlePower).ToList();
        uint rank = 0;
        foreach (var item in OrderedList) {
            rank++;
            item.Rank = rank;
        }
    }

    /// <summary>
    ///     Returns enhancement expiration as integer date (YYYYMMDD format).
    /// </summary>
    public int EnhancementExpirationDate() {
        return EnhancementExpDate.Year * 10000 + EnhancementExpDate.Month * 100 + EnhancementExpDate.Day;
    }

    /// <summary>
    ///     Adds item to arsenal with donation tracking, updating member donation records and shared battle power.
    /// </summary>
    public void AddItem(ConquerItem item, GameState client) {
        if (ItemDictionary.TryGetValue(item.UID, out var aItem1)) {
            aItem1.Update(item, client);
        }
        else {
            var aItem = new ArsenalItem(this, item, client);
            ItemDictionary.Add(aItem.Uid, aItem);
            client.ArsenalDonations[Position] += aItem.DonationWorth;
            Donation += aItem.DonationWorth;
            if (client.AsMember != null) {
                client.AsMember.ArsenalDonation = Donation;
                GuildMemberTable.Save(client.AsMember);
            }
        }

        OrderList();
    }

    /// <summary>
    ///     Removes item from arsenal and updates donation, adjusting member donation records if client is provided.
    /// </summary>
    public void RemoveItem(ArsenalItem item, GameState? client) {
        ItemDictionary.Remove(item.Uid);
        if (client != null) {
            client.ArsenalDonations[Position] -= item.DonationWorth;
            if (client.AsMember != null) {
                client.AsMember.ArsenalDonation = Donation;
                GuildMemberTable.Save(client.AsMember);
            }
        }

        Donation -= item.DonationWorth;

        OrderList();
    }

    /// <summary>
    ///     Removes all items inscribed by a specific player, typically used when a member leaves the guild.
    /// </summary>
    public void RemoveInscribedItemsBy(uint uid) {
        var array = ItemDictionary.Values.ToArray();
        foreach (var item in array)
            if (item.OwnerUid == uid)
                RemoveItem(item, null);

        OrderList();
    }

    public void Load(BinaryReader reader) {
        if (reader.BaseStream.Length == reader.BaseStream.Position) return;
        Position = reader.ReadByte();
        Unlocked = reader.ReadBoolean();
        Donation = reader.ReadUInt32();
        Enhancement = reader.ReadUInt32();
        EnhancementExpDate = DateTime.FromBinary(reader.ReadInt64());
        var itemCount = reader.ReadInt32();
        for (var i = 0; i < itemCount; i++) {
            var item = new ArsenalItem(this);
            item.Load(reader);
            ItemDictionary.Add(item.Uid, item);
        }

        OrderList();
        if (Enhancement == 0) return;
        if (DateTime.Now >= EnhancementExpDate)
            Enhancement = 0;
    }

    public void Save(BinaryWriter writer) {
        writer.Write(Position);
        writer.Write(Unlocked);
        writer.Write(Donation);
        writer.Write(Enhancement);
        writer.Write(EnhancementExpDate.Ticks);
        writer.Write(ItemDictionary.Count);
        foreach (var item in ItemDictionary.Values)
            item.Save(writer);
    }

    /// <summary>
    ///     Represents an inscribed item in the arsenal, tracking its battle power, donation worth, and owner information.
    /// </summary>
    public class ArsenalItem {
        private readonly Arsenal _super;
        public uint Id, Uid, Rank, BattlePower, DonationWorth, OwnerUid;
        public string Owner;
        public byte Plus, SocketOne, SocketTwo;

        public ArsenalItem(Arsenal super) {
            _super = super;
            Owner = string.Empty;
        }

        public ArsenalItem(Arsenal super, ConquerItem item, GameState client) {
            _super = super;
            Owner = string.Empty;
            Uid = item.UID;
            Update(item, client);
            OwnerUid = client.Entity.UID;
        }

        /// <summary>
        ///     Updates item stats when upgraded, recalculating battle power and donation worth based on new item properties.
        /// </summary>
        public void Update(ConquerItem item, GameState client) {
            var updated = Id != item.ID || Plus != item.Plus || SocketOne != (byte)item.SocketOne ||
                          SocketTwo != (byte)item.SocketTwo;
            Id = item.ID;
            Plus = item.Plus;
            SocketOne = (byte)item.SocketOne;
            SocketTwo = (byte)item.SocketTwo;
            Owner = client.Entity.Name;
            if (updated) {
                _super.Donation -= DonationWorth;
                BattlePower = CalculateBattlepower(item);
                DonationWorth = CalculateDonationWorth(item);
                _super.Donation += DonationWorth;
            }
        }

        private static uint CalculateBattlepower(ConquerItem item) {
            var bp = item.Plus + (item.ID % 10 - 5);
            if (item.SocketOne == Enums.Gem.NoSocket) return bp;
            bp += 1;
            if ((byte)item.SocketOne % 10 == 3)
                bp += 1;
            if (item.SocketTwo == Enums.Gem.NoSocket) return bp;
            bp += 1;
            if ((byte)item.SocketTwo % 10 == 3)
                bp += 1;

            return bp;
        }

        private static uint CalculateDonationWorth(ConquerItem item) {
            var remainder = item.ID % 10;
            var worth = remainder == 8 ? 1000u : remainder == 9 ? 16660u : 0u;

            if (item.SocketOne != Enums.Gem.NoSocket) {
                worth += 33330;
                if (item.SocketTwo != Enums.Gem.NoSocket)
                    worth += 100000;
            }

            switch (item.Plus) {
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

        /// <summary>
        ///     Loads item from binary reader, restoring all item properties and owner information.
        /// </summary>
        public void Load(BinaryReader reader) {
            Uid = reader.ReadUInt32();
            OwnerUid = reader.ReadUInt32();
            Id = reader.ReadUInt32();
            BattlePower = reader.ReadUInt32();
            DonationWorth = reader.ReadUInt32();
            Plus = reader.ReadByte();
            SocketOne = reader.ReadByte();
            SocketTwo = reader.ReadByte();
            Owner = reader.ReadString();
        }

        /// <summary>
        ///     Saves item to binary writer, persisting all item properties for later restoration.
        /// </summary>
        public void Save(BinaryWriter writer) {
            writer.Write(Uid);
            writer.Write(OwnerUid);
            writer.Write(Id);
            writer.Write(BattlePower);
            writer.Write(DonationWorth);
            writer.Write(Plus);
            writer.Write(SocketOne);
            writer.Write(SocketTwo);
            writer.Write(Owner);
        }
    }
}