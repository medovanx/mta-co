using System;
using System.IO;
using MTA.Network.GamePackets;

namespace MTA.Database
{
    public class DetainedItemTable
    {
        public static Counter Counter = new MTA.Counter(1);
        public static void LoadDetainedItems(Client.GameState client)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("detaineditems").Where("OwnerUID", client.Entity.UID))
            using (var reader = new MySqlReader(cmd))
            {
                while (reader.Read())
                {
                    DetainedItem item = new DetainedItem(true);
                    item.ItemUID = reader.ReadUInt32("ItemUID");
                    item.UID = item.ItemUID - 1;
                    item.Item = ConquerItemTable.LoadItem(item.ItemUID);
                    item.ConquerPointsCost = reader.ReadUInt32("ConquerPointsCost");
                    item.OwnerUID = reader.ReadUInt32("OwnerUID");
                    item.GainerName = reader.ReadString("OwnerName");
                    item.GainerUID = reader.ReadUInt32("GainerUID");
                    item.OwnerName = reader.ReadString("GainerName");
                    item.Date = DateTime.FromBinary(reader.ReadInt64("Date"));
                    item.DaysLeft = (uint)(TimeSpan.FromTicks(DateTime.Now.Ticks).Days - TimeSpan.FromTicks(item.Date.Ticks).Days);
                    if (DateTime.Now < item.Date.AddDays(7))
                        client.DeatinedItem.Add(item.UID, item);
                    else
                        if (item.Bound)
                            Claim(item, client);
                }
            }
        }
        public static void DetainItem(ConquerItem item, Client.GameState owner, Client.GameState gainer)
        {
            DetainedItem Item = new DetainedItem(true);
            Item.ItemUID = item.UID;
            Item.Item = item;
            Item.UID = Item.ItemUID - 1;
            Item.ConquerPointsCost = CalculateCost(item);
            Item.OwnerUID = owner.Entity.UID;
            Item.OwnerName = owner.Entity.Name;
            Item.GainerUID = gainer.Entity.UID;
            Item.GainerName = gainer.Entity.Name;
            Item.Date = DateTime.Now;
            Item.DaysLeft = 0;
            owner.DeatinedItem.Add(Item.UID, Item);
            owner.Send(Item);

            ItemAdding add = new ItemAdding(true);
            if (Item.Item.Purification.Available)
                add.Append(Item.Item.Purification);
            if (Item.Item.ExtraEffect.Available)
                add.Append(Item.Item.ExtraEffect);
            if (Item.Item.Purification.Available || Item.Item.ExtraEffect.Available)
                owner.Send(add);
          

            DetainedItem Item2 = new DetainedItem(true);
            Item2.ItemUID = item.UID;
            Item2.UID = Item2.ItemUID - 1;
            Item2.Item = item;
            Item2.Page = (byte)DetainedItem.ClaimPage;
            Item2.ConquerPointsCost = CalculateCost(item);
            Item2.OwnerUID = gainer.Entity.UID;
            Item2.GainerName = gainer.Entity.Name;
            Item2.GainerUID = owner.Entity.UID;
            Item2.OwnerName = owner.Entity.Name;
            Item2.Date = Item.Date;
            Item2.DaysLeft = 0;
            gainer.ClaimableItem.Add(Item2.UID, Item2);
            gainer.Send(Item2);

            add = new ItemAdding(true);
            if (Item2.Item.Purification.Available)
                add.Append(Item.Item.Purification);
            if (Item2.Item.ExtraEffect.Available)
                add.Append(Item.Item.ExtraEffect);
            if (Item2.Item.Purification.Available || Item2.Item.ExtraEffect.Available)
                gainer.Send(add);

            MySqlCommand cmd2 = new MySqlCommand(MySqlCommandType.DELETE).Delete("detaineditems", "ItemUID", Item.ItemUID);
            cmd2.Execute();
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("detaineditems"))
                cmd.Insert("ItemUID", item.UID).Insert("Date", Item.Date.Ticks)
                .Insert("ConquerPointsCost", Item.ConquerPointsCost).Insert("OwnerUID", owner.Entity.UID)
                .Insert("OwnerName", owner.Entity.Name).Insert("GainerUID", gainer.Entity.UID)
                .Insert("GainerName", gainer.Entity.Name).Execute();
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("claimitems"))
                cmd.Insert("ItemUID", item.UID).Insert("Date", Item.Date.Ticks)
                    .Insert("ConquerPointsCost", Item.ConquerPointsCost).Insert("OwnerUID", owner.Entity.UID)
                    .Insert("OwnerName", owner.Entity.Name).Insert("GainerUID", gainer.Entity.UID)
                    .Insert("GainerName", gainer.Entity.Name).Execute();
        }

        public static void Claim(DetainedItem item, Client.GameState owner)
        {
            MySqlCommand cmd = new MySqlCommand(MySqlCommandType.DELETE).Delete("detaineditems", "ItemUID", item.ItemUID);
            cmd.Execute();
        }

        public static uint CalculateCost(ConquerItem item)
        {
            int basic = 10;
            if (item.ID % 10 == 9)
                basic += 50;
            if (item.ID / 100000 == 4 || item.ID / 100000 == 5)
            {
                if (item.SocketOne != MTA.Game.Enums.Gem.NoSocket)
                    basic += 100;
                if (item.SocketTwo != MTA.Game.Enums.Gem.NoSocket)
                    basic += 400;
            }
            else
            {
                if (item.SocketOne != MTA.Game.Enums.Gem.NoSocket)
                    basic += 400;
                if (item.SocketTwo != MTA.Game.Enums.Gem.NoSocket)
                    basic += 1600;
            }
            basic += item.Plus * 100;
            return (uint)basic;
        }
    }
}
