using M1ndLink.Models;

namespace M1ndLink.Services;

public class TodoService : ITodoService
{
    private readonly IDatabaseService _db;
    private readonly IPlatformNotificationService _platformNotifications;
    private readonly INotificationService _notifications;

    public TodoService(IDatabaseService db, IPlatformNotificationService platformNotifications, INotificationService notifications)
    {
        _db = db;
        _platformNotifications = platformNotifications;
        _notifications = notifications;
    }

    public async Task<List<TodoTask>> GetAllAsync()
    {
        var all = await _db.GetAllAsync<TodoTask>();
        return all.OrderBy(task => task.IsCompleted)
            .ThenBy(task => task.DueAt)
            .ThenByDescending(task => task.Priority)
            .ToList();
    }

    public async Task<TodoTask?> GetByIdAsync(int id) =>
        await _db.GetByIdAsync<TodoTask>(id);

    public async Task SaveAsync(TodoTask task)
    {
        await _db.SaveAsync(task);

        if (task.NotificationId == 0)
        {
            task.NotificationId = NotificationKeys.TodoReminderBaseId + task.Id;
            await _db.SaveAsync(task);
        }

        if (task.IsCompleted)
        {
            await _platformNotifications.CancelAsync(task.NotificationId);
            return;
        }

        var firstNotifyTime = task.ReminderAt <= DateTime.Now
            ? DateTime.Now.AddMinutes(1)
            : task.ReminderAt;

        await _platformNotifications.CancelAsync(task.NotificationId);
        await _platformNotifications.ScheduleRepeatingAsync(
            task.NotificationId,
            $"{task.CategoryIcon} {task.Title}",
            $"{task.CategoryLabel} • due {task.DueAt:h:mm tt}. Tap to open your task list.",
            firstNotifyTime,
            TimeSpan.FromMinutes(15),
            NotificationKeys.RoutePayload("TodoList"));
    }

    public async Task DeleteAsync(TodoTask task)
    {
        if (task.NotificationId != 0)
            await _platformNotifications.CancelAsync(task.NotificationId);

        await _db.DeleteAsync(task);
    }

    public async Task ToggleCompleteAsync(TodoTask task, bool isCompleted)
    {
        task.IsCompleted = isCompleted;
        task.CompletedAt = isCompleted ? DateTime.Now : null;
        await _db.SaveAsync(task);

        if (isCompleted)
        {
            if (task.NotificationId != 0)
                await _platformNotifications.CancelAsync(task.NotificationId);

            await _notifications.AddAsync(new AppNotification
            {
                Title = "Task Completed",
                Message = $"You finished {task.Title}.",
                Icon = task.CategoryIcon,
                Category = NotificationCategory.System
            });
        }
        else
        {
            await SaveAsync(task);
        }
    }

    public async Task<int> GetPendingCountAsync()
    {
        var all = await _db.GetAllAsync<TodoTask>();
        return all.Count(task => !task.IsCompleted);
    }
}
