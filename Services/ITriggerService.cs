using M1ndLink.Models;

namespace M1ndLink.Services;

public interface ITriggerService
{
    Task<List<TriggerLog>> GetAllTriggersAsync();
    Task SaveTriggerAsync(TriggerLog trigger);
    Task DeleteTriggerAsync(int id);
}
