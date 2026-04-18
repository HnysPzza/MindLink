using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;
using System.Collections.ObjectModel;

namespace M1ndLink.ViewModels;

public partial class AddEditTaskViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ITodoService _todoService;
    private int _editingTaskId;

    [ObservableProperty] private string _taskTitle = string.Empty;
    [ObservableProperty] private string _taskDescription = string.Empty;
    [ObservableProperty] private TaskCategory _selectedCategory = TaskCategory.Personal;
    [ObservableProperty] private TaskPriority _selectedPriority = TaskPriority.Medium;
    [ObservableProperty] private DateTime _dueDate = DateTime.Today;
    [ObservableProperty] private TimeSpan _dueTime = new(18, 0, 0);
    [ObservableProperty] private int _selectedReminderOffset = 30;
    [ObservableProperty] private bool _isEditing;

    public ObservableCollection<TaskCategory> Categories { get; } = new(Enum.GetValues<TaskCategory>());
    public ObservableCollection<TaskPriority> Priorities { get; } = new(Enum.GetValues<TaskPriority>());
    public ObservableCollection<int> ReminderOffsets { get; } = new() { 15, 30, 60, 180, 1440 };
    public string PageHeading => IsEditing ? "Update Task" : "Create Task";

    public AddEditTaskViewModel(ITodoService todoService)
    {
        _todoService = todoService;
        Title = "Task";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        ResetState();

        if (!query.TryGetValue("TaskId", out var taskIdValue) || !int.TryParse(taskIdValue?.ToString(), out var taskId))
            return;

        Task.Run(async () =>
        {
            try
            {
                var task = await _todoService.GetByIdAsync(taskId);
                if (task == null)
                    return;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _editingTaskId = task.Id;
                    IsEditing = true;
                    TaskTitle = task.Title;
                    TaskDescription = task.Description;
                    SelectedCategory = task.Category;
                    SelectedPriority = task.Priority;
                    DueDate = task.DueAt.Date;
                    DueTime = task.DueAt.TimeOfDay;
                    SelectedReminderOffset = task.ReminderOffsetMinutes;
                    OnPropertyChanged(nameof(PageHeading));
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                    await Shell.Current.DisplayAlert("Error", $"Failed to load task: {ex.Message}", "OK"));
            }
        });
    }

    [RelayCommand]
    public async Task SaveTaskAsync()
    {
        if (string.IsNullOrWhiteSpace(TaskTitle))
        {
            await Shell.Current.DisplayAlert("Missing Title", "Give your task a short title.", "OK");
            return;
        }

        var existing = _editingTaskId == 0
            ? new TodoTask()
            : await _todoService.GetByIdAsync(_editingTaskId) ?? new TodoTask();

        existing.Title = TaskTitle.Trim();
        existing.Description = TaskDescription.Trim();
        existing.Category = SelectedCategory;
        existing.Priority = SelectedPriority;
        existing.DueAt = DueDate.Date.Add(DueTime);
        existing.ReminderOffsetMinutes = SelectedReminderOffset;

        await _todoService.SaveAsync(existing);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    public async Task DeleteTaskAsync()
    {
        if (_editingTaskId == 0)
            return;

        var existing = await _todoService.GetByIdAsync(_editingTaskId);
        if (existing == null)
            return;

        await _todoService.DeleteAsync(existing);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    public async Task CancelAsync() =>
        await Shell.Current.GoToAsync("..");

    private void ResetState()
    {
        _editingTaskId = 0;
        IsEditing = false;
        TaskTitle = string.Empty;
        TaskDescription = string.Empty;
        SelectedCategory = TaskCategory.Personal;
        SelectedPriority = TaskPriority.Medium;
        DueDate = DateTime.Today;
        DueTime = new TimeSpan(18, 0, 0);
        SelectedReminderOffset = 30;
        OnPropertyChanged(nameof(PageHeading));
    }
}
