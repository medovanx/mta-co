using System;
using System.Linq;
using MTA.Network.GamePackets;
using MTA.Client;

namespace MTA.Client.Commands.TestCommands
{
    public static class GeneralTestCommands
    {
        public static bool HandleCommand(GameState client, string[] data, string mess)
        {
            if (data.Length == 0) return false;

            return data[0].ToLower() switch
            {
                "testsend" => HandleTestSendCommand(client, data, mess),
                _ => false,
            };
        }

        private static bool HandleTestSendCommand(GameState client, string[] data, string mess)
        {
            try
            {
                // Map of numbers to message types
                var messageTypes = new[]
                {
                    (1, Message.Talk, "Talk"),
                    (2, Message.Whisper, "Whisper"),
                    (3, Message.Team, "Team"),
                    (4, Message.Guild, "Guild"),
                    (5, Message.System, "System"),
                    (6, Message.Center, "Center"),
                    (7, Message.TopLeft, "TopLeft"),
                    (8, Message.Tip, "Tip"),
                    (9, Message.PopUP, "PopUP"),
                    (10, Message.Dialog, "Dialog"),
                    (11, Message.Service, "Service"),
                    (12, Message.World, "World"),
                    (13, Message.BroadcastMessage, "BroadcastMessage"),
                    (14, Message.Monster, "Monster"),
                    (15, Message.SlideFromRight, "SlideFromRight"),
                    (16, Message.HawkMessage, "HawkMessage"),
                    (17, Message.SlideFromRightRedVib, "SlideFromRightRedVib"),
                    (18, Message.WhiteVibrate, "WhiteVibrate"),
                };

                // Validate input
                if (data.Length < 2 || !int.TryParse(data[1], out int typeNumber))
                {
                    var typeList = string.Join(", ", messageTypes.Select(m => $"{m.Item1}={m.Item3}"));
                    client.Send(new Message($"Usage: @test testsend <type>\nAvailable types: {typeList}",
                        System.Drawing.Color.Yellow, Message.Tip));
                    return true;
                }

                // Find the message type
                var messageType = messageTypes.FirstOrDefault(m => m.Item1 == typeNumber);
                if (messageType.Item1 == 0)
                {
                    var typeList = string.Join(", ", messageTypes.Select(m => $"{m.Item1}={m.Item3}"));
                    client.Send(new Message($"Invalid type number. Available types: {typeList}",
                        System.Drawing.Color.Red, Message.Tip));
                    return true;
                }

                // Get optional message text (default if not provided)
                string messageText = data.Length > 2
                    ? string.Join(" ", data.Skip(2))
                    : $"Test message - Type {typeNumber} ({messageType.Item3})";

                // Send the test message
                var testMessage = new Message(messageText, System.Drawing.Color.Cyan, messageType.Item2);
                testMessage.Send(client);

                // Confirm to player
                client.Send(new Message($"Sent test message with type {typeNumber} ({messageType.Item3})",
                    System.Drawing.Color.Green, Message.Tip));
            }
            catch (Exception ex)
            {
                client.Send(new Message($"Error sending test message: {ex.Message}",
                    System.Drawing.Color.Red, Message.Tip));
            }

            return true;
        }
    }
}

