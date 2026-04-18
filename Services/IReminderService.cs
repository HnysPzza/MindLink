using M1ndLink.Models;

namespace M1ndLink.Services;

public interface IReminderService
{
    Task<ReminderSettings> GetSettingsAsync();
    Task SaveSettingsAsync(ReminderSettings settings);
    Task EvaluateDueRemindersAsync();
}
