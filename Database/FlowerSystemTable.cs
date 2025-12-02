using System;
using System.Collections.Generic;

using System.Linq;
using MTA.Client;
using MTA.Game.Features.Flowers;

namespace MTA.Database
{

    public class FlowerSystemTable
    {
        public static uint[] TopLilies = new uint[2];
        public static uint[] TopOrchids = new uint[2];
        public static List<MTA.Game.Features.Flowers.Flowers> flowertoday = new List<MTA.Game.Features.Flowers.Flowers>();
        public static uint[] TopRedRoses = new uint[2];
        public static uint[] TopTulips = new uint[2];
        private static bool Exists(uint id)
        {
            try
            {
                using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("flowers").Where("id", id))
                using (var reader = new MySqlReader(cmd))
                {
                    if (reader.Read())
                    {
                        return true;
                    }
                }
            }
            catch
            {

            }
            return false;
        }

        public static void Flowers(Client.GameState Client)
        {
            Client.Entity.Flowers = new MTA.Game.Features.Flowers.Flowers();
            if (!Exists(Client.Entity.UID))
            {
                Insert(Client);
            }
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("flowers").Where("id", Client.Entity.UID))
            using (var reader = new MySqlReader(cmd))
            {
                if (reader.Read())
                {
                    Client.Entity.Flowers.id = reader.ReadUInt32("id");
                    try
                    {
                        Client.Entity.Flowers.LastFlowerSent = DateTime.FromBinary(reader.ReadInt64("last_flower_sent"));
                    }
                    catch
                    {
                    }
                    if ((Client.Entity.Body == 0x7d1) || (Client.Entity.Body == 0x7d2 || Client.Entity.Body == 1002) || (Client.Entity.Body == 1002))
                    {
                        if (Client.Entity.Flowers.LastFlowerSent.AddDays(1) <= DateTime.Now)
                        {
                            Client.Entity.Flowers.LastFlowerSent = DateTime.Now;
                            Client.Entity.Flowers.RedRoses2day = 0;
                            Client.Entity.Flowers.Lilies2day = 0;
                            Client.Entity.Flowers.Tulips2day = 0;
                            Client.Entity.Flowers.Orchads2day = 0;
                        }
                        else
                        {
                            Client.Entity.Flowers.RedRoses2day = reader.ReadUInt32("redrosestoday");
                            Client.Entity.Flowers.Lilies2day = reader.ReadUInt32("liliestoday");
                            Client.Entity.Flowers.Tulips2day = reader.ReadUInt32("tulipstoday");
                            Client.Entity.Flowers.Orchads2day = reader.ReadUInt32("orchadstoday");
                        }
                        Client.Entity.Flowers.RedRoses = reader.ReadUInt32("redroses");
                        Client.Entity.Flowers.Lilies = reader.ReadUInt32("lilies");
                        Client.Entity.Flowers.Tulips = reader.ReadUInt32("tulips");
                        Client.Entity.Flowers.Orchads = reader.ReadUInt32("orchads");
                        Client.Entity.Flowers.Name = reader.ReadString("name");
                        if (flowertoday.Contains(Client.Entity.Flowers))
                        {
                            flowertoday.Remove(Client.Entity.Flowers);
                        }
                        flowertoday.Add(Client.Entity.Flowers);
                    }
                }
            }
        }

        public struct Flowerss
        {
            public uint RedRoses;
            public string name;
            public uint id;
            public uint Orchads;
            public uint Tulips;
            public uint Lilies;
            public short Body;
        }
        public static List<Flowerss> FlowerList = new List<Flowerss>();
        public static void Load()
        {

            Flowerss flower = new Flowerss();
            FlowerList.Clear();
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("flowers"))
            using (var reader = new MySqlReader(cmd))
            {
                while (reader.Read())
                {
                    flower.id = reader.ReadUInt32("id");
                    flower.RedRoses = reader.ReadUInt32("redroses");
                    flower.Lilies = reader.ReadUInt32("lilies");
                    flower.Tulips = reader.ReadUInt32("tulips");
                    flower.Orchads = reader.ReadUInt32("orchads");
                    flower.name = reader.ReadString("name");
                    flower.Body = reader.ReadInt16("body");
                    if (flower.Body == 2001 || flower.Body == 2002 || flower.Body == 1002 || flower.Body == 1001)
                    {
                        FlowerList.Add(flower);
                    }
                }
            }
        }

        public static void Insert(Client.GameState client)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.INSERT).Insert("flowers"))
                cmd.Insert("id", client.Entity.UID)
             .Insert("redroses", client.Entity.Flowers.RedRoses)
             .Insert("redrosestoday", client.Entity.Flowers.RedRoses2day)
             .Insert("lilies", client.Entity.Flowers.Lilies)
             .Insert("liliestoday", client.Entity.Flowers.Lilies2day)
             .Insert("tulips", client.Entity.Flowers.Tulips)
             .Insert("tulipstoday", client.Entity.Flowers.Tulips2day)
             .Insert("orchads", client.Entity.Flowers.Orchads)
             .Insert("orchadstoday", client.Entity.Flowers.Orchads2day)
             .Insert("last_flower_sent", DateTime.Now.Subtract(TimeSpan.FromDays(1)).ToBinary())
             .Insert("name", client.Entity.Name)
             .Insert("body", client.Entity.Body).Execute();
        }

        public static void SaveFlowerTable(Client.GameState client)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update("flowers")
               .Set("redroses", client.Entity.Flowers.RedRoses)
               .Set("redrosestoday", client.Entity.Flowers.RedRoses2day)
               .Set("lilies", client.Entity.Flowers.Lilies)
               .Set("liliestoday", client.Entity.Flowers.Lilies2day)
               .Set("tulips", client.Entity.Flowers.Tulips)
               .Set("tulipstoday", client.Entity.Flowers.Tulips2day)
               .Set("orchads", client.Entity.Flowers.Orchads)
               .Set("last_flower_sent", client.Entity.Flowers.LastFlowerSent.ToBinary())
               .Set("orchadstoday", client.Entity.Flowers.Orchads2day).Where("id", client.Entity.UID).Execute();

        }
    }
}