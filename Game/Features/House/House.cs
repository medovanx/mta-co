using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MTA.Client;
using MTA.Game.Constants;
using MTA.Game.Features.House.Database;
using MTA.Game.Features.House.Database.Models;
using MTA.Network.GamePackets;
using Warehouse = MTA.Game.ConquerStructures.Warehouse;

namespace MTA.Game.Features.House;

public static class House {
    public static SafeDictionary<uint, HouseInfo> Houses = [];

    /// <summary>
    ///     Initializes all player houses from the database at server startup
    /// </summary>
    public static void LoadHouses() {
        HouseTable.Load();
    }

    /// <summary>
    ///     Persists the player's furniture layout to the database
    /// </summary>
    public static void SaveFurniture(GameState client) {
        if (!Houses.TryGetValue(client.Entity.UID, out var info))
            return;
        HouseTable.SaveFurniture(client, info);
    }

    /// <summary>
    ///     Creates a new level 1 house for the player
    /// </summary>
    public static void CreateHouse(GameState client) {
        HouseInfo info = new() {
            Uid = client.Entity.UID,
            Name = client.Entity.Name,
            Id = (ushort)client.Entity.UID,
            MapType = Maps.HOUSE_LV1,
            Level = 1,
            Furniture = []
        };
        if (!Houses.ContainsKey(info.Uid))
            Houses.Add(info.Uid, info);
        _ = new Map(info.Id, info.MapType, Kernel.Maps[info.MapType].Path);

        HouseTable.Create(client, info);
    }

    /// <summary>
    ///     Upgrades the player's house to the next level, changing the map type accordingly
    /// </summary>
    public static void UpgradeHouse(GameState client, byte level) {
        var newMapType = level switch {
            1 => Maps.HOUSE_LV2,
            2 => Maps.HOUSE_LV3,
            3 => Maps.HOUSE_LV4,
            4 => Maps.HOUSE_LV5,
            _ => Maps.HOUSE_LV1
        };

        level++;
        if (level > 5)
            return;

        HouseTable.Update(client, newMapType, level);

        if (Kernel.Maps.ContainsKey((ushort)client.Entity.UID)) {
            Kernel.Maps.Remove((ushort)client.Entity.UID);
            _ = new Map((ushort)client.Entity.UID, newMapType, Kernel.Maps[newMapType].Path);
        }

        if (!Houses.ContainsKey(client.Entity.UID)) return;
        Houses[client.Entity.UID].MapType = newMapType;
        Houses[client.Entity.UID].Level = level;
        SaveFurniture(client);
    }

    /// <summary>
    ///     Downgrades the player's house to the previous level, changing the map type accordingly
    /// </summary>
    public static void DowngradeHouse(GameState client, byte currentLevel) {
        if (currentLevel <= 1)
            return; // Cannot downgrade below level 1

        var newLevel = (byte)(currentLevel - 1);

        var newMapType = newLevel switch {
            1 => Maps.HOUSE_LV1,
            2 => Maps.HOUSE_LV2,
            3 => Maps.HOUSE_LV3,
            4 => Maps.HOUSE_LV4,
            _ => Maps.HOUSE_LV1
        };

        HouseTable.Update(client, newMapType, newLevel);

        if (Kernel.Maps.ContainsKey((ushort)client.Entity.UID)) {
            Kernel.Maps.Remove((ushort)client.Entity.UID);
            _ = new Map((ushort)client.Entity.UID, newMapType, Kernel.Maps[newMapType].Path);
        }

        if (!Houses.TryGetValue(client.Entity.UID, out var value)) return;
        value.MapType = newMapType;
        value.Level = newLevel;
        SaveFurniture(client);
    }

    /// <summary>
    ///     Teleports the player to their house at a random location
    /// </summary>
    public static void Teleport(GameState client, HouseInfo info) {
        client.Entity.AdvancedTeleport(true);
        var coordinates = Kernel.Maps[info.MapType].RandomCoordinates();
        var x = coordinates.Item1;
        var y = coordinates.Item2;
        if (client.Entity.EntityFlag == EntityFlag.Player)
            if (client.InQualifier())
                if (client.InQualifier())
                    if (client.Entity.MapID != 700 && client.Entity.MapID < 11000)
                        client.EndQualifier();

        client.Entity.X = x;
        client.Entity.Y = y;
        client.Entity.PX = 0;
        client.Entity.PY = 0;
        client.Entity.PreviousMapID = client.Entity.MapID;
        client.Entity.MapID = info.Id;

        Data data = new(true) {
            UID = client.Entity.UID,
            ID = Data.Teleport,
            dwParam = info.MapType,
            wParam1 = x,
            wParam2 = y
        };
        client.Send(data);
        client.Send(new MapStatus { BaseID = info.MapType, ID = info.Id });
        client.Entity.AdvancedTeleport(true);
    }

    /// <summary>
    ///     Initializes the house warehouse if the player has an item box furniture piece
    /// </summary>
    private static void HouseWarehouse(GameState client) {
        if (!Houses.TryGetValue(client.Entity.UID, out var info)) return;
        var itemBox = info.Furniture?.Values.FirstOrDefault(xx => xx.Mesh / 10 == 820);
        if (itemBox == null) return;
        if (client.Warehouses.ContainsKey((Warehouse.WarehouseID)itemBox.UID)) return;
        info.Warehouse ??= new Warehouse(null, (Warehouse.WarehouseID)itemBox.UID);
        client.Warehouses.Add((Warehouse.WarehouseID)itemBox.UID, info.Warehouse);
    }

    /// <summary>
    ///     Finds the house belonging to the player's spouse by name
    /// </summary>
    public static HouseInfo? SpouseHouse(string spouseName) {
        return Houses.Values.FirstOrDefault(house => house.Name == spouseName);
    }

    /// <summary>
    ///     Handles warehouse operations (view, deposit, withdraw) for the spouse's house warehouse
    /// </summary>
    public static bool SpouseWarehouse(GameState client, Network.GamePackets.Warehouse warehousePacket) {
        HouseWarehouse(client);
        var info = SpouseHouse(client.Entity.Spouse);
        if (info == null || client.Entity.MapID == client.Entity.UID)
            info = Houses[client.Entity.UID];
        if (client.Entity.MapID != info.Id) return false;
        switch (warehousePacket.Type) {
            case Network.GamePackets.Warehouse.Entire: {
                var wh = info.Warehouse;
                if (wh == null) return true;
                byte count = 0;
                warehousePacket.Count = 1;
                warehousePacket.Type = Network.GamePackets.Warehouse.AddItem;
                for (; count < wh.Count; count++) {
                    warehousePacket.Append(wh.Objects[count]);
                    client.Send(warehousePacket);
                    var add = new ItemAdding(true);
                    if (wh.Objects[count].Purification.Available)
                        add.Append(wh.Objects[count].Purification);
                    if (wh.Objects[count].ExtraEffect.Available)
                        add.Append(wh.Objects[count].ExtraEffect);
                    if (wh.Objects[count].Purification.Available || wh.Objects[count].ExtraEffect.Available)
                        client.Send(add);
                }

                return true;
            }
            case Network.GamePackets.Warehouse.AddItem: {
                var wh = info.Warehouse;
                if (wh == null) return true;
                if (client.Inventory.TryGetItem(warehousePacket.UID, out var item)) {
                    switch (item.ID) {
                        case >= 729960 and <= 729970:
                        case 729611 or 729612 or 729613 or 729614 or 729703:
                            return true;
                    }

                    if (!ConquerItem.isRune(item.UID)) {
                        if (wh.Add2(item, client)) {
                            warehousePacket.UID = 0;
                            warehousePacket.Count = 1;
                            warehousePacket.Append(item);
                            client.Send(warehousePacket);

                            var add = new ItemAdding(true);
                            if (item.Purification.Available)
                                add.Append(item.Purification);
                            if (item.ExtraEffect.Available)
                                add.Append(item.ExtraEffect);
                            if (item.Purification.Available || item.ExtraEffect.Available)
                                client.Send(add);

                            info.Warehouse = wh;
                            return true;
                        }
                    }
                    else {
                        client.Send(new Message("You can not store Flame Stone Rune's in Warehouse",
                            Color.Red, Message.TopLeft));
                    }
                }

                break;
            }
            case Network.GamePackets.Warehouse.RemoveItem: {
                if (!client.Partners.ContainsKey(info.Uid) && client.Entity.UID != info.Uid) {
                    client.Send(new Message("Sorry you cant, You Should be a Trade Partner.",
                        Message.TopLeft));
                    return true;
                }

                var wh = info.Warehouse;
                if (wh == null) return true;
                if (wh.ContainsUID(warehousePacket.UID))
                    if (wh.Remove2(warehousePacket.UID, client)) {
                        info.Warehouse = wh;
                        client.Send(warehousePacket);
                        return true;
                    }

                break;
            }
        }

        return false;
    }

    /// <summary>
    ///     Finds the item box furniture piece in the house that serves as a warehouse
    /// </summary>
    public static SobNpcSpawn? CheckItemBox(GameState client, HouseInfo info) {
        return info.Furniture?.Values.FirstOrDefault(xx => xx.Mesh / 10 == 820);
    }

    /// <summary>
    ///     Allows the player to reposition a furniture piece in their house
    /// </summary>
    public static void Move(GameState client, SobNpcSpawn sobNpc, HouseInfo info) {
        client.MessageBox("Do you want to change this furniture's place?", p => {
            info.Furniture?.Remove(sobNpc.UID);
            p.Screen.FullWipe();
            p.Screen.Reload();
            NpcRequest req2 = new(5) {
                Mesh = sobNpc.Mesh,
                NpcTyp = sobNpc.Type
            };
            p.Send(req2);
        });
    }
}