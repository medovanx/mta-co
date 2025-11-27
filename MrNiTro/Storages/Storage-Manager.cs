using System;
using System.Collections.Generic;
using Throne.World.Database.Information.Files;

namespace MTA
{
    public sealed class StorageManager
    {
        private static StorageInfo Info;
        public static Dictionary<uint, MTA.Network.GamePackets.WardrobeTitles> Data;

        public static void Load()
        {
            Storage.Read(out Info);
            Data = new Dictionary<uint, Network.GamePackets.WardrobeTitles>();
            using (var cmd = new Database.MySqlCommand(Database.MySqlCommandType.SELECT))
            {
                cmd.Select("Titles");
                var reader = cmd.CreateReader();
                while (reader.Read())
                {
                    var title = new MTA.Network.GamePackets.WardrobeTitles();
                    title.Id = reader.ReadUInt32("Id");
                    title.Points = reader.ReadInt32("Points");
                    title.Data = reader.ReadBlob("Data");
                    Data.Add(title.Id, title);
                }
            }
            Console.WriteLine("storageManager By jiMMy loaded.");
        }

        public static T Wing<T>(int _type, int _id)
        {
            object value = null;
            int trash = 0;
            if (typeof(T) == typeof(bool))
            {
                value = int.TryParse(Info.GetUnitByID(_id, Info.GetStorageByType(_type)).Param, out trash);
            }
            else if (typeof(T) == typeof(int))
            {
                var myType = _type.ToString();
                var myID = _id.ToString();


                while (myID.Length < 4)
                    myID = "0" + myID;
                value = int.Parse(myType + myID);
            }
            else
                throw new Exception("Unknow type : " + typeof(T).Name);
            return (T)Convert.ChangeType(value, typeof(T));
        }
        public static T Title<T>(int _type, int _id)
        {
            object value = null;
            int trash = 0;
            if (typeof(T) == typeof(bool))
            {
                value = !int.TryParse(Info.GetUnitByID(_id, Info.GetStorageByType(_type)).Param, out trash);
            }
            else if (typeof(T) == typeof(int))
            {
                var myType = _type.ToString();
                var myID = _id.ToString();

                while (myID.Length < 4)
                    myID = "0" + myID;
                value = int.Parse(myType + myID);
            }
            else
                throw new Exception("Unknow type : " + typeof(T).Name);
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static Network.GamePackets.WardrobeTitles Find(Func<Network.GamePackets.WardrobeTitles, bool> predicate)
        {
            foreach (var title in Data.Values)
            {
                if (predicate(title))
                    return title;
            }
            return null;
        }

        public static int GetTitlePoints(short _type, short _id)
        {
            if (_type == 1 && _id == 1000)
                return 150;
            else if (_type == 2018 && _id == 1)
                return 500;
            else if (_type == 2001 && _id == 2)
                return 300;
            else if (_type == 2002 && _id == 3)
                return 150;
            else if (_type == 2003 && _id == 4)
                return 300;
            else if (_type == 2004 && _id == 5)
                return 150;
            else if (_type == 2005 && _id == 6)
                return 150;
            else if (_type == 2006 && _id == 7)
                return 150;
            else if (_type == 2020 && _id == 2020)
                return 300;
            else if (_type == 2021 && _id == 2021)
                return 200;
            else if (_type == 2022 && _id == 2022)
                return 100;
            else if (_type == 2023 && _id == 2023)
                return 300;
            else if (_type == 2024 && _id == 2024)
                return 200;
            else if (_type == 2025 && _id == 2025)
                return 100;
            else if (_type == 2028 && _id == 2028)
                return 150;
            else if (_type == 2029 && _id == 2029)
                return 300;
            else if (_type == 2030 && _id == 2030)
                return 100;
            else if (_type == 2031 && _id == 2031)
                return 200;
            else if (_type == 6009 && _id == 6009)
                return 300;
            else if (_type == 6007 && _id == 6007)
                return 300;
            else if (_type == 6008 && _id == 6008)
                return 300;
            else if (_type == 2026 && _id == 2026)
                return 100;
            else if (_type == 2027 && _id == 2027)
                return 300;
            else if (_type == 2032 && _id == 2032)
                return 300;
            else if (_type == 2033 && _id == 2033)
                return 300;
            else if (_type == 6011 && _id == 6011)
                return 300;
            else if (_type == 2034 && _id == 2034)
                return 300;
            else if (_type == 2013 && _id == 14)
                return 300;
            else if (_type == 2014 && _id == 15)
                return 300;
            else if (_type == 2015 && _id == 16)
                return 300;
            else if (_type == 2016 && _id == 17)
                return 300;
            else if (_type == 2035 && _id == 2035)
                return 300;
            else if (_type == 2036 && _id == 2036)
                return 300;
            else if (_type == 2037 && _id == 2037)
                return 300;
            else if (_type == 2038 && _id == 2038)
                return 300;
            else if (_type == 2039 && _id == 2039)
                return 300;
            else if (_type == 2040 && _id == 2040)
                return 100;
            else if (_type == 2041 && _id == 2041)
                return 100;
            else if (_type == 2044 && _id == 2044)
                return 100;
            else if (_type == 2045 && _id == 2045)
                return 100;
            else if (_type == 6012 && _id == 6012)
                return 100;
            else if (_type == 2050 && _id == 2050)
                return 100;
            else if (_type == 2051 && _id == 2051)
                return 100;
            else if (_type == 2052 && _id == 2052)
                return 100;
            else if (_type == 2053 && _id == 2053)
                return 150;
            else if (_type == 2054 && _id == 2054)
                return 150;
            else if (_type == 2057 && _id == 2057)
                return 150;
            else if (_type == 2056 && _id == 2056)
                return 100;
            else if (_type == 2046 && _id == 2046)
                return 150;
            else if (_type == 2047 && _id == 2047)
                return 150;
            else if (_type == 2048 && _id == 2048)
                return 150;
            else if (_type == 2049 && _id == 2049)
                return 150;
            else if (_type == 2059 && _id == 2059)
                return 100;
            else if (_type == 2060 && _id == 2060)
                return 150;
            else if (_type == 2061 && _id == 2061)
                return 150;
            else if (_type == 2062 && _id == 2062)
                return 150;
            else if (_type == 6013 && _id == 6013)
                return 100;
            else if (_type == 6014 && _id == 6014)
                return 150;
            else if (_type == 6015 && _id == 6015)
                return 150;
            else if (_type == 6016 && _id == 6016)
                return 150;
            else if (_type == 6017 && _id == 6017)
                return 150;
            return 0;
        }
    }
}
