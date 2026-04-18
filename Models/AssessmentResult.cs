using SQLite;

namespace M1ndLink.Models;

[Table("AssessmentResults")]
public class AssessmentResult
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public int AnxietyScore  { get; set; }   // 1=Never  -> 5=Always
    public int SleepScore    { get; set; }   // 1=Very Poor -> 5=Excellent
    public int EnergyScore   { get; set; }   // 1=Exhausted -> 5=Energized
    public int SocialScore   { get; set; }   // 1=Isolated  -> 5=Connected
    public int ConcentrationScore { get; set; } = 3;
    public int AppetiteScore { get; set; } = 3;
    public int PhysicalSymptomsScore { get; set; } = 3;
    public int HopeScore { get; set; } = 3;
    public int ConfidenceScore { get; set; } = 3;
    public int CopingScore { get; set; } = 3;

    public double RiskScore =>
        (AnxietyScore
        + (6 - SleepScore)
        + (6 - EnergyScore)
        + (6 - SocialScore)
        + (6 - ConcentrationScore)
        + (6 - AppetiteScore)
        + PhysicalSymptomsScore
        + (6 - HopeScore)
        + (6 - ConfidenceScore)
        + (6 - CopingScore)) / 10.0;

    public string Recommendation => RiskLevel switch
    {
        "Low Level" => "You seem fairly steady today. Keep protecting your routine with small check-ins and recovery breaks.",
        "Medium Level" => "Your responses suggest some strain. A breathing session, short reflection, or reaching out to someone supportive could help.",
        _ => "Your answers suggest a heavier day. Slow things down, use crisis support if needed, and prioritize one safe calming step right now."
    };

    public string RiskLevel
    {
        get
        {
            var avg = RiskScore;
            return avg <= 2.0 ? "Low Level" : avg <= 3.5 ? "Medium Level" : "High Level";
        }
    }
}
