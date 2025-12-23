using System;
using System.Collections.Generic;
using MTA.Client;
using MTA.Database;

namespace MTA.Game.Events;

/// <summary>
///     Interface for all scheduled game events
/// </summary>
public interface IEvent {
    /// <summary>
    ///     Unique identifier for this event
    /// </summary>
    string EventId { get; }

    /// <summary>
    ///     Display name of the event
    /// </summary>
    string EventName { get; }

    /// <summary>
    ///     Whether the event is currently active
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    ///     The time when the event started (null if not started)
    /// </summary>
    DateTime? EventStartTime { get; }

    /// <summary>
    ///     The time when the event ended (null if not ended)
    /// </summary>
    DateTime? EventEndTime { get; }

    /// <summary>
    ///     Whether the event is manually overridden (GM controlled)
    /// </summary>
    bool IsManuallyOverridden { get; }

    /// <summary>
    ///     Whether the event is forced to be active (GM override)
    /// </summary>
    bool IsForcedActive { get; }

    /// <summary>
    ///     Event duration in minutes. If set, the event will automatically end after this duration.
    ///     Set to null for events without automatic duration-based ending.
    /// </summary>
    int? EventDurationMinutes { get; }

    /// <summary>
    ///     Check if the event should trigger at the current time
    /// </summary>
    bool ShouldTrigger(DateTime now);

    /// <summary>
    ///     Called when the event starts
    /// </summary>
    void OnStart();

    /// <summary>
    ///     Called when the event ends
    /// </summary>
    void OnEnd();

    /// <summary>
    ///     Called every second while the event is active (for periodic checks)
    /// </summary>
    void OnUpdate(DateTime now);

    /// <summary>
    ///     Get all scheduled times for this event
    /// </summary>
    IEnumerable<EventSchedule> GetSchedules();

    /// <summary>
    ///     Called when a monster is killed (optional - events can implement if they need to handle monster deaths)
    ///     Returns true if normal drop should be skipped (event handles rewards), false otherwise
    /// </summary>
    bool OnMonsterKilled(MonsterInformation monster, Entity killer);

    /// <summary>
    ///     Called every second to allow events to send pre-event warnings (optional)
    ///     This is called for all events, even inactive ones, so they can send warnings before the event starts
    /// </summary>
    void OnPreEventWarning(DateTime now);

    /// <summary>
    ///     Handle player action/event (optional - events can implement if they need to handle player actions like finishing a
    ///     race)
    /// </summary>
    void OnPlayerAction(GameState client, string action);

    /// <summary>
    ///     Handle incoming packet (optional - events can implement if they need to handle specific packets)
    ///     Returns true if packet was handled, false to let it fall through to normal processing
    /// </summary>
    bool HandlePacket(GameState client, byte[] packet, ushort packetId);
}

/// <summary>
///     Represents a scheduled time for an event
/// </summary>
public struct EventSchedule(int hour, int minute, int second = 0, DayOfWeek? dayOfWeek = null) {
    public int Hour { get; set; } = hour;
    public int Minute { get; set; } = minute;
    public int Second { get; set; } = second;
    public DayOfWeek? DayOfWeek { get; set; } = dayOfWeek; // null = every day

    public bool Matches(DateTime now) {
        if (DayOfWeek.HasValue && now.DayOfWeek != DayOfWeek.Value)
            return false;

        return now.Hour == Hour && now.Minute == Minute && now.Second == Second;
    }
}