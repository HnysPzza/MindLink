namespace M1ndLink.Views;

public partial class AdvancedReportingPage : ContentPage
{
    public AdvancedReportingPage(ViewModels.AdvancedReportingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
