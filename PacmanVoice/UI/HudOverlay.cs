using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PacmanVoice.Voice;

namespace PacmanVoice.UI;

/// <summary>
/// HUD overlay showing listening/frozen state and command hints
/// </summary>
public class HudOverlay
{
    private SpriteFont? _font;
    private Texture2D? _pixelTexture;
    private readonly VoiceInputController _voiceController;
    private readonly Game.GameSimulation _simulation;
    private readonly Game.CommandRouter _router;

    // Game over banner animation
    private double _gameOverBannerTimer = 0.0;
    private const double BannerDropDuration = 1.0; // seconds to drop the banner
    private const double BannerHeight = 120;

    public HudOverlay(VoiceInputController voiceController, Game.GameSimulation simulation, Game.CommandRouter router)
    {
        _voiceController = voiceController;
        _simulation = simulation;
        _router = router;
    }

    public void LoadContent(SpriteFont font)
    {
        _font = font;
    }

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        // Create a 1x1 white pixel texture for drawing rectangles
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    public void Update(double deltaTime)
    {
        // Update game over banner animation
        if (_simulation.State == Game.GameState.GameOver)
        {
            _gameOverBannerTimer = System.Math.Min(_gameOverBannerTimer + deltaTime, BannerDropDuration);
        }
        else
        {
            _gameOverBannerTimer = 0.0;
        }
    }

    public void Draw(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
    {
        Draw(spriteBatch, screenWidth, screenHeight, new Rectangle(0, 0, screenWidth, screenHeight));
    }

    public void Draw(SpriteBatch spriteBatch, int screenWidth, int screenHeight, Rectangle hudArea)
    {
        if (_font == null || _pixelTexture == null) return;

        var state = _simulation.State;

        // Draw HUD background (so text never sits on top of the playfield)
        DrawRectangle(spriteBatch, hudArea, new Color(0, 0, 0, 160));

        // Draw game status in the HUD panel
        var statusText = $"Level: {_simulation.CurrentLevel}  Score: {_simulation.Score}  Lives: {_simulation.Lives}  State: {state}";
        spriteBatch.DrawString(_font, statusText, new Vector2(hudArea.X + 10, hudArea.Y + 10), Color.White);
        
        // Draw power-up status if active
        if (_simulation.IsPowerUpActive)
        {
            var powerUpText = $"POWER-UP! ({_simulation.PowerUpTimeRemaining:F1}s)  Ghosts Eaten: {_simulation.GhostsEatenDuringPowerUp}";
            var powerUpColor = (int)(_simulation.PowerUpTimeRemaining % 0.4) > 0.2 ? Color.LimeGreen : Color.White;
            spriteBatch.DrawString(_font, powerUpText, new Vector2(hudArea.X + 10, hudArea.Y + 35), powerUpColor);
        }
        
        // Draw respawn countdown if respawning
        if (_simulation.IsRespawning)
        {
            var respawnText = $"RESPAWNING... {_simulation.RespawnTimeRemaining:F1}s";
            var respawnSize = _font.MeasureString(respawnText);
            var respawnPos = new Vector2(hudArea.X + (hudArea.Width - respawnSize.X) / 2, hudArea.Y + 35);
            spriteBatch.DrawString(_font, respawnText, respawnPos, Color.Orange);
        }

        // Draw last recognized command
        if (!string.IsNullOrEmpty(_voiceController.LastRecognizedText))
        {
            var lastCmdText = $"Last: {_voiceController.LastRecognizedText}";
            spriteBatch.DrawString(_font, lastCmdText, new Vector2(hudArea.X + 10, hudArea.Y + 40), Color.LightGreen);
        }

        // Draw listening indicator (in HUD panel)
        if (_voiceController.IsListening)
        {
            var listeningText = "LISTENING...";
            var size = _font.MeasureString(listeningText);
            var pos = new Vector2(hudArea.X + (hudArea.Width - size.X) / 2, hudArea.Y + 10);
            spriteBatch.DrawString(_font, listeningText, pos, Color.Yellow);

            var hintText = "Say 'never mind' to abort";
            var hintSize = _font.MeasureString(hintText);
            var hintPos = new Vector2(hudArea.X + (hudArea.Width - hintSize.X) / 2, hudArea.Y + 40);
            spriteBatch.DrawString(_font, hintText, hintPos, Color.LightGray);
        }

        // Draw quit confirmation overlay
        if (state == Game.GameState.QuitConfirmation)
        {
            // Semi-transparent background
            DrawRectangle(spriteBatch, new Rectangle(0, 0, screenWidth, screenHeight), new Color(0, 0, 0, 200));

            var quitText = "QUIT GAME?";
            var quitSize = _font.MeasureString(quitText);
            var quitPos = new Vector2((screenWidth - quitSize.X) / 2, screenHeight / 2 - 40);
            spriteBatch.DrawString(_font, quitText, quitPos, Color.Red);

            var confirmText = "Say 'quit confirm' to exit";
            var confirmSize = _font.MeasureString(confirmText);
            var confirmPos = new Vector2((screenWidth - confirmSize.X) / 2, screenHeight / 2 + 10);
            spriteBatch.DrawString(_font, confirmText, confirmPos, Color.White);

            var cancelText = "Say 'never mind' to cancel";
            var cancelSize = _font.MeasureString(cancelText);
            var cancelPos = new Vector2((screenWidth - cancelSize.X) / 2, screenHeight / 2 + 40);
            spriteBatch.DrawString(_font, cancelText, cancelPos, Color.LightGray);
        }

        // Draw status message if available
        if (!string.IsNullOrEmpty(_router.LastStatus))
        {
            var statusMsgText = _router.LastStatus;
            var msgSize = _font.MeasureString(statusMsgText);
            var msgPos = new Vector2(hudArea.X + (hudArea.Width - msgSize.X) / 2, hudArea.Bottom - 60);

            var msgBackgroundRect = new Rectangle((int)msgPos.X - 10, (int)msgPos.Y - 5, (int)msgSize.X + 20, (int)msgSize.Y + 10);
            DrawRectangle(spriteBatch, msgBackgroundRect, new Color(0, 0, 100, 180));

            spriteBatch.DrawString(_font, statusMsgText, msgPos, Color.White);
        }

        // Draw command hints at bottom
        if (state == Game.GameState.NotStarted)
        {
            var hintText = "Say 'begin game' to start";
            var hintSize = _font.MeasureString(hintText);
            var hintPos = new Vector2(hudArea.X + (hudArea.Width - hintSize.X) / 2, hudArea.Bottom - 30);
            spriteBatch.DrawString(_font, hintText, hintPos, Color.Cyan);
        }
        else if (state == Game.GameState.Playing)
        {
            var hintText = "Directions: up, down, left, right | pause game | game status | quit game";
            var hintSize = _font.MeasureString(hintText);
            var hintPos = new Vector2(hudArea.X + (hudArea.Width - hintSize.X) / 2, hudArea.Bottom - 30);
            spriteBatch.DrawString(_font, hintText, hintPos, new Color(150, 150, 150));
        }
        else if (state == Game.GameState.Paused)
        {
            var hintText = "Say 'resume game' or 'restart game'";
            var hintSize = _font.MeasureString(hintText);
            var hintPos = new Vector2(hudArea.X + (hudArea.Width - hintSize.X) / 2, hudArea.Bottom - 30);
            spriteBatch.DrawString(_font, hintText, hintPos, Color.Cyan);
        }
        else if (state == Game.GameState.GameOver)
        {
            DrawGameOverBanner(spriteBatch, screenWidth, screenHeight);
        }
    }

    private void DrawGameOverBanner(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
    {
        if (_font == null || _pixelTexture == null) return;

        // Calculate banner position with drop animation
        double progress = System.Math.Min(_gameOverBannerTimer / BannerDropDuration, 1.0);
        int bannerY = (int)(progress * BannerHeight) - (int)BannerHeight;

        var bannerRect = new Rectangle(0, bannerY, screenWidth, (int)BannerHeight);

        // Draw semi-transparent dark background for banner
        DrawRectangle(spriteBatch, bannerRect, new Color(0, 0, 0, 220));

        // Draw "GAME OVER" text
        var gameOverText = "GAME OVER";
        var gameOverSize = _font.MeasureString(gameOverText);
        var gameOverPos = new Vector2(
            (screenWidth - gameOverSize.X) / 2,
            bannerY + 15);
        spriteBatch.DrawString(_font, gameOverText, gameOverPos, Color.Red);

        // Draw final score
        var scoreText = $"Final Score: {_simulation.Score}";
        var scoreSize = _font.MeasureString(scoreText);
        var scorePos = new Vector2(
            (screenWidth - scoreSize.X) / 2,
            bannerY + 50);
        spriteBatch.DrawString(_font, scoreText, scorePos, Color.Yellow);

        // Draw voice commands hint (only after banner has fully dropped)
        if (progress >= 1.0)
        {
            var commandsText = "Say 'start new game' or 'quit to desktop'";
            var commandsSize = _font.MeasureString(commandsText);
            var commandsPos = new Vector2(
                (screenWidth - commandsSize.X) / 2,
                bannerY + 85);
            spriteBatch.DrawString(_font, commandsText, commandsPos, Color.Cyan);
        }    }

    private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        if (_pixelTexture != null)
        {
            spriteBatch.Draw(_pixelTexture, rect, color);
        }
    }
}