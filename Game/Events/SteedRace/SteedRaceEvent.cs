using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using MTA.Client;
using MTA.Database;
using MTA.Interfaces;
using MTA.Network.GamePackets;

namespace MTA.Game.Events.SteedRace;

/// <summary>
///     Steed Race Event - Horse racing tournament held every hour
/// </summary>
public class SteedRaceEvent : BaseEvent {
    private const int InvitationDurationSeconds = 30;
    private Map? _currentMap;
    private uint[]? _currentSettings;
    private bool _fiveSecondsLeft;
    private SobNpcSpawn? _gate;
    private bool _gateOpen;
    private DateTime? _gateOpened;
    private bool _invitationsExpired;
    private DateTime? _invitationsExpireDate;
    private bool _invitationsOut;
    private DateTime? _invitationsSentOut;
    private DateTime? _last5Seconds;
    private int _records;
    public override string EventId => "STEED_RACE";
    public override string EventName => "Steed Race";
    public override int? EventDurationMinutes => 31;
    public ushort CurrentMapId { get; private set; } = 1950;
    public bool CanJoin => IsActive && !_gateOpen;
    private ushort GateX { get; set; }
    private ushort GateY { get; set; }

    /// <summary>
    ///     Gets the number of seconds remaining until the race starts
    /// </summary>
    private int SecondsLeftUntilStart {
        get {
            if (!_invitationsSentOut.HasValue) return 0;
            return (int)(_invitationsSentOut.Value.AddMinutes(1) - DateTime.Now).TotalSeconds - 5;
        }
    }

    /// <inheritdoc />
    public override IEnumerable<EventSchedule> GetSchedules() {
        // Event runs at 15:00:00 on Monday through Friday
        yield return new EventSchedule(15, 0, 0, DayOfWeek.Monday);
        yield return new EventSchedule(15, 0, 0, DayOfWeek.Tuesday);
        yield return new EventSchedule(15, 0, 0, DayOfWeek.Wednesday);
        yield return new EventSchedule(15, 0, 0, DayOfWeek.Thursday);
        yield return new EventSchedule(15, 0, 0, DayOfWeek.Friday);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Creates a race map, sends invitations to all players, and initializes the race state.
    /// </remarks>
    public override void OnStart() {
        base.OnStart();

        CreateRaceMap();
        SendInvitations();
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Teleports all players out of the race map, resets the gate, and clears all event state.
    /// </remarks>
    public override void OnEnd() {
        base.OnEnd();

        // Teleport all players out
        if (_currentMap != null)
            foreach (var player in Program.Values)
                if (player.Entity.MapID == CurrentMapId)
                    Exit(player);

        // Reset gate
        if (_gate != null) {
            _gate.X = GateX;
            _gate.Y = GateY;
        }

        // Reset state
        Init();
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Handles invitation expiration, 5-second warning before race start, and gate opening timing.
    ///     Calls base.OnUpdate() to preserve automatic duration-based ending.
    /// </remarks>
    public override void OnUpdate(DateTime now) {
        // Check duration and end event if needed
        base.OnUpdate(now);

        if (!IsActive || !EventStartTime.HasValue) return;

        // Handle invitation expiration
        if (!_invitationsExpired && _invitationsSentOut.HasValue)
            if (now >= _invitationsExpireDate) {
                _invitationsExpired = true;
                _fiveSecondsLeft = false;
                _last5Seconds = _invitationsSentOut.Value.AddMinutes(1).AddSeconds(-12);
            }

        // Handle 5 seconds warning
        if (!_fiveSecondsLeft && _last5Seconds.HasValue)
            if (now > _last5Seconds) {
                _fiveSecondsLeft = true;
                SendData(Data.BeginSteedRace, uid: 1);
                _last5Seconds = _last5Seconds.Value.AddSeconds(5);
            }

        // Handle gate opening
        if (!_gateOpen && _last5Seconds.HasValue)
            if (now > _last5Seconds)
                OpenGate();
    }

    /// <summary>
    ///     Resets all event state variables to initial values
    /// </summary>
    private void Init() {
        _invitationsOut = _invitationsExpired = _gateOpen = _fiveSecondsLeft = false;
        _invitationsSentOut = null;
        _invitationsExpireDate = null;
        _last5Seconds = null;
        _gateOpened = null;
        _records = 0;
    }

    /// <summary>
    ///     Randomly selects and creates a race map from available race settings
    /// </summary>
    private void CreateRaceMap() {
        while (true) {
            var rand = Kernel.Random.Next(SteedRaceConstants.RaceSettings.Length);
            var mapId = (ushort)SteedRaceConstants.RaceSettings[rand][0];
            if (!MapsTable.MapInformations.ContainsKey(mapId)) continue;
            if (!Kernel.Maps.ContainsKey(mapId)) _ = new Map(mapId, "");

            if (!Kernel.Maps.ContainsKey(mapId)) continue;
            SetupRaceMap(mapId);
            break;
        }
    }

    /// <summary>
    ///     Sets up the selected race map - clears entities, creates gate, and generates potions
    /// </summary>
    private void SetupRaceMap(ushort mapId) {
        var index = -1;
        for (var i = 0; i < SteedRaceConstants.RaceSettings.Length; i++) {
            if (SteedRaceConstants.RaceSettings[i][0] != mapId) continue;
            index = i;
            break;
        }

        if (index == -1) return;

        _currentSettings = SteedRaceConstants.RaceSettings[index];
        CurrentMapId = mapId;
        _currentMap = Kernel.Maps[mapId];

        // Clear map
        foreach (var item in _currentMap.StaticEntities.Values)
            _currentMap.Floor[item.X, item.Y, MapObjectType.StaticEntity] = true;
        _currentMap.StaticEntities.Clear();
        _currentMap.Npcs.Clear();
        _currentMap.Entities.Clear();

        // Create gate
        _gate = new SobNpcSpawn {
            UID = 19501,
            X = (ushort)_currentSettings[6],
            Y = (ushort)_currentSettings[7],
            Mesh = (ushort)_currentSettings[8],
            ShowName = true,
            Name = " ",
            Type = Enums.NpcType.Furniture
        };
        GateX = _gate.X;
        GateY = _gate.Y;
        _currentMap.AddNpc(_gate);

        // Generate potions
        GeneratePotions();
    }

    /// <summary>
    ///     Generates 100 potions on the race map within specified coordinate limits
    /// </summary>
    private void GeneratePotions() {
        if (_currentSettings == null || _currentMap == null) return;

        uint count = 100;
        Tuple<ushort, ushort, int>[] limits = [
            new((ushort)_currentSettings[9], (ushort)_currentSettings[10],
                (int)_currentSettings[11]),
            new((ushort)_currentSettings[12], (ushort)_currentSettings[13],
                (int)_currentSettings[14]),
            new((ushort)_currentSettings[15], (ushort)_currentSettings[16],
                (int)_currentSettings[17])
        ];

        while (count > 0) {
            var x = (ushort)Kernel.Random.Next(0, _currentMap.Floor.Bounds.Width);
            var y = (ushort)Kernel.Random.Next(0, _currentMap.Floor.Bounds.Height);
            var valid = limits.Aggregate(false,
                (current, range) => current | (Kernel.GetDistance(x, y, range.Item1, range.Item2) < range.Item3));
            if (!valid) continue;
            if (!_currentMap.Floor[x, y, MapObjectType.StaticEntity] ||
                !_currentMap.Floor[x, y, MapObjectType.Player]) continue;
            var v = true;
            for (var i = 0; i < Map.XDir.Length; i++)
                if ((!_currentMap.Floor[x + Map.XDir[i], y + Map.YDir[i], MapObjectType.Player] ||
                     !_currentMap.Floor[x + Map.XDir[i], y + Map.YDir[i], MapObjectType.StaticEntity]) && v)
                    v = false;
            if (!v) continue;

            var item = new StaticEntity((uint)(x * 1000 + y), x, y, CurrentMapId);
            item.Pick();
            item.MapID = CurrentMapId;
            _currentMap.AddStaticEntity(item);
            count--;
        }
    }

    /// <summary>
    ///     Sends race invitation messages to all online players (except those in jail maps)
    /// </summary>
    private void SendInvitations() {
        _invitationsOut = true;
        _invitationsExpired = false;
        _invitationsSentOut = DateTime.Now;
        _invitationsExpireDate = _invitationsSentOut.Value.AddSeconds(InvitationDurationSeconds);

        foreach (var client in Program.Values) {
            if (client.Entity.MapID is >= 6000 and <= 6002) continue;
            client.MessageCancel = pClient => {
                pClient.Send(new Message(
                    "If you change your mind about joining the Steed Race you can see the Mount Trainer (Twin City qqq,www).",
                    Color.Red, Message.World));
            };
            client.MessageOK = pClient => {
                if (!IsActive) {
                    pClient.Send(new Message("The tournament has ended.", Color.Red, Message.Center));
                }
                else if (_invitationsExpired) {
                    pClient.Send(new Message("You lost your chance to join the steed race.", Color.Red,
                        Message.Center));
                }
                else if (_invitationsOut && !_invitationsExpired) {
                    if (!pClient.Spells.ContainsKey(7001)) {
                        pClient.Send("You need learn the riding skill!");
                    }
                    else {
                        if (!pClient.Equipment.Free(ConquerItem.Steed))
                            AddPlayer(pClient);
                        else
                            pClient.Send("You need to wear a horse first!");
                    }
                }
            };
            client.Send(new NpcReply(NpcReply.MessageBox, "Would you like to join the Steed Race?"));
            client.Send(new Data(true) { UID = client.Entity.UID, ID = Data.CountDown, dwParam = 60 });
        }

        BroadcastMessage(
            "The Horse Race is now open! You have one minute to sign up. Visit the Horse Race Manager in Twin City.",
            Color.White, Message.Center);
    }

    /// <summary>
    ///     Opens the race gate by removing it and setting the gate open flag
    /// </summary>
    private void OpenGate() {
        if (_gate == null) return;

        _gateOpened = DateTime.Now;
        _gateOpen = true;
        _gate.X = 0;
        _gate.Y = 0;
        Send(new Data(true) { UID = _gate.UID, ID = Data.RemoveEntity });
    }

    /// <summary>
    ///     Sends a packet to all players on the current race map
    /// </summary>
    private void Send(IPacket packet) {
        if (CurrentMapId != 0) Kernel.SendWorldMessage(packet, Program.Values, CurrentMapId);
    }

    /// <summary>
    ///     Sends a Data packet to all players on the current race map
    /// </summary>
    private void SendData(ushort id, uint value = 0, uint uid = 0) {
        Data? data = null;
        if (uid != 0)
            data = new Data(true) { UID = uid, ID = id, dwParam = value };

        foreach (var player in Program.Values)
            if (player.Entity.MapID == CurrentMapId) {
                data = uid == 0 ? new Data(true) { UID = player.Entity.UID, ID = id, dwParam = value } : data;
                if (data != null) player.Send(data);
            }
    }

    /// <summary>
    ///     Handles a player finishing the race - calculates rank, awards points, saves records, and teleports player out
    /// </summary>
    /// <summary>
    ///     Add a player to the race
    /// </summary>
    public void AddPlayer(GameState client) {
        var seconds = SecondsLeftUntilStart;
        if (seconds > 0)
            client.Send(new Data(true) { UID = client.Entity.UID, ID = Data.CountDown, dwParam = (uint)seconds });
        client.Entity.AddFlag(Update.Flags.Ride);
        var settings = GetCurrentSettings();
        if (settings != null) client.Entity.Teleport(CurrentMapId, (ushort)settings[1], (ushort)settings[2]);

        client.Send(new RaceRecord {
            Type = RaceRecordTypes.BestTime,
            Rank = (int)MapsTable.MapInformations[CurrentMapId].RaceRecord,
            dwParam = 1800000
        });
        client.Send(new RacePotion(true) { PotionType = Enums.RaceItemType.Null, Amount = 1 });
        client.Send(new RacePotion(true) { PotionType = Enums.RaceItemType.Null, Amount = 0 });
        client.Potions = new UsableRacePotion[5];
    }

    public void FinishRace(GameState client) {
        if (_gateOpened == null || _currentSettings == null) return;

        if (_records < 5) {
            var rank = Interlocked.Increment(ref _records);
            var span = DateTime.Now - _gateOpened.Value;
            var key = MapsTable.MapInformations[CurrentMapId];
            if (key.RaceRecord > span.TotalMilliseconds) {
                key.RaceRecord = (uint)span.TotalMilliseconds;
                MapsTable.SaveRecord(key);
            }

            var award = SteedRaceRewards.AwardPlayer((int)span.TotalMilliseconds, rank);
            client.RacePoints += (uint)award;
            Status(client, rank, (int)span.TotalMilliseconds, award);
            client.Send(new RaceRecord {
                Type = RaceRecordTypes.EndTime,
                Rank = rank,
                dwParam = (int)span.TotalMilliseconds,
                dwParam2 = award,
                Time = (int)span.TotalMilliseconds,
                Prize = award
            });
        }

        Exit(client);
    }

    /// <summary>
    ///     Broadcasts a race record status update to all players on the race map
    /// </summary>
    private void Status(GameState client, int rank, int time, int award) {
        var packet = new RaceRecord {
            Type = RaceRecordTypes.AddRecord,
            Rank = rank,
            Name = client.Entity.Name,
            Time = time,
            Prize = award
        };
        Send(packet);
    }

    /// <summary>
    ///     Teleports a player out of the race map back to their previous location (or Twin City as default)
    /// </summary>
    private void Exit(GameState client) {
        switch (client.Entity.PreviousMapID) {
            default: {
                client.Entity.Teleport(1002, 301, 278);
                break;
            }
            case 1000: {
                client.Entity.Teleport(1000, 500, 650);
                break;
            }
            case 1020: {
                client.Entity.Teleport(1020, 565, 562);
                break;
            }
            case 1011: {
                client.Entity.Teleport(1011, 188, 264);
                break;
            }
            case 1015: {
                client.Entity.Teleport(1015, 717, 571);
                break;
            }
        }
    }

    /// <summary>
    ///     Gets the current race map settings (coordinates, gate position, potion limits, etc.)
    /// </summary>
    private uint[]? GetCurrentSettings() {
        return _currentSettings;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Handles FinishSteedRace packets when the event is active and the player is on the race map.
    /// </remarks>
    public override bool HandlePacket(GameState client, byte[] packet, ushort packetId) {
        // Only handle FinishSteedRace packet when event is active
        if (packetId == Data.FinishSteedRace && IsActive)
            if (client.Entity.MapID == CurrentMapId) {
                FinishRace(client);
                return true; // Packet was handled
            }

        return false; // Let packet fall through to normal processing
    }
}