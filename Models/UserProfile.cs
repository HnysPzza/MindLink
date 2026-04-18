using SQLite;

namespace M1ndLink.Models;

[Table("UserProfile")]
public class UserProfile
{
    [PrimaryKey]
    public int Id { get; set; } = 1;
    public string Name              { get; set; } = "Friend";
    public string? AvatarPath       { get; set; }
    public string PersonalGoal      { get; set; } = "Feel better every day";
    public bool DailyNotifications  { get; set; } = false;
    public DateTime CreatedAt       { get; set; } = DateTime.Now;
}
