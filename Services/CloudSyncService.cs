using M1ndLink.Models;
using Postgrest.Models;
using System.Diagnostics;

namespace M1ndLink.Services;

public class CloudSyncService : ISyncService
{
    private readonly Supabase.Client _supabase;
    private readonly IDatabaseService _db;

    public CloudSyncService(Supabase.Client supabase, IDatabaseService db)
    {
        _supabase = supabase;
        _db = db;
    }

    public async Task PullFromCloudAsync()
    {
        var userId = _supabase.Auth.CurrentUser?.Id;
        if (string.IsNullOrEmpty(userId)) return;

        try
        {
            // 1. Wipe current local caches for these entities
            await _db.DeleteAllAsync<MoodEntry>();
            await _db.DeleteAllAsync<AssessmentResult>();
            await _db.DeleteAllAsync<ExerciseSession>();
            await _db.DeleteAllAsync<UserProfile>();

            // 2. Fetch from Cloud
            var moodsTask = _supabase.From<SupabaseMoodEntry>().Where(m => m.UserId == userId).Get();
            var assmtTask = _supabase.From<SupabaseAssessmentResult>().Where(a => a.UserId == userId).Get();
            var exersTask = _supabase.From<SupabaseExerciseSession>().Where(e => e.UserId == userId).Get();
            var profileTask = _supabase.From<SupabaseProfile>().Where(p => p.Id == userId).Get();

            await Task.WhenAll(moodsTask, assmtTask, exersTask, profileTask);
            var profileData = profileTask.Result.Models.FirstOrDefault();

            // 3. Map and Insert locally
            if (profileData != null)
            {
                await _db.SaveAsync(new UserProfile
                {
                    Id = 1,
                    Name = string.IsNullOrWhiteSpace(profileData.FullName) ? "Friend" : profileData.FullName,
                    PersonalGoal = string.IsNullOrWhiteSpace(profileData.Bio) ? "Feel better every day" : profileData.Bio,
                    AvatarPath = profileData.AvatarUrl,
                    DailyNotifications = false,
                    CreatedAt = DateTime.Now
                });
            }

            foreach (var m in moodsTask.Result.Models)
            {
                await _db.SaveAsync(new MoodEntry
                {
                    Date = m.Date,
                    MoodLevel = m.MoodLevel,
                    Notes = m.Notes
                });
            }

            foreach (var a in assmtTask.Result.Models)
            {
                await _db.SaveAsync(new AssessmentResult
                {
                    Date = a.Date,
                    AnxietyScore = a.AnxietyScore,
                    SleepScore = a.SleepScore,
                    EnergyScore = a.EnergyScore,
                    SocialScore = a.SocialScore
                });
            }

            foreach (var e in exersTask.Result.Models)
            {
                await _db.SaveAsync(new ExerciseSession
                {
                    ExerciseId = e.ExerciseId,
                    ExerciseTitle = e.ExerciseTitle,
                    ExerciseEmoji = e.ExerciseEmoji,
                    DurationSeconds = e.DurationSeconds,
                    CompletedAt = e.CompletedAt,
                    FinishedFully = e.FinishedFully
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Cloud Sync Pull Error: {ex.Message}");
        }
    }

    public async Task PushToCloudAsync()
    {
        var userId = _supabase.Auth.CurrentUser?.Id;
        if (string.IsNullOrEmpty(userId)) return;

        try
        {
            // Note: For a robust offline-first app, you'd usually track a 'SyncStatus' flag 
            // inside SQLite to only upload modified rows. 
            // For simplicity in this iteration, we grab everything and forcefully Upsert.

            var localMoods = await _db.GetAllAsync<MoodEntry>();
            var localAssmts = await _db.GetAllAsync<AssessmentResult>();
            var localExers = await _db.GetAllAsync<ExerciseSession>();

            var cloudMoods = localMoods.Select(m => new SupabaseMoodEntry
            {
                UserId = userId,
                Date = m.Date,
                MoodLevel = m.MoodLevel,
                Notes = m.Notes
            }).ToList();

            var cloudAssmts = localAssmts.Select(a => new SupabaseAssessmentResult
            {
                UserId = userId,
                Date = a.Date,
                AnxietyScore = a.AnxietyScore,
                SleepScore = a.SleepScore,
                EnergyScore = a.EnergyScore,
                SocialScore = a.SocialScore
            }).ToList();

            var cloudExers = localExers.Select(e => new SupabaseExerciseSession
            {
                UserId = userId,
                ExerciseId = e.ExerciseId,
                ExerciseTitle = e.ExerciseTitle,
                ExerciseEmoji = e.ExerciseEmoji,
                DurationSeconds = e.DurationSeconds,
                CompletedAt = e.CompletedAt,
                FinishedFully = e.FinishedFully
            }).ToList();

            if (cloudMoods.Any()) await _supabase.From<SupabaseMoodEntry>().Upsert(cloudMoods);
            if (cloudAssmts.Any()) await _supabase.From<SupabaseAssessmentResult>().Upsert(cloudAssmts);
            if (cloudExers.Any()) await _supabase.From<SupabaseExerciseSession>().Upsert(cloudExers);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Cloud Sync Push Error: {ex.Message}");
        }
    }
}
