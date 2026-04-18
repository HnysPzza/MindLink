using CommunityToolkit.Maui;
using M1ndLink.Services;
using M1ndLink.ViewModels;
using M1ndLink.Views;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace M1ndLink;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .UseMauiCommunityToolkit()
            .UseLocalNotification()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("DMSans-Regular.ttf",           "DMSansRegular");
                fonts.AddFont("DMSans-Medium.ttf",            "DMSansMedium");
                fonts.AddFont("DMSans-SemiBold.ttf",          "DMSansSemiBold");
                fonts.AddFont("DMSans-Bold.ttf",              "DMSansBold");
                fonts.AddFont("PlusJakartaSans-Regular.ttf",  "JakartaRegular");
                fonts.AddFont("PlusJakartaSans-Medium.ttf",   "JakartaMedium");
                fonts.AddFont("PlusJakartaSans-SemiBold.ttf", "JakartaSemiBold");
                fonts.AddFont("Syne-Bold.ttf",                "SyneBold");
                fonts.AddFont("DMSans-Bold.ttf",              "NunitoBold");
                fonts.AddFont("DMSans-SemiBold.ttf",          "NunitoSemiBold");
                fonts.AddFont("Lato-Regular.ttf",             "Lato");
                fonts.AddFont("Lato-SemiBold.ttf",            "LatoSemiBold");
            })
            .ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                handlers.AddHandler(typeof(Shell), typeof(M1ndLink.Platforms.Android.MindFlowShellRenderer));
#endif
            });

        // Services (Singleton)
        builder.Services.AddSingleton(provider => 
        {
            var options = new Supabase.SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false // Disabled to prevent WebSocket connection failures on startup
            };
            var client = new Supabase.Client(Constants.SupabaseUrl, Constants.SupabaseKey, options);
            return client;
        });

        builder.Services.AddSingleton<IAuthService, SupabaseAuthService>();
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddSingleton<IMoodService, MoodService>();
        builder.Services.AddSingleton<IExerciseService, ExerciseService>();
        builder.Services.AddSingleton<IMeditationService, MeditationService>();
        builder.Services.AddSingleton<ICrisisSupportService, CrisisSupportService>();
        builder.Services.AddSingleton<M1ndLink.Services.INotificationService, NotificationService>();
        builder.Services.AddSingleton<IPlatformNotificationService, PlatformNotificationService>();
        builder.Services.AddSingleton<IReminderService, ReminderService>();
        builder.Services.AddSingleton<ITodoService, TodoService>();
        builder.Services.AddSingleton<IDailyTipService, DailyTipService>();
        builder.Services.AddSingleton<IProfileService, SupabaseProfileService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<ISyncService, CloudSyncService>();
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        builder.Services.AddSingleton<IDataExportService, DataExportService>();
        builder.Services.AddSingleton<IHabitService, HabitService>();
        builder.Services.AddSingleton<IMedicationService, MedicationService>();
        builder.Services.AddSingleton<ISleepService, SleepService>();
        builder.Services.AddSingleton<ITriggerService, TriggerService>();
        builder.Services.AddSingleton<IWeatherService, WeatherService>();

        // ViewModels (Transient)
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<SignUpViewModel>();
        builder.Services.AddTransient<VerifyOtpViewModel>();
        builder.Services.AddTransient<OnboardingViewModel>();
        builder.Services.AddTransient<ProfileSetupViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<AssessmentViewModel>();
        builder.Services.AddTransient<AssessmentHistoryViewModel>();
        builder.Services.AddTransient<ExercisesViewModel>();
        builder.Services.AddTransient<MeditationLibraryViewModel>();
        builder.Services.AddTransient<MeditationPlayerViewModel>();
        builder.Services.AddTransient<CrisisSupportViewModel>();
        builder.Services.AddTransient<TodoListViewModel>();
        builder.Services.AddTransient<AddEditTaskViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<GeneralSettingsViewModel>();
        builder.Services.AddTransient<NotificationsViewModel>();
        builder.Services.AddTransient<ExerciseSessionViewModel>();
        builder.Services.AddTransient<MoodJournalViewModel>();
        builder.Services.AddTransient<EditProfileViewModel>();
        builder.Services.AddTransient<SafetyPlanViewModel>();
        builder.Services.AddTransient<MedicationViewModel>();
        builder.Services.AddTransient<HabitsViewModel>();
        builder.Services.AddTransient<SleepDiaryViewModel>();
        builder.Services.AddTransient<TriggersViewModel>();
        builder.Services.AddTransient<AdvancedReportingViewModel>();

        // Pages (Transient)
        builder.Services.AddTransient<LoadingPage>();
        builder.Services.AddTransient<OnboardingPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<SignUpPage>();
        builder.Services.AddTransient<VerifyOtpPage>();
        builder.Services.AddTransient<ProfileSetupPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<AssessmentPage>();
        builder.Services.AddTransient<AssessmentHistoryPage>();
        builder.Services.AddTransient<ExercisesPage>();
        builder.Services.AddTransient<MeditationLibraryPage>();
        builder.Services.AddTransient<MeditationPlayerPage>();
        builder.Services.AddTransient<CrisisSupportPage>();
        builder.Services.AddTransient<TodoListPage>();
        builder.Services.AddTransient<AddEditTaskPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<GeneralSettingsPage>();
        builder.Services.AddTransient<NotificationsPage>();
        builder.Services.AddTransient<ExerciseSessionPage>();
        builder.Services.AddTransient<MoodJournalPage>();
        builder.Services.AddTransient<EditProfilePage>();
        builder.Services.AddTransient<SafetyPlanPage>();
        builder.Services.AddTransient<MedicationPage>();
        builder.Services.AddTransient<HabitsPage>();
        builder.Services.AddTransient<SleepDiaryPage>();
        builder.Services.AddTransient<TriggersPage>();
        builder.Services.AddTransient<AdvancedReportingPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
