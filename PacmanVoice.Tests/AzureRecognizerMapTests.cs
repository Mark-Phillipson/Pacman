using System;
using System.Collections.Generic;
using Xunit;
using PacmanVoice.Voice;

namespace PacmanVoice.Tests;

public class AzureRecognizerMapTests
{
    private class TestableRecognizer : AzureRecognizer
    {
        public TestableRecognizer() : base("fake-key","fake-region") { }
        public new RecognitionResult CallMap(string text) => base.MapRecognizedText(text);
    }

    [Fact]
    public void MapRecognizedText_MatchesExactPhrase()
    {
        var rec = new TestableRecognizer();
        rec.UpdateGrammar(new[] { "up", "down" }, new Dictionary<string, CommandType> { { "up", CommandType.Up }, { "down", CommandType.Down } });

        var r = rec.CallMap("up");

        Assert.False(r.IsRejected);
        Assert.Equal(CommandType.Up, r.Command);
        Assert.Contains(CommandType.Up, r.Commands);
    }

    [Fact]
    public void MapRecognizedText_MatchesSubstring()
    {
        var rec = new TestableRecognizer();
        rec.UpdateGrammar(new[] { "left", "right" }, new Dictionary<string, CommandType> { { "left", CommandType.Left }, { "right", CommandType.Right } });

        var r = rec.CallMap("please go right now");

        Assert.False(r.IsRejected);
        Assert.Equal(CommandType.Right, r.Command);
        Assert.Contains(CommandType.Right, r.Commands);
    }

    [Fact]
    public void MapRecognizedText_UnknownIsRejected()
    {
        var rec = new TestableRecognizer();
        rec.UpdateGrammar(new[] { "start" }, new Dictionary<string, CommandType> { { "start", CommandType.BeginGame } });

        var r = rec.CallMap("nonsense");
        Assert.True(r.IsRejected);
        Assert.Equal(CommandType.Unknown, r.Command);
    }
}
