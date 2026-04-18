namespace M1ndLink.Services;

public interface IAuthService
{
    Task<(bool Success, string ErrorMessage)> SignInAsync(string email, string password);
    Task<(bool Success, string ErrorMessage)> SignUpAsync(string email, string password);
    Task<(bool Success, string ErrorMessage)> VerifyOtpAsync(string email, string token);
    Task SignOutAsync();
    bool IsUserLoggedIn();
    string? GetCurrentUserEmail();
    Task<bool> TryRestoreSessionAsync();
}
