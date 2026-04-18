using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Services;
using M1ndLink.Views;

namespace M1ndLink.ViewModels;

public partial class VerifyOtpViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IProfileService _profileService;
    private readonly IDatabaseService _db;
    private readonly INavigationService _navService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _otpCode = string.Empty;

    public VerifyOtpViewModel(IAuthService authService, IProfileService profileService, IDatabaseService db, INavigationService navService)
    {
        _authService = authService;
        _profileService = profileService;
        _db = db;
        _navService = navService;
        Title = "Verify Email";
    }

    [RelayCommand]
    private async Task VerifyAsync()
    {
        await VerifyCodeAsync();
    }

    /// <summary>
    /// Awaitable version called directly from code-behind to stay on the UI thread.
    /// Returns true on success, false on failure (so the page can show error state).
    /// </summary>
    public async Task<bool> VerifyCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(OtpCode) || OtpCode.Length < 6)
            return false;

        IsBusy = true;

        var result = await _authService.VerifyOtpAsync(Email, OtpCode);

        IsBusy = false;

        if (result.Success)
        {
            // Route dynamically using NavigationService so we check completion
            await _navService.NavigateAfterLoginAsync();
            return true;
        }
        else
        {
            return false;
        }
    }

    [RelayCommand]
    private void GoBackToLogin()
    {
        Application.Current!.MainPage = new LoginPage(new LoginViewModel(_authService, _profileService, _db, _navService));
    }
}
