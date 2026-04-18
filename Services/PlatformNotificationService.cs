using M1ndLink.Models;
using Plugin.LocalNotification;

namespace M1ndLink.Services;

public class PlatformNotificationService : IPlatformNotificationService
{
    public async Task<bool> EnsurePermissionsAsync()
    {
        if (DeviceInfo.Platform != DevicePlatform.Android && DeviceInfo.Platform != DevicePlatform.iOS)
            return true;

        if (await LocalNotificationCenter.Current.AreNotificationsEnabled())
            return true;

        try
        {
            return await LocalNotificationCenter.Current.RequestNotificationPermission();
        }
        catch
        {
            return false;
        }
    }

    public async Task SyncReminderNotificationsAsync(ReminderSettings settings)
    {
        await CancelAsync(NotificationKeys.MorningReminderId);
        await CancelAsync(NotificationKeys.EveningReminderId);

        if (!settings.DailyNotificationsEnabled)
            return;

        var hasPermission = await EnsurePermissionsAsync();
        if (!hasPermission)
            return;

        if (settings.MorningReminderEnabled)
        {
            await ShowAsync(new NotificationRequest
            {
                NotificationId = NotificationKeys.MorningReminderId,
                Title = "Morning Check-In",
                Description = "How are you feeling today? Open M1ndLink and log your mood.",
                ReturningData = NotificationKeys.RoutePayload("//Home"),
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = GetNextOccurrence(settings.MorningReminderMinutes),
                    RepeatType = NotificationRepeat.Daily
                }
            });
        }

        if (settings.EveningReminderEnabled)
        {
            await ShowAsync(new NotificationRequest
            {
                NotificationId = NotificationKeys.EveningReminderId,
                Title = "Evening Reflection",
                Description = "Take a quiet minute to reflect and add a note for today.",
                ReturningData = NotificationKeys.RoutePayload("//Home"),
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = GetNextOccurrence(settings.EveningReminderMinutes),
                    RepeatType = NotificationRepeat.Daily
                }
            });
        }
    }

    public async Task ShowImmediateAsync(int notificationId, string title, string message, string payload)
    {
        var hasPermission = await EnsurePermissionsAsync();
        if (!hasPermission)
            return;

        await ShowAsync(new NotificationRequest
        {
            NotificationId = notificationId,
            Title = title,
            Description = message,
            ReturningData = payload
        });
    }

    public async Task ScheduleRepeatingAsync(int notificationId, string title, string message, DateTime firstNotifyTime, TimeSpan repeatInterval, string payload)
    {
        var hasPermission = await EnsurePermissionsAsync();
        if (!hasPermission)
            return;

        await ShowAsync(new NotificationRequest
        {
            NotificationId = notificationId,
            Title = title,
            Description = message,
            ReturningData = payload,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = firstNotifyTime,
                RepeatType = NotificationRepeat.TimeInterval,
                NotifyRepeatInterval = repeatInterval
            }
        });
    }

    public async Task CancelAsync(int notificationId)
    {
        LocalNotificationCenter.Current.Cancel(notificationId);
        await Task.CompletedTask;
    }

    private static DateTime GetNextOccurrence(int minutesFromMidnight)
    {
        var now = DateTime.Now;
        var time = now.Date.AddMinutes(minutesFromMidnight);
        return time <= now ? time.AddDays(1) : time;
    }

    private static async Task ShowAsync(NotificationRequest request)
    {
        try
        {
            await LocalNotificationCenter.Current.Show(request);
        }
        catch (Exception)
        {
            // Swallow SecurityException thrown on Android 12+ when
            // SCHEDULE_EXACT_ALARM permission has not been granted by the user
            // in Settings > Special App Access > Alarms & Reminders.
        }
    }
}
