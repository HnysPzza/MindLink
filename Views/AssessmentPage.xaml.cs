using M1ndLink.ViewModels;
namespace M1ndLink.Views;
public partial class AssessmentPage : ContentPage
{
    public AssessmentPage(AssessmentViewModel vm) { InitializeComponent(); BindingContext = vm; }
}
