using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;
using System.Collections.ObjectModel;

namespace M1ndLink.ViewModels;

public partial class TriggersViewModel : BaseViewModel
{
    private readonly ITriggerService _triggerService;

    [ObservableProperty] private ObservableCollection<TriggerLog> _recentTriggers = new();
    
    [ObservableProperty] private string _triggerName = string.Empty;
    [ObservableProperty] private string _selectedCategory = "General";
    [ObservableProperty] private int _intensity = 5;

    public string[] Categories { get; } = new[] { "General", "Social", "Work", "Physical", "Environment", "Financial" };

    public TriggersViewModel(ITriggerService triggerService)
    {
        _triggerService = triggerService;
        Title = "Trigger Logs";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var data = await _triggerService.GetAllTriggersAsync();
            RecentTriggers = new ObservableCollection<TriggerLog>(data.OrderByDescending(t => t.Date));
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task SaveTriggerAsync()
    {
        if (string.IsNullOrWhiteSpace(TriggerName)) return;

        IsBusy = true;
        try
        {
            var log = new TriggerLog
            {
                Date = DateTime.Now,
                TriggerName = TriggerName,
                Category = SelectedCategory,
                Intensity = Intensity
            };
            await _triggerService.SaveTriggerAsync(log);

            TriggerName = string.Empty;
            Intensity = 5;
        }
        finally
        {
            IsBusy = false;
            await LoadAsync();
        }
    }

    [RelayCommand]
    public async Task GoBackAsync() => await Shell.Current.GoToAsync("..");
}
