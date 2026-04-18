using M1ndLink.ViewModels;

namespace M1ndLink.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private bool _isPasswordVisible = false;

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;
        EyeIcon.Text = _isPasswordVisible ? "Hide" : "Show";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        LogoImage.Opacity = 0;
        AppNameLabel.Opacity = 0;
        LoginButton.Opacity = 0;

        await Task.Delay(100);
        await LogoImage.FadeToAsync(1, 400, Easing.CubicOut);
        await AppNameLabel.FadeToAsync(1, 300, Easing.CubicOut);
        await LoginButton.FadeToAsync(1, 280, Easing.CubicOut);
    }
}
