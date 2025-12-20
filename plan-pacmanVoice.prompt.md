# Pac-Man Voice-Only (Windows 11) — Prototype Plan (System.Speech)

## Goal
Build a Pac‑Man clone playable on Windows 11 using **voice only**, with a twist: **any time voice is detected, the game freezes until the command is fully recognized, then resumes with no time penalty**.

Prototype uses **Windows System.Speech** (offline). Architecture must be pluggable so **Azure Speech-to-Text** can be added later.

## Hard Requirements
- Voice-only control.
- Freeze simulation immediately when speech is detected; resume only after final recognition (or rejection/timeout).
- “No time penalty”: timers/AI must not advance while frozen.
- All **non-direction** commands must be **>= 2 words**.
- Directions are exempt but must be **customizable per user**.
- Avoid the word **“cancel”**.
- Abort phrase: **“never mind”** and it must be shown in the UI.
- Quit flow: **“quit game”** then **“quit confirm”**.

## Default Commands (v1)
Directions (customizable):
- `up`, `down`, `left`, `right`

Non-direction (>= 2 words):
- `begin game`
- `pause game`
- `resume game`
- `restart game`
- `game status`
- `repeat that`
- `quit game` (enters quit confirmation state)
- `quit confirm` (exits)
- `never mind` (abort quit confirmation / dismiss)

## Architecture
- `SimulationClock`: runs only when not paused/frozen.
- `GameSimulation`: all gameplay logic (grid movement, pellets, ghosts, collisions) driven only by simulation delta.
- `IRecognizer`: abstraction for speech engines.
  - `SystemSpeechRecognizer` (offline now)
  - `AzureRecognizer` (later)
- `VoiceInputController`: freezes/unfreezes simulation based on recognizer lifecycle.
- `CommandGrammarBuilder`: builds recognition grammar from config + current UI state.
- `CommandPolicy/Parser`: enforces >=2 words for non-direction commands; directions are exempt; applies confidence threshold.
- `CommandRouter`: applies recognized command to game state.
- `HudOverlay`: displays listening/frozen state + command hints including “Say ‘never mind’ to abort”.

## Recognition (System.Speech)
- Use **command grammar** (not dictation) using `GrammarBuilder` + `Choices`.
- Subscribe to events:
  - `SpeechDetected` → freeze immediately
  - `SpeechRecognized` → apply command; resume
  - `SpeechRecognitionRejected` → resume
  - `RecognizeCompleted` / error → resume (safety)
- Configure timeouts for responsive end-of-utterance:
  - `InitialSilenceTimeout`, `EndSilenceTimeout`, `BabbleTimeout`

## Freeze Semantics (“No Time Penalty”)
- Keep rendering active so UI stays responsive.
- Stop accumulating simulation time while frozen (no catch-up burst on resume).
- Simulation runs iff `!UserPaused && !ListeningFrozen`.

## UI States
- Normal gameplay HUD.
- Listening/frozen overlay:
  - “Listening…” / “Recognizing…”
  - Show: “Say ‘never mind’ to abort.”
- Quit confirmation overlay:
  - “Say ‘quit confirm’ to exit.”
  - Only accept `quit confirm` or `never mind` in this state.

## Per-User Customization
- `voice-commands.json` with profiles.
- Each profile can override direction phrases (and other command phrases if needed).
- Validate at load:
  - non-direction phrases must be >=2 syllables
  - forbid “cancel”

## Implementation Steps
1) Scaffold MonoGame project and game loop with a `SimulationClock` that can freeze.
2) Add config loader and default `voice-commands.json`.
3) Implement `IRecognizer` + `SystemSpeechRecognizer`.
4) Implement `VoiceInputController` to freeze on `SpeechDetected` and resume on final/rejected.
5) Implement `CommandRouter` for movement + pause/resume/restart + quit flow.
6) Add HUD overlays for listening + quit confirmation including “never mind”.
7) Add hooks to later plug in `AzureRecognizer` using same `IRecognizer` interface.
