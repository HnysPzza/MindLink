using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;
using System.Collections.ObjectModel;

namespace M1ndLink.ViewModels;

// ── Simple data row for chart display ────────────────────────────────────────
public record CorrelationRow(string Label, double AvgMood, int Count);
public record WeatherMoodRow(string Emoji, string Condition, double AvgMood);

public partial class AdvancedReportingViewModel : BaseViewModel
{
    private readonly IDatabaseService _db;
    private readonly ISleepService    _sleepService;
    private readonly IHabitService    _habitService;

    // ── Sleep correlation ─────────────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<CorrelationRow> _sleepMoodRows = new();
    [ObservableProperty] private double _avgMoodGoodSleep  = 0;
    [ObservableProperty] private double _avgMoodPoorSleep  = 0;
    [ObservableProperty] private string _sleepInsight      = "Log more sleep and mood data to unlock insights.";

    // ── Habit correlation ─────────────────────────────────────────────────────
    [ObservableProperty] private int    _habitStreakTotal  = 0;
    [ObservableProperty] private string _habitInsight      = "Keep building routines to see trends here.";

    // ── Weather mood correlation ──────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<WeatherMoodRow> _weatherMoodRows = new();
    [ObservableProperty] private bool _hasWeatherData = false;
    [ObservableProperty] private string _weatherInsight    = "Log mood during different weather conditions to see correlations.";

    // ── 30-day overview ───────────────────────────────────────────────────────
    [ObservableProperty] private int    _totalMoodLogs     = 0;
    [ObservableProperty] private double _overallAvgMood    = 0;
    [ObservableProperty] private string _overallMoodEmoji  = "😐";
    [ObservableProperty] private ObservableCollection<float> _moodTrend = new();

    public AdvancedReportingViewModel(IDatabaseService db, ISleepService sleepService, IHabitService habitService)
    {
        _db           = db;
        _sleepService = sleepService;
        _habitService = habitService;
        Title = "Wellness Insights";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var allMoods = await _db.GetAllAsync<MoodEntry>();
            var last30   = allMoods
                .Where(m => m.Date >= DateTime.Today.AddDays(-30))
                .OrderBy(m => m.Date)
                .ToList();

            TotalMoodLogs  = last30.Count;
            MoodTrend      = last30.Count > 0
                ? new ObservableCollection<float>(last30.Select(m => (float)m.MoodLevel))
                : new ObservableCollection<float>();

            if (last30.Count > 0)
            {
                OverallAvgMood   = last30.Average(m => m.MoodLevel);
                OverallMoodEmoji = OverallAvgMood switch { >= 4.5 => "😄", >= 3.5 => "🙂", >= 2.5 => "😐", >= 1.5 => "😔", _ => "😢" };
            }

            await LoadSleepCorrelationAsync(last30);
            await LoadHabitCorrelationAsync();
            LoadWeatherCorrelation(last30);
        }
        finally { IsBusy = false; }
    }

    private async Task LoadSleepCorrelationAsync(List<MoodEntry> moodLogs)
    {
        var sleepEntries = await _sleepService.GetAllSleepEntriesAsync();
        if (sleepEntries.Count < 3 || moodLogs.Count < 3) return;

        var joined = (from s in sleepEntries
                      join m in moodLogs on s.Date.Date equals m.Date.Date
                      select new { s.HoursSlept, m.MoodLevel }).ToList();

        if (joined.Count < 2) return;

        var goodSleep = joined.Where(x => x.HoursSlept >= 7).ToList();
        var poorSleep = joined.Where(x => x.HoursSlept < 7).ToList();

        AvgMoodGoodSleep = goodSleep.Count > 0 ? goodSleep.Average(x => x.MoodLevel) : 0;
        AvgMoodPoorSleep = poorSleep.Count > 0 ? poorSleep.Average(x => x.MoodLevel) : 0;

        SleepMoodRows = new ObservableCollection<CorrelationRow>
        {
            new("≥ 7 hrs 😴", AvgMoodGoodSleep, goodSleep.Count),
            new("< 7 hrs 😩", AvgMoodPoorSleep, poorSleep.Count),
        };

        var diff = AvgMoodGoodSleep - AvgMoodPoorSleep;
        SleepInsight = diff > 0.5
            ? $"You tend to feel {diff:F1} points happier after a good night's sleep!"
            : diff < -0.3
            ? "Your mood appears resilient — you handle poor sleep quite well."
            : "Sleep quality doesn't appear to strongly affect your mood yet.";
    }

    private async Task LoadHabitCorrelationAsync()
    {
        var habits = await _habitService.GetAllHabitsAsync();
        HabitStreakTotal = habits.Sum(h => h.CurrentStreak);
        HabitInsight = HabitStreakTotal > 10
            ? $"Amazing! You have {HabitStreakTotal} active streak days across all habits. Keep it up!"
            : "Build streaks to see how your routines affect your overall wellbeing.";
    }

    private void LoadWeatherCorrelation(List<MoodEntry> moodLogs)
    {
        var withWeather = moodLogs.Where(m => !string.IsNullOrEmpty(m.WeatherCondition)).ToList();
        if (withWeather.Count < 3) return;

        var grouped = withWeather
            .GroupBy(m => m.WeatherCondition!)
            .Select(g => new WeatherMoodRow(
                g.First().WeatherEmoji ?? "🌡️",
                g.Key,
                g.Average(m => m.MoodLevel)))
            .OrderByDescending(r => r.AvgMood)
            .ToList();

        WeatherMoodRows = new ObservableCollection<WeatherMoodRow>(grouped);
        HasWeatherData  = WeatherMoodRows.Count > 0;

        if (grouped.Count > 1)
        {
            var best  = grouped.First();
            var worst = grouped.Last();
            WeatherInsight = $"You feel best during {best.Emoji} {best.Condition} days (avg {best.AvgMood:F1}/5)"
                           + $" and lowest during {worst.Emoji} {worst.Condition} (avg {worst.AvgMood:F1}/5).";
        }
    }

    [RelayCommand]
    public async Task GoBackAsync() => await Shell.Current.GoToAsync("..");
}
