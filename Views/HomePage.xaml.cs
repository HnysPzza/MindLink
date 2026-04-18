using M1ndLink.Services;
using M1ndLink.ViewModels;

namespace M1ndLink.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _vm;
    private readonly IServiceProvider _serviceProvider;
    private readonly Supabase.Client _supabase;

    public HomePage(HomeViewModel vm, IServiceProvider serviceProvider, Supabase.Client supabase) 
    { 
        InitializeComponent(); 
        _vm = vm; 
        BindingContext = vm; 
        _serviceProvider = serviceProvider;
        _supabase = supabase;
    }

    protected override async void OnAppearing() 
    { 
        base.OnAppearing(); 

        var userId = _supabase.Auth.CurrentUser?.Id;
        if (userId == null)
        {
            Application.Current!.MainPage = _serviceProvider.GetRequiredService<LoginPage>();
            return;
        }

        var profile = await _supabase.From<SupabaseProfile>().Where(p => p.Id == userId).Single();
        if (profile == null || !profile.IsSetupComplete)
        {
            Application.Current!.MainPage = _serviceProvider.GetRequiredService<ProfileSetupPage>();
            return;
        }

        await _vm.LoadDataCommand.ExecuteAsync(null); 
    }
}
