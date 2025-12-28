using System.Runtime.InteropServices;
using MTA.Extensions;
using MTA.Game.Constants;

namespace MTA.Database {
    public static class Storage {
        public static void Read(out StorageInfo storageInfo) {
            storageInfo = new StorageInfo();
            var reader = new IniFile(GameConstants.StoragePath);
            {
                storageInfo.Count = reader.ReadInt32("Storage", "StorageTypeCount", 0);
                storageInfo.Storages = new StorageInfo.Storage[storageInfo.Count];
                for (int i = 0; i < storageInfo.Count; i++) {
                    var mySection = (i + 1).ToString();
                    storageInfo.Storages[i] = new StorageInfo.Storage();
                    storageInfo.Storages[i].Type = reader.ReadInt32(mySection, "Type", 0);
                    storageInfo.Storages[i].UnitCount = reader.ReadInt32(mySection, "UnitCount", 0);
                    storageInfo.Storages[i].ViewType = reader.ReadInt32(mySection, "ViewType", 0);
                    storageInfo.Storages[i].Units = new StorageInfo.Storage.Unit[storageInfo.Storages[i].UnitCount];
                    for (int u = 0; u < storageInfo.Storages[i].UnitCount; u++) {
                        var unitSection = mySection + "-" + (u + 1).ToString();
                        storageInfo.Storages[i].Units[u] = new StorageInfo.Storage.Unit();
                        storageInfo.Storages[i].Units[u].ID = reader.ReadInt32(unitSection, "ID", 0);
                        storageInfo.Storages[i].Units[u].AniSection = reader.ReadString(unitSection, "AniSection");
                        storageInfo.Storages[i].Units[u].GetWayType3 = reader.ReadInt32(unitSection, "GetWayType3", 0);
                        storageInfo.Storages[i].Units[u].GetWayText3 = reader.ReadString(unitSection, "GetWayText3");
                        storageInfo.Storages[i].Units[u].Param = reader.ReadString(unitSection, "Param");
                        storageInfo.Storages[i].Units[u].Intro = reader.ReadString(unitSection, "Intro");
                    }
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class StorageInfo {
        public int Count;
        public Storage[] Storages;

        public class Storage {
            public int Type;
            public int UnitCount;
            public Unit[] Units;
            public int ViewType;

            public class Unit {
                public string AniSection;
                public string GetWayText3;
                public int GetWayType3;
                public int ID;
                public string Intro;
                public string Param;
            }
        }

        public Storage GetStorageByType(int Type) {
            foreach (var storage in Storages)
                if (storage != null && storage.Type == Type)
                    return storage;
            return null;
        }

        public Storage.Unit GetUnitByID(int ID, Storage _storage = null) {
            if (_storage != null) {
                foreach (var unit in _storage.Units)
                    if (unit.ID == ID)
                        return unit;
            }
            else {
                foreach (var storage in Storages)
                    if (storage != null)
                        foreach (var unit in storage.Units)
                            if (unit.ID == ID)
                                return unit;
            }

            return null;
        }
    }
}