namespace M1ndLink.Services;

public interface IDatabaseService
{
    Task InitAsync();
    Task<List<T>> GetAllAsync<T>() where T : new();
    Task<T?> GetByIdAsync<T>(int id) where T : new();
    Task<int> SaveAsync<T>(T item);
    Task<int> DeleteAsync<T>(T item);
    Task<int> DeleteAllAsync<T>() where T : new();
    Task ClearAllDataAsync();
}
