using M1ndLink.ViewModels;

namespace M1ndLink.Views;

public partial class GeneralSettingsPage : ContentPage
{
    private readonly GeneralSettingsViewModel _viewModel;

    public GeneralSettingsPage(GeneralSettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}
