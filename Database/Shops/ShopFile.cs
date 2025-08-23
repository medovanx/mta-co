using System;
using System.Collections.Generic;
using System.IO;

namespace MTA.Database
{
    public static class ShopFile
    {
        public static Dictionary<uint, Shop> Shops;
        public class Shop
        {
            public uint UID;
            public MoneyType MoneyType;
            public int Count { get { return Items.Count; } }
            public List<uint> Items;
        }
        public static void Load()
        {
            string[] text = File.ReadAllLines(Constants.ShopsPath);
            Shop shop = new Shop();
            for (int x = 0; x < text.Length; x++)
            {
                string line = text[x];
                string[] split = line.Split('=');
                if (split[0] == "Amount")
                    Shops = new Dictionary<uint, Shop>(int.Parse(split[1]));
                else if (split[0] == "ID")
                {
                    uint id = uint.Parse(split[1]);
                    if (Shops.ContainsKey(id))
                        shop = Shops[uint.Parse(split[1])];
                    else
                    {
                        shop = new Shop();
                        shop.Items = new List<uint>();
                        shop.UID = id;
                        Shops.Add(id, shop);
                    }
                    //if (shop.UID == 0)
                    //    shop.UID = uint.Parse(split[1]);
                    //else
                    //{
                    //    if (!Shops.ContainsKey(shop.UID))
                    //    {
                    //        Shops.Add(shop.UID, shop);
                    //        shop = new Shop();
                    //        shop.UID = uint.Parse(split[1]);
                    //    }
                    //}
                }
                else if (split[0] == "MoneyType")
                {
                    shop.MoneyType = (MoneyType)byte.Parse(split[1]);
                }
                else if (split[0] == "ItemAmount")
                {
                    shop.Items = new List<uint>(ushort.Parse(split[1]));
                }
                else if (split[0].Contains("Item") && split[0] != "ItemAmount")
                {
                    uint ID = uint.Parse(split[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                    if (!shop.Items.Contains(ID))
                        shop.Items.Add(ID);
                }
            }
            if (!Shops.ContainsKey(shop.UID))
                Shops.Add(shop.UID, shop);
            Console.WriteLine("Shops information loaded.");
        }

        public enum MoneyType
        {
            Gold = 0,
            ConquerPoints = 1,
            HonorPoints = 2
        }
    }
}
