using System.Text.Json;
using M1ndLink.Models;

namespace M1ndLink.Services;

public class DataExportService : IDataExportService
{
    private readonly IDatabaseService _databaseService;

    public DataExportService(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<string> ExportAsync()
    {
        var exportPayload = new
        {
            ExportedAt = DateTime.Now,
            MoodEntries = await _databaseService.GetAllAsync<MoodEntry>(),
            AssessmentResults = await _databaseService.GetAllAsync<AssessmentResult>(),
            ExerciseSessions = await _databaseService.GetAllAsync<ExerciseSession>(),
            Notifications = await _databaseService.GetAllAsync<AppNotification>(),
            EmergencyContacts = await _databaseService.GetAllAsync<EmergencyContact>(),
            TodoTasks = await _databaseService.GetAllAsync<TodoTask>(),
            ReminderSettings = await _databaseService.GetAllAsync<ReminderSettings>(),
            UserProfiles = await _databaseService.GetAllAsync<UserProfile>()
        };

        var exportDirectory = Path.Combine(FileSystem.AppDataDirectory, "exports");
        Directory.CreateDirectory(exportDirectory);

        var fileName = $"mindlink-export-{DateTime.Now:yyyyMMdd-HHmmss}.json";
        var filePath = Path.Combine(exportDirectory, fileName);

        var json = JsonSerializer.Serialize(exportPayload, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(filePath, json);
        return filePath;
    }
}
