using System;
using Xunit;
using PacmanVoice.Voice;

namespace PacmanVoice.Tests;

public class AzureRecognizerTests
{
    [Fact]
    public void Constructor_WithNoEnvironmentVariables_EmitsRecognitionError()
    {
        // Ensure env vars are not set for the test
        Environment.SetEnvironmentVariable("AZURE_SPEECH_KEY", null);
        Environment.SetEnvironmentVariable("AZURE_SPEECH_REGION", null);

        string? lastError = null;

        using var recognizer = new AzureRecognizer();
        recognizer.RecognitionError += (s, e) => lastError = e;

        // Start will emit a RecognitionError when not configured
        recognizer.Start();

        Assert.NotNull(lastError);
        Assert.Contains("not configured", lastError, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_WithEnvironmentVariables_DoesNotEmitError()
    {
        // Provide dummy (but present) values to simulate configuration
        Environment.SetEnvironmentVariable("AZURE_SPEECH_KEY", "fake-key");
        Environment.SetEnvironmentVariable("AZURE_SPEECH_REGION", "fake-region");

        string? lastError = null;
        using var recognizer = new AzureRecognizer();
        recognizer.RecognitionError += (s, e) => lastError = e;

        // Start to confirm there is no error when env vars exist
        recognizer.Start();
        Assert.Null(lastError);

        // Cleanup
        Environment.SetEnvironmentVariable("AZURE_SPEECH_KEY", null);
        Environment.SetEnvironmentVariable("AZURE_SPEECH_REGION", null);
    }
}
