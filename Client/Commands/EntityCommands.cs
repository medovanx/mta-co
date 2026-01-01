using System;

namespace MTA.Client.Commands {
    public static class EntityCommands {
        public static bool HandleCommand(GameState client, string[] data, string mess) {
            return data[0] switch {
                "gold" => HandleGoldCommand(client, data, mess),
                "cps" => HandleCpsCommand(client, data, mess),
                "bcps" => HandleBcpsCommand(client, data, mess),
                "vip" => HandleVipCommand(client, data, mess),
                "exp" => HandleExpCommand(client, data),
                "racepoints" => HandleRacePointsCommand(client, data),
                "honorpoints" => HandleHonorPointsCommand(client, data),
                "studypoints" => HandleStudyPointsCommand(client, data),
                "level" => HandleLevelCommand(client, data),
                "reallot" => HandleReallotCommand(client),
                "strength" => HandleAttributeCommand(client, data, AttributeType.Strength),
                "speed" => HandleAttributeCommand(client, data, AttributeType.Agility),
                "vitality" => HandleAttributeCommand(client, data, AttributeType.Vitality),
                "spirit" => HandleAttributeCommand(client, data, AttributeType.Spirit),
                "heal" => HandleHealCommand(client, data),
                "spell" => HandleSpellCommand(client, data),
                "die" => HandleDieCommand(client),
                "xp" => HandleXpCommand(client, data),
                "rev" => HandleRevCommand(client, data),
                "class" => HandleClassCommand(client, data, mess),
                "grank" => HandleGrankCommand(client, data),
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
        public static bool FindPlayerByName(string searchName, out GameState? foundPlayer, out int matchCount) {
            foundPlayer = null;
            matchCount = 0;

            if (string.IsNullOrWhiteSpace(searchName))
                return false;

            // Remove spaces from search term for easier matching
            var searchTerm = searchName.Replace(" ", "");

            foreach (var player in Kernel.GamePool.Values) {
                {
                    // Remove spaces from player name for comparison
                    var playerNameClean = player.Entity.Name.Replace(" ", "");

                    if (playerNameClean.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    foundPlayer = player;
                    matchCount++;
                    // If exact match, use it immediately
                    if (!playerNameClean.Equals(searchTerm, StringComparison.OrdinalIgnoreCase)) continue;
                    matchCount = 1;
                    break;
                }
            }

            return matchCount == 1;
        }

        private static bool HandleGoldCommand(GameState client, string[] data, string mess) {
            const ulong maxGold = 9999999999UL; // 9,999,999,999

            // Check if second parameter is a player name
            if (data.Length >= 2 && !ulong.TryParse(data[1], out _)) {
                // Extract player name - everything except the last parameter (which should be the amount)
                ulong amount = 0;

                // Try to find the amount parameter (should be the last numeric parameter)
                for (var i = data.Length - 1; i >= 1; i--) {
                    if (ulong.TryParse(data[i], out amount)) {
                        break;
                    }
                }

                if (amount == 0) {
                    client.Send(new Network.GamePackets.Message("Usage: @gold <player_name> <amount> or @gold <amount>",
                        System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Get player name - everything except the last parameter (amount)
                var playerNameSearch = mess.Substring(data[0].Length + 1).Trim();
                var lastSpaceIndex = playerNameSearch.LastIndexOf(' ');
                if (lastSpaceIndex >= 0) {
                    playerNameSearch = playerNameSearch.Substring(0, lastSpaceIndex).Trim();
                }

                if (string.IsNullOrWhiteSpace(playerNameSearch)) {
                    client.Send(new Network.GamePackets.Message("Usage: @gold <player_name> <amount> or @gold <amount>",
                        System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Search for player using utility function
                if (!FindPlayerByName(playerNameSearch, out var foundPlayer, out var matchCount)) {
                    if (matchCount == 0) {
                        client.Send(new Network.GamePackets.Message(
                            $"Player matching [{playerNameSearch}] not found or offline!",
                            System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    }
                    else {
                        client.Send(new Network.GamePackets.Message(
                            $"Multiple players found matching [{playerNameSearch}]. Please be more specific.",
                            System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    }

                    return true;
                }

                if (amount > maxGold) {
                    client.Send(new Network.GamePackets.Message($"Maximum gold amount is {maxGold:N0}",
                        System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                if (foundPlayer == null) return true;
                foundPlayer.Entity.Money = amount;
                foundPlayer.Send(new Network.GamePackets.Message(
                    $"Money set to {amount:N0} by {client.Entity.Name}",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                client.Send(new Network.GamePackets.Message(
                    $"Money set to {amount:N0} for [{foundPlayer.Entity.Name}]",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));

                return true;
            }

            // Self gold command
            if (!ulong.TryParse(data[1], out var selfAmount)) {
                client.Send(new Network.GamePackets.Message("Usage: @gold <amount> or @gold <player_name> <amount>",
                    System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            if (selfAmount > maxGold) {
                client.Send(new Network.GamePackets.Message($"Maximum gold amount is {maxGold:N0}",
                    System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            client.Entity.Money = selfAmount;
            client.Send(new Network.GamePackets.Message($"Money set to {selfAmount:N0}", System.Drawing.Color.Green,
                Network.GamePackets.Message.Tip));
            return true;
        }

        private static bool HandleCpsCommand(GameState client, string[] data, string mess) {
            const uint maxCps = 999999999U; // 999,999,999

            // Check if second parameter is a player name
            if (data.Length >= 2 && !ulong.TryParse(data[1], out _)) {
                // Extract player name - everything except the last parameter (which should be the amount)
                ulong amountLong = 0;

                // Try to find the amount parameter (should be the last numeric parameter)
                for (var i = data.Length - 1; i >= 1; i--) {
                    if (ulong.TryParse(data[i], out amountLong)) {
                        break;
                    }
                }

                if (amountLong == 0) {
                    client.Send(new Network.GamePackets.Message("Usage: @cps <player_name> <amount> or @cps <amount>",
                        System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Get player name - everything except the last parameter (amount)
                var playerNameSearch = mess.Substring(data[0].Length + 1).Trim();
                var lastSpaceIndex = playerNameSearch.LastIndexOf(' ');
                if (lastSpaceIndex >= 0) {
                    playerNameSearch = playerNameSearch.Substring(0, lastSpaceIndex).Trim();
                }

                if (string.IsNullOrWhiteSpace(playerNameSearch)) {
                    client.Send(new Network.GamePackets.Message("Usage: @cps <player_name> <amount> or @cps <amount>",
                        System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Search for player using utility function
                if (!FindPlayerByName(playerNameSearch, out var foundPlayer, out var matchCount)) {
                    if (matchCount == 0) {
                        client.Send(new Network.GamePackets.Message(
                            $"Player matching [{playerNameSearch}] not found or offline!",
                            System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    }
                    else {
                        client.Send(new Network.GamePackets.Message(
                            $"Multiple players found matching [{playerNameSearch}]. Please be more specific.",
                            System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    }

                    return true;
                }

                if (amountLong > maxCps) {
                    client.Send(new Network.GamePackets.Message($"Maximum CPs amount is {maxCps:N0}",
                        System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                foundPlayer?.Entity.ConquerPoints = (uint)amountLong;
                if (foundPlayer == null) return true;
                foundPlayer.Send(new Network.GamePackets.Message(
                    $"CPs set to {amountLong:N0} by {client.Entity.Name}",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                client.Send(new Network.GamePackets.Message(
                    $"CPs set to {amountLong:N0} for [{foundPlayer.Entity.Name}]",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));

                return true;
            }

            // Self CPs command
            if (!ulong.TryParse(data[1], out var selfAmountLong)) {
                client.Send(new Network.GamePackets.Message("Usage: @cps <amount> or @cps <player_name> <amount>",
                    System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            if (selfAmountLong > maxCps) {
                client.Send(new Network.GamePackets.Message($"Maximum CPs amount is {maxCps:N0}",
                    System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            client.Entity.ConquerPoints = (uint)selfAmountLong;
            client.Send(new Network.GamePackets.Message($"CPs set to {selfAmountLong:N0}",
                System.Drawing.Color.Green,
                Network.GamePackets.Message.Tip));
            return true;
        }

        private static bool HandleBcpsCommand(GameState client, string[] data, string mess) {
            const uint maxBcps = 999999999U; // 999,999,999

            // Check if second parameter is a player name
            if (data.Length >= 2 && !ulong.TryParse(data[1], out _)) {
                // Extract player name - everything except the last parameter (which should be the amount)
                ulong amountLong = 0;

                // Try to find the amount parameter (should be the last numeric parameter)
                for (var i = data.Length - 1; i >= 1; i--) {
                    if (ulong.TryParse(data[i], out amountLong)) {
                        break;
                    }
                }

                if (amountLong == 0) {
                    client.Send(new Network.GamePackets.Message("Usage: @bcps <player_name> <amount> or @bcps <amount>",
                        System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Get player name - everything except the last parameter (amount)
                var playerNameSearch = mess.Substring(data[0].Length + 1).Trim();
                var lastSpaceIndex = playerNameSearch.LastIndexOf(' ');
                if (lastSpaceIndex >= 0) {
                    playerNameSearch = playerNameSearch.Substring(0, lastSpaceIndex).Trim();
                }

                if (string.IsNullOrWhiteSpace(playerNameSearch)) {
                    client.Send(new Network.GamePackets.Message("Usage: @bcps <player_name> <amount> or @bcps <amount>",
                        System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Search for player using utility function
                if (!FindPlayerByName(playerNameSearch, out var foundPlayer, out var matchCount)) {
                    if (matchCount == 0) {
                        client.Send(new Network.GamePackets.Message(
                            $"Player matching [{playerNameSearch}] not found or offline!",
                            System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    }
                    else {
                        client.Send(new Network.GamePackets.Message(
                            $"Multiple players found matching [{playerNameSearch}]. Please be more specific.",
                            System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    }

                    return true;
                }

                if (amountLong > maxBcps) {
                    client.Send(new Network.GamePackets.Message($"Maximum Bound CPs amount is {maxBcps:N0}",
                        System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                foundPlayer?.Entity.BoundCps = (uint)amountLong;
                if (foundPlayer == null) return true;
                foundPlayer.Send(new Network.GamePackets.Message(
                    $"Bound CPs set to {amountLong:N0} by {client.Entity.Name}",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                client.Send(new Network.GamePackets.Message(
                    $"Bound CPs set to {amountLong:N0} for [{foundPlayer.Entity.Name}]",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));

                return true;
            }

            // Self BCPs command
            if (!ulong.TryParse(data[1], out var selfAmountLong)) {
                client.Send(new Network.GamePackets.Message("Usage: @bcps <amount> or @bcps <player_name> <amount>",
                    System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            if (selfAmountLong > maxBcps) {
                client.Send(new Network.GamePackets.Message($"Maximum Bound CPs amount is {maxBcps:N0}",
                    System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            client.Entity.BoundCps = (uint)selfAmountLong;
            client.Send(new Network.GamePackets.Message($"Bound CPs set to {selfAmountLong:N0}",
                System.Drawing.Color.Green,
                Network.GamePackets.Message.Tip));
            return true;
        }

        private static bool HandleVipCommand(GameState client, string[] data, string mess) {
            const byte maxVipLevel = 6;

            // Check if second parameter is a player name
            if (data.Length >= 2 && !byte.TryParse(data[1], out _)) {
                // Extract player name - everything except the last parameter (which should be the level)
                byte level = 0;

                // Try to find the level parameter (should be the last numeric parameter)
                for (var i = data.Length - 1; i >= 1; i--) {
                    if (byte.TryParse(data[i], out level)) {
                        break;
                    }
                }

                if (level == 0 && data.Length < 3) {
                    client.Send(new Network.GamePackets.Message("Usage: @vip <player_name> <level> (0-6)",
                        System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Get player name - everything except the last parameter (level)
                var playerNameSearch = mess.Substring(data[0].Length + 1).Trim();
                if (data.Length >= 3) {
                    // Remove the level from the end
                    var lastSpaceIndex = playerNameSearch.LastIndexOf(' ');
                    if (lastSpaceIndex >= 0) {
                        playerNameSearch = playerNameSearch.Substring(0, lastSpaceIndex).Trim();
                    }
                }

                if (string.IsNullOrWhiteSpace(playerNameSearch)) {
                    client.Send(new Network.GamePackets.Message("Usage: @vip <player_name> <level> (0-6)",
                        System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Search for player using utility function
                if (!FindPlayerByName(playerNameSearch, out var foundPlayer, out var matchCount)) {
                    if (matchCount == 0) {
                        client.Send(new Network.GamePackets.Message(
                            $"Player matching [{playerNameSearch}] not found or offline!",
                            System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    }
                    else {
                        client.Send(new Network.GamePackets.Message(
                            $"Multiple players found matching [{playerNameSearch}]. Please be more specific.",
                            System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    }

                    return true;
                }

                if (level > maxVipLevel) {
                    client.Send(new Network.GamePackets.Message($"Maximum VIP level is {maxVipLevel}",
                        System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                if (foundPlayer == null) return true;
                foundPlayer.Entity.VIPLevel = level;
                var vip = new Network.GamePackets.VipStatus();
                foundPlayer.Send(vip.ToArray());
                foundPlayer.Screen.FullWipe();
                foundPlayer.Screen.Reload();
                foundPlayer.Send(new Network.GamePackets.Message(
                    $"VIP level set to {level} by {client.Entity.Name}",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                client.Send(new Network.GamePackets.Message(
                    $"VIP level set to {level} for [{foundPlayer.Entity.Name}]",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));

                return true;
            }

            // Self VIP command
            if (!byte.TryParse(data[1], out var selfLevel)) {
                client.Send(new Network.GamePackets.Message("Usage: @vip <level> (0-6) or @vip <player_name> <level>",
                    System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            if (selfLevel > maxVipLevel) {
                client.Send(new Network.GamePackets.Message($"Maximum VIP level is {maxVipLevel}",
                    System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            client.Entity.VIPLevel = selfLevel;
            var vipSelf = new Network.GamePackets.VipStatus();
            client.Send(vipSelf.ToArray());
            client.Screen.FullWipe();
            client.Screen.Reload();
            client.Send(new Network.GamePackets.Message($"VIP level set to {selfLevel}", System.Drawing.Color.Green,
                Network.GamePackets.Message.Tip));
            return true;
        }

        private static bool HandleExpCommand(GameState client, string[] data) {
            if (ulong.TryParse(data[1], out var exp)) {
                client.Entity.Experience = exp;
                return true;
            }

            return false;
        }

        private static bool HandleRacePointsCommand(GameState client, string[] data) {
            if (uint.TryParse(data[1], out var racePoints)) {
                client.RacePoints += racePoints;
                return true;
            }

            return false;
        }

        private static bool HandleHonorPointsCommand(GameState client, string[] data) {
            if (uint.TryParse(data[1], out var honorPoints)) {
                client.CurrentHonor += honorPoints;
                return true;
            }

            return false;
        }

        private static bool HandleStudyPointsCommand(GameState client, string[] data) {
            if (ushort.TryParse(data[1], out var studyPoints)) {
                client.Entity.SubClasses.StudyPoints = studyPoints;
                client.Entity.SubClasses.Send(client);
                return true;
            }

            return false;
        }

        private static bool HandleLevelCommand(GameState client, string[] data) {
            if (byte.TryParse(data[1], out var level)) {
                if (level > 140) {
                    client.Send(new Network.GamePackets.Message($"Level cannot be greater than 140.",
                        System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
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

        private static bool HandleReallotCommand(GameState client) {
            if (client.Entity.Reborn == 0) {
                client.Send(new Network.GamePackets.Message("You must be reborn to use this command.",
                    System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                return true;
            }

            // Get all currently distributed points
            var distributedPoints = (ushort)(client.Entity.Agility + client.Entity.Strength +
                                             client.Entity.Vitality + client.Entity.Spirit);

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
            client.Send(new Network.GamePackets.Message("Attributes have been reset.", System.Drawing.Color.Green,
                Network.GamePackets.Message.Tip));
            return true;
        }

        private enum AttributeType {
            Strength,
            Agility,
            Vitality,
            Spirit
        }

        private static bool HandleAttributeCommand(GameState client, string[] data, AttributeType attributeType) {
            if (!ushort.TryParse(data[1], out var amount)) {
                client.Send(new Network.GamePackets.Message(
                    $"Invalid amount. Usage: @{attributeType.ToString().ToLower()} <amount>",
                    System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                return true;
            }

            if (client.Entity.Atributes < amount) {
                client.Send(new Network.GamePackets.Message(
                    $"Not enough attribute points. Available: {client.Entity.Atributes}",
                    System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                return true;
            }

            switch (attributeType) {
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
            client.Send(new Network.GamePackets.Message(
                $"Added {amount} points to {attributeType}. Remaining: {client.Entity.Atributes}",
                System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
            return true;
        }

        private static bool HandleHealCommand(GameState client, string[] data) {
            var hpAmount = client.Entity.MaxHitpoints;

            if (data.Length >= 2) {
                if (!uint.TryParse(data[1], out hpAmount)) {
                    client.Send(new Network.GamePackets.Message("Invalid HP amount. Must be a positive number.",
                        System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    return true;
                }

                if (hpAmount > client.Entity.MaxHitpoints)
                    hpAmount = client.Entity.MaxHitpoints;
            }

            client.Entity.Hitpoints = hpAmount;
            client.Entity.Mana = client.Entity.MaxMana;

            var update = new Network.GamePackets.Update(true) {
                UID = client.Entity.UID,
                UpdateCount = 2
            };
            update.Append(Network.GamePackets.Update.Hitpoints, client.Entity.Hitpoints);
            update.Append(Network.GamePackets.Update.Mana, client.Entity.Mana);
            client.Send(update);

            var message = hpAmount == client.Entity.MaxHitpoints
                ? "You have been fully healed."
                : $"You have been healed to {hpAmount} HP.";
            client.Send(new Network.GamePackets.Message(message, System.Drawing.Color.Green,
                Network.GamePackets.Message.Tip));
            return true;
        }

        private static bool HandleSpellCommand(GameState client, string[] data) {
            if (data.Length < 2) {
                client.Send(new Network.GamePackets.Message("Usage: @spell <spell_id> [level]",
                    System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                return true;
            }

            if (!ushort.TryParse(data[1], out var spellId)) {
                client.Send(new Network.GamePackets.Message("Invalid spell ID. Must be a number.",
                    System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                return true;
            }

            byte spellLevel = 0;
            if (data.Length >= 3) {
                if (!byte.TryParse(data[2], out spellLevel)) {
                    client.Send(new Network.GamePackets.Message("Invalid level. Must be a number between 0-255.",
                        System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    return true;
                }
            }

            var spell = new Network.GamePackets.Spell(true) {
                ID = spellId,
                Level = spellLevel,
                Experience = 0
            };

            if (client.AddSpell(spell)) {
                client.Send(new Network.GamePackets.Message(
                    $"Successfully learned spell {spellId} at level {spellLevel}.",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
            }
            else {
                client.Send(new Network.GamePackets.Message(
                    $"You already know spell {spellId}. Use a higher level to upgrade it.",
                    System.Drawing.Color.Yellow, Network.GamePackets.Message.Tip));
            }

            return true;
        }

        private static bool HandleDieCommand(GameState client) {
            client.Entity.Die(null);
            return true;
        }

        private static bool HandleXpCommand(GameState client, string[] data) {
            if (!ulong.TryParse(data[1], out _)) return false;
            client.Entity.AddFlag(Network.GamePackets.Update.Flags.XPList);
            client.XPListStamp = Time32.Now;
            return true;
        }

        private static bool HandleRevCommand(GameState client, string[] data) {
            var targetClient = client;

            // If a parameter is provided, try to find the target player
            if (data.Length > 1) {
                var targetName = data[1];
                GameState? foundClient = null;

                // If not found by UID, search by name using the utility function
                if (targetClient == client && foundClient == null) {
                    if (FindPlayerByName(targetName, out var foundPlayer, out var matchCount)) {
                        targetClient = foundPlayer;
                    }
                    else {
                        if (matchCount == 0) {
                            client.Send(new Network.GamePackets.Message(
                                $"Player matching [{targetName}] not found or offline!",
                                System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                        }
                        else {
                            client.Send(new Network.GamePackets.Message(
                                $"Multiple players found matching [{targetName}]. Please be more specific.",
                                System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                        }

                        return true;
                    }
                }

                // If target not found, send error message
                if (targetClient == client && foundClient == null) {
                    client.Send(new Network.GamePackets.Message("Player not found: " + targetName,
                        System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    return true;
                }

                // Send confirmation to the GM
                if (targetClient != client) {
                    if (targetClient != null)
                        client.Send(new Network.GamePackets.Message("Revived player: " + targetClient.Entity.Name,
                            System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                }
            }

            // Apply the revive logic to target
            if (targetClient == null) return true;
            targetClient.Entity.Action = Game.Enums.ConquerAction.None;
            targetClient.ReviveStamp = Time32.Now;
            targetClient.Attackable = false;

            targetClient.Entity.TransformationID = 0;
            targetClient.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Dead);
            targetClient.Entity.RemoveFlag(Network.GamePackets.Update.Flags.Ghost);
            targetClient.Entity.Hitpoints = targetClient.Entity.MaxHitpoints;
            // Set Attackable to true and notify other players about the state change
            targetClient.Attackable = true;
            targetClient.Entity.Update(targetClient.Entity.StatusFlag, targetClient.Entity.StatusFlag2,
                targetClient.Entity.StatusFlag3, 0, 0, 0, 0, true);

            // Send notification to target if reviving someone else
            if (targetClient != client) {
                targetClient.Send(new Network.GamePackets.Message(client.Entity.Name + " has revived you!",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
            }

            return true;
        }

        private static bool HandleClassCommand(GameState client, string[] data, string mess) {
            // Check if second parameter is a player name
            if (data.Length >= 2 && !byte.TryParse(data[1], out _)) {
                // Extract player name - everything except the last parameter (which should be the class ID)
                byte classId = 0;

                // Try to find the class ID parameter (should be the last numeric parameter)
                for (var i = data.Length - 1; i >= 1; i--) {
                    if (byte.TryParse(data[i], out classId)) {
                        break;
                    }
                }

                if (classId == 0) {
                    client.Send(new Network.GamePackets.Message(
                        "Usage: @class <player_name> <class_id> or @class <class_id>",
                        System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    client.Send(new Network.GamePackets.Message(
                        "Valid class IDs: 15 (Trojan), 25 (Warrior), 45 (Archer), 55 (Ninja), 65 (Monk), 75 (Pirate), 85 (Leelong), 135 (Water), 145 (Fire), 165 (Windwalker)",
                        System.Drawing.Color.Yellow,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Get player name - everything except the last parameter (class ID)
                var playerNameSearch = mess.Substring(data[0].Length + 1).Trim();
                var lastSpaceIndex = playerNameSearch.LastIndexOf(' ');
                if (lastSpaceIndex >= 0) {
                    playerNameSearch = playerNameSearch.Substring(0, lastSpaceIndex).Trim();
                }

                if (string.IsNullOrWhiteSpace(playerNameSearch)) {
                    client.Send(new Network.GamePackets.Message(
                        "Usage: @class <player_name> <class_id> or @class <class_id>",
                        System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Search for player using utility function
                if (!FindPlayerByName(playerNameSearch, out var foundPlayer, out var matchCount)) {
                    if (matchCount == 0) {
                        client.Send(new Network.GamePackets.Message(
                            $"Player matching [{playerNameSearch}] not found or offline!",
                            System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    }
                    else {
                        client.Send(new Network.GamePackets.Message(
                            $"Multiple players found matching [{playerNameSearch}]. Please be more specific.",
                            System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                    }

                    return true;
                }

                if (!IsValidClassId(classId)) {
                    client.Send(new Network.GamePackets.Message($"Invalid class ID: {classId}",
                        System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    client.Send(new Network.GamePackets.Message(
                        "Valid class IDs: 15 (Trojan), 25 (Warrior), 45 (Archer), 55 (Ninja), 65 (Monk), 75 (Pirate), 85 (Leelong), 135 (Water), 145 (Fire), 165 (Windwalker)",
                        System.Drawing.Color.Yellow,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                if (foundPlayer == null) return true;
                foundPlayer.Entity.Class = classId;

                // Recalculate stats like in HandleLevelCommand
                Database.DataHolder.GetStats(foundPlayer.Entity.Class, foundPlayer.Entity.Level, foundPlayer);
                foundPlayer.CalculateStatBonus();
                foundPlayer.CalculateHPBonus();
                foundPlayer.GemAlgorithm();

                // Reload screen to show changes
                foundPlayer.Screen.FullWipe();
                foundPlayer.Screen.Reload();

                var className = GetClassNameFromId(classId);
                foundPlayer.Send(new Network.GamePackets.Message(
                    $"Class set to {className} (ID: {classId}) by {client.Entity.Name}",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                client.Send(new Network.GamePackets.Message(
                    $"Class set to {className} (ID: {classId}) for [{foundPlayer.Entity.Name}]",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));

                return true;
            }

            // Self class command
            if (!byte.TryParse(data[1], out var selfClassId)) {
                client.Send(new Network.GamePackets.Message(
                    "Usage: @class <class_id> or @class <player_name> <class_id>",
                    System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                client.Send(new Network.GamePackets.Message(
                    "Valid class IDs: 15 (Trojan), 25 (Warrior), 45 (Archer), 55 (Ninja), 65 (Monk), 75 (Pirate), 85 (Leelong), 135 (Water), 145 (Fire), 165 (Windwalker)",
                    System.Drawing.Color.Yellow,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            if (!IsValidClassId(selfClassId)) {
                client.Send(new Network.GamePackets.Message($"Invalid class ID: {selfClassId}",
                    System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                client.Send(new Network.GamePackets.Message(
                    "Valid class IDs: 15 (Trojan), 25 (Warrior), 45 (Archer), 55 (Ninja), 65 (Monk), 75 (Pirate), 85 (Leelong), 135 (Water), 145 (Fire), 165 (Windwalker)",
                    System.Drawing.Color.Yellow,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            client.Entity.Class = selfClassId;

            // Recalculate stats like in HandleLevelCommand
            Database.DataHolder.GetStats(client.Entity.Class, client.Entity.Level, client);
            client.CalculateStatBonus();
            client.CalculateHPBonus();
            client.GemAlgorithm();

            // Reload screen to show changes
            client.Screen.FullWipe();
            client.Screen.Reload();

            var selfClassName = GetClassNameFromId(selfClassId);
            client.Send(new Network.GamePackets.Message($"Class set to {selfClassName} (ID: {selfClassId})",
                System.Drawing.Color.Green,
                Network.GamePackets.Message.Tip));
            return true;
        }

        private static bool IsValidClassId(byte classId) {
            // Valid class ID ranges based on RebornCommands.cs
            return (classId >= 10 && classId <= 15) || // Trojan
                   (classId >= 20 && classId <= 25) || // Warrior
                   (classId >= 40 && classId <= 45) || // Archer
                   (classId >= 50 && classId <= 55) || // Ninja
                   (classId >= 60 && classId <= 65) || // Monk
                   (classId >= 70 && classId <= 75) || // Pirate
                   (classId >= 80 && classId <= 85) || // Leelong
                   (classId >= 130 && classId <= 135) || // Water
                   (classId >= 140 && classId <= 145) || // Fire
                   (classId >= 160 && classId <= 165); // Windwalker
        }

        private static string GetClassNameFromId(byte classId) {
            if (classId is >= 10 and <= 15) return "Trojan";
            if (classId is >= 20 and <= 25) return "Warrior";
            if (classId is >= 40 and <= 45) return "Archer";
            if (classId is >= 50 and <= 55) return "Ninja";
            if (classId is >= 60 and <= 65) return "Monk";
            if (classId is >= 70 and <= 75) return "Pirate";
            if (classId is >= 80 and <= 85) return "Leelong";
            if (classId is >= 130 and <= 135) return "Water";
            if (classId is >= 140 and <= 145) return "Fire";
            if (classId is >= 160 and <= 165) return "Windwalker";
            return "Unknown";
        }

        private static bool HandleGrankCommand(GameState client, string[] data) {
            if (data.Length < 2) {
                client.Send(new Network.GamePackets.Message("Usage: @grank <rank_id>",
                    System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                return true;
            }

            if (!ushort.TryParse(data[1], out var rankId)) {
                client.Send(new Network.GamePackets.Message("Invalid rank ID. Must be a number.",
                    System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                return true;
            }

            // Check if player is in a guild
            if (client.Guild == null || client.AsMember == null) {
                client.Send(new Network.GamePackets.Message("You must be in a guild to use this command.",
                    System.Drawing.Color.Red, Network.GamePackets.Message.Tip));
                return true;
            }

            // Update rank in memory only (not saved to database) - direct injection
            var oldRank = client.Entity.GuildRank;
            client.Entity.GuildRank = rankId;
            client.AsMember.Rank = (Game.Features.Guilds.Constants.MemberRank)rankId;

            // Update guild battle power based on new rank
            client.Entity.GuildBattlePower = client.Guild.GetSharedBattlePower(client.AsMember.Rank);

            // Send guild info packet to refresh display (like promotion does)
            client.Guild.SendGuild(client);
            client.Guild.SendMembers(client, 0);

            // Refresh screen to show changes
            client.Screen.FullWipe();
            client.Screen.Reload();

            client.Send(new Network.GamePackets.Message(
                $"Guild rank updated from {oldRank} to {rankId} [In-memory only, not saved]",
                System.Drawing.Color.Green, Network.GamePackets.Message.Tip));

            return true;
        }
    }
}