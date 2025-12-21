using System.Speech.Recognition;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

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

    private const int MaxDirectionSequenceLength = 8;
    private static readonly string[] DirectionConnectors = ["and", "then", "next"];

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

            var directionPhrases = _commandMap
                .Where(kvp => IsDirection(kvp.Value))
                .Select(kvp => kvp.Key)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var nonDirectionPhrases = _commandMap
                .Where(kvp => !IsDirection(kvp.Value))
                .Select(kvp => kvp.Key)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            // Grammar 1: Non-direction commands (single phrase)
            if (nonDirectionPhrases.Length > 0)
            {
                var cmdChoices = new Choices(nonDirectionPhrases);
                var cmdGrammar = new Grammar(new GrammarBuilder(cmdChoices))
                {
                    Name = "NonDirectionCommands"
                };
                _recognizer.LoadGrammar(cmdGrammar);
            }

            // Grammar 2: Direction sequences without connectors (e.g., "up left down")
            if (directionPhrases.Length > 0)
            {
                var dirChoices = new Choices(directionPhrases);
                var seqNoConnector = new GrammarBuilder();
                seqNoConnector.Append(dirChoices, 1, MaxDirectionSequenceLength);
                var dirSeqGrammar = new Grammar(seqNoConnector)
                {
                    Name = "DirectionSequence"
                };
                _recognizer.LoadGrammar(dirSeqGrammar);

                // Grammar 3: Direction sequences with connectors (e.g., "up and left then down")
                var connectorChoices = new Choices(DirectionConnectors);
                var seqWithConnectors = new GrammarBuilder();
                seqWithConnectors.Append(dirChoices);

                var tail = new GrammarBuilder();
                tail.Append(connectorChoices);
                tail.Append(dirChoices);
                seqWithConnectors.Append(tail, 0, MaxDirectionSequenceLength - 1);

                var dirSeqWithConnectorsGrammar = new Grammar(seqWithConnectors)
                {
                    Name = "DirectionSequenceWithConnectors"
                };
                _recognizer.LoadGrammar(dirSeqWithConnectorsGrammar);
            }
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

        // Map recognized text to command(s)
        List<CommandType> commands;
        CommandType commandType;

        if (_commandMap.TryGetValue(text, out var cmd))
        {
            commandType = cmd;
            commands = new List<CommandType> { cmd };
        }
        else if (TryParseDirectionSequence(text, out var parsedDirections))
        {
            commands = parsedDirections;
            commandType = commands[0];
        }
        else
        {
            commandType = CommandType.Unknown;
            commands = new List<CommandType> { CommandType.Unknown };
        }

        var result = new RecognitionResult
        {
            Command = commandType,
            Commands = commands,
            Confidence = confidence,
            Text = text,
            IsRejected = false
        };

        SpeechRecognized?.Invoke(this, result);
    }

    private static bool IsDirection(CommandType commandType) =>
        commandType is CommandType.Up or CommandType.Down or CommandType.Left or CommandType.Right;

    private bool TryParseDirectionSequence(string text, out List<CommandType> commands)
    {
        commands = new List<CommandType>();

        if (string.IsNullOrWhiteSpace(text) || _commandMap.Count == 0)
        {
            return false;
        }

        // 1) If connectors are present, split on them and map each segment
        var connectorPattern = $@"\\b(?:{string.Join("|", DirectionConnectors.Select(Regex.Escape))})\\b";
        var parts = Regex
            .Split(text, connectorPattern, RegexOptions.IgnoreCase)
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToList();

        if (parts.Count > 1)
        {
            foreach (var part in parts)
            {
                if (!_commandMap.TryGetValue(part, out var cmd) || !IsDirection(cmd))
                {
                    commands.Clear();
                    return false;
                }

                commands.Add(cmd);
                if (commands.Count >= MaxDirectionSequenceLength) break;
            }

            return commands.Count > 1;
        }

        // 2) No connectors: greedily match configured direction phrases from left to right
        // This supports single-word and multi-word direction phrases.
        var directionPhrasePairs = _commandMap
            .Where(kvp => IsDirection(kvp.Value))
            .Select(kvp => (Phrase: kvp.Key, Command: kvp.Value))
            .Distinct()
            .OrderByDescending(x => x.Phrase.Length)
            .ToList();

        if (directionPhrasePairs.Count == 0)
        {
            return false;
        }

        var remaining = NormalizeSpaces(text);

        while (remaining.Length > 0 && commands.Count < MaxDirectionSequenceLength)
        {
            // Strip any leading connectors (in case recognition included them)
            var trimmed = StripLeadingConnector(remaining);
            if (!ReferenceEquals(trimmed, remaining))
            {
                remaining = trimmed;
                continue;
            }

            var matched = false;
            foreach (var (phrase, cmd) in directionPhrasePairs)
            {
                if (StartsWithWholePhrase(remaining, phrase))
                {
                    commands.Add(cmd);
                    remaining = NormalizeSpaces(remaining.Substring(phrase.Length));
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                commands.Clear();
                return false;
            }
        }

        return commands.Count > 1;
    }

    private static string NormalizeSpaces(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        return Regex.Replace(text.Trim(), @"\\s+", " ");
    }

    private static string StripLeadingConnector(string text)
    {
        var normalized = NormalizeSpaces(text);
        foreach (var c in DirectionConnectors)
        {
            if (normalized.StartsWith(c + " ", StringComparison.OrdinalIgnoreCase))
            {
                return NormalizeSpaces(normalized.Substring(c.Length));
            }
        }

        return text;
    }

    private static bool StartsWithWholePhrase(string text, string phrase)
    {
        if (text.Length < phrase.Length) return false;
        if (!text.StartsWith(phrase, StringComparison.OrdinalIgnoreCase)) return false;
        if (text.Length == phrase.Length) return true;
        return char.IsWhiteSpace(text[phrase.Length]);
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
