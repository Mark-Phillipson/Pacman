using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace PacmanVoice.Voice;

/// <summary>
/// Azure Cognitive Services Speech SDK recognizer.
///
/// This implementation initializes the Speech SDK when environment variables
/// are present and provides a simple Start/Stop continuous recognition flow.
/// For MAUI usage, configure `AZURE_SPEECH_KEY` and `AZURE_SPEECH_REGION` via
/// secure configuration or use a token service for production.
/// </summary>
public class AzureRecognizer : IRecognizer, IDisposable
{
    public event EventHandler? SpeechDetected;
    public event EventHandler<RecognitionResult>? SpeechRecognized;
    public event EventHandler? SpeechRejected;
    public event EventHandler? RecognitionCompleted;
    public event EventHandler<string>? RecognitionError;

    private SpeechRecognizer? _recognizer;
    private SpeechConfig? _config;
    private bool _configured;
    private CancellationTokenSource? _cts;

    // Local grammar/map for mapping recognized text to CommandType
    private readonly Dictionary<string, CommandType> _commandMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _phrases = new();

    /// <summary>
    /// Default constructor reads subscription key/region from environment variables.
    /// </summary>
    public AzureRecognizer()
        : this(Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY"), Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION"))
    {
    }

    /// <summary>
    /// Create using subscription key and region.
    /// </summary>
    public AzureRecognizer(string? subscriptionKey, string? region, string? language = null)
    {
        if (string.IsNullOrEmpty(subscriptionKey) || string.IsNullOrEmpty(region))
        {
            RecognitionError?.Invoke(this, "Azure Speech is not configured. Set AZURE_SPEECH_KEY and AZURE_SPEECH_REGION environment variables or configure the service in MAUI app settings.");
            _configured = false;
            return;
        }

        try
        {
            _config = SpeechConfig.FromSubscription(subscriptionKey, region);
            if (!string.IsNullOrEmpty(language))
                _config.SpeechRecognitionLanguage = language;

            var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            _recognizer = new SpeechRecognizer(_config, audioConfig);
            AttachEventHandlers();
            _configured = true;
        }
        catch (Exception ex)
        {
            RecognitionError?.Invoke(this, $"Failed to initialize Azure Speech: {ex.Message}");
            _configured = false;
        }
    }

    /// <summary>
    /// Create using a token provider function for secure auth (tokenProvider returns an auth token string).
    /// </summary>
    public AzureRecognizer(Func<CancellationToken, Task<string>> tokenProvider, string region, string? language = null)
    {
        if (tokenProvider == null || string.IsNullOrEmpty(region))
        {
            RecognitionError?.Invoke(this, "Token provider or region is not configured for Azure speech.");
            _configured = false;
            return;
        }

        try
        {
            // Use an ephemeral token for initial initialization. The token should be refreshed externally as needed.
            var token = tokenProvider(CancellationToken.None).GetAwaiter().GetResult();
            _config = SpeechConfig.FromAuthorizationToken(token, region);
            if (!string.IsNullOrEmpty(language))
                _config.SpeechRecognitionLanguage = language;

            var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            _recognizer = new SpeechRecognizer(_config, audioConfig);
            AttachEventHandlers();
            _configured = true;
        }
        catch (Exception ex)
        {
            RecognitionError?.Invoke(this, $"Failed to initialize Azure Speech (token): {ex.Message}");
            _configured = false;
        }
    }

    private void AttachEventHandlers()
    {
        if (_recognizer == null) return;

        _recognizer.Recognizing += (s, e) => SpeechDetected?.Invoke(this, EventArgs.Empty);

        _recognizer.Recognized += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech && !string.IsNullOrEmpty(e.Result.Text))
            {
                var r = MapRecognizedText(e.Result.Text);
                r.Text = e.Result.Text;
                SpeechRecognized?.Invoke(this, r);
            }
            else if (e.Result.Reason == ResultReason.NoMatch)
            {
                SpeechRejected?.Invoke(this, EventArgs.Empty);
            }
        };

        _recognizer.Canceled += (s, e) => RecognitionError?.Invoke(this, e.ErrorDetails ?? "Recognition canceled.");

        _recognizer.SessionStopped += (s, e) => RecognitionCompleted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Starts continuous recognition asynchronously.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (!_configured || _recognizer == null)
        {
            RecognitionError?.Invoke(this, "Cannot start AzureRecognizer: not configured.");
            return;
        }

        if (_cts != null)
            return; // already running

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            await _recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            RecognitionError?.Invoke(this, $"Recognition start error: {ex.Message}");
        }
    }

    /// <summary>
    /// Stops continuous recognition asynchronously.
    /// </summary>
    public async Task StopAsync()
    {
        if (_cts == null || _recognizer == null)
            return;

        try
        {
            _cts.Cancel();
            await _recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            RecognitionError?.Invoke(this, $"Recognition stop error: {ex.Message}");
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
        }
    }

    // Back-compat Start/Stop that call async variants
    public void Start() => _ = StartAsync();
    public void Stop() => _ = StopAsync();

    public void UpdateGrammar(IEnumerable<string> phrases, Dictionary<string, CommandType> commandMap)
    {
        // Update local map
        _commandMap.Clear();
        _phrases.Clear();

        foreach (var kv in commandMap)
        {
            _commandMap[kv.Key.ToString()] = kv.Value;
        }

        foreach (var p in phrases)
        {
            _phrases.Add(p);
        }

        if (_recognizer == null)
            return;

        try
        {
            var grammar = PhraseListGrammar.FromRecognizer(_recognizer);
            grammar.Clear();
            foreach (var p in _phrases)
                grammar.AddPhrase(p);
        }
        catch (Exception ex)
        {
            RecognitionError?.Invoke(this, $"Failed to update grammar: {ex.Message}");
        }
    }

    /// <summary>
    /// Protected mapping function that converts recognized text into a RecognitionResult.
    /// Can be overridden for tests.
    /// </summary>
    protected virtual RecognitionResult MapRecognizedText(string text)
    {
        var result = new RecognitionResult { Text = text };

        var found = new List<CommandType>();

        // Try simple exact matches first
        foreach (var p in _phrases)
        {
            if (string.Equals(p, text, StringComparison.OrdinalIgnoreCase))
            {
                if (_commandMap.TryGetValue(p, out var cmd))
                    found.Add(cmd);
            }
        }

        // If none found, try substring matches
        if (found.Count == 0)
        {
            foreach (var kv in _commandMap)
            {
                if (!string.IsNullOrEmpty(kv.Key) && text.IndexOf(kv.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    found.Add(kv.Value);
                }
            }
        }

        if (found.Count == 0)
        {
            result.Command = CommandType.Unknown;
            result.IsRejected = true;
        }
        else
        {
            result.Commands = found;
            result.Command = found[0];
            result.IsRejected = false;
        }

        return result;
    }

    public void Dispose()
    {
        try
        {
            _ = StopAsync();
            _recognizer?.Dispose();
            _config = null;
            _recognizer = null;
        }
        catch (Exception ex)
        {
            RecognitionError?.Invoke(this, $"Dispose error: {ex.Message}");
        }
    }
}
