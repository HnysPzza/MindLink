using M1ndLink.ViewModels;
namespace M1ndLink.Views;
public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _vm;
    public ProfilePage(ProfileViewModel vm) { InitializeComponent(); _vm = vm; BindingContext = vm; }
    protected override async void OnAppearing() { base.OnAppearing(); await _vm.LoadProfileCommand.ExecuteAsync(null); }
}
