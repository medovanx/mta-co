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
public static class HouseNpcSpawnHandler {
    /// <summary>
    ///     Handles NpcSpawn packets for house furniture placement.
    /// </summary>
    /// <returns>True if packet was handled (house-related), false to let default handler process</returns>
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        // Check action - same validation as default handler
        if (client.Action != 2)
            return false; // Let default handler process

        // Check if this is house-related (not statue map and player has house)
        if (client.Entity.MapID == Maps.GuildWarMap || !Houses.TryGetValue(client.Entity.UID, out var value))
            return false; // Let default handler process (statue or no house)

        // Deserialize NpcSpawn packet
        var spawn = new NpcSpawn(false);
        spawn.Deserialize(packet);

        // Create SobNpcSpawn object
        var furniture = new SobNpcSpawn {
            Owner = client,
            UID = client.Map.EntityUIDCounter2.Next,
            Mesh = spawn.Mesh,
            MapID = client.Entity.MapID,
            X = spawn.X,
            Y = spawn.Y,
            // Set furniture type based on mesh
            Type = spawn.Mesh / 10 == 820
                ? Enums.NpcType.Talker // Item box
                : Enums.NpcType.RegularFurniture // Regular furniture
        };

        // Only check for existing item box if placing an item box
        if (furniture.Mesh / 10 == 820) {
            var itemBox = CheckItemBox(client, value);
            if (itemBox != null) {
                client.MessageBox("You already have an Item Box in your house!");
                return true; // Packet handled (error case)
            }
        }

        // Generate unique UID (ensure it doesn't conflict with existing furniture)
        do {
            furniture.UID = client.Map.EntityUIDCounter2.Next;
        } while (value.Furniture!.ContainsKey(furniture.UID));

        value.Furniture!.Add(furniture.UID, furniture);

        // Save to database
        var type = (byte)(furniture.Mesh / 10 == 820
            ? Enums.NpcType.Talker // Item Box
            : Enums.NpcType.RegularFurniture);
        HouseFurnitureTable.AddFurniture(client.Entity.UID, furniture, type);
        client.Inventory.Remove(client.spawnItem, Enums.ItemUse.Remove);

        // Send screen spawn
        client.SendScreenSpawn(furniture, true);
        client.Screen.FullWipe();
        client.Screen.Reload();

        return true; // Packet handled successfully
    }
}