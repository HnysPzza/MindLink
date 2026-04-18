using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;

namespace M1ndLink.ViewModels;

public partial class GeneralSettingsViewModel : BaseViewModel
{
    private readonly IReminderService _reminderService;
    private readonly IDatabaseService _databaseService;
    private readonly IThemeService _themeService;
    private readonly IDataExportService _dataExportService;
    private readonly IAuthService _authService;
    private bool _isLoading;

    [ObservableProperty] private bool _dailyNotifications;
    [ObservableProperty] private bool _morningReminderEnabled;
    [ObservableProperty] private TimeSpan _morningReminderTime = new(8, 0, 0);
    [ObservableProperty] private bool _eveningReminderEnabled;
    [ObservableProperty] private TimeSpan _eveningReminderTime = new(20, 0, 0);
    [ObservableProperty] private bool _streakReminderEnabled;
    [ObservableProperty] private bool _exerciseSuggestionEnabled;
    [ObservableProperty] private string _selectedTheme = "System";
    [ObservableProperty] private string _accountEmail = "Not signed in";
    [ObservableProperty] private string _exportStatus = "Export your data to a JSON file anytime.";
    [ObservableProperty] private string _appVersion = AppInfo.Current.VersionString;

    public IReadOnlyList<string> ThemeOptions { get; } = new[] { "System", "Light", "Dark" };

    public GeneralSettingsViewModel(IReminderService reminderService, IDatabaseService databaseService, IThemeService themeService, IDataExportService dataExportService, IAuthService authService)
    {
        _reminderService = reminderService;
        _databaseService = databaseService;
        _themeService = themeService;
        _dataExportService = dataExportService;
        _authService = authService;
        Title = "General Settings";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            _isLoading = true;
            var settings = await _reminderService.GetSettingsAsync();
            DailyNotifications = settings.DailyNotificationsEnabled;
            MorningReminderEnabled = settings.MorningReminderEnabled;
            MorningReminderTime = TimeSpan.FromMinutes(settings.MorningReminderMinutes);
            EveningReminderEnabled = settings.EveningReminderEnabled;
            EveningReminderTime = TimeSpan.FromMinutes(settings.EveningReminderMinutes);
            StreakReminderEnabled = settings.StreakReminderEnabled;
            ExerciseSuggestionEnabled = settings.ExerciseSuggestionEnabled;
            SelectedTheme = _themeService.GetThemeMode().ToString();
            AccountEmail = _authService.GetCurrentUserEmail() ?? "Not signed in";
        }
        finally
        {
            _isLoading = false;
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var profile = await _databaseService.GetByIdAsync<UserProfile>(1) ?? new UserProfile();
        var existingSettings = await _reminderService.GetSettingsAsync();
        profile.Id = 1;
        profile.DailyNotifications = DailyNotifications;
        await _databaseService.SaveAsync(profile);

        await _reminderService.SaveSettingsAsync(new ReminderSettings
        {
            Id = 1,
            DailyNotificationsEnabled = DailyNotifications,
            MorningReminderEnabled = MorningReminderEnabled,
            MorningReminderMinutes = (int)MorningReminderTime.TotalMinutes,
            EveningReminderEnabled = EveningReminderEnabled,
            EveningReminderMinutes = (int)EveningReminderTime.TotalMinutes,
            StreakReminderEnabled = StreakReminderEnabled,
            ExerciseSuggestionEnabled = ExerciseSuggestionEnabled,
            LastMorningReminderAt = existingSettings.LastMorningReminderAt,
            LastEveningReminderAt = existingSettings.LastEveningReminderAt,
            LastStreakReminderAt = existingSettings.LastStreakReminderAt,
            LastExerciseSuggestionAt = existingSettings.LastExerciseSuggestionAt
        });

        if (Enum.TryParse<AppThemeMode>(SelectedTheme, true, out var mode))
        {
            _themeService.SetThemeMode(mode);
        }

        await Shell.Current.DisplayAlert("Saved", "Your settings were updated.", "OK");
    }

    [RelayCommand]
    private async Task ExportDataAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            var filePath = await _dataExportService.ExportAsync();
            ExportStatus = $"Last export saved to: {filePath}";
            await Shell.Current.DisplayAlert("Export Complete", $"Your data was exported successfully.\n\n{filePath}", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Export Failed", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    partial void OnDailyNotificationsChanged(bool value)
    {
        if (_isLoading)
            return;

        if (!value)
        {
            MorningReminderEnabled = false;
            EveningReminderEnabled = false;
            StreakReminderEnabled = false;
            ExerciseSuggestionEnabled = false;
        }
        else if (!MorningReminderEnabled && !EveningReminderEnabled && !StreakReminderEnabled && !ExerciseSuggestionEnabled)
        {
            MorningReminderEnabled = true;
            EveningReminderEnabled = true;
            StreakReminderEnabled = true;
            ExerciseSuggestionEnabled = true;
        }
    }
}
