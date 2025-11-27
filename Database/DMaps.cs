using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections;

namespace MTA.Database
{
    public class DMaps
    {
        public static Hashtable MapOwner = new Hashtable();
        public static SafeDictionary<ushort, string> MapPaths = new SafeDictionary<ushort, string>(280);
        public static void Load()
        {
            if (File.Exists(Constants.DataHolderPath + "GameMap.dat"))
            {
                Time32 start = Time32.Now;
                FileStream FS = new FileStream(Constants.DataHolderPath + "GameMap.dat", FileMode.Open);
                BinaryReader BR = new BinaryReader(FS);
                uint MapCount = BR.ReadUInt32();
                for (uint i = 0; i < MapCount; i++)
                {
                    ushort MapID = (ushort)BR.ReadUInt32();
                    string Path = Program.Encoding.GetString(BR.ReadBytes(BR.ReadInt32()));
                    if (MapID >= 1712 && MapID <= 1720)
                    {
                        BR.ReadUInt32();
                        continue;
                    }
                    if (Path.Contains(".7z"))
                    {
                        Path = System.IO.Path.ChangeExtension(Path, ".dmap");
                    }
                    MapsTable.MapInformation info = new MapsTable.MapInformation();
                    if (MapsTable.MapInformations.ContainsKey(MapID))
                        info = MapsTable.MapInformations[MapID];
                    if (info.ID == info.BaseID)
                        new Game.Map(MapID, Path);
                    MapPaths.Add(MapID, Path);
                    BR.ReadInt32();
                }
                foreach (var kval in MapsTable.MapInformations.Values)
                    if (kval.BaseID != kval.ID)
                        new Game.Map(kval.ID, kval.BaseID, MapPaths[kval.BaseID]);
                BR.Close();
                FS.Close();
                Console.WriteLine(MapPaths.Count + " Game maps loaded successfully.");
            }
            else
                Console.WriteLine("The specified Lucky folder doesn't exist. Game map couldn't be loaded.");
        }
        public static void LoadHouses()
        {
            if (System.IO.File.Exists(Constants.DataHolderPath + "DMapOwner.dat"))
            {
                FileStream FS = new FileStream(Constants.DataHolderPath + "DMapOwner.dat", FileMode.Open);
                BinaryReader BR = new BinaryReader(FS);

                try
                {
                    int MapCount = BR.ReadInt32();
                    for (int i = 0; i < MapCount; i++)
                    {
                        uint Owner = BR.ReadUInt32();
                        ushort Mapid = BR.ReadUInt16();
                        ushort StMapid = BR.ReadUInt16();
                        bool Success = CreateDynamicMap(Mapid, StMapid, Owner);
                    }
                }
                catch { }
                BR.Close();
                FS.Close();
            }
        }
        public static bool CreateDynamicMap(ushort mapadd, ushort mapneed, uint ownerid)
        {
            bool addedmap = false;
            if (DMaps.MapOwner.Contains(Convert.ToInt32(ownerid)))
                return false;
            while (Kernel.Maps.ContainsKey(mapadd))
            {
                mapadd++;
            }
            FileStream FS = new FileStream(Constants.DataHolderPath + "GameMap.dat", FileMode.Open);
            BinaryReader BR = new BinaryReader(FS);
            uint MapCount = BR.ReadUInt32();
            for (uint i = 0; i < MapCount; i++)
            {
                ushort MapID = (ushort)BR.ReadUInt32();
                string Path = Program.Encoding.GetString(BR.ReadBytes(BR.ReadInt32()));
                if (mapneed == MapID)
                {
                    ushort NewMapID = mapadd;
                    Game.Map D = new Game.Map(NewMapID, Path);
                    Kernel.Maps.Add(NewMapID, D);
                    //Kernel.Maps.Add(NewMapID, mapneed);
                    MapOwner.Add(Convert.ToInt32(ownerid), NewMapID);
                    addedmap = true;
                    break;
                }
                BR.ReadInt32();
            }
            BR.Close();
            FS.Close();
            return addedmap;
        }
        public static bool DeleteDynamicMap(ushort mapadd, uint ownerid)
        {
            bool deletedmap = false;
            if (!DMaps.MapOwner.Contains(Convert.ToInt32(ownerid)))
                return false;

            ushort NewMapID = mapadd;
            Kernel.Maps.Remove(NewMapID);
            MapOwner.Remove(Convert.ToInt32(ownerid));
            deletedmap = true;

            return deletedmap;
        }
        public static ushort GetHouseID(uint owner)
        {
            int key = Convert.ToInt32(owner);
            if (MapOwner.Contains(key))
            {
                return Convert.ToUInt16(MapOwner[key]);
            }
            return 0;
        }
        public static bool HouseUpgrade(ushort Map)
        {
            ushort STHseID = Convert.ToUInt16(Kernel.Maps[Map]);
            if (STHseID == 1099)
                return true;
            return false;
        }
        public static void Save()
        {
            FileStream FS = new FileStream(Constants.DataHolderPath + "DMapOwner.dat", FileMode.OpenOrCreate);
            BinaryWriter BW = new BinaryWriter(FS);
            BW.Write(MapOwner.Count);
            foreach (DictionaryEntry Map in MapOwner)
            {
                BW.Write(Convert.ToUInt32(Map.Key));
                BW.Write(Convert.ToUInt16(Map.Value));
                //BW.Write(Convert.ToUInt16(Kernel.Maps.[Map.Value]));
            }
            BW.Close();
            FS.Close();
        }
    }
}
