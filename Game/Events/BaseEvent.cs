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
        return GetSchedules().Any(schedule => schedule.Matches(now));
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
        if (!IsActive || !EventStartTime.HasValue || !EventDurationMinutes.HasValue) return;
        var elapsed = now - EventStartTime.Value;
        if (elapsed.TotalMinutes >= EventDurationMinutes.Value) OnEnd();
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Default implementation returns false (don't skip normal drops). Override in derived classes if they need to handle
    ///     monster deaths and skip normal drops. Return true to skip normal drop, false to keep it.
    /// </remarks>
    public virtual bool OnMonsterKilled(MonsterInformation monster, Entity killer) {
        return false;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Default implementation does nothing. Override in derived classes if they need to send pre-event warnings.
    /// </remarks>
    public virtual void OnPreEventWarning(DateTime now) { }

    /// <inheritdoc />
    /// <remarks>
    ///     Default implementation does nothing. Override in derived classes if they need to perform cleanup when inactive.
    /// </remarks>
    public virtual void OnUpdateWhenInactive(DateTime now) { }

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
        if (IsActive) return;
        IsManuallyOverridden = true;
        IsForcedActive = true;
        OnStart();
    }

    /// <summary>
    ///     Force stop the event (GM override)
    /// </summary>
    public virtual void ForceStop() {
        if (!IsActive) return;
        IsManuallyOverridden = true;
        IsForcedActive = false;
        OnEnd();
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
    protected static void BroadcastMessage(string message, Color color, uint position = Message.System) {
        Kernel.SendWorldMessage(new Message(message, color, position), Program.Values);
    }

    /// <summary>
    ///     Send message to specific players
    /// </summary>
    protected static void SendMessageToPlayers(IEnumerable<GameState> players, string message,
        uint position = Message.System) {
        foreach (var client in players) client.Send(new Message(message, position));
    }

    /// <summary>
    ///     Teleport all players from specified maps to a specific location
    /// </summary>
    protected static void TeleportPlayersFromMaps(IEnumerable<ushort> mapIds, ushort targetMapId, ushort targetX,
        ushort targetY) {
        var materializedMapIds = mapIds as ushort[] ?? mapIds.ToArray();
        foreach (var client in Program.Values) {
            if (materializedMapIds.Any(mapId => client.Entity.MapID == mapId)) {
                client.Entity.Teleport(targetMapId, targetX, targetY);
            }
        }
    }

    /// <summary>
    ///     Send an auto-invite message box to all online players with teleportation on accept
    /// </summary>
    /// <param name="message">The invitation message to display</param>
    /// <param name="targetMapId">Map ID to teleport player to when they accept</param>
    /// <param name="targetX">X coordinate to teleport player to when they accept</param>
    /// <param name="targetY">Y coordinate to teleport player to when they accept</param>
    /// <param name="timeoutSeconds">Timeout in seconds before the message box expires (default: 60)</param>
    protected static void AutoInviteAllPlayers(string message, ushort targetMapId, ushort targetX, ushort targetY,
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
        return EventScheduleFormatter.FormatScheduleDescription(GetSchedules());
    }

    /// <summary>
    ///     Ensure a monster spawns at a specific location on a map, or updates existing monsters' respawn settings
    ///     If coordinates are provided, spawns/updates a monster at that exact location
    ///     If coordinates are null, updates all monsters of the specified npctype in the specified maps
    /// </summary>
    /// <param name="mapIds">Map IDs to configure monsters for</param>
    /// <param name="npctype">Monster npctype (ID) from MonsterInformation</param>
    /// <param name="respawnTimeSeconds">Respawn time in seconds (default: 30)</param>
    /// <param name="x">Optional X coordinate to spawn at (null = update all existing monsters of this type)</param>
    /// <param name="y">Optional Y coordinate to spawn at (null = update all existing monsters of this type)</param>
    /// <param name="customName">Optional custom name for the monster (null = use default name from MonsterInformation)</param>
    /// <param name="isBoss">Whether to mark this monster as a boss (default: uses value from MonsterInformation)</param>
    /// <returns>The spawned/updated Entity, or null if monster information not found</returns>
    protected static Entity? EnsureMonsterSpawn(IEnumerable<ushort> mapIds, uint npctype, int respawnTimeSeconds = 30,
        ushort? x = null, ushort? y = null, string? customName = null, bool? isBoss = null) {
        if (!MonsterInformation.MonsterInformations.TryGetValue(npctype, out var monsterInfo)) return null;

        Entity? result = null;

        foreach (var mapId in mapIds) {
            if (!Kernel.Maps.TryGetValue(mapId, out var map)) continue;

            // If coordinates are provided, spawn/update at specific location
            if (x.HasValue && y.HasValue) {
                // Check if monster already exists at this location
                var existingEntity = map.Entities.Values.FirstOrDefault(e =>
                    e.MonsterInfo.ID == npctype && e.X == x.Value && e.Y == y.Value);

                if (existingEntity != null) {
                    // Update existing monster
                    existingEntity.MonsterInfo.IsRespawnAble = true;
                    existingEntity.MonsterInfo.RespawnTime = respawnTimeSeconds;
                    if (customName != null) existingEntity.Name = customName;
                    if (isBoss.HasValue) existingEntity.MonsterInfo.Boss = isBoss.Value;
                    result = existingEntity;
                    continue;
                }

                // Create new monster entity at specific location
                var mt = monsterInfo.Copy();
                var entity = new Entity(EntityFlag.Monster, false) {
                    MapObjType = MapObjectType.Monster,
                    MonsterInfo = mt
                };
                entity.MonsterInfo.Owner = entity;
                entity.Name = customName ?? mt.Name;
                entity.MinAttack = mt.MinAttack;
                entity.MaxAttack = entity.MagicAttack = mt.MaxAttack;
                entity.Hitpoints = entity.MaxHitpoints = mt.Hitpoints;
                entity.Defence = mt.Defence;
                entity.Body = mt.Mesh;
                entity.Level = mt.Level;
                entity.UID = map.EntityUIDCounter.Next;
                entity.MapID = mapId;
                entity.SendUpdates = true;
                entity.MonsterInfo.IsRespawnAble = true;
                entity.MonsterInfo.RespawnTime = respawnTimeSeconds;
                if (isBoss.HasValue) entity.MonsterInfo.Boss = isBoss.Value;

                // Ensure valid coordinates
                var spawnX = x.Value;
                var spawnY = y.Value;
                if (!map.SelectCoordonates(ref spawnX, ref spawnY)) {
                    // If coordinates are invalid, try to find nearby valid spot
                    for (var offset = 1; offset <= 5; offset++) {
                        var testX = (ushort)(x.Value + offset);
                        var testY = y.Value;
                        if (!map.SelectCoordonates(ref testX, ref testY)) continue;
                        spawnX = testX;
                        spawnY = testY;
                        break;
                    }
                }

                entity.X = spawnX;
                entity.Y = spawnY;

                // Add to map
                map.AddEntity(entity);

                // Send spawn to all players on the map
                foreach (var client in Program.Values) {
                    if (client.Entity.MapID != mapId) continue;
                    if (Kernel.GetDistance(client.Entity.X, client.Entity.Y, entity.X, entity.Y) >=
                        Constants.nScreenDistance) continue;
                    entity.SendSpawn(client, false);
                    var stringPacket = new _String(true) {
                        UID = entity.UID,
                        Type = _String.Effect
                    };
                    stringPacket.Texts.Add("MBStandard");
                    client.Send(stringPacket);
                }

                result = entity;
            }
            else {
                // No coordinates provided - update all existing monsters of this type in the map
                foreach (var entity in map.Entities.Values.Where(e => e.MonsterInfo.ID == npctype)) {
                    // Ensure monsters can respawn
                    entity.MonsterInfo.IsRespawnAble = true;
                    entity.MonsterInfo.RespawnTime = respawnTimeSeconds;
                    if (customName != null) entity.Name = customName;
                    if (isBoss.HasValue) entity.MonsterInfo.Boss = isBoss.Value;

                    // Revive dead monsters immediately
                    if (entity.Dead) {
                        entity.Hitpoints = entity.MonsterInfo.Hitpoints;
                        entity.RemoveFlag(entity.StatusFlag);
                        entity.StatusFlag = 0;
                        entity.CauseOfDeathIsMagic = false;
                    }

                    // Send spawn to nearby players
                    foreach (var client in Program.Values) {
                        if (client.Entity.MapID != mapId) continue;
                        if (Kernel.GetDistance(client.Entity.X, client.Entity.Y, entity.X, entity.Y) >=
                            Constants.nScreenDistance) continue;
                        entity.SendSpawn(client, false);
                        var stringPacket = new _String(true) {
                            UID = entity.UID,
                            Type = _String.Effect
                        };
                        stringPacket.Texts.Add("MBStandard");
                        client.Send(stringPacket);
                    }

                    result = entity;
                }
            }
        }

        return result;
    }
}