using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PacmanVoice.Voice;
using CommunityToolkit.Maui;

namespace PacmanVoice.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts => { });

        // Register recognizer service (AzureRecognizer uses env vars by default)
        builder.Services.AddSingleton<IRecognizer, AzureRecognizer>();

        // Register the game with DI so hosts (MAUI) can supply an IRecognizer to the game
        builder.Services.AddSingleton<PacmanVoice.PacmanGame>(sp => new PacmanVoice.PacmanGame(sp.GetRequiredService<IRecognizer>()));

        builder.Services.AddSingleton<MainPage>();

        // Lifecycle hooks for platforms: pause/resume game and stop/start recognizer on background/foreground
        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
            events.AddAndroid(android => android
                .OnPause(activity =>
                {
                    var sp = MauiHost.Services;
                    var game = sp?.GetService<PacmanVoice.PacmanGame>();
                    var recognizer = sp?.GetService<IRecognizer>();
                    try { game?.PauseForHost(); } catch { }
                    try { recognizer?.Stop(); } catch { }
                })
                .OnResume(activity =>
                {
                    var sp = MauiHost.Services;
                    var game = sp?.GetService<PacmanVoice.PacmanGame>();
                    var recognizer = sp?.GetService<IRecognizer>();
                    try { game?.ResumeForHost(); } catch { }
                    try { recognizer?.Start(); } catch { }
                })
            );
#endif
#if IOS
            events.AddiOS(ios => ios
                .DidEnterBackground(uiApp =>
                {
                    var sp = MauiHost.Services;
                    var game = sp?.GetService<PacmanVoice.PacmanGame>();
                    var recognizer = sp?.GetService<IRecognizer>();
                    try { game?.PauseForHost(); } catch { }
                    try { recognizer?.Stop(); } catch { }
                })
                .WillEnterForeground(uiApp =>
                {
                    var sp = MauiHost.Services;
                    var game = sp?.GetService<PacmanVoice.PacmanGame>();
                    var recognizer = sp?.GetService<IRecognizer>();
                    try { game?.ResumeForHost(); } catch { }
                    try { recognizer?.Start(); } catch { }
                })
            );
#endif
#if WINDOWS
            events.AddWindows(windows => windows
                .OnSuspending((wnd, args) =>
                {
                    var sp = MauiHost.Services;
                    var game = sp?.GetService<PacmanVoice.PacmanGame>();
                    var recognizer = sp?.GetService<IRecognizer>();
                    try { game?.PauseForHost(); } catch { }
                    try { recognizer?.Stop(); } catch { }
                })
                .OnActivated((wnd, args) =>
                {
                    var sp = MauiHost.Services;
                    var game = sp?.GetService<PacmanVoice.PacmanGame>();
                    var recognizer = sp?.GetService<IRecognizer>();
                    try { game?.ResumeForHost(); } catch { }
                    try { recognizer?.Start(); } catch { }
                })
            );
#endif
        });

        var app = builder.Build();
        MauiHost.Services = app.Services;

        return app;
    }
}