using M1ndLink.Models;

namespace M1ndLink.Services;

public interface ISleepService
{
    Task<List<SleepEntry>> GetAllSleepEntriesAsync();
    Task<SleepEntry> GetSleepEntryForDateAsync(DateTime date);
    Task SaveSleepEntryAsync(SleepEntry entry);
    Task DeleteSleepEntryAsync(int id);
}
