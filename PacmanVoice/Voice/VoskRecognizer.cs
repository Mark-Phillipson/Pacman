#if ANDROID
using System;
using System.Collections.Generic;
using PacmanVoice.Voice;

namespace PacmanVoice.Voice;

/// <summary>
/// Android-only recognizer stub for Vosk. Implementation will be filled in during Phase 3.
/// </summary>
public class VoskRecognizer : IRecognizer, IDisposable
{
    public event EventHandler? SpeechDetected;
    public event EventHandler<RecognitionResult>? SpeechRecognized;
    public event EventHandler? SpeechRejected;
    public event EventHandler? RecognitionCompleted;
    public event EventHandler<string>? RecognitionError;

    public VoskRecognizer()
    {
        // TODO: Initialize Vosk model from Android assets and start audio capture using AudioRecord
    }

    public void Start()
    {
        // TODO: Start audio capture and feed to Vosk recognizer
    }

    public void Stop()
    {
        // TODO: Stop audio capture
    }

    public void UpdateGrammar(IEnumerable<string> phrases, Dictionary<string, CommandType> commandMap)
    {
        // TODO: Update vocabulary / grammar used by Vosk recognizer
    }

    public void Dispose()
    {
        // TODO: Dispose Vosk resources
    }
}
#else
using System;
using System.Collections.Generic;
using PacmanVoice.Voice;

namespace PacmanVoice.Voice;

/// <summary>
/// Non-Android placeholder for compilation on desktop platforms.
/// Emits an error event to indicate this recognizer is Android-only.
/// </summary>
public class VoskRecognizer : IRecognizer
{
    public event EventHandler? SpeechDetected;
    public event EventHandler<RecognitionResult>? SpeechRecognized;
    public event EventHandler? SpeechRejected;
    public event EventHandler? RecognitionCompleted;
    public event EventHandler<string>? RecognitionError;

    public VoskRecognizer()
    {
        RecognitionError?.Invoke(this, "VoskRecognizer is only supported on Android. Use SystemSpeechRecognizer on Windows.");
    }

    public void Start() { }
    public void Stop() { }
    public void UpdateGrammar(IEnumerable<string> phrases, Dictionary<string, CommandType> commandMap) { }
}
#endif