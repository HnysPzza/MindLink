using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Services;
using M1ndLink.Views;

namespace M1ndLink.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IProfileService _profileService;
    private readonly IDatabaseService _db;
    private readonly INavigationService _navService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    public LoginViewModel(IAuthService authService, IProfileService profileService, IDatabaseService db, INavigationService navService)
    {
        _authService = authService;
        _profileService = profileService;
        _db = db;
        _navService = navService;
        Title = "Login";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", "Please enter both email and password.", "OK");
            return;
        }

        IsBusy = true;

        var result = await _authService.SignInAsync(Email, Password);
        
        IsBusy = false;

        if (result.Success)
        {
            // Navigate dynamically based on profile completion
            await _navService.NavigateAfterLoginAsync();
        }
        else
        {
            bool wantsToVerify = await Application.Current?.MainPage?.DisplayAlert("Login Failed", $"Login failed: {result.ErrorMessage}\n\nDo you have a verification code to enter?", "Yes, verify email", "No, try again") == true;
            if (wantsToVerify)
            {
                var verifyVm = new VerifyOtpViewModel(_authService, _profileService, _db, _navService) { Email = Email };
                Application.Current!.MainPage = new VerifyOtpPage(verifyVm);
            }
        }
    }

    [RelayCommand]
    private async Task GoToSignUpAsync()
    {
        // Pushing sign up onto the stack or changing root depends on how we set up the Auth Shell.
        // For simplicity, if we are on a login page that isn't in a shell, we might just set MainPage. 
        // Or if it IS in a shell, we use Shell.Current.GoToAsync.
        // Assuming we build AuthPages without Shell for bare minimal flow initially:
        Application.Current!.MainPage = new SignUpPage(new SignUpViewModel(_authService, _profileService, _db, _navService));
    }
}
