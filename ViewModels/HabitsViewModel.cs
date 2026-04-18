using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;
using System.Collections.ObjectModel;

namespace M1ndLink.ViewModels;

public partial class HabitItemViewModel : ObservableObject
{
    [ObservableProperty] private Habit _habit;
    [ObservableProperty] private bool _isCompletedToday;

    public HabitItemViewModel(Habit habit, bool isCompletedToday)
    {
        _habit = habit;
        _isCompletedToday = isCompletedToday;
    }
}

public partial class HabitsViewModel : BaseViewModel
{
    private readonly IHabitService _habitService;

    [ObservableProperty] private ObservableCollection<HabitItemViewModel> _habits = new();

    [ObservableProperty] private string _newHabitName = string.Empty;
    [ObservableProperty] private string _newHabitIcon = "💧";
    [ObservableProperty] private bool _isAddHabitVisible = false;

    public HabitsViewModel(IHabitService habitService)
    {
        _habitService = habitService;
        Title = "My Habits";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var rawHabits = await _habitService.GetAllHabitsAsync();
            var list = new List<HabitItemViewModel>();
            foreach (var h in rawHabits)
            {
                bool comp = await _habitService.IsHabitCompletedTodayAsync(h.Id);
                list.Add(new HabitItemViewModel(h, comp));
            }
            Habits = new ObservableCollection<HabitItemViewModel>(
                list.OrderByDescending(h => h.IsCompletedToday ? 0 : 1));
        }
        catch (Exception)
        {
            // Silently fail — DB may not be ready yet on first run
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public void ShowAddHabit() => IsAddHabitVisible = true;

    [RelayCommand]
    public void CancelAddHabit()
    {
        IsAddHabitVisible = false;
        NewHabitName = "";
        NewHabitIcon = "💧";
    }

    [RelayCommand]
    public async Task SaveHabitAsync()
    {
        if (string.IsNullOrWhiteSpace(NewHabitName)) return;

        IsBusy = true;
        try
        {
            var h = new Habit { Name = NewHabitName, Icon = NewHabitIcon };
            await _habitService.SaveHabitAsync(h);
            CancelAddHabit();
        }
        catch (System.Security.SecurityException)
        {
            await Shell.Current.DisplayAlert("Permission Required", 
                "To get exact reminders for this habit, please allow 'Alarms & Reminders' for M1ndLink in your Android Settings.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to save habit: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            await LoadAsync();  // runs after IsBusy=false so the guard doesn't skip it
        }
    }

    // Called from code-behind — no XAML binding to parent VM needed
    public async Task ToggleHabitAsync(HabitItemViewModel item)
    {
        if (item == null) return;
        try { await _habitService.ToggleHabitCompletionAsync(item.Habit.Id); }
        catch (Exception) { }
        await LoadAsync();
    }

    public async Task DeleteHabitAsync(HabitItemViewModel item)
    {
        if (item == null) return;
        try { await _habitService.DeleteHabitAsync(item.Habit.Id); }
        catch (Exception) { }
        await LoadAsync();
    }

    [RelayCommand]
    public async Task GoBackAsync() => await Shell.Current.GoToAsync("..");
}
