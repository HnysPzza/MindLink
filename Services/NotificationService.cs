using M1ndLink.Models;

namespace M1ndLink.Services;

public class NotificationService : INotificationService
{
    private readonly IDatabaseService _db;

    public NotificationService(IDatabaseService db) => _db = db;

    public async Task AddAsync(AppNotification notification)
    {
        notification.CreatedAt = DateTime.Now;
        notification.IsRead    = false;
        await _db.SaveAsync(notification);
    }

    public async Task<List<AppNotification>> GetAllAsync()
    {
        var all = await _db.GetAllAsync<AppNotification>();
        return all.OrderByDescending(n => n.CreatedAt).ToList();
    }

    public async Task MarkAllReadAsync()
    {
        var all = await _db.GetAllAsync<AppNotification>();
        foreach (var n in all.Where(n => !n.IsRead))
        {
            n.IsRead = true;
            await _db.SaveAsync(n);
        }
    }

    public async Task MarkReadAsync(int id)
    {
        var n = await _db.GetByIdAsync<AppNotification>(id);
        if (n == null) return;
        n.IsRead = true;
        await _db.SaveAsync(n);
    }

    public async Task DeleteAsync(int id)
    {
        var n = await _db.GetByIdAsync<AppNotification>(id);
        if (n != null) await _db.DeleteAsync(n);
    }

    public async Task<int> GetUnreadCountAsync()
    {
        var all = await _db.GetAllAsync<AppNotification>();
        return all.Count(n => !n.IsRead);
    }
}
