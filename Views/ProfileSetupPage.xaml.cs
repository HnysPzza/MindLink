using M1ndLink.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace M1ndLink.Views;

public partial class ProfileSetupPage : ContentPage
{
    private readonly ProfileSetupViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;

    public ProfileSetupPage(ProfileSetupViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Stagger fade-in
        TitleLabel.Opacity = 0;
        SubtitleLabel.Opacity = 0;
        FormCard.Opacity = 0;
        SaveButton.Opacity = 0;

        await Task.Delay(80);
        await TitleLabel.FadeTo(1, 300, Easing.CubicOut);
        await SubtitleLabel.FadeTo(1, 250, Easing.CubicOut);
        await FormCard.FadeTo(1, 400, Easing.CubicOut);
        await SaveButton.FadeTo(1, 300, Easing.CubicOut);

        // Auto-focus name field
        await Task.Delay(150);
        NameEntry.Focus();
    }

    private void OnSkipTapped(object sender, EventArgs e)
    {
        // Navigate directly to app without saving profile
        Application.Current!.MainPage = _serviceProvider.GetRequiredService<AppShell>();
    }
}
