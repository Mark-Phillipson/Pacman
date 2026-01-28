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
        builder.Services.AddSingleton<MainPage>();

        return builder.Build();
    }
}