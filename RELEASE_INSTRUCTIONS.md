# Creating a GitHub Release for Pacman Voice

## Quick Steps

### 1. Create Release Package
The game is already built in: `PacmanVoice\bin\Release\net10.0\win-x64\publish\`

Create a zip file with:
```powershell
cd C:\Users\MPhil\source\repos\Pacman
Compress-Archive -Path "PacmanVoice\bin\Release\net10.0\win-x64\publish\*" -DestinationPath "PacmanVoice-v1.0-win-x64.zip" -Force
```

### 2. Create GitHub Release
1. Go to your repository: https://github.com/Mark-Phillipson/Pacman
2. Click **Releases** → **Create a new release**
3. Click **Choose a tag** → Type `v1.0` → **Create new tag**
4. Set Release title: `Pacman Voice v1.0`
5. Add release notes (see template below)
6. Drag and drop `PacmanVoice-v1.0-win-x64.zip`
7. Click **Publish release**

### 3. Release Notes Template

```markdown
# Pac-Man Voice v1.0 🎮🎤

A voice-controlled Pac-Man clone with offline speech recognition!

## 🎮 How to Play

1. **Download** `PacmanVoice-v1.0-win-x64.zip`
2. **Extract** all files to a folder
3. **Run** `PacmanVoice.exe`
4. Allow microphone access when prompted
5. Say **"begin game"** to start!

## ✨ Features

- 🎙️ **100% Voice Control** - No keyboard needed
- 🔇 **Offline Recognition** - Uses Windows Speech (no internet required)
- ⏸️ **Smart Freeze** - Game pauses while listening
- 🎯 **Customizable Commands** - Edit `voice-commands.json`

## 🎤 Voice Commands

- **Directions**: Say "north","east", "south", "west", or  "right", "down", "left"
- **Start**: "begin game"
- **Pause**: "pause game"
- **Resume**: "resume game"
- **Quit**: "quit game" then "quit confirm"
- **Status**: "game status"

## 📋 Requirements

- ✅ Windows 10/11
- ✅ Microphone
- ✅ ~80 MB disk space
- ❌ No .NET installation needed (self-contained)

## 🐛 Known Issues

- First voice command may take 2-3 seconds to initialize
- Windows Speech works best with clear pronunciation
- Game window must have focus for voice input

## 📝 Customization

Edit `voice-commands.json` to customize voice phrases. See [README](https://github.com/Mark-Phillipson/Pacman/blob/main/README.md) for details.

---

**Platform**: Windows x64 only  
**License**: MIT  
**Feedback**: Open an issue!
```

## Additional Tips

### For Future Releases
- **Update version numbers** in the tag, filename, and release notes
- **Create a changelog** listing new features and bug fixes
- **Test on a clean Windows machine** before releasing

### Optional: Add Release Badge to README
Add to the top of README.md:
```markdown
[![Latest Release](https://img.shields.io/github/v/release/Mark-Phillipson/Pacman)](https://github.com/Mark-Phillipson/Pacman/releases/latest)
```

### Building for Different Configurations
```bash
# Smaller file size (requires .NET 10 installed)
dotnet publish -c Release -r win-x64 --self-contained false

# Different Windows versions
dotnet publish -c Release -r win-x86 --self-contained true  # 32-bit
dotnet publish -c Release -r win-arm64 --self-contained true  # ARM64
```
