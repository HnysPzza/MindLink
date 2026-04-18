using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;
using System.Collections.ObjectModel;

namespace M1ndLink.ViewModels;

public partial class MeditationLibraryViewModel : BaseViewModel
{
    private readonly IMeditationService _meditations;

    [ObservableProperty] private ObservableCollection<Meditation> _items = new();
    [ObservableProperty] private Meditation? _featuredMeditation;
    [ObservableProperty] private string _featuredReason = "A guided session to help you slow down and reset.";

    public MeditationLibraryViewModel(IMeditationService meditations)
    {
        _meditations = meditations;
        Title = "Meditation Library";
    }

    [RelayCommand]
    public Task LoadAsync()
    {
        var list = _meditations.GetMeditations().ToList();
        FeaturedMeditation = list.FirstOrDefault();
        Items = new ObservableCollection<Meditation>(list.Skip(1));
        return Task.CompletedTask;
    }

    [RelayCommand]
    public async Task OpenMeditationAsync(Meditation? meditation)
    {
        if (meditation == null)
            return;

        await Shell.Current.GoToAsync($"MeditationPlayer?MeditationId={meditation.Id}");
    }

    [RelayCommand]
    public async Task GoBackAsync() => await Shell.Current.GoToAsync("..");
}
