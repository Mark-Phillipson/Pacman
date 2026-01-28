using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using PacmanVoice.Voice;

namespace PacmanVoice.Tests
{
    class FakeRecognizer : IRecognizer
    {
        public event EventHandler? SpeechDetected;
        public event EventHandler<RecognitionResult>? SpeechRecognized;
        public event EventHandler? SpeechRejected;
        public event EventHandler? RecognitionCompleted;
        public event EventHandler<string>? RecognitionError;

        public bool StartCalled { get; private set; }
        public bool StopCalled { get; private set; }

        public void Start() { StartCalled = true; }
        public void Stop() { StopCalled = true; }
        public void UpdateGrammar(IEnumerable<string> phrases, Dictionary<string, CommandType> commandMap) { }
    }

    public class PacmanGameTests
    {
        [Fact]
        public void ConstructorInjection_UsesInjectedRecognizer()
        {
            // Ensure a minimal voice-commands.json exists in the test output directory so Initialize can proceed
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "voice-commands.json");
            var minimal = "{ \"defaultProfile\": \"default\", \"profiles\": { \"default\": { \"directions\": { \"up\": \"up\", \"down\": \"down\", \"left\": \"left\", \"right\": \"right\" }, \"commands\": { \"beginGame\": \"begin game\" } } } }";
            File.WriteAllText(configPath, minimal);

            var fake = new FakeRecognizer();
            using var game = new PacmanVoice.PacmanGame(fake);

            // Call EnsureInitialized to trigger voice controller setup and Start (Start sets StartCalled on the fake)
            game.EnsureInitialized();

            Assert.True(fake.StartCalled, "Injected recognizer Start should have been called during initialization.");

            // Cleanup
            try { File.Delete(configPath); } catch { }
        }
    }
}
