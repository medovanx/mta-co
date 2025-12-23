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
            // Spawn a boss at specific coordinates with 15 minute respawn
            EnsureMonsterSpawn([MapConstants.MY_MAP], MonsterConstants.BossMonster, 900, 100, 200, isBoss: true);
            // Or update existing monsters' respawn settings (no coordinates = update all of that type)
            EnsureMonsterSpawn([MapConstants.MY_MAP], MonsterConstants.RegularMonster, 30);
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
        // Returns true to skip normal drop, false to keep it
        public override bool OnMonsterKilled(Database.MonsterInformation monster, Game.Entity killer)
        {
            if (!IsActive || monster.Name != "SpecialMonster") return false;
            if (monster.Owner.MapID != MapConstants.MY_MAP) return false;
            
            if (killer.Owner != null)
                MyNewEventRewards.OnMonsterKilled(killer.Owner, monster.Name, monster.Owner.MapID);
            
            return true; // Skip normal drop since we handled it
        }

        // Optional: Send pre-event warnings
        public override void OnPreEventWarning(DateTime now)
        {
            // 5 minutes before event starts
            if (now is { Hour: 11, Minute: 55, Second: 0 })
                BroadcastMessage("My New Event will begin in 5 minutes!", Color.White);
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

## Monster Spawning

Use `EnsureMonsterSpawn()` to spawn monsters programmatically or update existing monsters' respawn settings:

```csharp
// Spawn a boss at specific coordinates with custom name and respawn time
EnsureMonsterSpawn(
    [MapConstants.ProudSea],           // Map IDs (array)
    MonsterConstants.Blackbeard,        // Monster npctype (from MonsterConstants)
    900,                                // Respawn time in seconds (15 minutes)
    129,                                // X coordinate (optional - null = update existing)
    178,                                // Y coordinate (optional - null = update existing)
    "Blackbeard",                       // Custom name (optional)
    isBoss: true                        // Mark as boss (optional)
);

// Update all existing monsters of a type (no coordinates = update all)
EnsureMonsterSpawn(
    [MapConstants.CAPTAIN_CASTLE_BEGINNER, MapConstants.CAPTAIN_CASTLE_ADVANCED],
    MonsterConstants.Captain,
    10                                  // Respawn time in seconds
);
```

**Key Features:**
- **Specific location spawn**: Provide `x` and `y` coordinates to spawn at that exact location
- **Update existing monsters**: Omit `x` and `y` to update all existing monsters of that type in the specified maps
- **Uses monster constants**: Uses `npctype` (from `MonsterConstants`) instead of names for reliability
- **Automatic respawn**: Sets `RespawnTime` which is automatically handled by the game's respawn system
- **Boss flag**: Can mark monsters as bosses using the `isBoss` parameter

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
| `EnsureMonsterSpawn()` | Spawn or update monsters at specific locations, or update existing monsters' respawn settings |
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
4. Events return `true` to skip normal drop, `false` to keep it
5. If any event returns `true`, normal drop is skipped (OR logic)

**Important:** Return `true` from `OnMonsterKilled()` for event monsters to skip normal drops, otherwise players get both event rewards AND normal drops. Events check their own maps, so conflicts are unlikely.

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
| `OnMonsterKilled()` | No | Handle monster death rewards. Returns `bool`: `true` = skip normal drop, `false` = keep it |
| `OnPreEventWarning()` | No | Send pre-event warnings (called for all events, even inactive ones) |
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
- **MonsterTable.cs**: Calls `EventScheduler.OnMonsterKilled()` and uses return value to determine if normal drop should be skipped
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

### Captain's Castle Conquest
See `CaptainsCastleConquest/` folder for complete implementation:
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
8. **Monster spawning**: Use `EnsureMonsterSpawn()` to spawn monsters at specific locations or update existing monsters' respawn settings
9. **Monster kills**: Return `true` from `OnMonsterKilled()` to skip normal drops when event handles rewards
10. **Pre-event warnings**: Override `OnPreEventWarning()` for warnings before event starts (optional)
11. **Packet handling**: Override `HandlePacket()` for self-contained integration
12. **No external calls**: Events are self-contained - no need to call event methods from `World.cs` or elsewhere
13. **No PacketHandler modifications**: Never add event-specific code to `PacketHandler.cs` - use `HandlePacket()` instead
