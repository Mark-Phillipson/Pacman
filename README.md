# Pac-Man Voice

A voice-controlled Pac-Man clone built with MonoGame and Windows System.Speech for offline voice recognition.

## Features

- **Voice-only control** - No keyboard or mouse required during gameplay
- **Freeze-on-listen** - Game simulation freezes when speech is detected and resumes after recognition
- **No time penalty** - Timers and AI don't advance while frozen
- **Customizable commands** - Configure voice phrases per user profile
- **Quit safety** - Two-step quit confirmation to prevent accidental exits

## Requirements

- Windows 11 (or Windows 10)
- .NET 9.0 SDK or later
- Microphone for voice input

## Building

```bash
cd PacmanVoice
dotnet restore
dotnet build
```

## Running

```bash
cd PacmanVoice
dotnet run
```

On first run, Windows may prompt you to allow microphone access for speech recognition.

## Running with PowerShell

To run the Pac-Man Voice application using PowerShell, follow these steps:

1. Open PowerShell.
2. Navigate to the project directory:
   ```powershell
   cd PacmanVoice
   ```
3. Run the application:
   ```powershell
   dotnet run
   ```

On first run, Windows may prompt you to allow microphone access for speech recognition.

## Voice Commands

### Default Commands

**Directions** (1 word, customizable):
- `north`
- `south`
- `east`
- `west`
- `down`
- `left`
- `right`

**Game Control** (2+ words required):
- `begin game` - Start the game
- `pause game` - Pause gameplay
- `resume game` - Resume from pause
- `restart game` - Restart the current game
- `game status` - Speak current score and lives
- `repeat that` - Repeat the last status message
- `quit game` - Enter quit confirmation mode
- `quit confirm` - Confirm quit and exit
- `never mind` - Cancel quit confirmation or dismiss messages

### Rules

1. All non-direction commands must be **2 or more words**
2. Direction commands can be customized per user
3. The word **"cancel"** is forbidden (use "never mind" instead)
4. During quit confirmation, only `quit confirm` or `never mind` are accepted

## Customizing Commands

Edit `voice-commands.json` to customize voice phrases:

```json
{
  "defaultProfile": "default",
  "profiles": {
    "default": {
      "directions": {
        "north": "north",
        "south": "south",
        "east": "east",
        "west": "west"
      },
      "commands": {
        "beginGame": "begin game",
        ...
      }
    }
  }
}
```

You can create multiple profiles and switch between them by changing `defaultProfile`.

## Architecture

The game is designed with a pluggable architecture to support multiple speech recognition engines:

- **SimulationClock** - Manages game time with pause/freeze support
- **GameSimulation** - Core game logic driven by simulation time
- **IRecognizer** - Abstraction for speech engines
  - **SystemSpeechRecognizer** - Windows System.Speech implementation (offline)
  - Future: Azure Speech-to-Text support
- **VoiceInputController** - Freezes/unfreezes simulation during recognition
- **CommandRouter** - Routes recognized commands to game actions
- **GameRenderer** - Renders the game grid, Pac-Man, ghosts, and pellets
- **HudOverlay** - Displays listening state, status messages, and command hints

## Game States

1. **NotStarted** - Say "begin game" to start
2. **Playing** - Active gameplay
3. **Paused** - Game paused via voice command
4. **QuitConfirmation** - Waiting for "quit confirm" or "never mind"
5. **GameOver** - All lives lost

## How It Works

### Freeze Semantics

When speech is detected:
1. `SpeechDetected` event fires → Simulation freezes immediately
2. Recognition continues in background
3. `SpeechRecognized` or `SpeechRejected` event fires → Simulation resumes
4. No game time accumulates during freeze → No time penalty

The rendering loop continues running while frozen to keep the UI responsive.

### Recognition Flow

1. User speaks a command
2. System.Speech detects voice activity
3. Game freezes (no time passes for Pac-Man or ghosts)
4. Speech recognition completes
5. Command is parsed and mapped to game action
6. Game resumes with command applied
7. HUD shows recognized command

## Troubleshooting

**No voice recognition:**
- Check Windows microphone permissions
- Ensure default audio input device is set correctly
- Verify System.Speech is working (Windows Speech Recognition)

**Game won't start:**
- Ensure you're running on Windows (System.Speech is Windows-only)
- Check that voice-commands.json is copied to output directory

**Commands not recognized:**
- Speak clearly and at normal volume
- Wait for "LISTENING..." indicator to disappear before speaking again
- Try customizing commands to match your accent/pronunciation

## License

This is a prototype implementation for educational purposes.

## Future Enhancements

- Azure Speech-to-Text integration for cloud-based recognition
- More sophisticated maze layouts
- Power pellets and vulnerable ghost state
- Sound effects (DEATH.WAV, EATFRUIT.WAV, etc. are included)
- High score tracking
- Multiple difficulty levels

## Summary of Commands for Publishing and Releasing

To publish and release the Pac-Man Voice application, you can use the following commands in sequence:

```powershell
# Navigate to the project directory
cd PacmanVoice

# Restore dependencies and build the project
dotnet restore

dotnet build

# Publish the project as a standalone application
dotnet publish -c Release -o publish --self-contained

# Navigate to the publish directory
cd publish

# Zip the published directory
Compress-Archive -Path * -DestinationPath ../PacmanVoice.zip

# Go back to the project directory
cd ..

# Upload the release to GitHub using the command line
gh release create v1.0.2 PacmanVoice.zip --title "Release v1.0.2" --notes "Initial release of Pac-Man Voice application"
```

Make sure you have the GitHub CLI installed and authenticated before running the last command.
