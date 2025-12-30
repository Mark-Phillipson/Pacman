using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PacmanVoice.Voice;
using System.Collections.Generic;

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
    private List<ScoreRecorder.ScoreEntry> _topScores = new();
    private string _playerName = "Player One";
    private ScoreRecorder.ScoreEntry? _highlightScore;

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

    internal void SetTopScores(IEnumerable<ScoreRecorder.ScoreEntry> scores)
    {
        _topScores = new List<ScoreRecorder.ScoreEntry>(scores ?? new List<ScoreRecorder.ScoreEntry>());
    }

    internal void SetHighlightedScore(ScoreRecorder.ScoreEntry? score)
    {
        _highlightScore = score;
    }

        internal void SetPlayerName(string name)
        {
            _playerName = name;
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

    public void DrawLeftHud(SpriteBatch spriteBatch, Rectangle hudArea, int screenWidth, int screenHeight)
    {
        if (_font == null || _pixelTexture == null) return;

        var state = _simulation.State;

        // Draw HUD background
        DrawRectangle(spriteBatch, hudArea, new Color(0, 0, 0, 160));

        // Draw game status vertically
        var yPos = hudArea.Y + 10;
        var levelText = $"Level: {_simulation.CurrentLevel}";
        spriteBatch.DrawString(_font, levelText, new Vector2(hudArea.X + 5, yPos), Color.White);

        yPos += 25;
        var scoreText = $"Score: {_simulation.Score}";
        spriteBatch.DrawString(_font, scoreText, new Vector2(hudArea.X + 5, yPos), Color.White);

        yPos += 25;
        var livesText = $"Lives: {_simulation.Lives}";
        spriteBatch.DrawString(_font, livesText, new Vector2(hudArea.X + 5, yPos), Color.White);

        // Draw power-up status if active
        if (_simulation.IsPowerUpActive)
        {
            yPos += 30;
            var powerUpText = $"POWER-UP!";
            var powerUpColor = (int)(_simulation.PowerUpTimeRemaining % 0.4) > 0.2 ? Color.LimeGreen : Color.White;
            spriteBatch.DrawString(_font, powerUpText, new Vector2(hudArea.X + 5, yPos), powerUpColor);

            yPos += 20;
            var timeText = $"{_simulation.PowerUpTimeRemaining:F1}s";
            spriteBatch.DrawString(_font, timeText, new Vector2(hudArea.X + 5, yPos), powerUpColor);
        }

        // Draw ghost lock timer if ghosts are locked in pen
        if (_simulation.AreGhostsLockedInPen)
        {
            yPos += 30;
            var lockText = $"GHOSTS LOCKED";
            spriteBatch.DrawString(_font, lockText, new Vector2(hudArea.X + 5, yPos), Color.Magenta);
            
            yPos += 20;
            var lockTimeText = $"{_simulation.GhostLockTimeRemaining:F1}s";
            spriteBatch.DrawString(_font, lockTimeText, new Vector2(hudArea.X + 5, yPos), Color.Magenta);
        }

        // Draw respawn countdown if respawning
        if (_simulation.IsRespawning)
        {
            yPos += 30;
            var respawnText = $"RESPAWN";
            spriteBatch.DrawString(_font, respawnText, new Vector2(hudArea.X + 5, yPos), Color.Orange);
            
            yPos += 20;
            var timeText = $"{_simulation.RespawnTimeRemaining:F1}s";
            spriteBatch.DrawString(_font, timeText, new Vector2(hudArea.X + 5, yPos), Color.Orange);
        }
    }

    public void DrawRightHud(SpriteBatch spriteBatch, Rectangle hudArea, int screenWidth, int screenHeight)
    {
        if (_font == null || _pixelTexture == null) return;

        var state = _simulation.State;

        // Draw HUD background
        DrawRectangle(spriteBatch, hudArea, new Color(0, 0, 0, 160));

        var yPos = hudArea.Y + 10;

        // Draw listening indicator
        if (_voiceController.IsListening)
        {
            var listeningText = "LISTENING...";
            spriteBatch.DrawString(_font, listeningText, new Vector2(hudArea.X + 5, yPos), Color.Yellow);

            yPos += 30;
            var hintText = "Say 'never mind' to abort";
            spriteBatch.DrawString(_font, hintText, new Vector2(hudArea.X + 5, yPos), Color.LightGray);
        }

        // Draw last recognized command
        if (!string.IsNullOrEmpty(_voiceController.LastRecognizedText))
        {
            yPos += 30;
            var lastCmdText = $"Last:";
            spriteBatch.DrawString(_font, lastCmdText, new Vector2(hudArea.X + 5, yPos), Color.LightGreen);

            yPos += 20;
            spriteBatch.DrawString(_font, _voiceController.LastRecognizedText, new Vector2(hudArea.X + 5, yPos), Color.LightGreen);
        }

        // Draw status message if available
        if (!string.IsNullOrEmpty(_router.LastStatus))
        {
            yPos += 30;
            var statusMsgText = _router.LastStatus;
            var msgBackgroundRect = new Rectangle(hudArea.X + 2, (int)yPos - 2, hudArea.Width - 4, 20);
            DrawRectangle(spriteBatch, msgBackgroundRect, new Color(0, 0, 100, 180));
            spriteBatch.DrawString(_font, statusMsgText, new Vector2(hudArea.X + 5, yPos), Color.White);
        }

        // Draw command hints at bottom of right panel
        yPos = hudArea.Bottom - 80;
        if (state == Game.GameState.NotStarted)
        {
            DrawTopScoresOverlay(spriteBatch, screenWidth, screenHeight, "Say 'begin game' to start");
        }
        else if (state == Game.GameState.Playing)
        {
            // Comprehensive command list with arrow hints
            spriteBatch.DrawString(_font, "Voice Commands:", new Vector2(hudArea.X + 5, yPos), Color.Gray);
            yPos += 22;

            // Direction arrows
            spriteBatch.DrawString(_font, "↑ north    ↓ south", new Vector2(hudArea.X + 5, yPos), Color.LightGray);
            yPos += 18;
            spriteBatch.DrawString(_font, "← west     → east", new Vector2(hudArea.X + 5, yPos), Color.LightGray);
            yPos += 26;

            // Batch mode
            spriteBatch.DrawString(_font, "batch entry | apply batch | never mind", new Vector2(hudArea.X + 5, yPos), new Color(180,180,180));
            yPos += 22;

            // Speed control
            spriteBatch.DrawString(_font, "speed up | slow down | normal speed", new Vector2(hudArea.X + 5, yPos), new Color(180,180,180));
            yPos += 22;

            // Game control
            spriteBatch.DrawString(_font, "begin game | pause game | resume game | restart game", new Vector2(hudArea.X + 5, yPos), new Color(160,160,160));
            yPos += 22;

            // Status and quit
            spriteBatch.DrawString(_font, "game status | repeat that | quit game | quit confirm", new Vector2(hudArea.X + 5, yPos), new Color(160,160,160));
        }
        else if (state == Game.GameState.Paused)
        {
            var hintText = "Say 'resume'";
            spriteBatch.DrawString(_font, hintText, new Vector2(hudArea.X + 5, yPos), Color.Cyan);
        }
        else if (state == Game.GameState.GameOver)
        {
            var hintText = "Say 'new game'";
            spriteBatch.DrawString(_font, hintText, new Vector2(hudArea.X + 5, yPos), Color.Cyan);
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

        // Draw listening indicator right-aligned in HUD (avoid covering status/last command)
        if (_voiceController.IsListening)
        {
            var listeningText = "LISTENING...";
            var size = _font.MeasureString(listeningText);
            var pos = new Vector2(hudArea.Right - size.X - 10, hudArea.Y + 10);
            spriteBatch.DrawString(_font, listeningText, pos, Color.Yellow);

            var hintText = "Say 'never mind' to abort";
            var hintSize = _font.MeasureString(hintText);
            var hintPos = new Vector2(hudArea.Right - hintSize.X - 10, hudArea.Y + 40);
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
            var hintText = "Directions: north, south, west, east | pause game | game status | quit game";
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
            DrawTopScoresOverlay(spriteBatch, screenWidth, screenHeight, "Say 'start new game' or 'quit to desktop'");
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
        }
    }

    public void DrawGameOverOverlay(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
    {
        if (_simulation.State == Game.GameState.GameOver)
        {
            DrawTopScoresOverlay(spriteBatch, screenWidth, screenHeight, "Say 'start new game' or 'quit to desktop'");
            DrawGameOverBanner(spriteBatch, screenWidth, screenHeight);
        }
    }

    private void DrawTopScoresOverlay(SpriteBatch spriteBatch, int screenWidth, int screenHeight, string hintText)
    {
        if (_font == null || _pixelTexture == null) return;

        // Semi-transparent backdrop covering screen
        DrawRectangle(spriteBatch, new Rectangle(0, 0, screenWidth, screenHeight), new Color(0, 0, 0, 200));

        var title = "Top Scores";
        var titleSize = _font.MeasureString(title);
        var titlePos = new Vector2((screenWidth - titleSize.X) / 2, 40);
        spriteBatch.DrawString(_font, title, titlePos, Color.Yellow);

        var playerLabel = $"Player: {_playerName}";
        var playerSize = _font.MeasureString(playerLabel);
        spriteBatch.DrawString(_font, playerLabel, new Vector2((screenWidth - playerSize.X) / 2, titlePos.Y + titleSize.Y + 5), Color.Cyan);

        var y = titlePos.Y + titleSize.Y + playerSize.Y + 25;
        var x = screenWidth / 2 - 260; // left align block near center

        if (_topScores.Count == 0)
        {
            var msg = "No scores recorded yet";
            var msgSize = _font.MeasureString(msg);
            spriteBatch.DrawString(_font, msg, new Vector2((screenWidth - msgSize.X) / 2, y), Color.LightGray);
            y += msgSize.Y + 20;
        }
        else
        {
            int rank = 1;
            foreach (var s in _topScores)
            {
                if (rank > 20) break;
                var name = string.IsNullOrWhiteSpace(s.Player) ? "(anon)" : s.Player;
                var line = $"{rank,2}. {s.Score,6}  {name,-20}  {s.Timestamp:yyyy-MM-dd}";
                var color = IsHighlighted(s) ? Color.Yellow : Color.White;
                spriteBatch.DrawString(_font, line, new Vector2(x, y), color);
                y += _font.LineSpacing + 2;
                rank++;
            }
        }

        var combinedHint = string.IsNullOrWhiteSpace(hintText) ? "" : hintText;
        var hintSize = _font.MeasureString(combinedHint);
        var hintPos = new Vector2((screenWidth - hintSize.X) / 2, screenHeight - 60);
        spriteBatch.DrawString(_font, combinedHint, hintPos, Color.Cyan);
    }

    private bool IsHighlighted(ScoreRecorder.ScoreEntry entry)
    {
        if (_highlightScore == null) return false;
        return entry.Score == _highlightScore.Score
            && entry.Timestamp == _highlightScore.Timestamp
            && string.Equals(entry.Player ?? string.Empty, _highlightScore.Player ?? string.Empty, System.StringComparison.Ordinal);
    }

    private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        if (_pixelTexture != null)
        {
            spriteBatch.Draw(_pixelTexture, rect, color);
        }
    }
}