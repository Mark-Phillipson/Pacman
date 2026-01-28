// Temporary entry point shim to allow local builds when MAUI platform targets are excluded for local development.
// This file is a short-term workaround and should be removed when targeting net10.0-android again.

public static class Program
{
    public static int Main()
    {
        // No-op entry point for compile-only scenarios.
        return 0;
    }
}
