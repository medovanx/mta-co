using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;

namespace MTA.Game.Events;

/// <summary>
///     Base class for scheduled events with common functionality
/// </summary>
public abstract class BaseEvent : IEvent {
    /// <inheritdoc />
    public abstract string EventId { get; }

    /// <inheritdoc />
    public abstract string EventName { get; }

    /// <inheritdoc />
    public bool IsActive { get; private set; }

    /// <inheritdoc />
    public DateTime? EventStartTime { get; private set; }

    /// <inheritdoc />
    public DateTime? EventEndTime { get; private set; }

    /// <inheritdoc />
    public virtual int? EventDurationMinutes { get; } = null;

    /// <inheritdoc />
    public bool IsManuallyOverridden { get; private set; }

    /// <inheritdoc />
    public bool IsForcedActive { get; private set; }

    /// <inheritdoc />
    public abstract IEnumerable<EventSchedule> GetSchedules();

    /// <inheritdoc />
    /// <remarks>
    ///     Default implementation checks if the current time matches any of the scheduled times.
    /// </remarks>
    public virtual bool ShouldTrigger(DateTime now) {
        foreach (var schedule in GetSchedules())
            if (schedule.Matches(now))
                return true;

        return false;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Default implementation sets IsActive to true and records the start time.
    /// </remarks>
    public virtual void OnStart() {
        IsActive = true;
        EventStartTime = DateTime.Now;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Default implementation sets IsActive to false and records the end time.
    /// </remarks>
    public virtual void OnEnd() {
        IsActive = false;
        EventEndTime = DateTime.Now;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Default implementation checks event duration if EventDurationMinutes is set and calls OnEnd() if exceeded.
    ///     Override in derived classes for periodic updates, but call base.OnUpdate(now) to preserve duration checking.
    /// </remarks>
    public virtual void OnUpdate(DateTime now) {
        // Check duration-based ending
        if (IsActive && EventStartTime.HasValue && EventDurationMinutes.HasValue) {
            var elapsed = now - EventStartTime.Value;
            if (elapsed.TotalMinutes >= EventDurationMinutes.Value) OnEnd();
        }
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Default implementation does nothing. Override in derived classes if they need to handle monster deaths.
    /// </remarks>
    public virtual void OnMonsterKilled(MonsterInformation monster, Entity killer) { }

    /// <inheritdoc />
    /// <remarks>
    ///     Default implementation returns false (don't skip normal drops). Override in derived classes if the event handles
    ///     rewards for specific monsters.
    /// </remarks>
    public virtual bool ShouldSkipNormalDrop(MonsterInformation monster, ushort mapId) {
        return false;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Default implementation does nothing. Override in derived classes if they need to handle player actions.
    /// </remarks>
    public virtual void OnPlayerAction(GameState client, string action) { }

    /// <inheritdoc />
    /// <remarks>
    ///     Default implementation returns false (packet not handled). Override in derived classes if they need to handle
    ///     specific packets.
    /// </remarks>
    public virtual bool HandlePacket(GameState client, byte[] packet, ushort packetId) {
        return false;
    }

    /// <summary>
    ///     Force start the event (GM override)
    /// </summary>
    public virtual void ForceStart() {
        if (!IsActive) {
            IsManuallyOverridden = true;
            IsForcedActive = true;
            OnStart();
        }
    }

    /// <summary>
    ///     Force stop the event (GM override)
    /// </summary>
    public virtual void ForceStop() {
        if (IsActive) {
            IsManuallyOverridden = true;
            IsForcedActive = false;
            OnEnd();
        }
    }

    /// <summary>
    ///     Clear manual override and return to scheduled timing
    /// </summary>
    public virtual void ClearOverride() {
        IsManuallyOverridden = false;
        IsForcedActive = false;
        if (IsActive) OnEnd();
    }

    /// <summary>
    ///     Send message to all online players
    /// </summary>
    protected void BroadcastMessage(string message, Color color, uint position = Message.System) {
        Kernel.SendWorldMessage(new Message(message, color, position), Program.Values);
    }

    /// <summary>
    ///     Send message to specific players
    /// </summary>
    protected void SendMessageToPlayers(IEnumerable<GameState> players, string message,
        uint position = Message.System) {
        foreach (var client in players) client.Send(new Message(message, position));
    }

    /// <summary>
    ///     Teleport all players from specified maps
    /// </summary>
    protected void TeleportPlayersFromMaps(IEnumerable<ushort> mapIds, ushort targetMapId, ushort targetX,
        ushort targetY) {
        var materializedMapIds = mapIds as ushort[] ?? mapIds.ToArray();
        foreach (var client in Program.Values)
            if (materializedMapIds.Any(mapId => client.Entity.MapID == mapId))
                client.Entity.Teleport(targetMapId, targetX, targetY);
    }

    /// <summary>
    ///     Send an auto-invite message box to all online players with teleportation on accept
    /// </summary>
    /// <param name="message">The invitation message to display</param>
    /// <param name="targetMapId">Map ID to teleport player to when they accept</param>
    /// <param name="targetX">X coordinate to teleport player to when they accept</param>
    /// <param name="targetY">Y coordinate to teleport player to when they accept</param>
    /// <param name="timeoutSeconds">Timeout in seconds before the message box expires (default: 60)</param>
    protected void AutoInviteAllPlayers(string message, ushort targetMapId, ushort targetX, ushort targetY,
        uint timeoutSeconds = 60) {
        foreach (var client in Program.Values) {
            client.MessageBox(message, p => { p.Entity.Teleport(targetMapId, targetX, targetY); }, null,
                timeoutSeconds);
        }
    }

    /// <summary>
    ///     Get a formatted string describing when this event runs (for NPC dialogs)
    /// </summary>
    public string GetScheduleDescription() {
        var schedules = GetSchedules().ToList();
        if (schedules.Count == 0)
            return "Schedule not available.";

        // Group schedules by pattern
        var timeGroups = schedules.GroupBy(s => new { s.Hour, s.Minute, s.Second }).ToList();

        if (timeGroups.Count == 1) {
            var schedule = schedules[0];
            var timeStr = $"{schedule.Hour:D2}:{schedule.Minute:D2}";

            // Check if it's every hour (same minute across all hours)
            if (schedules.Count == 24 && schedules.All(s => s.Minute == schedule.Minute && !s.DayOfWeek.HasValue))
                return $"at :{schedule.Minute:D2} of every hour";

            // Check if it's every day
            if (!schedule.DayOfWeek.HasValue) return $"daily at {timeStr}";

            // Specific day(s)
            var days = schedules.Where(s => s.DayOfWeek.HasValue).Select(s => s.DayOfWeek!.Value).Distinct()
                .ToList();
            return days.Count == 1 ? $"every {days[0]} at {timeStr}" : $"at {timeStr} on {string.Join(", ", days)}";
        }

        // Multiple different times
        var timeStrings = timeGroups.Select(g => {
            var s = g.First();
            var timeStr = $"{s.Hour:D2}:{s.Minute:D2}";
            if (s.DayOfWeek.HasValue) return $"{timeStr} on {s.DayOfWeek.Value}";

            return timeStr;
        }).ToList();

        if (timeStrings.Count == 2) return $"daily at {timeStrings[0]} and {timeStrings[1]}";

        return $"at {string.Join(", ", timeStrings.Take(timeStrings.Count - 1))}, and {timeStrings.Last()}";
    }

    /// <summary>
    ///     Ensure monsters in specified maps have proper respawn settings
    ///     Called automatically when event starts
    ///     Override this method to customize respawn behavior for specific events
    /// </summary>
    /// <param name="mapIds">Maps to configure respawns for</param>
    /// <param name="monsterNames">Monster names to configure (null = all monsters)</param>
    /// <param name="respawnTimeSeconds">Respawn time in seconds (default: 30)</param>
    protected virtual void EnsureMonsterRespawns(IEnumerable<ushort> mapIds, IEnumerable<string> monsterNames,
        int respawnTimeSeconds = 30) {
        foreach (var mapId in mapIds) {
            if (!Kernel.Maps.ContainsKey(mapId))
                continue;

            var map = Kernel.Maps[mapId];

            foreach (var entity in map.Entities.Values.Where(entity =>
                         monsterNames.Contains(entity.MonsterInfo.Name))) {
                // Ensure monsters can respawn
                entity.MonsterInfo.IsRespawnAble = true;
                entity.MonsterInfo.RespawnTime = respawnTimeSeconds;

                // Revive dead monsters immediately
                if (entity.Dead) {
                    entity.Hitpoints = entity.MonsterInfo.Hitpoints;
                    entity.RemoveFlag(entity.StatusFlag);
                    entity.StatusFlag = 0;
                    entity.CauseOfDeathIsMagic = false;

                    // Send spawn to nearby players
                    foreach (var client in Program.Values)
                        if (client.Map.ID == mapId)
                            if (Kernel.GetDistance(client.Entity.X, client.Entity.Y, entity.X, entity.Y) <
                                Constants.nScreenDistance) {
                                entity.SendSpawn(client, false);
                                var stringPacket = new _String(true) {
                                    UID = entity.UID,
                                    Type = _String.Effect
                                };
                                stringPacket.Texts.Add("MBStandard");
                                client.Send(stringPacket);
                            }
                }
            }
        }
    }
}