using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;
using System.Collections.ObjectModel;

namespace M1ndLink.ViewModels;

public partial class NotificationsViewModel : BaseViewModel
{
    private readonly INotificationService _notifications;

    [ObservableProperty] private ObservableCollection<AppNotification> _items = new();
    [ObservableProperty] private bool _isEmpty = false;

    public NotificationsViewModel(INotificationService notifications)
    {
        _notifications = notifications;
        Title = "Notifications";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var list = await _notifications.GetAllAsync();
            Items   = new ObservableCollection<AppNotification>(list);
            IsEmpty = list.Count == 0;

            // Auto-mark all as read when page opens
            await _notifications.MarkAllReadAsync();
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task DeleteNotificationAsync(AppNotification notif)
    {
        await _notifications.DeleteAsync(notif.Id);
        Items.Remove(notif);
        IsEmpty = Items.Count == 0;
    }

    [RelayCommand]
    public async Task ClearAllAsync()
    {
        var confirm = await Shell.Current.DisplayAlert(
            "Clear All", "Remove all notifications?", "Clear", "Cancel");
        if (!confirm) return;

        foreach (var n in Items.ToList())
            await _notifications.DeleteAsync(n.Id);

        Items.Clear();
        IsEmpty = true;
    }

    [RelayCommand]
    public async Task GoBackAsync() =>
        await Shell.Current.GoToAsync("..");
}
