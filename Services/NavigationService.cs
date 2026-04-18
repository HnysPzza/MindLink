using M1ndLink.Views;
using Postgrest.Models;
using Microsoft.Extensions.DependencyInjection;

namespace M1ndLink.Services;

public class NavigationService : INavigationService
{
    private readonly Supabase.Client _supabase;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISyncService _syncService;

    public NavigationService(Supabase.Client supabase, IServiceProvider serviceProvider, ISyncService syncService)
    {
        _supabase = supabase;
        _serviceProvider = serviceProvider;
        _syncService = syncService;
    }

    public async Task NavigateAfterLoginAsync()
    {
        try
        {
            var userId = _supabase.Auth.CurrentUser?.Id;

            if (string.IsNullOrEmpty(userId))
            {
                Application.Current!.MainPage = _serviceProvider.GetRequiredService<LoginPage>();
                return;
            }

            var response = await _supabase
                .From<SupabaseProfile>()
                .Where(p => p.Id == userId)
                .Single();

            if (response == null || !response.IsSetupComplete)
            {
                Application.Current!.MainPage = _serviceProvider.GetRequiredService<ProfileSetupPage>();
            }
            else
            {
                await _syncService.PullFromCloudAsync();

                Application.Current!.MainPage = new AppShell();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NavigateAfterLogin error: {ex.Message}");
            Application.Current!.MainPage = _serviceProvider.GetRequiredService<ProfileSetupPage>();
        }
    }
}
