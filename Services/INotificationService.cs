using M1ndLink.Models;

namespace M1ndLink.Services;

public interface INotificationService
{
    Task AddAsync(AppNotification notification);
    Task<List<AppNotification>> GetAllAsync();
    Task MarkAllReadAsync();
    Task MarkReadAsync(int id);
    Task DeleteAsync(int id);
    Task<int> GetUnreadCountAsync();
}
