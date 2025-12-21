using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PacmanVoice.Core;
using PacmanVoice.Voice;
using PacmanVoice.Game;
using PacmanVoice.UI;
using System;
using System.IO;

namespace PacmanVoice;

public class PacmanGame : Microsoft.Xna.Framework.Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private SpriteFont? _font;
    private Texture2D? _pixelTexture;

    private SimulationClock? _clock;
    private GameSimulation? _simulation;
    private VoiceInputController? _voiceController;
    private CommandRouter? _commandRouter;
    private GameRenderer? _gameRenderer;
    private HudOverlay? _hudOverlay;
    private SoundEffectManager? _soundManager;

    private bool _initialized = false;
    private string? _initError = null;

    public PacmanGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Set window size
        _graphics.PreferredBackBufferWidth = 800;
        _graphics.PreferredBackBufferHeight = 860;
    }

    protected override void Initialize()
    {
        try
        {
            // Check if running on Windows (System.Speech requires Windows)
            if (!OperatingSystem.IsWindows())
            {
                _initError = "This game requires Windows to run (System.Speech is Windows-only)";
                Console.WriteLine(_initError);
                return;
            }

            // Initialize core systems
            _clock = new SimulationClock();
            _simulation = new GameSimulation();
            _commandRouter = new CommandRouter(_simulation, this);

            // Initialize sound manager
            _soundManager = new SoundEffectManager(Content);

            // Subscribe to game events for sound effects
            _simulation.PlayerDied += () => _soundManager?.PlaySound("death", 0.8f);
            _simulation.PelletEaten += () => _soundManager?.PlaySound("eatfruit", 0.5f);
            _simulation.GhostEaten += () => _soundManager?.PlaySound("eatghost", 0.7f);
            _simulation.LevelStart += () => _soundManager?.PlaySound("theme", 0.6f);
            _simulation.PowerUpActivated += () => _soundManager?.PlaySound("freeman", 1.0f);
            _simulation.FruitEaten += () => _soundManager?.PlaySound("eatfruit", 0.8f);

            // Load voice commands configuration
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "voice-commands.json");
            var config = VoiceConfigLoader.LoadConfig(configPath);
            var profile = config.Profiles[config.DefaultProfile];

            // Initialize voice recognition
            var recognizer = new SystemSpeechRecognizer();
            _voiceController = new VoiceInputController(recognizer, _clock);

            // Subscribe to command events
            _voiceController.CommandRecognized += OnCommandRecognized;
            _voiceController.ErrorOccurred += OnVoiceError;

            // Update grammar with commands
            var phrases = profile.GetAllPhrases();
            var commandMap = profile.GetCommandMap();
            _voiceController.UpdateGrammar(phrases, commandMap);

            // Start voice recognition
            _voiceController.Start();

            // Initialize renderers
            _gameRenderer = new GameRenderer(_simulation);
            _hudOverlay = new HudOverlay(_voiceController, _simulation, _commandRouter);

            _initialized = true;
        }
        catch (Exception ex)
        {
            _initError = $"Initialization error: {ex.Message}\n\nNote: This game requires Windows and microphone access for voice recognition.";
            Console.WriteLine(_initError);
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // 1x1 pixel texture for simple UI lines (border)
        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        try
        {
            // Load font
            _font = Content.Load<SpriteFont>("DefaultFont");

            // Load sound effects
            _soundManager?.LoadSounds();

            if (_gameRenderer != null)
            {
                _gameRenderer.LoadContent(Content, GraphicsDevice);
            }

            if (_hudOverlay != null)
            {
                _hudOverlay.Initialize(GraphicsDevice);
                if (_font != null)
                {
                    _hudOverlay.LoadContent(_font);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Content loading error: {ex.Message}");
            _initError = $"Failed to load content: {ex.Message}";
        }
    }

    protected override void Update(GameTime gameTime)
    {
        // Allow escape key for emergency exit
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
            return;
        }

        if (!_initialized || _clock == null || _simulation == null)
            return;

        // Update simulation clock
        var deltaSeconds = gameTime.ElapsedGameTime.TotalSeconds;
        _clock.Update(deltaSeconds);

        // Update animation
        _gameRenderer?.Update(deltaSeconds);

        // Update HUD overlay (for game over banner animation)
        _hudOverlay?.Update(deltaSeconds);

        // Update simulation (only if clock is running)
        if (_clock.IsRunning)
        {
            // Execute at most one queued voice command per frame
            _commandRouter?.Update();
            _simulation.Update(deltaSeconds);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        if (_spriteBatch == null)
            return;

        _spriteBatch.Begin();

        if (_initError != null)
        {
            // Draw error message
            DrawText(_spriteBatch, _initError, new Vector2(10, 10), Color.Red);
        }
        else if (_initialized && _gameRenderer != null && _hudOverlay != null)
        {
            const int hudHeight = 140;
            const int borderThickness = 4;

            var screenWidth = _graphics.PreferredBackBufferWidth;
            var screenHeight = _graphics.PreferredBackBufferHeight;
            var playfieldHeight = screenHeight - hudHeight - borderThickness;

            // Draw game
            _gameRenderer.Draw(_spriteBatch, new Rectangle(0, 0, screenWidth, playfieldHeight));

            // Bottom border separating playfield from HUD
            if (_pixelTexture != null)
            {
                var borderRect = new Rectangle(0, playfieldHeight, screenWidth, borderThickness);
                _spriteBatch.Draw(_pixelTexture, borderRect, Color.White);
            }

            // Draw HUD
            var hudArea = new Rectangle(0, playfieldHeight + borderThickness, screenWidth, hudHeight);
            _hudOverlay.Draw(_spriteBatch, screenWidth, screenHeight, hudArea);
        }
        else
        {
            DrawText(_spriteBatch, "Initializing...", new Vector2(10, 10), Color.White);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
    {
        if (_font != null)
        {
            spriteBatch.DrawString(_font, text, position, color);
        }
    }

    private void OnCommandRecognized(object? sender, RecognitionResult result)
    {
        if (_commandRouter != null)
        {
            _commandRouter.HandleCommand(result);
        }
    }

    private void OnVoiceError(object? sender, string error)
    {
        Console.WriteLine($"Voice error: {error}");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _voiceController?.Dispose();
            _soundManager?.Dispose();
        }
        base.Dispose(disposing);
    }
}
