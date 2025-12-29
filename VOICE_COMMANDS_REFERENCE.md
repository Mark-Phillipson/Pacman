# Voice Commands Quick Reference

## All Voice Commands for Pacman Game

### Starting & Game Control (Existing)
| Command | Effect | When Available |
|---------|--------|-----------------|
| "begin game" | Start a new game | Game Not Started |
| "pause game" | Pause the game | Playing |
| "resume game" | Resume from pause | Paused |
| "restart game" | Start over | Anytime (except menu) |
| "quit game" | Confirm quit | Playing/Paused |
| "quit confirm" | Confirm quit request | Quit Confirmation |
| "never mind" | Cancel action/message | Most states |
| "game status" | Report score/level/lives | Playing/Paused |
| "repeat that" | Repeat last status | Playing/Paused |

### Movement - Normal Play (Existing)
| Command | Effect | When Available |
|---------|--------|-----------------|
| "north" | Move up | Playing |
| "south" | Move down | Playing |
| "east" | Move right | Playing |
| "west" | Move left | Playing |

### Movement - Batch Mode (NEW)
| Command | Effect | When Available |
|---------|--------|-----------------|
| "batch entry" | Enter batch planning mode | Playing |
| "north" / "south" / "east" / "west" | Queue a direction | Batch Mode |
| "apply batch" | Execute all queued moves | Batch Mode |
| "never mind" | Cancel batch (don't apply) | Batch Mode |

### Game Speed Control (NEW)
| Command | Effect | When Available |
|---------|--------|-----------------|
| "speed up" | Increase game speed 20% | Anytime (except menus) |
| "slow down" | Decrease game speed ~17% | Anytime (except menus) |
| "normal speed" | Reset to normal speed | Anytime (except menus) |

### Game Over (Existing)
| Command | Effect | When Available |
|---------|--------|-----------------|
| "start new game" | Play again | Game Over |
| "quit to desktop" | Exit game | Game Over |

---

## Command Usage Examples

### Example 1: Normal Gameplay
```
1. "begin game"          → Game starts, ready to play
2. "south"               → Pacman moves down
3. "east"                → Pacman moves right
4. "pause game"          → Game pauses
5. "resume game"         → Game continues
6. "speed up"            → Game moves 20% faster
7. "quit game"           → Game asks for confirmation
8. "quit confirm"        → Game exits
```

### Example 2: Using Batch Mode
```
1. "begin game"          → Game starts
2. "batch entry"         → Enter batch mode (game pauses)
3. "south"               → Queue first move
4. "east"                → Queue second move
5. "east"                → Queue third move
6. "south"               → Queue fourth move
7. "apply batch"         → Execute: south → east → east → south
                           Game returns to normal play
```

### Example 3: Speed Control
```
1. "begin game"          → Game starts at normal speed
2. "speed up"            → Speed = 1.2x (20% faster)
3. "speed up"            → Speed = 1.44x (44% faster)
4. "speed up"            → Speed = 1.73x (73% faster)
5. "slow down"           → Speed = 1.44x (back to 44% faster)
6. "normal speed"        → Speed = 1.0x (reset to normal)
7. "slow down"           → Speed = 0.83x (~17% slower)
8. "slow down"           → Speed = 0.69x (~30% slower)
```

### Example 4: Complex Gameplay
```
1. "begin game"                    → Game starts
2. "slow down"                     → Make it easier
3. "slow down"                     → Even slower
4. "south"                         → Move manually
5. "east"                          → Move manually
6. "batch entry"                   → Enter batch mode
7. "south"                         → Queue move
8. "east"                          → Queue move
9. "south"                         → Queue move
10. "apply batch"                  → Execute queued moves
11. "speed up"                     → Increase speed
12. "pause game"                   → Pause to think
13. "game status"                  → Check score
14. "resume game"                  → Continue
```

---

## Game Speed Multiplier Reference

### How It Works
- **Multiplier 1.0** = Normal speed (default)
- **Multiplier > 1.0** = Faster (e.g., 2.0 = double speed)
- **Multiplier < 1.0** = Slower (e.g., 0.5 = half speed)

### Speed Up Sequence
| Times You Say "speed up" | Multiplier | Speed |
|---|---|---|
| 0 | 1.00x | Normal |
| 1 | 1.20x | 20% faster |
| 2 | 1.44x | 44% faster |
| 3 | 1.73x | 73% faster |
| 4 | 2.07x | 2x faster |
| 5 | 2.49x | 2.5x faster |

### Speed Down Sequence
| Times You Say "slow down" | Multiplier | Speed |
|---|---|---|
| 0 | 1.00x | Normal |
| 1 | 0.83x | 17% slower |
| 2 | 0.69x | 30% slower |
| 3 | 0.58x | 42% slower |
| 4 | 0.48x | 52% slower |
| 5 | 0.40x | 60% slower |

### Boundaries
- **Maximum Speed**: 3.0x (cannot go faster)
- **Minimum Speed**: 0.1x (cannot go slower)
- You can say "normal speed" anytime to reset to 1.0x

---

## Tips for Using New Features

### Batch Mode Tips
1. **Best For**: Planning complex paths around ghosts
2. **Planning**: Use when paused to think about your next moves
3. **Length**: Queue 3-5 moves for strategic advantage
4. **Cancel**: Say "never mind" if you change your mind before applying

### Speed Control Tips
1. **Easier Play**: Use "slow down" to give yourself more reaction time
2. **Challenge**: Use "speed up" to make the game harder
3. **Temporary**: Speed changes apply immediately, just say "normal speed" to reset
4. **Save Your Game**: Speed setting is lost when game ends (resets to 1.0x)

### Combination Strategy
1. Use **"slow down"** to slow the game
2. Use **"batch entry"** to plan carefully
3. Queue up your moves while game is paused
4. Use **"apply batch"** to execute moves
5. Use **"speed up"** for the execute phase
6. Great for strategic players!

---

## Troubleshooting

### "I said the command but nothing happened"

**Problem**: Command not recognized
- **Solution**: Make sure you're in the right game state:
  - "batch entry" → must be Playing
  - Directions in batch mode → must be in BatchMode
  - Speed commands → work anytime

**Problem**: Recognizer didn't hear you clearly
- **Solution**: Speak clearly and at normal volume
- **Solution**: Make sure microphone is working
- **Solution**: Try again with different pronunciation

### "I'm in batch mode but my directions aren't executing"

**Problem**: Forgot to say "apply batch"
- **Solution**: Say "apply batch" to execute all queued moves

**Problem**: Said "never mind" by accident
- **Solution**: You exited batch mode without applying
- **Solution**: Say "batch entry" again to queue new moves

### "The game is too fast / too slow"

**Problem**: Want to adjust speed
- **Solution**: Say "speed up" to make faster or "slow down" to make slower
- **Solution**: Say "normal speed" to reset to default

**Problem**: Already at maximum/minimum speed
- **Solution**: Maximum speed is 3.0x (can't go faster)
- **Solution**: Minimum speed is 0.1x (can't go slower)

---

## Command Syntax Rules

All commands must be spoken clearly:
- **Batch commands**: "batch entry", "apply batch" (must be exact)
- **Speed commands**: "speed up", "slow down", "normal speed" (must be exact)
- **Direction commands**: "north", "south", "east", "west" (single words OK)
- **Other commands**: 2+ words required for safety (e.g., "begin game", not just "begin")

No "ums", "ahs", or extra words - the recognizer needs clean speech.
