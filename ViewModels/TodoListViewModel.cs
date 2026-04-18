using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;
using System.Collections.ObjectModel;

namespace M1ndLink.ViewModels;

public partial class TodoListViewModel : BaseViewModel
{
    private readonly ITodoService _todoService;

    [ObservableProperty] private ObservableCollection<TodoTask> _tasks = new();
    [ObservableProperty] private ObservableCollection<TodoTask> _filteredTasks = new();
    [ObservableProperty] private string _selectedFilter = "Today";
    [ObservableProperty] private bool _hasTasks;
    [ObservableProperty] private int _pendingCount;
    [ObservableProperty] private int _completedCount;

    public IReadOnlyList<string> Filters { get; } = new[] { "Today", "Upcoming", "Completed" };
    public string FilterSummary => SelectedFilter switch
    {
        "Completed" => "Review what you have already finished and keep the momentum going.",
        "Upcoming" => "Look ahead at the next responsibilities on your plate.",
        _ => "Focus on the most important tasks that need your attention today."
    };

    public TodoListViewModel(ITodoService todoService)
    {
        _todoService = todoService;
        Title = "My Tasks";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            var tasks = await _todoService.GetAllAsync();
            Tasks = new ObservableCollection<TodoTask>(tasks);
            PendingCount = tasks.Count(task => !task.IsCompleted);
            CompletedCount = tasks.Count(task => task.IsCompleted);
            ApplyFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task OpenAddTaskAsync() =>
        await Shell.Current.GoToAsync("AddEditTask");

    [RelayCommand]
    public async Task CancelAsync() =>
        await Shell.Current.GoToAsync("..");

    [RelayCommand]
    public async Task OpenTaskAsync(TodoTask? task)
    {
        if (task == null)
            return;

        await Shell.Current.GoToAsync($"AddEditTask?TaskId={task.Id}");
    }

    [RelayCommand]
    public async Task ToggleTaskAsync(TodoTask? task)
    {
        if (task == null)
            return;

        await _todoService.ToggleCompleteAsync(task, !task.IsCompleted);
        await LoadAsync();
    }

    [RelayCommand]
    public async Task DeleteTaskAsync(TodoTask? task)
    {
        if (task == null)
            return;

        await _todoService.DeleteAsync(task);
        await LoadAsync();
    }

    [RelayCommand]
    public void SetFilter(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return;

        SelectedFilter = filter;
        ApplyFilter();
    }

    partial void OnSelectedFilterChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        IEnumerable<TodoTask> filtered = Tasks;
        var today = DateTime.Today;

        filtered = SelectedFilter switch
        {
            "Completed" => filtered.Where(task => task.IsCompleted),
            "Upcoming" => filtered.Where(task => !task.IsCompleted && task.DueAt.Date > today),
            _ => filtered.Where(task => !task.IsCompleted && task.DueAt.Date <= today)
        };

        FilteredTasks = new ObservableCollection<TodoTask>(filtered.OrderBy(task => task.DueAt));
        HasTasks = FilteredTasks.Count > 0;
        OnPropertyChanged(nameof(FilterSummary));
    }
}
