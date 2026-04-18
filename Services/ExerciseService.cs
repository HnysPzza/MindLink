using M1ndLink.Models;

namespace M1ndLink.Services;

public interface IExerciseService
{
    List<Exercise> GetExercises();
    Exercise? GetById(int id);
    Task SaveSessionAsync(ExerciseSession session);
    Task<List<ExerciseSession>> GetSessionsAsync();
    Task<int> GetTotalSessionsAsync();

    // ── Favorites ─────────────────────────────────────────────────────────
    Task<List<int>> GetFavoriteIdsAsync();
    Task ToggleFavoriteAsync(int exerciseId);
    Task<bool> IsFavoriteAsync(int exerciseId);

    // ── Custom exercises ──────────────────────────────────────────────────
    Task<List<CustomExercise>> GetCustomExercisesAsync();
    Task SaveCustomExerciseAsync(CustomExercise ex);
    Task DeleteCustomExerciseAsync(CustomExercise ex);

    /// <summary>Returns ALL exercises: built-in + custom, with favorites marked via IsFavorite flag.</summary>
    Task<List<Exercise>> GetAllExercisesAsync();
}

public class ExerciseService : IExerciseService
{
    private readonly IDatabaseService _db;
    private readonly ISyncService _syncService;

    // ── Breathing phase definitions ───────────────────────────────────────
    public static readonly Dictionary<ExerciseType, (int Inhale, int Hold, int Exhale)> BreathingPhases = new()
    {
        { ExerciseType.Breathing,  (4, 7, 8) },
        { ExerciseType.Meditation, (4, 2, 6) },
        { ExerciseType.Grounding,  (4, 0, 4) },
    };

    public ExerciseService(IDatabaseService db, ISyncService syncService)
    {
        _db = db;
        _syncService = syncService;
    }

    public List<Exercise> GetExercises() => new()
    {
        new() { Id=1, Title="4-7-8 Breathing",      Description="Relaxes the nervous system",     IconEmoji="🌬️", Type=ExerciseType.Breathing,  DurationMinutes=5,  GradientStart=Color.FromArgb("#DBEAFE"), GradientEnd=Color.FromArgb("#BFDBFE") },
        new() { Id=2, Title="Mindful Meditation",   Description="Helps restore focus",            IconEmoji="🧘",  Type=ExerciseType.Meditation, DurationMinutes=10, GradientStart=Color.FromArgb("#EFF6FF"), GradientEnd=Color.FromArgb("#DBEAFE") },
        new() { Id=3, Title="5-4-3-2-1 Grounding",  Description="Connect with your surroundings", IconEmoji="🌿", Type=ExerciseType.Grounding,  DurationMinutes=7,  GradientStart=Color.FromArgb("#BFDBFE"), GradientEnd=Color.FromArgb("#93C5FD") },
        new() { Id=4, Title="Deep Relaxation",      Description="Full body tension release",      IconEmoji="🌊", Type=ExerciseType.Meditation, DurationMinutes=15, GradientStart=Color.FromArgb("#E0E7FF"), GradientEnd=Color.FromArgb("#C7D2FE") },
        new() { Id=5, Title="Quick Calm",           Description="A rapid reset for busy moments", IconEmoji="⚡",  Type=ExerciseType.Breathing,  DurationMinutes=2,  GradientStart=Color.FromArgb("#FEF3C7"), GradientEnd=Color.FromArgb("#FDE68A") }
    };

    public Exercise? GetById(int id) => GetExercises().FirstOrDefault(e => e.Id == id);

    // ── Sessions ──────────────────────────────────────────────────────────
    public async Task SaveSessionAsync(ExerciseSession session)
    {
        await _db.SaveAsync(session);
        _ = Task.Run(() => _syncService.PushToCloudAsync());
    }

    public async Task<List<ExerciseSession>> GetSessionsAsync() =>
        await _db.GetAllAsync<ExerciseSession>();

    public async Task<int> GetTotalSessionsAsync() =>
        (await _db.GetAllAsync<ExerciseSession>()).Count;

    // ── Favorites ─────────────────────────────────────────────────────────
    public async Task<List<int>> GetFavoriteIdsAsync()
    {
        var all = await _db.GetAllAsync<FavoriteExercise>();
        return all.Select(f => f.ExerciseId).ToList();
    }

    public async Task<bool> IsFavoriteAsync(int exerciseId)
    {
        var all = await _db.GetAllAsync<FavoriteExercise>();
        return all.Any(f => f.ExerciseId == exerciseId);
    }

    public async Task ToggleFavoriteAsync(int exerciseId)
    {
        var all = await _db.GetAllAsync<FavoriteExercise>();
        var existing = all.FirstOrDefault(f => f.ExerciseId == exerciseId);
        if (existing != null)
            await _db.DeleteAsync(existing);
        else
            await _db.SaveAsync(new FavoriteExercise { ExerciseId = exerciseId });
    }

    // ── Custom exercises ──────────────────────────────────────────────────
    public async Task<List<CustomExercise>> GetCustomExercisesAsync() =>
        await _db.GetAllAsync<CustomExercise>();

    public async Task SaveCustomExerciseAsync(CustomExercise ex) =>
        await _db.SaveAsync(ex);

    public async Task DeleteCustomExerciseAsync(CustomExercise ex) =>
        await _db.DeleteAsync(ex);

    // ── Combined list ─────────────────────────────────────────────────────
    public async Task<List<Exercise>> GetAllExercisesAsync()
    {
        var builtIn = GetExercises();
        var custom  = await GetCustomExercisesAsync();
        var favIds  = await GetFavoriteIdsAsync();

        var combined = new List<Exercise>(builtIn);
        combined.AddRange(custom.Select(c => c.ToExercise()));

        foreach (var ex in combined)
            ex.IsFavorite = favIds.Contains(ex.Id);

        // Sort: favorites first, then by ID
        return combined.OrderByDescending(e => e.IsFavorite).ThenBy(e => e.Id).ToList();
    }
}
