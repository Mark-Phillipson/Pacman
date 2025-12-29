# New Pacman Game Features - User Guide

## Batch Entry Mode

### What is Batch Entry Mode?

Batch Entry Mode allows you to plan your moves strategically. Instead of giving one direction command at a time, you can queue up multiple directions and execute them all at once. The game will pause while you're planning so you have time to think about your strategy.

### How to Use Batch Entry Mode

1. **Enter Batch Mode**: Say "**batch entry**"
   - The game pauses and enters batch mode
   
2. **Queue Your Moves**: Say direction commands
   - Say "**south**", "**east**", "**west**", or "**north**" to queue each move
   - Queue as many moves as you want (e.g., "south", "east", "south", "east")
   
3. **Execute the Batch**: Say "**apply batch**"
   - The game resumes and executes all your queued moves in order
   - Each move happens one grid step at a time
   
4. **Cancel (Optional)**: Say "**never mind**"
   - Exits batch mode without executing any queued moves
   - Returns to normal playing

### Example

```
Player says: "batch entry"
→ Game pauses, batch mode active

Player says: "south"
→ First move queued

Player says: "east"
→ Second move queued

Player says: "south"
→ Third move queued

Player says: "apply batch"
→ Game resumes and executes: south → east → south
```

## Game Speed Control

### What is Game Speed Control?

You can adjust how fast the game plays. Make it slower for strategic play or faster for a more challenging experience.

### How to Control Game Speed

- **Say "speed up"** - Makes the game 20% faster
  - Say it multiple times for even faster gameplay
  - Maximum speed: 3.0x (3 times normal speed)
  
- **Say "slow down"** - Makes the game ~17% slower
  - Say it multiple times for even slower gameplay
  - Minimum speed: 0.1x (10 times slower)
  
- **Say "normal speed"** - Resets to normal speed (1.0x)

### Speed Multiplier Examples

| What You Say | Speed Multiplier | Effect |
|---|---|---|
| "speed up" (once) | 1.2x | 20% faster |
| "speed up" (twice) | 1.44x | 44% faster |
| "speed up" (5 times) | 2.49x | ~2.5x faster |
| "slow down" (once) | 0.83x | ~17% slower |
| "slow down" (twice) | 0.69x | ~30% slower |
| "normal speed" | 1.0x | Back to normal |

### Tips

- Use **"speed up"** to make the game more challenging
- Use **"slow down"** to give yourself more reaction time
- Speed adjustment applies to both Pac-Man and ghosts
- You can change speed at any time during gameplay

## Combining Features

You can use batch entry mode and speed control together:

```
Player says: "speed up"
→ Game becomes faster

Player says: "batch entry"
→ Enter batch mode with the faster speed

Player says: "south", "east", "north"
→ Queue moves

Player says: "apply batch"
→ Moves execute at the faster speed
```

## Summary of New Voice Commands

| Command | Effect | Available In |
|---|---|---|
| "batch entry" | Enter batch mode | Playing state |
| "south" / "east" / "west" / "north" | Queue a direction | Batch mode |
| "apply batch" | Execute queued moves | Batch mode |
| "never mind" | Cancel batch mode | Batch mode |
| "speed up" | Increase game speed 20% | Anytime (Playing or Paused) |
| "slow down" | Decrease game speed ~17% | Anytime (Playing or Paused) |
| "normal speed" | Reset to normal speed | Anytime (Playing or Paused) |

## Troubleshooting

**Q: The game doesn't recognize my batch entry command**
- A: Make sure you're saying "batch entry" (two words) while the game is actively playing
- The game must be in the Playing state to enter batch mode

**Q: I said a direction but nothing happened**
- A: Directions are only recognized in batch mode
- To change direction during normal play, just say the direction (north, south, east, west)
- In batch mode, directions are queued but not executed until you say "apply batch"

**Q: Can I pause the game while in batch mode?**
- A: No, batch mode itself is a pause state
- Say "never mind" to exit batch mode and return to playing

**Q: What's the fastest/slowest the game can go?**
- A: Fastest: 3.0x (say "speed up" about 5 times)
- A: Slowest: 0.1x (say "slow down" about 10 times)
