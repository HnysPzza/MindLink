using SQLite;

namespace M1ndLink.Models;

[Table("MoodEntries")]
public class MoodEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    // 1=Very Sad  2=Sad  3=Neutral  4=Happy  5=Very Happy
    public int MoodLevel { get; set; }
    public string? Notes { get; set; }
    public double? Temperature { get; set; }       // °C from Open-Meteo
    public string? WeatherCondition { get; set; }  // e.g. "Rainy"
    public string? WeatherEmoji { get; set; }      // e.g. "🌧️"

    public string MoodEmoji => MoodLevel switch
    {
        1 => "😢", 2 => "😔", 3 => "😐", 4 => "🙂", 5 => "😄", _ => "😐"
    };

    public string MoodLabel => MoodLevel switch
    {
        1 => "Very Sad", 2 => "Sad", 3 => "Neutral", 4 => "Happy", 5 => "Very Happy", _ => "Neutral"
    };
}
