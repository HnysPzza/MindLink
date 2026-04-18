namespace M1ndLink.Models;

public class Exercise
{
    public int Id { get; set; }
    public string Title         { get; set; } = string.Empty;
    public string Description   { get; set; } = string.Empty;
    public string IconEmoji     { get; set; } = string.Empty;
    public ExerciseType Type    { get; set; }
    public int DurationMinutes  { get; set; }
    public Color GradientStart  { get; set; } = Color.FromArgb("#DBEAFE");
    public Color GradientEnd    { get; set; } = Color.FromArgb("#BFDBFE");
    public bool IsFavorite      { get; set; } = false;
    public string FavoriteIcon  => IsFavorite ? "⭐" : "☆";
}

public enum ExerciseType { Breathing, Meditation, Grounding }
