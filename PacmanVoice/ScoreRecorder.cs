using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace PacmanVoice
{
    internal static class ScoreRecorder
    {
        private static readonly object _lock = new();
        private static readonly string _appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PacmanVoice");
        private static readonly string _scoresDir = Path.Combine(_appDir, "scores");
        private static readonly string _scoresCsv = Path.Combine(_scoresDir, "scores.csv");
        private static readonly string _playerFile = Path.Combine(_appDir, "player1.txt");

        public sealed class ScoreEntry
        {
            public DateTimeOffset Timestamp { get; init; }
            public int Score { get; init; }
            public string? Player { get; init; }
        }

        public static void RecordScore(int score, string? playerName = null, DateTimeOffset? when = null)
        {
            try
            {
                Directory.CreateDirectory(_scoresDir);

                var ts = when ?? DateTimeOffset.Now;
                var name = string.IsNullOrWhiteSpace(playerName) ? LoadPlayerOneName() : playerName?.Trim();

                var hasHeader = File.Exists(_scoresCsv) && new FileInfo(_scoresCsv).Length > 0;
                var sb = new StringBuilder();
                if (!hasHeader)
                {
                    sb.AppendLine("timestamp,score,player");
                }
                // CSV safe: wrap name in quotes and escape quotes
                var safeName = name == null ? string.Empty : "\"" + name.Replace("\"", "\"\"") + "\"";
                sb.AppendLine($"{ts:O},{score},{safeName}");

                lock (_lock)
                {
                    File.AppendAllText(_scoresCsv, sb.ToString());
                }
            }
            catch (Exception ex)
            {
                try { Logger.LogException("ScoreRecorder.RecordScore", ex); } catch { }
            }
        }

        public static List<ScoreEntry> LoadTopScores(int max = 20)
        {
            var results = new List<ScoreEntry>();
            try
            {
                if (!File.Exists(_scoresCsv)) return results;

                foreach (var line in File.ReadLines(_scoresCsv))
                {
                    var entry = ParseLine(line);
                    if (entry != null)
                    {
                        results.Add(entry);
                    }
                }

                results = results
                    .OrderByDescending(s => s.Score)
                    .ThenBy(s => s.Timestamp)
                    .Take(Math.Max(1, max))
                    .ToList();
            }
            catch (Exception ex)
            {
                try { Logger.LogException("ScoreRecorder.LoadTopScores", ex); } catch { }
            }

            return results;
        }

        private static ScoreEntry? ParseLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            // Skip header
            if (line.StartsWith("timestamp", StringComparison.OrdinalIgnoreCase)) return null;

            var parts = line.Split(',', 3);
            if (parts.Length < 2) return null;

            if (!DateTimeOffset.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var ts))
            {
                return null;
            }

            if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var score))
            {
                return null;
            }

            string? player = null;
            if (parts.Length >= 3)
            {
                player = parts[2].Trim();
                if (player.Length == 0)
                {
                    player = null;
                }
                else if (player.StartsWith('"') && player.EndsWith('"') && player.Length >= 2)
                {
                    player = player.Substring(1, player.Length - 2).Replace("\"\"", "\"");
                }
            }

            return new ScoreEntry
            {
                Timestamp = ts,
                Score = score,
                Player = player,
            };
        }

        public static string? LoadPlayerOneName()
        {
            try
            {
                // Environment variable takes priority
                var env = Environment.GetEnvironmentVariable("PACMAN_PLAYER1");
                if (!string.IsNullOrWhiteSpace(env)) return env.Trim();

                // Stored in app data
                if (File.Exists(_playerFile))
                {
                    var txt = File.ReadAllText(_playerFile).Trim();
                    if (!string.IsNullOrWhiteSpace(txt)) return txt;
                }

                // Fallback: next to the executable (portable usage)
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var localPlayerFile = Path.Combine(baseDir, "player1.txt");
                if (File.Exists(localPlayerFile))
                {
                    var txt = File.ReadAllText(localPlayerFile).Trim();
                    if (!string.IsNullOrWhiteSpace(txt)) return txt;
                }
            }
            catch (Exception ex)
            {
                try { Logger.LogException("ScoreRecorder.LoadPlayerOneName", ex); } catch { }
            }
            return "Player One"; // Player name is always 'Player One' now
        }

        // public static void SetPlayerOneName(string? name) { }
    }
}
