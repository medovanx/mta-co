using System.Linq;
using MTA.Client;
using MTA.Game.Constants;
using MTA.Game.Features.House.Database;
using MTA.Network.GamePackets;
using MTA.Network.PacketHandlers;
using static MTA.Game.Features.House.House;

namespace MTA.Game.Features.House.Packets;

/// <summary>
///     Handles NpcSpawn packets (2030) for house furniture placement.
///     Intercepts house-related spawns and handles them completely, or returns false to let default handler process.
/// </summary>
[PacketHandler(Constants.Packets.MsgNpcInfo)]
public static class HouseFurnitureSpawnHandler {
    /// <summary>
    ///     Handles NpcSpawn packets for house furniture placement.
    /// </summary>
    /// <returns>True if packet was handled (house-related), false to let default handler process</returns>
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        // Check action - same validation as default handler
        if (client.Action != 2)
            return false; // Let default handler process

        // Check if this is house-related (not statue map and player has house)
        if (client.Entity.MapID == Maps.GuildWarMap || !Houses.TryGetValue(client.Entity.UID, out var house))
            return false; // Let default handler process (statue or no house)

        // Only allow one item box per house
        var spawn = new NpcSpawn(false);
        spawn.Deserialize(packet);
        var isItemBox = spawn.Mesh / 10 == 820;
        if (isItemBox) {
            var itemBox = house.Furniture?.Values.FirstOrDefault(xx => xx.Mesh / 10 == 820);
            if (itemBox != null) {
                client.MessageBox("You already have an Item Box in your house!");
                return true; // Packet handled (error case)
            }
        }

        // Create SobNpcSpawn object
        var furniture = new SobNpcSpawn {
            Owner = client,
            UID = client.Map.EntityUIDCounter2.Next,
            Mesh = spawn.Mesh,
            MapID = client.Entity.MapID,
            X = spawn.X,
            Y = spawn.Y,
            // Set furniture type based on mesh
            Type = isItemBox
                ? Enums.NpcType.Talker // Item box
                : Enums.NpcType.RegularFurniture // Regular furniture
        };

        // Generate unique UID (ensure it doesn't conflict with existing furniture)
        do {
            furniture.UID = client.Map.EntityUIDCounter2.Next;
        } while (house.Furniture!.ContainsKey(furniture.UID));

        house.Furniture!.Add(furniture.UID, furniture);

        // Save to database
        var type = (byte)(isItemBox
            ? Enums.NpcType.Talker // Item Box
            : Enums.NpcType.RegularFurniture);
        HouseFurnitureTable.AddFurniture(client.Entity.UID, furniture, type);

        // Remove spawn item from inventory (if item exists)
        if (client.spawnItem != null)
            client.Inventory.Remove(client.spawnItem, Enums.ItemUse.Remove);

        // Send screen spawn
        client.SendScreenSpawn(furniture, true);
        client.Screen.FullWipe();
        client.Screen.Reload();

        return true; // Packet handled successfully
    }
}