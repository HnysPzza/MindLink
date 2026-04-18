using SQLite;

namespace M1ndLink.Models;

[Table("ReminderSettings")]
public class ReminderSettings
{
    [PrimaryKey]
    public int Id { get; set; } = 1;
    public bool DailyNotificationsEnabled { get; set; } = false;
    public bool MorningReminderEnabled { get; set; } = true;
    public int MorningReminderMinutes { get; set; } = 8 * 60;
    public bool EveningReminderEnabled { get; set; } = true;
    public int EveningReminderMinutes { get; set; } = 20 * 60;
    public bool StreakReminderEnabled { get; set; } = true;
    public bool ExerciseSuggestionEnabled { get; set; } = true;
    public DateTime? LastMorningReminderAt { get; set; }
    public DateTime? LastEveningReminderAt { get; set; }
    public DateTime? LastStreakReminderAt { get; set; }
    public DateTime? LastExerciseSuggestionAt { get; set; }
}
