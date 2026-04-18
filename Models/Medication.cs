using SQLite;

namespace M1ndLink.Models;

[Table("medications")]
public class Medication
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty; // e.g. "Daily", "Weekly"
    public int CurrentPillCount { get; set; } = 0;
    public int RefillThreshold { get; set; } = 5;
    public TimeSpan ReminderTime { get; set; }
    public bool IsActive { get; set; } = true;
}

[Table("medication_doses")]
public class MedicationDose
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int MedicationId { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public bool IsTaken { get; set; } = false;
}
