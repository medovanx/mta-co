using System;
using System.Collections.Generic;
using System.IO;

namespace MTA.Database
{
    public static class HonorShop
    {
        public const uint UID = 6000;

        public static HonorShopBase Shop;
        public class HonorShopBase
        {
            public int Count { get { return Items.Count; } }
            public Dictionary<uint, uint> Items;
        }
        public static void Load()
        {
            string[] text = File.ReadAllLines(Constants.HonorShopPath);
            Shop = new HonorShopBase();
            for (int x = 0; x < text.Length; x++)
            {
                string line = text[x];
                if (line.StartsWith("Item"))
                {
                    line = line.Remove(0, 11);
                    Shop.Items = new Dictionary<uint, uint>(int.Parse(line));
                }
                else if (line.StartsWith("Recommend"))
                {
                    break;
                }
                else
                {
                    if (line.Length > 3)
                    {
                        string[] Parts = line.Split(',');
                        uint itemID = uint.Parse(Parts[0]);
                        uint cost = uint.Parse(Parts[1]);
                        Shop.Items.Add(itemID, cost);
                    }
                }
            }
            Console.WriteLine("Honor shop information loaded.");
        }
    }
}
