using System;
using System.Collections.Generic;
using System.Linq;
using MTA.Client;
using System.Drawing;
using MTA.Network.GamePackets;

namespace MTA.Game.Events.CpCastle
{
    /// <summary>
    /// CP Castle Event
    /// </summary>
    public class CpCastleEvent : BaseEvent
    {
        public override string EventId => "CP_CASTLE";
        public override string EventName => "CP Castle Event";

        private const int EVENT_START_HOUR_1 = 14;
        private const int EVENT_START_HOUR_2 = 20;
        private const int EVENT_DURATION_MINUTES = 30;
        private const int WARNING_TIME_10_MIN = 10;
        private const int WARNING_TIME_5_MIN = 5;

        // Castle map IDs
        private static readonly ushort[] CASTLE_MAPS = [MapConstants.CP_CASTLE_BEGINNER, MapConstants.CP_CASTLE_ADVANCED];
        private const ushort TWIN_CITY_X = 300;
        private const ushort TWIN_CITY_Y = 280;

        private DateTime? _lastWarning10Min = null;
        private DateTime? _lastWarning5Min = null;

        public override IEnumerable<EventSchedule> GetSchedules()
        {
            // Event start times
            yield return new EventSchedule(EVENT_START_HOUR_1, 0, 0);
            yield return new EventSchedule(EVENT_START_HOUR_2, 0, 0);
        }

        public override bool ShouldTrigger(DateTime now)
        {
            // Check if event should start
            if ((now.Hour == EVENT_START_HOUR_1 || now.Hour == EVENT_START_HOUR_2) && now.Minute == 0 && now.Second == 0)
            {
                return true;
            }

            // Check if event should end
            if (IsActive)
            {
                if (EventStartTime.HasValue)
                {
                    var elapsed = now - EventStartTime.Value;
                    if (elapsed.TotalMinutes >= EVENT_DURATION_MINUTES)
                    {
                        return true; // Trigger end
                    }
                }
            }

            return false;
        }

        public override void OnStart()
        {
            base.OnStart();

            BroadcastMessage("Hurry! The CP Castle Event has begun! Log in now to participate!", Color.White, Message.Service);

            // Auto-invite all players
            foreach (var client in Program.Values)
            {
                client.MessageBox("The CP Castle Event has begun! Would you like to join?",
                   p => { p.Entity.Teleport(MapConstants.TWIN_CITY, 288, 280); }, null, 60);
            }

            Kernel.SendWorldMessage(new Message("The CP Castle War has begun!", Color.White, Message.Center), Program.Values);

            EnsureMonsterRespawns([MapConstants.CP_CASTLE_BEGINNER, MapConstants.CP_CASTLE_ADVANCED], ["Captain"], 10);
        }

        public override void OnEnd()
        {
            base.OnEnd();

            BroadcastMessage("The CP Castle Event has ended. See you next time!", Color.White, Message.System);

            // Teleport all players out of castle maps
            TeleportPlayersFromMaps(CASTLE_MAPS, MapConstants.TWIN_CITY, TWIN_CITY_X, TWIN_CITY_Y);
        }

        public override void OnUpdate(DateTime now)
        {
            if (!IsActive || !EventStartTime.HasValue)
                return;

            var elapsed = now - EventStartTime.Value;
            var remainingMinutes = EVENT_DURATION_MINUTES - elapsed.TotalMinutes;

            // Check for end condition
            if (remainingMinutes <= 0)
            {
                OnEnd();
                return;
            }

            // Warning messages (only show once per event)
            if (remainingMinutes <= WARNING_TIME_10_MIN && _lastWarning10Min != EventStartTime)
            {
                BroadcastMessage("The CP Castle Event will end in 10 minutes. Hurry to get your rewards!", Color.White, Message.System);
                _lastWarning10Min = EventStartTime;
            }
            else if (remainingMinutes <= WARNING_TIME_5_MIN && _lastWarning5Min != EventStartTime)
            {
                BroadcastMessage("The CP Castle Event will end in 5 minutes. Hurry to get your rewards!", Color.White, Message.System);
                _lastWarning5Min = EventStartTime;
            }
        }

        /// <summary>
        /// Check if event is currently active (for NPC checks)
        /// </summary>
        public static bool IsEventActive()
        {
            var gameEvent = EventScheduler.GetEvent("CP_CASTLE");
            if (gameEvent == null || gameEvent is not CpCastleEvent cpCastleEvent || !cpCastleEvent.IsActive)
                return false;

            var now = DateTime.Now;
            if (!cpCastleEvent.EventStartTime.HasValue)
                return false;

            var elapsed = now - cpCastleEvent.EventStartTime.Value;
            return elapsed.TotalMinutes < EVENT_DURATION_MINUTES;
        }

        /// <summary>
        /// Handle monster death for CP Castle event
        /// </summary>
        public override void OnMonsterKilled(Database.MonsterInformation monster, Game.Entity killer)
        {
            // Only handle Captain monsters in CP Castle maps (3030, 3031, 3032, 3033) during active event
            if (!IsActive)
                return;

            if (monster.Owner == null)
                return;

            // Check if monster is in any CP Castle map
            ushort mapId = monster.Owner.MapID;
            if (mapId != MapConstants.CP_CASTLE_BEGINNER && mapId != MapConstants.CP_CASTLE_ADVANCED)
                return;

            if (monster.Name != "Captain")
                return;

            if (killer.Owner != null)
            {
                // Give rewards for killing Captain during CP Castle event
                CpCastleRewards.OnMonsterKilled(killer.Owner, monster.Name, monster.Owner.MapID);
            }
        }

        /// <summary>
        /// Skip normal drop for Captain in CP Castle map when event is active
        /// Event system handles rewards
        /// </summary>
        public override bool ShouldSkipNormalDrop(MTA.Database.MonsterInformation monster, ushort mapId)
        {
            // Skip normal CP drop for Captain in any CP Castle map when event is active
            if (!IsActive || monster.Owner == null)
                return false;

            ushort ownerMapId = monster.Owner.MapID;
            return (ownerMapId == MapConstants.CP_CASTLE_BEGINNER || ownerMapId == MapConstants.CP_CASTLE_ADVANCED) && monster.Name == "Captain";
        }

        /// <summary>
        /// Send pre-event warnings (called from World.cs for timing before event starts)
        /// </summary>
        public static void SendPreEventWarnings(DateTime now)
        {
            // 5 minutes before (13:55 / 19:55)
            if ((now.Hour == EVENT_START_HOUR_1 - 1 && now.Minute == 55 && now.Second == 0) ||
                (now.Hour == EVENT_START_HOUR_2 - 1 && now.Minute == 55 && now.Second == 0))
            {
                foreach (var client in Program.Values)
                {
                    client.Send(new Message("The CP Castle Event will begin in 5 minutes. Get ready!", Message.System));
                }
            }

            // 10 seconds before (13:59:50 / 19:59:50)
            if ((now.Hour == EVENT_START_HOUR_1 - 1 && now.Minute == 59 && now.Second == 50) ||
                (now.Hour == EVENT_START_HOUR_2 - 1 && now.Minute == 59 && now.Second == 50))
            {
                foreach (var client in Program.Values)
                {
                    client.Send(new Message("The CP Castle Event will begin in 10 seconds. Get ready!", Message.System));
                }
            }
        }
    }
}

