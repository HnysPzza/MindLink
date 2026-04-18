using M1ndLink.ViewModels;

namespace M1ndLink.Views;

public partial class TodoListPage : ContentPage
{
    private readonly TodoListViewModel _vm;

    public TodoListPage(TodoListViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
