# Event System

Centralized scheduled event management framework. All event logic, timing, and rewards are organized in dedicated folders.

## Architecture

### Core Components

| Component | Purpose |
|-----------|---------|
| `IEvent.cs` | Interface defining event contract |
| `BaseEvent.cs` | Base class with common functionality (GM overrides, helpers, automatic scheduling) |
| `EventScheduler.cs` | Central manager - registers events, handles timing, monster kills |

### Architecture Flow

```
GetSchedules() → Define when event starts (once)
    ↓
Base.ShouldTrigger() → Automatically checks schedules
    ↓
OnStart() → Event begins
    ↓
OnUpdate() → base.OnUpdate() checks EventDurationMinutes
    ↓
OnEnd() → Event ends (called automatically if duration exceeded)
```

**Key Benefits:**
- **No Redundancy**: Define schedules once, use automatically
- **Automatic Ending**: Set `EventDurationMinutes`, base class handles it
- **Self-Contained**: Events handle their own packets via `HandlePacket()`

### Folder Structure

```
Game/Events/
├── IEvent.cs              # Event interface
├── BaseEvent.cs           # Base class
├── EventScheduler.cs      # Central manager
└── [EventName]/           # Each event gets its own folder
    ├── [EventName]Event.cs    # Main event logic
    └── [EventName]Rewards.cs  # Rewards (optional)
```

## Quick Start: Adding a New Event

### 1. Create Event Folder
Create `Game/Events/MyNewEvent/`

### 2. Create Event Class
```csharp
namespace MTA.Game.Events.MyNewEvent
{
    public class MyNewEvent : BaseEvent
    {
        public override string EventId => "MY_NEW_EVENT";
        public override string EventName => "My New Event";

        private const int EVENT_START_HOUR = 12;
        private const int EVENT_DURATION_MINUTES = 60;

        /// <summary>
        /// Event duration in minutes. Set to null for events without automatic duration-based ending.
        /// </summary>
        protected override int? EventDurationMinutes => EVENT_DURATION_MINUTES;

        /// <summary>
        /// Define when the event should start. Base class automatically handles triggering.
        /// </summary>
        public override IEnumerable<EventSchedule> GetSchedules()
        {
            // Event runs every day at 12:00:00
            yield return new EventSchedule(EVENT_START_HOUR, 0, 0);
        }

        // No need to override ShouldTrigger() - base class automatically checks GetSchedules()

        public override void OnStart()
        {
            base.OnStart();
            BroadcastMessage("Event started!", Color.White, Message.System);
            EnsureMonsterRespawns([MapConstants.MY_MAP], ["MonsterName"], 30);
        }

        public override void OnEnd()
        {
            base.OnEnd();
            BroadcastMessage("Event ended!", Color.White, Message.System);
            TeleportPlayersFromMaps([MapConstants.MY_MAP], MapConstants.TWIN_CITY, 300, 280);
        }

        public override void OnUpdate(DateTime now)
        {
            // Always call base.OnUpdate() first to preserve automatic duration checking
            base.OnUpdate(now);
            
            if (!IsActive || !EventStartTime.HasValue) return;

            var elapsed = now - EventStartTime.Value;
            var remaining = EVENT_DURATION_MINUTES - elapsed.TotalMinutes;

            // Warnings
            if (remaining <= 10 && remaining > 9.9)
                BroadcastMessage("Ending in 10 minutes!", Color.White, Message.System);
            
            // Note: Duration-based ending is handled automatically by base.OnUpdate()
        }

        // Optional: Handle monster kills
        public override void OnMonsterKilled(Database.MonsterInformation monster, Game.Entity killer)
        {
            if (!IsActive || monster.Name != "SpecialMonster") return;
            if (killer.Owner != null)
                MyNewEventRewards.OnMonsterKilled(killer.Owner, monster.Name, monster.Owner.MapID);
        }

        // Optional: Skip normal drops
        public override bool ShouldSkipNormalDrop(Database.MonsterInformation monster, ushort mapId)
        {
            return IsActive && mapId == MapConstants.MY_MAP && monster.Name == "SpecialMonster";
        }

        // Optional: Handle packets
        public override bool HandlePacket(Client.GameState client, byte[] packet, ushort packetId)
        {
            if (packetId == Data.MyCustomPacket && IsActive)
            {
                // Handle the packet
                return true; // Packet was handled
            }
            return false; // Let packet fall through to normal processing
        }
    }
}
```

### 3. Create Rewards Class (Optional)
```csharp
namespace MTA.Game.Events.MyNewEvent
{
    public class MyNewEventRewards
    {
        public static void OnMonsterKilled(GameState client, string monsterName, ushort mapId)
        {
            if (monsterName == "SpecialMonster")
            {
                client.Entity.ConquerPoints += 1000;
                client.Send(new Message("You received 1000 CPs!", Color.White, Message.TopLeft));
            }
        }
    }
}
```

### 4. Register Event
In `EventScheduler.Initialize()`:
```csharp
RegisterEvent(new MTA.Game.Events.MyNewEvent.MyNewEvent());
```

### 5. Add to Project File
```xml
<Compile Include="Game\Events\MyNewEvent\MyNewEvent.cs" />
<Compile Include="Game\Events\MyNewEvent\MyNewEventRewards.cs" />
```

## Event Scheduling

**Key Principle:** Define schedules once in `GetSchedules()`. The base `ShouldTrigger()` automatically checks these schedules - no need to override it!

```csharp
// Every day at 14:00
yield return new EventSchedule(14, 0, 0);

// Every Monday at 20:00
yield return new EventSchedule(20, 0, 0, DayOfWeek.Monday);

// Every day at 12:30:45
yield return new EventSchedule(12, 30, 45);

// Multiple times per day
yield return new EventSchedule(14, 0, 0);  // 2:00 PM
yield return new EventSchedule(20, 0, 0);  // 8:00 PM

// Weekdays only (Monday-Friday)
yield return new EventSchedule(15, 0, 0, DayOfWeek.Monday);
yield return new EventSchedule(15, 0, 0, DayOfWeek.Tuesday);
yield return new EventSchedule(15, 0, 0, DayOfWeek.Wednesday);
yield return new EventSchedule(15, 0, 0, DayOfWeek.Thursday);
yield return new EventSchedule(15, 0, 0, DayOfWeek.Friday);
```

## Duration-Based Ending

Events can automatically end after a specified duration using the `EventDurationMinutes` property:

```csharp
/// <summary>
/// Event duration in minutes. Set to null for events without automatic ending.
/// </summary>
protected override int? EventDurationMinutes => 30; // Event ends 30 minutes after start
```

**How it works:**
- Base `OnUpdate()` automatically checks duration
- If duration exceeded, calls `OnEnd()` automatically
- Always call `base.OnUpdate(now)` first in your override to preserve this behavior

## BaseEvent Properties & Methods

### Properties
| Property | Purpose |
|----------|---------|
| `EventDurationMinutes` | Override to set event duration (null = no automatic ending) |
| `IsActive` | Whether event is currently running |
| `EventStartTime` | When the event started (protected) |
| `EventEndTime` | When the event ended (protected) |

### Helper Methods
| Method | Purpose |
|--------|---------|
| `BroadcastMessage()` | Send message to all players |
| `SendMessageToPlayers()` | Send to specific players |
| `TeleportPlayersFromMaps()` | Teleport players out of event maps |
| `EnsureMonsterRespawns()` | Configure respawns and revive dead monsters |
| `ForceStart()` / `ForceStop()` | GM override controls |
| `ClearOverride()` | Return to scheduled timing |

## GM Commands

- `@event [EventId] start` - Force start
- `@event [EventId] stop` - Force stop
- `@event [EventId] clear` - Clear override
- `@event -l` - List all events

## Monster Kill Integration

**Flow:**
1. `MonsterTable.Drop()` → `EventScheduler.OnMonsterKilled()`
2. All events receive notification via `OnMonsterKilled()`
3. Events check if monster is relevant and handle rewards
4. `EventScheduler.ShouldSkipNormalDrop()` checked before normal drops

**Important:** Override `ShouldSkipNormalDrop()` to return `true` for event monsters, otherwise players get both event rewards AND normal drops.

## IEvent Interface Methods

| Method | Required | Purpose |
|--------|----------|---------|
| `EventId` | Yes | Unique identifier |
| `EventName` | Yes | Display name |
| `GetSchedules()` | Yes | Return scheduled start times |
| `ShouldTrigger()` | **No** | **Base class handles automatically** - checks `GetSchedules()` |
| `OnStart()` | Yes | Called when event begins |
| `OnEnd()` | Yes | Called when event ends |
| `OnUpdate()` | No | Called every second while active. **Always call `base.OnUpdate(now)` first** |
| `OnMonsterKilled()` | No | Handle monster death rewards |
| `ShouldSkipNormalDrop()` | No | Skip normal drops for event monsters |
| `HandlePacket()` | No | Handle incoming packets (self-contained integration) |

**Important:** You should **NOT** override `ShouldTrigger()` unless you have a very specific reason. The base implementation automatically checks your `GetSchedules()` and handles start times. For end times, use `EventDurationMinutes` and let `base.OnUpdate()` handle it.

## Packet Handling (Self-Contained Integration)

Events can handle their own packets without modifying `PacketHandler.cs`. This keeps events completely self-contained.

### How It Works

1. **Override `HandlePacket()`**: Implement packet handling directly in your event class
2. **Check packet ID**: Return `true` if handled, `false` to let it fall through
3. **No registration needed**: The system automatically calls `HandlePacket()` for all active events

### Example

```csharp
/// <summary>
/// Handle incoming packets - self-contained event integration
/// </summary>
public override bool HandlePacket(Client.GameState client, byte[] packet, ushort packetId)
{
    // Only handle specific packet when event is active
    if (packetId == Data.FinishSteedRace && IsActive)
    {
        if (client.Entity.MapID == _currentMapId)
        {
            FinishRace(client);
            return true; // Packet was handled
        }
    }
    
    return false; // Let packet fall through to normal processing
}
```

**Important:** 
- Always check `IsActive` before handling packets
- Return `true` only if you actually handled the packet
- Return `false` to let the packet continue to normal processing
- Packet handlers are checked **before** the normal switch statement, so they take priority
- For Data packets (packet type 10010), the `packetId` parameter contains the command ID (e.g., `Data.FinishSteedRace = 402`), not the packet type

## Integration Points

- **World.cs**: Calls `EventScheduler.Initialize()` on startup, `EventScheduler.Update()` every second
- **MonsterTable.cs**: Calls `EventScheduler.OnMonsterKilled()`, checks `EventScheduler.ShouldSkipNormalDrop()`
- **NPC Handlers**: Check status via `EventScheduler.GetEvent()` and `IsActive` property
- **GM Commands**: `EventCommands.cs` handles event control
- **PacketHandler.cs**: Automatically calls `HandlePacket()` on all active events before normal processing

## Examples

### SteedRace Event
See `SteedRace/` folder for complete implementation:
- Weekday-only scheduling (Monday-Friday at 15:00)
- Duration-based ending (31 minutes)
- Packet handling via `HandlePacket()`
- Custom update logic (invitations, gate opening)
- Shared constants in `SteedRaceConstants.cs`

### CpCastle Event
See `CpCastle/` folder for complete implementation:
- Multiple scheduled times per day (14:00 and 20:00)
- Duration-based ending (30 minutes)
- Warning messages (10 min and 5 min warnings)
- Map-specific monster rewards
- Monster respawn configuration

## Architecture Principles

### Single Source of Truth
- **Define schedules once** in `GetSchedules()` - base class automatically uses them
- **No redundancy** - don't override `ShouldTrigger()` to re-check the same times
- **Duration-based ending** - use `EventDurationMinutes` instead of time-based end checks

### Automatic Handling
- Base `ShouldTrigger()` automatically checks `GetSchedules()` for start times
- Base `OnUpdate()` automatically checks `EventDurationMinutes` for end times
- Only override when you need custom behavior

## Best Practices

1. **Centralize**: Keep all event code in the event's folder
2. **Constants**: Define map IDs, times, durations as constants
3. **Don't override `ShouldTrigger()`**: Use `GetSchedules()` - base class handles it automatically
4. **Use `EventDurationMinutes`**: For duration-based ending instead of time checks
5. **Always call `base.OnUpdate(now)`**: First in your override to preserve duration checking
6. **Null checks**: Always check `IsActive` and null values
7. **Clear messages**: Use descriptive broadcast messages
8. **Monster respawns**: Call `EnsureMonsterRespawns()` in `OnStart()` if needed
9. **Packet handling**: Override `HandlePacket()` for self-contained integration
10. **No PacketHandler modifications**: Never add event-specific code to `PacketHandler.cs` - use `HandlePacket()` instead
