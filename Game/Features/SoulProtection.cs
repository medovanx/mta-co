using System;
using System.Collections.Generic;
using System.IO;
using MTA.Network;

namespace MTA.MaTrix {
    public class SoulProtection {
        public static SafeDictionary<uint, SOulInfo> Soul_Protections = new SafeDictionary<uint, SOulInfo>();

        public static void Load() {
            string[] lines = File.ReadAllLines(Constants.SoulProtectionPath);
            foreach (var item in lines) {
                try {
                    var coloums = item.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    SOulInfo info = new SOulInfo();
                    info.UID = uint.Parse(coloums[0]);
                    info.ItemType = uint.Parse(coloums[1]);
                    info.Pos = (PacketHandler.Positions)uint.Parse(coloums[2]);
                    if (!Soul_Protections.ContainsKey(info.UID))
                        Soul_Protections.Add(info.UID, info);
                }
                catch (Exception e) {
                    Console.WriteLine(e.ToString());
                    throw;
                }
            }

            Console.WriteLine(Soul_Protections.Count + " Soul_Protections loaded successfully.");
        }

        public struct SOulInfo {
            public uint UID;
            public uint ItemType;
            public PacketHandler.Positions Pos;
        }
    }
}