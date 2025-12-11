# Event System

Centralized scheduled event management framework. All event logic, timing, and rewards are organized in dedicated folders.

## Architecture

### Core Components

| Component | Purpose |
|-----------|---------|
| `IEvent.cs` | Interface defining event contract |
| `BaseEvent.cs` | Base class with common functionality (GM overrides, helpers) |
| `EventScheduler.cs` | Central manager - registers events, handles timing, monster kills |

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

        public override IEnumerable<EventSchedule> GetSchedules()
        {
            yield return new EventSchedule(EVENT_START_HOUR, 0, 0);
        }

        public override bool ShouldTrigger(DateTime now)
        {
            // Start check
            if (now.Hour == EVENT_START_HOUR && now.Minute == 0 && now.Second == 0)
                return true;

            // End check
            if (IsActive && EventStartTime.HasValue)
            {
                var elapsed = now - EventStartTime.Value;
                if (elapsed.TotalMinutes >= EVENT_DURATION_MINUTES)
                    return true;
            }
            return false;
        }

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
            if (!IsActive || !EventStartTime.HasValue) return;

            var elapsed = now - EventStartTime.Value;
            var remaining = EVENT_DURATION_MINUTES - elapsed.TotalMinutes;

            // Warnings
            if (remaining <= 10 && remaining > 9.9)
                BroadcastMessage("Ending in 10 minutes!", Color.White, Message.System);

            // Auto-end
            if (remaining <= 0) OnEnd();
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

```csharp
// Every day at 14:00
yield return new EventSchedule(14, 0, 0);

// Every Monday at 20:00
yield return new EventSchedule(20, 0, 0, DayOfWeek.Monday);

// Every day at 12:30:45
yield return new EventSchedule(12, 30, 45);
```

## BaseEvent Helper Methods

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
| `GetSchedules()` | Yes | Return scheduled times |
| `ShouldTrigger()` | Yes | Check if event should start/end |
| `OnStart()` | Yes | Called when event begins |
| `OnEnd()` | Yes | Called when event ends |
| `OnUpdate()` | No | Called every second while active |
| `OnMonsterKilled()` | No | Handle monster death rewards |
| `ShouldSkipNormalDrop()` | No | Skip normal drops for event monsters |

## Integration Points

- **World.cs**: Calls `EventScheduler.Initialize()` on startup, `EventScheduler.Update()` every second
- **MonsterTable.cs**: Calls `EventScheduler.OnMonsterKilled()`, checks `EventScheduler.ShouldSkipNormalDrop()`
- **NPC Handlers**: Check status via `EventScheduler.GetEvent()` and `IsActive` property
- **GM Commands**: `EventCommands.cs` handles event control

## Example

See `CpCastle/` folder for complete implementation:
- Multiple scheduled times per day
- Duration-based ending
- Warning messages
- Map-specific monster rewards
- Monster respawn configuration

## Best Practices

1. **Centralize**: Keep all event code in the event's folder
2. **Constants**: Define map IDs, times, durations as constants
3. **Override selectively**: Only override methods you need
4. **Null checks**: Always check `IsActive` and null values
5. **Clear messages**: Use descriptive broadcast messages
6. **Monster respawns**: Call `EnsureMonsterRespawns()` in `OnStart()` if needed
