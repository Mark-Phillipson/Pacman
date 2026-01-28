# PacmanVoice MAUI + Azure Speech Checklist

## Overview
Port PacmanVoice to **.NET MAUI** (Android / iOS / Windows) and use **Azure Cognitive Services Speech** (cloud-first) for speech recognition. MAUI will host the MonoGame renderer in a platform shell and provide cross-platform permission, lifecycle, and dependency injection for a unified `IRecognizer` service.

---

## Phase 1: Development Environment Setup

- [ ] Install .NET MAUI workload (Visual Studio: "Mobile development with .NET" that includes MAUI)
- [ ] Verify .NET SDK (recommended 8.0+) and Visual Studio supports MAUI
- [x] Set up Android emulator or connect physical device via USB debugging (keeps Android testing)
- [x] Verify device connection with `adb devices`
- [ ] Create an **Azure Speech** resource (Free tier) and note **Key** and **Region**
- [ ] Install Azure CLI (optional) and sign in for resource management (`az login`)
- [ ] Ensure microphone permissions and platform-specific debugging tools are available

> ⚠️ Note: For production, **do not embed** Azure keys in client apps — use a token server or Azure AD-managed tokens. For local development, store keys securely in environment variables (`AZURE_SPEECH_KEY`, `AZURE_SPEECH_REGION`) or use user secrets.

---

## Phase 2: Project Configuration

- [ ] Add a new MAUI host project (`PacmanVoice.Maui`) and keep the existing game logic in a shared library
- [ ] Register `IRecognizer` as a service in MAUI DI and add platform-specific implementations (e.g., `AzureRecognizer`, `PlatformRecognizer`) as transient/singleton services
- [ ] Add `Microsoft.CognitiveServices.Speech` NuGet package to the MAUI project
- [ ] Add configuration for `AZURE_SPEECH_KEY` and `AZURE_SPEECH_REGION` (environment variables or secrets)
- [ ] Add runtime permission request helpers for microphone access across platforms
- [ ] Update packaging / CI to avoid checking in secrets and to support MAUI builds

---

## Phase 3: Azure Speech Integration (Spike)

- [ ] Create `AzureRecognizer.cs` implementing `IRecognizer` (stub + unit tests)
- [ ] Initialize `SpeechConfig` using `SpeechConfig.FromSubscription(key, region)` or token-based auth
- [ ] Implement `Start()` / `Stop()` using `SpeechRecognizer` (SDK handles microphone input on platforms)
- [ ] Implement `UpdateGrammar()` using phrase lists / simple post-processing to map recognized text to `CommandType` values
- [ ] Add event raising for `SpeechDetected`, `SpeechRecognized`, `SpeechRejected`, and `RecognitionError`
- [ ] Implement robust error handling and transient network failure handling (retry/backoff)
- [ ] Add optional offline fallback plan (e.g., Vosk) if network is unavailable — mark as **optional**
- [ ] Test recognition in isolation (desktop MAUI or Android emulator) using test phrases in `voice-commands.json`

> ℹ️ Note: The Speech SDK provides high-accuracy cloud recognition and phrase-list/grammar features for small-vocabulary apps. It also supports audio streaming and custom models if you later need improved accuracy.

---

## Phase 4: Platform & Lifecycle Integration

- [ ] Wire `IRecognizer` via MAUI DI and update `PacmanGame` to obtain the recognizer from the host services
- [ ] Request microphone permission at runtime and handle denial gracefully
- [ ] Handle app lifecycle (pause/resume) via MAUI lifecycle events and pause recognition when needed
- [ ] Add a user-facing toggle to select Cloud (Azure) vs Local (Vosk) mode (optional)

---

## Phase 5: Content & Assets

- [ ] Verify game audio assets and formats are supported on mobile platforms
- [ ] Ensure `voice-commands.json` is included in app resources and accessible via MAUI cross-platform file APIs

---

## Phase 6: UI/UX Adjustments

- [ ] Add visual indicators for active listening and recognition results
- [ ] Add microphone permission explanation in settings
- [ ] Test HUD scaling and touch controls for mobile devices

---

## Phase 7: Testing & Debugging

- [ ] Provision an Azure Speech resource and confirm free-tier quotas meet development needs
- [ ] Build and deploy MAUI app to Android emulator and physical device
- [ ] Test voice commands: "up", "down", "left", "right", "start", "pause", "quit"
- [ ] Test offline / network-failure behavior and fallback UX
- [ ] Monitor Azure usage and add telemetry (optional) to track recognition events and costs

---

## Phase 8: Release & Security

- [ ] Replace any development keys with a secure token service for production clients
- [ ] Document Azure resource setup and maintenance in `README.md`
- [ ] Test release build on target devices
- [ ] Update release notes explaining cloud dependency and privacy considerations

---

## Notes

**Azure Speech Resource Name / Region:** _______________  
**Client Auth Method (env / token service):** _______________  
**Test Devices:** _______________  

### Issues / Considerations
- Do not ship subscription keys in the client. Use a token broker or Azure AD for production scenarios.
- Verify free-tier quotas for your monthly dev usage before relying on cloud-only mode.

### Resources
- [Azure Speech SDK (C#) Docs](https://learn.microsoft.com/azure/cognitive-services/speech-service/)
- [Speech SDK NuGet: Microsoft.CognitiveServices.Speech](https://www.nuget.org/packages/Microsoft.CognitiveServices.Speech/)
- [MAUI Docs - Microsoft](https://learn.microsoft.com/dotnet/maui/)
- [Azure Speech Pricing & Free Tier](https://azure.microsoft.com/pricing/details/cognitive-services/speech-services/)

---

**Next action:** Create an `AzureRecognizer` spike and add `AzureRecognizer.cs` stub in `PacmanVoice/Voice/` so we can iterate on SDK usage and tests.

