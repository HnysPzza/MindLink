using CommunityToolkit.Mvvm.ComponentModel;

namespace M1ndLink.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty] private string _title = string.Empty;
    public bool IsNotBusy => !IsBusy;
}
