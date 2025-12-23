using PacmanVoice.Voice;
using Microsoft.Xna.Framework;
using System;
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
    private double _dispatchCooldownSeconds = 0;

    public string LastStatus => _lastStatus;

    public CommandRouter(GameSimulation simulation, Microsoft.Xna.Framework.Game game)
    {
        _simulation = simulation;
        _game = game;
    }

    public void ClearQueuedCommands()
    {
        _queuedCommands.Clear();
        _dispatchCooldownSeconds = 0;
    }

    /// <summary>
    /// Runs at most one queued command. Call once per frame/tick.
    /// </summary>
    public void Update(double deltaSeconds)
    {
        if (_dispatchCooldownSeconds > 0)
        {
            _dispatchCooldownSeconds -= deltaSeconds;
        }

        if (_queuedCommands.Count == 0) return;

        if (_dispatchCooldownSeconds > 0) return;

        DispatchNextQueuedCommand();
    }

    public void HandleCommand(RecognitionResult result)
    {
        // If multiple commands were recognized (typically a direction sequence), queue them.
        if (result.Commands is { Count: > 1 })
        {
            Console.WriteLine($"Queuing {result.Commands.Count} direction commands");
            foreach (var cmd in result.Commands)
            {
                _queuedCommands.Enqueue(cmd);
            }

            // Dispatch the first one immediately, then throttle the rest to the move interval
            // so they are applied one per grid step instead of all in the same frame.
            DispatchNextQueuedCommand();
            return;
        }

        HandleCommandInternal(result);
    }

    private void DispatchNextQueuedCommand()
    {
        if (_queuedCommands.Count == 0) return;

        var next = _queuedCommands.Dequeue();
        _dispatchCooldownSeconds = _simulation.MoveIntervalSeconds;
        HandleCommandInternal(new RecognitionResult { Command = next, Commands = new List<CommandType> { next } });
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

        // Handle game over state
        if (state == GameState.GameOver)
        {
            switch (result.Command)
            {
                case CommandType.StartNewGame:
                    _simulation.Restart();
                    break;
                case CommandType.QuitFromGameOver:
                    _game.Exit();
                    break;
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

            case CommandType.StartNewGame:
                // Only valid during game over state (handled above)
                break;

            case CommandType.QuitFromGameOver:
                // Only valid during game over state (handled above)
                break;
        }
    }
}
