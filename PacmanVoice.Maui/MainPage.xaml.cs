using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using PacmanVoice.Voice;
using System.Threading.Tasks;

namespace PacmanVoice.Maui;

public partial class MainPage : ContentPage
{
    readonly IRecognizer _recognizer;
    readonly PacmanVoice.PacmanGame _game;

    public MainPage(IRecognizer recognizer, PacmanVoice.PacmanGame game)
    {
        InitializeComponent();
        _recognizer = recognizer;
        _game = game;

        _recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
        _recognizer.SpeechDetected += (s, e) => MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = "Status: Detecting speech...");
        _recognizer.RecognitionError += (s, e) => MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = "Error: " + e);
        _recognizer.RecognitionCompleted += (s, e) => MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = "Status: Completed");
        _recognizer.SpeechRejected += (s, e) => MainThread.BeginInvokeOnMainThread(() => { StatusLabel.Text = "Status: Rejected"; ResultLabel.Text = "Recognized: (rejected)"; });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Ensure the game is initialized and resume simulation
        try
        {
            _game.EnsureInitialized();
            _game.ResumeForHost();
        }
        catch { }

        // Start recognizer
        try
        {
            StatusLabel.Text = "Status: Listening";
            _recognizer.Start();
        }
        catch (System.Exception ex)
        {
            StatusLabel.Text = "Error: " + ex.Message;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Pause simulation and stop recognizer when page goes out of view
        try
        {
            _game.PauseForHost();
        }
        catch { }

        try
        {
            _recognizer.Stop();
            StatusLabel.Text = "Status: Stopped";
        }
        catch { }
    }

    private void Recognizer_SpeechRecognized(object? sender, RecognitionResult e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusLabel.Text = "Status: Recognized";
            ResultLabel.Text = $"Recognized: {e.Text} ({e.Command})";
        });
    }

    async void OnStartClicked(object sender, EventArgs e)
    {
        var granted = await EnsureMicrophonePermissionAsync();
        if (!granted)
        {
            StatusLabel.Text = "Status: Permission required";
            return;
        }

        StatusLabel.Text = "Status: Listening";
        // If the IRecognizer implementation exposes Start/StartAsync, prefer async. We call Start() for simplicity.
        _recognizer.Start();
    }

    void OnStopClicked(object sender, EventArgs e)
    {
        _recognizer.Stop();
        StatusLabel.Text = "Status: Stopped";
    }

    async void OnRequestPermissionClicked(object sender, EventArgs e)
    {
        var granted = await EnsureMicrophonePermissionAsync(true);
        if (granted)
        {
            StatusLabel.Text = "Permission: Granted";
            PermissionExplanation.IsVisible = false;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Ensure the game is initialized and resume simulation
        try
        {
            _game.EnsureInitialized();
            _game.ResumeForHost();
        }
        catch { }

        // Start recognizer only if permission is available
        _ = MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                var granted = await EnsureMicrophonePermissionAsync();
                if (granted)
                {
                    StatusLabel.Text = "Status: Listening";
                    _recognizer.Start();
                }
                else
                {
                    StatusLabel.Text = "Permission required to listen";
                }
            }
            catch (System.Exception ex)
            {
                StatusLabel.Text = "Error: " + ex.Message;
            }
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Pause simulation and stop recognizer when page goes out of view
        try
        {
            _game.PauseForHost();
        }
        catch { }

        try
        {
            _recognizer.Stop();
            StatusLabel.Text = "Status: Stopped";
        }
        catch { }
    }

    private async System.Threading.Tasks.Task<bool> EnsureMicrophonePermissionAsync(bool showSettingsIfDenied = false)
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
        if (status == PermissionStatus.Granted)
            return true;

        if (status == PermissionStatus.Denied || status == PermissionStatus.Disabled)
        {
            // Provide explanation and offer to open app settings
            PermissionExplanation.Text = "Microphone access is required for voice commands. Please enable microphone permission in settings.";
            PermissionExplanation.IsVisible = true;

            if (showSettingsIfDenied)
            {
                var result = await DisplayAlert("Microphone permission", "Microphone access is required for voice commands. Open app settings to enable?", "Open Settings", "Cancel");
                if (result)
                {
                    try { AppInfo.ShowSettingsUI(); } catch { }
                }
            }

            // Try requesting permission if platform allows
            var req = await Permissions.RequestAsync<Permissions.Microphone>();
            return req == PermissionStatus.Granted;
        }

        // Request permission for other statuses (Unknown, etc.)
        var newStatus = await Permissions.RequestAsync<Permissions.Microphone>();
        return newStatus == PermissionStatus.Granted;
    }
}