using System;
using System.Linq;
using System.Collections.Generic;
using MTA.Network.GamePackets;
using MTA.Interfaces;
using MTA.Game;

namespace MTA.Client.Commands
{
    public static class NpcCommands
    {
        // Track test mesh NPCs for cleanup
        private static readonly Dictionary<ushort, HashSet<uint>> TestMeshNpcsByMap = [];

        // Track pagination data per player
        private static readonly Dictionary<uint, (ushort mapId, ushort startX, ushort startY, int currentPage)> PlayerMeshPageData = [];

        public static bool HandleCommand(GameState client, string[] data, string mess)
        {
            return data[0] switch
            {
                "npcjump" => HandleNpcJumpCommand(client, data, mess),
                "npceffect" => HandleNpcEffectCommand(client, data, mess),
                "gotonpc" => HandleGotoNpcCommand(client, data, mess),
                "reloadnpc" => HandleReloadNpcCommand(client, data, mess),
                "npcskin" => HandleNpcSkinCommand(client, data, mess),
                "renamenpc" => HandleRenameNpcCommand(client, data, mess),
                "movenpc" => HandleMoveNpcCommand(client, data, mess),
                "deletenpc" => HandleDeleteNpcCommand(client, data, mess),
                "addnpc" => HandleAddNpcCommand(client, data, mess),
                "spawnmeshes" => HandleSpawnMeshesCommand(client, data, mess),
                "deletemeshes" => HandleDeleteMeshesCommand(client, data, mess),
                _ => false,
            };
        }

        private static bool HandleNpcJumpCommand(GameState client, string[] data, string mess)
        {
            foreach (var npc in client.Map.Npcs.Values)
            {
                var x = (ushort)(npc.X + 2);
                var y = (ushort)(npc.Y + 2);
                TwoMovements jump = new TwoMovements
                {
                    X = x,
                    Y = y,
                    EntityCount = 1,
                    FirstEntity = npc.UID,
                    MovementType = TwoMovements.Jump
                };
                client.SendScreen(jump);
            }

            return true;
        }

        private static bool HandleNpcEffectCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                client.Send(new Message("Usage: @npceffect <effect_id>", System.Drawing.Color.Yellow,
                    Message.Tip));
                return true;
            }

            foreach (var npc in client.Map.Npcs.Values)
            {
                _String str = new _String(true)
                {
                    UID = npc.UID,
                    TextsCount = 1,
                    Type = _String.Effect
                };
                str.Texts.Add(data[1]);
                client.SendScreen(str);
            }

            return true;
        }

        private static bool HandleGotoNpcCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                client.Send(new Message("Usage: @gotonpc <npc_id>", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            var npcId = uint.Parse(data[1]);
            INpc foundNpc = null;

            // Search through all maps for the NPC
            foreach (var map in Kernel.Maps.Values)
            {
                if (!map.Npcs.TryGetValue(npcId, out var npc)) continue;
                foundNpc = npc;
                break;
            }

            if (foundNpc != null)
            {
                client.Entity.Teleport(foundNpc.MapID, foundNpc.X, foundNpc.Y);
            }
            else
            {
                client.Send(new Message("NPC with ID " + npcId + " not found!", System.Drawing.Color.Yellow,
                    Message.Tip));
            }

            return true;
        }

        private static bool HandleReloadNpcCommand(GameState client, string[] data, string mess)
        {
            try
            {
                World.ScriptEngine.Check_Updates();
                client.Send(new Message("NPC scripts reloaded successfully!", System.Drawing.Color.Green,
                    Message.Tip));
            }
            catch (Exception ex)
            {
                client.Send(new Message("Error reloading NPC scripts: " + ex.Message, System.Drawing.Color.Yellow,
                    Message.Tip));
            }

            return true;
        }

        private static bool HandleNpcSkinCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 3)
            {
                client.Send(new Message("Usage: @npcskin <npc_id> <mesh_id>", System.Drawing.Color.Yellow,
                    Message.Tip));
                client.Send(
                    new Message("Example: @npcskin 100 29680", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            var npcId = uint.Parse(data[1]);
            var meshId = ushort.Parse(data[2]);
            INpc foundNpc = null;
            Map foundMap = null;

            // Search through all maps for the NPC
            foreach (var map in Kernel.Maps.Values)
            {
                if (!map.Npcs.TryGetValue(npcId, out var npc)) continue;
                foundNpc = npc;
                foundMap = map;
                break;
            }

            if (foundNpc == null)
            {
                client.Send(new Message("NPC with ID " + npcId + " not found!", System.Drawing.Color.Yellow,
                    Message.Tip));
                return true;
            }

            // Update NPC mesh
            foundNpc.Mesh = meshId;

            // Update database
            try
            {
                using (var cmd = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE))
                {
                    cmd.Update("npcs")
                        .Set("lookface", meshId)
                        .Where("id", npcId)
                        .Execute();
                }

                // Reload screens for all players on the map
                foreach (var player in Kernel.GamePool.Values)
                {
                    if (player.Entity == null || player.Entity.MapID != foundMap.ID) continue;
                    player.Screen.FullWipe();
                    player.Screen.Reload();
                }

                client.Send(new Message(
                    "NPC [" + (foundNpc.Name ?? "ID: " + npcId) + "] skin changed to " + meshId,
                    System.Drawing.Color.Green, Message.Tip));
            }
            catch (Exception ex)
            {
                client.Send(new Message("Error updating NPC skin in database: " + ex.Message,
                    System.Drawing.Color.Yellow, Message.Tip));
            }

            return true;
        }

        private static bool HandleRenameNpcCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 3)
            {
                client.Send(new Message("Usage: @renamenpc <npc_id> <new_name>", System.Drawing.Color.Yellow,
                    Message.Tip));
                client.Send(
                    new Message("Example: @renamenpc 100 NewNPCName", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            var npcId = uint.Parse(data[1]);
            var newName = mess.Substring(data[0].Length + data[1].Length + 2).Trim();

            if (string.IsNullOrEmpty(newName))
            {
                client.Send(new Message("Error: NPC name cannot be empty!", System.Drawing.Color.Yellow,
                    Message.Tip));
                return true;
            }

            INpc foundNpc = null;
            Map foundMap = null;

            // Search through all maps for the NPC
            foreach (var map in Kernel.Maps.Values)
            {
                if (!map.Npcs.TryGetValue(npcId, out var npc)) continue;
                foundNpc = npc;
                foundMap = map;
                break;
            }

            if (foundNpc == null)
            {
                client.Send(new Message("NPC with ID " + npcId + " not found!", System.Drawing.Color.Yellow,
                    Message.Tip));
                return true;
            }

            var oldName = foundNpc.Name ?? "ID: " + npcId;

            // Update NPC name
            foundNpc.Name = newName;

            // Update database
            try
            {
                using (var cmd = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE))
                {
                    cmd.Update("npcs")
                        .Set("name", newName)
                        .Where("id", npcId)
                        .Execute();
                }

                // Reload screens for all players on the map
                foreach (var player in Kernel.GamePool.Values)
                {
                    if (player.Entity == null || player.Entity.MapID != foundMap.ID) continue;
                    player.Screen.FullWipe();
                    player.Screen.Reload();
                }

                client.Send(new Message(
                    "NPC [" + oldName + "] renamed to [" + newName + "]",
                    System.Drawing.Color.Green, Message.Tip));
            }
            catch (Exception ex)
            {
                // Revert name change on error
                foundNpc.Name = oldName;
                client.Send(new Message("Error updating NPC name in database: " + ex.Message,
                    System.Drawing.Color.Yellow, Message.Tip));
            }

            return true;
        }

        private static bool HandleMoveNpcCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                client.Send(new Message("Usage: @movenpc <npc_id>", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            uint npcId = uint.Parse(data[1]);
            INpc foundNpc = null;
            Map foundMap = null;

            // Search through all maps for the NPC
            foreach (var map in Kernel.Maps.Values)
            {
                if (!map.Npcs.TryGetValue(npcId, out var npc)) continue;
                foundNpc = npc;
                foundMap = map;
                break;
            }

            if (foundNpc == null)
            {
                client.Send(new Message("NPC with ID " + npcId + " not found!", System.Drawing.Color.Yellow,
                    Message.Tip));
                return true;
            }

            // Remove NPC from old map
            foundMap.Npcs.Remove(npcId);

            // Update NPC position to player's position
            foundNpc.X = client.Entity.X;
            foundNpc.Y = client.Entity.Y;
            foundNpc.MapID = client.Entity.MapID;

            // Add NPC to current map
            var targetMap = Kernel.Maps[client.Entity.MapID];
            targetMap.Npcs.Remove(npcId);
            targetMap.Npcs.Add(npcId, foundNpc);

            // Update database
            try
            {
                using (var cmd = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE))
                {
                    cmd.Update("npcs")
                        .Set("cellx", foundNpc.X)
                        .Set("celly", foundNpc.Y)
                        .Set("mapid", foundNpc.MapID)
                        .Where("id", npcId)
                        .Execute();
                }

                // Reload screens for all players on the map
                foreach (var player in Kernel.GamePool.Values)
                {
                    if (player.Entity == null || player.Entity.MapID != client.Entity.MapID) continue;
                    player.Screen.FullWipe();
                    player.Screen.Reload();
                }

                client.Send(new Message(
                    "NPC [" + (foundNpc.Name ?? "ID: " + npcId) + "] moved to your position",
                    System.Drawing.Color.Green, Message.Tip));
            }
            catch (Exception ex)
            {
                client.Send(new Message("Error updating NPC in database: " + ex.Message,
                    System.Drawing.Color.Yellow, Message.Tip));
            }

            return true;
        }

        private static bool HandleDeleteNpcCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                client.Send(new Message("Usage: @deletenpc <npc_id>", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            var npcId = uint.Parse(data[1]);
            INpc foundNpc = null;
            Map foundMap = null;

            // Search through all maps for the NPC
            foreach (var map in Kernel.Maps.Values)
            {
                if (!map.Npcs.TryGetValue(npcId, out var npc)) continue;
                foundNpc = npc;
                foundMap = map;
                break;
            }

            if (foundNpc == null)
            {
                client.Send(new Message("NPC with ID " + npcId + " not found!", System.Drawing.Color.Yellow,
                    Message.Tip));
                return true;
            }

            // Remove NPC from map
            foundMap.RemoveNpc(foundNpc);

            // Delete from database
            try
            {
                using (var cmd = new Database.MySqlCommand(Database.MySqlCommandType.DELETE))
                {
                    cmd.Delete("npcs", "id", npcId).Execute();
                }

                // Reload screens for all players on the map
                foreach (var player in Kernel.GamePool.Values)
                {
                    if (player.Entity != null && player.Entity.MapID == foundMap.ID)
                    {
                        player.Screen.FullWipe();
                        player.Screen.Reload();
                    }
                }

                client.Send(new Message("NPC [" + (foundNpc.Name ?? "ID: " + npcId) + "] deleted successfully",
                    System.Drawing.Color.Green, Message.Tip));
            }
            catch (Exception ex)
            {
                client.Send(new Message("Error deleting NPC from database: " + ex.Message,
                    System.Drawing.Color.Yellow, Message.Tip));
            }

            return true;
        }

        private static bool HandleAddNpcCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 3)
            {
                client.Send(new Message("Usage: @addnpc <name> <mesh>", System.Drawing.Color.Yellow,
                    Message.Tip));
                client.Send(new Message("Example: @addnpc TestNPC 100", System.Drawing.Color.Yellow,
                    Message.Tip));
                return true;
            }

            // Last token is the mesh
            ushort mesh = ushort.Parse(data[data.Length - 1]);

            // Everything between command and mesh is the name
            string name;
            if (data.Length == 3)
            {
                name = data[1];
            }
            else
            {
                // Extract name from message, removing command and mesh
                int meshStartPos = mess.LastIndexOf(" " + data[data.Length - 1]);
                name = mess.Substring(data[0].Length + 1, meshStartPos - data[0].Length - 1).Trim();
            }

            if (string.IsNullOrEmpty(name))
            {
                name = "NPC";
            }

            // Default type to Talker
            byte type = 2;

            // Generate a unique NPC ID
            uint npcId = 100000; // Start from 100000
            try
            {
                using (var cmd = new Database.MySqlCommand(Database.MySqlCommandType.SELECT))
                {
                    cmd.Select("npcs");
                    using var reader = new Database.MySqlReader(cmd);
                    while (reader.Read())
                    {
                        uint existingId = reader.ReadUInt32("id");
                        if (existingId >= npcId)
                            npcId = existingId + 1;

                    }
                }

                // Check if ID exists in memory
                while (true)
                {
                    var exists = Kernel.Maps.Values.Any(map => map.Npcs.ContainsKey(npcId));

                    if (!exists)
                        break;
                    npcId++;
                }
            }
            catch
            {
                // If query fails, use a high starting number
                npcId = 1000000;
                foreach (var map in Kernel.Maps.Values)
                {
                    foreach (var existingNpc in map.Npcs.Values)
                    {
                        if (existingNpc.UID >= npcId)
                            npcId = existingNpc.UID + 1;
                    }
                }
            }

            // Create new NPC
            INpc npc = new NpcSpawn
            {
                UID = npcId,
                Mesh = mesh,
                Type = (Enums.NpcType)type,
                X = client.Entity.X,
                Y = client.Entity.Y,
                MapID = client.Entity.MapID,
                Name = name
            };

            // Add to map
            client.Map.AddNpc(npc);

            // Insert into database
            try
            {
                using (var cmd = new Database.MySqlCommand(Database.MySqlCommandType.INSERT))
                {
                    cmd.Insert("npcs")
                        .Insert("id", npc.UID)
                        .Insert("name", npc.Name)
                        .Insert("type", (int)npc.Type)
                        .Insert("lookface", npc.Mesh)
                        .Insert("mapid", npc.MapID)
                        .Insert("cellx", npc.X)
                        .Insert("celly", npc.Y)
                        .Insert("effect", "")
                        .Execute();
                }

                // Reload screens for all players on the map
                foreach (var player in Kernel.GamePool.Values)
                {
                    if (player.Entity == null || player.Entity.MapID != client.Entity.MapID) continue;
                    player.Screen.FullWipe();
                    player.Screen.Reload();
                }
            }
            catch (Exception ex)
            {
                // Remove from map if database insert failed
                client.Map.RemoveNpc(npc);
                client.Send(new Message("Error saving NPC to database: " + ex.Message, System.Drawing.Color.Yellow,
                    Message.Tip));
            }

            return true;
        }

        private static bool HandleSpawnMeshesCommand(GameState client, string[] data, string mess)
        {
            try
            {
                const int npcsPerPage = 500, maxMeshId = 65535, meshesPerNpc = 10;
                const uint baseNpcIdStart = 900000000;

                int totalNpcs = (maxMeshId / meshesPerNpc) + 1; // 6554 NPCs total
                int maxPages = (int)Math.Ceiling((double)totalNpcs / npcsPerPage); // ~14 pages

                // Validate input
                if (data.Length < 2 || !int.TryParse(data[1], out int page) || page < 1 || page > maxPages)
                {
                    client.Send(new Message(data.Length < 2
                        ? $"Usage: @spawnmeshes <page> (pages 1-{maxPages}, {npcsPerPage} NPCs per page)"
                        : $"Invalid page number. Use pages 1-{maxPages}.",
                        data.Length < 2 ? System.Drawing.Color.Yellow : System.Drawing.Color.Red, Message.Tip));
                    return true;
                }

                // Fixed map and line positions
                const ushort targetMapId = 1000;
                const ushort lineStartX = 704, lineStartY = 763; // Starting position
                const ushort lineEndX = 315, lineEndY = 79; // Ending position
                const ushort spacing = 15; // Spacing between NPCs along the line
                const ushort zigzagOffset = 8; // Offset for zigzag pattern (left/right)

                // Calculate mesh ranges
                int startMeshIndex = (page - 1) * npcsPerPage;
                int npcsThisPage = Math.Min(npcsPerPage, totalNpcs - startMeshIndex);
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

                client.Send(new Message($"Spawning page {page}/{maxPages}: {npcsThisPage} NPCs (meshes {startMeshIndex * meshesPerNpc}-{(startMeshIndex + npcsThisPage - 1) * meshesPerNpc})...",
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


