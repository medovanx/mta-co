using System.Collections.Generic;
using MTA.Client;
using MTA.Game.Features.House.Database;
using MTA.Game.Features.House.Database.Models;
using MTA.Network.GamePackets;

namespace MTA.Game.Features.House;

public static class Furniture {
    public static readonly SafeDictionary<uint, ushort> FurnitureItems = new();
    public static readonly SafeDictionary<uint, FurnitureInfo> FurnitureVendors = new();

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

    /// <summary>
    ///     Sends the reposition a furniture piece request dialog
    /// </summary>
    public static void MoveFurniture(GameState client, SobNpcSpawn sobNpc, HouseInfo info) {
        client.MessageBox("Do you want to change this furniture's place?", player => {
            info.Furniture?.Remove(sobNpc.UID);
            player.Screen.FullWipe();
            player.Screen.Reload();
            NpcRequest npcRequest = new(5) {
                Mesh = sobNpc.Mesh,
                NpcTyp = sobNpc.Type
            };
            player.Send(npcRequest);
        });
    }
}