using System;

namespace PacmanVoice.Game;

/// <summary>
/// Game state
/// </summary>
public enum GameState
{
    NotStarted,
    Playing,
    Paused,
    QuitConfirmation,
    GameOver
}

/// <summary>
/// Direction for movement
/// </summary>
public enum Direction
{
    None,
    Up,
    Down,
    Left,
    Right
}

/// <summary>
/// Position on the game grid
/// </summary>
public struct GridPosition
{
    public int X { get; set; }
    public int Y { get; set; }

    public GridPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static GridPosition operator +(GridPosition a, GridPosition b)
    {
        return new GridPosition(a.X + b.X, a.Y + b.Y);
    }

    public static bool operator ==(GridPosition a, GridPosition b)
    {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(GridPosition a, GridPosition b)
    {
        return !(a == b);
    }

    public override bool Equals(object? obj)
    {
        return obj is GridPosition other && this == other;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}
