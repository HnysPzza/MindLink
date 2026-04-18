using SQLite;

namespace M1ndLink.Models;

[Table("Notifications")]
public class AppNotification
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Title    { get; set; } = string.Empty;
    public string Message  { get; set; } = string.Empty;
    public string Icon     { get; set; } = "🔔";
    public bool   IsRead   { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public NotificationCategory Category { get; set; } = NotificationCategory.System;

    // Formatted relative time: "2 min ago", "Yesterday", "Mar 12"
    public string RelativeTime
    {
        get
        {
            var diff = DateTime.Now - CreatedAt;
            if (diff.TotalMinutes < 1)  return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
            if (diff.TotalHours   < 24) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays    < 2)  return "Yesterday";
            return CreatedAt.ToString("MMM d");
        }
    }

    // Color accent per category
    public string CategoryColor => Category switch
    {
        NotificationCategory.Mood       => "#3B82F6",
        NotificationCategory.Assessment => "#2563EB",
        NotificationCategory.Exercise   => "#60A5FA",
        NotificationCategory.Profile    => "#BFDBFE",
        NotificationCategory.Reminder   => "#7C3AED",
        _                               => "#93C5FD"
    };
}

public enum NotificationCategory
{
    System,
    Mood,
    Assessment,
    Exercise,
    Profile,
    Reminder
}
