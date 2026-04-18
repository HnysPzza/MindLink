using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;
using System.Collections.ObjectModel;

namespace M1ndLink.ViewModels;

public partial class MoodJournalViewModel : BaseViewModel
{
    private readonly IMoodService _mood;

    [ObservableProperty] private ObservableCollection<MoodEntry> _entries = new();
    [ObservableProperty] private ObservableCollection<float> _moodChartData = new();
    [ObservableProperty] private bool _isEmpty = true;

    public MoodJournalViewModel(IMoodService mood)
    {
        _mood = mood;
        Title = "Mood Journal";
    }

    [RelayCommand]
    public async Task LoadEntriesAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            // Load all entries, newest first
            var all = await _mood.GetRecentEntriesAsync(365);
            var sorted = all.OrderByDescending(e => e.Date).ToList();
            Entries = new ObservableCollection<MoodEntry>(sorted);
            IsEmpty = Entries.Count == 0;

            // Take the 30 most recent for the chart, reverse them so they go left-to-right (oldest -> newest on chart)
            var last30 = sorted.Take(30).Reverse().Select(e => (float)e.MoodLevel).ToList();
            if (last30.Count < 2 && last30.Count > 0) 
            {
                // Chart needs at least 2 points to draw a line
                last30.Add(last30[0]);
            }
            MoodChartData = new ObservableCollection<float>(last30);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task GoBackAsync() =>
        await Shell.Current.GoToAsync("..");
}
