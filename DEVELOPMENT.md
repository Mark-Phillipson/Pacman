Development setup
=================

This project requires a .NET 10 SDK that includes the Mono runtime pack (e.g., **10.0.2**). Use the `global.json` at the repo root to pin the SDK used by the repo.

Quick setup
-----------
1. Verify installed SDKs and runtimes:
   - `dotnet --list-sdks`
   - `dotnet --list-runtimes`

2. Install .NET 10 SDK (recommended version pinned in `global.json`):
   - https://dotnet.microsoft.com/download/dotnet/10.0
   - Install the SDK that includes the Mono runtime pack (10.0.2 recommended).
Temporary local workaround
--------------------------
If you're unable to install .NET 10 locally, a temporary workaround is to target **net8.0 / net8.0-android** locally to allow builds to succeed. This is intended as a short-term measure; CI is configured to build using .NET 10.
3. Install MAUI workloads (if you haven't):
   - `dotnet workload install maui`
   - `dotnet workload restore`

4. Build the MAUI project:
   - `dotnet build PacmanVoice.Maui/PacmanVoice.Maui.csproj`

5. Android-specific:
   - Ensure Android SDK / emulator or device is available.
   - Use Visual Studio Mobile workload for easier debugging.

CI / GitHub Actions
-------------------
A workflow is included at `.github/workflows/dotnet.yml` that installs the pinned SDK and workloads in CI.

Notes
-----
- Do not embed Azure subscription keys in the app for production; use a token broker or Azure AD token service.
- If you need help installing the SDK or updating CI, I can add a checkout + setup snippet for your CI system.
