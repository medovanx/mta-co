using System;
using System.Collections.Generic;
using System.Drawing;
using MTA.Database;
using MTA.Network.GamePackets;
using static MTA.Game.MapConstants;

namespace MTA.Game.Events.TreasureHunt;

/// <summary>
///     Treasure Hunt Event - Collect coins from monsters and trade them for rewards
/// </summary>
public class TreasureHuntEvent : BaseEvent {
    public const ushort CoinsMap = 7015;
    public const ushort CoinsX = 60;
    public const ushort CoinsY = 60;
    private const ushort TradeMap = 7010;
    private const ushort TradeX = 59;
    private const ushort TradeY = 59;
    private const ushort TwinCityMap = 1002;
    private const ushort TwinCityX = 429;
    private const ushort TwinCityY = 378;

    // Coin item IDs
    public static readonly uint GoldCoin = 711609;
    public static readonly uint SilverCoin = 711610;
    public static readonly uint CopperCoin = 711611;
    public override string EventId => "TREASURE_HUNT";
    public override string EventName => "Treasure Hunt";

    public override int? EventDurationMinutes => 60;

    /// <inheritdoc />
    public override IEnumerable<EventSchedule> GetSchedules() {
        // Event runs at 16:00:00 every day
        yield return new EventSchedule(16, 0);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Starts the Treasure Hunt event, sends invitations, and broadcasts start message.
    /// </remarks>
    public override void OnStart() {
        base.OnStart();

        AutoInviteAllPlayers("The Treasure Hunt has begun! Would you like to join?", TWIN_CITY, 322, 269);

        BroadcastMessage("The Treasure Hunt has begun! Collect coins from monsters and trade them for rewards!",
            Color.White, Message.Center);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Ends the Treasure Hunt event and teleports all players out.
    /// </remarks>
    public override void OnEnd() {
        base.OnEnd();

        // Teleport all players out of coins map
        foreach (var client in Program.Values) {
            if (client.Entity.MapID != CoinsMap) continue;
            client.Entity.BringToLife();
            client.Entity.Teleport(TradeMap, TradeX, TradeY);
            client.Send("Treasure Hunt has Ended and You have teleported to tc");
        }
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Checks if event is still running and teleports players if not.
    /// </remarks>
    public override void OnUpdate(DateTime now) {
        // Check duration and end event if needed
        base.OnUpdate(now);

        // Additional check: teleport players if event ended
        if (IsActive) return;
        foreach (var client in Program.Values) {
            if (client.Entity.MapID != CoinsMap || client.Account.State == AccountTable.AccountState.GM) continue;
            client.Entity.Teleport(TwinCityMap, TwinCityX, TwinCityY);
            client.Send("Treasure Hunt has Ended and You have teleported to tc");
        }
    }
}