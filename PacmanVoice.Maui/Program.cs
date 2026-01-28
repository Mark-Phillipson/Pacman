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

        return builder.Build();
    }
}