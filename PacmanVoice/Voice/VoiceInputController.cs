using PacmanVoice.Core;
using System;
using System.Collections.Generic;

namespace PacmanVoice.Voice;

/// <summary>
/// Controls voice input and manages simulation freeze during recognition
/// </summary>
public class VoiceInputController : IDisposable
{
    private readonly IRecognizer _recognizer;
    private readonly SimulationClock _clock;
    private string _lastRecognizedText = string.Empty;

    public event EventHandler<RecognitionResult>? CommandRecognized;
    public event EventHandler<string>? ErrorOccurred;

    public bool IsListening { get; private set; }
    public string LastRecognizedText => _lastRecognizedText;

    public VoiceInputController(IRecognizer recognizer, SimulationClock clock)
    {
        _recognizer = recognizer;
        _clock = clock;

        // Subscribe to recognizer events
        _recognizer.SpeechDetected += OnSpeechDetected;
        _recognizer.SpeechRecognized += OnSpeechRecognized;
        _recognizer.SpeechRejected += OnSpeechRejected;
        _recognizer.RecognitionCompleted += OnRecognitionCompleted;
        _recognizer.RecognitionError += OnRecognitionError;
    }

    public void Start()
    {
        _recognizer.Start();
    }

    public void Stop()
    {
        _recognizer.Stop();
        _clock.SetListeningFrozen(false);
        IsListening = false;
    }

    public void ForceResetListeningState()
    {
        _clock.SetListeningFrozen(false);
        IsListening = false;
    }

    public void UpdateGrammar(IEnumerable<string> phrases, Dictionary<string, CommandType> commandMap)
    {
        _recognizer.UpdateGrammar(phrases, commandMap);
    }

    private void OnSpeechDetected(object? sender, EventArgs e)
    {
        // Freeze simulation immediately when speech is detected
        _clock.SetListeningFrozen(true);
        IsListening = true;
    }

    private void OnSpeechRecognized(object? sender, RecognitionResult result)
    {
        _lastRecognizedText = result.Text;

        // Unfreeze simulation after recognition
        _clock.SetListeningFrozen(false);
        IsListening = false;

        // Forward the recognized command
        CommandRecognized?.Invoke(this, result);
    }

    private void OnSpeechRejected(object? sender, EventArgs e)
    {
        // Unfreeze simulation even if speech was rejected
        _clock.SetListeningFrozen(false);
        IsListening = false;
    }

    private void OnRecognitionCompleted(object? sender, EventArgs e)
    {
        // Safety: unfreeze on completion
        _clock.SetListeningFrozen(false);
        IsListening = false;
    }

    private void OnRecognitionError(object? sender, string error)
    {
        // Safety: unfreeze on error
        _clock.SetListeningFrozen(false);
        IsListening = false;

        ErrorOccurred?.Invoke(this, error);
    }

    public void Dispose()
    {
        if (_recognizer is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
