using SQLite;

namespace M1ndLink.Models;

[Table("sleep_entries")]
public class SleepEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public DateTime Date { get; set; } = DateTime.Today;
    public DateTime BedTime { get; set; }
    public DateTime WakeTime { get; set; }
    public double HoursSlept { get; set; }
    public int QualityScore { get; set; } // 1 to 5 scale
    public string Notes { get; set; } = string.Empty;
}
