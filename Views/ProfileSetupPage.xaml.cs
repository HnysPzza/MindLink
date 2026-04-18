using M1ndLink.ViewModels;

namespace M1ndLink.Views;

public partial class ProfileSetupPage : ContentPage
{
    private readonly ProfileSetupViewModel _viewModel;

    public ProfileSetupPage(ProfileSetupViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
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
        Application.Current!.MainPage = new AppShell();
    }
}
