using System;
using System.Collections.Generic;
using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Game.Events.CaptainsCastleConquest;
using MTA.Game.Events.DizzyLand;
using MTA.Game.Events.GuildWar;
using MTA.Game.Events.SteedRace;
using MTA.Game.Events.TreasureInTheBlue;

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
            // Call pre-event warnings for all events (even inactive ones, so they can send warnings before start)
            gameEvent.OnPreEventWarning(now);

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
            if (gameEvent.IsActive) {
                gameEvent.OnUpdate(now);
            }
            else {
                // Call OnUpdateWhenInactive for inactive events (allows cleanup tasks)
                gameEvent.OnUpdateWhenInactive(now);
            }
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
        RegisterEvent(new CaptainsCastleConquestEvent());
        RegisterEvent(new SteedRaceEvent());
        RegisterEvent(new DizzyLandEvent());
        RegisterEvent(new TreasureInTheBlueEvent());
        var guildWarEvent = new GuildWarEvent();
        RegisterEvent(guildWarEvent);
        // Initialize Guild War event to restore pole keeper from database
        guildWarEvent.Initialize();
    }

    /// <summary>
    ///     Notify all events of a monster death and check if normal drop should be skipped
    ///     Returns true if any event wants to skip normal drop (OR logic)
    /// </summary>
    public static bool OnMonsterKilled(MonsterInformation monster, Entity killer) {
        var shouldSkip = false;
        foreach (var unused in Events.Where(gameEvent => gameEvent.OnMonsterKilled(monster, killer))) {
            shouldSkip = true; // OR logic: if any event says skip, skip
        }

        return shouldSkip;
    }

    /// <summary>
    ///     Notify all events of an entity attack and check if attack was handled
    ///     Returns true if any event handled the attack (OR logic - skip normal damage processing)
    /// </summary>
    public static bool OnEntityAttacked(Entity attacker, Interfaces.IMapObject attacked, uint damage) {
        var wasHandled = false;
        foreach (var unused in Events.Where(gameEvent => gameEvent.OnEntityAttacked(attacker, attacked, damage))) {
            wasHandled = true; // OR logic: if any event handles it, skip normal processing
        }

        return wasHandled;
    }

    /// <summary>
    ///     Notify all events of player movement and check if movement should be blocked
    ///     Returns true if any event blocks movement (OR logic - block if any event says block)
    /// </summary>
    public static bool OnPlayerMovement(GameState client, ushort oldX, ushort oldY) {
        var shouldBlock = false;
        foreach (var unused in Events.Where(gameEvent => gameEvent.OnPlayerMovement(client, oldX, oldY))) {
            shouldBlock = true; // OR logic: if any event blocks it, block movement
        }

        return shouldBlock;
    }
}