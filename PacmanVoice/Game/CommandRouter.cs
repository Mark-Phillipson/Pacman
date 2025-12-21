using PacmanVoice.Voice;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace PacmanVoice.Game;

/// <summary>
/// Routes recognized commands to game actions
/// </summary>
public class CommandRouter
{
    private readonly GameSimulation _simulation;
    private readonly Microsoft.Xna.Framework.Game _game;
    private string _lastStatus = string.Empty;
    private readonly Queue<CommandType> _queuedCommands = new();

    public string LastStatus => _lastStatus;

    public CommandRouter(GameSimulation simulation, Microsoft.Xna.Framework.Game game)
    {
        _simulation = simulation;
        _game = game;
    }

    /// <summary>
    /// Runs at most one queued command. Call once per frame/tick.
    /// </summary>
    public void Update()
    {
        if (_queuedCommands.Count == 0) return;

        var next = _queuedCommands.Dequeue();
        HandleCommandInternal(new RecognitionResult { Command = next, Commands = new List<CommandType> { next } });
    }

    public void HandleCommand(RecognitionResult result)
    {
        // If multiple commands were recognized (typically a direction sequence), queue them.
        if (result.Commands is { Count: > 1 })
        {
            foreach (var cmd in result.Commands)
            {
                _queuedCommands.Enqueue(cmd);
            }
            return;
        }

        HandleCommandInternal(result);
    }

    private void HandleCommandInternal(RecognitionResult result)
    {
        // Handle commands based on current game state
        var state = _simulation.State;

        if (state == GameState.QuitConfirmation)
        {
            // In quit confirmation, only accept quit confirm or never mind
            if (result.Command == CommandType.QuitConfirm)
            {
                _game.Exit();
            }
            else if (result.Command == CommandType.NeverMind)
            {
                _simulation.CancelQuitConfirmation();
            }
            return;
        }

        // Handle commands based on type
        switch (result.Command)
        {
            case CommandType.Up:
                if (state == GameState.Playing)
                    _simulation.SetDirection(Direction.Up);
                break;

            case CommandType.Down:
                if (state == GameState.Playing)
                    _simulation.SetDirection(Direction.Down);
                break;

            case CommandType.Left:
                if (state == GameState.Playing)
                    _simulation.SetDirection(Direction.Left);
                break;

            case CommandType.Right:
                if (state == GameState.Playing)
                    _simulation.SetDirection(Direction.Right);
                break;

            case CommandType.BeginGame:
                if (state == GameState.NotStarted)
                {
                    _simulation.Begin();
                }
                break;

            case CommandType.PauseGame:
                if (state == GameState.Playing)
                {
                    _simulation.Pause();
                }
                break;

            case CommandType.ResumeGame:
                if (state == GameState.Paused)
                {
                    _simulation.Resume();
                }
                break;

            case CommandType.RestartGame:
                _simulation.Restart();
                break;

            case CommandType.GameStatus:
                _lastStatus = _simulation.GetStatus();
                break;

            case CommandType.RepeatThat:
                // Status was already saved, just display it again
                break;

            case CommandType.QuitGame:
                _simulation.EnterQuitConfirmation();
                break;

            case CommandType.NeverMind:
                // Can be used to dismiss status messages or other UI states
                _lastStatus = string.Empty;
                break;
        }
    }
}
