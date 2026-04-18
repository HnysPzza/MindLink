using M1ndLink.ViewModels;
namespace M1ndLink.Views;
public partial class ExercisesPage : ContentPage
{
    public ExercisesPage(ExercisesViewModel vm) 
    { 
        InitializeComponent(); 
        BindingContext = vm; 
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ExercisesViewModel vm)
        {
            vm.LoadRecommendationsCommand.Execute(null);
        }
    }
}
