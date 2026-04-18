namespace M1ndLink.Models;

public class Meditation
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string IconEmoji { get; set; } = "🧘";
    public int DurationMinutes { get; set; }
    public Color GradientStart { get; set; } = Color.FromArgb("#DBEAFE");
    public Color GradientEnd { get; set; } = Color.FromArgb("#BFDBFE");
    public IReadOnlyList<string> Benefits { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Script { get; set; } = Array.Empty<string>();
    public string DurationLabel => $"{DurationMinutes} min";
    public string BenefitsLabel => string.Join(" • ", Benefits);
}
