using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using static MTA.Kernel;

namespace MTA.Game.Events.TreasureInTheBlue;

/// <summary>
///     Treasure in the Blue Event - Collect coins from monsters in the Proud Sea and trade them for rewards
///     Based on official event: https://co.99.com/guide/quests/2012/blue_treasure.shtml
/// </summary>
public class TreasureInTheBlueEvent : BaseEvent {
    public readonly TreasureInTheBlueCoinTracker CoinTracker = new();

    private const double CopperCoinDropRate = 0.35;
    private const double SilverCoinDropRate = 0.25;
    private const double GoldenOctopusDropRate = 0.25;

    private static readonly Random Random = new();

    // Golden Octopus drop rewards with weighted odds (Gold Coin has the lowest probability)
    private static readonly (uint itemId, double weight)[] GoldenOctopusRewards = [
        (ItemConstants.LotteryTicket, 0.30),
        (ItemConstants.QuestChanceB, 0.30),
        (ItemConstants.PenitenceAmulet, 0.50),
        (ItemConstants.DragonSoulTicket, 0.20),
        (ItemConstants.HorseRacingPointsPack5K, 0.15),
        (ItemConstants.ExpBall, 0.18),
        (ItemConstants.EnduranceBook, 0.15),
        (ItemConstants.Meteor, 0.30),
        (ItemConstants.GoldCoin, 0.10)
    ];

    // Boss spawn tracking
    private uint? _lastBossUid;
    private bool _lastBossWasAlive;
    private bool _bossInitialSpawnDone;
    private const int BossInitialSpawnTimeMinutes = 5;

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
        _bossInitialSpawnDone = false;

        AutoInviteAllPlayers("The Treasure in the Blue has begun! Would you like to join the Proud Sea?",
            MapConstants.TwinCity,
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

        RemoveEventCoinsFromAllPlayers();

        TeleportPlayersFromMaps([MapConstants.ProudSea, MapConstants.TreasureInTheBlue_PrizeCenter],
            MapConstants.TwinCity, 304, 287);

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

        // Spawn Blackbeard 5 minutes after event starts
        if (!_bossInitialSpawnDone && EventStartTime.HasValue) {
            var elapsed = now - EventStartTime.Value;
            if (elapsed.TotalMinutes >= BossInitialSpawnTimeMinutes) {
                EnsureMonsterSpawn([MapConstants.ProudSea], MonsterConstants.Blackbeard, respawnTimeSeconds: 900,
                    x: 129, y: 178, isBoss: true);
                _bossInitialSpawnDone = true;
            }
        }

        CheckBossSpawn();
    }

    /// <summary>
    ///     Called when event is inactive to clean up (remove coins from players who log in after event ends)
    ///     This is called by EventScheduler even when the event is inactive
    /// </summary>
    public override void OnUpdateWhenInactive(DateTime now) {
        RemoveEventCoinsFromAllPlayers();
        TeleportPlayersFromMaps([MapConstants.ProudSea, MapConstants.TreasureInTheBlue_PrizeCenter],
            MapConstants.TwinCity, 304, 287);
    }

    /// <summary>
    ///     Remove all event coins from all player inventories (called when event ends or is inactive)
    /// </summary>
    private static void RemoveEventCoinsFromAllPlayers() {
        foreach (var client in Program.Values) {
            while (client.Inventory.Contains(ItemConstants.CopperCoin, 1)) {
                client.Inventory.Remove(ItemConstants.CopperCoin, 1);
            }

            while (client.Inventory.Contains(ItemConstants.SilverCoin, 1)) {
                client.Inventory.Remove(ItemConstants.SilverCoin, 1);
            }

            while (client.Inventory.Contains(ItemConstants.GoldCoin, 1)) {
                client.Inventory.Remove(ItemConstants.GoldCoin, 1);
            }
        }
    }

    /// <summary>
    ///     Check if the Blackbeard boss has spawned and notify players in the map
    /// </summary>
    private void CheckBossSpawn() {
        if (!Maps.TryGetValue(MapConstants.ProudSea, out var map)) return;

        // Find the boss monster (Blackbeard)
        var boss = map.Entities.Values.FirstOrDefault(entity => entity.MonsterInfo.ID == MonsterConstants.Blackbeard);

        if (boss == null) {
            _lastBossWasAlive = false;
            _lastBossUid = null;
            return;
        }

        // Check if boss just spawned (was dead, now alive)
        var isAlive = !boss.Dead;
        if (isAlive && !_lastBossWasAlive && _lastBossUid != boss.UID) {
            NotifyBossSpawn(boss.X, boss.Y);
            _lastBossUid = boss.UID;
        }

        _lastBossWasAlive = isAlive;
    }

    /// <summary>
    ///     Notify all players in ProudSea map that the boss has spawned
    /// </summary>
    private static void NotifyBossSpawn(ushort bossX, ushort bossY) {
        foreach (var client in Program.Values) {
            if (client.Entity.MapID != MapConstants.ProudSea) continue;

            client.MessageBox(
                "BLACKBEARD HAS APPEARED! Defeat the pirate boss to claim Gold Coins! Teleport to battle?",
                p => { p.Entity.Teleport(MapConstants.ProudSea, bossX, bossY); },
                null,
                30 // 30 second timeout
            );
        }
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
        return IsActive && mapId == MapConstants.ProudSea;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Handle coin drops when event monsters are killed
    ///     Returns true to skip normal drop, false to keep it
    /// </remarks>
    public override bool OnMonsterKilled(MonsterInformation monster, Entity killer) {
        if (!IsActive) return false;
        if (monster.Owner.MapID != MapConstants.ProudSea) return false;

        var npctype = monster.ID;

        switch (npctype) {
            // Golden Octopus - Randomly drops weighted random reward
            case MonsterConstants.GoldenOctopus: {
                if (!(Random.NextDouble() < GoldenOctopusDropRate)) return true; // Skip drop
                var rewardItemId = TreasureInTheBlueHelpers.SelectWeightedReward(GoldenOctopusRewards);
                DropItemOnGround(monster, rewardItemId);

                return true; // Skip normal drop
            }

            // Coins Stealer - Randomly drops Copper Coin (50% chance)
            case MonsterConstants.CoinsStealer: {
                if (Random.NextDouble() < CopperCoinDropRate) {
                    DropItemOnGround(monster, ItemConstants.CopperCoin);
                }

                return true; // Skip normal drop
            }

            // Silver Octopus - Randomly drops Silver Coin (50% chance)
            case MonsterConstants.SilverOctopus:
                if (Random.NextDouble() < SilverCoinDropRate) {
                    DropItemOnGround(monster, ItemConstants.SilverCoin);
                }

                return true; // Skip normal drop

            // Blackbeard - Always drops 4-9 Gold Coins (random)
            case MonsterConstants.Blackbeard: {
                var coinCount = Random.Next(4, 10);
                for (var i = 0; i < coinCount; i++) {
                    DropItemOnGround(monster, ItemConstants.GoldCoin);
                }

                return true; // Skip normal drop
            }
        }

        return false; // Not handled by this event, keep normal drop
    }


    /// <summary>
    ///     Drop an item on the ground at the monster's location
    /// </summary>
    /// <param name="monster">The monster that was killed</param>
    /// <param name="coinId">The item ID to drop</param>
    private static void DropItemOnGround(MonsterInformation monster, uint coinId) {
        if (!ConquerItemInformation.BaseInformations.TryGetValue(coinId, out var infos)) return;
        if (!Maps.TryGetValue(monster.Owner.MapID, out var map)) return;
        var x = monster.Owner.X;
        var y = monster.Owner.Y;

        // Find valid drop coordinates
        if (!map.SelectCoordonates(ref x, ref y)) return;

        // Create floor item
        var floorItem = new FloorItem(true) {
            Item = new ConquerItem(true) {
                Color = (Enums.Color)Kernel.Random.Next(4, 8),
                ID = coinId,
                MaximDurability = infos.Durability,
                Durability = infos.Durability,
                MobDropped = true
            },
            ValueType = FloorItem.FloorValueType.Item,
            ItemID = coinId,
            MapID = monster.Owner.MapID,
            MapObjType = MapObjectType.Item,
            X = x,
            Y = y,
            Type = FloorItem.Drop,
            OnFloor = Time32.Now
        };
        floorItem.ItemColor = floorItem.Item.Color;
        floorItem.UID = FloorItem.FloorUID.Next;
        while (map.Npcs.ContainsKey(floorItem.UID))
            floorItem.UID = FloorItem.FloorUID.Next;

        // Add to map and notify nearby players
        map.AddFloorItem(floorItem);
        monster.SendScreenSpawn(floorItem);
    }
}