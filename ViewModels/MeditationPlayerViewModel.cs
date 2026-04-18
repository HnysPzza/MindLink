using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;

namespace M1ndLink.ViewModels;

public partial class MeditationPlayerViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IMeditationService _meditations;
    private readonly INotificationService _notifications;
    private CancellationTokenSource? _playbackCts;

    [ObservableProperty] private Meditation? _meditation;
    [ObservableProperty] private string _meditationTitle = string.Empty;
    [ObservableProperty] private string _subtitle = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _category = string.Empty;
    [ObservableProperty] private string _difficulty = string.Empty;
    [ObservableProperty] private string _iconEmoji = "🧘";
    [ObservableProperty] private string _benefits = string.Empty;
    [ObservableProperty] private string _durationLabel = string.Empty;
    [ObservableProperty] private string _currentPrompt = "Choose a calm space, then press Start Narration.";
    [ObservableProperty] private string _statusText = "Ready to begin";
    [ObservableProperty] private double _progress = 0;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _isCompleted;
    [ObservableProperty] private int _currentStep = 0;
    [ObservableProperty] private int _totalSteps = 0;

    public MeditationPlayerViewModel(IMeditationService meditations, INotificationService notifications)
    {
        _meditations = meditations;
        _notifications = notifications;
        Title = "Meditation";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("MeditationId", out var value) && int.TryParse(value?.ToString(), out var id))
        {
            var meditation = _meditations.GetById(id);
            if (meditation != null)
                LoadMeditation(meditation);
        }
    }

    private void LoadMeditation(Meditation meditation)
    {
        StopPlayback();
        Meditation = meditation;
        MeditationTitle = meditation.Title;
        Subtitle = meditation.Subtitle;
        Description = meditation.Description;
        Category = meditation.Category;
        Difficulty = meditation.Difficulty;
        IconEmoji = meditation.IconEmoji;
        Benefits = meditation.BenefitsLabel;
        DurationLabel = meditation.DurationLabel;
        CurrentPrompt = "Choose a calm space, then press Start Narration.";
        StatusText = "Ready to begin";
        Progress = 0;
        CurrentStep = 0;
        TotalSteps = meditation.Script.Count;
        IsPlaying = false;
        IsCompleted = false;
    }

    [RelayCommand]
    public async Task StartNarrationAsync()
    {
        if (Meditation == null || IsPlaying)
            return;

        _playbackCts?.Cancel();
        _playbackCts = new CancellationTokenSource();
        var token = _playbackCts.Token;
        IsPlaying = true;
        IsCompleted = false;
        StatusText = "Narration in progress";

        try
        {
            for (var i = 0; i < Meditation.Script.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                CurrentStep = i + 1;
                CurrentPrompt = Meditation.Script[i];
                Progress = (double)(i + 1) / Meditation.Script.Count;

                await TextToSpeech.Default.SpeakAsync(Meditation.Script[i], new SpeechOptions
                {
                    Rate = 0.8f,
                    Pitch = 1f,
                    Volume = 1f
                }, token);

                await Task.Delay(900, token);
            }

            IsCompleted = true;
            StatusText = "Session complete";
            CurrentPrompt = "Take one more breath and notice how you feel now.";

            await _notifications.AddAsync(new AppNotification
            {
                Title = "Meditation Completed",
                Message = $"You finished {Meditation.Title}.",
                Icon = Meditation.IconEmoji,
                Category = NotificationCategory.Exercise
            });
        }
        catch (OperationCanceledException)
        {
            StatusText = "Narration stopped";
            if (!IsCompleted && Meditation != null)
                CurrentPrompt = "You can restart whenever you are ready.";
        }
        finally
        {
            IsPlaying = false;
        }
    }

    [RelayCommand]
    public void StopNarration()
    {
        StopPlayback();
        if (Meditation != null && !IsCompleted)
        {
            StatusText = "Narration stopped";
            CurrentPrompt = "You can restart whenever you are ready.";
        }
    }

    [RelayCommand]
    public async Task GoBackAsync()
    {
        StopPlayback();
        await Shell.Current.GoToAsync("..");
    }

    private void StopPlayback()
    {
        if (_playbackCts == null)
            return;

        _playbackCts.Cancel();
        _playbackCts.Dispose();
        _playbackCts = null;
        IsPlaying = false;
    }
}
