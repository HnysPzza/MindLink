using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using M1ndLink.Models;
using M1ndLink.Services;
using M1ndLink.Views;

namespace M1ndLink.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly IDatabaseService _db;
    private readonly INotificationService _notifications;
    private readonly IMoodService _mood;
    private readonly IReminderService _reminders;
    private readonly IAuthService _authService;
    private readonly IProfileService _profileService;
    private readonly INavigationService _navService;
    private readonly ICrisisSupportService _crisisSupportService;
    private readonly IServiceProvider _serviceProvider;
    private bool _isLoadingSettings;

    [ObservableProperty] private string _userName             = "Friend";
    [ObservableProperty] private string _personalGoal         = "Feel better every day";
    [ObservableProperty] private ImageSource? _profileImageSource = null;
    [ObservableProperty] private bool   _hasProfileImage      = false;
    [ObservableProperty] private int    _moodStreak           = 0;
    [ObservableProperty] private bool   _dailyNotifications   = false;
    [ObservableProperty] private bool   _morningReminderEnabled = true;
    [ObservableProperty] private TimeSpan _morningReminderTime = new(8, 0, 0);
    [ObservableProperty] private bool   _eveningReminderEnabled = true;
    [ObservableProperty] private TimeSpan _eveningReminderTime = new(20, 0, 0);
    [ObservableProperty] private bool   _streakReminderEnabled = true;
    [ObservableProperty] private bool   _exerciseSuggestionEnabled = true;
    [ObservableProperty] private int    _exercisesCompleted   = 0;
    [ObservableProperty] private int    _assessmentsCompleted = 0;
    [ObservableProperty] private double _exerciseRingProgress  = 0;
    [ObservableProperty] private double _assessmentRingProgress = 0;
    [ObservableProperty] private int    _emergencyContactCount = 0;
    [ObservableProperty] private string _primaryContactSummary = "No primary contact saved yet";
    [ObservableProperty] private string _notificationStatusSummary = "Notifications are off";

    public ProfileViewModel(IDatabaseService db, INotificationService notifications, IMoodService mood, IReminderService reminders, IAuthService authService, IProfileService profileService, INavigationService navService, ICrisisSupportService crisisSupportService, IServiceProvider serviceProvider) 
    { 
        _db = db; 
        _notifications = notifications;
        _mood = mood;
        _reminders = reminders;
        _authService = authService;
        _profileService = profileService;
        _navService = navService;
        _crisisSupportService = crisisSupportService;
        _serviceProvider = serviceProvider;
        Title = "Profile"; 
    }

    [RelayCommand]
    public async Task EditProfileAsync() =>
        await Shell.Current.GoToAsync("EditProfile");

    [RelayCommand]
    public async Task OpenSettingsAsync() =>
        await Shell.Current.GoToAsync("GeneralSettings");

    [RelayCommand]
    public async Task OpenEmergencyContactsAsync() =>
        await Shell.Current.GoToAsync("CrisisSupport");

    [RelayCommand]
    public async Task OpenAdvancedReportingAsync() =>
        await Shell.Current.GoToAsync("AdvancedReporting");

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
    public async Task LoadProfileAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            _isLoadingSettings = true;
            var profile = await _db.GetByIdAsync<UserProfile>(1);
            if (profile != null)
            {
                UserName           = profile.Name;
                PersonalGoal       = profile.PersonalGoal;
                DailyNotifications = profile.DailyNotifications;
                
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
                    HasProfileImage = false;
                }
            }

            var reminderSettings = await _reminders.GetSettingsAsync();
            DailyNotifications = reminderSettings.DailyNotificationsEnabled;
            MorningReminderEnabled = reminderSettings.MorningReminderEnabled;
            MorningReminderTime = TimeSpan.FromMinutes(reminderSettings.MorningReminderMinutes);
            EveningReminderEnabled = reminderSettings.EveningReminderEnabled;
            EveningReminderTime = TimeSpan.FromMinutes(reminderSettings.EveningReminderMinutes);
            StreakReminderEnabled = reminderSettings.StreakReminderEnabled;
            ExerciseSuggestionEnabled = reminderSettings.ExerciseSuggestionEnabled;
            NotificationStatusSummary = reminderSettings.DailyNotificationsEnabled
                ? $"Daily reminders on • Morning {MorningReminderTime:h\\:mm} • Evening {EveningReminderTime:h\\:mm}"
                : "Notifications are off";

            ExercisesCompleted   = (await _db.GetAllAsync<ExerciseSession>()).Count;
            AssessmentsCompleted = (await _db.GetAllAsync<AssessmentResult>()).Count;

            ExerciseRingProgress  = Math.Min(ExercisesCompleted  / 30.0, 1.0);
            AssessmentRingProgress = Math.Min(AssessmentsCompleted / 10.0, 1.0);

            MoodStreak = await _mood.GetMoodStreakAsync();

            var emergencyContacts = await _crisisSupportService.GetEmergencyContactsAsync();
            EmergencyContactCount = emergencyContacts.Count;
            var primaryContact = emergencyContacts.FirstOrDefault(contact => contact.IsPrimary) ?? emergencyContacts.FirstOrDefault();
            PrimaryContactSummary = primaryContact == null
                ? "No primary contact saved yet"
                : $"Primary: {primaryContact.Name} • {primaryContact.PhoneNumber}";
        }
        finally
        {
            _isLoadingSettings = false;
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SaveProfileAsync()
    {
        var existingProfile = await _db.GetByIdAsync<UserProfile>(1) ?? new UserProfile();
        var existingReminderSettings = await _reminders.GetSettingsAsync();
        existingProfile.Id = 1;
        existingProfile.Name = UserName;
        existingProfile.PersonalGoal = PersonalGoal;
        existingProfile.DailyNotifications = DailyNotifications;

        await _db.SaveAsync(existingProfile);

        await _reminders.SaveSettingsAsync(new ReminderSettings
        {
            Id = 1,
            DailyNotificationsEnabled = DailyNotifications,
            MorningReminderEnabled = MorningReminderEnabled,
            MorningReminderMinutes = (int)MorningReminderTime.TotalMinutes,
            EveningReminderEnabled = EveningReminderEnabled,
            EveningReminderMinutes = (int)EveningReminderTime.TotalMinutes,
            StreakReminderEnabled = StreakReminderEnabled,
            ExerciseSuggestionEnabled = ExerciseSuggestionEnabled,
            LastMorningReminderAt = existingReminderSettings.LastMorningReminderAt,
            LastEveningReminderAt = existingReminderSettings.LastEveningReminderAt,
            LastStreakReminderAt = existingReminderSettings.LastStreakReminderAt,
            LastExerciseSuggestionAt = existingReminderSettings.LastExerciseSuggestionAt
        });

        Preferences.Set("user_name", UserName);

        await _notifications.AddAsync(new AppNotification
        {
            Title    = "Profile Updated",
            Message  = "Your reminder preferences and profile details were updated.",
            Icon     = "👤",
            Category = NotificationCategory.Profile
        });

        await _reminders.EvaluateDueRemindersAsync();

        await Shell.Current.DisplayAlert("Saved", "Profile updated!", "OK");
    }

    partial void OnDailyNotificationsChanged(bool value)
    {
        if (_isLoadingSettings)
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

    [RelayCommand]
    public async Task SignOutAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert("Log Out", "Are you sure you want to log out?", "Yes", "No");
        if (!confirm) return;

        IsBusy = true;
        await _authService.SignOutAsync();
        await _db.ClearAllDataAsync(); // Nuke all local cached data
        IsBusy = false;

        Application.Current!.MainPage = _serviceProvider.GetRequiredService<OnboardingPage>();
    }
}
