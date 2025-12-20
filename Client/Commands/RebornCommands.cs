namespace MTA.Client.Commands
{
    public static class RebornCommands
    {
        public static bool HandleCommand(GameState client, string[] data, string mess)
        {
            switch (data[0])
            {
                case "reborn":
                    return HandleRebornCommand(client, data, mess);

                default:
                    return false;
            }
        }

        private static bool HandleRebornCommand(GameState client, string[] data, string mess)
        {
            GameState targetClient = client;
            string classesString = "";

            // Find the part that contains hyphens (the classes string)
            int classesIndex = -1;
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i].Contains("-"))
                {
                    classesIndex = i;
                    break;
                }
            }

            if (classesIndex == -1)
            {
                // No hyphen found - invalid format
                client.Send(new Network.GamePackets.Message("Usage: @reborn <first>-<second>-<current> or @reborn <player_name> <first>-<second>-<current>", System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                client.Send(new Network.GamePackets.Message("Example: @reborn monk-ninja-water", System.Drawing.Color.Yellow,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            // Reconstruct the classes string from the hyphen-containing part onwards
            classesString = string.Join(" ", data, classesIndex, data.Length - classesIndex);

            // If classesIndex > 1, we have a player name
            if (classesIndex > 1)
            {
                // Format: @reborn <player_name> <first>-<second>-<current>
                // Reconstruct player name from parts before the classes
                string playerNameSearch = string.Join(" ", data, 1, classesIndex - 1);

                if (string.IsNullOrWhiteSpace(playerNameSearch))
                {
                    client.Send(new Network.GamePackets.Message("Usage: @reborn <first>-<second>-<current> or @reborn <player_name> <first>-<second>-<current>", System.Drawing.Color.Red,
                        Network.GamePackets.Message.Tip));
                    return true;
                }

                // Search for player using utility function from EntityCommands
                if (!EntityCommands.FindPlayerByName(playerNameSearch, out GameState foundPlayer, out int matchCount))
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
            }
            // else: Self command, targetClient is already set to client

            // Parse the classes string (format: first-second-current)
            string[] classes = classesString.Split('-');
            if (classes.Length != 3)
            {
                client.Send(new Network.GamePackets.Message("Usage: @reborn <first>-<second>-<current>", System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                client.Send(new Network.GamePackets.Message("Example: @reborn monk-ninja-water", System.Drawing.Color.Yellow,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            byte firstClass = GetClassIdFromName(classes[0]);
            byte secondClass = GetClassIdFromName(classes[1]);
            byte currentClass = GetClassIdFromName(classes[2]);

            if (firstClass == 0 || secondClass == 0 || currentClass == 0)
            {
                client.Send(new Network.GamePackets.Message("Invalid class name(s). Valid classes: trojan, warrior, archer, ninja, monk, pirate, leelong, water, fire, windwalker", System.Drawing.Color.Red,
                    Network.GamePackets.Message.Tip));
                return true;
            }

            // Set the rebirth information
            targetClient.Entity.FirstRebornClass = firstClass;
            targetClient.Entity.SecondRebornClass = secondClass;
            targetClient.Entity.Class = currentClass;
            targetClient.Entity.Reborn = 2; // Second rebirth

            // Save to database
            Database.EntityTable.SaveEntity(targetClient);

            // Reload screen to show changes
            targetClient.Screen.FullWipe();
            targetClient.Screen.Reload();

            string firstClassName = GetClassNameFromId(firstClass);
            string secondClassName = GetClassNameFromId(secondClass);
            string currentClassName = GetClassNameFromId(currentClass);

            if (targetClient == client)
            {
                client.Send(new Network.GamePackets.Message($"Rebirth set: First={firstClassName}, Second={secondClassName}, Current={currentClassName}", System.Drawing.Color.Green,
                    Network.GamePackets.Message.Tip));
            }
            else
            {
                targetClient.Send(new Network.GamePackets.Message($"Rebirth set by {client.Entity.Name}: First={firstClassName}, Second={secondClassName}, Current={currentClassName}",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
                client.Send(new Network.GamePackets.Message($"Rebirth set for [{targetClient.Entity.Name}]: First={firstClassName}, Second={secondClassName}, Current={currentClassName}",
                    System.Drawing.Color.Green, Network.GamePackets.Message.Tip));
            }

            return true;
        }


        private static byte GetClassIdFromName(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
                return 0;

            string classLower = className.Trim().ToLower();
            switch (classLower)
            {
                case "trojan":
                    return 15;
                case "warrior":
                    return 25;
                case "archer":
                    return 45;
                case "ninja":
                    return 55;
                case "monk":
                    return 65;
                case "pirate":
                    return 75;
                case "leelong":
                    return 85;
                case "water":
                    return 135;
                case "fire":
                    return 145;
                case "windwalker":
                    return 165;
                default:
                    return 0;
            }
        }

        private static string GetClassNameFromId(byte classId)
        {
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

    }
}

