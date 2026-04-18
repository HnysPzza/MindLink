namespace M1ndLink.Models;

public class HomeActivityItem
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string RelativeTime =>
        DateTime.Now - OccurredAt < TimeSpan.FromHours(1)
            ? $"{Math.Max(1, (int)(DateTime.Now - OccurredAt).TotalMinutes)} min ago"
            : DateTime.Now - OccurredAt < TimeSpan.FromDays(1)
                ? $"{Math.Max(1, (int)(DateTime.Now - OccurredAt).TotalHours)}h ago"
                : OccurredAt.ToString("MMM d");
}
