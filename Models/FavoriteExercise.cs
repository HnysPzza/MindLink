using SQLite;

namespace M1ndLink.Models;

/// <summary>
/// Persists which exercise IDs a user has marked as favorites.
/// </summary>
[Table("FavoriteExercises")]
public class FavoriteExercise
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int ExerciseId { get; set; }
}
