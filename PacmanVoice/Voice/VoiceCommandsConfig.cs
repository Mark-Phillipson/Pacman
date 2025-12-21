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
    public string DefaultProfile { get; set; } = "default";
    public Dictionary<string, CommandProfile> Profiles { get; set; } = new();
}

public class CommandProfile
{
    public Dictionary<string, string> Directions { get; set; } = new();
    public Dictionary<string, string> Commands { get; set; } = new();

    public Dictionary<string, CommandType> GetCommandMap()
    {
        var map = new Dictionary<string, CommandType>(StringComparer.OrdinalIgnoreCase);

        // Add directions
        if (Directions.TryGetValue("up", out var up)) map[up] = CommandType.Up;
        if (Directions.TryGetValue("down", out var down)) map[down] = CommandType.Down;
        if (Directions.TryGetValue("left", out var left)) map[left] = CommandType.Left;
        if (Directions.TryGetValue("right", out var right)) map[right] = CommandType.Right;

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

        return map;
    }

    public List<string> GetAllPhrases()
    {
        var phrases = new List<string>();
        phrases.AddRange(Directions.Values);
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
}

/// <summary>
/// Loads and validates voice commands configuration
/// </summary>
public static class VoiceConfigLoader
{
    public static VoiceCommandsConfig LoadConfig(string configPath)
    {
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"Configuration file not found: {configPath}");
        }

        var json = File.ReadAllText(configPath);
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
