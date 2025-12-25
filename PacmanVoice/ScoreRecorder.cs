using System;
using System.IO;
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
            return null;
        }

        public static void SetPlayerOneName(string? name)
        {
            try
            {
                Directory.CreateDirectory(_appDir);
                if (string.IsNullOrWhiteSpace(name))
                {
                    if (File.Exists(_playerFile)) File.Delete(_playerFile);
                }
                else
                {
                    File.WriteAllText(_playerFile, name.Trim());
                }
            }
            catch (Exception ex)
            {
                try { Logger.LogException("ScoreRecorder.SetPlayerOneName", ex); } catch { }
            }
        }
    }
}
