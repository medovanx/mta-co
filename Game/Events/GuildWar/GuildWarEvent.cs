using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Game.ConquerStructures.Society;
using MTA.Game.Constants;
using MTA.Interfaces;
using MTA.Network.GamePackets;
using static MTA.Game.Events.GuildWar.GuildWarConstants;

namespace MTA.Game.Events.GuildWar;

public class GuildWarEvent : BaseEvent {
    private bool _isFirstRound = true;
    private Guild? _poleKeeper;
    private string[] _scoreMessages = [];
    private SafeDictionary<uint, Guild> _scores = new(100);
    private bool _scoresChanged;
    private Time32 _scoreSendStamp;

    // Pole repair state
    private ulong _repairAllocatedFunds;
    private Time32 _lastRepairTime;
    private Guild? _repairingGuild;

    public override string EventId => "GUILD_WAR";
    public override string EventName => "Guild War";
    public override int? EventDurationMinutes => GuildWarConstants.EventDurationMinutes;

    /// <summary>
    ///     Current pole keeper (winning guild)
    /// </summary>
    public Guild? PoleKeeper => _poleKeeper;

    /// <summary>
    ///     West gate reference (for item handlers)
    /// </summary>
    public SobNpcSpawn? WestGate { get; private set; }

    /// <summary>
    ///     East gate reference (for item handlers)
    /// </summary>
    public SobNpcSpawn? EastGate { get; private set; }

    /// <summary>
    ///     Flame10th flag (for rune system) - controls if final rune upgrade is available
    /// </summary>
    public bool Flame10Th { get; set; } = false;

    /// <summary>
    ///     Pole reference (for name checks)
    /// </summary>
    public SobNpcSpawn? Pole { get; private set; }

    /// <summary>
    ///     Whether pole repair is currently active
    /// </summary>
    public bool IsRepairActive => _repairAllocatedFunds > 0 && _repairingGuild != null;

    /// <summary>
    ///     Amount of silver currently allocated for repair
    /// </summary>
    public ulong RepairAllocatedFunds => _repairAllocatedFunds;

    /// <summary>
    ///     Define when the event should start. Base class automatically handles triggering.
    /// </summary>
    public override IEnumerable<EventSchedule> GetSchedules() {
        // Event runs every Saturday at 20:00:00
        yield return new EventSchedule(20, 0, 0, DayOfWeek.Saturday);
    }

    /// <summary>
    ///     Initialize event state from database (called on server startup)
    ///     Restores pole keeper and pole name from the last war
    /// </summary>
    public void Initialize() {
        // Only restore if event is not active (between wars)
        if (IsActive) return;

        // Load latest history from database
        var latest = GuildWarHistoryTable.GetLatest();
        if (latest == null) return; // No previous war

        // Find the guild by ID
        if (!Kernel.Guilds.TryGetValue(latest.GuildId, out var guild)) return; // Guild not found

        // Restore pole keeper
        _poleKeeper = guild;

        // Restore pole name if pole exists
        Pole = Kernel.Maps.GetValueOrDefault(Maps.GuildWarMap)?.Npcs.GetValueOrDefault(PoleNpcId) as SobNpcSpawn;
        if (Pole != null) {
            Pole.Name = guild.Name;
            // Update pole in database to ensure consistency
            UpdatePole(Pole);
        }

        // Restore gates and set them to open (not attackable when event is not active)
        WestGate = Kernel.Maps.GetValueOrDefault(Maps.GuildWarMap)?.Npcs
            .GetValueOrDefault(WestGateNpcId) as SobNpcSpawn;
        EastGate =
            Kernel.Maps.GetValueOrDefault(Maps.GuildWarMap)?.Npcs.GetValueOrDefault(EastGateNpcId) as SobNpcSpawn;
        if (WestGate != null) {
            WestGate.Mesh = WestGateOpenMesh;
            Kernel.SendWorldMessage(WestGate, Program.Values, Maps.GuildWarMap);
        }

        if (EastGate != null) {
            EastGate.Mesh = EastGateOpenMesh;
            Kernel.SendWorldMessage(EastGate, Program.Values, Maps.GuildWarMap);
        }
    }

    /// <summary>
    ///     Called when the event starts
    /// </summary>
    public override void OnStart() {
        base.OnStart();

        Pole = Kernel.Maps.GetValueOrDefault(Maps.GuildWarMap)?.Npcs.GetValueOrDefault(PoleNpcId) as SobNpcSpawn;
        WestGate = Kernel.Maps.GetValueOrDefault(Maps.GuildWarMap)?.Npcs
            .GetValueOrDefault(WestGateNpcId) as SobNpcSpawn;
        EastGate =
            Kernel.Maps.GetValueOrDefault(Maps.GuildWarMap)?.Npcs.GetValueOrDefault(EastGateNpcId) as SobNpcSpawn;

        if (WestGate == null || EastGate == null || Pole == null) {
            BroadcastMessage("Guild War: Failed to initialize - NPCs not found!", Color.Red, Message.Center);
            return;
        }

        // Reset scores
        _scores = new SafeDictionary<uint, Guild>(100);
        _isFirstRound = true;
        _scoresChanged = false;
        _scoreMessages = [];
        _poleKeeper = null;

        // Reset pole name for new war (will be set when a guild wins a round)
        Pole.Name = "";
        Kernel.SendWorldMessage(Pole, Program.Values, Maps.GuildWarMap);
        UpdatePole(Pole);

        // Stop any active repair
        if (_repairAllocatedFunds > 0) StopRepair();

        // Reset all guild scores
        foreach (var guild in Kernel.Guilds.Values) guild.sWarScore = 0;

        // Reset all Guild War titles from all players (database-wide)
        // Remove Top Guild Leader title (flagtype 1) from all players
        new MySqlCommand(MySqlCommandType.DELETE)
            .Delete("status", "status", Update.Flags.TopGuildLeader)
            .And("flagtype", 1)
            .Execute();

        // Remove Top Deputy Leader title (flagtype 1) from all players
        new MySqlCommand(MySqlCommandType.DELETE)
            .Delete("status", "status", Update.Flags.TopDeputyLeader)
            .And("flagtype", 1)
            .Execute();

        // Remove Top Member Leader title (flagtype 0) from all players
        new MySqlCommand(MySqlCommandType.DELETE)
            .Delete("status", "status", (ulong)TitlePacket.Titles.membmerguild)
            .And("flagtype", 0)
            .Execute();

        // Also remove from online players so they're updated immediately
        foreach (var client in Kernel.GamePool.Values) {
            client.Entity.RemoveTopStatus(Update.Flags.TopGuildLeader, 1);
            client.Entity.RemoveTopStatus(Update.Flags.TopDeputyLeader, 1);
            client.Entity.RemoveTopStatus((ulong)TitlePacket.Titles.membmerguild);
        }

        // Reset gates (update mesh and HP)
        WestGate.Mesh = WestGateClosedMesh;
        EastGate.Mesh = EastGateClosedMesh;
        WestGate.Hitpoints = WestGate.MaxHitpoints;
        EastGate.Hitpoints = EastGate.MaxHitpoints;

        // Send gate updates to all players on the map (full NPC spawn packet for proper update)
        Kernel.SendWorldMessage(WestGate, Program.Values, Maps.GuildWarMap);
        Kernel.SendWorldMessage(EastGate, Program.Values, Maps.GuildWarMap);

        // Broadcast start message
        BroadcastMessage("Guild war has began!", Color.Red, Message.Center);

        // Send event alert to all players
        foreach (var client in Kernel.GamePool.Values) {
            client.Entity.DeputyLeader = 0;
            AutoInviteAllPlayers("The Guild War has begun! Would you like to join?",
                Maps.TwinCity,
                225, 237);
        }
    }

    /// <summary>
    ///     Called when the event ends
    /// </summary>
    public override void OnEnd() {
        base.OnEnd();

        if (_poleKeeper != null) {
            BroadcastMessage(
                $"The guild, {_poleKeeper.Name}, owned by {_poleKeeper.LeaderName} has won this guild war!",
                Color.White, Message.Center);
            GuildWarHistoryTable.Create(_poleKeeper, _poleKeeper.Leader.ID, _poleKeeper.LeaderName, DateTime.Now);
        }
        else {
            BroadcastMessage("Guild war has ended and there was no winner!", Color.Red, Message.Center);
        }

        // Stop repair if active
        if (_repairAllocatedFunds > 0) StopRepair();

        // Update pole in database
        if (Pole != null) UpdatePole(Pole);
    }

    /// <summary>
    ///     Called every second while the event is active
    /// </summary>
    public override void OnUpdate(DateTime now) {
        // Always call base.OnUpdate() first to preserve automatic duration checking
        base.OnUpdate(now);

        if (!IsActive) {
            // Stop repair if war is not active
            if (_repairAllocatedFunds > 0) StopRepair();
            return;
        }

        // Process pole repair
        ProcessRepair();

        // Send scores every 3 seconds
        if (Time32.Now <= _scoreSendStamp.AddSeconds(ScoreBroadcastIntervalSeconds)) return;
        _scoreSendStamp = Time32.Now;
        SendScores();
    }

    /// <summary>
    ///     Handle player movement - gate collision detection
    /// </summary>
    public override bool OnPlayerMovement(GameState client, ushort oldX, ushort oldY) {
        // Only handle movement on Guild War map
        if (client.Entity.MapID != Maps.GuildWarMap) return false;

        // Get gate references if not already set
        if (WestGate == null || EastGate == null) {
            WestGate =
                Kernel.Maps.GetValueOrDefault(Maps.GuildWarMap)?.Npcs.GetValueOrDefault(WestGateNpcId) as SobNpcSpawn;
            EastGate =
                Kernel.Maps.GetValueOrDefault(Maps.GuildWarMap)?.Npcs.GetValueOrDefault(EastGateNpcId) as SobNpcSpawn;
        }

        if (WestGate == null || EastGate == null) return false; // Gates not found

        // Only block movement if gates are closed (open gates allow passage)
        // Check right gate collision (only if closed)
        if (EastGate.Mesh == EastGateClosedMesh &&
            oldX >= EastGate.X && client.Entity.X <= EastGate.X &&
            client.Entity.Y < WestGate.Y) {
            client.Entity.X = oldX;
            client.Entity.Y = oldY;
            client.Disconnect();
            return true; // Movement blocked
        }

        // Check left gate collision (only if closed)
        if (WestGate.Mesh == WestGateClosedMesh &&
            oldY < WestGate.Y && client.Entity.Y > WestGate.Y &&
            client.Entity.X < EastGate.X) {
            client.Entity.X = oldX;
            client.Entity.Y = oldY;
            client.Disconnect();
            return true; // Movement blocked
        }

        return false; // Allow movement (gates are open or not in collision path)
    }

    /// <summary>
    ///     Handle player revive - teleport to Guild War Prison during active war
    /// </summary>
    public override bool OnPlayerRevive(GameState client, ushort currentMapId) {
        // Only handle revives during active Guild War
        if (!IsActive) return false;

        // Only teleport players who died on the Guild War map
        if (currentMapId != Maps.GuildWarMap) return false;

        // Teleport to Guild War Prison
        client.Entity.Teleport(Maps.GuildWarPrison, 31, 74);
        return true; // Event handled the revive
    }

    /// <summary>
    ///     Handle entity attacks - pole and gate damage in Guild War
    /// </summary>
    public override bool OnEntityAttacked(Entity attacker, IMapObject attacked, uint damage) {
        // Only handle attacks on the Guild War map
        if (attacker.MapID != Maps.GuildWarMap) return false;

        // Only handle attacks on SobNpcSpawn (pole or gates)
        if (attacked is not SobNpcSpawn npc) return false;

        // Prevent gate damage when event is not active (gates should be open and not attackable)
        if (!IsActive) {
            if (npc.UID == WestGateNpcId || npc.UID == EastGateNpcId) {
                return true; // Block gate damage when event is not active
            }

            // For pole, check database history to prevent pole keeper from attacking when event is not active
            if (npc.UID == PoleNpcId && attacker.Owner.Guild != null) {
                var latest = GuildWarHistoryTable.GetLatest();
                if (latest != null && latest.GuildId == attacker.Owner.Guild.ID) {
                    return true; // Block pole damage for pole keeper when event is not active
                }
            }

            return false; // Allow normal processing for other cases when event is not active
        }

        switch (npc.UID) {
            // Handle pole damage
            // If pole keeper guild is attacking, skip damage
            // During active war, only check _poleKeeper (database check removed from active war path)
            case PoleNpcId when attacker.Owner.Guild != null && _poleKeeper == attacker.Owner.Guild:
                return true; // Skip normal damage processing
            // From here on, event is active
            case PoleNpcId: {
                var actualDamage = damage;

                // Check if pole keeper has funds
                if (_poleKeeper is { SilverFund: > 0 }) {
                    // Calculate reward
                    var reward = (ulong)damage * PoleAttackRewardPerDamage;

                    // Ensure reward doesn't exceed available funds
                    reward = Math.Min(reward, _poleKeeper.SilverFund);

                    // Give money to attacker
                    attacker.Owner.Entity.Money += reward;

                    // Deduct from pole keeper's fund
                    _poleKeeper.SilverFund -= reward;
                    GuildTable.SaveFunds(_poleKeeper);

                    // Send guild update to all online members of pole keeper guild
                    foreach (var member in _poleKeeper.Members.Values.Where(member => member.IsOnline)) {
                        if (member.Client != null) {
                            _poleKeeper.SendGuild(member.Client);
                        }
                    }
                }
                else {
                    // Pole keeper has no funds - apply 10x damage
                    actualDamage = damage * PoleDamageMultiplierWhenFundsEmpty;
                }

                // Apply damage to pole
                if (npc.Hitpoints <= actualDamage)
                    npc.Hitpoints = 0;
                else
                    npc.Hitpoints -= actualDamage;

                // Add score for the attacking guild (use original damage for scoring)
                AddScore(damage, attacker.Owner.Guild);

                // Event handled this attack
                return true;
            }
            // Handle gate damage
            case WestGateNpcId:
            case EastGateNpcId: {
                // If pole keeper guild is attacking their own gates, skip damage
                // During active war, only check _poleKeeper (database check removed from active war path)
                if (attacker.Owner.Guild != null && _poleKeeper == attacker.Owner.Guild) {
                    return true; // Skip normal damage processing
                }

                // Apply damage to gate
                if (npc.Hitpoints <= damage) {
                    npc.Hitpoints = 0;
                    npc.Mesh = npc.UID switch {
                        // Set broken mesh when gate is destroyed
                        WestGateNpcId => WestGateBrokenMesh,
                        EastGateNpcId => EastGateBrokenMesh,
                        _ => npc.Mesh
                    };
                }
                else {
                    npc.Hitpoints -= damage;
                }

                // Broadcast gate update
                Kernel.SendWorldMessage(npc, Program.Values, Maps.GuildWarMap);

                // Event handled this attack
                return true;
            }
            default:
                return false; // Not a pole or gate, let normal damage processing handle it
        }
    }

    /// <summary>
    ///     Process pole repair - restore HP every 10 seconds
    /// </summary>
    private void ProcessRepair() {
        if (_repairAllocatedFunds == 0 || _repairingGuild == null || Pole == null) return;
        if (Pole.Hitpoints >= Pole.MaxHitpoints) {
            // Pole is full, stop repair
            StopRepair();
            return;
        }

        // Check if 10 seconds have passed
        if (Time32.Now <= _lastRepairTime.AddSeconds(PoleRepairIntervalSeconds)) return;

        // Calculate HP needed
        var hpNeeded = Pole.MaxHitpoints - Pole.Hitpoints;
        if (hpNeeded == 0) {
            StopRepair();
            return;
        }

        // Calculate maximum HP we can restore with available funds
        var maxHpFromFunds = (uint)(_repairAllocatedFunds * PoleRepairSilverPerHp);

        // Calculate HP to restore (10,000 HP, remaining needed, or what we can afford)
        var hpToRestore = Math.Min(PoleRepairHpPerInterval, Math.Min(hpNeeded, maxHpFromFunds));

        // Calculate actual cost based on HP to restore (10 HP = 1 Silver)
        var silverCost = hpToRestore / PoleRepairSilverPerHp;
        if (silverCost == 0 && hpToRestore > 0) silverCost = 1; // Minimum 1 silver if restoring any HP

        // Ensure we don't exceed allocated funds
        if (silverCost > _repairAllocatedFunds) {
            silverCost = (uint)_repairAllocatedFunds;
            hpToRestore = silverCost * PoleRepairSilverPerHp;
            if (hpToRestore > hpNeeded) hpToRestore = hpNeeded;
        }

        // Deduct cost from allocated funds (funds were already deducted at start)
        _repairAllocatedFunds -= silverCost;

        // Restore HP (add to current HP)
        var newHp = Pole.Hitpoints + hpToRestore;
        Pole.Hitpoints = Math.Min(Pole.MaxHitpoints, newHp);

        // Update guild fund in database (since we already deducted at start, we just need to update display)
        // The actual fund was already deducted, but we need to refresh the display
        // Note: The guild fund was already deducted when repair started, so we don't deduct again here
        // We just update the display to show the current state

        // Update last repair time
        _lastRepairTime = Time32.Now;

        // Broadcast pole update
        Kernel.SendWorldMessage(Pole, Program.Values, Maps.GuildWarMap);

        // Update pole in database
        UpdatePole(Pole);

        // Send guild update to all online members to refresh fund display
        if (_repairingGuild != null) {
            foreach (var member in _repairingGuild.Members.Values.Where(member => member.IsOnline)) {
                if (member.Client != null) {
                    _repairingGuild.SendGuild(member.Client);
                }
            }
        }

        // If funds exhausted or pole is full, stop repair
        if (_repairAllocatedFunds == 0 || Pole.Hitpoints >= Pole.MaxHitpoints) {
            StopRepair();
        }
    }

    /// <summary>
    ///     Add score to a guild when pole is damaged
    /// </summary>
    private void AddScore(uint addScore, Guild? guild) {
        if (!IsActive || guild == null || Pole == null) return;

        guild.sWarScore += addScore;
        _scoresChanged = true;
        if (!_scores.ContainsKey(guild.ID)) _scores.Add(guild.ID, guild);

        // Check if pole is destroyed
        if ((int)Pole.Hitpoints <= 0) FinishRound();
    }

    /// <summary>
    ///     Finish a round when pole is destroyed
    /// </summary>
    private void FinishRound() {
        if (Pole == null || WestGate == null || EastGate == null) return;

        // Update previous round winner stats
        if (_poleKeeper != null && !_isFirstRound) {
            if (_poleKeeper.Wins == 0)
                _poleKeeper.Losts++;
            else
                _poleKeeper.Wins--;
            GuildTable.UpdateGuildWarStats(_poleKeeper);
        }

        _isFirstRound = false;

        // Sort scores and determine winner
        var previousPoleKeeper = _poleKeeper;
        SortScores(out _poleKeeper);

        // Stop repair if pole keeper changed
        if (previousPoleKeeper != _poleKeeper && _repairAllocatedFunds > 0) {
            StopRepair();
        }

        // Give 10% bonus from previous pole keeper's fund to new pole keeper
        if (previousPoleKeeper != null && _poleKeeper != null && previousPoleKeeper != _poleKeeper &&
            previousPoleKeeper.SilverFund > 0) {
            // Calculate 10% bonus using integer division to avoid precision issues
            var bonus = previousPoleKeeper.SilverFund / 10; // 10% of fund

            // Ensure we don't transfer more than available
            if (bonus > 0 && bonus <= previousPoleKeeper.SilverFund) {
                // Deduct bonus from previous pole keeper's fund first
                previousPoleKeeper.SilverFund -= bonus;
                GuildTable.SaveFunds(previousPoleKeeper);

                // Add bonus to new pole keeper's fund
                _poleKeeper.SilverFund += bonus;
                GuildTable.SaveFunds(_poleKeeper);

                // Send guild updates to all online members of both guilds
                foreach (var member in _poleKeeper.Members.Values.Where(member => member.IsOnline)) {
                    if (member.Client != null) {
                        _poleKeeper.SendGuild(member.Client);
                    }
                }

                foreach (var member in previousPoleKeeper.Members.Values.Where(member => member.IsOnline)) {
                    if (member.Client != null) {
                        previousPoleKeeper.SendGuild(member.Client);
                    }
                }

                // Broadcast bonus message
                BroadcastMessage(
                    $"{_poleKeeper.Name} has received {bonus:N0} silver (10% of {previousPoleKeeper.Name}'s guild fund) for knocking down the pole!",
                    Color.Gold, Message.Center);
            }
        }

        if (_poleKeeper != null) {
            BroadcastMessage(
                $"The guild, {_poleKeeper.Name}, owned by {_poleKeeper.LeaderName} has won this guild war round!",
                Color.Red, Message.Center);

            // Update winner stats
            if (_poleKeeper.Losts == 0)
                _poleKeeper.Wins++;
            else
                _poleKeeper.Losts--;
            GuildTable.UpdateGuildWarStats(_poleKeeper);

            // Update pole name
            Pole.Name = _poleKeeper.Name;
        }

        // Reset pole HP
        Pole.Hitpoints = Pole.MaxHitpoints;
        Kernel.SendWorldMessage(Pole, Program.Values, Maps.GuildWarMap);

        // Reset for next round
        Reset();
    }

    /// <summary>
    ///     Reset gates and scores for next round
    /// </summary>
    private void Reset() {
        if (WestGate == null || EastGate == null || Pole == null) return;

        _scores = new SafeDictionary<uint, Guild>(100);

        // Reset gate meshes
        WestGate.Mesh = WestGateClosedMesh;
        EastGate.Mesh = EastGateClosedMesh;

        // Reset gate and pole HP
        WestGate.Hitpoints = WestGate.MaxHitpoints;
        EastGate.Hitpoints = EastGate.MaxHitpoints;
        Pole.Hitpoints = Pole.MaxHitpoints;

        // Send gate updates
        var upd = new Update(true) {
            UID = WestGate.UID
        };
        upd.Append(Update.Mesh, WestGate.Mesh);
        upd.Append(Update.Hitpoints, WestGate.Hitpoints);
        Kernel.SendWorldMessage(upd, Program.Values, Maps.GuildWarMap);
        upd.Clear();
        upd.UID = EastGate.UID;
        upd.Append(Update.Mesh, EastGate.Mesh);
        upd.Append(Update.Hitpoints, EastGate.Hitpoints);
        Kernel.SendWorldMessage(upd, Program.Values, Maps.GuildWarMap);

        // Reset all guild scores
        foreach (var guild in Kernel.Guilds.Values) guild.sWarScore = 0;
    }

    /// <summary>
    ///     Send score messages to players
    /// </summary>
    private void SendScores() {
        if (_scores.Count == 0) return;

        if (_scoresChanged) SortScores(out _);

        for (var c = 0; c < _scoreMessages.Length; c++) {
            var msg = new Message(_scoreMessages[c], Color.Red,
                c == 0 ? Message.FirstRightCorner : Message.ContinueRightCorner);
            Kernel.SendWorldMessage(msg, Program.Values, Maps.GuildWarMap);
            Kernel.SendWorldMessage(msg, Program.Values, Maps.GuildWarPrison);
        }
    }

    /// <summary>
    ///     Sort scores and determine winner
    /// </summary>
    private void SortScores(out Guild? winner) {
        winner = null;
        var ret = new List<string>();

        var place = 0;
        foreach (var guild in _scores.Values.OrderByDescending(p => p.sWarScore)) {
            if (place == 0) winner = guild;
            var str = $"No  {place + 1}: {guild.Name}({guild.sWarScore})";
            ret.Add(str);
            place++;
            if (place >= 4) break;
        }

        _scoresChanged = false;
        _scoreMessages = ret.ToArray();
    }

    /// <summary>
    ///     Update pole in database
    /// </summary>
    private static void UpdatePole(SobNpcSpawn pole) {
        new MySqlCommand(MySqlCommandType.UPDATE)
            .Update("sobnpcs")
            .Set("name", pole.Name)
            .Set("life", pole.Hitpoints)
            .Where("id", pole.UID)
            .Execute();
    }

    /// <summary>
    ///     Helper method to get the active Guild War event instance
    /// </summary>
    public static GuildWarEvent? GetActiveEvent() {
        return EventScheduler.GetEvent("GUILD_WAR") as GuildWarEvent;
    }

    /// <summary>
    ///     Start pole repair using guild fund
    /// </summary>
    public bool StartRepair(Guild guild, ulong silverAmount) {
        if (Pole == null || silverAmount == 0) return false;
        if (Pole.Hitpoints >= Pole.MaxHitpoints) return false;
        if (guild.SilverFund < silverAmount) return false;
        if (_repairAllocatedFunds > 0) return false; // Already repairing

        // Deduct funds from guild (deduct all at start to reserve funds)
        guild.SilverFund -= silverAmount;
        GuildTable.SaveFunds(guild);

        // Send guild update to all online members immediately
        foreach (var member in guild.Members.Values.Where(member => member.IsOnline)) {
            if (member.Client != null) {
                guild.SendGuild(member.Client);
            }
        }

        // Start repair
        _repairAllocatedFunds = silverAmount;
        _repairingGuild = guild;
        _lastRepairTime = Time32.Now;

        return true;
    }

    /// <summary>
    ///     Stop pole repair and refund remaining funds
    /// </summary>
    public ulong StopRepair(Guild? guild = null) {
        if (_repairAllocatedFunds == 0 || _repairingGuild == null) return 0;

        // Only allow the repairing guild to stop, or if no guild specified, allow it
        if (guild != null && _repairingGuild != guild) return 0;

        var refunded = _repairAllocatedFunds;
        var guildToUpdate = _repairingGuild;

        _repairAllocatedFunds = 0;
        _repairingGuild = null;

        if (guildToUpdate == null) return refunded;
        guildToUpdate.SilverFund += refunded;
        GuildTable.SaveFunds(guildToUpdate);

        // Send guild update to all online members
        foreach (var member in guildToUpdate.Members.Values.Where(member => member.IsOnline)) {
            if (member.Client != null) {
                guildToUpdate.SendGuild(member.Client);
            }
        }

        return refunded;
    }
}