using System;
using System.Collections.Generic;
using System.Linq;
using MTA.Client;
using System.Drawing;
using MTA.Network.GamePackets;
using MTA;

namespace MTA.Game.Events
{
    /// <summary>
    /// Base class for scheduled events with common functionality
    /// </summary>
    public abstract class BaseEvent : IEvent
    {
        public abstract string EventId { get; }
        public abstract string EventName { get; }
        public bool IsActive { get; protected set; }

        protected DateTime? EventStartTime { get; set; }
        protected DateTime? EventEndTime { get; set; }

        /// <summary>
        /// Whether the event is manually overridden (GM controlled)
        /// </summary>
        public bool IsManuallyOverridden { get; protected set; }

        /// <summary>
        /// Whether the event is forced to be active (GM override)
        /// </summary>
        public bool IsForcedActive { get; protected set; }

        public abstract IEnumerable<EventSchedule> GetSchedules();

        public virtual bool ShouldTrigger(DateTime now)
        {
            foreach (var schedule in GetSchedules())
            {
                if (schedule.Matches(now))
                    return true;
            }
            return false;
        }

        public virtual void OnStart()
        {
            IsActive = true;
            EventStartTime = DateTime.Now;
        }

        public virtual void OnEnd()
        {
            IsActive = false;
            EventEndTime = DateTime.Now;
        }

        /// <summary>
        /// Force start the event (GM override)
        /// </summary>
        public virtual void ForceStart()
        {
            if (!IsActive)
            {
                IsManuallyOverridden = true;
                IsForcedActive = true;
                OnStart();
            }
        }

        /// <summary>
        /// Force stop the event (GM override)
        /// </summary>
        public virtual void ForceStop()
        {
            if (IsActive)
            {
                IsManuallyOverridden = true;
                IsForcedActive = false;
                OnEnd();
            }
        }

        /// <summary>
        /// Clear manual override and return to scheduled timing
        /// </summary>
        public virtual void ClearOverride()
        {
            IsManuallyOverridden = false;
            IsForcedActive = false;
            if (IsActive)
            {
                OnEnd();
            }
        }

        public virtual void OnUpdate(DateTime now)
        {
            // Override in derived classes for periodic updates
        }

        public virtual void OnMonsterKilled(MTA.Database.MonsterInformation monster, Game.Entity killer)
        {
            // Override in derived classes if they need to handle monster deaths
        }

        /// <summary>
        /// Check if normal monster drop should be skipped for this monster
        /// Override in derived classes if the event handles rewards for specific monsters
        /// </summary>
        public virtual bool ShouldSkipNormalDrop(MTA.Database.MonsterInformation monster, ushort mapId)
        {
            // Default: don't skip normal drops
            return false;
        }

        /// <summary>
        /// Send message to all online players
        /// </summary>
        protected void BroadcastMessage(string message, System.Drawing.Color color, uint position = Message.System)
        {
            Kernel.SendWorldMessage(new Message(message, color, position), Program.Values);
        }

        /// <summary>
        /// Send message to specific players
        /// </summary>
        protected void SendMessageToPlayers(IEnumerable<Client.GameState> players, string message, uint position = Message.System)
        {
            foreach (var client in players)
            {
                client.Send(new Message(message, position));
            }
        }

        /// <summary>
        /// Teleport all players from specified maps
        /// </summary>
        protected void TeleportPlayersFromMaps(IEnumerable<ushort> mapIds, ushort targetMapId, ushort targetX, ushort targetY)
        {
            foreach (var client in Program.Values)
            {
                foreach (var mapId in mapIds)
                {
                    if (client.Entity.MapID == mapId)
                    {
                        client.Entity.Teleport(targetMapId, targetX, targetY);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Ensure monsters in specified maps have proper respawn settings
        /// Called automatically when event starts
        /// Override this method to customize respawn behavior for specific events
        /// </summary>
        /// <param name="mapIds">Maps to configure respawns for</param>
        /// <param name="monsterNames">Monster names to configure (null = all monsters)</param>
        /// <param name="respawnTimeSeconds">Respawn time in seconds (default: 30)</param>
        protected virtual void EnsureMonsterRespawns(IEnumerable<ushort> mapIds, IEnumerable<string> monsterNames = null, int respawnTimeSeconds = 30)
        {
            if (mapIds == null)
                return;

            foreach (var mapId in mapIds)
            {
                if (!Kernel.Maps.ContainsKey(mapId))
                    continue;

                var map = Kernel.Maps[mapId];
                if (map?.Entities == null)
                    continue;

                foreach (var entity in map.Entities.Values)
                {
                    if (entity?.MonsterInfo == null)
                        continue;

                    // If specific monster names provided, only configure those
                    if (monsterNames != null && !monsterNames.Contains(entity.MonsterInfo.Name))
                        continue;

                    // Ensure monsters can respawn
                    entity.MonsterInfo.IsRespawnAble = true;
                    entity.MonsterInfo.RespawnTime = respawnTimeSeconds;

                    // Revive dead monsters immediately
                    if (entity.Dead)
                    {
                        entity.Hitpoints = entity.MonsterInfo.Hitpoints;
                        entity.RemoveFlag(entity.StatusFlag);
                        entity.StatusFlag = 0;
                        entity.CauseOfDeathIsMagic = false;

                        // Send spawn to nearby players
                        foreach (var client in Program.Values)
                        {
                            if (client.Map.ID == mapId)
                            {
                                if (Kernel.GetDistance(client.Entity.X, client.Entity.Y, entity.X, entity.Y) < Constants.nScreenDistance)
                                {
                                    entity.SendSpawn(client, false);
                                    var stringPacket = new _String(true)
                                    {
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
        }
    }
}

