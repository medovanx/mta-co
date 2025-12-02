using System;
using MTA.Network.GamePackets;

namespace MTA.Client.Commands
{
    public static class TeleportCommands
    {
        public static bool HandleCommand(GameState client, string[] data, string mess)
        {
            return data[0] switch
            {
                "gotoplayer" => HandleGotoPlayerCommand(client, data, mess),
                "summon" => HandleSummonCommand(client, data, mess),
                "summonall" => HandleSummonAllCommand(client, data, mess),
                "teleport" => HandleTeleportCommand(client, data, mess),
                "map" => HandleMapCommand(client, data, mess),
                _ => false
            };
        }

        private static bool HandleGotoPlayerCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                client.Send(new Message("Usage: @gotoplayer <player_name>", System.Drawing.Color.Red, Message.Tip));
                return true;
            }

            string playerName = mess.Substring(data[0].Length + 1).Trim();

            if (!EntityCommands.FindPlayerByName(playerName, out GameState targetPlayer, out int matchCount))
            {
                if (matchCount == 0)
                {
                    client.Send(new Message("Player [" + playerName + "] not found or offline!",
                        System.Drawing.Color.Red, Message.Tip));
                }
                else
                {
                    client.Send(new Message("Multiple players found matching [" + playerName + "]. Please be more specific.",
                        System.Drawing.Color.Red, Message.Tip));
                }
                return true;
            }

            if (targetPlayer.Entity != null)
            {
                client.Entity.Teleport(targetPlayer.Entity.MapID, targetPlayer.Entity.X, targetPlayer.Entity.Y);
                client.Send(new Message("Teleported to player [" + targetPlayer.Entity.Name + "]",
                    System.Drawing.Color.Green, Message.Tip));
            }

            return true;
        }

        private static bool HandleSummonCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                client.Send(new Message("Usage: @summon <player_name>", System.Drawing.Color.Red, Message.Tip));
                return true;
            }

            string playerName = mess.Substring(data[0].Length + 1).Trim();

            if (!EntityCommands.FindPlayerByName(playerName, out GameState targetPlayer, out int matchCount))
            {
                if (matchCount == 0)
                {
                    client.Send(new Message("Player [" + playerName + "] not found or offline!",
                        System.Drawing.Color.Red, Message.Tip));
                }
                else
                {
                    client.Send(new Message("Multiple players found matching [" + playerName + "]. Please be more specific.",
                        System.Drawing.Color.Red, Message.Tip));
                }
                return true;
            }

            if (targetPlayer.Entity != null)
            {
                targetPlayer.Entity.Teleport(client.Entity.MapID, client.Entity.X, client.Entity.Y);
                targetPlayer.Send(new Message("You have been summoned by [" + client.Entity.Name + "]",
                    System.Drawing.Color.Green, Message.Tip));
                client.Send(new Message("Brought player [" + targetPlayer.Entity.Name + "] to you",
                    System.Drawing.Color.Green, Message.Tip));
            }

            return true;
        }

        private static bool HandleSummonAllCommand(GameState client, string[] data, string mess)
        {
            int count = 0;
            foreach (var player in Kernel.GamePool.Values)
            {
                if (player.Entity != null && player != client)
                {
                    player.Entity.Teleport(client.Entity.MapID, client.Entity.X, client.Entity.Y);
                    count++;
                }
            }

            client.Send(new Message($"Summoned {count} player(s) to your location.",
                System.Drawing.Color.Green, Message.Tip));
            return true;
        }

        private static bool HandleTeleportCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                client.Send(new Message("Usage: @teleport <mapid/city> [x] [y]", System.Drawing.Color.Red, Message.Tip));
                client.Send(new Message("Example: @teleport 1002 432 378 or @teleport tc", System.Drawing.Color.Yellow, Message.Tip));
                client.Send(new Message("Cities: tc, pc, ac, dc, bc", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            // Check if it's a location shortcut
            string destination = data[1].ToLower();
            switch (destination)
            {
                case "twin":
                    client.Entity.Teleport(1002, 303, 281);
                    return true;
                case "phoenix":
                    client.Entity.Teleport(1011, 193, 266);
                    return true;
                case "ape":
                    client.Entity.Teleport(1020, 560, 546);
                    return true;
                case "desert":
                    client.Entity.Teleport(1000, 499, 648);
                    return true;
                case "bird":
                    client.Entity.Teleport(1015, 723, 573);
                    return true;
                case "guild":
                    client.Entity.Teleport(1038, 087, 095);
                    return true;
                case "jobcenter":
                    client.Entity.Teleport(1004, 43, 49);
                    return true;
            }

            // Otherwise, treat as map ID with optional coordinates
            if (!ushort.TryParse(data[1], out ushort mapId))
            {
                client.Send(new Message("Invalid map ID. Must be a number between 0 and 65535.",
                    System.Drawing.Color.Red, Message.Tip));
                return true;
            }

            ushort x = 0;
            ushort y = 0;

            // Parse X coordinate if provided
            if (data.Length >= 3)
            {
                if (!ushort.TryParse(data[2], out x))
                {
                    client.Send(new Message("Invalid X coordinate. Must be a number between 0 and 65535.",
                        System.Drawing.Color.Red, Message.Tip));
                    return true;
                }
            }

            // Parse Y coordinate if provided
            if (data.Length >= 4)
            {
                if (!ushort.TryParse(data[3], out y))
                {
                    client.Send(new Message("Invalid Y coordinate. Must be a number between 0 and 65535.",
                        System.Drawing.Color.Red, Message.Tip));
                    return true;
                }
            }

            // If coordinates not provided, use default spawn location or current location
            if (x == 0 && y == 0)
            {
                // Try to get default spawn location from map, or use current location
                if (Kernel.Maps.ContainsKey(mapId))
                {
                    // Use a safe default location (center-ish)
                    x = 100;
                    y = 100;
                }
            }

            client.Entity.Teleport(mapId, x, y);
            return true;
        }

        private static bool HandleMapCommand(GameState client, string[] data, string mess)
        {
            if (client.Entity != null)
            {
                client.Send(new Message($"Current Map ID: {client.Entity.MapID} | Position: ({client.Entity.X}, {client.Entity.Y})",
                    System.Drawing.Color.Cyan, Message.Tip));
            }
            return true;
        }
    }
}
