using System;
using System.Collections.Generic;
using System.Linq;
using MTA.Database;
using MTA.Game.Events.CpCastle;
using MTA.Game.Events.DizzyLand;
using MTA.Game.Events.SteedRace;
using MTA.Game.Events.TreasureHunt;

namespace MTA.Game.Events;

/// <summary>
///     Manages all scheduled events and their timing
/// </summary>
public static class EventScheduler {
    private static readonly List<IEvent> Events = [];

    /// <summary>
    ///     Register an event to be managed by the scheduler
    /// </summary>
    private static void RegisterEvent(IEvent gameEvent) {
        if (Events.All(e => e.EventId != gameEvent.EventId)) Events.Add(gameEvent);
    }

    /// <summary>
    ///     Unregister an event
    /// </summary>
    public static void UnregisterEvent(string eventId) {
        Events.RemoveAll(e => e.EventId == eventId);
    }

    /// <summary>
    ///     Get all registered events
    /// </summary>
    public static IEnumerable<IEvent> GetAllEvents() {
        return Events.AsReadOnly();
    }

    /// <summary>
    ///     Get a specific event by ID
    /// </summary>
    public static IEvent? GetEvent(string eventId) {
        return Events.FirstOrDefault(e => e.EventId == eventId);
    }

    /// <summary>
    ///     Update all events - should be called every second from World.cs
    /// </summary>
    public static void Update(DateTime now) {
        // Check for events that should trigger
        foreach (var gameEvent in Events) {
            // Skip scheduled triggers if event is manually overridden
            if (gameEvent is BaseEvent { IsManuallyOverridden: true }) {
                // Only update active events that are manually overridden
                if (gameEvent.IsActive) gameEvent.OnUpdate(now);

                continue;
            }

            if (gameEvent.ShouldTrigger(now))
                if (!gameEvent.IsActive)
                    gameEvent.OnStart();

            // Update active events
            if (gameEvent.IsActive) gameEvent.OnUpdate(now);
        }
    }

    /// <summary>
    ///     Force start an event (GM command)
    /// </summary>
    public static bool ForceStartEvent(string eventId) {
        var gameEvent = GetEvent(eventId);
        if (gameEvent is not BaseEvent baseEvent)
            return false;

        baseEvent.ForceStart();
        return true;
    }

    /// <summary>
    ///     Force stop an event (GM command)
    /// </summary>
    public static bool ForceStopEvent(string eventId) {
        var gameEvent = GetEvent(eventId);
        if (gameEvent is not BaseEvent baseEvent)
            return false;

        baseEvent.ForceStop();
        return true;
    }

    /// <summary>
    ///     Clear manual override for an event (GM command)
    /// </summary>
    public static bool ClearEventOverride(string eventId) {
        var gameEvent = GetEvent(eventId);
        if (gameEvent is not BaseEvent baseEvent)
            return false;

        baseEvent.ClearOverride();
        return true;
    }

    /// <summary>
    ///     Initialize and register all events (called on server startup)
    /// </summary>
    public static void Initialize() {
        RegisterEvent(new CpCastleEvent());
        RegisterEvent(new SteedRaceEvent());
        RegisterEvent(new DizzyLandEvent());
        RegisterEvent(new TreasureHuntEvent());
    }

    /// <summary>
    ///     Notify all events of a monster death
    /// </summary>
    public static void OnMonsterKilled(MonsterInformation monster, Entity killer) {
        foreach (var gameEvent in Events) gameEvent.OnMonsterKilled(monster, killer);
    }

    /// <summary>
    ///     Check if any active event wants to skip normal drop for this monster
    /// </summary>
    public static bool ShouldSkipNormalDrop(MonsterInformation monster, ushort mapId) {
        foreach (var gameEvent in Events)
            if (gameEvent.ShouldSkipNormalDrop(monster, mapId))
                return true;

        return false;
    }
}