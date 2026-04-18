using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;

namespace M1ndLink.ViewModels;

public enum SessionPhase { Ready, Active, Done }
public enum BreathPhase  { Inhale, Hold, Exhale }

public partial class ExerciseSessionViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IExerciseService     _exercises;
    private readonly INotificationService _notifications;

    // ── Exercise meta ─────────────────────────────────────────────────────
    [ObservableProperty] private Exercise?      _exercise;
    [ObservableProperty] private string         _exerciseTitle    = string.Empty;
    [ObservableProperty] private string         _exerciseEmoji    = string.Empty;
    [ObservableProperty] private string         _exerciseDesc     = string.Empty;
    [ObservableProperty] private Color          _gradientStart    = Color.FromArgb("#DBEAFE");
    [ObservableProperty] private Color          _gradientEnd      = Color.FromArgb("#BFDBFE");

    // ── Session state ─────────────────────────────────────────────────────
    [ObservableProperty] private SessionPhase   _phase            = SessionPhase.Ready;
    [ObservableProperty] private BreathPhase    _breathPhase      = BreathPhase.Inhale;
    [ObservableProperty] private string         _breathLabel      = "Breathe In";
    [ObservableProperty] private string         _phaseInstruction = "Find a comfortable position\nand press Begin when you're ready.";

    // ── Countdown ─────────────────────────────────────────────────────────
    [ObservableProperty] private int            _totalSeconds;       // full duration in seconds
    [ObservableProperty] private int            _remainingSeconds;   // ticking down
    [ObservableProperty] private string         _countdownDisplay    = "0:00";
    [ObservableProperty] private double         _sessionProgress     = 0;  // 0→1 for outer ring

    // ── Breathing ring ────────────────────────────────────────────────────
    [ObservableProperty] private double         _breathProgress      = 0;  // 0→1 (inhale), 1→0 (exhale)
    [ObservableProperty] private int            _breathPhaseSeconds;  // current phase countdown

    // ── Completion ────────────────────────────────────────────────────────
    [ObservableProperty] private string         _completionTitle     = "Great job! 🎉";
    [ObservableProperty] private string         _completionMessage   = string.Empty;
    [ObservableProperty] private bool           _finishedFully       = false;

    // ── Internals ─────────────────────────────────────────────────────────
    private IDispatcherTimer? _mainTimer;
    private IDispatcherTimer? _breathTimer;
    private int _elapsedSeconds;
    private int _breathElapsed;
    private int _inhale, _hold, _exhale;

    // Exposed so the page view can subscribe and drive SkiaSharp animations
    public event Action<double, BreathPhase>? BreathAnimationRequested;

    public ExerciseSessionViewModel(IExerciseService exercises, INotificationService notifications)
    {
        _exercises     = exercises;
        _notifications = notifications;
        Title = "Session";
    }

    // ── IQueryAttributable ────────────────────────────────────────────────
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("ExerciseId", out var val) &&
            int.TryParse(val?.ToString(), out int id))
        {
            var ex = _exercises.GetById(id);
            if (ex != null) LoadExercise(ex);
        }
    }

    private void LoadExercise(Exercise ex)
    {
        Exercise         = ex;
        ExerciseTitle    = ex.Title;
        ExerciseEmoji    = ex.IconEmoji;
        ExerciseDesc     = ex.Description;
        GradientStart    = ex.GradientStart;
        GradientEnd      = ex.GradientEnd;
        TotalSeconds     = ex.DurationMinutes * 60;
        RemainingSeconds = TotalSeconds;
        CountdownDisplay = FormatTime(TotalSeconds);
        SessionProgress  = 0;

        var phases = ExerciseService.BreathingPhases[ex.Type];
        _inhale = phases.Inhale;
        _hold   = phases.Hold;
        _exhale = phases.Exhale;

        PhaseInstruction = ex.Type switch
        {
            ExerciseType.Breathing  => $"Inhale {_inhale}s · Hold {_hold}s · Exhale {_exhale}s",
            ExerciseType.Meditation => "Focus on your breath and let thoughts pass.",
            ExerciseType.Grounding  => "Name 5 things you see, 4 you can touch, 3 you hear…",
            _                       => ex.Description
        };
    }

    // ── Commands ──────────────────────────────────────────────────────────
    [RelayCommand]
    public void Begin()
    {
        if (Phase != SessionPhase.Ready) return;
        _elapsedSeconds = 0;
        Phase = SessionPhase.Active;
        StartMainTimer();
        StartBreathCycle();
    }

    [RelayCommand]
    public async Task EndEarlyAsync()
    {
        StopTimers();
        FinishedFully = false;
        await FinalizeSessionAsync();
    }

    [RelayCommand]
    public async Task DoneAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    public async Task GoBackAsync()
    {
        StopTimers();
        await Shell.Current.GoToAsync("..");
    }

    // ── Main countdown timer ──────────────────────────────────────────────
    private void StartMainTimer()
    {
        _mainTimer = Application.Current!.Dispatcher.CreateTimer();
        _mainTimer.Interval = TimeSpan.FromSeconds(1);
        _mainTimer.Tick += OnMainTick;
        _mainTimer.Start();
    }

    private void OnMainTick(object? s, EventArgs e)
    {
        _elapsedSeconds++;
        RemainingSeconds = Math.Max(TotalSeconds - _elapsedSeconds, 0);
        CountdownDisplay = FormatTime(RemainingSeconds);
        SessionProgress  = (double)_elapsedSeconds / TotalSeconds;

        if (RemainingSeconds <= 0)
        {
            StopTimers();
            FinishedFully = true;
            _ = FinalizeSessionAsync();
        }
    }

    // ── Breathing phase cycle ─────────────────────────────────────────────
    private void StartBreathCycle() => AdvanceToBreathPhase(BreathPhase.Inhale);

    private void AdvanceToBreathPhase(BreathPhase next)
    {
        _breathTimer?.Stop();
        BreathPhase = next;
        _breathElapsed = 0;

        // How long this phase lasts
        int phaseDuration = next switch
        {
            BreathPhase.Inhale => _inhale,
            BreathPhase.Hold   => _hold > 0 ? _hold : 1,
            BreathPhase.Exhale => _exhale,
            _                  => _inhale
        };
        BreathPhaseSeconds = phaseDuration;

        BreathLabel = next switch
        {
            BreathPhase.Inhale => $"Breathe In  ({_inhale}s)",
            BreathPhase.Hold   => $"Hold  ({_hold}s)",
            BreathPhase.Exhale => $"Breathe Out  ({_exhale}s)",
            _                  => string.Empty
        };

        // Signal starting animated value to page
        double startProgress = next == BreathPhase.Inhale ? 0 : (next == BreathPhase.Hold ? 1 : 1);
        BreathAnimationRequested?.Invoke(startProgress, next);

        _breathTimer = Application.Current!.Dispatcher.CreateTimer();
        _breathTimer.Interval = TimeSpan.FromSeconds(1);
        _breathTimer.Tick += (_, _) => OnBreathTick(phaseDuration);
        _breathTimer.Start();
    }

    private void OnBreathTick(int phaseDuration)
    {
        if (Phase != SessionPhase.Active) { _breathTimer?.Stop(); return; }

        _breathElapsed++;
        double t = (double)_breathElapsed / phaseDuration;

        BreathProgress = BreathPhase switch
        {
            BreathPhase.Inhale => t,         // grow 0→1
            BreathPhase.Hold   => 1.0,        // stay full
            BreathPhase.Exhale => 1.0 - t,   // shrink 1→0
            _                  => 0
        };
        BreathPhaseSeconds = Math.Max(phaseDuration - _breathElapsed, 0);

        if (_breathElapsed >= phaseDuration)
        {
            var next = BreathPhase switch
            {
                BreathPhase.Inhale => _hold > 0 ? BreathPhase.Hold : BreathPhase.Exhale,
                BreathPhase.Hold   => BreathPhase.Exhale,
                BreathPhase.Exhale => BreathPhase.Inhale,
                _                  => BreathPhase.Inhale
            };
            AdvanceToBreathPhase(next);
        }
    }

    // ── Finalise session ──────────────────────────────────────────────────
    private async Task FinalizeSessionAsync()
    {
        Phase = SessionPhase.Done;

        int actualSeconds = _elapsedSeconds;
        string timeStr    = FormatTime(actualSeconds);

        CompletionTitle   = FinishedFully ? "You did it! 🎉" : "Good effort! 💪";
        CompletionMessage = FinishedFully
            ? $"You completed {ExerciseTitle} in {timeStr}.\nTake a moment to notice how you feel."
            : $"You spent {timeStr} on {ExerciseTitle}.\nEvery moment of mindfulness counts.";

        // Save SQLite session record (Syncs to Cloud automatically via hook)
        if (Exercise != null)
        {
            await _exercises.SaveSessionAsync(new ExerciseSession
            {
                ExerciseId      = Exercise.Id,
                ExerciseTitle   = Exercise.Title,
                ExerciseEmoji   = Exercise.IconEmoji,
                DurationSeconds = actualSeconds,
                FinishedFully   = FinishedFully,
                CompletedAt     = DateTime.Now
            });
        }

        await _notifications.AddAsync(new AppNotification
        {
            Title    = FinishedFully ? "Exercise Completed 🎉" : "Exercise Ended",
            Message  = FinishedFully
                           ? $"You completed {ExerciseTitle}. Great job!"
                           : $"You spent {timeStr} on {ExerciseTitle}.",
            Icon     = ExerciseEmoji,
            Category = NotificationCategory.Exercise
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private void StopTimers()
    {
        _mainTimer?.Stop();
        _breathTimer?.Stop();
    }

    private static string FormatTime(int totalSeconds)
    {
        var t = TimeSpan.FromSeconds(totalSeconds);
        return $"{(int)t.TotalMinutes}:{t.Seconds:D2}";
    }
}
