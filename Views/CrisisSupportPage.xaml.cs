using M1ndLink.ViewModels;

namespace M1ndLink.Views;

public partial class CrisisSupportPage : ContentPage
{
    private readonly CrisisSupportViewModel _vm;

    public CrisisSupportPage(CrisisSupportViewModel vm)
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
}
