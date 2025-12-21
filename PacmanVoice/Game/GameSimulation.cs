using System;
using System.Collections.Generic;

namespace PacmanVoice.Game;

/// <summary>
/// Core game simulation - all gameplay logic driven by simulation time
/// </summary>
public class GameSimulation
{
    private const int GridWidth = 28;
    private const int GridHeight = 31;
    private const double MoveInterval = 0.15; // seconds per grid cell
    private const double GhostSpeedMultiplierLevel2 = 0.85; // Ghosts move 15% faster on level 2
    private const double PowerUpDuration = 8.0; // Power-up lasts 8 seconds when fruit is eaten
    private const double RespawnDelay = 2.0; // Pause for 2 seconds after death before resuming

    // Classic-inspired: start on a safe corridor near the lower middle.
    private static readonly GridPosition PacmanStart = new(13, 23);

    // Ghosts begin inside the central "pen".
    private static readonly (GridPosition Position, Direction Direction)[] GhostStartStates =
    [
        (new GridPosition(12, 15), Direction.Left),
        (new GridPosition(13, 15), Direction.Right),
        (new GridPosition(14, 15), Direction.Left),
        (new GridPosition(13, 16), Direction.Right),
    ];

    private GameState _state = GameState.NotStarted;
    private GridPosition _pacmanPos;
    private Direction _currentDirection = Direction.None;
    private Direction _nextDirection = Direction.None;
    private readonly Queue<Direction> _directionQueue = new();
    private double _moveTimer;
    private int _score;
    private int _lives = 3;
    private int _currentLevel = 1;
    private int _totalPellets = 0;
    private int _pelletsEaten = 0;
    private bool[,] _pellets = new bool[GridWidth, GridHeight];
    private bool[,] _walls = new bool[GridWidth, GridHeight];
    private List<Ghost> _ghosts = new();
    
    // Power-up system fields
    private bool _isPowerUpActive = false;
    private double _powerUpTimer = 0;
    private int _ghostsEatenDuringPowerUp = 0;
    private GridPosition? _fruitPosition = null;
    private Random _fruitRandom = new();
    private const int FruitSpawnChance = 15; // Percentage chance to spawn fruit when eating a pellet
    
    // Respawn system
    private bool _isRespawning = false;
    private double _respawnTimer = 0;

    public event Action? PlayerDied;
    public event Action? PelletEaten;
    public event Action? GhostEaten;
    public event Action? LevelStart;
    public event Action? LevelCompleted;
    public event Action? PowerUpActivated;
    public event Action? PowerUpEnded;
    public event Action? FruitEaten;

    public GameState State => _state;
    public GridPosition PacmanPosition => _pacmanPos;
    public Direction CurrentDirection => _currentDirection;
    public int Score => _score;
    public int Lives => _lives;
    public int CurrentLevel => _currentLevel;
    public bool IsPowerUpActive => _isPowerUpActive;
    public double PowerUpTimeRemaining => _isPowerUpActive ? Math.Max(0, PowerUpDuration - _powerUpTimer) : 0;
    public GridPosition? FruitPosition => _fruitPosition;
    public int GhostsEatenDuringPowerUp => _ghostsEatenDuringPowerUp;
    public bool IsRespawning => _isRespawning;
    public double RespawnTimeRemaining => _isRespawning ? Math.Max(0, RespawnDelay - _respawnTimer) : 0;

    public GameSimulation()
    {
        InitializeLevel();
    }

    private void InitializeLevel()
    {
        if (_currentLevel == 1)
        {
            InitializeLevel1();
        }
        else if (_currentLevel == 2)
        {
            InitializeLevel2();
        }
        else
        {
            // Default to level 1 for any level > 2
            InitializeLevel1();
        }
    }

    private void InitializeLevel1()
    {
        // Classic-inspired maze: single-tile corridors surrounded by walls.
        // Note: this is intentionally *inspired by* the arcade feel, not a 1:1 copy.

        static void Carve(bool[,] walls, int x, int y)
        {
            walls[x, y] = false;
        }

        static void CarveH(bool[,] walls, int x1, int x2, int y)
        {
            if (x2 < x1) (x1, x2) = (x2, x1);
            for (int x = x1; x <= x2; x++) walls[x, y] = false;
        }

        static void CarveV(bool[,] walls, int x, int y1, int y2)
        {
            if (y2 < y1) (y1, y2) = (y2, y1);
            for (int y = y1; y <= y2; y++) walls[x, y] = false;
        }

        static void CarveRect(bool[,] walls, int x1, int y1, int x2, int y2)
        {
            if (x2 < x1) (x1, x2) = (x2, x1);
            if (y2 < y1) (y1, y2) = (y2, y1);
            for (int x = x1; x <= x2; x++)
                for (int y = y1; y <= y2; y++)
                    walls[x, y] = false;
        }

        // 1) Start with everything as a wall.
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                _walls[x, y] = true;
                _pellets[x, y] = false;
            }
        }

        // 2) Carve the outer ring corridor (inside the border) - single tile wide
        CarveH(_walls, 1, GridWidth - 2, 1);
        CarveH(_walls, 1, GridWidth - 2, GridHeight - 2);
        CarveV(_walls, 1, 1, GridHeight - 2);
        CarveV(_walls, GridWidth - 2, 1, GridHeight - 2);

        // 3) Carve a central spine and several horizontal lanes - all single tile paths.
        var cx = GridWidth / 2 - 1; // 13 on 28-wide grid
        CarveV(_walls, cx, 1, GridHeight - 2);

        // Horizontal lanes crossing the center - single tile corridors
        int[] horizontalPaths = [4, 8, 12, 18, 22, 26];

        foreach (var y in horizontalPaths)
        {
            CarveH(_walls, 1, GridWidth - 2, y);
        }

        // 4) Side vertical lanes - single tile wide only
        CarveV(_walls, 4, 1, GridHeight - 2);
        CarveV(_walls, GridWidth - 5, 1, GridHeight - 2);

        // 5) Additional connecting paths - all single tile wide
        CarveV(_walls, 7, 4, 8);
        CarveV(_walls, 7, 12, 18);
        CarveV(_walls, 7, 22, 26);
        CarveV(_walls, GridWidth - 8, 4, 8);
        CarveV(_walls, GridWidth - 8, 12, 18);
        CarveV(_walls, GridWidth - 8, 22, 26);

        // 6) Central ghost pen (open interior), connected to the spine via two doors.
        // Pen interior
        CarveRect(_walls, cx - 2, 14, cx + 2, 16);
        // Doors
        Carve(_walls, cx, 13);
        Carve(_walls, cx, 17);
        // Small buffer corridors around the pen
        CarveH(_walls, cx - 4, cx + 4, 13);
        CarveH(_walls, cx - 4, cx + 4, 17);

        // 7) Populate pellets on walkable tiles.
        for (int x = 1; x <= GridWidth - 2; x++)
        {
            for (int y = 1; y <= GridHeight - 2; y++)
            {
                if (!_walls[x, y])
                {
                    _pellets[x, y] = true;
                }
            }
        }

        // No pellets inside the ghost pen.
        for (int x = cx - 2; x <= cx + 2; x++)
        {
            for (int y = 14; y <= 16; y++)
            {
                _pellets[x, y] = false;
            }
        }

        // Starting position
        _pacmanPos = PacmanStart;
        _pellets[_pacmanPos.X, _pacmanPos.Y] = false;

        // Count total pellets
        _totalPellets = 0;
        _pelletsEaten = 0;
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                if (_pellets[x, y]) _totalPellets++;
            }
        }

        // Initialize ghosts
        _ghosts.Clear();
        foreach (var (pos, dir) in GhostStartStates)
        {
            _ghosts.Add(new Ghost(pos, dir));
        }
    }

    private void InitializeLevel2()
    {
        // Level 2 has a different maze layout - more complex with additional paths
        static void Carve(bool[,] walls, int x, int y)
        {
            walls[x, y] = false;
        }

        static void CarveH(bool[,] walls, int x1, int x2, int y)
        {
            if (x2 < x1) (x1, x2) = (x2, x1);
            for (int x = x1; x <= x2; x++) walls[x, y] = false;
        }

        static void CarveV(bool[,] walls, int x, int y1, int y2)
        {
            if (y2 < y1) (y1, y2) = (y2, y1);
            for (int y = y1; y <= y2; y++) walls[x, y] = false;
        }

        static void CarveRect(bool[,] walls, int x1, int y1, int x2, int y2)
        {
            if (x2 < x1) (x1, x2) = (x2, x1);
            if (y2 < y1) (y1, y2) = (y2, y1);
            for (int x = x1; x <= x2; x++)
                for (int y = y1; y <= y2; y++)
                    walls[x, y] = false;
        }

        // 1) Start with everything as a wall.
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                _walls[x, y] = true;
                _pellets[x, y] = false;
            }
        }

        // 2) Carve the outer ring corridor
        CarveH(_walls, 1, GridWidth - 2, 1);
        CarveH(_walls, 1, GridWidth - 2, GridHeight - 2);
        CarveV(_walls, 1, 1, GridHeight - 2);
        CarveV(_walls, GridWidth - 2, 1, GridHeight - 2);

        // 3) Different pattern - create a more maze-like structure
        var cx = GridWidth / 2 - 1; // 13 on 28-wide grid
        
        // Vertical lanes at different positions
        CarveV(_walls, 3, 1, GridHeight - 2);
        CarveV(_walls, 6, 1, GridHeight - 2);
        CarveV(_walls, 9, 1, GridHeight - 2);
        CarveV(_walls, GridWidth - 4, 1, GridHeight - 2);
        CarveV(_walls, GridWidth - 7, 1, GridHeight - 2);
        CarveV(_walls, GridWidth - 10, 1, GridHeight - 2);

        // Horizontal lanes at different positions
        int[] horizontalPaths = [3, 6, 10, 14, 19, 23, 27];
        foreach (var y in horizontalPaths)
        {
            CarveH(_walls, 1, GridWidth - 2, y);
        }

        // Create some additional zigzag patterns
        CarveV(_walls, 12, 3, 10);
        CarveV(_walls, 15, 3, 10);
        CarveV(_walls, 12, 14, 19);
        CarveV(_walls, 15, 14, 19);
        CarveV(_walls, 12, 23, 27);
        CarveV(_walls, 15, 23, 27);

        // Central ghost pen (same position as level 1)
        CarveRect(_walls, cx - 2, 14, cx + 2, 16);
        Carve(_walls, cx, 13);
        Carve(_walls, cx, 17);
        CarveH(_walls, cx - 4, cx + 4, 13);
        CarveH(_walls, cx - 4, cx + 4, 17);

        // 4) Populate pellets on walkable tiles.
        for (int x = 1; x <= GridWidth - 2; x++)
        {
            for (int y = 1; y <= GridHeight - 2; y++)
            {
                if (!_walls[x, y])
                {
                    _pellets[x, y] = true;
                }
            }
        }

        // No pellets inside the ghost pen.
        for (int x = cx - 2; x <= cx + 2; x++)
        {
            for (int y = 14; y <= 16; y++)
            {
                _pellets[x, y] = false;
            }
        }

        // Starting position
        _pacmanPos = PacmanStart;
        _pellets[_pacmanPos.X, _pacmanPos.Y] = false;

        // Count total pellets
        _totalPellets = 0;
        _pelletsEaten = 0;
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                if (_pellets[x, y]) _totalPellets++;
            }
        }

        // Initialize ghosts with faster speed on level 2
        _ghosts.Clear();
        foreach (var (pos, dir) in GhostStartStates)
        {
            _ghosts.Add(new Ghost(pos, dir, _currentLevel));
        }
    }

    public void Update(double deltaSeconds)
    {
        if (_state != GameState.Playing) return;

        // Handle respawn delay after death
        if (_isRespawning)
        {
            _respawnTimer += deltaSeconds;
            if (_respawnTimer >= RespawnDelay)
            {
                _isRespawning = false;
                _respawnTimer = 0;
                
                // Apply any queued direction that came in during respawn
                if (_directionQueue.Count > 0)
                {
                    _nextDirection = _directionQueue.Dequeue();
                }
            }
            return; // Don't process game logic during respawn
        }

        _moveTimer += deltaSeconds;
        
        // Update power-up timer
        if (_isPowerUpActive)
        {
            _powerUpTimer += deltaSeconds;
            if (_powerUpTimer >= PowerUpDuration)
            {
                _isPowerUpActive = false;
                _powerUpTimer = 0;
                _ghostsEatenDuringPowerUp = 0;
                PowerUpEnded?.Invoke();
            }
        }

        if (_moveTimer >= MoveInterval)
        {
            _moveTimer -= MoveInterval;

            // If there is no pending direction, peek the next queued one
            if (_nextDirection == Direction.None && _directionQueue.Count > 0)
            {
                _nextDirection = _directionQueue.Peek();
            }

            // Try to change direction at grid boundary
            if (_nextDirection != Direction.None && CanMove(_nextDirection))
            {
                _currentDirection = _nextDirection;
                _nextDirection = Direction.None;
                if (_directionQueue.Count > 0)
                {
                    _directionQueue.Dequeue();
                }
            }

            // Move pacman
            if (_currentDirection != Direction.None && CanMove(_currentDirection))
            {
                _pacmanPos = GetNextPosition(_pacmanPos, _currentDirection);

                // Check pellet collection
                if (_pellets[_pacmanPos.X, _pacmanPos.Y])
                {
                    _pellets[_pacmanPos.X, _pacmanPos.Y] = false;
                    _score += 10;
                    _pelletsEaten++;
                    PelletEaten?.Invoke();

                    // Chance to spawn fruit
                    if (_fruitPosition == null && _fruitRandom.Next(100) < FruitSpawnChance)
                    {
                        SpawnFruit();
                    }

                    // Check if level is complete
                    if (_pelletsEaten >= _totalPellets)
                    {
                        AdvanceLevel();
                    }
                }
                
                // Check fruit collection
                if (_fruitPosition.HasValue && _pacmanPos == _fruitPosition.Value)
                {
                    _fruitPosition = null;
                    _isPowerUpActive = true;
                    _powerUpTimer = 0;
                    _ghostsEatenDuringPowerUp = 0;
                    FruitEaten?.Invoke();
                    PowerUpActivated?.Invoke();
                }

                if (TryHandlePacmanGhostCollision())
                {
                    return;
                }
            }

            // Move ghosts
            foreach (var ghost in _ghosts)
            {
                ghost.Update(_walls, GridWidth, GridHeight);
            }

            // Also check collision after ghosts move (otherwise a ghost moving onto Pac-Man is ignored)
            if (TryHandlePacmanGhostCollision())
            {
                return;
            }
        }
    }

    private bool TryHandlePacmanGhostCollision()
    {
        foreach (var ghost in _ghosts)
        {
            if (ghost.Position != _pacmanPos)
            {
                continue;
            }

            if (_isPowerUpActive)
            {
                // Pacman eats the ghost during power-up
                EatGhost(ghost);
                return false;
            }
            else
            {
                // Ghost kills Pacman
                LoseLife();
                return true;
            }
        }

        return false;
    }
    
    private void EatGhost(Ghost ghost)
    {
        // Calculate bonus points: 200, 400, 800, 1600 for 1st through 4th ghost
        int[] bonusPoints = [200, 400, 800, 1600];
        int ghostIndex = Math.Min(_ghostsEatenDuringPowerUp, bonusPoints.Length - 1);
        int points = bonusPoints[ghostIndex];
        
        _score += points;
        _ghostsEatenDuringPowerUp++;
        ghost.ResetToStart();
        GhostEaten?.Invoke();
    }

    private void LoseLife()
    {
        _lives--;
        PlayerDied?.Invoke();

        if (_lives <= 0)
        {
            _state = GameState.GameOver;
            return;
        }

        ResetActorsAfterDeath();
        
        // Enter respawn state with delay
        _isRespawning = true;
        _respawnTimer = 0;
    }
    
    private void SpawnFruit()
    {
        // Find a random walkable position to spawn the fruit
        List<GridPosition> validPositions = new();
        for (int x = 1; x < GridWidth - 1; x++)
        {
            for (int y = 1; y < GridHeight - 1; y++)
            {
                if (!_walls[x, y])
                {
                    validPositions.Add(new GridPosition(x, y));
                }
            }
        }

        if (validPositions.Count > 0)
        {
            _fruitPosition = validPositions[_fruitRandom.Next(validPositions.Count)];
        }
    }

    private void ResetActorsAfterDeath()
    {
        _pacmanPos = PacmanStart;
        _currentDirection = Direction.None;
        _nextDirection = Direction.None;
        _directionQueue.Clear();
        _moveTimer = 0;
        _isPowerUpActive = false;
        _powerUpTimer = 0;
        _ghostsEatenDuringPowerUp = 0;
        _fruitPosition = null;

        foreach (var ghost in _ghosts)
        {
            ghost.ResetToStart();
        }
    }

    private void AdvanceLevel()
    {
        _currentLevel++;
        LevelCompleted?.Invoke();
        _currentDirection = Direction.None;
        _nextDirection = Direction.None;
        _directionQueue.Clear();
        _moveTimer = 0;
        _isPowerUpActive = false;
        _powerUpTimer = 0;
        _ghostsEatenDuringPowerUp = 0;
        _fruitPosition = null;
        _isRespawning = false;
        _respawnTimer = 0;
        InitializeLevel();
        LevelStart?.Invoke();
    }

    public double GetGhostMoveInterval()
    {
        if (_currentLevel == 2)
        {
            return MoveInterval * GhostSpeedMultiplierLevel2;
        }
        return MoveInterval;
    }

    public void SetDirection(Direction direction)
    {
        // If a direction is already pending for the next move tick,
        // queue this request to be applied on subsequent ticks.
        if (_nextDirection == Direction.None)
        {
            _nextDirection = direction;
        }
        else
        {
            _directionQueue.Enqueue(direction);
        }
    }

    public void Begin()
    {
        _state = GameState.Playing;
        _currentDirection = Direction.None;
        _nextDirection = Direction.None;
        _directionQueue.Clear();
        _moveTimer = 0;
        _isRespawning = false;
        _respawnTimer = 0;
        LevelStart?.Invoke();
    }

    public void Pause()
    {
        if (_state == GameState.Playing)
            _state = GameState.Paused;
    }

    public void Resume()
    {
        if (_state == GameState.Paused)
            _state = GameState.Playing;
    }

    public void Restart()
    {
        _score = 0;
        _lives = 3;
        _currentLevel = 1;
        _moveTimer = 0;
        _currentDirection = Direction.None;
        _nextDirection = Direction.None;
        _directionQueue.Clear();
        _isPowerUpActive = false;
        _powerUpTimer = 0;
        _ghostsEatenDuringPowerUp = 0;
        _fruitPosition = null;
        _isRespawning = false;
        _respawnTimer = 0;
        InitializeLevel();
        _state = GameState.Playing;
        
        // Ensure voice recognizer is also reset on restart
        // This will trigger a reset via the game's update loop
        OnRestartTriggered?.Invoke();
    }
    
    public event Action? OnRestartTriggered;

    public void EnterQuitConfirmation()
    {
        _state = GameState.QuitConfirmation;
    }

    public void CancelQuitConfirmation()
    {
        _state = GameState.Playing;
    }

    public string GetStatus()
    {
        return $"Score: {_score}, Lives: {_lives}, State: {_state}";
    }

    private bool CanMove(Direction direction)
    {
        var nextPos = GetNextPosition(_pacmanPos, direction);
        return IsValidPosition(nextPos) && !_walls[nextPos.X, nextPos.Y];
    }

    private GridPosition GetNextPosition(GridPosition pos, Direction direction)
    {
        return direction switch
        {
            Direction.Up => new GridPosition(pos.X, pos.Y - 1),
            Direction.Down => new GridPosition(pos.X, pos.Y + 1),
            Direction.Left => new GridPosition(pos.X - 1, pos.Y),
            Direction.Right => new GridPosition(pos.X + 1, pos.Y),
            _ => pos
        };
    }

    private bool IsValidPosition(GridPosition pos)
    {
        return pos.X >= 0 && pos.X < GridWidth && pos.Y >= 0 && pos.Y < GridHeight;
    }

    public bool[,] GetWalls() => _walls;
    public bool[,] GetPellets() => _pellets;
    public IReadOnlyList<Ghost> GetGhosts() => _ghosts;
    public (int Width, int Height) GetGridSize() => (GridWidth, GridHeight);
    public GridPosition? GetFruitPosition() => _fruitPosition;
}

/// <summary>
/// Simple ghost enemy
/// </summary>
public class Ghost
{
    private GridPosition _position;
    private Direction _direction;
    private readonly GridPosition _startPosition;
    private readonly Direction _startDirection;
    private readonly int _level;
    private Random _random = new();

    public GridPosition Position => _position;
    public int Level => _level;

    public Ghost(GridPosition startPos, Direction startDir, int level = 1)
    {
        _position = startPos;
        _direction = startDir;
        _level = level;

        _startPosition = startPos;
        _startDirection = startDir;
    }

    public void ResetToStart()
    {
        _position = _startPosition;
        _direction = _startDirection;
    }

    public void Update(bool[,] walls, int gridWidth, int gridHeight)
    {
        // Simple AI: try to continue in current direction, if blocked pick a random valid direction
        var nextPos = GetNextPosition(_position, _direction);

        if (!IsValidMove(nextPos, walls, gridWidth, gridHeight))
        {
            // Pick a random valid direction
            var validDirections = new List<Direction>();
            foreach (Direction dir in Enum.GetValues(typeof(Direction)))
            {
                if (dir == Direction.None) continue;
                var testPos = GetNextPosition(_position, dir);
                if (IsValidMove(testPos, walls, gridWidth, gridHeight))
                {
                    validDirections.Add(dir);
                }
            }

            if (validDirections.Count > 0)
            {
                _direction = validDirections[_random.Next(validDirections.Count)];
                nextPos = GetNextPosition(_position, _direction);
            }
        }

        if (IsValidMove(nextPos, walls, gridWidth, gridHeight))
        {
            _position = nextPos;
        }
    }

    private GridPosition GetNextPosition(GridPosition pos, Direction direction)
    {
        return direction switch
        {
            Direction.Up => new GridPosition(pos.X, pos.Y - 1),
            Direction.Down => new GridPosition(pos.X, pos.Y + 1),
            Direction.Left => new GridPosition(pos.X - 1, pos.Y),
            Direction.Right => new GridPosition(pos.X + 1, pos.Y),
            _ => pos
        };
    }

    private bool IsValidMove(GridPosition pos, bool[,] walls, int gridWidth, int gridHeight)
    {
        return pos.X >= 0 && pos.X < gridWidth && pos.Y >= 0 && pos.Y < gridHeight && !walls[pos.X, pos.Y];
    }
}
