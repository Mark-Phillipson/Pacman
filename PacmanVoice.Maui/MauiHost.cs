using System;

namespace PacmanVoice.Maui
{
    // Small helper to expose the built service provider to lifecycle event handlers
    public static class MauiHost
    {
        public static IServiceProvider? Services { get; set; }
    }
}
