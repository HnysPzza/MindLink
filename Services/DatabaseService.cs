using M1ndLink.Models;
using SQLite;

namespace M1ndLink.Services;

public class DatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection? _db;

    private async Task EnsureInitAsync()
    {
        if (_db != null) return;
        await InitAsync();
    }

    public async Task InitAsync()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "mindflow.db3");
        _db = new SQLiteAsyncConnection(path,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        await _db.CreateTableAsync<MoodEntry>();
        await _db.CreateTableAsync<AssessmentResult>();
        await _db.CreateTableAsync<UserProfile>();
        await _db.CreateTableAsync<AppNotification>();
        await _db.CreateTableAsync<ReminderSettings>();
        await _db.CreateTableAsync<EmergencyContact>();
        await _db.CreateTableAsync<TodoTask>();
        await _db.CreateTableAsync<ExerciseSession>();
        await _db.CreateTableAsync<FavoriteExercise>();
        await _db.CreateTableAsync<CustomExercise>();
        await _db.CreateTableAsync<Habit>();
        await _db.CreateTableAsync<HabitLog>();
        await _db.CreateTableAsync<Medication>();
        await _db.CreateTableAsync<MedicationDose>();
        await _db.CreateTableAsync<SleepEntry>();
        await _db.CreateTableAsync<TriggerLog>();
        await TryAddColumnAsync("AssessmentResults", "ConcentrationScore", "INTEGER NOT NULL DEFAULT 3");
        await TryAddColumnAsync("AssessmentResults", "AppetiteScore", "INTEGER NOT NULL DEFAULT 3");
        await TryAddColumnAsync("AssessmentResults", "PhysicalSymptomsScore", "INTEGER NOT NULL DEFAULT 3");
        await TryAddColumnAsync("AssessmentResults", "HopeScore", "INTEGER NOT NULL DEFAULT 3");
        await TryAddColumnAsync("AssessmentResults", "ConfidenceScore", "INTEGER NOT NULL DEFAULT 3");
        await TryAddColumnAsync("AssessmentResults", "CopingScore", "INTEGER NOT NULL DEFAULT 3");
        // Phase 4 – weather columns on MoodEntries
        await TryAddColumnAsync("MoodEntries", "Temperature",      "REAL");
        await TryAddColumnAsync("MoodEntries", "WeatherCondition", "TEXT");
        await TryAddColumnAsync("MoodEntries", "WeatherEmoji",     "TEXT");
    }

    public async Task<List<T>> GetAllAsync<T>() where T : new()
    {
        await EnsureInitAsync();
        return await _db!.Table<T>().ToListAsync();
    }

    public async Task<T?> GetByIdAsync<T>(int id) where T : new()
    {
        await EnsureInitAsync();
        return await _db!.FindAsync<T>(id);
    }

    public async Task<int> SaveAsync<T>(T item)
    {
        await EnsureInitAsync();
        return await _db!.InsertOrReplaceAsync(item);
    }

    public async Task<int> DeleteAsync<T>(T item)
    {
        await EnsureInitAsync();
        return await _db!.DeleteAsync(item);
    }

    public async Task<int> DeleteAllAsync<T>() where T : new()
    {
        await EnsureInitAsync();
        return await _db!.DeleteAllAsync<T>();
    }

    public async Task ClearAllDataAsync()
    {
        await EnsureInitAsync();
        await _db!.DeleteAllAsync<MoodEntry>();
        await _db!.DeleteAllAsync<AssessmentResult>();
        await _db!.DeleteAllAsync<UserProfile>();
        await _db!.DeleteAllAsync<AppNotification>();
        await _db!.DeleteAllAsync<ReminderSettings>();
        await _db!.DeleteAllAsync<EmergencyContact>();
        await _db!.DeleteAllAsync<TodoTask>();
        await _db!.DeleteAllAsync<ExerciseSession>();
        await _db!.DeleteAllAsync<FavoriteExercise>();
        await _db!.DeleteAllAsync<CustomExercise>();
        await _db!.DeleteAllAsync<Habit>();
        await _db!.DeleteAllAsync<HabitLog>();
        await _db!.DeleteAllAsync<Medication>();
        await _db!.DeleteAllAsync<MedicationDose>();
        await _db!.DeleteAllAsync<SleepEntry>();
        await _db!.DeleteAllAsync<TriggerLog>();
        
        Preferences.Remove("user_name");
        Preferences.Remove("avatar_path");
    }

    private async Task TryAddColumnAsync(string tableName, string columnName, string definition)
    {
        try
        {
            await _db!.ExecuteAsync($"ALTER TABLE {tableName} ADD COLUMN {columnName} {definition}");
        }
        catch
        {
        }
    }
}
