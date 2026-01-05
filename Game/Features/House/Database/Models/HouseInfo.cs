using System.Collections.Generic;
using MTA.Network.GamePackets;
using Warehouse = MTA.Game.ConquerStructures.Warehouse;

namespace MTA.Game.Features.House.Database.Models;

/// <summary>
///     Represents a house with its furniture and warehouse
/// </summary>
public class HouseInfo {
    public Dictionary<uint, SobNpcSpawn>? Furniture;
    public ushort Id;
    public ushort Level;
    public ushort MapType;
    public uint Uid;
    public Warehouse? Warehouse;
}