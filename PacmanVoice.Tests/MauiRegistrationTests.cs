using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using PacmanVoice.Voice;

namespace PacmanVoice.Tests
{
    class FakeRecognizerForDI : IRecognizer
    {
        public event EventHandler? SpeechDetected;
        public event EventHandler<RecognitionResult>? SpeechRecognized;
        public event EventHandler? SpeechRejected;
        public event EventHandler? RecognitionCompleted;
        public event EventHandler<string>? RecognitionError;

        public bool StartCalled { get; private set; }
        public void Start() { StartCalled = true; }
        public void Stop() { }
        public void UpdateGrammar(System.Collections.Generic.IEnumerable<string> phrases, System.Collections.Generic.Dictionary<string, CommandType> commandMap) { }
    }

    public class MauiRegistrationTests
    {
        [Fact]
        public void ServiceCollection_ResolvesPacmanGame_WithInjectedRecognizer()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IRecognizer, FakeRecognizerForDI>();
            services.AddSingleton<PacmanVoice.PacmanGame>(sp => new PacmanVoice.PacmanGame(sp.GetRequiredService<IRecognizer>()));

            var provider = services.BuildServiceProvider();

            var game = provider.GetRequiredService<PacmanVoice.PacmanGame>();

            // Create minimal config file so Initialize doesn't fail
            var configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "voice-commands.json");
            var minimal = "{\n  \"defaultProfile\": \"default\",\n  \"profiles\": { \"default\": { \"directions\": { \"up\": \"up\", \"down\": \"down\", \"left\": \"left\", \"right\": \"right\" }, \"commands\": { \"beginGame\": \"begin game\" } } } }";
            System.IO.File.WriteAllText(configPath, minimal);

            // Call Initialize to ensure the injected recognizer is started via the voice controller
            game.Initialize();

            var recognizer = (FakeRecognizerForDI)provider.GetRequiredService<IRecognizer>();
            Assert.True(recognizer.StartCalled);

            try { System.IO.File.Delete(configPath); } catch { }
        }
    }
}
