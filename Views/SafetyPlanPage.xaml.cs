using M1ndLink.ViewModels;

namespace M1ndLink.Views;

public partial class SafetyPlanPage : ContentPage
{
    public SafetyPlanPage(SafetyPlanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
