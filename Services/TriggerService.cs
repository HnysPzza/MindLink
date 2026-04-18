using M1ndLink.Models;

namespace M1ndLink.Services;

public class TriggerService : ITriggerService
{
    private readonly IDatabaseService _db;

    public TriggerService(IDatabaseService db)
    {
        _db = db;
    }

    public async Task<List<TriggerLog>> GetAllTriggersAsync()
    {
        return await _db.GetAllAsync<TriggerLog>();
    }

    public async Task SaveTriggerAsync(TriggerLog trigger)
    {
        await _db.SaveAsync(trigger);
    }

    public async Task DeleteTriggerAsync(int id)
    {
        var trigger = await _db.GetByIdAsync<TriggerLog>(id);
        if (trigger != null)
            await _db.DeleteAsync(trigger);
    }
}
