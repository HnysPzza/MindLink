using M1ndLink.Models;

namespace M1ndLink.Services;

public interface ITodoService
{
    Task<List<TodoTask>> GetAllAsync();
    Task<TodoTask?> GetByIdAsync(int id);
    Task SaveAsync(TodoTask task);
    Task DeleteAsync(TodoTask task);
    Task ToggleCompleteAsync(TodoTask task, bool isCompleted);
    Task<int> GetPendingCountAsync();
}
