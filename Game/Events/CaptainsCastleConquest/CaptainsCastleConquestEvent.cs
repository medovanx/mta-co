using System;
using System.Collections.Generic;
using System.Drawing;
using MTA.Database;
using MTA.Game.Constants;
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
        [Maps.CAPTAIN_CASTLE_BEGINNER, Maps.CAPTAIN_CASTLE_ADVANCED];

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

        AutoInviteAllPlayers("The Captain's Castle Conquest has begun! Would you like to join?", Maps.TwinCity,
            288,
            280);

        Kernel.SendWorldMessage(new Message("The Captain's Castle Conquest has begun!", Color.White, Message.Center),
            Program.Values);

        EnsureMonsterSpawn([Maps.CAPTAIN_CASTLE_BEGINNER, Maps.CAPTAIN_CASTLE_ADVANCED],
            Monsters.Captain, 10);
    }

    /// <inheritdoc />
    public override void OnEnd() {
        base.OnEnd();
        TeleportPlayersFromMaps(CastleMaps, Maps.TwinCity, TwinCityX, TwinCityY);
        BroadcastMessage(
            "The Captain's Castle Conquest has ended. See you next time!",
            Color.White, Message.Center);
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

    /// <inheritdoc />
    /// <remarks>
    ///     Handle monster death for Captain's Castle Conquest event
    ///     Returns true to skip normal drop, false to keep it
    /// </remarks>
    public override bool OnMonsterKilled(MonsterInformation monster, Entity killer) {
        // Only handle Captain monsters in Captain's Castle Conquest maps during active event
        if (!IsActive)
            return false;

        // Check if monster is in any Captain's Castle Conquest map
        var mapId = monster.Owner.MapID;
        if (mapId != Maps.CAPTAIN_CASTLE_BEGINNER && mapId != Maps.CAPTAIN_CASTLE_ADVANCED)
            return false;

        if (monster.ID != Monsters.Captain)
            return false;

        CaptainsCastleConquestRewards.OnMonsterKilled(killer.Owner, monster.Name, monster.Owner.MapID);
        return true; // Skip normal drop
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Send pre-event warnings before the event starts
    /// </remarks>
    public override void OnPreEventWarning(DateTime now) {
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