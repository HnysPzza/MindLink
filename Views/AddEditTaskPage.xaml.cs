using M1ndLink.ViewModels;

namespace M1ndLink.Views;

public partial class AddEditTaskPage : ContentPage
{
    public AddEditTaskPage(AddEditTaskViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
