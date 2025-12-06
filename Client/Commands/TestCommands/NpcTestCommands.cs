using System;
using System.Linq;
using System.Collections.Generic;
using MTA.Network.GamePackets;
using MTA.Interfaces;
using MTA.Game;

namespace MTA.Client.Commands.TestCommands
{
    public static class NpcTestCommands
    {
        // Track test mesh NPCs for cleanup
        private static readonly Dictionary<ushort, HashSet<uint>> TestMeshNpcsByMap = [];

        // Track pagination data per player
        private static readonly Dictionary<uint, (ushort mapId, ushort startX, ushort startY, int currentPage)> PlayerMeshPageData = [];

        public static bool HandleCommand(GameState client, string[] data, string mess)
        {
            if (data.Length == 0) return false;

            return data[0].ToLower() switch
            {
                "spawnmeshes" => HandleSpawnMeshesCommand(client, data, mess),
                "deletemeshes" => HandleDeleteMeshesCommand(client, data, mess),
                _ => false,
            };
        }

        private static bool HandleSpawnMeshesCommand(GameState client, string[] data, string mess)
        {
            try
            {
                const int maxMeshId = 65535, meshesPerNpc = 10;
                const uint baseNpcIdStart = 900000000;

                // Parse parameters
                int amount = 500; // Default amount
                int page = 1; // Default page
                ushort spacing = 2; // Default spacing
                ushort zigzagOffset = 1; // Default zigzag offset

                // Parse amount (first parameter after spawnmeshes)
                if (data.Length < 2 || !int.TryParse(data[1], out amount) || amount < 1)
                {
                    client.Send(new Message("Usage: @test npc spawnmeshes <amount> [-p <page>] [-spacing <value>]\nExample: @test npc spawnmeshes 200 -p 2 -spacing 3",
                        System.Drawing.Color.Yellow, Message.Tip));
                    return true;
                }

                // Parse flags
                for (int i = 2; i < data.Length; i++)
                {
                    if (data[i].ToLower() == "-p" && i + 1 < data.Length)
                    {
                        if (!int.TryParse(data[i + 1], out page) || page < 1)
                        {
                            client.Send(new Message("Invalid page number. Must be a positive integer.",
                                System.Drawing.Color.Red, Message.Tip));
                            return true;
                        }
                        i++; // Skip the next parameter as we've consumed it
                    }
                    else if (data[i].ToLower() == "-spacing" && i + 1 < data.Length)
                    {
                        if (!ushort.TryParse(data[i + 1], out spacing) || spacing < 1)
                        {
                            client.Send(new Message("Invalid spacing value. Must be a positive integer.",
                                System.Drawing.Color.Red, Message.Tip));
                            return true;
                        }
                        i++; // Skip the next parameter as we've consumed it
                    }
                }

                // Calculate total NPCs and max pages dynamically
                int totalNpcs = (maxMeshId / meshesPerNpc) + 1; // 6554 NPCs total
                int maxPages = (int)Math.Ceiling((double)totalNpcs / amount); // Dynamic pages based on amount

                // Validate page
                if (page > maxPages)
                {
                    client.Send(new Message($"Invalid page number. Maximum pages: {maxPages} (with {amount} NPCs per page)",
                        System.Drawing.Color.Red, Message.Tip));
                    return true;
                }

                // Fixed map and line positions
                const ushort targetMapId = 1000;
                const ushort lineStartX = 704, lineStartY = 763; // Starting position
                const ushort lineEndX = 315, lineEndY = 79; // Ending position

                // Calculate mesh ranges based on amount
                int startMeshIndex = (page - 1) * amount;
                int npcsThisPage = Math.Min(amount, totalNpcs - startMeshIndex);
                uint baseNpcId = baseNpcIdStart + ((uint)(page - 1) * 1000000);

                // Remove previous page if switching
                if (PlayerMeshPageData.TryGetValue(client.Entity.UID, out var pageData) &&
                    pageData.currentPage != page && pageData.currentPage > 0)
                {
                    RemovePageNPCs(targetMapId, pageData.currentPage,
                        baseNpcIdStart + ((uint)(pageData.currentPage - 1) * 1000000));
                    client.Send(new Message($"Removed page {pageData.currentPage} NPCs.", System.Drawing.Color.Cyan, Message.Tip));
                }

                PlayerMeshPageData[client.Entity.UID] = (targetMapId, lineStartX, lineStartY, page);
                Map targetMap = Kernel.Maps[targetMapId];

                // Initialize NPC tracking
                if (!TestMeshNpcsByMap.ContainsKey(targetMapId))
                    TestMeshNpcsByMap[targetMapId] = [];
                var createdNpcs = TestMeshNpcsByMap[targetMapId];

                client.Send(new Message($"Spawning page {page}/{maxPages}: {npcsThisPage} NPCs (meshes {startMeshIndex * meshesPerNpc}-{(startMeshIndex + npcsThisPage - 1) * meshesPerNpc}) with spacing {spacing}...",
                    System.Drawing.Color.Yellow, Message.Tip));

                RemovePageNPCs(targetMapId, page, baseNpcId);

                // Calculate line direction and perpendicular for zigzag
                double dx = lineEndX - lineStartX, dy = lineEndY - lineStartY;
                double lineLength = Math.Sqrt(dx * dx + dy * dy);
                double unitX = dx / lineLength, unitY = dy / lineLength;
                double perpX = -unitY, perpY = unitX; // Perpendicular vector for zigzag

                // Create NPCs in a zigzag line
                int created = 0;
                for (int i = 0; i < npcsThisPage; i++)
                {
                    // Calculate position along the line with spacing
                    double distanceAlongLine = i * spacing;
                    double t = distanceAlongLine / lineLength; // Normalized position (0 to 1)
                    if (t > 1.0) break; // Stop if we've gone past the end

                    // Base position along the line
                    double baseX = lineStartX + (dx * t);
                    double baseY = lineStartY + (dy * t);

                    // Apply zigzag pattern (alternate left/right)
                    bool zigRight = (i % 2) == 0;
                    double offsetX = perpX * zigzagOffset * (zigRight ? 1 : -1);
                    double offsetY = perpY * zigzagOffset * (zigRight ? 1 : -1);

                    ushort x = (ushort)(baseX + offsetX);
                    ushort y = (ushort)(baseY + offsetY);

                    INpc npc = new NpcSpawn
                    {
                        UID = baseNpcId + (uint)created,
                        Mesh = (ushort)((startMeshIndex + created) * meshesPerNpc),
                        Type = Enums.NpcType.Talker,
                        X = x,
                        Y = y,
                        MapID = targetMapId,
                        Name = ((startMeshIndex + created) * meshesPerNpc).ToString()
                    };

                    targetMap.AddNpc(npc, false);
                    createdNpcs.Add(npc.UID);
                    created++;
                }

                // Always teleport player to starting position
                client.Entity.Teleport(targetMapId, lineStartX, lineStartY);
                ReloadMapScreens(targetMapId);

                client.Send(new Message($"Page {page}/{maxPages} complete! Created {created} NPCs (mesh range: {startMeshIndex * meshesPerNpc}-{(startMeshIndex + created - 1) * meshesPerNpc})",
                    System.Drawing.Color.Green, Message.Tip));
                client.Send(new Message($"Line from ({lineStartX},{lineStartY}) to ({lineEndX},{lineEndY}) on map {targetMapId}",
                    System.Drawing.Color.Cyan, Message.Tip));
            }
            catch (Exception ex)
            {
                client.Send(new Message($"Error spawning mesh page: {ex.Message}", System.Drawing.Color.Red, Message.Tip));
            }
            return true;
        }

        private static void ReloadMapScreens(ushort mapId)
        {
            foreach (var player in Kernel.GamePool.Values)
            {
                if (player.Entity == null || player.Entity.MapID != mapId) continue;
                player.Screen.FullWipe();
                player.Screen.Reload();
            }
        }

        private static void RemovePageNPCs(ushort mapId, int page, uint baseNpcId)
        {
            // Validate map and NPC tracking exist
            if (!Kernel.Maps.ContainsKey(mapId) || !TestMeshNpcsByMap.ContainsKey(mapId))
                return;

            const uint pageIdRange = 1000000; // ID range per page
            var map = Kernel.Maps[mapId];
            var npcs = TestMeshNpcsByMap[mapId];

            // Calculate ID range for this page
            uint pageStartId = baseNpcId;
            uint pageEndId = baseNpcId + pageIdRange;

            // Find and remove all NPCs in this page's ID range
            var toRemove = npcs.Where(id => id >= pageStartId && id < pageEndId).ToList();
            foreach (var npcId in toRemove)
            {
                if (map.Npcs.ContainsKey(npcId))
                {
                    map.RemoveNpc(map.Npcs[npcId]);
                }
                npcs.Remove(npcId);
            }
        }

        private static bool HandleDeleteMeshesCommand(GameState client, string[] data, string mess)
        {
            try
            {
                ushort mapId = client.Entity.MapID;

                // Check if there are any mesh NPCs on this map
                if (!TestMeshNpcsByMap.ContainsKey(mapId) || TestMeshNpcsByMap[mapId].Count == 0)
                {
                    client.Send(new Message("No mesh NPCs found on this map.",
                        System.Drawing.Color.Yellow, Message.Tip));
                    return true;
                }

                // Validate map exists
                if (!Kernel.Maps.ContainsKey(mapId))
                {
                    client.Send(new Message("Map not found.",
                        System.Drawing.Color.Red, Message.Tip));
                    return true;
                }

                // Remove all NPCs from the map
                var map = Kernel.Maps[mapId];
                var createdNpcs = TestMeshNpcsByMap[mapId];
                int removed = 0;

                foreach (var npcId in createdNpcs.ToList())
                {
                    if (map.Npcs.ContainsKey(npcId))
                    {
                        map.RemoveNpc(map.Npcs[npcId]);
                        removed++;
                    }
                }

                // Clean up tracking data
                createdNpcs.Clear();
                TestMeshNpcsByMap.Remove(mapId);

                // Clear player page data for players on this map
                var playersToClear = PlayerMeshPageData
                    .Where(kvp => kvp.Value.mapId == mapId)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var playerId in playersToClear)
                {
                    PlayerMeshPageData.Remove(playerId);
                }

                // Reload screens for all players on the map
                ReloadMapScreens(mapId);

                client.Send(new Message($"Removed {removed} mesh NPCs from the map.",
                    System.Drawing.Color.Green, Message.Tip));
            }
            catch (Exception ex)
            {
                client.Send(new Message($"Error deleting mesh NPCs: {ex.Message}",
                    System.Drawing.Color.Red, Message.Tip));
            }

            return true;
        }
    }
}

