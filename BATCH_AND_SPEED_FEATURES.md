# Batch Mode and Game Speed Features Implementation

## Overview

This document describes the implementation of two new features requested in `NewRequest.md`:

1. **Batch Entry Mode** - Queue multiple direction commands and apply them in sequence
2. **Game Speed Control** - Adjust game speed slower or faster based on user preference

## Features

### 1. Batch Entry Mode

#### Purpose
Allows strategic planning by entering a batch of direction commands all at once, with the game pausing until the batch is executed. This reduces the need for constant interaction during gameplay.

#### Voice Commands

- **"batch entry"** - Enter batch mode (game pauses)
- **"south", "east", "west", "north"** - Queue direction commands (while in batch mode)
- **"apply batch"** - Execute all queued commands in sequence
- **"never mind"** - Cancel batch mode and return to playing without applying

#### How It Works

1. Player says "batch entry" while the game is playing
2. Game transitions to `BatchMode` state (game paused)
3. Player queues up directions: e.g., "south", "east", "west"
4. Player says "apply batch"
5. Game returns to `Playing` state and executes all queued directions in sequence, one per grid step

#### Implementation Details

**GameState.cs** - Added `BatchMode` state:
```csharp
public enum GameState
{
    NotStarted,
    Playing,
    Paused,
    BatchMode,        // NEW
    QuitConfirmation,
    GameOver
}
```

**GameSimulation.cs** - Added batch mode fields and methods:
```csharp
// Batch mode for queuing multiple direction commands
private List<Direction> _batchDirections = new();
private bool _isBatchMode = false;

// Properties
public bool IsBatchMode => _isBatchMode;
public List<Direction> BatchDirections => _batchDirections;

// Methods
public void EnterBatchMode() { ... }      // Transition from Playing to BatchMode
public void AddBatchDirection(Direction direction) { ... }  // Queue a direction
public void ApplyBatch() { ... }           // Return to Playing and prepare to execute
public void ExitBatchMode() { ... }        // Cancel batch mode
```

**CommandRouter.cs** - Handles batch mode commands:
- When in `BatchMode` state, direction commands are queued instead of executed immediately
- `ApplyBatch` command queues all directions to the command queue for sequential execution
- `NeverMind` command exits batch mode without applying commands

**voice-commands.json** - Added new commands:
```json
"batchEntry": "batch entry",
"applyBatch": "apply batch"
```

**CommandType.cs** - Added new command types:
```csharp
BatchEntry,
ApplyBatch
```

### 2. Game Speed Control

#### Purpose
Allows players to adjust the overall game speed to match their preference, making the game slower for strategic play or faster for a more challenging experience.

#### Voice Commands

- **"speed up"** - Increase game speed by 20% (multiplier × 1.2)
- **"slow down"** - Decrease game speed by ~17% (multiplier ÷ 1.2)
- **"normal speed"** - Reset to normal speed (multiplier = 1.0)

#### How It Works

1. Speed multiplier starts at 1.0 (normal speed)
2. Player says "speed up" - multiplier becomes 1.2
3. Game timing intervals are adjusted: smaller interval = faster movement
4. Range is bounded: 0.1x (10x slower) to 3.0x (3x faster)

#### Implementation Details

**GameSimulation.cs** - Added game speed multiplier:
```csharp
private double _gameSpeedMultiplier = 1.0;

public double GameSpeedMultiplier
{
    get => _gameSpeedMultiplier;
    set
    {
        var v = value;
        if (double.IsNaN(v) || double.IsInfinity(v)) v = 1.0;
        if (v < 0.1) v = 0.1;   // prevent zero/negative or too fast
        if (v > 3.0) v = 3.0;   // prevent extreme slowness
        _gameSpeedMultiplier = v;
    }
}

public void SetGameSpeed(double multiplier) { ... }
```

**Movement Interval Adjustment** - Speed multiplier applied to move intervals:

`GetPacmanMoveInterval()`:
```csharp
// Apply game speed multiplier
// faster multiplier means smaller interval
return interval / _gameSpeedMultiplier;
```

`GetGhostMoveInterval()`:
```csharp
// Apply game speed multiplier
interval /= _gameSpeedMultiplier;
return interval;
```

**CommandRouter.cs** - Handles speed commands:
```csharp
case CommandType.SpeedUp:
    _simulation.GameSpeedMultiplier = Math.Min(_simulation.GameSpeedMultiplier * 1.2, 3.0);
    break;

case CommandType.SlowDown:
    _simulation.GameSpeedMultiplier = Math.Max(_simulation.GameSpeedMultiplier / 1.2, 0.1);
    break;

case CommandType.NormalSpeed:
    _simulation.GameSpeedMultiplier = 1.0;
    break;
```

**voice-commands.json** - Added speed commands:
```json
"speedUp": "speed up",
"slowDown": "slow down",
"normalSpeed": "normal speed"
```

**CommandType.cs** - Added command types:
```csharp
SpeedUp,
SlowDown,
NormalSpeed
```

## Files Modified

1. **Voice/CommandType.cs**
   - Added: `BatchEntry`, `ApplyBatch`, `SpeedUp`, `SlowDown`, `NormalSpeed`

2. **Voice/VoiceCommandsConfig.cs**
   - Updated: `GetCommandMap()` to include new command mappings

3. **voice-commands.json**
   - Added batch entry commands: `batchEntry`, `applyBatch`
   - Added speed commands: `speedUp`, `slowDown`, `normalSpeed`

4. **Game/GameState.cs**
   - Added: `BatchMode` state

5. **Game/GameSimulation.cs**
   - Added: Batch mode fields (`_batchDirections`, `_isBatchMode`)
   - Added: Game speed multiplier field (`_gameSpeedMultiplier`)
   - Added: Batch mode methods (`EnterBatchMode()`, `AddBatchDirection()`, `ApplyBatch()`, `ExitBatchMode()`, `SetGameSpeed()`)
   - Added: Batch mode and speed properties
   - Modified: `GetPacmanMoveInterval()` - Apply speed multiplier
   - Modified: `GetGhostMoveInterval()` - Apply speed multiplier

6. **Game/CommandRouter.cs**
   - Added: Batch mode command handling (when in `BatchMode` state)
   - Added: Speed control command handling
   - Modified: `HandleCommandInternal()` to support new commands

## Design Decisions

### Batch Mode
- Uses a separate `BatchMode` state to clearly indicate when the game is paused for batch entry
- Directions are stored as `Direction` enum values and converted to command types only when applied
- Batch directions are processed through the existing command queuing system for consistent timing

### Game Speed
- Speed is a multiplier rather than absolute value, making it intuitive (2.0 = double speed)
- Applied at the move interval level rather than delta time to maintain game logic consistency
- Bounds prevent extreme slowness (0.1x) or extreme speed (3.0x) which would be unplayable

### Voice Commands
- Multi-word commands (2+ words) follow the project's safety guidelines
- "batch entry" and "apply batch" are clear and consistent with existing command patterns
- Speed commands are simple and intuitive: "speed up", "slow down", "normal speed"

## Testing Recommendations

1. **Batch Mode Testing**
   - Enter batch mode and queue multiple directions (4-5 commands)
   - Verify game pauses during batch entry
   - Apply batch and verify directions execute in order
   - Test "never mind" to cancel batch without applying

2. **Game Speed Testing**
   - Use "speed up" multiple times and verify acceleration
   - Use "slow down" to verify deceleration
   - Use "normal speed" to reset
   - Verify bounds: cannot go below 0.1x or above 3.0x
   - Test with ghost AI to ensure consistent behavior

3. **Edge Cases**
   - Exit batch mode during "apply batch" command processing
   - Queue batch directions while game is paused
   - Combine batch mode with pause/resume commands

## Future Enhancements

1. Status feedback for current game speed (e.g., "Game speed: 1.5x")
2. Batch mode feedback (e.g., "Batch entry active, queued commands: 3")
3. Configurable speed step size (currently 1.2x multiplier)
4. Visual indicators in the game UI for batch mode status
5. Ability to preview batch commands before applying
