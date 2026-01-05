using System;
using MTA.Client;
using MTA.Network.GamePackets;
using MTA.Network.PacketHandlers;
using static MTA.Game.Features.House.House;
using HouseInfo = MTA.Game.Features.House.Database.Models.HouseInfo;

namespace MTA.Game.Features.House.Packets;

/// <summary>
///     Handles NpcRequest packets (2031/2032) for house furniture interactions.
///     Handles clicking on furniture (item box warehouse access, moving furniture).
/// </summary>
[PacketHandler(Constants.Packets.MsgNpc, Constants.Packets.MsgTaskDialog)]
public static class HouseFurntiureRequestHandler {
    /// <summary>
    ///     Handles NpcRequest packets for house furniture interactions.
    /// </summary>
    /// <returns>True if packet was handled (house-related), false to let default handler process</returns>
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        // Check action - same validation as default handler
        if (client.Action != 2)
            return false; // Let default handler process

        // Deserialize NpcRequest packet
        var request = new NpcRequest();
        request.Deserialize(packet);

        // Check if this is house-related (player has house or is in spouse's house)
        HouseInfo? house = null;
        var isOwnHouse = Houses.TryGetValue(client.Entity.UID, out var ownHouse);

        if (isOwnHouse) {
            house = ownHouse;
        }
        else {
            var spouseHouse = SpouseHouse(client.Entity.Spouse);
            if (spouseHouse != null && client.Entity.MapID == (ushort)spouseHouse.Uid) {
                house = spouseHouse;
            }
        }

        // If not in a house, let default handler process
        if (house == null)
            return false;

        // Check if the NPC being interacted with is furniture
        if (!house.Furniture!.TryGetValue(request.NpcID, out var furnitureNpc))
            return false; // Not furniture, let default handler process

        var isItemBox = furnitureNpc.Mesh / 10 == 820;
        if (isItemBox) {
            // Open warehouse window for item box
            var data = new Data(true) {
                ID = Data.OpenWindow,
                UID = client.Entity.UID,
                TimeStamp = Time32.Now,
                dwParam = Data.WindowCommands.Warehouse,
                wParam1 = client.Entity.X,
                wParam2 = client.Entity.Y
            };
            client.Send(data);
        }

        // Handle furniture movement (for all furniture, including item boxes)
        Furniture.MoveFurniture(client, furnitureNpc, house);
        return true; // Packet handled (warehouse opened and/or movement dialog shown)
    }
}