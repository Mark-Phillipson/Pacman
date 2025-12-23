using System;
using PacmanVoice;

Logger.Init();

AppDomain.CurrentDomain.UnhandledException += (s, e) =>
{
	try { Logger.LogException("UnhandledException", e.ExceptionObject as Exception); }
	catch { }
};

System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) =>
{
	try { Logger.LogException("UnobservedTaskException", e.Exception); e.SetObserved(); }
	catch { }
};

try
{
	using var game = new PacmanVoice.PacmanGame();
	game.Run();
}
catch (Exception ex)
{
	try { Logger.LogException("StartupException", ex); }
	catch { }
	throw;
}
