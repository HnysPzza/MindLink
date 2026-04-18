namespace M1ndLink.Views;

public partial class HabitsPage : ContentPage
{
    public HabitsPage(ViewModels.HabitsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Code-behind handler — avoids CollectionView cell virtualization
    // breaking x:Reference / RelativeSource bindings in Release/AOT mode.
    private async void OnHabitTapped(object sender, TappedEventArgs e)
    {
        if (BindingContext is ViewModels.HabitsViewModel vm &&
            sender is BindableObject bo &&
            bo.BindingContext is ViewModels.HabitItemViewModel item)
        {
            await vm.ToggleHabitAsync(item);
        }
    }
}
