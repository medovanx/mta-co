using System;
using System.Linq;
using MTA.Network.GamePackets;
using MTA.Game.Events;
using System.Drawing;

namespace MTA.Client.Commands
{
    /// <summary>
    /// GM commands for controlling scheduled events
    /// </summary>
    public static class EventCommands
    {
        public static bool HandleCommand(GameState client, string[] data, string mess)
        {
            if (data.Length == 0)
                return false;

            // Check if it's a list command: @event -l or @event -list
            if (data[0].ToLower() == "event" && data.Length > 1 && data[1].ToLower() == "-l")
            {
                return HandleEventsListCommand(client, data, mess);
            }

            // Check if it's the main event command: @event <eventId> <start|stop|clear>
            if (data[0].ToLower() == "event")
            {
                return HandleEventCommand(client, data, mess);
            }

            return false;
        }

        private static bool HandleEventCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 3)
            {
                client.Send(new Message("Usage: @event <eventId> <start|stop|clear>", Color.Yellow, Message.TopLeft));
                client.Send(new Message("Example: @event CP_CASTLE start", Color.Yellow, Message.TopLeft));
                client.Send(new Message("List events: @event -l", Color.Yellow, Message.TopLeft));
                return true;
            }

            string eventId = data[1].ToUpper();
            string action = data[2].ToLower();

            var gameEvent = EventScheduler.GetEvent(eventId);
            if (gameEvent == null)
            {
                client.Send(new Message($"Event '{eventId}' not found.", Color.Red, Message.TopLeft));
                client.Send(new Message("Use @event -l to see all available events.", Color.Yellow, Message.TopLeft));
                return true;
            }

            bool success;
            string message;
            switch (action)
            {
                case "start":
                    success = EventScheduler.ForceStartEvent(eventId);
                    message = success
                        ? $"Event '{gameEvent.EventName}' has been forced to start."
                        : $"Failed to start event '{gameEvent.EventName}'.";
                    break;

                case "stop":
                    success = EventScheduler.ForceStopEvent(eventId);
                    message = success
                        ? $"Event '{gameEvent.EventName}' has been forced to stop."
                        : $"Failed to stop event '{gameEvent.EventName}'.";
                    break;

                case "clear":
                    success = EventScheduler.ClearEventOverride(eventId);
                    message = success
                        ? $"Manual override cleared for '{gameEvent.EventName}'. Event will follow scheduled timing."
                        : $"Failed to clear override for '{gameEvent.EventName}'.";
                    break;

                default:
                    client.Send(new Message("Invalid action. Use: start, stop, or clear", Color.Red, Message.TopLeft));
                    return true;
            }

            client.Send(new Message(message, success ? Color.Green : Color.Red, Message.TopLeft));

            // Broadcast to all players if event was started/stopped
            if (success && (action == "start" || action == "stop"))
            {
                string broadcastMessage = action == "start"
                    ? $"GM has manually started: {gameEvent.EventName}"
                    : $"GM has manually stopped: {gameEvent.EventName}";

                Kernel.SendWorldMessage(new Message(broadcastMessage, Color.Orange, Message.System), Program.Values);
            }

            return true;
        }

        private static bool HandleEventsListCommand(GameState client, string[] data, string mess)
        {
            var events = EventScheduler.GetAllEvents().ToList();

            if (events.Count == 0)
            {
                client.Send(new Message("No events are registered.", Color.Yellow, Message.System));
                return true;
            }

            client.Send(new Message("=== Registered Events ===", Color.Cyan, Message.System));

            foreach (var gameEvent in events)
            {
                string status = gameEvent.IsActive ? "ACTIVE" : "INACTIVE";
                Color statusColor = gameEvent.IsActive ? Color.Green : Color.Gray;

                string overrideStatus = "";
                if (gameEvent is BaseEvent baseEvent && baseEvent.IsManuallyOverridden)
                {
                    overrideStatus = baseEvent.IsForcedActive ? " [MANUALLY STARTED]" : " [MANUALLY STOPPED]";
                }

                client.Send(new Message($"#42 {gameEvent.EventId} - {gameEvent.EventName} [{status}]{overrideStatus}",
                    statusColor, Message.System));
            }
            Kernel.SendWorldMessage(new Message("======================", Color.Cyan, Message.System), Program.Values);
            client.Send(new Message("Usage: @event <eventId> <start|stop|clear>", Color.Yellow, Message.System));
            return true;
        }
    }
}

