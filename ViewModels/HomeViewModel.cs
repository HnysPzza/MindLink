using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;
using System.Collections.ObjectModel;

namespace M1ndLink.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly IMoodService _mood;
    private readonly INotificationService _notifications;
    private readonly IDatabaseService _db;
    private readonly IReminderService _reminders;
    private readonly ITodoService _todoService;
    private readonly IDailyTipService _dailyTipService;
    private readonly ICrisisSupportService _crisisSupportService;
    private readonly IWeatherService _weatherService;

    [ObservableProperty] private string _greeting           = "Hi there 👋";
    [ObservableProperty] private string _currentMoodEmoji   = "😐";
    [ObservableProperty] private string _currentMoodLabel   = "You're feeling Neutral right now.";
    [ObservableProperty] private int    _moodStreak         = 0;
    [ObservableProperty] private string _riskLevel          = "Low Level";
    [ObservableProperty] private string _riskColor          = "#22C55E";
    [ObservableProperty] private string _userName           = "Friend";
    [ObservableProperty] private ObservableCollection<float> _moodChartData = new();

    // ── Weekly summary ────────────────────────────────────────────────────
    [ObservableProperty] private string _weeklySummaryText      = "";
    [ObservableProperty] private string _weeklySummaryEmoji     = "📊";
    [ObservableProperty] private bool   _hasWeeklySummary       = false;
    [ObservableProperty] private ObservableCollection<WeekDayEntry> _weekDays = new();

    // ── Risk history chart ────────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<float> _riskChartData = new();
    [ObservableProperty] private bool   _hasRiskData = false;

    [ObservableProperty] private ImageSource? _profileImageSource = null;
    [ObservableProperty] private bool         _hasProfileImage    = false;
    [ObservableProperty] private int  _unreadNotificationCount  = 0;
    [ObservableProperty] private bool _hasUnreadNotifications   = false;
    [ObservableProperty] private string _dailyTipTitle = string.Empty;
    [ObservableProperty] private string _dailyTipContent = string.Empty;
    [ObservableProperty] private string _dailyTipIcon = "💡";
    [ObservableProperty] private int _pendingTaskCount = 0;
    [ObservableProperty] private ObservableCollection<HomeActivityItem> _recentActivities = new();
    [ObservableProperty] private bool _hasRecentActivities = false;

    // ── Two-step mood log state ──────────────────────────────────────────
    [ObservableProperty] private int    _pendingMoodLevel   = 0;   // 0 = nothing selected
    [ObservableProperty] private string _pendingMoodEmoji   = string.Empty;
    [ObservableProperty] private string _pendingMoodLabel   = string.Empty;
    [ObservableProperty] private string _moodNoteText       = string.Empty;
    [ObservableProperty] private bool   _isNoteEntryVisible = false; // step-2 panel

    public HomeViewModel(IMoodService mood, INotificationService notifications, IDatabaseService db, IReminderService reminders, ITodoService todoService, IDailyTipService dailyTipService, ICrisisSupportService crisisSupportService, IWeatherService weatherService)
    { 
        _mood = mood; 
        _notifications = notifications;
        _db = db;
        _reminders = reminders;
        _todoService = todoService;
        _dailyTipService = dailyTipService;
        _crisisSupportService = crisisSupportService;
        _weatherService = weatherService;
        Title = "Home"; 
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            await _reminders.EvaluateDueRemindersAsync();

            var profile = await _db.GetByIdAsync<UserProfile>(1);
            if (profile != null)
            {
                UserName = profile.Name;
                Greeting = $"Hi, {UserName} 👋";

                var avatarPath = profile.AvatarPath;
                if (!string.IsNullOrEmpty(avatarPath))
                {
                    if (avatarPath.StartsWith("http"))
                    {
                        ProfileImageSource = ImageSource.FromUri(new Uri(avatarPath));
                        HasProfileImage    = true;
                    }
                    else if (File.Exists(avatarPath))
                    {
                        ProfileImageSource = ImageSource.FromFile(avatarPath);
                        HasProfileImage    = true;
                    }
                    else
                    {
                        ProfileImageSource = null;
                        HasProfileImage    = false;
                    }
                }
                else
                {
                    ProfileImageSource = null;
                    HasProfileImage    = false;
                }
            }
            else
            {
                UserName = "Friend";
                Greeting = "Hi, Friend 👋";
                ProfileImageSource = null;
                HasProfileImage    = false;
            }

            var notifs = await _notifications.GetAllAsync();
            UnreadNotificationCount = notifs.Count(n => !n.IsRead);
            HasUnreadNotifications  = UnreadNotificationCount > 0;
            PendingTaskCount = await _todoService.GetPendingCountAsync();

            var dailyTip = await _dailyTipService.GetTipOfTheDayAsync();
            DailyTipTitle = dailyTip.Title;
            DailyTipContent = dailyTip.Content;
            DailyTipIcon = dailyTip.Icon;

            var today = await _mood.GetTodaysMoodAsync();
            if (today != null)
            {
                CurrentMoodEmoji = today.MoodEmoji;
                CurrentMoodLabel = $"You're feeling {today.MoodLabel} right now.";
            }

            MoodStreak = await _mood.GetMoodStreakAsync();

            var assessment = await _mood.GetLatestAssessmentAsync();
            if (assessment != null)
            {
                RiskLevel = assessment.RiskLevel;
                RiskColor = assessment.RiskLevel switch
                {
                    "Low Level"    => "#22C55E",
                    "Medium Level" => "#F59E0B",
                    _              => "#EF4444"
                };
            }

            var entries = await _mood.GetRecentEntriesAsync(30);
            MoodChartData = new ObservableCollection<float>(entries.Select(e => (float)e.MoodLevel));

            // ── Weekly summary ──────────────────────────────────────────
            var weekEntries = await _mood.GetRecentEntriesAsync(7);
            var thisWeek = weekEntries.Where(e => e.Date >= DateTime.Today.AddDays(-6)).ToList();

            // Build 7-day strip (Mon-Sun or last 7 days)
            var weekDays = new ObservableCollection<WeekDayEntry>();
            for (int i = 6; i >= 0; i--)
            {
                var date  = DateTime.Today.AddDays(-i);
                var match = thisWeek.FirstOrDefault(e => e.Date.Date == date);
                weekDays.Add(new WeekDayEntry
                {
                    DayLabel  = date.ToString("ddd")[..2],
                    MoodEmoji = match?.MoodEmoji ?? "",
                    HasEntry  = match != null,
                    MoodColor = match != null ? MoodLevelToColor(match.MoodLevel) : "#E5E7EB"
                });
            }
            WeekDays = weekDays;

            if (thisWeek.Count > 0)
            {
                HasWeeklySummary = true;
                var dominant = thisWeek.GroupBy(e => e.MoodLabel)
                    .OrderByDescending(g => g.Count()).First();
                WeeklySummaryText  = $"This week you felt {dominant.Key} {dominant.Count()} of {thisWeek.Count} days";
                WeeklySummaryEmoji = thisWeek.OrderByDescending(e => e.Date).First().MoodEmoji;
            }
            else
            {
                HasWeeklySummary = false;
            }

            // ── Risk history chart ──────────────────────────────────────
            var assessments = await _mood.GetAssessmentsAsync();
            var sortedAssessments = assessments.OrderBy(a => a.Date).ToList();
            if (sortedAssessments.Count >= 2)
            {
                HasRiskData = true;
                RiskChartData = new ObservableCollection<float>(
                    sortedAssessments.Select(a => RiskLevelToFloat(a.RiskLevel)));
            }
            else
            {
                HasRiskData = false;
            }

            var recentExercises = await _db.GetAllAsync<ExerciseSession>();
            var recentTasks = await _db.GetAllAsync<TodoTask>();
            var activityItems = new List<HomeActivityItem>();

            activityItems.AddRange(entries
                .OrderByDescending(entry => entry.Date)
                .Take(2)
                .Select(entry => new HomeActivityItem
                {
                    Icon = entry.MoodEmoji,
                    Title = $"Mood logged: {entry.MoodLabel}",
                    Subtitle = string.IsNullOrWhiteSpace(entry.Notes) ? "Mood check-in saved." : entry.Notes,
                    OccurredAt = entry.Date
                }));

            activityItems.AddRange(sortedAssessments
                .OrderByDescending(assessmentItem => assessmentItem.Date)
                .Take(2)
                .Select(assessmentItem => new HomeActivityItem
                {
                    Icon = "📋",
                    Title = $"Assessment: {assessmentItem.RiskLevel}",
                    Subtitle = "Your emotional check-in was saved.",
                    OccurredAt = assessmentItem.Date
                }));

            activityItems.AddRange(recentExercises
                .OrderByDescending(session => session.CompletedAt)
                .Take(2)
                .Select(session => new HomeActivityItem
                {
                    Icon = session.ExerciseEmoji,
                    Title = $"Exercise: {session.ExerciseTitle}",
                    Subtitle = session.FinishedFully ? "Completed fully." : "Ended early but still counted.",
                    OccurredAt = session.CompletedAt
                }));

            activityItems.AddRange(recentTasks
                .Where(task => task.CompletedAt.HasValue)
                .OrderByDescending(task => task.CompletedAt)
                .Take(2)
                .Select(task => new HomeActivityItem
                {
                    Icon = task.CategoryIcon,
                    Title = $"Task done: {task.Title}",
                    Subtitle = $"{task.CategoryLabel} • {task.PriorityLabel} priority",
                    OccurredAt = task.CompletedAt!.Value
                }));

            RecentActivities = new ObservableCollection<HomeActivityItem>(activityItems
                .OrderByDescending(item => item.OccurredAt)
                .Take(5));
            HasRecentActivities = RecentActivities.Count > 0;
        }
        finally { IsBusy = false; }
    }

    /// <summary>Step 1 — user taps an emoji to select a mood level.</summary>
    [RelayCommand]
    public void SelectMood(object? parameter)
    {
        if (!int.TryParse(parameter?.ToString(), out int level) || level < 1 || level > 5)
            return;

        var tmp = new MoodEntry { MoodLevel = level };
        PendingMoodLevel   = level;
        PendingMoodEmoji   = tmp.MoodEmoji;
        PendingMoodLabel   = tmp.MoodLabel;
        MoodNoteText       = string.Empty;  // clear previous draft
        IsNoteEntryVisible = true;
    }

    /// <summary>Step 2 — user confirms and saves with the optional note.</summary>
    [RelayCommand]
    public async Task ConfirmMoodAsync()
    {
        if (IsBusy || PendingMoodLevel == 0) return;
        IsBusy = true;
        try
        {
            // Fetch weather silently in background (won't block or crash if unavailable)
            var weather = await _weatherService.GetCurrentWeatherAsync();

            var today = await _mood.GetTodaysMoodAsync();
            if (today != null)
            {
                today.MoodLevel = PendingMoodLevel;
                today.Notes     = MoodNoteText.Trim();
                today.Date      = DateTime.Now;
                if (weather != null)
                {
                    today.Temperature      = weather.TemperatureCelsius;
                    today.WeatherCondition = weather.Condition;
                    today.WeatherEmoji     = weather.Emoji;
                }
                await _mood.SaveMoodAsync(today);
            }
            else
            {
                await _mood.SaveMoodAsync(new MoodEntry
                {
                    MoodLevel          = PendingMoodLevel,
                    Notes              = MoodNoteText.Trim(),
                    Temperature        = weather?.TemperatureCelsius,
                    WeatherCondition   = weather?.Condition,
                    WeatherEmoji       = weather?.Emoji,
                });
            }

            await _notifications.AddAsync(new AppNotification
            {
                Title    = "Mood Logged",
                Message  = $"You logged feeling {PendingMoodLabel} today.",
                Icon     = PendingMoodEmoji,
                Category = NotificationCategory.Mood
            });

            // Reset panel
            IsNoteEntryVisible = false;
            MoodNoteText       = string.Empty;
            PendingMoodLevel   = 0;

            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Could not save mood: {ex.Message}", "OK");
        }
        finally { IsBusy = false; }
    }

    /// <summary>Dismiss the note panel without saving.</summary>
    [RelayCommand]
    public void CancelMoodEntry()
    {
        IsNoteEntryVisible = false;
        MoodNoteText       = string.Empty;
        PendingMoodLevel   = 0;
    }

    [RelayCommand]
    public async Task StartDictationAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
#if ANDROID || IOS
            var isGranted = await Permissions.CheckStatusAsync<Permissions.Microphone>();
            if (isGranted != PermissionStatus.Granted)
            {
                isGranted = await Permissions.RequestAsync<Permissions.Microphone>();
            }

            if (isGranted != PermissionStatus.Granted)
            {
                await Shell.Current.DisplayAlert("Permission Denied", "Microphone access is required.", "OK");
                return;
            }

            // In a full implementation, we'd call SpeechToText.Default.ListenAsync here.
            // Using a stub alert for now pending CommunityToolkit.Maui.Media injection.
            await Shell.Current.DisplayAlert("Listening", "(Speech-to-Text recording simulation: Speak now)", "OK");
            MoodNoteText += " [Voice dictation added...]"; 
#else
            await Shell.Current.DisplayAlert("Not Supported", "Voice input is only supported on mobile devices.", "OK");
#endif
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Could not start dictation: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task OpenNotificationsAsync() =>
        await Shell.Current.GoToAsync("Notifications");

    [RelayCommand]
    public async Task NavigateToAssessmentAsync() =>
        await Shell.Current.GoToAsync("//Assessment");

    [RelayCommand]
    public async Task ViewJournalAsync() =>
        await Shell.Current.GoToAsync("MoodJournal");

    [RelayCommand]
    public async Task OpenCrisisSupportAsync() =>
        await Shell.Current.GoToAsync("CrisisSupport");

    [RelayCommand]
    public async Task TriggerSosAsync()
    {
        var primaryContact = await _crisisSupportService.GetPrimaryEmergencyContactAsync();
        if (primaryContact == null)
        {
            await Shell.Current.DisplayAlert("No Primary Contact", "Add a primary emergency contact first so one-tap SOS can call the right person.", "OK");
            await Shell.Current.GoToAsync("CrisisSupport");
            return;
        }

        var success = await _crisisSupportService.TryCallAsync(primaryContact.PhoneNumber);
        if (!success)
            await Shell.Current.DisplayAlert("Call Not Available", "Your device could not start the phone call.", "OK");
    }

    [RelayCommand]
    public async Task OpenTodoListAsync() =>
        await Shell.Current.GoToAsync("TodoList");

    [RelayCommand]
    public async Task OpenExercisesAsync() =>
        await Shell.Current.GoToAsync("//Exercises");

    [RelayCommand]
    public async Task OpenHabitsAsync() =>
        await Shell.Current.GoToAsync("Habits");

    [RelayCommand]
    public async Task OpenMedicationAsync() =>
        await Shell.Current.GoToAsync("Medication");

    [RelayCommand]
    public async Task OpenSleepDiaryAsync() =>
        await Shell.Current.GoToAsync("SleepDiary");

    [RelayCommand]
    public async Task OpenTriggersAsync() =>
        await Shell.Current.GoToAsync("Triggers");

    [RelayCommand]
    public void StartMoodCheckIn()
    {
        if (PendingMoodLevel == 0)
        {
            PendingMoodLevel = 3;
            PendingMoodEmoji = "😐";
            PendingMoodLabel = "Neutral";
        }

        IsNoteEntryVisible = true;
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private static string MoodLevelToColor(int level) => level switch
    {
        1 => "#EF4444",  // red
        2 => "#F59E0B",  // amber
        3 => "#A3A3A3",  // neutral gray
        4 => "#22C55E",  // green
        5 => "#2563EB",  // blue
        _ => "#E5E7EB"
    };

    private static float RiskLevelToFloat(string riskLevel) => riskLevel switch
    {
        "Low Level"    => 1f,
        "Medium Level" => 2f,
        "High Level"   => 3f,
        _              => 1f
    };
}

/// <summary>Tiny data object for the 7-day week strip on the Home page.</summary>
public class WeekDayEntry
{
    public string DayLabel  { get; set; } = string.Empty;  // "Mo", "Tu", etc.
    public string MoodEmoji { get; set; } = string.Empty;
    public bool   HasEntry  { get; set; }
    public string MoodColor { get; set; } = "#E5E7EB";
}
