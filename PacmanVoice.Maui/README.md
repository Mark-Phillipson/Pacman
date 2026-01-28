PacmanVoice MAUI Spike

This spike app demonstrates wiring `AzureRecognizer` into a MAUI host with:
- Dependency injection for `IRecognizer` (registered as `AzureRecognizer`).
- Microphone permission request and basic Start/Stop UI.
- Simple UI to display recognized text and status.

How to run:
1. Ensure MAUI workloads are installed (Visual Studio or `dotnet workload install maui`).
2. Set environment variables for Azure Speech locally for testing:
   - `AZURE_SPEECH_KEY`
   - `AZURE_SPEECH_REGION`
3. From the solution root, run `dotnet restore` and `dotnet build PacmanVoice.Maui\PacmanVoice.Maui.csproj`.
4. Run on a target platform (`dotnet build -f net10.0-android` and deploy via Visual Studio or `dotnet publish`).

Notes:
- This spike intentionally focuses on speech integration, not embedding the MonoGame renderer. The game host can be added to the MAUI shell later.
- For production, don't embed Azure keys in the client; use a token broker.
