using M1ndLink.ViewModels;

namespace M1ndLink.Views;

public partial class AssessmentHistoryPage : ContentPage
{
    private readonly AssessmentHistoryViewModel _vm;

    public AssessmentHistoryPage(AssessmentHistoryViewModel vm)
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
