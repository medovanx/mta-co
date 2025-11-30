using System;
using MTA.Network.GamePackets;

namespace MTA.Client.Commands
{
    public static class TeleportCommands
    {
        public static bool HandleCommand(GameState client, string[] data, string mess)
        {
            switch (data[0])
            {
                case "gotoplayer":
                    return HandleGotoPlayerCommand(client, data, mess);

                case "summon":
                    return HandleSummonCommand(client, data, mess);

                case "summonall":
                    return HandleSummonAllCommand(client, data, mess);

                case "teleport":
                    return HandleTeleportCommand(client, data, mess);

                default:
                    return false;
            }
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
                client.Send(new Message("Usage: @teleport <mapid> [x] [y]", System.Drawing.Color.Red, Message.Tip));
                client.Send(new Message("Example: @teleport 1002 432 378", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

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
    }
}
