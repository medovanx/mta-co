using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace MTA.MaTrix
{
    public class SoulProtection
    {
        public struct SOulInfo
        {
            public uint UID;
            public uint ItemType;
            public Network.PacketHandler.Positions Pos;
        }
        public static SafeDictionary<uint, SOulInfo> Soul_Protections = new SafeDictionary<uint, SOulInfo>();
        public static void Load()
        {
            if (File.Exists(Constants.DataHolderPath + "souls_protection.txt"))
            {
                string[] lines = File.ReadAllLines(Constants.DataHolderPath + "souls_protection.txt");
                foreach (var item in lines)
                {
                    try
                    {

                        var coloums = item.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        SOulInfo info = new SOulInfo();
                        info.UID = uint.Parse(coloums[0]);
                        info.ItemType = uint.Parse(coloums[1]);
                        info.Pos = (Network.PacketHandler.Positions)uint.Parse(coloums[2]);
                        if (!Soul_Protections.ContainsKey(info.UID))
                            Soul_Protections.Add(info.UID, info);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        throw;
                    }
                   
                }

                Console.WriteLine(Soul_Protections.Count + " Soul_Protections loaded successfully.");
            }
        }
    }
}
