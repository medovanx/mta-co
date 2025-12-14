using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MTA.Network.GamePackets;
using MTA.Game;

namespace MTA.Client.Commands.TestCommands
{
    public static class MessageTestCommands
    {
        private static readonly Dictionary<string, uint> MessageTypeMap = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
        {
            { "CS", Message.CS },
            { "Talk", Message.Talk },
            { "Whisper", Message.Whisper },
            { "Team", Message.Team },
            { "Guild", Message.Guild },
            { "Clan", Message.Clan },
            { "System", Message.System },
            { "Friend", Message.Friend },
            { "Ally", Message.Ally },
            { "Center", Message.Center },
            { "TopLeft", Message.TopLeft },
            { "Service", Message.Service },
            { "Tip", Message.Tip },
            { "World", Message.World },
            { "Qualifier", Message.Qualifier },
            { "PopUP", Message.PopUP },
            { "Dialog", Message.Dialog },
            { "Website", Message.Website },
            { "FirstRightCorner", Message.FirstRightCorner },
            { "ContinueRightCorner", Message.ContinueRightCorner },
            { "SystemWhisper", Message.SystemWhisper },
            { "GuildAnnouncement", Message.GuildAnnouncement },
            { "Agate", Message.Agate },
            { "ArenaQualifier", Message.ArenaQualifier },
            { "BroadcastMessage", Message.BroadcastMessage },
            { "Monster", Message.Monster },
            { "SlideFromRight", Message.SlideFromRight },
            { "HawkMessage", Message.HawkMessage },
            { "SlideFromRightRedVib", Message.SlideFromRightRedVib },
            { "WhiteVibrate", Message.WhiteVibrate }
        };

        public static bool HandleCommand(GameState client, string[] data, string mess)
        {
            if (data.Length == 0) return false;

            return data[0].ToLower() switch
            {
                "message" => HandleMessageCommand(client, data, mess),
                _ => false,
            };
        }

        private static bool HandleMessageCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                client.Send(new Message("Usage: @message -t <Type> [message text]", System.Drawing.Color.Yellow, Message.Tip));
                client.Send(new Message("Available types: CS, Talk, Whisper, Team, Guild, Clan, System, Friend, Ally, Center, TopLeft, Service, Tip, World, Qualifier, PopUP, Dialog, Website, FirstRightCorner, ContinueRightCorner, SystemWhisper, GuildAnnouncement, Agate, ArenaQualifier, BroadcastMessage, Monster, SlideFromRight, HawkMessage, SlideFromRightRedVib, WhiteVibrate", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            var args = ParseArguments(data, mess);

            if (!args.ContainsKey("t") || string.IsNullOrEmpty(args["t"]))
            {
                client.Send(new Message("Type (-t) is required!", System.Drawing.Color.Red, Message.Tip));
                client.Send(new Message("Usage: @message -t <Type> [message text]", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            string typeStr = args["t"];
            if (!MessageTypeMap.TryGetValue(typeStr, out uint messageType))
            {
                client.Send(new Message($"Invalid message type: {typeStr}", System.Drawing.Color.Red, Message.Tip));
                client.Send(new Message("Available types: CS, Talk, Whisper, Team, Guild, Clan, System, Friend, Ally, Center, TopLeft, Service, Tip, World, Qualifier, PopUP, Dialog, Website, FirstRightCorner, ContinueRightCorner, SystemWhisper, GuildAnnouncement, Agate, ArenaQualifier, BroadcastMessage, Monster, SlideFromRight, HawkMessage, SlideFromRightRedVib, WhiteVibrate", System.Drawing.Color.Yellow, Message.Tip));
                return true;
            }

            // Extract message text - everything after the type parameter
            string messageText = "Test message for type: " + typeStr;

            // First check if message text was provided via -m parameter
            if (args.ContainsKey("m") && !string.IsNullOrEmpty(args["m"]))
            {
                messageText = args["m"];
            }
            else
            {
                // Try to extract remaining text from the original message
                // Find the position after "-t <type>" and get the rest
                Match typeMatch = Regex.Match(mess, @"-t\s+(\S+)", RegexOptions.IgnoreCase);
                if (typeMatch.Success)
                {
                    int typeEndPos = typeMatch.Index + typeMatch.Length;
                    if (typeEndPos < mess.Length)
                    {
                        string remaining = mess.Substring(typeEndPos).Trim();
                        // Check if there's any non-flag text remaining
                        if (!string.IsNullOrEmpty(remaining) && !remaining.StartsWith("-"))
                        {
                            messageText = remaining;
                        }
                    }
                }
            }

            // Default color based on message type
            System.Drawing.Color color = System.Drawing.Color.White;
            if (messageType == Message.System || messageType == Message.Tip)
                color = System.Drawing.Color.Yellow;
            else if (messageType == Message.Center || messageType == Message.TopLeft)
                color = System.Drawing.Color.Red;

            // Send the message
            var message = new Message(messageText, color, messageType);
            message.Send(client);

            client.Send(new Message($"Sent message with type '{typeStr}' (ID: {messageType})", System.Drawing.Color.Green, Message.Tip));
            return true;
        }

        private static Dictionary<string, string> ParseArguments(string[] data, string mess, int startIndex = 1)
        {
            var args = new Dictionary<string, string>();

            // Parse arguments normally
            for (int i = startIndex; i < data.Length; i++)
            {
                if (data[i].StartsWith("-"))
                {
                    string key = data[i].Substring(1).ToLower();

                    if (i + 1 < data.Length && !data[i + 1].StartsWith("-"))
                    {
                        string value = data[i + 1];
                        args[key] = value;
                        i++; // Skip the value in next iteration
                    }
                    else
                    {
                        args[key] = "";
                    }
                }
            }

            // For message text (-m), extract quoted value directly from original message to preserve casing and spaces
            Match messageMatch = Regex.Match(mess, @"-m\s+(['""])(.*?)\1", RegexOptions.IgnoreCase);
            if (messageMatch.Success)
            {
                args["m"] = messageMatch.Groups[2].Value;
            }

            return args;
        }
    }
}
