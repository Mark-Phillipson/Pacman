# PacmanVoice Android Port Checklist

## Overview
Port PacmanVoice to Android using multi-target .csproj and Vosk offline speech recognition.

---

## Phase 1: Development Environment Setup

- [ ] Verify Android SDK is installed (Visual Studio Installer → "Mobile development with .NET")
- [ ] Install Android NDK (required for native Vosk libraries)
- [ ] Set up Android emulator or connect physical device via USB debugging
- [ ] Verify device connection with `adb devices`
- [ ] Note target Android version: _______ (API level: _______)

---

## Phase 2: Project Configuration

- [ ] Convert `PacmanVoice.csproj` to multi-target (`net10.0;net10.0-android`)
- [ ] Add conditional package reference for `MonoGame.Framework.DesktopGL` (Windows only)
- [ ] Add conditional package reference for `MonoGame.Framework.Android` (Android only)
- [ ] Add `Vosk` NuGet package
- [ ] Remove or conditionally exclude `System.Speech` package for Android
- [ ] Set Android minimum SDK version to API 26 (Android 8.0)
- [ ] Create `AndroidManifest.xml` with `RECORD_AUDIO` permission

---

## Phase 3: Vosk Speech Recognition

- [ ] Download Vosk small English model (`vosk-model-small-en-us-0.15`)
- [ ] Add model files to Android assets folder
- [ ] Create `VoskRecognizer.cs` implementing `IRecognizer` interface
- [ ] Implement audio capture using Android's `AudioRecord` API
- [ ] Implement Vosk model initialization and audio processing
- [ ] Implement command matching against `voice-commands.json` vocabulary
- [ ] Test Vosk recognition in isolation before integrating

---

## Phase 4: Platform-Conditional Code

- [ ] Add `#if ANDROID` conditionals in `PacmanGame.cs` for recognizer creation
- [ ] Update file/config loading to use `TitleContainer.OpenStream()` for cross-platform
- [ ] Handle Android asset paths for `voice-commands.json`
- [ ] Add Android activity lifecycle handling (pause/resume)
- [ ] Conditionally exclude Windows-specific error messages

---

## Phase 5: Content Pipeline

- [ ] Update `Content.mgcb` to support Android platform
- [ ] Convert WAV audio files to OGG format for Android
- [ ] Verify sprite/font assets work on Android
- [ ] Test content loading on Android device/emulator

---

## Phase 6: UI/UX Adjustments

- [ ] Test game rendering on various Android screen sizes
- [ ] Adjust HUD scaling for mobile screens (optional)
- [ ] Add touch control fallback (optional but recommended)
- [ ] Test audio playback on Android

---

## Phase 7: Testing & Debugging

- [ ] Build and deploy to Android emulator
- [ ] Test voice commands: "up", "down", "left", "right"
- [ ] Test voice commands: "start", "pause", "quit"
- [ ] Test game audio (death, eat fruit, theme, etc.)
- [ ] Test on physical Android device
- [ ] Verify microphone permission prompt appears
- [ ] Test gameplay responsiveness

---

## Phase 8: Release Preparation

- [ ] Configure release build settings
- [ ] Sign APK/AAB for distribution
- [ ] Test release build on device
- [ ] Document any Android-specific usage notes
- [ ] Update README.md with Android instructions

---

## Notes

**Target Android Version:** _______________  
**Vosk Model Used:** _______________  
**Test Devices:** _______________  

### Issues Encountered
- 

### Resources
- [Vosk Models](https://alphacephei.com/vosk/models)
- [MonoGame Android Setup](https://docs.monogame.net/articles/getting_started/platforms/android.html)
- [Vosk Android Example](https://github.com/alphacep/vosk-android-demo)
