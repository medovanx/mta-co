using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MTA.Network.GamePackets;
using MTA.Interfaces;
using MTA.Game;

namespace MTA.Client.Commands {
    public static partial class NpcCommands {
        public static bool HandleCommand(GameState client, string[] data, string mess) {
            return data[0] switch {
                "npcjump" => HandleNpcJumpCommand(client),
                "gotonpc" => HandleGotoNpcCommand(client, data),
                "editnpc" => HandleEditNpcCommand(client, data, mess),
                "movenpc" => HandleMoveNpcCommand(client, data),
                "deletenpc" => HandleDeleteNpcCommand(client, data),
                "addnpc" => HandleAddNpcCommand(client, data, mess),
                "reloadnpcs" => HandleReloadNpcsCommand(client, data),
                _ => false,
            };
        }

        private static bool HandleNpcJumpCommand(GameState client) {
            foreach (var jump in from npc in client.Map.Npcs.Values
                     let x = (ushort)(npc.X + 2)
                     let y = (ushort)(npc.Y + 2)
                     select new TwoMovements {
                         X = x,
                         Y = y,
                         EntityCount = 1,
                         FirstEntity = npc.UID,
                         MovementType = TwoMovements.Jump
                     }) {
                client.SendScreen(jump);
            }

            return true;
        }

        private static bool HandleGotoNpcCommand(GameState client, string[] data) {
            if (data.Length < 2) {
                client.Send(new Message("Usage: @gotonpc <npc_id>", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            var npcId = uint.Parse(data[1]);
            INpc? foundNpc = null;

            // Search through all maps for the NPC
            foreach (var map in Kernel.Maps.Values) {
                if (!map.Npcs.TryGetValue(npcId, out var npc)) continue;
                foundNpc = npc;
                break;
            }

            if (foundNpc != null) {
                client.Entity.Teleport(foundNpc.MapID, foundNpc.X, foundNpc.Y);
            }
            else {
                client.Send(new Message("NPC with ID " + npcId + " not found!", System.Drawing.Color.Yellow,
                    Message.Tip));
            }

            return true;
        }

        private static Dictionary<string, string> ParseArguments(string[] data, string mess, int startIndex = 1) {
            var args = new Dictionary<string, string>();

            // For name parameter (-n), extract quoted value directly from original message to preserve casing
            var nameMatch = MyRegex().Match(mess);
            if (nameMatch.Success) {
                args["n"] = nameMatch.Groups[2].Value;
            }

            // For effect parameter (-e), extract quoted value directly from original message
            var effectMatch = MyRegex1().Match(mess);
            if (effectMatch.Success) {
                args["e"] = effectMatch.Groups[2].Value;
            }

            // Parse other arguments normally
            for (var i = startIndex; i < data.Length; i++) {
                if (!data[i].StartsWith($"-")) continue;
                var key = data[i][1..].ToLower();

                switch (key) {
                    // Skip -n if we already extracted it from the quoted string
                    case "n" when nameMatch.Success: {
                        // Skip the value tokens for -n
                        if (i + 1 < data.Length && !"-".StartsWith(data[i + 1])) {
                            // Check if it's a quoted value that spans multiple tokens
                            var firstValue = data[i + 1];
                            if ("'".StartsWith(firstValue) || "\"".StartsWith(firstValue)) {
                                var quoteChar = firstValue[0];
                                // Check if the quote is closed in the same token
                                if (firstValue.EndsWith(quoteChar.ToString()) && firstValue.Length > 1) {
                                    i++; // Skip single token with quoted value
                                }
                                else {
                                    // Count tokens until closing quote
                                    var tokensToSkip = 1;
                                    for (var j = i + 2; j < data.Length; j++) {
                                        tokensToSkip++;
                                        if (data[j].EndsWith(quoteChar.ToString()))
                                            break;
                                    }

                                    i += tokensToSkip;
                                }
                            }
                            else {
                                i++; // Skip single token value
                            }
                        }

                        continue;
                    }
                    // Skip -e if we already extracted it from the quoted string
                    case "e" when effectMatch.Success: {
                        // Skip the value tokens for -e
                        if (i + 1 < data.Length && !"-".StartsWith(data[i + 1])) {
                            // Check if it's a quoted value that spans multiple tokens
                            var firstValue = data[i + 1];
                            if ("'".StartsWith(firstValue) || "\"".StartsWith(firstValue)) {
                                var quoteChar = firstValue[0];
                                // Check if the quote is closed in the same token
                                if (firstValue.EndsWith(quoteChar.ToString()) && firstValue.Length > 1) {
                                    i++; // Skip single token with quoted value
                                }
                                else {
                                    // Count tokens until closing quote
                                    var tokensToSkip = 1;
                                    for (var j = i + 2; j < data.Length; j++) {
                                        tokensToSkip++;
                                        if (data[j].EndsWith(quoteChar.ToString()))
                                            break;
                                    }

                                    i += tokensToSkip;
                                }
                            }
                            else {
                                i++; // Skip single token value
                            }
                        }

                        continue;
                    }
                }

                if (i + 1 < data.Length && !"-".StartsWith(data[i + 1])) {
                    var value = data[i + 1];
                    // Strip quotes from value if present (for parameters not handled by regex)
                    if (("'".StartsWith(value) && "'".EndsWith(value)) ||
                        ("\"".StartsWith(value) && "\"".EndsWith(value))) {
                        value = value.Substring(1, value.Length - 2);
                    }

                    args[key] = value;
                    i++; // Skip the value in next iteration
                }
                else {
                    args[key] = "";
                }
            }

            return args;
        }

        private static bool HandleEditNpcCommand(GameState client, string[] data, string mess) {
            if (data.Length < 2) {
                client.Send(new Message("Usage: @editnpc <npc_id> [-n <name>] [-s <skin>] [-e <effect>|none]",
                    System.Drawing.Color.Yellow,
                    Message.Tip));
                client.Send(new Message("Example: @editnpc 100 -n 'New Name' -s 29680 -e ninjapk_third",
                    System.Drawing.Color.Yellow, Message.Tip));
                client.Send(new Message("To remove effect: @editnpc 100 -e none", System.Drawing.Color.Yellow,
                    Message.Tip));
                return true;
            }

            if (!uint.TryParse(data[1], out var npcId)) {
                client.Send(new Message("Invalid NPC ID!", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            INpc? foundNpc = null;
            Map? foundMap = null;

            // Search through all maps for the NPC
            foreach (var map in Kernel.Maps.Values) {
                if (!map.Npcs.TryGetValue(npcId, out var npc)) continue;
                foundNpc = npc;
                foundMap = map;
                break;
            }

            if (foundNpc == null) {
                client.Send(new Message("NPC with ID " + npcId + " not found!", System.Drawing.Color.Yellow,
                    Message.Tip));
                return true;
            }

            var args = ParseArguments(data, mess, 2); // Start from index 2 to skip NPC ID
            var hasChanges = false;
            var changes = new List<string>();

            // Update name if provided
            if (args.TryGetValue("n", out var nameValue) && !string.IsNullOrEmpty(nameValue)) {
                var oldName = foundNpc.Name;
                foundNpc.Name = nameValue;
                changes.Add("name: " + oldName + " -> " + nameValue);
                hasChanges = true;
            }

            // Update skin/mesh if provided
            if (args.TryGetValue("s", out var skinValue) && !string.IsNullOrEmpty(skinValue)) {
                if (ushort.TryParse(skinValue, out var meshId)) {
                    foundNpc.Mesh = meshId;
                    changes.Add("skin: " + meshId);
                    hasChanges = true;
                }
                else {
                    client.Send(new Message("Invalid skin/mesh ID!", System.Drawing.Color.Yellow, Message.Tip));
                    return true;
                }
            }

            // Update effect if provided (empty string or "none" removes effect)
            if (args.ContainsKey("e")) {
                if (string.IsNullOrEmpty(args["e"]) ||
                    args["e"].Equals("none", StringComparison.CurrentCultureIgnoreCase)) {
                    changes.Add("effect: removed");
                }
                else {
                    changes.Add("effect: " + args["e"]);
                }

                hasChanges = true;
            }

            if (!hasChanges) {
                client.Send(new Message("No changes specified. Use -n, -s, or -e to modify NPC.",
                    System.Drawing.Color.Yellow,
                    Message.Tip));
                return true;
            }

            // Update database
            try {
                using (var cmd = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE)) {
                    var updateCmd = cmd.Update("npcs");

                    if (args.TryGetValue("n", out var dbNameValue) && !string.IsNullOrEmpty(dbNameValue)) {
                        updateCmd.Set("name", dbNameValue);
                    }

                    if (args.TryGetValue("s", out var dbSkinValue) && !string.IsNullOrEmpty(dbSkinValue) &&
                        ushort.TryParse(dbSkinValue, out var meshId)) {
                        updateCmd.Set("lookface", meshId);
                    }

                    if (args.ContainsKey("e")) {
                        // Empty string or "none" removes the effect
                        var effectValue = (string.IsNullOrEmpty(args["e"]) || args["e"].ToLower() == "none")
                            ? ""
                            : args["e"];
                        updateCmd.Set("effect", effectValue);
                        // Also update the NPC object in memory
                        foundNpc.effect = effectValue;
                    }

                    updateCmd.Where("id", npcId).Execute();
                }

                // Reload screens for all players on the map
                foreach (var player in Kernel.GamePool.Values) {
                    if (foundMap != null && player.Entity.MapID != foundMap.ID) continue;
                    player.Screen.FullWipe();
                    player.Screen.Reload();
                }

                client.Send(new Message(
                    "NPC [" + (foundNpc.Name) + "] updated: " + string.Join(", ", changes),
                    System.Drawing.Color.Green, Message.Tip));
            }
            catch (Exception ex) {
                client.Send(new Message("Error updating NPC in database: " + ex.Message,
                    System.Drawing.Color.Yellow, Message.Tip));
            }

            return true;
        }

        private static bool HandleMoveNpcCommand(GameState client, string[] data) {
            if (data.Length < 2) {
                client.Send(new Message("Usage: @movenpc <npc_id> [x] [y]", System.Drawing.Color.Yellow, Message.Tip));
                client.Send(new Message("Example: @movenpc 1520 (moves to your position)", System.Drawing.Color.Yellow, Message.Tip));
                client.Send(new Message("Example: @movenpc 1520 50 50 (moves to x50 y50)", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            var npcId = uint.Parse(data[1]);
            INpc? foundNpc = null;
            Map? foundMap = null;

            // Search through all maps for the NPC
            foreach (var map in Kernel.Maps.Values) {
                if (!map.Npcs.TryGetValue(npcId, out var npc)) continue;
                foundNpc = npc;
                foundMap = map;
                break;
            }

            if (foundNpc == null) {
                client.Send(new Message("NPC with ID " + npcId + " not found!", System.Drawing.Color.Yellow,
                    Message.Tip));
                return true;
            }

            // Determine target position
            ushort targetX, targetY;
            bool usePlayerPosition = true;

            if (data.Length >= 4) {
                // Try to parse x and y coordinates
                if (ushort.TryParse(data[2], out var x) && ushort.TryParse(data[3], out var y)) {
                    targetX = x;
                    targetY = y;
                    usePlayerPosition = false;
                }
                else {
                    client.Send(new Message("Invalid x or y coordinate! Using your position instead.", System.Drawing.Color.Yellow, Message.Tip));
                    targetX = client.Entity.X;
                    targetY = client.Entity.Y;
                }
            }
            else {
                // Use player's position
                targetX = client.Entity.X;
                targetY = client.Entity.Y;
            }

            // Remove NPC from old map
            foundMap?.Npcs.Remove(npcId);

            // Update NPC position
            foundNpc.X = targetX;
            foundNpc.Y = targetY;
            foundNpc.MapID = client.Entity.MapID;

            // Add NPC to current map
            var targetMap = Kernel.Maps[client.Entity.MapID];
            targetMap.Npcs.Remove(npcId);
            targetMap.Npcs.Add(npcId, foundNpc);

            // Update database
            try {
                using (var cmd = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE)) {
                    cmd.Update("npcs")
                        .Set("cellx", foundNpc.X)
                        .Set("celly", foundNpc.Y)
                        .Set("mapid", foundNpc.MapID)
                        .Where("id", npcId)
                        .Execute();
                }

                // Reload screens for all players on the map
                foreach (var player in Kernel.GamePool.Values) {
                    if (player.Entity.MapID != client.Entity.MapID) continue;
                    player.Screen.FullWipe();
                    player.Screen.Reload();
                }

                var moveMessage = usePlayerPosition
                    ? "NPC [" + (foundNpc.Name) + "] moved to your position"
                    : "NPC [" + (foundNpc.Name) + "] moved to position (" + targetX + ", " + targetY + ")";
                client.Send(new Message(moveMessage, System.Drawing.Color.Green, Message.Tip));
            }
            catch (Exception ex) {
                client.Send(new Message("Error updating NPC in database: " + ex.Message,
                    System.Drawing.Color.Yellow, Message.Tip));
            }

            return true;
        }

        private static bool HandleDeleteNpcCommand(GameState client, string[] data) {
            if (data.Length < 2) {
                client.Send(new Message("Usage: @deletenpc <npc_id>", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            var npcId = uint.Parse(data[1]);
            INpc? foundNpc = null;
            Map? foundMap = null;

            // Search through all maps for the NPC
            foreach (var map in Kernel.Maps.Values) {
                if (!map.Npcs.TryGetValue(npcId, out var npc)) continue;
                foundNpc = npc;
                foundMap = map;
                break;
            }

            if (foundNpc == null) {
                client.Send(new Message("NPC with ID " + npcId + " not found!", System.Drawing.Color.Yellow,
                    Message.Tip));
                return true;
            }

            // Remove NPC from map
            foundMap?.RemoveNpc(foundNpc);

            // Delete from database
            try {
                using (var cmd = new Database.MySqlCommand(Database.MySqlCommandType.DELETE)) {
                    cmd.Delete("npcs", "id", npcId).Execute();
                }

                // Reload screens for all players on the map
                foreach (var player in Kernel.GamePool.Values) {
                    if (foundMap != null && player.Entity.MapID != foundMap.ID) continue;
                    player.Screen.FullWipe();
                    player.Screen.Reload();
                }

                client.Send(new Message("NPC [" + (foundNpc.Name) + "] deleted successfully",
                    System.Drawing.Color.Green, Message.Tip));
            }
            catch (Exception ex) {
                client.Send(new Message("Error deleting NPC from database: " + ex.Message,
                    System.Drawing.Color.Yellow, Message.Tip));
            }

            return true;
        }

        private static bool HandleAddNpcCommand(GameState client, string[] data, string mess) {
            if (data.Length < 2) {
                client.Send(new Message("Usage: @addnpc -n <name> -s <skin> [-e <effect>] [-t]",
                    System.Drawing.Color.Yellow,
                    Message.Tip));
                client.Send(new Message("Example: @addnpc -n test -s 1002 -e ninjapk_third",
                    System.Drawing.Color.Yellow,
                    Message.Tip));
                client.Send(new Message(
                    "Use quotes for values with spaces: @addnpc -n \"Some Name\" -s 1002 -e \"effect name\"",
                    System.Drawing.Color.Yellow,
                    Message.Tip));
                client.Send(new Message(
                    "Use -t to create temporary NPC (not saved to database): @addnpc -n test -s 1002 -t",
                    System.Drawing.Color.Yellow,
                    Message.Tip));
                return true;
            }

            var args = ParseArguments(data, mess);
            var isTemporary = args.ContainsKey("t");

            // Get name
            string name;
            if (args.TryGetValue("n", out var nameValue) && !string.IsNullOrEmpty(nameValue)) {
                name = nameValue;
            }
            else {
                client.Send(new Message("Name (-n) is required!", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            // Get skin/mesh
            ushort mesh;
            if (args.TryGetValue("s", out var skinValue) && !string.IsNullOrEmpty(skinValue)) {
                if (!ushort.TryParse(skinValue, out mesh)) {
                    client.Send(new Message("Invalid skin/mesh ID!", System.Drawing.Color.Yellow, Message.Tip));
                    return true;
                }
            }
            else {
                client.Send(new Message("Skin (-s) is required!", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            // Get effect (optional)
            var effect = "";
            if (args.TryGetValue("e", out var effectValue) && !string.IsNullOrEmpty(effectValue)) {
                effect = effectValue;
            }

            // Default type to Talker
            byte type = 2;

            // Generate a unique NPC ID in range 1-99999
            var existingIds = new HashSet<uint>();

            // Collect existing IDs from database (for permanent NPCs)
            if (!isTemporary) {
                try {
                    using var cmd = new Database.MySqlCommand(Database.MySqlCommandType.SELECT);
                    cmd.Select("npcs");
                    using var reader = new Database.MySqlReader(cmd);
                    while (reader.Read()) {
                        var existingId = reader.ReadUInt32("id");
                        if (existingId is >= 1 and <= 99999) {
                            existingIds.Add(existingId);
                        }
                    }
                }
                catch {
                    // If query fails, continue with memory check only
                }
            }

            // Collect existing IDs from memory (for both temporary and permanent NPCs)
            foreach (var existingNpcId in Kernel.Maps.Values.SelectMany(map => map.Npcs.Keys)) {
                if (existingNpcId is >= 1 and <= 99999) {
                    existingIds.Add(existingNpcId);
                }
            }

            // Find first available ID in range 1-99999 (except 12)
            uint npcId = 0;
            for (uint id = 1; id <= 99999; id++) {
                if (id == 12) continue; // Skip ID 12 (warehouse)
                if (!existingIds.Contains(id)) {
                    npcId = id;
                    break;
                }
            }

            if (npcId == 0) {
                client.Send(new Message("No available NPC ID found in range 1-99999. All IDs are in use!",
                    System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            // Create new NPC
            INpc npc = new NpcSpawn {
                UID = npcId,
                Mesh = mesh,
                Type = (Enums.NpcType)type,
                X = client.Entity.X,
                Y = client.Entity.Y,
                MapID = client.Entity.MapID,
                Name = name,
                effect = effect
            };

            // Add to map
            client.Map.AddNpc(npc);

            // Insert into database only if not temporary
            if (!isTemporary) {
                try {
                    using (var cmd = new Database.MySqlCommand(Database.MySqlCommandType.INSERT)) {
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
                    foreach (var player in Kernel.GamePool.Values) {
                        if (player.Entity.MapID != client.Entity.MapID) continue;
                        player.Screen.FullWipe();
                        player.Screen.Reload();
                    }

                    var successMsg = "NPC [" + name + "] created with ID " + npcId + " (skin: " + mesh + ")";
                    if (!string.IsNullOrEmpty(effect)) {
                        successMsg += " (effect: " + effect + ")";
                    }

                    client.Send(new Message(successMsg, System.Drawing.Color.Green, Message.Tip));
                }
                catch (Exception ex) {
                    // Remove from map if database insert failed
                    client.Map.RemoveNpc(npc);
                    client.Send(new Message("Error saving NPC to database: " + ex.Message, System.Drawing.Color.Yellow,
                        Message.Tip));
                }
            }
            else {
                // Temporary NPC - just reload screens
                foreach (var player in Kernel.GamePool.Values) {
                    if (player.Entity.MapID != client.Entity.MapID) continue;
                    player.Screen.FullWipe();
                    player.Screen.Reload();
                }

                var successMsg = "Temporary NPC [" + name + "] created with ID " + npcId + " (skin: " + mesh + ")";
                if (!string.IsNullOrEmpty(effect)) {
                    successMsg += " (effect: " + effect + ")";
                }

                successMsg += " [Not saved to database]";
                client.Send(new Message(successMsg, System.Drawing.Color.Green, Message.Tip));
            }

            return true;
        }

        private static bool HandleReloadNpcsCommand(GameState client, string[] data) {
            try {
                if (data.Length > 1 && (data[1].ToLower() == "-m" || data[1].ToLower() == "-map")) {
                    // Reload only current map
                    var map = client.Map;

                    // Clear all NPCs from this map
                    var npcsToRemove = new List<INpc>(map.Npcs.Values);
                    foreach (var npc in npcsToRemove) {
                        map.RemoveNpc(npc);
                    }

                    map.Npcs.Clear();

                    // Reload NPCs from database
                    map.LoadNpcs();
                    var npcsReloaded = map.Npcs.Count;

                    // Reload screens for all players on this map
                    foreach (var player in Kernel.GamePool.Values) {
                        if (player.Entity.MapID != map.ID) continue;
                        player.Screen.FullWipe();
                        player.Screen.Reload();
                    }

                    client.Send(new Message(
                        $"NPCs reloaded for current map (Map ID: {map.ID})! {npcsReloaded} NPC(s) loaded from database.",
                        System.Drawing.Color.Green, Message.Tip));
                }
                else {
                    // Reload all maps
                    var mapsReloaded = 0;
                    var npcsReloaded = 0;

                    // Reload NPCs for all maps
                    foreach (var map in Kernel.Maps.Values) {
                        // Clear all NPCs from this map
                        var npcsToRemove = new List<INpc>(map.Npcs.Values);
                        foreach (var npc in npcsToRemove) {
                            map.RemoveNpc(npc);
                        }

                        map.Npcs.Clear();

                        // Reload NPCs from database
                        map.LoadNpcs();
                        mapsReloaded++;
                        npcsReloaded += map.Npcs.Count;
                    }

                    // Reload screens for all players on all maps
                    foreach (var player in Kernel.GamePool.Values) {
                        player.Screen.FullWipe();
                        player.Screen.Reload();
                    }

                    client.Send(new Message(
                        $"NPCs reloaded successfully! {mapsReloaded} map(s) reloaded, {npcsReloaded} NPC(s) loaded from database.",
                        System.Drawing.Color.Green, Message.Tip));
                }
            }
            catch (Exception ex) {
                client.Send(new Message("Error reloading NPCs: " + ex.Message,
                    System.Drawing.Color.Yellow, Message.Tip));
            }

            return true;
        }

        [GeneratedRegex(@"-n\s+(['""])(.*?)\1", RegexOptions.IgnoreCase, "en-GB")]
        private static partial Regex MyRegex();
        [GeneratedRegex(@"-e\s+(['""])(.*?)\1", RegexOptions.IgnoreCase, "en-GB")]
        private static partial Regex MyRegex1();
    }
}