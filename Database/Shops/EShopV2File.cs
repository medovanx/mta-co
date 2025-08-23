using System;
using System.Collections.Generic;
using System.IO;

namespace MTA.Database
{
    public static class EShopV2File
    {
        public static void Load()
        {
            string[] text = File.ReadAllLines(Constants.EShopsV2Path);
            ShopFile.Shop shop = new ShopFile.Shop();
            for (int x = 0; x < text.Length; x++)
            {
                string line = text[x].Trim().Replace("_", " ");
                if (line == "")
                    continue;
                string[] split = line.Split(new string[] { " ", "=" }, StringSplitOptions.RemoveEmptyEntries);

                if (split[0] == "ID")
                {
                    uint id = uint.Parse(split[1]);
                    if (EShopFile.Shops.ContainsKey(id))
                        shop = EShopFile.Shops[uint.Parse(split[1])];
                    else
                    {
                        shop = new ShopFile.Shop();
                        shop.Items = new List<uint>();
                        shop.UID = id;
                        EShopFile.Shops.Add(id, shop);
                    }
                }
                else if (split[0] == "MoneyType")
                {
                    shop.MoneyType = (ShopFile.MoneyType)byte.Parse(split[1]);
                }
                else if (split[0].Contains("Item") || split[0].Contains("item"))
                {
                    if (split[0].StartsWith("Item") || split[0].StartsWith("item"))
                    {
                        uint ID = uint.Parse(split[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                        if (!shop.Items.Contains(ID)) shop.Items.Add(ID);
                    }
                    else
                    {
                        uint ID = uint.Parse(split[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                        if (!shop.Items.Contains(ID)) shop.Items.Add(ID);
                    }
                }
                else if (split[0].Length != 0 && split[0] != "[recommend]" && split[0] != "Amount")
                {
                    uint ID = uint.Parse(split[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                    if (!shop.Items.Contains(ID)) shop.Items.Add(ID);
                }
            }
            Console.WriteLine("EShopsV2 information loaded.");
        }
    }
}
