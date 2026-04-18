using M1ndLink.ViewModels;

namespace M1ndLink.Views;

public partial class MeditationLibraryPage : ContentPage
{
    private readonly MeditationLibraryViewModel _vm;

    public MeditationLibraryPage(MeditationLibraryViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
