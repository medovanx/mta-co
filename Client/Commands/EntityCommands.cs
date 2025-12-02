using System;

namespace MTA.Client.Commands
{
    public static class EntityCommands
    {
        public static bool HandleCommand(GameState client, string[] data, string mess)
        {
            return (global::System.String)data[0] switch
            {
                "gold" => handleGoldCommand(client, data, mess),
                "cps" => handleCpsCommand(client, data, mess),
                "bcps" => handleBcpsCommand(client, data, mess),
                "vip" => HandleVipCommand(client, data, mess),
                "exp" => HandleExpCommand(client, data, mess),
                "racepoints" => HandleRacePointsCommand(client, data, mess),
                "honorpoints" => HandleHonorPointsCommand(client, data, mess),
                "studypoints" => HandleStudyPointsCommand(client, data, mess),
                "level" => HandleLevelCommand(client, data, mess),
                "reallot" => HandleReallotCommand(client, data, mess),
                "strength" => HandleAttributeCommand(client, data, AttributeType.Strength),
                "speed" => HandleAttributeCommand(client, data, AttributeType.Agility),
                "vitality" => HandleAttributeCommand(client, data, AttributeType.Vitality),
                "spirit" => HandleAttributeCommand(client, data, AttributeType.Spirit),
                "heal" => HandleHealCommand(client, data, mess),
                "spell" => HandleSpellCommand(client, data, mess),
                "die" => HandleDieCommand(client, data, mess),
                _ => false,
            };
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
                if (level > 140)
                {
                    client.Send(new Network.GamePackets.Message($"Level cannot be greater than 140.", System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    return true;
                }
                client.Entity.Level = level;
                Database.DataHolder.GetStats(client.Entity.Class, client.Entity.Level, client);
                client.CalculateStatBonus();
                client.CalculateHPBonus();
                client.GemAlgorithm();
                return true;
            }
            return false;
        }

        private static bool HandleReallotCommand(GameState client, string[] data, string mess)
        {
            if (client.Entity.Reborn == 0)
            {
                client.Send(new Network.GamePackets.Message("You must be reborn to use this command.",
                    System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                return true;
            }

            // Get all currently distributed points
            ushort distributedPoints = (ushort)(client.Entity.Agility + client.Entity.Strength + client.Entity.Vitality + client.Entity.Spirit);

            // Reset all attributes to base values
            client.Entity.Agility = 0;
            client.Entity.Strength = 0;
            client.Entity.Vitality = 1;
            client.Entity.Spirit = 0;

            // Add all distributed points to available attributes
            client.Entity.Atributes += (ushort)(distributedPoints - 1); // Subtract 1 for the base Vitality

            client.CalculateStatBonus();
            client.CalculateHPBonus();
            Database.EntityTable.SaveEntity(client);
            client.Send(new Network.GamePackets.Message("Attributes have been reset.", System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
            return true;
        }

        private enum AttributeType
        {
            Strength,
            Agility,
            Vitality,
            Spirit
        }

        private static bool HandleAttributeCommand(GameState client, string[] data, AttributeType attributeType)
        {
            if (!ushort.TryParse(data[1], out ushort amount))
            {
                client.Send(new Network.GamePackets.Message($"Invalid amount. Usage: @{attributeType.ToString().ToLower()} <amount>",
                    System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                return true;
            }

            if (client.Entity.Atributes < amount)
            {
                client.Send(new Network.GamePackets.Message($"Not enough attribute points. Available: {client.Entity.Atributes}",
                    System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                return true;
            }

            switch (attributeType)
            {
                case AttributeType.Strength:
                    client.Entity.Strength += amount;
                    break;
                case AttributeType.Agility:
                    client.Entity.Agility += amount;
                    break;
                case AttributeType.Vitality:
                    client.Entity.Vitality += amount;
                    break;
                case AttributeType.Spirit:
                    client.Entity.Spirit += amount;
                    break;
            }

            client.Entity.Atributes -= amount;
            client.CalculateStatBonus();
            client.CalculateHPBonus();
            Database.EntityTable.SaveEntity(client);
            client.Send(new Network.GamePackets.Message($"Added {amount} points to {attributeType}. Remaining: {client.Entity.Atributes}",
                System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
            return true;
        }

        private static bool HandleHealCommand(GameState client, string[] data, string mess)
        {
            uint hpAmount = client.Entity.MaxHitpoints;

            if (data.Length >= 2)
            {
                if (!uint.TryParse(data[1], out hpAmount))
                {
                    client.Send(new Network.GamePackets.Message("Invalid HP amount. Must be a positive number.",
                        System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    return true;
                }

                if (hpAmount > client.Entity.MaxHitpoints)
                    hpAmount = client.Entity.MaxHitpoints;
            }

            client.Entity.Hitpoints = hpAmount;
            client.Entity.Mana = client.Entity.MaxMana;

            var update = new Network.GamePackets.Update(true)
            {
                UID = client.Entity.UID,
                UpdateCount = 2
            };
            update.Append(Network.GamePackets.Update.Hitpoints, client.Entity.Hitpoints);
            update.Append(Network.GamePackets.Update.Mana, client.Entity.Mana);
            client.Send(update);

            string message = hpAmount == client.Entity.MaxHitpoints
                ? "You have been fully healed."
                : $"You have been healed to {hpAmount} HP.";
            client.Send(new Network.GamePackets.Message(message, System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
            return true;
        }

        private static bool HandleSpellCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                client.Send(new Network.GamePackets.Message("Usage: @spell <spell_id> [level]",
                    System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                return true;
            }

            if (!ushort.TryParse(data[1], out ushort spellId))
            {
                client.Send(new Network.GamePackets.Message("Invalid spell ID. Must be a number.",
                    System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                return true;
            }

            byte spellLevel = 0;
            if (data.Length >= 3)
            {
                if (!byte.TryParse(data[2], out spellLevel))
                {
                    client.Send(new Network.GamePackets.Message("Invalid level. Must be a number between 0-255.",
                        System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    return true;
                }
            }

            var spell = new Network.GamePackets.Spell(true)
            {
                ID = spellId,
                Level = spellLevel,
                Experience = 0
            };

            if (client.AddSpell(spell))
            {
                client.Send(new Network.GamePackets.Message($"Successfully learned spell {spellId} at level {spellLevel}.",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
            }
            else
            {
                client.Send(new Network.GamePackets.Message($"You already know spell {spellId}. Use a higher level to upgrade it.",
                    System.Drawing.Color.Yellow, Network.GamePackets.Message.Tip));
            }

            return true;
        }

        private static bool HandleDieCommand(GameState client, string[] data, string mess)
        {
            if (client.Entity.Dead)
            {
                client.Send(new Network.GamePackets.Message("You are already dead.",
                    System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                return true;
            }

            client.Entity.Hitpoints = 0;
            client.Entity.DeathStamp = Time32.Now;

            var update = new Network.GamePackets.Update(true)
            {
                UID = client.Entity.UID,
                UpdateCount = 1
            };
            update.Append(Network.GamePackets.Update.Hitpoints, 0);
            client.Send(update);

            client.Entity.AddFlag(Network.GamePackets.Update.Flags.Dead);
            client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Fly);
            client.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Ride);

            var str = new Network.GamePackets._String(true)
            {
                UID = client.Entity.UID,
                TextsCount = 1,
                Type = Network.GamePackets._String.Effect
            };
            str.Texts.Add("endureXPdeath");
            client.Entity.SendScreen(str);

            client.Send(new Network.GamePackets.Message("You have died.", System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
            return true;
        }
    }
}

