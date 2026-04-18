using M1ndLink.ViewModels;

namespace M1ndLink.Views;

public partial class EditProfilePage : ContentPage
{
    private readonly EditProfileViewModel _vm;

    public EditProfilePage(EditProfileViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }

    private async void OnBackTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");
}
