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

    private GameState _state = GameState.NotStarted;
    private GridPosition _pacmanPos;
    private Direction _currentDirection = Direction.None;
    private Direction _nextDirection = Direction.None;
    private double _moveTimer;
    private int _score;
    private int _lives = 3;
    private bool[,] _pellets = new bool[GridWidth, GridHeight];
    private bool[,] _walls = new bool[GridWidth, GridHeight];
    private List<Ghost> _ghosts = new();

    public GameState State => _state;
    public GridPosition PacmanPosition => _pacmanPos;
    public Direction CurrentDirection => _currentDirection;
    public int Score => _score;
    public int Lives => _lives;

    public GameSimulation()
    {
        InitializeLevel();
    }

    private void InitializeLevel()
    {
        // Simple maze layout - walls around edges, some interior walls
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                // Border walls
                if (x == 0 || x == GridWidth - 1 || y == 0 || y == GridHeight - 1)
                {
                    _walls[x, y] = true;
                    _pellets[x, y] = false;
                }
                else
                {
                    _walls[x, y] = false;
                    _pellets[x, y] = true; // Place pellets everywhere except walls
                }
            }
        }

        // Add some interior walls for interest
        for (int x = 5; x < 10; x++)
        {
            _walls[x, 10] = true;
            _pellets[x, 10] = false;
            _walls[x, 20] = true;
            _pellets[x, 20] = false;
        }

        for (int x = 18; x < 23; x++)
        {
            _walls[x, 10] = true;
            _pellets[x, 10] = false;
            _walls[x, 20] = true;
            _pellets[x, 20] = false;
        }

        // Starting position
        _pacmanPos = new GridPosition(GridWidth / 2, GridHeight / 2);
        _pellets[_pacmanPos.X, _pacmanPos.Y] = false;

        // Initialize ghosts
        _ghosts.Clear();
        _ghosts.Add(new Ghost(new GridPosition(5, 5), Direction.Right));
        _ghosts.Add(new Ghost(new GridPosition(GridWidth - 6, 5), Direction.Left));
        _ghosts.Add(new Ghost(new GridPosition(5, GridHeight - 6), Direction.Right));
        _ghosts.Add(new Ghost(new GridPosition(GridWidth - 6, GridHeight - 6), Direction.Left));
    }

    public void Update(double deltaSeconds)
    {
        if (_state != GameState.Playing) return;

        _moveTimer += deltaSeconds;

        if (_moveTimer >= MoveInterval)
        {
            _moveTimer -= MoveInterval;

            // Try to change direction if requested
            if (_nextDirection != Direction.None && CanMove(_nextDirection))
            {
                _currentDirection = _nextDirection;
                _nextDirection = Direction.None;
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
                }

                // Check ghost collision
                foreach (var ghost in _ghosts)
                {
                    if (ghost.Position == _pacmanPos)
                    {
                        _lives--;
                        if (_lives <= 0)
                        {
                            _state = GameState.GameOver;
                        }
                        else
                        {
                            // Reset position
                            _pacmanPos = new GridPosition(GridWidth / 2, GridHeight / 2);
                            _currentDirection = Direction.None;
                        }
                        break;
                    }
                }
            }

            // Move ghosts
            foreach (var ghost in _ghosts)
            {
                ghost.Update(_walls, GridWidth, GridHeight);
            }
        }
    }

    public void SetDirection(Direction direction)
    {
        _nextDirection = direction;
    }

    public void Begin()
    {
        _state = GameState.Playing;
        _currentDirection = Direction.None;
        _nextDirection = Direction.None;
        _moveTimer = 0;
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
        _moveTimer = 0;
        _currentDirection = Direction.None;
        _nextDirection = Direction.None;
        InitializeLevel();
        _state = GameState.Playing;
    }

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
}

/// <summary>
/// Simple ghost enemy
/// </summary>
public class Ghost
{
    private GridPosition _position;
    private Direction _direction;
    private Random _random = new();

    public GridPosition Position => _position;

    public Ghost(GridPosition startPos, Direction startDir)
    {
        _position = startPos;
        _direction = startDir;
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
