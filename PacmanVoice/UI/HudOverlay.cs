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

    public void Draw(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
    {
        if (_font == null || _pixelTexture == null) return;

        var state = _simulation.State;

        // Draw game status at top
        var statusText = $"Score: {_simulation.Score}  Lives: {_simulation.Lives}  State: {state}";
        spriteBatch.DrawString(_font, statusText, new Vector2(10, 10), Color.White);

        // Draw listening indicator
        if (_voiceController.IsListening)
        {
            var listeningText = "LISTENING...";
            var size = _font.MeasureString(listeningText);
            var pos = new Vector2((screenWidth - size.X) / 2, 60);
            
            // Draw background
            var backgroundRect = new Rectangle((int)pos.X - 10, (int)pos.Y - 5, (int)size.X + 20, (int)size.Y + 10);
            DrawRectangle(spriteBatch, backgroundRect, new Color(0, 0, 0, 180));
            
            spriteBatch.DrawString(_font, listeningText, pos, Color.Yellow);

            // Show abort hint
            var hintText = "Say 'never mind' to abort";
            var hintSize = _font.MeasureString(hintText);
            var hintPos = new Vector2((screenWidth - hintSize.X) / 2, pos.Y + size.Y + 5);
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
            var msgPos = new Vector2((screenWidth - msgSize.X) / 2, screenHeight - 60);
            
            // Draw background
            var msgBackgroundRect = new Rectangle((int)msgPos.X - 10, (int)msgPos.Y - 5, (int)msgSize.X + 20, (int)msgSize.Y + 10);
            DrawRectangle(spriteBatch, msgBackgroundRect, new Color(0, 0, 100, 180));
            
            spriteBatch.DrawString(_font, statusMsgText, msgPos, Color.White);
        }

        // Draw last recognized command
        if (!string.IsNullOrEmpty(_voiceController.LastRecognizedText))
        {
            var lastCmdText = $"Last: {_voiceController.LastRecognizedText}";
            spriteBatch.DrawString(_font, lastCmdText, new Vector2(10, 40), Color.LightGreen);
        }

        // Draw command hints at bottom
        if (state == Game.GameState.NotStarted)
        {
            var hintText = "Say 'begin game' to start";
            var hintSize = _font.MeasureString(hintText);
            var hintPos = new Vector2((screenWidth - hintSize.X) / 2, screenHeight - 30);
            spriteBatch.DrawString(_font, hintText, hintPos, Color.Cyan);
        }
        else if (state == Game.GameState.Playing)
        {
            var hintText = "Directions: up, down, left, right | pause game | game status | quit game";
            var hintSize = _font.MeasureString(hintText);
            var hintPos = new Vector2((screenWidth - hintSize.X) / 2, screenHeight - 30);
            spriteBatch.DrawString(_font, hintText, hintPos, new Color(150, 150, 150));
        }
        else if (state == Game.GameState.Paused)
        {
            var hintText = "Say 'resume game' or 'restart game'";
            var hintSize = _font.MeasureString(hintText);
            var hintPos = new Vector2((screenWidth - hintSize.X) / 2, screenHeight - 30);
            spriteBatch.DrawString(_font, hintText, hintPos, Color.Cyan);
        }
    }

    private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        if (_pixelTexture != null)
        {
            spriteBatch.Draw(_pixelTexture, rect, color);
        }
    }
}
