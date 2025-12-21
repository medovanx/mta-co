using System;
using System.Collections.Generic;
using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Game.MapConstants;

namespace MTA.Game.Events.DizzyLand;

/// <summary>
///     DizzyLand War Event - Runs every hour at :49:00
/// </summary>
public class DizzyLandEvent : BaseEvent {
    public override string EventId => "DIZZY_LAND";
    public override string EventName => "DizzyLand War";

    public override int? EventDurationMinutes => 3;

    private byte AliveCount { get; set; }

    /// <inheritdoc />
    public override IEnumerable<EventSchedule> GetSchedules() {
        // Event runs every hour at :49:00
        for (var hour = 0; hour < 24; hour++) yield return new EventSchedule(hour, 49);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Starts the DizzyLand war and checks alive players.
    /// </remarks>
    public override void OnStart() {
        base.OnStart();

        AutoInviteAllPlayers("The Dizzy Land event has begun! Would you like to join?", TWIN_CITY, 328,
            248);

        CheckAlive();
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Ends the DizzyLand war and broadcasts end message.
    /// </remarks>
    public override void OnEnd() {
        base.OnEnd();
        BroadcastMessage("DizzyLand War has been ended have fun in the next Time!", Color.Red, Message.TopLeft);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Checks alive players and applies Confused flag to players on the map.
    /// </remarks>
    public override void OnUpdate(DateTime now) {
        // Check duration and end event if needed
        base.OnUpdate(now);

        if (!IsActive) return;

        CheckAlive();
    }

    /// <summary>
    ///     Counts alive players and applies Confused flag to players on DizzyLand map
    /// </summary>
    private void CheckAlive() {
        AliveCount = 0;
        foreach (var client in Kernel.GamePool.Values) {
            if (client.Entity.MapID != DIZZY_LAND ||
                client.Entity is not { Hitpoints: >= 1, Dead: false }) continue;
            AliveCount++;

            // Apply Confused flag
            if (!client.Entity.ContainsFlag(Update.Flags.Confused)) client.Entity.AddFlag(Update.Flags.Confused);
        }
    }

    /// <summary>
    ///     Claims prize for the winner and ends the event
    /// </summary>
    public void ClaimPrize(GameState client) {
        if (!IsActive) return;

        client.Entity.ConquerPoints += 500;
        BroadcastMessage(
            $"Congratulations, {client.Entity.Name} has won in DizzyLand War and claimed 500 ConquerPoints!",
            Color.Red, Message.TopLeft);

        OnEnd();
    }

    /// <summary>
    ///     Closes signup and ends the event
    /// </summary>
    public void CloseSignUp() {
        if (!IsActive) return;

        BroadcastMessage("You cant signup into DizzyLand War Come again next Time!", Color.Red, Message.TopLeft);
        OnEnd();
    }
}