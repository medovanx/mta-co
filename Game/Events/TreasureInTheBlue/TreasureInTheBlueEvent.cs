using System;
using System.Collections.Generic;
using System.Drawing;
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

    private const double CopperCoinDropRate = 0.25;
    private const double SilverCoinDropRate = 0.15;

    private static readonly Random Random = new();

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

        // Teleport all players out of event maps (ProudSea and PrizeCenter)
        foreach (var client in Program.Values) {
            var mapId = client.Entity.MapID;
            if (mapId != MapConstants.ProudSea && mapId != MapConstants.TreasureInTheBlue_PrizeCenter) continue;
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
                var mapId = client.Entity.MapID;
                if ((mapId != MapConstants.ProudSea && mapId != MapConstants.TreasureInTheBlue_PrizeCenter) ||
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
            // Golden Octopus - Always drops Gold Coin (100% chance)
            case MonsterConstants.GoldenOctopus:
                DropCoinOnGround(monster, ItemConstants.GoldCoin);
                return true; // Skip normal drop

            // Coins Stealer - Randomly drops Copper Coin (50% chance)
            case MonsterConstants.CoinsStealer: {
                if (Random.NextDouble() < CopperCoinDropRate) {
                    DropCoinOnGround(monster, ItemConstants.CopperCoin);
                }

                return true; // Skip normal drop
            }

            // Silver Octopus - Randomly drops Silver Coin (50% chance)
            case MonsterConstants.SilverOctopus:
                if (Random.NextDouble() < SilverCoinDropRate) {
                    DropCoinOnGround(monster, ItemConstants.SilverCoin);
                }

                return true; // Skip normal drop
        }

        return false; // Not handled by this event, keep normal drop
    }

    /// <summary>
    ///     Drop a coin on the ground at the monster's location
    /// </summary>
    /// <param name="monster">The monster that was killed</param>
    /// <param name="coinId">The coin item ID to drop</param>
    private static void DropCoinOnGround(MonsterInformation monster, uint coinId) {
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