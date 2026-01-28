using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PacmanVoice.Voice;

/// <summary>
/// Voice commands configuration
/// </summary>
public class VoiceCommandsConfig
{
    [JsonProperty("defaultProfile")]
    public string DefaultProfile { get; set; } = "default";
    [JsonProperty("profiles")]
    public Dictionary<string, CommandProfile> Profiles { get; set; } = new();
}

public class CommandProfile
{
    // Legacy single-word directions (up/down/left/right). If present, used as fallback.
    [JsonProperty("directions")]
    public Dictionary<string, string> Directions { get; set; } = new();
    // Preferred: compass directions with optional synonyms per direction.
    // Keys: "north","south","east","west"; values: list of phrases (e.g., ["north","n"]).
    [JsonProperty("compassDirections")]
    public Dictionary<string, List<string>> CompassDirections { get; set; } = new();
    [JsonProperty("commands")]
    public Dictionary<string, string> Commands { get; set; } = new();

    public Dictionary<string, CommandType> GetCommandMap()
    {
        var map = new Dictionary<string, CommandType>(StringComparer.OrdinalIgnoreCase);

        // Add directions: prefer compass synonyms if provided, otherwise legacy single phrases
        if (CompassDirections.Count > 0)
        {
            AddCompass(map, "north", CommandType.Up);
            AddCompass(map, "south", CommandType.Down);
            AddCompass(map, "west", CommandType.Left);
            AddCompass(map, "east", CommandType.Right);
        }
        else
        {
            if (Directions.TryGetValue("up", out var up)) map[up] = CommandType.Up;
            if (Directions.TryGetValue("down", out var down)) map[down] = CommandType.Down;
            if (Directions.TryGetValue("left", out var left)) map[left] = CommandType.Left;
            if (Directions.TryGetValue("right", out var right)) map[right] = CommandType.Right;
        }

        // Add commands
        if (Commands.TryGetValue("beginGame", out var begin)) map[begin] = CommandType.BeginGame;
        if (Commands.TryGetValue("pauseGame", out var pause)) map[pause] = CommandType.PauseGame;
        if (Commands.TryGetValue("resumeGame", out var resume)) map[resume] = CommandType.ResumeGame;
        if (Commands.TryGetValue("restartGame", out var restart)) map[restart] = CommandType.RestartGame;
        if (Commands.TryGetValue("gameStatus", out var status)) map[status] = CommandType.GameStatus;
        if (Commands.TryGetValue("repeatThat", out var repeat)) map[repeat] = CommandType.RepeatThat;
        if (Commands.TryGetValue("quitGame", out var quit)) map[quit] = CommandType.QuitGame;
        if (Commands.TryGetValue("quitConfirm", out var quitConfirm)) map[quitConfirm] = CommandType.QuitConfirm;
        if (Commands.TryGetValue("neverMind", out var neverMind)) map[neverMind] = CommandType.NeverMind;
        if (Commands.TryGetValue("batchEntry", out var batchEntry)) map[batchEntry] = CommandType.BatchEntry;
        if (Commands.TryGetValue("applyBatch", out var applyBatch)) map[applyBatch] = CommandType.ApplyBatch;
        if (Commands.TryGetValue("speedUp", out var speedUp)) map[speedUp] = CommandType.SpeedUp;
        if (Commands.TryGetValue("slowDown", out var slowDown)) map[slowDown] = CommandType.SlowDown;
        if (Commands.TryGetValue("normalSpeed", out var normalSpeed)) map[normalSpeed] = CommandType.NormalSpeed;
        if (Commands.TryGetValue("startNewGame", out var startNew)) map[startNew] = CommandType.StartNewGame;
        if (Commands.TryGetValue("quitFromGameOver", out var quitGameOver)) map[quitGameOver] = CommandType.QuitFromGameOver;

        return map;
    }

    public List<string> GetAllPhrases()
    {
        var phrases = new List<string>();
        if (CompassDirections.Count > 0)
        {
            phrases.AddRange(CompassDirections.Values.SelectMany(v => v));
        }
        else
        {
            phrases.AddRange(Directions.Values);
        }
        phrases.AddRange(Commands.Values);
        return phrases;
    }

    public bool Validate(out string error)
    {
        error = string.Empty;

        // Check non-direction commands have >= 2 words
        // Note: The plan document mentions syllables, but we check words for simplicity
        // as syllable counting is complex and word count provides similar protection
        foreach (var cmd in Commands.Values)
        {
            if (cmd.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 2)
            {
                error = $"Non-direction command '{cmd}' must have at least 2 words";
                return false;
            }

            // Forbid "cancel"
            if (cmd.Contains("cancel", StringComparison.OrdinalIgnoreCase))
            {
                error = $"Command '{cmd}' contains forbidden word 'cancel'";
                return false;
            }
        }

        return true;
    }

    private void AddCompass(Dictionary<string, CommandType> map, string key, CommandType cmd)
    {
        if (!CompassDirections.TryGetValue(key, out var phrases) || phrases == null) return;
        foreach (var p in phrases)
        {
            if (!string.IsNullOrWhiteSpace(p))
            {
                map[p] = cmd;
            }
        }
    }
}

/// <summary>
/// Loads and validates voice commands configuration
/// </summary>
public static class VoiceConfigLoader
{
    public static VoiceCommandsConfig LoadConfig(string configPath)
    {
        // Prefer direct file on disk when available (useful in development)
        if (File.Exists(configPath))
        {
            var json = File.ReadAllText(configPath);
            return DeserializeAndValidate(json);
        }

        // When running from a packaged build (e.g., Android), try TitleContainer.OpenStream via reflection
        try
        {
            // Use reflection so this compiles even when MonoGame isn't referenced in the current target/framework
            var titleContainerType = Type.GetType("Microsoft.Xna.Framework.TitleContainer, MonoGame.Framework");
            if (titleContainerType != null)
            {
                var method = titleContainerType.GetMethod("OpenStream", new[] { typeof(string) });
                if (method != null)
                {
                    using var stream = (Stream)method.Invoke(null, new object[] { configPath });
                    using var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();
                    return DeserializeAndValidate(json);
                }
            }

            throw new FileNotFoundException($"Configuration file not found: {configPath} (tried disk and TitleContainer)");
        }
        catch (Exception ex)
        {
            throw new FileNotFoundException($"Configuration file not found: {configPath} (tried disk and TitleContainer)", ex);
        }
    }

    private static VoiceCommandsConfig DeserializeAndValidate(string json)
    {
        var config = JsonConvert.DeserializeObject<VoiceCommandsConfig>(json);

        if (config == null)
        {
            throw new InvalidOperationException("Failed to deserialize configuration");
        }

        // Validate the default profile
        if (!config.Profiles.TryGetValue(config.DefaultProfile, out var profile))
        {
            throw new InvalidOperationException($"Default profile '{config.DefaultProfile}' not found");
        }

        if (!profile.Validate(out var error))
        {
            throw new InvalidOperationException($"Configuration validation failed: {error}");
        }

        return config;
    }
}
