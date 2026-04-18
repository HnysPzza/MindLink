using M1ndLink.ViewModels;

namespace M1ndLink.Views;

public partial class MeditationPlayerPage : ContentPage
{
    private readonly MeditationPlayerViewModel _vm;

    public MeditationPlayerPage(MeditationPlayerViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.StopNarrationCommand.Execute(null);
    }
}
