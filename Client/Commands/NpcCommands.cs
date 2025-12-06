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
        public static bool HandleCommand(GameState client, string[] data, string mess)
        {
            return data[0] switch
            {
                "npcjump" => HandleNpcJumpCommand(client, data, mess),
                "gotonpc" => HandleGotoNpcCommand(client, data, mess),
                "editnpc" => HandleEditNpcCommand(client, data, mess),
                "movenpc" => HandleMoveNpcCommand(client, data, mess),
                "deletenpc" => HandleDeleteNpcCommand(client, data, mess),
                "addnpc" => HandleAddNpcCommand(client, data, mess),
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

        private static Dictionary<string, string> ParseArguments(string[] data, string mess, int startIndex = 1)
        {
            var args = new Dictionary<string, string>();
            for (int i = startIndex; i < data.Length; i++)
            {
                if (data[i].StartsWith("-"))
                {
                    string key = data[i].Substring(1).ToLower();
                    if (i + 1 < data.Length && !data[i + 1].StartsWith("-"))
                    {
                        args[key] = data[i + 1];
                        i++; // Skip the value in next iteration
                    }
                    else
                    {
                        args[key] = "";
                    }
                }
            }
            return args;
        }

        private static bool HandleEditNpcCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                client.Send(new Message("Usage: @editnpc <npc_id> [-n <name>] [-s <skin>] [-e <effect>|none]", System.Drawing.Color.Yellow,
                    Message.Tip));
                client.Send(new Message("Example: @editnpc 100 -n NewName -s 29680 -e ninjapk_third", System.Drawing.Color.Yellow, Message.Tip));
                client.Send(new Message("To remove effect: @editnpc 100 -e none", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            if (!uint.TryParse(data[1], out var npcId))
            {
                client.Send(new Message("Invalid NPC ID!", System.Drawing.Color.Yellow, Message.Tip));
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

            var args = ParseArguments(data, mess, 2); // Start from index 2 to skip NPC ID
            bool hasChanges = false;
            var changes = new List<string>();

            // Update name if provided
            if (args.ContainsKey("n") && !string.IsNullOrEmpty(args["n"]))
            {
                var oldName = foundNpc.Name ?? "ID: " + npcId;
                foundNpc.Name = args["n"];
                changes.Add("name: " + oldName + " -> " + args["n"]);
                hasChanges = true;
            }

            // Update skin/mesh if provided
            if (args.ContainsKey("s") && !string.IsNullOrEmpty(args["s"]))
            {
                if (ushort.TryParse(args["s"], out var meshId))
                {
                    foundNpc.Mesh = meshId;
                    changes.Add("skin: " + meshId);
                    hasChanges = true;
                }
                else
                {
                    client.Send(new Message("Invalid skin/mesh ID!", System.Drawing.Color.Yellow, Message.Tip));
                    return true;
                }
            }

            // Update effect if provided (empty string or "none" removes effect)
            if (args.ContainsKey("e"))
            {
                if (string.IsNullOrEmpty(args["e"]) || args["e"].ToLower() == "none")
                {
                    changes.Add("effect: removed");
                    hasChanges = true;
                }
                else
                {
                    changes.Add("effect: " + args["e"]);
                    hasChanges = true;
                }
            }

            if (!hasChanges)
            {
                client.Send(new Message("No changes specified. Use -n, -s, or -e to modify NPC.", System.Drawing.Color.Yellow,
                    Message.Tip));
                return true;
            }

            // Update database
            try
            {
                using (var cmd = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE))
                {
                    var updateCmd = cmd.Update("npcs");

                    if (args.ContainsKey("n") && !string.IsNullOrEmpty(args["n"]))
                    {
                        updateCmd.Set("name", args["n"]);
                    }

                    if (args.ContainsKey("s") && !string.IsNullOrEmpty(args["s"]) && ushort.TryParse(args["s"], out var meshId))
                    {
                        updateCmd.Set("lookface", meshId);
                    }

                    if (args.ContainsKey("e"))
                    {
                        // Empty string or "none" removes the effect
                        string effectValue = (string.IsNullOrEmpty(args["e"]) || args["e"].ToLower() == "none") ? "" : args["e"];
                        updateCmd.Set("effect", effectValue);
                        // Also update the NPC object in memory
                        foundNpc.effect = effectValue;
                    }

                    updateCmd.Where("id", npcId).Execute();
                }

                // Reload screens for all players on the map
                foreach (var player in Kernel.GamePool.Values)
                {
                    if (player.Entity == null || player.Entity.MapID != foundMap.ID) continue;
                    player.Screen.FullWipe();
                    player.Screen.Reload();
                }

                client.Send(new Message(
                    "NPC [" + (foundNpc.Name ?? "ID: " + npcId) + "] updated: " + string.Join(", ", changes),
                    System.Drawing.Color.Green, Message.Tip));
            }
            catch (Exception ex)
            {
                client.Send(new Message("Error updating NPC in database: " + ex.Message,
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
            if (data.Length < 2)
            {
                client.Send(new Message("Usage: @addnpc -n <name> -s <skin> [-e <effect>]", System.Drawing.Color.Yellow,
                    Message.Tip));
                client.Send(new Message("Example: @addnpc -n test -s 1002 -e ninjapk_third", System.Drawing.Color.Yellow,
                    Message.Tip));
                return true;
            }

            var args = ParseArguments(data, mess);

            // Get name
            string name = "NPC";
            if (args.ContainsKey("n") && !string.IsNullOrEmpty(args["n"]))
            {
                name = args["n"];
            }
            else
            {
                client.Send(new Message("Name (-n) is required!", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            // Get skin/mesh
            ushort mesh = 0;
            if (args.ContainsKey("s") && !string.IsNullOrEmpty(args["s"]))
            {
                if (!ushort.TryParse(args["s"], out mesh))
                {
                    client.Send(new Message("Invalid skin/mesh ID!", System.Drawing.Color.Yellow, Message.Tip));
                    return true;
                }
            }
            else
            {
                client.Send(new Message("Skin (-s) is required!", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            // Get effect (optional)
            string effect = "";
            if (args.ContainsKey("e") && !string.IsNullOrEmpty(args["e"]))
            {
                effect = args["e"];
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
                        .Insert("effect", effect)
                        .Execute();
                }

                // Reload screens for all players on the map
                foreach (var player in Kernel.GamePool.Values)
                {
                    if (player.Entity == null || player.Entity.MapID != client.Entity.MapID) continue;
                    player.Screen.FullWipe();
                    player.Screen.Reload();
                }

                var successMsg = "NPC [" + name + "] created with ID " + npcId + " (skin: " + mesh + ")";
                if (!string.IsNullOrEmpty(effect))
                {
                    successMsg += " (effect: " + effect + ")";
                }
                client.Send(new Message(successMsg, System.Drawing.Color.Green, Message.Tip));
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
    }
}


