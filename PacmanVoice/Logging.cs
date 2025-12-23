using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace PacmanVoice
{
    internal static class Logger
    {
        private static readonly object _lock = new();
        private static string _logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PacmanVoice", "logs", "startup.log");

        public static void Init()
        {
            try
            {
                var dir = Path.GetDirectoryName(_logPath) ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                Directory.CreateDirectory(dir);
                Log($"--- Log start: {DateTime.UtcNow:O} ---");
                LogEnvironment();
            }
            catch { }
        }

        private static void LogEnvironment()
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"OS: {Environment.OSVersion} 64Bit:{Environment.Is64BitOperatingSystem}");
                sb.AppendLine($"Runtime: {Environment.Version}");
                var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                sb.AppendLine($"Assembly: {asm.GetName().Name} {asm.GetName().Version}");
                sb.AppendLine($"WorkingDir: {Environment.CurrentDirectory}");
                sb.AppendLine($"ExePath: {Assembly.GetEntryAssembly()?.Location}");
                Log(sb.ToString());
            }
            catch { }
        }

        public static void Log(string message)
        {
            try
            {
                lock (_lock)
                {
                    File.AppendAllText(_logPath, $"{DateTime.UtcNow:O} INFO: {message}{Environment.NewLine}");
                }
            }
            catch { }
        }

        public static void LogException(string tag, Exception? ex)
        {
            try
            {
                lock (_lock)
                {
                    var text = new StringBuilder();
                    text.AppendLine($"{DateTime.UtcNow:O} ERROR [{tag}]: {ex?.Message}");
                    if (ex != null) text.AppendLine(ex.ToString());
                    File.AppendAllText(_logPath, text.ToString());
                }
            }
            catch { }
        }
    }
}
