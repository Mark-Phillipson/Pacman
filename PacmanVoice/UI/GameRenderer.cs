using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace PacmanVoice.UI;

/// <summary>
/// Renders the game board and entities
/// </summary>
public class GameRenderer
{
    private const int CellSize = 20;
    private Texture2D? _pixelTexture;
    private readonly Game.GameSimulation _simulation;
    
    // Sprite textures
    private Dictionary<string, Texture2D?> _pacmanTextures = new();
    private Texture2D? _ghostRedTexture;
    private Texture2D? _ghostPinkTexture;
    private Texture2D? _ghostCyanTexture;
    private Texture2D? _ghostOrangeTexture;
    private Dictionary<string, Texture2D?> _fruitTextures = new();
    
    // Animation tracking
    private double _animationTimer = 0.0;
    private const double AnimationFrameDuration = 0.1; // 100ms per frame
    private const int AnimationFrameCount = 3; // 0: closed, 1: slightly open, 2: wide open

    public GameRenderer(Game.GameSimulation simulation)
    {
        _simulation = simulation;
    }

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        // Create a 1x1 white pixel texture for drawing
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
        
        // Load sprite textures
        try
        {
            // Load Pacman animation frames for all directions
            string[] directions = { "right", "left", "up", "down" };
            foreach (var direction in directions)
            {
                for (int frame = 0; frame < AnimationFrameCount; frame++)
                {
                    string key = $"{direction}_{frame}";
                    try
                    {
                        _pacmanTextures[key] = content.Load<Texture2D>($"Sprites/Pacman/pacman_{direction}_{frame}");
                    }
                    catch
                    {
                        // Fallback to static sprite if animation frames not available
                        _pacmanTextures[key] = null;
                    }
                }
            }
            
            _ghostRedTexture = content.Load<Texture2D>("Sprites/Ghosts/ghost_red");
            _ghostPinkTexture = content.Load<Texture2D>("Sprites/Ghosts/ghost_pink");
            _ghostCyanTexture = content.Load<Texture2D>("Sprites/Ghosts/ghost_cyan");
            _ghostOrangeTexture = content.Load<Texture2D>("Sprites/Ghosts/ghost_orange");
            
            // Load fruit textures
            string[] fruits = { "cherry", "strawberry", "orange", "apple", "melon", "banana", "grape" };
            foreach (var fruit in fruits)
            {
                try
                {
                    _fruitTextures[fruit] = content.Load<Texture2D>($"Sprites/Fruits/{fruit}");
                }
                catch { /* Fruit texture not available */ }
            }
        }
        catch
        {
            // If sprites aren't available, we'll fall back to colored rectangles
        }
    }
    
    public void Update(double deltaSeconds)
    {
        // Update animation timer
        _animationTimer += deltaSeconds;
        if (_animationTimer >= AnimationFrameDuration * AnimationFrameCount)
        {
            _animationTimer -= AnimationFrameDuration * AnimationFrameCount;
        }
    }

    public void Draw(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
    {
        Draw(spriteBatch, new Rectangle(0, 0, screenWidth, screenHeight));
    }

    public void Draw(SpriteBatch spriteBatch, Rectangle playfieldArea)
    {
        if (_pixelTexture == null) return;

        var (gridWidth, gridHeight) = _simulation.GetGridSize();
        var walls = _simulation.GetWalls();
        var pellets = _simulation.GetPellets();
        var ghosts = _simulation.GetGhosts();

        // Center the grid inside the playfield (keeps HUD separate)
        var offsetX = playfieldArea.X + (playfieldArea.Width - gridWidth * CellSize) / 2;
        var offsetY = playfieldArea.Y + (playfieldArea.Height - gridHeight * CellSize) / 2;

        // Draw walls
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (walls[x, y])
                {
                    var rect = new Rectangle(offsetX + x * CellSize, offsetY + y * CellSize, CellSize, CellSize);
                    spriteBatch.Draw(_pixelTexture, rect, Color.Blue);
                }
            }
        }

        // Draw pellets (small white dots)
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (pellets[x, y])
                {
                    var pelletSize = 4;
                    var rect = new Rectangle(
                        offsetX + x * CellSize + (CellSize - pelletSize) / 2,
                        offsetY + y * CellSize + (CellSize - pelletSize) / 2,
                        pelletSize,
                        pelletSize
                    );
                    spriteBatch.Draw(_pixelTexture, rect, Color.White);
                }
            }
        }

        // Draw ghosts with sprites or fallback to colored rectangles
        int ghostIndex = 0;
        foreach (var ghost in ghosts)
        {
            var ghostRect = new Rectangle(
                offsetX + ghost.Position.X * CellSize,
                offsetY + ghost.Position.Y * CellSize,
                CellSize,
                CellSize
            );
            
            Texture2D? ghostTexture = GetGhostTexture(ghostIndex);
            
            if (ghostTexture != null)
            {
                spriteBatch.Draw(ghostTexture, ghostRect, Color.White);
            }
            else
            {
                // Fallback to colored rectangles
                var ghostColor = GetGhostColor(ghostIndex);
                spriteBatch.Draw(_pixelTexture, ghostRect, ghostColor);
            }
            
            ghostIndex++;
        }

        // Draw Pacman with animated sprite or fallback to colored rectangle
        var pacmanPos = _simulation.PacmanPosition;
        var pacmanRect = new Rectangle(
            offsetX + pacmanPos.X * CellSize,
            offsetY + pacmanPos.Y * CellSize,
            CellSize,
            CellSize
        );
        
        // Get current animation frame and direction
        var currentFrame = (int)(_animationTimer / AnimationFrameDuration) % AnimationFrameCount;
        var direction = GetDirectionString(_simulation.CurrentDirection);
        var pacmanTexture = GetPacmanTexture(direction, currentFrame);
        
        if (pacmanTexture != null)
        {
            spriteBatch.Draw(pacmanTexture, pacmanRect, Color.White);
        }
        else
        {
            spriteBatch.Draw(_pixelTexture, pacmanRect, Color.Yellow);
        }
    }
    
    private string GetDirectionString(Game.Direction direction)
    {
        return direction switch
        {
            Game.Direction.Up => "up",
            Game.Direction.Down => "down",
            Game.Direction.Left => "left",
            Game.Direction.Right => "right",
            _ => "right" // Default to right if no direction
        };
    }
    
    private Texture2D? GetPacmanTexture(string direction, int frame)
    {
        string key = $"{direction}_{frame}";
        if (_pacmanTextures.TryGetValue(key, out var texture))
        {
            return texture;
        }
        return null;
    }
    
    private Texture2D? GetGhostTexture(int ghostIndex)
    {
        return ghostIndex switch
        {
            0 => _ghostRedTexture,      // Ghost 0: Red (Blinky)
            1 => _ghostPinkTexture,     // Ghost 1: Pink (Pinky)
            2 => _ghostCyanTexture,     // Ghost 2: Cyan (Inky)
            3 => _ghostOrangeTexture,   // Ghost 3: Orange (Clyde)
            _ => null
        };
    }
    
    private Color GetGhostColor(int ghostIndex)
    {
        return ghostIndex switch
        {
            0 => Color.Red,       // Ghost 0: Red (Blinky)
            1 => Color.HotPink,   // Ghost 1: Pink (Pinky)
            2 => Color.Cyan,      // Ghost 2: Cyan (Inky)
            3 => Color.Orange,    // Ghost 3: Orange (Clyde)
            _ => Color.Red
        };
    }}