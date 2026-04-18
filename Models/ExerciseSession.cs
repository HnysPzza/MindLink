using SQLite;

namespace M1ndLink.Models;

/// <summary>
/// Persisted record of a completed exercise session.
/// </summary>
[Table("ExerciseSessions")]
public class ExerciseSession
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int      ExerciseId      { get; set; }
    public string   ExerciseTitle   { get; set; } = string.Empty;
    public string   ExerciseEmoji   { get; set; } = string.Empty;
    public int      DurationSeconds { get; set; }   // actual seconds spent
    public DateTime CompletedAt     { get; set; } = DateTime.Now;
    public bool     FinishedFully   { get; set; }   // false = ended early
}
