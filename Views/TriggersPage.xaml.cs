namespace M1ndLink.Views;

public partial class TriggersPage : ContentPage
{
    public TriggersPage(ViewModels.TriggersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
