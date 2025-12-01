using System;
using System.Linq;
using MTA.Network.GamePackets;
using MTA.Interfaces;
using MTA.Game;

namespace MTA.Client.Commands
{
    public static class NpcCommands
    {
        public static bool HandleCommand(GameState client, string[] data, string mess)
        {
            return (global::System.String)data[0] switch
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
                client.Send(new Message("Usage: @addnpc <mesh> <type> [name]", System.Drawing.Color.Yellow,
                    Message.Tip));
                client.Send(new Message("Example: @addnpc 100 2 TestNPC", System.Drawing.Color.Yellow,
                    Message.Tip));
                return true;
            }

            ushort mesh = ushort.Parse(data[1]);
            byte type = byte.Parse(data[2]);
            string name = data.Length >= 4
                ? mess.Substring(data[0].Length + data[1].Length + data[2].Length + 3)
                : "NPC";

            // Generate a unique NPC ID
            uint npcId = 100000; // Start from 100000
            try
            {
                using (var cmd = new Database.MySqlCommand(Database.MySqlCommandType.SELECT))
                {
                    cmd.Select("npcs");
                    using (var reader = new Database.MySqlReader(cmd))
                    {
                        while (reader.Read())
                        {
                            uint existingId = reader.ReadUInt32("id");
                            if (existingId >= npcId)
                                npcId = existingId + 1;
                        }
                    }
                }

                // Also check sobnpcs table
                using (var cmd = new Database.MySqlCommand(Database.MySqlCommandType.SELECT))
                {
                    cmd.Select("sobnpcs");
                    using (var reader = new Database.MySqlReader(cmd))
                    {
                        while (reader.Read())
                        {
                            uint existingId = reader.ReadUInt32("id");
                            if (existingId >= npcId)
                                npcId = existingId + 1;
                        }
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
            INpc npc = new NpcSpawn();
            npc.UID = npcId;
            npc.Mesh = mesh;
            npc.Type = (Enums.NpcType)type;
            npc.X = client.Entity.X;
            npc.Y = client.Entity.Y;
            npc.MapID = client.Entity.MapID;
            npc.Name = name;

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

                client.Send(new Message(
                    "NPC [" + name + "] (ID: " + npcId + ") created successfully at your position",
                    System.Drawing.Color.Green, Message.Tip));
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


