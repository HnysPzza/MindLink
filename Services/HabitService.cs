using M1ndLink.Models;

namespace M1ndLink.Services;

public class HabitService : IHabitService
{
    private readonly IDatabaseService _db;

    public HabitService(IDatabaseService db)
    {
        _db = db;
    }

    public async Task<List<Habit>> GetAllHabitsAsync()
    {
        return await _db.GetAllAsync<Habit>();
    }

    public async Task SaveHabitAsync(Habit habit)
    {
        await _db.SaveAsync(habit);
    }

    public async Task DeleteHabitAsync(int id)
    {
        var habit = await _db.GetByIdAsync<Habit>(id);
        if (habit != null)
            await _db.DeleteAsync(habit);
    }

    public async Task<bool> IsHabitCompletedTodayAsync(int habitId)
    {
        var logs = await _db.GetAllAsync<HabitLog>();
        return logs.Any(l => l.HabitId == habitId && l.Date.Date == DateTime.Today && l.IsCompleted);
    }

    public async Task ToggleHabitCompletionAsync(int habitId)
    {
        var logs = await _db.GetAllAsync<HabitLog>();
        var log = logs.FirstOrDefault(l => l.HabitId == habitId && l.Date.Date == DateTime.Today);

        if (log == null)
        {
            log = new HabitLog { HabitId = habitId, Date = DateTime.Today, IsCompleted = true };
            await _db.SaveAsync(log);
            await UpdateStreakAsync(habitId, true);
        }
        else
        {
            log.IsCompleted = !log.IsCompleted;
            await _db.SaveAsync(log);
            await UpdateStreakAsync(habitId, log.IsCompleted);
        }
    }

    private async Task UpdateStreakAsync(int habitId, bool added)
    {
        var habit = await _db.GetByIdAsync<Habit>(habitId);
        if (habit != null)
        {
            if (added)
            {
                habit.CurrentStreak += 1;
                if (habit.CurrentStreak > habit.BestStreak)
                    habit.BestStreak = habit.CurrentStreak;
            }
            else
            {
                habit.CurrentStreak -= 1;
                if (habit.CurrentStreak < 0) habit.CurrentStreak = 0;
            }
            await _db.SaveAsync(habit);
        }
    }
}
