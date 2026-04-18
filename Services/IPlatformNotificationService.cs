using M1ndLink.Models;

namespace M1ndLink.Services;

public interface IPlatformNotificationService
{
    Task<bool> EnsurePermissionsAsync();
    Task SyncReminderNotificationsAsync(ReminderSettings settings);
    Task ShowImmediateAsync(int notificationId, string title, string message, string payload);
    Task ScheduleRepeatingAsync(int notificationId, string title, string message, DateTime firstNotifyTime, TimeSpan repeatInterval, string payload);
    Task CancelAsync(int notificationId);
}
