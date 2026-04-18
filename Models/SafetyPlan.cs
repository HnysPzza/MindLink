using SQLite;

namespace M1ndLink.Models;

[Table("safety_plans")]
public class SafetyPlan
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public string WarningSigns { get; set; } = string.Empty;
    public string CopingStrategies { get; set; } = string.Empty;
    public string PeopleToContact { get; set; } = string.Empty;
    public string ProfessionalsToContact { get; set; } = string.Empty;
    public string SafeEnvironments { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
