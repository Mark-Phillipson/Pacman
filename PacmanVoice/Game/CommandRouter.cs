using PacmanVoice.Voice;
using Microsoft.Xna.Framework;

namespace PacmanVoice.Game;

/// <summary>
/// Routes recognized commands to game actions
/// </summary>
public class CommandRouter
{
    private readonly GameSimulation _simulation;
    private readonly Microsoft.Xna.Framework.Game _game;
    private string _lastStatus = string.Empty;

    public string LastStatus => _lastStatus;

    public CommandRouter(GameSimulation simulation, Microsoft.Xna.Framework.Game game)
    {
        _simulation = simulation;
        _game = game;
    }

    public void HandleCommand(RecognitionResult result)
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
                _simulation.SetDirection(Direction.Up);
                break;

            case CommandType.Down:
                _simulation.SetDirection(Direction.Down);
                break;

            case CommandType.Left:
                _simulation.SetDirection(Direction.Left);
                break;

            case CommandType.Right:
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
