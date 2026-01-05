using MTA.Database;
using MTA.Game.Features.House.Database.Mappers;
using MTA.Game.Features.House.Database.Schema;
using MTA.Network.GamePackets;

namespace MTA.Game.Features.House.Database;

public static class FurnitureTable {
    public static void Load() {
        using var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select(FurnitureSchema.Tables.FurnitureTable);
        using var reader = new MySqlReader(cmd);

        while (reader.Read()) {
            var record = FurnitureMappers.MapFurniture(reader);
            var info = new Furniture.FurnitureInfo {
                NpcId = record.NpcId,
                Type = (Enums.NpcType)record.Type,
                Mesh = record.Mesh,
                Map = record.Map,
                X = record.X,
                Y = record.Y,
                ItemId = record.ItemId,
                Price = record.Price
            };

            if (!Furniture.FurnitureVendors.ContainsKey(info.NpcId))
                Furniture.FurnitureVendors.Add(info.NpcId, info);
            if (!Furniture.FurnitureItems.ContainsKey(info.ItemId))
                Furniture.FurnitureItems.Add(info.ItemId, info.Mesh);
            if (!Kernel.Maps.ContainsKey(info.Map)) continue;

            Kernel.Maps[info.Map].Npcs.Remove(info.NpcId);

            Kernel.Maps[info.Map].AddNpc(new NpcSpawn {
                Type = info.Type,
                UID = info.NpcId,
                MapID = info.Map,
                Mesh = info.Mesh,
                X = info.X,
                Y = info.Y
            });
        }

        Console.WriteLine(
            $"[Furniture] {Furniture.FurnitureVendors.Count} furniture items loaded successfully from database.");
    }
}