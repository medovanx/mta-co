using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MTA.Database;
using MTA.Game;
using MTA.Network.GamePackets;
using static MTA.Kernel;

namespace MTA.Client.Commands.TestCommands {
    public static class MonsterTestCommands {
        // Track test monsters for cleanup and npctype identification
        private static readonly Dictionary<ushort, HashSet<uint>> TestMonstersByMap = [];

        public static bool HandleCommand(GameState client, string[] data, string mess) {
            if (data.Length == 0) return false;

            return data[0].ToLower() switch {
                "spawnmonsters" => HandleSpawnMonstersCommand(client, data),
                "deletemonsters" => HandleDeleteMonstersCommand(client),
                _ => false,
            };
        }

        private static bool HandleSpawnMonstersCommand(GameState client, string[] data) {
            try {
                if (data.Length < 2) {
                    client.Send(new Message(
                        "Usage: @spawnmonsters <npctype1> [npctype2] [range] ...\nExample: @spawnmonsters 100 200 300-305",
                        Color.Yellow, Message.Tip));
                    return true;
                }

                var map = client.Map;

                // Initialize monster tracking for this map
                if (!TestMonstersByMap.ContainsKey(map.ID))
                    TestMonstersByMap[map.ID] = [];

                var spawnedMonsters = TestMonstersByMap[map.ID];
                var spawned = 0;

                // Collect all npctypes (individual and from ranges)
                var npctypesToSpawn = new List<uint>();

                // Parse npctypes from arguments
                for (var i = 1; i < data.Length; i++) {
                    var arg = data[i];

                    // Check if it's a range (e.g., "100-900")
                    if (arg.Contains("-")) {
                        var rangeParts = arg.Split('-');
                        if (rangeParts.Length == 2 &&
                            uint.TryParse(rangeParts[0].Trim(), out uint start) &&
                            uint.TryParse(rangeParts[1].Trim(), out uint end)) {
                            if (start > end) {
                                client.Send(new Message($"Invalid range: {arg} (start > end). Skipping.",
                                    Color.Yellow, Message.Tip));
                                continue;
                            }

                            // Add all npctypes in range
                            for (var npctype = start; npctype <= end; npctype++) {
                                npctypesToSpawn.Add(npctype);
                            }
                        }
                        else {
                            client.Send(new Message(
                                $"Invalid range format: {arg}. Use format: start-end (e.g., 100-900). Skipping.",
                                Color.Yellow, Message.Tip));
                        }
                    }
                    else {
                        // Single npctype
                        if (!uint.TryParse(arg, out var npctype)) {
                            client.Send(new Message($"Invalid npctype: {arg}. Skipping.",
                                Color.Yellow, Message.Tip));
                            continue;
                        }

                        npctypesToSpawn.Add(npctype);
                    }
                }

                // Spawn monsters for each npctype
                foreach (var npctype in npctypesToSpawn) {
                    // Get monster information
                    if (!MonsterInformation.MonsterInformations.ContainsKey(npctype)) {
                        continue; // Skip silently if not found
                    }

                    var mt = MonsterInformation.MonsterInformations[npctype];

                    // Spawn monster near player
                    var entity = new Entity(EntityFlag.Monster, false) {
                        MapObjType = MapObjectType.Monster,
                        MonsterInfo = mt.Copy()
                    };
                    entity.MonsterInfo.Owner = entity;
                    entity.Name = mt.Name;
                    entity.MinAttack = mt.MinAttack;
                    entity.MaxAttack = entity.MagicAttack = mt.MaxAttack;
                    entity.Hitpoints = entity.MaxHitpoints = mt.Hitpoints;
                    entity.Defence = mt.Defence;
                    entity.Body = mt.Mesh;
                    entity.Level = mt.Level;
                    entity.UID = map.EntityUIDCounter.Next;
                    entity.MapID = map.ID;
                    entity.SendUpdates = true;

                    // Position near player with random offset
                    entity.X = (ushort)(client.Entity.X + Kernel.Random.Next(-5, 6));
                    entity.Y = (ushort)(client.Entity.Y + Kernel.Random.Next(-5, 6));

                    // Ensure valid coordinates
                    if (entity.X >= map.Floor.Bounds.Width) entity.X = (ushort)(map.Floor.Bounds.Width - 1);
                    if (entity.Y >= map.Floor.Bounds.Height) entity.Y = (ushort)(map.Floor.Bounds.Height - 1);

                    map.AddEntity(entity);
                    entity.SendSpawn(client);
                    spawnedMonsters.Add(entity.UID);
                    spawned++;

                    client.Send(new Message($"Spawned monster: {mt.Name} (npctype: {npctype})",
                        Color.Green, Message.Tip));
                }

                client.Send(new Message($"Spawned {spawned} test monster(s). Kill them to see their npctype!",
                    Color.Cyan, Message.Tip));
            }
            catch (Exception ex) {
                client.Send(new Message($"Error spawning test monsters: {ex.Message}",
                    Color.Red, Message.Tip));
            }

            return true;
        }

        private static bool HandleDeleteMonstersCommand(GameState client) {
            try {
                var mapId = client.Entity.MapID;

                // Check if there are any test monsters on this map
                if (!TestMonstersByMap.TryGetValue(mapId, out var testMonsters) || testMonsters.Count == 0) {
                    client.Send(new Message("No test monsters found on this map.",
                        Color.Yellow, Message.Tip));
                    return true;
                }

                // Validate map exists
                if (!Maps.TryGetValue(mapId, out var map)) {
                    client.Send(new Message("Map not found.",
                        Color.Red, Message.Tip));
                    return true;
                }

                var removed = 0;

                foreach (var entity in from monsterId in testMonsters.ToList()
                         where map.Entities.ContainsKey(monsterId)
                         select map.Entities[monsterId]
                         into entity
                         where entity.EntityFlag == EntityFlag.Monster
                         select entity) {
                    map.RemoveEntity(entity);
                    removed++;
                }

                // Clean up tracking data
                testMonsters.Clear();
                TestMonstersByMap.Remove(mapId);

                // Reload screens for all players on the map
                ReloadMapScreens(mapId);

                client.Send(new Message($"Removed {removed} test monster(s) from the map.",
                    Color.Green, Message.Tip));
            }
            catch (Exception ex) {
                client.Send(new Message($"Error deleting test monsters: {ex.Message}",
                    Color.Red, Message.Tip));
            }

            return true;
        }

        private static void ReloadMapScreens(ushort mapId) {
            foreach (var player in GamePool.Values) {
                if (player.Entity.MapID != mapId) continue;
                player.Screen.FullWipe();
                player.Screen.Reload();
            }
        }
    }
}