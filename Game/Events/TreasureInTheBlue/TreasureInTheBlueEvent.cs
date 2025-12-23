using System;
using System.Collections.Generic;
using System.Drawing;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;

namespace MTA.Game.Events.TreasureInTheBlue;

/// <summary>
///     Treasure in the Blue Event - Collect coins from monsters in the Proud Sea and trade them for rewards
///     Based on official event: https://co.99.com/guide/quests/2012/blue_treasure.shtml
/// </summary>
public class TreasureInTheBlueEvent : BaseEvent {
    public readonly TreasureInTheBlueCoinTracker CoinTracker = new();

    public override string EventId => "TREASURE_IN_THE_BLUE";
    public override string EventName => "Treasure in the Blue";

    public override int? EventDurationMinutes => 60;

    /// <inheritdoc />
    public override IEnumerable<EventSchedule> GetSchedules() {
        // Event runs Monday-Saturday at 12:30 and 20:30
        for (var day = DayOfWeek.Monday; day <= DayOfWeek.Saturday; day++) {
            yield return new EventSchedule(12, 30, 0, day);
            yield return new EventSchedule(20, 30, 0, day);
        }
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Starts the Treasure in the Blue event, sends invitations, and broadcasts start message.
    /// </remarks>
    public override void OnStart() {
        base.OnStart();

        CoinTracker.Reset();

        AutoInviteAllPlayers("The Treasure in the Blue has begun! Would you like to join the Proud Sea?",
            MapConstants.TWIN_CITY,
            323, 269);

        BroadcastMessage(
            "The Treasure in the Blue has begun! Venture into the Proud Sea and collect ancient coins! Remember: coins expire after 60 minutes!",
            Color.White, Message.Center);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Ends the Treasure in the Blue event and teleports all players out.
    /// </remarks>
    public override void OnEnd() {
        base.OnEnd();

        // Teleport all players out of coins map
        foreach (var client in Program.Values) {
            if (client.Entity.MapID != MapConstants.ProudSea) continue;
            client.Entity.BringToLife();
            client.Entity.Teleport(MapConstants.TWIN_CITY, 304, 287);
        }

        BroadcastMessage(
            "The Treasure in the Blue has ended! All adventurers have been returned to Twin City. Thank you for participating!",
            Color.White, Message.Center);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Checks if event is still running, handles coin expiration, and teleports players if not.
    /// </remarks>
    public override void OnUpdate(DateTime now) {
        // Check duration and end event if needed
        base.OnUpdate(now);

        if (!IsActive) {
            // Teleport players if event ended
            foreach (var client in Program.Values) {
                if (client.Entity.MapID != MapConstants.ProudSea ||
                    client.Account.State == AccountTable.AccountState.GM) continue;
                client.Entity.Teleport(MapConstants.TWIN_CITY, 304, 287);
            }

            BroadcastMessage(
                "The Treasure in the Blue has ended! All adventurers have been returned to Twin City. Thank you for participating!",
                Color.White, Message.Center);

            return;
        }

        CoinTracker.CheckExpiredCoins(now);
    }

    /// <summary>
    ///     Record when a player acquires a coin (for expiration tracking)
    /// </summary>
    public void RecordCoinAcquisition(GameState client, uint coinType) {
        if (!IsActive) return;
        CoinTracker.RecordCoinAcquisition(client, coinType);
    }

    /// <summary>
    ///     Check if PvP rules should apply (no PK points, no exp loss)
    ///     This should be checked by the combat/death handling system
    /// </summary>
    /// <remarks>
    ///     In the Proud Sea:
    ///     - No PK points are gained for kills
    ///     - No experience is lost on death
    /// </remarks>
    public bool ShouldApplyPvPRules(ushort mapId) {
        return IsActive && mapId == MapConstants.TreasureInTheBlue;
    }
}