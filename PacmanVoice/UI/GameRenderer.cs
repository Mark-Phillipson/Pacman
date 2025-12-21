using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PacmanVoice.UI;

/// <summary>
/// Renders the game board and entities
/// </summary>
public class GameRenderer
{
    private const int CellSize = 20;
    private Texture2D? _pixelTexture;
    private readonly Game.GameSimulation _simulation;

    public GameRenderer(Game.GameSimulation simulation)
    {
        _simulation = simulation;
    }

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        // Create a 1x1 white pixel texture for drawing
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
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

        // Draw pellets
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

        // Draw ghosts
        foreach (var ghost in ghosts)
        {
            var ghostRect = new Rectangle(
                offsetX + ghost.Position.X * CellSize + 2,
                offsetY + ghost.Position.Y * CellSize + 2,
                CellSize - 4,
                CellSize - 4
            );
            spriteBatch.Draw(_pixelTexture, ghostRect, Color.Red);
        }

        // Draw Pacman
        var pacmanPos = _simulation.PacmanPosition;
        var pacmanRect = new Rectangle(
            offsetX + pacmanPos.X * CellSize + 2,
            offsetY + pacmanPos.Y * CellSize + 2,
            CellSize - 4,
            CellSize - 4
        );
        spriteBatch.Draw(_pixelTexture, pacmanRect, Color.Yellow);
    }
}
