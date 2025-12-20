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

public class Game1 : Microsoft.Xna.Framework.Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private SpriteFont? _font;

    private SimulationClock? _clock;
    private GameSimulation? _simulation;
    private VoiceInputController? _voiceController;
    private CommandRouter? _commandRouter;
    private GameRenderer? _gameRenderer;
    private HudOverlay? _hudOverlay;

    private bool _initialized = false;
    private string? _initError = null;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Set window size
        _graphics.PreferredBackBufferWidth = 800;
        _graphics.PreferredBackBufferHeight = 700;
    }

    protected override void Initialize()
    {
        try
        {
            // Initialize core systems
            _clock = new SimulationClock();
            _simulation = new GameSimulation();
            _commandRouter = new CommandRouter(_simulation, this);

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
            _initError = $"Initialization error: {ex.Message}";
            Console.WriteLine(_initError);
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        try
        {
            // Load font
            _font = Content.Load<SpriteFont>("DefaultFont");

            if (_gameRenderer != null)
            {
                _gameRenderer.LoadContent(GraphicsDevice);
            }

            if (_hudOverlay != null && _font != null)
            {
                _hudOverlay.LoadContent(_font);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Content loading error: {ex.Message}");
            _initError = $"Failed to load content: {ex.Message}";
        }
    }

    private SpriteFont CreateBasicFont()
    {
        // This is a placeholder - in a real implementation you would load from Content Pipeline
        // For now we'll just return null and handle it in the draw
        return null!;
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

        // Update simulation (only if clock is running)
        if (_clock.IsRunning)
        {
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
            // Draw game
            _gameRenderer.Draw(_spriteBatch, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            // Draw HUD
            _hudOverlay.Draw(_spriteBatch, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
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
        }
        base.Dispose(disposing);
    }
}
