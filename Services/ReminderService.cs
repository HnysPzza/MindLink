using M1ndLink.Models;

namespace M1ndLink.Services;

public class ReminderService : IReminderService
{
    private readonly IDatabaseService _db;
    private readonly IMoodService _mood;
    private readonly INotificationService _notifications;
    private readonly IPlatformNotificationService _platformNotifications;

    public ReminderService(IDatabaseService db, IMoodService mood, INotificationService notifications, IPlatformNotificationService platformNotifications)
    {
        _db = db;
        _mood = mood;
        _notifications = notifications;
        _platformNotifications = platformNotifications;
    }

    public async Task<ReminderSettings> GetSettingsAsync()
    {
        var settings = await _db.GetByIdAsync<ReminderSettings>(1);
        if (settings != null)
            return settings;

        var profile = await _db.GetByIdAsync<UserProfile>(1);
        return new ReminderSettings
        {
            DailyNotificationsEnabled = profile?.DailyNotifications ?? false
        };
    }

    public async Task SaveSettingsAsync(ReminderSettings settings)
    {
        settings.Id = 1;
        settings.MorningReminderMinutes = NormalizeMinutes(settings.MorningReminderMinutes, 8 * 60);
        settings.EveningReminderMinutes = NormalizeMinutes(settings.EveningReminderMinutes, 20 * 60);
        await _db.SaveAsync(settings);
        await _platformNotifications.SyncReminderNotificationsAsync(settings);
    }

    public async Task EvaluateDueRemindersAsync()
    {
        var settings = await GetSettingsAsync();
        if (!settings.DailyNotificationsEnabled)
            return;

        var now = DateTime.Now;
        var todayMood = await _mood.GetTodaysMoodAsync();
        var changed = false;

        if (settings.MorningReminderEnabled
            && todayMood == null
            && HasReachedTime(now, settings.MorningReminderMinutes)
            && !WasSentToday(settings.LastMorningReminderAt, now))
        {
            await _notifications.AddAsync(new AppNotification
            {
                Title = "Morning Check-In",
                Message = "How are you feeling today? Log your mood to start the day with clarity.",
                Icon = "🌤️",
                Category = NotificationCategory.Reminder
            });
            settings.LastMorningReminderAt = now;
            changed = true;
        }

        var needsEveningPrompt = todayMood == null || string.IsNullOrWhiteSpace(todayMood.Notes);
        if (settings.EveningReminderEnabled
            && needsEveningPrompt
            && HasReachedTime(now, settings.EveningReminderMinutes)
            && !WasSentToday(settings.LastEveningReminderAt, now))
        {
            await _notifications.AddAsync(new AppNotification
            {
                Title = "Evening Reflection",
                Message = todayMood == null
                    ? "Take a minute to log how today felt before you wind down."
                    : "Add a short note to today’s mood entry so you can reflect on what shaped your day.",
                Icon = "🌙",
                Category = NotificationCategory.Reminder
            });
            settings.LastEveningReminderAt = now;
            changed = true;
        }

        var recentEntries = await _mood.GetRecentEntriesAsync(14);
        var latestEntry = recentEntries.OrderByDescending(e => e.Date).FirstOrDefault();
        var streakAtRisk = latestEntry == null || latestEntry.Date.Date <= now.Date.AddDays(-2);
        if (settings.StreakReminderEnabled
            && streakAtRisk
            && now.Hour >= 11
            && !WasSentToday(settings.LastStreakReminderAt, now))
        {
            await _notifications.AddAsync(new AppNotification
            {
                Title = "Keep Your Streak Alive",
                Message = "You haven’t checked in for a couple of days. A quick mood log today keeps your momentum going.",
                Icon = "🔥",
                Category = NotificationCategory.Reminder
            });
            await _platformNotifications.ShowImmediateAsync(
                NotificationKeys.StreakReminderId,
                "Keep Your Streak Alive",
                "You haven’t checked in for a couple of days. A quick mood log today keeps your momentum going.",
                NotificationKeys.RoutePayload("//Home"));
            settings.LastStreakReminderAt = now;
            changed = true;
        }

        var latestAssessment = await _mood.GetLatestAssessmentAsync();
        var hasElevatedRisk = latestAssessment != null
            && latestAssessment.Date >= now.AddDays(-7)
            && (latestAssessment.RiskLevel == "High Level" || latestAssessment.RiskLevel == "Medium Level");
        var completedExercises = await _db.GetAllAsync<ExerciseSession>();
        var hasExerciseToday = completedExercises.Any(session => session.CompletedAt.Date == now.Date);
        if (settings.ExerciseSuggestionEnabled
            && hasElevatedRisk
            && !hasExerciseToday
            && now.Hour >= 14
            && !WasSentToday(settings.LastExerciseSuggestionAt, now))
        {
            await _notifications.AddAsync(new AppNotification
            {
                Title = "Try a Breathing Break",
                Message = latestAssessment!.RiskLevel == "High Level"
                    ? "Your recent assessment suggests a tougher day. A short breathing exercise might help you reset."
                    : "A quick grounding or breathing exercise could help you stay steady today.",
                Icon = "🫁",
                Category = NotificationCategory.Reminder
            });
            await _platformNotifications.ShowImmediateAsync(
                NotificationKeys.ExerciseReminderId,
                "Try a Breathing Break",
                latestAssessment!.RiskLevel == "High Level"
                    ? "Your recent assessment suggests a tougher day. A short breathing exercise might help you reset."
                    : "A quick grounding or breathing exercise could help you stay steady today.",
                NotificationKeys.RoutePayload("//Exercises"));
            settings.LastExerciseSuggestionAt = now;
            changed = true;
        }

        if (changed)
            await SaveSettingsAsync(settings);
    }

    private static bool HasReachedTime(DateTime now, int minutesFromMidnight)
    {
        var currentMinutes = (now.Hour * 60) + now.Minute;
        return currentMinutes >= minutesFromMidnight;
    }

    private static bool WasSentToday(DateTime? sentAt, DateTime now) =>
        sentAt.HasValue && sentAt.Value.Date == now.Date;

    private static int NormalizeMinutes(int value, int fallback)
    {
        if (value < 0 || value >= 24 * 60)
            return fallback;

        return value;
    }
}
