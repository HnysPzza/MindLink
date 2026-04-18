using Postgrest.Attributes;
using Postgrest.Models;

namespace M1ndLink.Models;

[Table("mood_entries")]
public class SupabaseMoodEntry : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("mood_level")]
    public int MoodLevel { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }
}

[Table("assessment_results")]
public class SupabaseAssessmentResult : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("anxiety_score")]
    public int AnxietyScore { get; set; }

    [Column("sleep_score")]
    public int SleepScore { get; set; }

    [Column("energy_score")]
    public int EnergyScore { get; set; }

    [Column("social_score")]
    public int SocialScore { get; set; }
}

[Table("exercise_sessions")]
public class SupabaseExerciseSession : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("exercise_id")]
    public int ExerciseId { get; set; }

    [Column("exercise_title")]
    public string ExerciseTitle { get; set; } = string.Empty;

    [Column("exercise_emoji")]
    public string ExerciseEmoji { get; set; } = string.Empty;

    [Column("duration_seconds")]
    public int DurationSeconds { get; set; }

    [Column("completed_at")]
    public DateTime CompletedAt { get; set; }

    [Column("finished_fully")]
    public bool FinishedFully { get; set; }
}

[Table("daily_tips")]
public class SupabaseDailyTip : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("category")]
    public string Category { get; set; } = string.Empty;

    [Column("icon")]
    public string? Icon { get; set; }
}
