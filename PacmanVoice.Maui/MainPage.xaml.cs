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

        // Toggle buttons
        StartButton.IsEnabled = false;
        StopButton.IsEnabled = true;
    }

    void OnStopClicked(object sender, EventArgs e)
    {
        _recognizer.Stop();
        StatusLabel.Text = "Status: Stopped";

        // Toggle buttons
        StartButton.IsEnabled = true;
        StopButton.IsEnabled = false;
    }

    async void OnRequestPermissionClicked(object sender, EventArgs e)
    {
        var granted = await EnsureMicrophonePermissionAsync(true);
        if (granted)
        {
            StatusLabel.Text = "Permission: Granted";
            PermissionExplanation.IsVisible = false;

            // Enable Start when permission granted
            StartButton.IsEnabled = true;
            RequestPermissionButton.IsEnabled = false;
        }

        // Update Azure status
        UpdateAzureStatusLabel();
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

                // Update Azure status label
                UpdateAzureStatusLabel();
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
        {
            // Update UI
            StartButton.IsEnabled = true;
            RequestPermissionButton.IsEnabled = false;
            PermissionExplanation.IsVisible = false;
            return true;
        }

        if (status == PermissionStatus.Denied || status == PermissionStatus.Disabled)
        {
            // Provide explanation and offer to open app settings
            PermissionExplanation.Text = "Microphone access is required for voice commands. Please enable microphone permission in settings.";
            PermissionExplanation.IsVisible = true;
            StartButton.IsEnabled = false;
            RequestPermissionButton.IsEnabled = true;

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
            var granted = req == PermissionStatus.Granted;
            StartButton.IsEnabled = granted;
            RequestPermissionButton.IsEnabled = !granted;
            PermissionExplanation.IsVisible = !granted;
            return granted;
        }

        // Request permission for other statuses (Unknown, etc.)
        var newStatus = await Permissions.RequestAsync<Permissions.Microphone>();
        var ok = newStatus == PermissionStatus.Granted;
        StartButton.IsEnabled = ok;
        RequestPermissionButton.IsEnabled = !ok;
        PermissionExplanation.IsVisible = !ok;
        return ok;
    }

    private void UpdateAzureStatusLabel()
    {
        try
        {
            var appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            var credFile = System.IO.Path.Combine(appData, "azure-speech.json");
            if (System.IO.File.Exists(credFile))
            {
                var json = System.IO.File.ReadAllText(credFile);
                var obj = System.Text.Json.JsonDocument.Parse(json).RootElement;
                var key = obj.GetProperty("key").GetString();
                AzureStatusLabel.Text = string.IsNullOrEmpty(key) ? "Azure: Not configured" : "Azure: Configured";
                SaveAzureButton.IsEnabled = true;
                ClearAzureButton.IsEnabled = true;
            }
            else
            {
                AzureStatusLabel.Text = "Azure: Not configured";
                SaveAzureButton.IsEnabled = true;
                ClearAzureButton.IsEnabled = false;
            }
        }
        catch
        {
            AzureStatusLabel.Text = "Azure: Unknown";
            SaveAzureButton.IsEnabled = true;
            ClearAzureButton.IsEnabled = false;
        }
    }

    async void OnSaveAzureClicked(object sender, EventArgs e)
    {
        var key = AzureKeyEntry.Text?.Trim();
        var region = AzureRegionEntry.Text?.Trim();
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(region))
        {
            AzureStatusLabel.Text = "Azure: Key and Region required";
            return;
        }

        try
        {
            var appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            var credFile = System.IO.Path.Combine(appData, "azure-speech.json");
            var obj = System.Text.Json.JsonSerializer.Serialize(new { key = key, region = region });
            System.IO.File.WriteAllText(credFile, obj);

            AzureStatusLabel.Text = "Azure: Configured";
            SaveAzureButton.IsEnabled = true;
            ClearAzureButton.IsEnabled = true;

            // Try to configure running recognizer if it's AzureRecognizer
            if (_recognizer is PacmanVoice.Voice.AzureRecognizer az)
            {
                az.Configure(key, region);
            }
        }
        catch (System.Exception ex)
        {
            AzureStatusLabel.Text = "Azure: Save failed";
        }
    }

    async void OnClearAzureClicked(object sender, EventArgs e)
    {
        try
        {
            var appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            var credFile = System.IO.Path.Combine(appData, "azure-speech.json");
            if (System.IO.File.Exists(credFile))
                System.IO.File.Delete(credFile);

            AzureStatusLabel.Text = "Azure: Not configured";
            ClearAzureButton.IsEnabled = false;

            // If the recognizer is AzureRecognizer, mark not configured
            if (_recognizer is PacmanVoice.Voice.AzureRecognizer az)
            {
                az.Configure(null, null);
            }
        }
        catch { AzureStatusLabel.Text = "Azure: Clear failed"; }
    }
}