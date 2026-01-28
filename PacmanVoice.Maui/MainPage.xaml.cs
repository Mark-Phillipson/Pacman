using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using PacmanVoice.Voice;
using System.Threading.Tasks;

namespace PacmanVoice.Maui;

public partial class MainPage : ContentPage
{
    readonly IRecognizer _recognizer;

    public MainPage(IRecognizer recognizer)
    {
        InitializeComponent();
        _recognizer = recognizer;
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