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
        var status = await Permissions.RequestAsync<Permissions.Microphone>();
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlertAsync("Permission", "Microphone permission is required to listen.", "OK");
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
}