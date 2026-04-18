namespace M1ndLink.Services;

public enum AppThemeMode
{
    System,
    Light,
    Dark
}

public interface IThemeService
{
    AppThemeMode GetThemeMode();
    void ApplySavedTheme();
    void SetThemeMode(AppThemeMode mode);
}
