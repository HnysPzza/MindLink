using SQLite;

namespace M1ndLink.Models;

[Table("trigger_logs")]
public class TriggerLog
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public string TriggerName { get; set; } = string.Empty;
    public string Category { get; set; } = "General"; // e.g. Social, Work, Cognitive
    public int Intensity { get; set; } = 1; // 1-10
}
