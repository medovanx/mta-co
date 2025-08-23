using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MTA.Game;
using MTA.Interfaces;
using MTA.Network.GamePackets;

namespace MTA.Database
{
    public class Furniture
    {
        public struct FurInfo
        {
            public uint npcid;
            public Enums.NpcType type;
            public ushort mesh;
            public ushort map;
            public ushort x;
            public ushort y;
            public uint itemid;

        }
        public static SafeDictionary<uint, ushort> FurnituresItems = new SafeDictionary<uint, ushort>();       
        public static SafeDictionary<uint, FurInfo> Furnitures = new SafeDictionary<uint, FurInfo>();
        public static void Load()
        {
            if (File.Exists(Constants.DataHolderPath + "Furniture.txt"))
            {
                string[] lines = File.ReadAllLines(Constants.DataHolderPath + "Furniture.txt");
                foreach (var item in lines)
                {
                    var coloums = item.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    FurInfo info = new FurInfo();
                    info.npcid = uint.Parse(coloums[0]);
                    info.type = (Enums.NpcType)uint.Parse(coloums[1]);
                    info.mesh = ushort.Parse(coloums[2]);
                    info.map = ushort.Parse(coloums[3]);
                    info.x = ushort.Parse(coloums[4]);
                    info.y = ushort.Parse(coloums[5]);
                    info.itemid = uint.Parse(coloums[6]);
                    if (!Furnitures.ContainsKey(info.npcid))
                        Furnitures.Add(info.npcid, info);
                    if (!FurnituresItems.ContainsKey(info.itemid))
                        FurnituresItems.Add(info.itemid, info.mesh);
                    if (Kernel.Maps.ContainsKey(info.map))
                    {
                        if (Kernel.Maps[info.map].Npcs.ContainsKey(info.npcid))
                            Kernel.Maps[info.map].Npcs.Remove(info.npcid);
                        Kernel.Maps[info.map].AddNpc(new Network.GamePackets.NpcSpawn() { Type= info.type, UID = info.npcid, MapID = info.map, Mesh = info.mesh, X = info.x, Y = info.y });
                    }
                                        
                }

                Console.WriteLine(Furnitures.Count + " Furnitures loaded successfully.");
            }
           
        }
        
    }
}
