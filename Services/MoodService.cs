using M1ndLink.Models;

namespace M1ndLink.Services;

public interface IMoodService
{
    Task<List<MoodEntry>> GetRecentEntriesAsync(int days = 30);
    Task<MoodEntry?> GetTodaysMoodAsync();
    Task SaveMoodAsync(MoodEntry entry);
    Task<int> GetMoodStreakAsync();
    Task<List<AssessmentResult>> GetAssessmentsAsync();
    Task SaveAssessmentAsync(AssessmentResult result);
    Task<AssessmentResult?> GetLatestAssessmentAsync();
}

public class MoodService : IMoodService
{
    private readonly IDatabaseService _db;
    private readonly ISyncService _syncService;

    public MoodService(IDatabaseService db, ISyncService syncService)
    {
        _db = db;
        _syncService = syncService;
    }

    public async Task<List<MoodEntry>> GetRecentEntriesAsync(int days = 30)
    {
        var all = await _db.GetAllAsync<MoodEntry>();
        return all.Where(e => e.Date >= DateTime.Now.AddDays(-days))
                  .OrderBy(e => e.Date).ToList();
    }

    public async Task<MoodEntry?> GetTodaysMoodAsync()
    {
        var all = await _db.GetAllAsync<MoodEntry>();
        return all.FirstOrDefault(e => e.Date.Date == DateTime.Today);
    }

    public async Task SaveMoodAsync(MoodEntry entry) 
    {
        await _db.SaveAsync(entry);
        _ = Task.Run(() => _syncService.PushToCloudAsync());
    }

    public async Task<int> GetMoodStreakAsync()
    {
        var all    = await _db.GetAllAsync<MoodEntry>();
        var dates  = all.Select(e => e.Date.Date).Distinct().OrderByDescending(d => d).ToList();
        int streak = 0;
        var expect = DateTime.Today;
        foreach (var d in dates)
        {
            if (d == expect) { streak++; expect = expect.AddDays(-1); }
            else break;
        }
        return streak;
    }

    public async Task<List<AssessmentResult>> GetAssessmentsAsync() =>
        await _db.GetAllAsync<AssessmentResult>();

    public async Task SaveAssessmentAsync(AssessmentResult r) 
    {
        await _db.SaveAsync(r);
        _ = Task.Run(() => _syncService.PushToCloudAsync());
    }

    public async Task<AssessmentResult?> GetLatestAssessmentAsync()
    {
        var all = await _db.GetAllAsync<AssessmentResult>();
        return all.OrderByDescending(a => a.Date).FirstOrDefault();
    }
}
