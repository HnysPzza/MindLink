using SQLite;

namespace M1ndLink.Models;

/// <summary>
/// User-created custom exercise stored in SQLite.
/// </summary>
[Table("CustomExercises")]
public class CustomExercise
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Title         { get; set; } = string.Empty;
    public int    DurationMinutes { get; set; } = 5;
    public string TypeName      { get; set; } = "Breathing";   // ExerciseType.ToString()
    public string IconEmoji     { get; set; } = "🧘";

    /// <summary>Convert to the standard Exercise model used by the UI.</summary>
    public Exercise ToExercise() => new()
    {
        Id              = 1000 + Id,          // offset to avoid collisions with built-in IDs
        Title           = Title,
        Description     = "Custom exercise",
        IconEmoji       = IconEmoji,
        Type            = Enum.TryParse<ExerciseType>(TypeName, out var t) ? t : ExerciseType.Breathing,
        DurationMinutes = DurationMinutes,
        GradientStart   = Color.FromArgb("#FEE2E2"),
        GradientEnd     = Color.FromArgb("#FECACA")
    };
}
