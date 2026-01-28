# PacmanVoice — Current Status ✅

## Overview
- Project pivoted to **.NET MAUI** hosting for cross-platform (Android/iOS/Windows) UI, with the game logic retained in the shared project.
- Speech recognition now implemented using **Azure Cognitive Services Speech** (cloud-first) with an optional offline fallback (Vosk) flagged as optional.

## What is implemented
- **AzureRecognizer** (production-focused):
  - `PacmanVoice/Voice/AzureRecognizer.cs` — constructors for subscription/key, token-provider, real SDK integration (SpeechConfig / SpeechRecognizer), event handlers, Start/Stop (async), phrase-list grammar and mapping to `CommandType`.
  - `PacmanVoice/Voice/AzureProvisioning.md` — provisioning notes and key rotation guidance.
- **Unit tests**:
  - `PacmanVoice.Tests` contains tests for constructor behavior and mapping logic (5 tests passing locally).
- **MAUI spike** (`PacmanVoice.Maui`):
  - Simple UI with Start/Stop buttons, status/result labels, microphone permission handling and DI registration (IRecognizer -> AzureRecognizer).
  - Android manifest requests `RECORD_AUDIO`.

## Build & Test status
- Unit tests: **succeeding locally** (test project targets `net8.0` for fast feedback). ✅
- MAUI spike: **builds successfully for Android** (compile & package warnings noted for Speech native libs). ✅
- Full solution restore previously failed due to missing Mono runtime pack; this was resolved by updating workloads / SDKs and local installs.

## Environment & Security
- **Environment variables required** for Azure Speech: `AZURE_SPEECH_KEY` and `AZURE_SPEECH_REGION`.
  - Add persistently via PowerShell: `[Environment]::SetEnvironmentVariable("AZURE_SPEECH_KEY","<key>","User")` and `[Environment]::SetEnvironmentVariable("AZURE_SPEECH_REGION","<region>","User")`.
- **Important security note:** Do **not** commit subscription keys to source control or paste them in logs/check-ins. If a key was exposed, **regenerate/rotate** it immediately in the Azure portal.

## Next recommended steps (pick one)
1. Embed the **MonoGame renderer** inside the MAUI shell (Android first). 🔲
2. Implement a **token-broker** example and update `AzureRecognizer` to use token auth for production (recommended). 🔲
3. Deploy the MAUI spike to an Android emulator/device and perform live voice tests (requires env vars to be present on device). 🔲

## Quick commands
- Run tests: `dotnet test PacmanVoice.Tests\PacmanVoice.Tests.csproj`
- Build MAUI spike (Android): `dotnet build PacmanVoice.Maui\PacmanVoice.Maui.csproj -f net10.0-android`

---

If you'd like, I can proceed to embed the MonoGame view into the MAUI app (option 1) or add a token broker example (option 2). Which should I do next? 
