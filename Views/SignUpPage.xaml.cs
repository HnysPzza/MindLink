using M1ndLink.ViewModels;

namespace M1ndLink.Views;

public partial class SignUpPage : ContentPage
{
    public SignUpPage(SignUpViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private bool _isPasswordVisible = false;
    private bool _isConfirmPasswordVisible = false;

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;
        EyeIconPwd.Text = _isPasswordVisible ? "Hide" : "Show";
    }

    private void OnToggleConfirmPasswordClicked(object sender, EventArgs e)
    {
        _isConfirmPasswordVisible = !_isConfirmPasswordVisible;
        ConfirmPasswordEntry.IsPassword = !_isConfirmPasswordVisible;
        EyeIconConfirm.Text = _isConfirmPasswordVisible ? "Hide" : "Show";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        AppNameLabel.Opacity = 0;
        HeaderPanel.Opacity = 0;
        AuthCard.Opacity = 0;
        FooterPanel.Opacity = 0;
        RegisterButton.Opacity = 0;

        await Task.Delay(100);
        await AppNameLabel.FadeToAsync(1, 320, Easing.CubicOut);
        await HeaderPanel.FadeToAsync(1, 360, Easing.CubicOut);
        await AuthCard.FadeToAsync(1, 400, Easing.CubicOut);
        await RegisterButton.FadeToAsync(1, 300, Easing.CubicOut);
        await FooterPanel.FadeToAsync(1, 250, Easing.CubicOut);
    }
}
