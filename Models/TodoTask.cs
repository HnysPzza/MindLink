using SQLite;

namespace M1ndLink.Models;

public enum TaskCategory
{
    SelfCare,
    Appointments,
    Medication,
    Goals,
    Personal,
    Work
}

public enum TaskPriority
{
    Low,
    Medium,
    High
}

[Table("TodoTasks")]
public class TodoTask
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskCategory Category { get; set; } = TaskCategory.Personal;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime DueAt { get; set; } = DateTime.Now.AddHours(4);
    public int ReminderOffsetMinutes { get; set; } = 30;
    public int NotificationId { get; set; }
    public DateTime? LastNotificationSentAt { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime ReminderAt => DueAt.AddMinutes(-ReminderOffsetMinutes);
    public string CategoryLabel => Category switch
    {
        TaskCategory.SelfCare => "Self-Care",
        TaskCategory.Appointments => "Appointments",
        TaskCategory.Medication => "Medication",
        TaskCategory.Goals => "Goals",
        TaskCategory.Personal => "Personal",
        _ => "Work"
    };
    public string CategoryIcon => Category switch
    {
        TaskCategory.SelfCare => "🌿",
        TaskCategory.Appointments => "📅",
        TaskCategory.Medication => "💊",
        TaskCategory.Goals => "🎯",
        TaskCategory.Personal => "✨",
        _ => "💼"
    };
    public string CategoryColor => Category switch
    {
        TaskCategory.SelfCare => "#14B8A6",
        TaskCategory.Appointments => "#3B82F6",
        TaskCategory.Medication => "#EC4899",
        TaskCategory.Goals => "#F59E0B",
        TaskCategory.Personal => "#8B5CF6",
        _ => "#475569"
    };
    public string PriorityLabel => Priority.ToString();
    public string PriorityColor => Priority switch
    {
        TaskPriority.Low => "#22C55E",
        TaskPriority.Medium => "#F59E0B",
        _ => "#EF4444"
    };
    public string DueDisplay => DueAt.ToString("MMM d • h:mm tt");
    public bool IsOverdue => !IsCompleted && DueAt < DateTime.Now;
}
