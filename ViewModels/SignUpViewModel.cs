using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Services;
using M1ndLink.Views;

namespace M1ndLink.ViewModels;

public partial class SignUpViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IProfileService _profileService;
    private readonly IDatabaseService _db;
    private readonly INavigationService _navService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _hasEmailError = false;

    [ObservableProperty]
    private bool _hasPasswordError = false;

    [ObservableProperty]
    private bool _hasConfirmPasswordError = false;

    public SignUpViewModel(IAuthService authService, IProfileService profileService, IDatabaseService db, INavigationService navService)
    {
        _authService = authService;
        _profileService = profileService;
        _db = db;
        _navService = navService;
        Title = "Create Account";
    }

    [RelayCommand]
    private async Task SignUpAsync()
    {
        // Reset warnings
        HasEmailError = false;
        HasPasswordError = false;
        HasConfirmPasswordError = false;

        bool isInvalid = false;

        if (string.IsNullOrWhiteSpace(Email))
        {
            HasEmailError = true;
            isInvalid = true;
        }

        if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
        {
            HasPasswordError = true;
            isInvalid = true;
        }

        if (Password != ConfirmPassword)
        {
            HasConfirmPasswordError = true;
            isInvalid = true;
        }

        if (isInvalid) return;

        IsBusy = true;

        var result = await _authService.SignUpAsync(Email, Password);

        IsBusy = false;

        if (result.Success)
        {
            // Wipe any corrupted or stale local data just in case
            await _db.ClearAllDataAsync();

            // If Supabase immediately authenticated them (because email confirmations are turned off), bypass OTP!
            if (_authService.IsUserLoggedIn())
            {
                await Application.Current?.MainPage?.DisplayAlert("Welcome!", "Your account has been created successfully.", "OK");
                await _navService.NavigateAfterLoginAsync();
            }
            else
            {
                await Application.Current?.MainPage?.DisplayAlert("Success", "Account created! Please check your email for the verification code.", "OK");
                
                // Go to OTP Verification
                var verifyVm = new VerifyOtpViewModel(_authService, _profileService, _db, _navService) { Email = Email };
                Application.Current!.MainPage = new VerifyOtpPage(verifyVm);
            }
        }
        else
        {
            await Application.Current?.MainPage?.DisplayAlert("Sign Up Failed", $"Could not create account: {result.ErrorMessage}", "OK");
        }
    }

    [RelayCommand]
    private void GoBackToLogin()
    {
        Application.Current!.MainPage = new LoginPage(new LoginViewModel(_authService, _profileService, _db, _navService));
    }
}
