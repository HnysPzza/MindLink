using SQLite;

namespace M1ndLink.Models;

[Table("habits")]
public class Habit
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "💧";
    public int CurrentStreak { get; set; } = 0;
    public int BestStreak { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

[Table("habit_logs")]
public class HabitLog
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int HabitId { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public bool IsCompleted { get; set; } = false;
}
