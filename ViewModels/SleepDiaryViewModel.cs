using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;
using System.Collections.ObjectModel;

namespace M1ndLink.ViewModels;

public partial class SleepDiaryViewModel : BaseViewModel
{
    private readonly ISleepService _sleepService;

    [ObservableProperty] private ObservableCollection<SleepEntry> _sleepHistory = new();
    
    [ObservableProperty] private TimeSpan _bedTime = new TimeSpan(22, 30, 0);
    [ObservableProperty] private TimeSpan _wakeTime = new TimeSpan(7, 0, 0);
    [ObservableProperty] private int _qualityScore = 3;
    [ObservableProperty] private string _notes = string.Empty;

    public SleepDiaryViewModel(ISleepService sleepService)
    {
        _sleepService = sleepService;
        Title = "Sleep Diary";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var entries = await _sleepService.GetAllSleepEntriesAsync();
            SleepHistory = new ObservableCollection<SleepEntry>(entries.OrderByDescending(e => e.Date));
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task SaveSleepAsync()
    {
        IsBusy = true;
        try
        {
            var entry = new SleepEntry
            {
                Date = DateTime.Today,
                BedTime = DateTime.Today.Add(BedTime),
                WakeTime = DateTime.Today.Add(WakeTime),
                QualityScore = QualityScore,
                Notes = Notes
            };

            await _sleepService.SaveSleepEntryAsync(entry);
            
            // reset
            Notes = string.Empty;
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
