using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;
using System.Collections.ObjectModel;

namespace M1ndLink.ViewModels;

public partial class AssessmentHistoryViewModel : BaseViewModel
{
    private readonly IMoodService _moodService;

    [ObservableProperty] private ObservableCollection<AssessmentResult> _items = new();
    [ObservableProperty] private bool _isEmpty = true;

    public AssessmentHistoryViewModel(IMoodService moodService)
    {
        _moodService = moodService;
        Title = "Assessment History";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var assessments = await _moodService.GetAssessmentsAsync();
        Items = new ObservableCollection<AssessmentResult>(assessments.OrderByDescending(item => item.Date));
        IsEmpty = Items.Count == 0;
    }

    [RelayCommand]
    public async Task GoBackAsync() =>
        await Shell.Current.GoToAsync("..");
}
