namespace M1ndLink.Services;

public class ThemeService : IThemeService
{
    private const string ThemePreferenceKey = "theme_mode";

    public AppThemeMode GetThemeMode()
    {
        var value = Preferences.Get(ThemePreferenceKey, AppThemeMode.System.ToString());
        return Enum.TryParse<AppThemeMode>(value, true, out var mode)
            ? mode
            : AppThemeMode.System;
    }

    public void ApplySavedTheme()
    {
        ApplyTheme(GetThemeMode());
    }

    public void SetThemeMode(AppThemeMode mode)
    {
        Preferences.Set(ThemePreferenceKey, mode.ToString());
        ApplyTheme(mode);
    }

    private static void ApplyTheme(AppThemeMode mode)
    {
        if (Application.Current == null)
            return;

        Application.Current.UserAppTheme = mode switch
        {
            AppThemeMode.Light => AppTheme.Light,
            AppThemeMode.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }
}
