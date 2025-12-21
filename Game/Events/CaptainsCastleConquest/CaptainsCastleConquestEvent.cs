using System;
using System.Collections.Generic;
using System.Drawing;
using MTA.Database;
using MTA.Network.GamePackets;

namespace MTA.Game.Events.CaptainsCastleConquest;

/// <summary>
///     Captain's Castle Conquest Event
/// </summary>
public class CaptainsCastleConquestEvent : BaseEvent {
    private const int EventStartHour1 = 14;
    private const int EventStartHour2 = 20;
    private const int WarningTime10Min = 10;
    private const int WarningTime5Min = 5;

    private const ushort TwinCityX = 300;
    private const ushort TwinCityY = 280;

    // Castle map IDs
    private static readonly ushort[] CastleMaps =
        [MapConstants.CP_CASTLE_BEGINNER, MapConstants.CP_CASTLE_ADVANCED];

    private DateTime? _lastWarning10Min;
    private DateTime? _lastWarning5Min;
    public override string EventId => "CAPTAINS_CASTLE_CONQUEST";
    public override string EventName => "Captain's Castle Conquest";
    public override int? EventDurationMinutes => 30;

    /// <inheritdoc />
    public override IEnumerable<EventSchedule> GetSchedules() {
        // Event start times
        yield return new EventSchedule(EventStartHour1, 0);
        yield return new EventSchedule(EventStartHour2, 0);
    }

    /// <inheritdoc />
    public override void OnStart() {
        base.OnStart();

        AutoInviteAllPlayers("The Captain's Castle Conquest has begun! Would you like to join?", MapConstants.TWIN_CITY,
            288,
            280);

        Kernel.SendWorldMessage(new Message("The Captain's Castle Conquest has begun!", Color.White, Message.Center),
            Program.Values);

        EnsureMonsterRespawns([MapConstants.CP_CASTLE_BEGINNER, MapConstants.CP_CASTLE_ADVANCED], ["Captain"], 10);
    }

    /// <inheritdoc />
    public override void OnEnd() {
        base.OnEnd();

        BroadcastMessage("The Captain's Castle Conquest has ended. See you next time!", Color.White);

        // Teleport all players out of castle maps
        TeleportPlayersFromMaps(CastleMaps, MapConstants.TWIN_CITY, TwinCityX, TwinCityY);
    }

    /// <inheritdoc />
    public override void OnUpdate(DateTime now) {
        // Check duration and end event if needed
        base.OnUpdate(now);

        if (!IsActive || !EventStartTime.HasValue)
            return;

        var elapsed = now - EventStartTime.Value;
        var remainingMinutes = EventDurationMinutes!.Value - elapsed.TotalMinutes;

        switch (remainingMinutes) {
            // Warning messages (only show once per event)
            case <= WarningTime10Min when _lastWarning10Min != EventStartTime:
                BroadcastMessage("The Captain's Castle Conquest will end in 10 minutes. Hurry to get your rewards!",
                    Color.White);
                _lastWarning10Min = EventStartTime;
                break;
            case <= WarningTime5Min when _lastWarning5Min != EventStartTime:
                BroadcastMessage("The Captain's Castle Conquest will end in 5 minutes. Hurry to get your rewards!",
                    Color.White);
                _lastWarning5Min = EventStartTime;
                break;
        }
    }

    /// <summary>
    ///     Handle monster death for Captain's Castle Conquest event
    /// </summary>
    /// <param name="monster">The monster that was killed</param>
    /// <param name="killer">The entity that killed the monster</param>
    public override void OnMonsterKilled(MonsterInformation monster, Entity killer) {
        // Only handle Captain monsters in Captain's Castle Conquest maps (3030, 3031, 3032, 3033) during active event
        if (!IsActive)
            return;

        // Check if monster is in any Captain's Castle Conquest map
        var mapId = monster.Owner.MapID;
        if (mapId != MapConstants.CP_CASTLE_BEGINNER && mapId != MapConstants.CP_CASTLE_ADVANCED)
            return;

        if (monster.Name != "Captain")
            return;

        CaptainsCastleConquestRewards.OnMonsterKilled(killer.Owner, monster.Name, monster.Owner.MapID);
    }

    /// <summary>
    ///     Skip normal drop for Captain in Captain's Castle Conquest map when event is active
    ///     Event system handles rewards
    /// </summary>
    /// <param name="monster">The monster that was killed</param>
    /// <param name="mapId">The map ID of the monster</param>
    public override bool ShouldSkipNormalDrop(MonsterInformation monster, ushort mapId) {
        // Skip normal CP drop for Captain in any Captain's Castle Conquest map when event is active
        if (!IsActive)
            return false;

        var ownerMapId = monster.Owner.MapID;
        return ownerMapId is MapConstants.CP_CASTLE_BEGINNER or MapConstants.CP_CASTLE_ADVANCED &&
               monster.Name == "Captain";
    }

    /// <summary>
    ///     Send pre-event warnings (called from World.cs for timing before event starts)
    /// </summary>
    /// <param name="now">The current date and time</param>
    public static void SendPreEventWarnings(DateTime now) {
        // 5 minutes before (13:55 / 19:55)
        if (now is { Hour: EventStartHour1 - 1, Minute: 55, Second: 0 } ||
            now is { Hour: EventStartHour2 - 1, Minute: 55, Second: 0 })
            foreach (var client in Program.Values)
                client.Send(new Message("The Captain's Castle Conquest will begin in 5 minutes. Get ready!",
                    Message.System));

        switch (now) {
            // 10 seconds before (13:59:50 / 19:59:50)
            case { Hour: EventStartHour1 - 1, Minute: 59, Second: 50 }:
            case { Hour: EventStartHour2 - 1, Minute: 59, Second: 50 }: {
                foreach (var client in Program.Values)
                    client.Send(new Message("The Captain's Castle Conquest will begin in 10 seconds. Get ready!",
                        Message.System));
                break;
            }
        }
    }
}