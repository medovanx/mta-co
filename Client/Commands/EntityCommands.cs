using System;

namespace MTA.Client.Commands
{
    public static class EntityCommands
    {
        public static bool HandleCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                return false;
            }

            switch (data[0])
            {
                case "gold":
                    return handleGoldCommand(client, data, mess);

                case "cps":
                    return handleCpsCommand(client, data, mess);

                case "bcps":
                    return handleBcpsCommand(client, data, mess);

                case "vip":
                    return HandleVipCommand(client, data, mess);

                case "exp":
                    return HandleExpCommand(client, data, mess);

                case "racepoints":
                    return HandleRacePointsCommand(client, data, mess);

                case "honorpoints":
                    return HandleHonorPointsCommand(client, data, mess);

                case "studypoints":
                    return HandleStudyPointsCommand(client, data, mess);

                case "level":
                    return HandleLevelCommand(client, data, mess);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Finds a player by name (supports partial matching and handles spaces)
        /// </summary>
        /// <param name="searchName">The name to search for (can be partial)</param>
        /// <param name="foundPlayer">The found player, or null if not found or multiple matches</param>
        /// <param name="matchCount">Number of players that matched</param>
        /// <returns>True if exactly one player was found, false otherwise</returns>
        public static bool FindPlayerByName(string searchName, out GameState foundPlayer, out int matchCount)
        {
            foundPlayer = null;
            matchCount = 0;

            if (string.IsNullOrWhiteSpace(searchName))
                return false;

            // Remove spaces from search term for easier matching
            string searchTerm = searchName.Replace(" ", "");

            foreach (var player in Kernel.GamePool.Values)
            {
                if (player.Entity != null)
                {
                    // Remove spaces from player name for comparison
                    string playerNameClean = player.Entity.Name.Replace(" ", "");

                    if (playerNameClean.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        foundPlayer = player;
                        matchCount++;
                        // If exact match, use it immediately
                        if (playerNameClean.Equals(searchTerm, StringComparison.OrdinalIgnoreCase))
                        {
                            matchCount = 1;
                            break;
                        }
                    }
                }
            }

            return matchCount == 1;
        }

        private static bool handleGoldCommand(GameState client, string[] data, string mess)
        {
            const ulong maxGold = 9999999999UL; // 9,999,999,999
            GameState targetClient = client;

            // Check if second parameter is a player name
            if (data.Length >= 2 && !ulong.TryParse(data[1], out _))
            {
                // Extract player name - everything except the last parameter (which should be the amount)
                ulong amount = 0;

                // Try to find the amount parameter (should be the last numeric parameter)
                for (int i = data.Length - 1; i >= 1; i--)
                {
                    if (ulong.TryParse(data[i], out amount))
                    {
                        break;
                    }
                }

                if (amount == 0)
                {
                    client.Send(new Network.GamePackets.Message("Usage: @gold <player_name> <amount> or @gold <amount>", System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Get player name - everything except the last parameter (amount)
                string playerNameSearch = mess.Substring(data[0].Length + 1).Trim();
                int lastSpaceIndex = playerNameSearch.LastIndexOf(' ');
                if (lastSpaceIndex >= 0)
                {
                    playerNameSearch = playerNameSearch.Substring(0, lastSpaceIndex).Trim();
                }

                if (string.IsNullOrWhiteSpace(playerNameSearch))
                {
                    client.Send(new Network.GamePackets.Message("Usage: @gold <player_name> <amount> or @gold <amount>", System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Search for player using utility function
                if (!FindPlayerByName(playerNameSearch, out GameState foundPlayer, out int matchCount))
                {
                    if (matchCount == 0)
                    {
                        client.Send(new Network.GamePackets.Message($"Player matching [{playerNameSearch}] not found or offline!",
                            System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    }
                    else
                    {
                        client.Send(new Network.GamePackets.Message($"Multiple players found matching [{playerNameSearch}]. Please be more specific.",
                            System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    }
                    return true;
                }

                targetClient = foundPlayer;

                if (amount > maxGold)
                {
                    client.Send(new Network.GamePackets.Message($"Maximum gold amount is {maxGold:N0}", System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                targetClient.Entity.Money = amount;
                targetClient.Send(new Network.GamePackets.Message($"Money set to {amount:N0} by {client.Entity.Name}",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                client.Send(new Network.GamePackets.Message($"Money set to {amount:N0} for [{targetClient.Entity.Name}]",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                return true;
            }

            // Self gold command
            if (!ulong.TryParse(data[1], out var selfAmount))
            {
                client.Send(new Network.GamePackets.Message("Usage: @gold <amount> or @gold <player_name> <amount>", System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            if (selfAmount > maxGold)
            {
                client.Send(new Network.GamePackets.Message($"Maximum gold amount is {maxGold:N0}", System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            client.Entity.Money = selfAmount;
            client.Send(new Network.GamePackets.Message($"Money set to {selfAmount:N0}", System.Drawing.Color.Green,
                Network.GamePackets.Message.Tip));
            return true;
        }

        private static bool handleCpsCommand(GameState client, string[] data, string mess)
        {
            const uint maxCps = 999999999U; // 999,999,999
            GameState targetClient = client;

            // Check if second parameter is a player name
            if (data.Length >= 2 && !ulong.TryParse(data[1], out _))
            {
                // Extract player name - everything except the last parameter (which should be the amount)
                ulong amountLong = 0;

                // Try to find the amount parameter (should be the last numeric parameter)
                for (int i = data.Length - 1; i >= 1; i--)
                {
                    if (ulong.TryParse(data[i], out amountLong))
                    {
                        break;
                    }
                }

                if (amountLong == 0)
                {
                    client.Send(new Network.GamePackets.Message("Usage: @cps <player_name> <amount> or @cps <amount>", System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Get player name - everything except the last parameter (amount)
                string playerNameSearch = mess.Substring(data[0].Length + 1).Trim();
                int lastSpaceIndex = playerNameSearch.LastIndexOf(' ');
                if (lastSpaceIndex >= 0)
                {
                    playerNameSearch = playerNameSearch.Substring(0, lastSpaceIndex).Trim();
                }

                if (string.IsNullOrWhiteSpace(playerNameSearch))
                {
                    client.Send(new Network.GamePackets.Message("Usage: @cps <player_name> <amount> or @cps <amount>", System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Search for player using utility function
                if (!FindPlayerByName(playerNameSearch, out GameState foundPlayer, out int matchCount))
                {
                    if (matchCount == 0)
                    {
                        client.Send(new Network.GamePackets.Message($"Player matching [{playerNameSearch}] not found or offline!",
                            System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    }
                    else
                    {
                        client.Send(new Network.GamePackets.Message($"Multiple players found matching [{playerNameSearch}]. Please be more specific.",
                            System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    }
                    return true;
                }

                targetClient = foundPlayer;

                if (amountLong > maxCps)
                {
                    client.Send(new Network.GamePackets.Message($"Maximum CPs amount is {maxCps:N0}", System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                targetClient.Entity.ConquerPoints = (uint)amountLong;
                targetClient.Send(new Network.GamePackets.Message($"Conquer Points set to {amountLong:N0} by {client.Entity.Name}",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                client.Send(new Network.GamePackets.Message($"Conquer Points set to {amountLong:N0} for [{targetClient.Entity.Name}]",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                return true;
            }

            // Self CPs command
            if (!ulong.TryParse(data[1], out var selfAmountLong))
            {
                client.Send(new Network.GamePackets.Message("Usage: @cps <amount> or @cps <player_name> <amount>", System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            if (selfAmountLong > maxCps)
            {
                client.Send(new Network.GamePackets.Message($"Maximum CPs amount is {maxCps:N0}", System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            client.Entity.ConquerPoints = (uint)selfAmountLong;
            client.Send(new Network.GamePackets.Message($"Conquer Points set to {selfAmountLong:N0}", System.Drawing.Color.Green,
                Network.GamePackets.Message.Tip));
            return true;
        }

        private static bool handleBcpsCommand(GameState client, string[] data, string mess)
        {
            const uint maxBcps = 999999999U; // 999,999,999
            GameState targetClient = client;

            // Check if second parameter is a player name
            if (data.Length >= 2 && !ulong.TryParse(data[1], out _))
            {
                // Extract player name - everything except the last parameter (which should be the amount)
                ulong amountLong = 0;

                // Try to find the amount parameter (should be the last numeric parameter)
                for (int i = data.Length - 1; i >= 1; i--)
                {
                    if (ulong.TryParse(data[i], out amountLong))
                    {
                        break;
                    }
                }

                if (amountLong == 0)
                {
                    client.Send(new Network.GamePackets.Message("Usage: @bcps <player_name> <amount> or @bcps <amount>", System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Get player name - everything except the last parameter (amount)
                string playerNameSearch = mess.Substring(data[0].Length + 1).Trim();
                int lastSpaceIndex = playerNameSearch.LastIndexOf(' ');
                if (lastSpaceIndex >= 0)
                {
                    playerNameSearch = playerNameSearch.Substring(0, lastSpaceIndex).Trim();
                }

                if (string.IsNullOrWhiteSpace(playerNameSearch))
                {
                    client.Send(new Network.GamePackets.Message("Usage: @bcps <player_name> <amount> or @bcps <amount>", System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Search for player using utility function
                if (!FindPlayerByName(playerNameSearch, out GameState foundPlayer, out int matchCount))
                {
                    if (matchCount == 0)
                    {
                        client.Send(new Network.GamePackets.Message($"Player matching [{playerNameSearch}] not found or offline!",
                            System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    }
                    else
                    {
                        client.Send(new Network.GamePackets.Message($"Multiple players found matching [{playerNameSearch}]. Please be more specific.",
                            System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    }
                    return true;
                }

                targetClient = foundPlayer;

                if (amountLong > maxBcps)
                {
                    client.Send(new Network.GamePackets.Message($"Maximum Bound CPs amount is {maxBcps:N0}", System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                targetClient.Entity.BoundCps = (uint)amountLong;
                targetClient.Send(new Network.GamePackets.Message($"Bound CPs set to {amountLong:N0} by {client.Entity.Name}",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                client.Send(new Network.GamePackets.Message($"Bound CPs set to {amountLong:N0} for [{targetClient.Entity.Name}]",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                return true;
            }

            // Self BCPs command
            if (!ulong.TryParse(data[1], out var selfAmountLong))
            {
                client.Send(new Network.GamePackets.Message("Usage: @bcps <amount> or @bcps <player_name> <amount>", System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            if (selfAmountLong > maxBcps)
            {
                client.Send(new Network.GamePackets.Message($"Maximum Bound CPs amount is {maxBcps:N0}", System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            client.Entity.BoundCps = (uint)selfAmountLong;
            client.Send(new Network.GamePackets.Message($"Bound CPs set to {selfAmountLong:N0}", System.Drawing.Color.Green,
                Network.GamePackets.Message.Tip));
            return true;
        }

        private static bool HandleVipCommand(GameState client, string[] data, string mess)
        {
            const byte maxVipLevel = 6;
            GameState targetClient = client;

            // Check if second parameter is a player name
            if (data.Length >= 2 && !byte.TryParse(data[1], out _))
            {
                // Extract player name - everything except the last parameter (which should be the level)
                byte level = 0;

                // Try to find the level parameter (should be the last numeric parameter)
                for (int i = data.Length - 1; i >= 1; i--)
                {
                    if (byte.TryParse(data[i], out level))
                    {
                        break;
                    }
                }

                if (level == 0 && data.Length < 3)
                {
                    client.Send(new Network.GamePackets.Message("Usage: @vip <player_name> <level> (0-6)", System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Get player name - everything except the last parameter (level)
                string playerNameSearch = mess.Substring(data[0].Length + 1).Trim();
                if (data.Length >= 3)
                {
                    // Remove the level from the end
                    int lastSpaceIndex = playerNameSearch.LastIndexOf(' ');
                    if (lastSpaceIndex >= 0)
                    {
                        playerNameSearch = playerNameSearch.Substring(0, lastSpaceIndex).Trim();
                    }
                }

                if (string.IsNullOrWhiteSpace(playerNameSearch))
                {
                    client.Send(new Network.GamePackets.Message("Usage: @vip <player_name> <level> (0-6)", System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Search for player using utility function
                if (!FindPlayerByName(playerNameSearch, out GameState foundPlayer, out int matchCount))
                {
                    if (matchCount == 0)
                    {
                        client.Send(new Network.GamePackets.Message($"Player matching [{playerNameSearch}] not found or offline!",
                            System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    }
                    else
                    {
                        client.Send(new Network.GamePackets.Message($"Multiple players found matching [{playerNameSearch}]. Please be more specific.",
                            System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    }
                    return true;
                }

                targetClient = foundPlayer;

                if (level > maxVipLevel)
                {
                    client.Send(new Network.GamePackets.Message($"Maximum VIP level is {maxVipLevel}", System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                targetClient.Entity.VIPLevel = level;
                Network.GamePackets.VipStatus vip = new Network.GamePackets.VipStatus();
                targetClient.Send(vip.ToArray());
                targetClient.Screen.FullWipe();
                targetClient.Screen.Reload();
                targetClient.Send(new Network.GamePackets.Message($"VIP level set to {level} by {client.Entity.Name}",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                client.Send(new Network.GamePackets.Message($"VIP level set to {level} for [{targetClient.Entity.Name}]",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                return true;
            }

            // Self VIP command
            if (!byte.TryParse(data[1], out var selfLevel))
            {
                client.Send(new Network.GamePackets.Message("Usage: @vip <level> (0-6) or @vip <player_name> <level>", System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            if (selfLevel > maxVipLevel)
            {
                client.Send(new Network.GamePackets.Message($"Maximum VIP level is {maxVipLevel}", System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            client.Entity.VIPLevel = selfLevel;
            Network.GamePackets.VipStatus vipSelf = new Network.GamePackets.VipStatus();
            client.Send(vipSelf.ToArray());
            client.Screen.FullWipe();
            client.Screen.Reload();
            client.Send(new Network.GamePackets.Message($"VIP level set to {selfLevel}", System.Drawing.Color.Green,
                Network.GamePackets.Message.Tip));
            return true;
        }

        private static bool HandleExpCommand(GameState client, string[] data, string mess)
        {
            if (ulong.TryParse(data[1], out var exp))
            {
                client.Entity.Experience = exp;
                return true;
            }
            return false;
        }

        private static bool HandleRacePointsCommand(GameState client, string[] data, string mess)
        {
            if (uint.TryParse(data[1], out var racePoints))
            {
                client.RacePoints += racePoints;
                return true;
            }
            return false;
        }

        private static bool HandleHonorPointsCommand(GameState client, string[] data, string mess)
        {
            if (uint.TryParse(data[1], out var honorPoints))
            {
                client.CurrentHonor += honorPoints;
                return true;
            }
            return false;
        }

        private static bool HandleStudyPointsCommand(GameState client, string[] data, string mess)
        {
            if (ushort.TryParse(data[1], out var studyPoints))
            {
                if (studyPoints > 9999)
                {
                    client.Send(new Network.GamePackets.Message("Study points cannot be greater than 9,999.", System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    return true;
                }
                client.Entity.SubClasses.StudyPoints = studyPoints;
                client.Entity.SubClasses.Send(client);
                return true;
            }
            return false;
        }

        private static bool HandleLevelCommand(GameState client, string[] data, string mess)
        {
            if (byte.TryParse(data[1], out var level))
            {
                client.Entity.Level = level;
                Database.DataHolder.GetStats(client.Entity.Class, client.Entity.Level, client);
                client.CalculateStatBonus();
                client.CalculateHPBonus();
                client.GemAlgorithm();
                return true;
            }
            return false;
        }
    }
}

