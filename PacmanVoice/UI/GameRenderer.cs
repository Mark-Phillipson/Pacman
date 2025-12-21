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
    private Texture2D? _pacmanTexture;
    private Texture2D? _ghostRedTexture;
    private Texture2D? _ghostPinkTexture;
    private Texture2D? _ghostCyanTexture;
    private Texture2D? _ghostOrangeTexture;
    private Dictionary<string, Texture2D?> _fruitTextures = new();

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
            _pacmanTexture = content.Load<Texture2D>("Sprites/Pacman/pacman_right");
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

        // Draw Pacman with sprite or fallback to colored rectangle
        var pacmanPos = _simulation.PacmanPosition;
        var pacmanRect = new Rectangle(
            offsetX + pacmanPos.X * CellSize,
            offsetY + pacmanPos.Y * CellSize,
            CellSize,
            CellSize
        );
        
        if (_pacmanTexture != null)
        {
            spriteBatch.Draw(_pacmanTexture, pacmanRect, Color.White);
        }
        else
        {
            spriteBatch.Draw(_pixelTexture, pacmanRect, Color.Yellow);
        }
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