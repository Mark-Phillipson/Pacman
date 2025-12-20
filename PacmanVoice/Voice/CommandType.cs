namespace PacmanVoice.Voice;

/// <summary>
/// Recognized command types
/// </summary>
public enum CommandType
{
    // Directions
    Up,
    Down,
    Left,
    Right,

    // Game control
    BeginGame,
    PauseGame,
    ResumeGame,
    RestartGame,
    GameStatus,
    RepeatThat,
    QuitGame,
    QuitConfirm,
    NeverMind,

    // Unknown/rejected
    Unknown
}

/// <summary>
/// Result of command recognition
/// </summary>
public class RecognitionResult
{
    public CommandType Command { get; set; }
    public float Confidence { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsRejected { get; set; }
}
