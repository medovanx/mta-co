using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MTA.Database
{
    public class AddingInformationTable
    {
        public struct SoulGearInformation
        {
            public uint ID;
            public int ItemIdentifier;
        }
        public static SafeDictionary<uint, SoulGearInformation> SoulGearItems = new SafeDictionary<uint, SoulGearInformation>();

        public static void Load()
        {
            string[] data = File.ReadAllLines(Constants.SoulGearInformation);
            foreach (var line in data)
            {
                string[] datas = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                SoulGearInformation info = new SoulGearInformation();
                info.ID = uint.Parse(datas[0]);
                info.ItemIdentifier = int.Parse(datas[1]);
                SoulGearItems.Add(info.ID, info);
            }
        }
    }
}
