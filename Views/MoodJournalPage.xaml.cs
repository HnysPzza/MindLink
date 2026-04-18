using M1ndLink.ViewModels;

namespace M1ndLink.Views;

public partial class MoodJournalPage : ContentPage
{
    private readonly MoodJournalViewModel _vm;

    public MoodJournalPage(MoodJournalViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadEntriesCommand.ExecuteAsync(null);
    }
}
