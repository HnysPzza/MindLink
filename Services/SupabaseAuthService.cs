using Supabase.Gotrue;

namespace M1ndLink.Services;

public class SupabaseAuthService : IAuthService
{
    private readonly Supabase.Client _supabase;
    private const string SessionKey = "supabase_session";

    public SupabaseAuthService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<(bool Success, string ErrorMessage)> SignInAsync(string email, string password)
    {
        try
        {
            var session = await _supabase.Auth.SignIn(email, password);
            if (session?.User != null)
            {
                await SaveSessionAsync(session);
                return (true, string.Empty);
            }
            return (false, "Login failed");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string ErrorMessage)> SignUpAsync(string email, string password)
    {
        try
        {
            var session = await _supabase.Auth.SignUp(email, password);
            bool success = session?.User != null || _supabase.Auth.CurrentUser != null;
            return (success, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string ErrorMessage)> VerifyOtpAsync(string email, string token)
    {
        try
        {
            var session = await _supabase.Auth.VerifyOTP(email, token, Supabase.Gotrue.Constants.EmailOtpType.Signup);
            if (session?.User != null)
            {
                await SaveSessionAsync(session);
                return (true, string.Empty);
            }
            return (false, "Verification failed");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task SignOutAsync()
    {
        await _supabase.Auth.SignOut();
        await ClearSessionAsync();
    }

    public bool IsUserLoggedIn()
    {
        return _supabase.Auth.CurrentUser != null;
    }

    public string? GetCurrentUserEmail()
    {
        return _supabase.Auth.CurrentUser?.Email;
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        try
        {
            var sessionJson = await SecureStorage.GetAsync(SessionKey);
            if (string.IsNullOrEmpty(sessionJson))
                return false;

            var session = System.Text.Json.JsonSerializer.Deserialize<Session>(sessionJson);
            if (session == null)
                return false;

            await _supabase.Auth.SetSession(session.AccessToken, session.RefreshToken);
            return _supabase.Auth.CurrentUser != null;
        }
        catch
        {
            return false;
        }
    }

    private async Task SaveSessionAsync(Session session)
    {
        try
        {
            var sessionJson = System.Text.Json.JsonSerializer.Serialize(session);
            await SecureStorage.SetAsync(SessionKey, sessionJson);
        }
        catch
        {
            // Silently fail if secure storage is unavailable
        }
    }

    private async Task ClearSessionAsync()
    {
        try
        {
            SecureStorage.Remove(SessionKey);
            await Task.CompletedTask;
        }
        catch
        {
            // Silently fail
        }
    }
}
