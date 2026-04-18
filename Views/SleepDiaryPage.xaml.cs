namespace M1ndLink.Views;

public partial class SleepDiaryPage : ContentPage
{
    public SleepDiaryPage(ViewModels.SleepDiaryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
