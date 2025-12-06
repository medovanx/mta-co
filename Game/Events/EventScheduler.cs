using System;
using System.Collections.Generic;
using System.Linq;

namespace MTA.Game.Events
{
    /// <summary>
    /// Manages all scheduled events and their timing
    /// </summary>
    public static class EventScheduler
    {
        private static readonly List<IEvent> _events = new List<IEvent>();
        private static DateTime _lastCheck = DateTime.Now;

        /// <summary>
        /// Register an event to be managed by the scheduler
        /// </summary>
        public static void RegisterEvent(IEvent gameEvent)
        {
            if (!_events.Any(e => e.EventId == gameEvent.EventId))
            {
                _events.Add(gameEvent);
            }
        }

        /// <summary>
        /// Unregister an event
        /// </summary>
        public static void UnregisterEvent(string eventId)
        {
            _events.RemoveAll(e => e.EventId == eventId);
        }

        /// <summary>
        /// Get all registered events
        /// </summary>
        public static IEnumerable<IEvent> GetAllEvents()
        {
            return _events.AsReadOnly();
        }

        /// <summary>
        /// Get a specific event by ID
        /// </summary>
        public static IEvent GetEvent(string eventId)
        {
            return _events.FirstOrDefault(e => e.EventId == eventId);
        }

        /// <summary>
        /// Update all events - should be called every second from World.cs
        /// </summary>
        public static void Update(DateTime now)
        {
            // Check for events that should trigger
            foreach (var gameEvent in _events)
            {
                // Skip scheduled triggers if event is manually overridden
                if (gameEvent is BaseEvent baseEvent && baseEvent.IsManuallyOverridden)
                {
                    // Only update active events that are manually overridden
                    if (gameEvent.IsActive)
                    {
                        gameEvent.OnUpdate(now);
                    }
                    continue;
                }

                if (gameEvent.ShouldTrigger(now))
                {
                    if (!gameEvent.IsActive)
                    {
                        gameEvent.OnStart();
                    }
                }

                // Update active events
                if (gameEvent.IsActive)
                {
                    gameEvent.OnUpdate(now);
                }
            }

            _lastCheck = now;
        }

        /// <summary>
        /// Force start an event (GM command)
        /// </summary>
        public static bool ForceStartEvent(string eventId)
        {
            var gameEvent = GetEvent(eventId);
            if (gameEvent == null || gameEvent is not BaseEvent baseEvent)
                return false;

            baseEvent.ForceStart();
            return true;
        }

        /// <summary>
        /// Force stop an event (GM command)
        /// </summary>
        public static bool ForceStopEvent(string eventId)
        {
            var gameEvent = GetEvent(eventId);
            if (gameEvent == null || gameEvent is not BaseEvent baseEvent)
                return false;

            baseEvent.ForceStop();
            return true;
        }

        /// <summary>
        /// Clear manual override for an event (GM command)
        /// </summary>
        public static bool ClearEventOverride(string eventId)
        {
            var gameEvent = GetEvent(eventId);
            if (gameEvent == null || gameEvent is not BaseEvent baseEvent)
                return false;

            baseEvent.ClearOverride();
            return true;
        }

        /// <summary>
        /// Initialize all events (call on server startup)
        /// </summary>
        public static void Initialize()
        {
            // Register all events here
            RegisterEvent(new MTA.Game.Events.CpCastle.CpCastleEvent());
            // Add more events as they are created...
        }

        /// <summary>
        /// Notify all events of a monster death
        /// </summary>
        public static void OnMonsterKilled(MTA.Database.MonsterInformation monster, Game.Entity killer)
        {
            foreach (var gameEvent in _events)
            {
                gameEvent.OnMonsterKilled(monster, killer);
            }
        }

        /// <summary>
        /// Check if any active event wants to skip normal drop for this monster
        /// </summary>
        public static bool ShouldSkipNormalDrop(MTA.Database.MonsterInformation monster, ushort mapId)
        {
            foreach (var gameEvent in _events)
            {
                if (gameEvent.ShouldSkipNormalDrop(monster, mapId))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

