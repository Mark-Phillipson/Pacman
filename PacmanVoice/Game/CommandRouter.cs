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
    // Junction-aware dispatch: we no longer use a fixed cooldown. We only
    // dispatch the next queued command when the requested turn is possible
    // and there is no pending direction request in the simulation.

    public string LastStatus => _lastStatus;

    public CommandRouter(GameSimulation simulation, Microsoft.Xna.Framework.Game game)
    {
        _simulation = simulation;
        _game = game;
    }

    public void ClearQueuedCommands()
    {
        _queuedCommands.Clear();
    }

    /// <summary>
    /// Runs at most one queued command. Call once per frame/tick.
    /// </summary>
    public void Update(double deltaSeconds)
    {
        if (_queuedCommands.Count == 0) return;
        if (_simulation.State != GameState.Playing) return;

        // If a previous SetDirection request is still pending, wait
        if (_simulation.HasPendingDirectionRequest) return;

        // Peek next queued command and only dispatch when that turn is possible
        var nextCmd = _queuedCommands.Peek();
        var dir = ToDirection(nextCmd);
        if (dir != Direction.None && _simulation.CanAcceptDirection(dir))
        {
            _queuedCommands.Dequeue();
            _simulation.SetDirection(dir);
        }
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
            // Do not dispatch immediately; Update() will apply each when the turn is possible
            return;
        }

        HandleCommandInternal(result);
    }

    private Direction ToDirection(CommandType cmd)
    {
        return cmd switch
        {
            CommandType.Up => Direction.Up,
            CommandType.Down => Direction.Down,
            CommandType.Left => Direction.Left,
            CommandType.Right => Direction.Right,
            _ => Direction.None
        };
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

        // Handle batch mode state
        if (state == GameState.BatchMode)
        {
            switch (result.Command)
            {
                case CommandType.Up:
                    _simulation.AddBatchDirection(Direction.Up);
                    break;
                case CommandType.Down:
                    _simulation.AddBatchDirection(Direction.Down);
                    break;
                case CommandType.Left:
                    _simulation.AddBatchDirection(Direction.Left);
                    break;
                case CommandType.Right:
                    _simulation.AddBatchDirection(Direction.Right);
                    break;
                case CommandType.ApplyBatch:
                    _simulation.ApplyBatch();
                    // Queue all batch directions for execution; Update() will dispatch at junctions
                    foreach (var dir in _simulation.BatchDirections)
                    {
                        var dirCmd = dir switch
                        {
                            Direction.Up => CommandType.Up,
                            Direction.Down => CommandType.Down,
                            Direction.Left => CommandType.Left,
                            Direction.Right => CommandType.Right,
                            _ => CommandType.Unknown
                        };
                        if (dirCmd != CommandType.Unknown)
                        {
                            _queuedCommands.Enqueue(dirCmd);
                        }
                    }
                    _simulation.BatchDirections.Clear();
                    break;
                case CommandType.NeverMind:
                    // Exit batch mode without applying
                    _simulation.ExitBatchMode();
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
                Console.WriteLine($"[CommandRouter] BeginGame command, state={state}");
                if (state == GameState.NotStarted)
                {
                    Console.WriteLine($"[CommandRouter] Calling _simulation.Begin()");
                    _simulation.Begin();
                    Console.WriteLine($"[CommandRouter] _simulation.Begin() returned, state now={_simulation.State}");
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
                // Only allow quitting from active gameplay
                if (state == GameState.Playing || state == GameState.Paused)
                {
                    _simulation.EnterQuitConfirmation();
                }
                break;

            case CommandType.NeverMind:
                // Can be used to dismiss status messages or other UI states
                _lastStatus = string.Empty;
                break;

            case CommandType.BatchEntry:
                if (state == GameState.Playing)
                {
                    _simulation.EnterBatchMode();
                }
                break;

            case CommandType.ApplyBatch:
                // Only valid in batch mode (handled above)
                break;

            case CommandType.SpeedUp:
                _simulation.GameSpeedMultiplier = Math.Min(_simulation.GameSpeedMultiplier * 1.2, 3.0);
                break;

            case CommandType.SlowDown:
                _simulation.GameSpeedMultiplier = Math.Max(_simulation.GameSpeedMultiplier / 1.2, 0.1);
                break;

            case CommandType.NormalSpeed:
                _simulation.GameSpeedMultiplier = 1.0;
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
