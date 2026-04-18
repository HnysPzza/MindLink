namespace M1ndLink.Services;

public interface INavigationService
{
    /// <summary>
    /// Routes the user to the correct page (AppShell or ProfileSetupPage) based on 
    /// whether their profile setup is fully complete in Supabase.
    /// </summary>
    Task NavigateAfterLoginAsync();
}
