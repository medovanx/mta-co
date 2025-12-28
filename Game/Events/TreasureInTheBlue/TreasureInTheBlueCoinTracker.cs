using System;
using System.Collections.Generic;
using System.Linq;
using MTA.Client;
using MTA.Database;
using static MTA.Game.Constants.Items.BasicItems;

namespace MTA.Game.Events.TreasureInTheBlue;

/// <summary>
///     Handles all coin-related logic for the Treasure in the Blue event:
///     - Coin acquisition tracking
///     - Coin expiration (60 minutes)
///     - Reward claiming and availability
///     - Remaining rewards tracking
/// </summary>
public class TreasureInTheBlueCoinTracker {
    // Limited rewards per round
    private int _copperCoinRewardsRemaining = 400;
    private int _silverCoinRewardsRemaining = 200;
    private int _goldCoinRewardsRemaining = 100;

    // Track coin acquisition times (player UID -> coin type -> acquisition time)
    private readonly Dictionary<uint, Dictionary<uint, DateTime>> _coinAcquisitionTimes = new();

    /// <summary>
    ///     Reset all coin tracking and reward counters (called when event starts)
    /// </summary>
    public void Reset() {
        _copperCoinRewardsRemaining = 400;
        _silverCoinRewardsRemaining = 200;
        _goldCoinRewardsRemaining = 100;
        _coinAcquisitionTimes.Clear();
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
        var playerId = client.Entity.UID;
        if (!_coinAcquisitionTimes.ContainsKey(playerId))
            _coinAcquisitionTimes[playerId] = new Dictionary<uint, DateTime>();

        _coinAcquisitionTimes[playerId][coinType] = DateTime.Now;
    }

    /// <summary>
    ///     Check for expired coins and remove them from player inventories
    /// </summary>
    public void CheckExpiredCoins(DateTime now) {
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
                var coinName = ConquerItemInformation.BaseInformations[coinType].Name;
                client.Send(
                    $"Your {coinName} has expired! Remember to exchange coins within 60 minutes!");
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
    ///     Get remaining rewards count for display
    /// </summary>
    public (int Copper, int Silver, int Gold) GetRemainingRewards() {
        return (_copperCoinRewardsRemaining, _silverCoinRewardsRemaining, _goldCoinRewardsRemaining);
    }
}