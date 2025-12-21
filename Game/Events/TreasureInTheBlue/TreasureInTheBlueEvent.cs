using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using static MTA.Game.MapConstants;

namespace MTA.Game.Events.TreasureInTheBlue;

/// <summary>
///     Treasure in the Blue Event - Collect coins from monsters in the Proud Sea and trade them for rewards
///     Based on official event: https://co.99.com/guide/quests/2012/blue_treasure.shtml
/// </summary>
public class TreasureInTheBlueEvent : BaseEvent {
    public const ushort CoinsMap = MapConstants.TREASURE_IN_THE_BLUE; // Proud Sea map
    public const ushort CoinsX = 60;
    public const ushort CoinsY = 60;
    private const ushort TradeMap = 7010;
    private const ushort TradeX = 59;
    private const ushort TradeY = 59;
    private const ushort TwinCityMap = 1002;
    private const ushort TwinCityX = 429;
    private const ushort TwinCityY = 378;

    // Coin item IDs
    private const uint GoldCoin = 711609;
    private const uint SilverCoin = 711610;
    private const uint CopperCoin = 711611;

    // Limited rewards per round
    private int _copperCoinRewardsRemaining = 400;
    private int _silverCoinRewardsRemaining = 200;
    private int _goldCoinRewardsRemaining = 100;

    // Track coin acquisition times (player UID -> coin type -> acquisition time)
    private readonly Dictionary<uint, Dictionary<uint, DateTime>> _coinAcquisitionTimes = new();

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

        // Reset reward counters
        _copperCoinRewardsRemaining = 400;
        _silverCoinRewardsRemaining = 200;
        _goldCoinRewardsRemaining = 100;

        // Clear coin tracking
        _coinAcquisitionTimes.Clear();

        AutoInviteAllPlayers("The Treasure in the Blue has begun! Would you like to join the Proud Sea?", TWIN_CITY,
            301, 529);

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
            if (client.Entity.MapID != CoinsMap) continue;
            client.Entity.BringToLife();
            client.Entity.Teleport(TradeMap, TradeX, TradeY);
            client.Send("Treasure in the Blue has Ended and You have teleported to tc");
        }

        // Clear coin tracking
        _coinAcquisitionTimes.Clear();
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
                if (client.Entity.MapID != CoinsMap || client.Account.State == AccountTable.AccountState.GM) continue;
                client.Entity.Teleport(TwinCityMap, TwinCityX, TwinCityY);
                client.Send("Treasure in the Blue has Ended and You have teleported to tc");
            }

            return;
        }

        // Check for expired coins (60 minutes)
        CheckExpiredCoins(now);

        // Check if all rewards are claimed - end event early
        if (_copperCoinRewardsRemaining <= 0 && _silverCoinRewardsRemaining <= 0 && _goldCoinRewardsRemaining <= 0) {
            BroadcastMessage(
                "All rewards have been claimed! The Treasure in the Blue event is ending early. The ship will return shortly.",
                Color.White, Message.Center);
            OnEnd();
        }
    }

    /// <summary>
    ///     Check if a reward can be claimed for the given coin type
    /// </summary>
    public bool CanClaimReward(uint coinType) {
        return coinType switch {
            CopperCoin => _copperCoinRewardsRemaining > 0,
            SilverCoin => _silverCoinRewardsRemaining > 0,
            GoldCoin => _goldCoinRewardsRemaining > 0,
            _ => false
        };
    }

    /// <summary>
    ///     Claim a reward for the given coin type. Returns true if successful.
    /// </summary>
    public bool ClaimReward(uint coinType) {
        return coinType switch {
            CopperCoin when _copperCoinRewardsRemaining > 0 => --_copperCoinRewardsRemaining >= 0,
            SilverCoin when _silverCoinRewardsRemaining > 0 => --_silverCoinRewardsRemaining >= 0,
            GoldCoin when _goldCoinRewardsRemaining > 0 => --_goldCoinRewardsRemaining >= 0,
            _ => false
        };
    }

    /// <summary>
    ///     Record when a player acquires a coin (for expiration tracking)
    /// </summary>
    public void RecordCoinAcquisition(GameState client, uint coinType) {
        if (!IsActive) return;

        var playerId = client.Entity.UID;
        if (!_coinAcquisitionTimes.ContainsKey(playerId))
            _coinAcquisitionTimes[playerId] = new Dictionary<uint, DateTime>();

        _coinAcquisitionTimes[playerId][coinType] = DateTime.Now;
    }

    /// <summary>
    ///     Check for expired coins and remove them from player inventories
    /// </summary>
    private void CheckExpiredCoins(DateTime now) {
        var expiredCoins = new List<(uint playerId, uint coinType)>();

        foreach (var (playerId, coinTimes) in _coinAcquisitionTimes) {
            foreach (var coinType in from coinEntry in coinTimes
                     let coinType = coinEntry.Key
                     let acquisitionTime = coinEntry.Value
                     let elapsed = now - acquisitionTime
                     where elapsed.TotalMinutes >= 60
                     select coinType) {
                expiredCoins.Add((playerId, coinType));

                // Find the client and remove the coin
                var client = Program.Values.FirstOrDefault(c => c.Entity.UID == playerId);
                if (client == null || !client.Inventory.Contains(coinType, 1)) continue;
                // Remove coin from inventory (remove 1 of the coin type)
                client.Inventory.Remove(coinType, 1);
                client.Send(
                    $"Your {GetCoinName(coinType)} has expired! Remember to exchange coins within 60 minutes!");
            }
        }

        // Remove expired coins from tracking
        foreach (var (playerId, coinType) in expiredCoins) {
            if (!_coinAcquisitionTimes.TryGetValue(playerId, out var time)) continue;
            time.Remove(coinType);
            if (_coinAcquisitionTimes[playerId].Count == 0) _coinAcquisitionTimes.Remove(playerId);
        }
    }

    /// <summary>
    ///     Get the name of a coin type
    /// </summary>
    private static string GetCoinName(uint coinType) {
        return coinType switch {
            CopperCoin => "Copper Coin",
            SilverCoin => "Silver Coin",
            GoldCoin => "Gold Coin",
            _ => "Coin"
        };
    }

    /// <summary>
    ///     Get remaining rewards count for display
    /// </summary>
    public (int Copper, int Silver, int Gold) GetRemainingRewards() {
        return (_copperCoinRewardsRemaining, _silverCoinRewardsRemaining, _goldCoinRewardsRemaining);
    }

    /// <summary>
    ///     Check if a player is in the Proud Sea (event map)
    ///     Used by combat system to apply PvP rules: no PK points, no exp loss on death
    /// </summary>
    private bool IsPlayerInEventMap(ushort mapId) {
        return IsActive && mapId == CoinsMap;
    }

    /// <summary>
    ///     Check if PvP rules should apply (no PK points, no exp loss)
    ///     This should be checked by the combat/death handling system
    /// </summary>
    /// <remarks>
    ///     According to the event guide, in the Proud Sea:
    ///     - No PK points are gained for kills
    ///     - No experience is lost on death
    ///     This method can be called by the combat system to determine if these rules apply.
    /// </remarks>
    public bool ShouldApplyPvPRules(ushort mapId) {
        return IsPlayerInEventMap(mapId);
    }
}