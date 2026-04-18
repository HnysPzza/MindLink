using M1ndLink.Services;
using M1ndLink.Views;
using M1ndLink.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Plugin.LocalNotification;

namespace M1ndLink;

public partial class App : Application
{
    private readonly IAuthService _authService;
    private readonly IProfileService _profileService;
    private readonly IDatabaseService _db;
    private readonly INavigationService _navService;
    private readonly Supabase.Client _supabaseClient;
    private readonly IReminderService _reminderService;
    private readonly IPlatformNotificationService _platformNotificationService;
    private readonly IThemeService _themeService;
    private readonly IServiceProvider _serviceProvider;
    private readonly LoadingPage _loadingPage;

    public App(IAuthService authService, IProfileService profileService, IDatabaseService db, Supabase.Client supabaseClient, INavigationService navService, IReminderService reminderService, IPlatformNotificationService platformNotificationService, IThemeService themeService, IServiceProvider serviceProvider) 
    { 
        InitializeComponent(); 
        
        _authService = authService;
        _profileService = profileService;
        _db = db;
        _navService = navService;
        _supabaseClient = supabaseClient;
        _reminderService = reminderService;
        _platformNotificationService = platformNotificationService;
        _themeService = themeService;
        _serviceProvider = serviceProvider;
        
        // Resolve the loading page only after application resources are fully registered (InitializeComponent has already run)
        _loadingPage = _serviceProvider.GetRequiredService<LoadingPage>();

        _themeService.ApplySavedTheme();

        LocalNotificationCenter.Current.NotificationActionTapped += async e =>
        {
            if (e.IsDismissed || string.IsNullOrWhiteSpace(e.Request.ReturningData))
                return;

            if (!e.Request.ReturningData.StartsWith("route:"))
                return;

            var route = e.Request.ReturningData[6..];
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Current?.MainPage is Shell)
                    await Shell.Current.GoToAsync(route);
            });
        };

        MainPage = _loadingPage;
    }

    protected override async void OnStart()
    {
        base.OnStart();

        try
        {
            _loadingPage.SetStatus("Connecting securely...");
            await _supabaseClient.InitializeAsync();

            _loadingPage.SetStatus("Enabling reminders...");
            await _platformNotificationService.EnsurePermissionsAsync();
            await _platformNotificationService.SyncReminderNotificationsAsync(await _reminderService.GetSettingsAsync());

            _loadingPage.SetStatus("Checking your session...");
            
            // Try to restore previous session from secure storage
            await _authService.TryRestoreSessionAsync();
            
            if (_authService.IsUserLoggedIn())
            {
                await _navService.NavigateAfterLoginAsync();
            }
            else
            {
                MainPage = _serviceProvider.GetRequiredService<OnboardingPage>();
            }
        }
        catch (Exception ex)
        {
            MainPage = new ContentPage 
            { 
                BackgroundColor = Colors.White,
                Content = new Label 
                { 
                    Text = $"Failed to connect to Supabase: {ex.Message}\nMake sure your Constants.cs has valid URLs.", 
                    VerticalOptions = LayoutOptions.Center, 
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Colors.Red,
                    Margin = new Thickness(20)
                } 
            };
        }
    }
}