using M1ndLink.Models;

namespace M1ndLink.Services;

public class SleepService : ISleepService
{
    private readonly IDatabaseService _db;

    public SleepService(IDatabaseService db)
    {
        _db = db;
    }

    public async Task<List<SleepEntry>> GetAllSleepEntriesAsync()
    {
        return await _db.GetAllAsync<SleepEntry>();
    }

    public async Task<SleepEntry> GetSleepEntryForDateAsync(DateTime date)
    {
        var entries = await _db.GetAllAsync<SleepEntry>();
        return entries.FirstOrDefault(e => e.Date.Date == date.Date);
    }

    public async Task SaveSleepEntryAsync(SleepEntry entry)
    {
        // Calculate hours properly and enforce bounds
        if (entry.WakeTime > entry.BedTime)
        {
            entry.HoursSlept = (entry.WakeTime - entry.BedTime).TotalHours;
        }
        else
        {
            // Assuming Bedtime was the previous day
            entry.HoursSlept = (entry.WakeTime.AddDays(1) - entry.BedTime).TotalHours;
        }

        await _db.SaveAsync(entry);
    }

    public async Task DeleteSleepEntryAsync(int id)
    {
        var entry = await _db.GetByIdAsync<SleepEntry>(id);
        if (entry != null)
            await _db.DeleteAsync(entry);
    }
}
