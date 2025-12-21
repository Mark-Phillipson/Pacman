using System;
using System.Collections.Generic;

namespace PacmanVoice.Voice;

/// <summary>
/// Abstraction for speech recognition engines
/// </summary>
public interface IRecognizer
{
    event EventHandler? SpeechDetected;
    event EventHandler<RecognitionResult>? SpeechRecognized;
    event EventHandler? SpeechRejected;
    event EventHandler? RecognitionCompleted;
    event EventHandler<string>? RecognitionError;

    void Start();
    void Stop();
    void UpdateGrammar(IEnumerable<string> phrases, Dictionary<string, CommandType> commandMap);
}
