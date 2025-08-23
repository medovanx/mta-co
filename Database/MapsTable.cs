using System.Collections.Generic;
using System;


namespace MTA.Database
{
    public class MapsTable
    {
        public struct MapInformation
        {
            public ushort ID;
            public ushort BaseID;
            public uint Status;
            public uint Weather;
            public uint RaceRecord;


            public uint Owner;
            public uint HouseLevel;
            public uint Box;
            public uint BoxX;
            public uint BoxY;
        }
        public static CareDictionary<ushort, MapInformation> MapInformations = new CareDictionary<ushort, MapInformation>(280);
        private static IniFile RaceRecords;
        public static void Load()
        {
            RaceRecords = new IniFile(Constants.DatabaseBasePath + "racerecords.ini", "record");
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("maps"))
            using (var reader = cmd.CreateReader())
            {
                while (reader.Read())
                {
                    MapInformation info = new MapInformation();
                    info.ID = reader.ReadUInt16("Id");
                    info.BaseID = reader.ReadUInt16("Mapdoc");
                    info.Status = reader.ReadUInt32("Type");
                    info.Weather = reader.ReadUInt32("Weather");
                    info.RaceRecord = RaceRecords[info.ID, 0].Cast<uint>();
                    MapInformations.Add(info.ID, info);
                }
            }
            Console.WriteLine("Map informations loaded.");
        }

        internal static void SaveRecord(MapInformation key)
        {
            RaceRecords[key.ID] = key.RaceRecord;
        }
    }
}
