using System.Collections.Generic;

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

    // Game over commands
    StartNewGame,
    QuitFromGameOver,

    // Unknown/rejected
    Unknown
}

/// <summary>
/// Result of command recognition
/// </summary>
public class RecognitionResult
{
    public CommandType Command { get; set; }
    /// <summary>
    /// If provided, represents one or more recognized commands in order.
    /// For single-command recognitions, this will typically contain one item.
    /// </summary>
    public List<CommandType> Commands { get; set; } = new();
    public float Confidence { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsRejected { get; set; }
}
