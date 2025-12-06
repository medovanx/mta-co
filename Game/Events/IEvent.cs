using System;
using System.Collections.Generic;
using MTA.Client;

namespace MTA.Game.Events
{
    /// <summary>
    /// Interface for all scheduled game events
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Unique identifier for this event
        /// </summary>
        string EventId { get; }

        /// <summary>
        /// Display name of the event
        /// </summary>
        string EventName { get; }

        /// <summary>
        /// Whether the event is currently active
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Check if the event should trigger at the current time
        /// </summary>
        bool ShouldTrigger(DateTime now);

        /// <summary>
        /// Called when the event starts
        /// </summary>
        void OnStart();

        /// <summary>
        /// Called when the event ends
        /// </summary>
        void OnEnd();

        /// <summary>
        /// Called every second while the event is active (for periodic checks)
        /// </summary>
        void OnUpdate(DateTime now);

        /// <summary>
        /// Get all scheduled times for this event
        /// </summary>
        IEnumerable<EventSchedule> GetSchedules();

        /// <summary>
        /// Called when a monster is killed (optional - events can implement if they need to handle monster deaths)
        /// </summary>
        void OnMonsterKilled(MTA.Database.MonsterInformation monster, Game.Entity killer);

        /// <summary>
        /// Check if normal monster drop should be skipped for this monster (event handles rewards instead)
        /// </summary>
        bool ShouldSkipNormalDrop(MTA.Database.MonsterInformation monster, ushort mapId);
    }

    /// <summary>
    /// Represents a scheduled time for an event
    /// </summary>
    public struct EventSchedule
    {
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        public DayOfWeek? DayOfWeek { get; set; } // null = every day

        public EventSchedule(int hour, int minute, int second = 0, DayOfWeek? dayOfWeek = null)
        {
            Hour = hour;
            Minute = minute;
            Second = second;
            DayOfWeek = dayOfWeek;
        }

        public bool Matches(DateTime now)
        {
            if (DayOfWeek.HasValue && now.DayOfWeek != DayOfWeek.Value)
                return false;

            return now.Hour == Hour && now.Minute == Minute && now.Second == Second;
        }
    }
}

