using System.Speech.Recognition;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.Versioning;

namespace PacmanVoice.Voice;

/// <summary>
/// Speech recognizer using System.Speech (offline recognition)
/// </summary>
[SupportedOSPlatform("windows")]
public class SystemSpeechRecognizer : IRecognizer, IDisposable
{
    private SpeechRecognitionEngine? _recognizer;
    private Dictionary<string, CommandType> _commandMap = new();
    private bool _isRunning;

    public event EventHandler? SpeechDetected;
    public event EventHandler<RecognitionResult>? SpeechRecognized;
    public event EventHandler? SpeechRejected;
    public event EventHandler? RecognitionCompleted;
    public event EventHandler<string>? RecognitionError;

    public SystemSpeechRecognizer()
    {
        try
        {
            _recognizer = new SpeechRecognitionEngine();
            _recognizer.SetInputToDefaultAudioDevice();

            // Configure timeouts for responsive recognition
            _recognizer.InitialSilenceTimeout = TimeSpan.FromSeconds(3);
            _recognizer.BabbleTimeout = TimeSpan.FromSeconds(2);
            _recognizer.EndSilenceTimeout = TimeSpan.FromSeconds(0.5);
            _recognizer.EndSilenceTimeoutAmbiguous = TimeSpan.FromSeconds(1);

            // Subscribe to events
            _recognizer.SpeechDetected += OnSpeechDetected;
            _recognizer.SpeechRecognized += OnSpeechRecognized;
            _recognizer.SpeechRecognitionRejected += OnSpeechRecognitionRejected;
            _recognizer.RecognizeCompleted += OnRecognizeCompleted;
        }
        catch (Exception ex)
        {
            RecognitionError?.Invoke(this, $"Failed to initialize speech recognizer: {ex.Message}");
        }
    }

    public void Start()
    {
        if (_recognizer == null || _isRunning) return;

        try
        {
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            _isRunning = true;
        }
        catch (Exception ex)
        {
            RecognitionError?.Invoke(this, $"Failed to start recognition: {ex.Message}");
        }
    }

    public void Stop()
    {
        if (_recognizer == null || !_isRunning) return;

        try
        {
            _recognizer.RecognizeAsyncStop();
            _isRunning = false;
        }
        catch (Exception ex)
        {
            RecognitionError?.Invoke(this, $"Failed to stop recognition: {ex.Message}");
        }
    }

    public void UpdateGrammar(IEnumerable<string> phrases, Dictionary<string, CommandType> commandMap)
    {
        if (_recognizer == null) return;

        _commandMap = new Dictionary<string, CommandType>(commandMap, StringComparer.OrdinalIgnoreCase);

        try
        {
            // Clear existing grammars
            _recognizer.UnloadAllGrammars();

            // Create new grammar with command phrases
            var choices = new Choices(phrases.ToArray());
            var grammarBuilder = new GrammarBuilder(choices);
            var grammar = new Grammar(grammarBuilder);

            _recognizer.LoadGrammar(grammar);
        }
        catch (Exception ex)
        {
            RecognitionError?.Invoke(this, $"Failed to update grammar: {ex.Message}");
        }
    }

    private void OnSpeechDetected(object? sender, SpeechDetectedEventArgs e)
    {
        SpeechDetected?.Invoke(this, EventArgs.Empty);
    }

    private void OnSpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
    {
        var text = e.Result.Text;
        var confidence = e.Result.Confidence;

        // Map recognized text to command
        var commandType = _commandMap.TryGetValue(text, out var cmd) ? cmd : CommandType.Unknown;

        var result = new RecognitionResult
        {
            Command = commandType,
            Confidence = confidence,
            Text = text,
            IsRejected = false
        };

        SpeechRecognized?.Invoke(this, result);
    }

    private void OnSpeechRecognitionRejected(object? sender, SpeechRecognitionRejectedEventArgs e)
    {
        SpeechRejected?.Invoke(this, EventArgs.Empty);
    }

    private void OnRecognizeCompleted(object? sender, RecognizeCompletedEventArgs e)
    {
        RecognitionCompleted?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (_recognizer != null)
        {
            Stop();
            _recognizer.Dispose();
            _recognizer = null;
        }
    }
}
