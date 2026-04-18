namespace M1ndLink.Views;

public partial class MedicationPage : ContentPage
{
    public MedicationPage(ViewModels.MedicationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Code-behind handler — avoids CollectionView cell virtualization
    // breaking Command bindings to the parent ViewModel in Release/AOT mode.
    private async void OnTakeDoseClicked(object sender, EventArgs e)
    {
        if (BindingContext is ViewModels.MedicationViewModel vm &&
            sender is BindableObject bo &&
            bo.BindingContext is Models.Medication med)
        {
            await vm.MarkTakenAsync(med);
        }
    }
}
