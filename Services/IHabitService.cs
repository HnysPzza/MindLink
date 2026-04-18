using M1ndLink.Models;

namespace M1ndLink.Services;

public interface IHabitService
{
    Task<List<Habit>> GetAllHabitsAsync();
    Task SaveHabitAsync(Habit habit);
    Task DeleteHabitAsync(int id);
    Task<bool> IsHabitCompletedTodayAsync(int habitId);
    Task ToggleHabitCompletionAsync(int habitId);
}
