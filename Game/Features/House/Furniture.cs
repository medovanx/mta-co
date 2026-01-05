using System.Collections.Generic;
using MTA.Game.Features.House.Database;

namespace MTA.Game.Features.House;

public static class Furniture {
    public static SafeDictionary<uint, ushort> FurnitureItems = new();
    public static SafeDictionary<uint, FurnitureInfo> FurnitureVendors = new();

    public static void Load() {
        FurnitureTable.Load();
    }

    public struct FurnitureInfo {
        public uint NpcId;
        public Enums.NpcType Type;
        public ushort Mesh;
        public ushort Map;
        public ushort X;
        public ushort Y;
        public uint ItemId;
        public uint Price;
    }
}