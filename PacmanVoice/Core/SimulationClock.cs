namespace PacmanVoice.Core;

/// <summary>
/// Manages game time with support for pausing and freezing.
/// When frozen or paused, no simulation time accumulates.
/// </summary>
public class SimulationClock
{
    private double _accumulatedTime;
    private bool _userPaused;
    private bool _listeningFrozen;

    public double TotalSimulationTime => _accumulatedTime;
    public bool IsUserPaused => _userPaused;
    public bool IsListeningFrozen => _listeningFrozen;
    public bool IsRunning => !_userPaused && !_listeningFrozen;

    public void Update(double deltaSeconds)
    {
        if (IsRunning)
        {
            _accumulatedTime += deltaSeconds;
        }
    }

    public void SetUserPaused(bool paused)
    {
        _userPaused = paused;
    }

    public void SetListeningFrozen(bool frozen)
    {
        _listeningFrozen = frozen;
    }

    public void Reset()
    {
        _accumulatedTime = 0;
    }
}
