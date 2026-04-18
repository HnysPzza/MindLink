using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using M1ndLink.Models;
using M1ndLink.Services;
using System.Collections.ObjectModel;

namespace M1ndLink.ViewModels;

public partial class CrisisSupportViewModel : BaseViewModel
{
    private readonly ICrisisSupportService _crisisSupport;
    private readonly INotificationService _notifications;

    [ObservableProperty] private ObservableCollection<EmergencyContact> _emergencyContacts = new();
    [ObservableProperty] private ObservableCollection<CrisisResource> _crisisResources = new();
    [ObservableProperty] private ObservableCollection<GroundingStep> _groundingSteps = new();
    [ObservableProperty] private string _newContactName = string.Empty;
    [ObservableProperty] private string _newContactRelationship = string.Empty;
    [ObservableProperty] private string _newContactPhoneNumber = string.Empty;
    [ObservableProperty] private bool _newContactIsPrimary = false;
    [ObservableProperty] private bool _hasContacts = false;
    [ObservableProperty] private ObservableCollection<DeviceContactOption> _availableDeviceContacts = new();
    [ObservableProperty] private string _deviceContactSearchText = string.Empty;
    [ObservableProperty] private bool _isPickingDeviceContact = false;
    [ObservableProperty] private bool _hasAvailableDeviceContacts = false;

    public CrisisSupportViewModel(ICrisisSupportService crisisSupport, INotificationService notifications)
    {
        _crisisSupport = crisisSupport;
        _notifications = notifications;
        Title = "Crisis Support";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            var contacts = await _crisisSupport.GetEmergencyContactsAsync();
            EmergencyContacts = new ObservableCollection<EmergencyContact>(contacts);
            HasContacts = contacts.Count > 0;
            CrisisResources = new ObservableCollection<CrisisResource>(_crisisSupport.GetCrisisResources());
            GroundingSteps = new ObservableCollection<GroundingStep>(_crisisSupport.GetGroundingSteps());
        }
        finally
        {
            IsBusy = false;
        }
    }

    public IEnumerable<DeviceContactOption> FilteredDeviceContacts => string.IsNullOrWhiteSpace(DeviceContactSearchText)
        ? AvailableDeviceContacts
        : AvailableDeviceContacts.Where(contact => contact.SearchText.Contains(DeviceContactSearchText, StringComparison.OrdinalIgnoreCase));

    partial void OnDeviceContactSearchTextChanged(string value) => OnPropertyChanged(nameof(FilteredDeviceContacts));

    partial void OnAvailableDeviceContactsChanged(ObservableCollection<DeviceContactOption> value)
    {
        HasAvailableDeviceContacts = value.Count > 0;
        OnPropertyChanged(nameof(FilteredDeviceContacts));
    }

    [RelayCommand]
    public void CancelDeviceContactPicker()
    {
        IsPickingDeviceContact = false;
        DeviceContactSearchText = string.Empty;
    }

    [RelayCommand]
    public void SelectDeviceContact(DeviceContactOption? contact)
    {
        if (contact == null)
            return;

        NewContactName = contact.DisplayName;
        if (string.IsNullOrWhiteSpace(NewContactRelationship))
            NewContactRelationship = "Imported from contacts";
        NewContactPhoneNumber = contact.PhoneNumber;
        IsPickingDeviceContact = false;
        DeviceContactSearchText = string.Empty;
    }

    [RelayCommand]
    public async Task SaveEmergencyContactAsync()
    {
        if (string.IsNullOrWhiteSpace(NewContactName) || string.IsNullOrWhiteSpace(NewContactPhoneNumber))
        {
            await Shell.Current.DisplayAlert("Missing Details", "Enter at least a name and phone number.", "OK");
            return;
        }

        try
        {
            await _crisisSupport.SaveEmergencyContactAsync(new EmergencyContact
            {
                Name = NewContactName.Trim(),
                Relationship = NewContactRelationship.Trim(),
                PhoneNumber = NewContactPhoneNumber.Trim(),
                IsPrimary = NewContactIsPrimary
            });

            await _notifications.AddAsync(new AppNotification
            {
                Title = "Emergency Contact Saved",
                Message = $"{NewContactName.Trim()} is now available from your Crisis Support page.",
                Icon = "🆘",
                Category = NotificationCategory.Profile
            });

            NewContactName = string.Empty;
            NewContactRelationship = string.Empty;
            NewContactPhoneNumber = string.Empty;
            NewContactIsPrimary = false;

            await LoadAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Could Not Save", ex.Message, "OK");
        }
    }

    [RelayCommand]
    public async Task PickDeviceContactAsync()
    {
        try
        {
            var contacts = (await Microsoft.Maui.ApplicationModel.Communication.Contacts.Default.GetAllAsync())
                .Where(contact => !string.IsNullOrWhiteSpace(contact.DisplayName)
                    && contact.Phones.Any(phone => !string.IsNullOrWhiteSpace(phone.PhoneNumber)))
                .Select(contact => new DeviceContactOption
                {
                    DisplayName = contact.DisplayName,
                    PhoneNumber = contact.Phones.First(phone => !string.IsNullOrWhiteSpace(phone.PhoneNumber)).PhoneNumber
                })
                .OrderBy(contact => contact.DisplayName)
                .ThenBy(contact => contact.PhoneNumber)
                .ToList();

            if (contacts.Count == 0)
            {
                await Shell.Current.DisplayAlert("No Contacts Found", "No saved contacts with phone numbers were available to import.", "OK");
                return;
            }

            AvailableDeviceContacts = new ObservableCollection<DeviceContactOption>(contacts);
            HasAvailableDeviceContacts = AvailableDeviceContacts.Count > 0;
            DeviceContactSearchText = string.Empty;
            IsPickingDeviceContact = true;
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Permission Required", "Contacts permission is needed to choose an existing contact.", "OK");
        }
        catch (FeatureNotSupportedException)
        {
            await Shell.Current.DisplayAlert("Not Supported", "Contact picking is not available on this device.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Could Not Open Contacts", ex.Message, "OK");
        }
    }

    [RelayCommand]
    public async Task DeleteContactAsync(EmergencyContact? contact)
    {
        if (contact == null)
            return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Remove Contact",
            $"Remove {contact.Name} from your emergency contacts?",
            "Remove",
            "Cancel");

        if (!confirm)
            return;

        await _crisisSupport.DeleteEmergencyContactAsync(contact);
        await LoadAsync();
    }

    [RelayCommand]
    public async Task CallContactAsync(EmergencyContact? contact)
    {
        if (contact == null)
            return;

        var success = await _crisisSupport.TryCallAsync(contact.PhoneNumber);
        if (!success)
            await Shell.Current.DisplayAlert("Call Not Available", "Your device could not start the phone call.", "OK");
    }

    [RelayCommand]
    public async Task TextContactAsync(EmergencyContact? contact)
    {
        if (contact == null)
            return;

        var success = await _crisisSupport.TryTextAsync(contact.PhoneNumber, "I need support right now. Can you check in with me?");
        if (!success)
            await Shell.Current.DisplayAlert("Text Not Available", "Your device could not open messaging right now.", "OK");
    }

    [RelayCommand]
    public async Task CallResourceAsync(CrisisResource? resource)
    {
        if (resource == null)
            return;

        var success = await _crisisSupport.TryCallAsync(resource.PhoneNumber);
        if (!success)
            await Shell.Current.DisplayAlert("Call Not Available", "Your device could not start the phone call.", "OK");
    }

    [RelayCommand]
    public async Task TextResourceAsync(CrisisResource? resource)
    {
        if (resource == null || !resource.HasSms)
            return;

        var success = await _crisisSupport.TryTextAsync(resource.SmsNumber, "I need support right now.");
        if (!success)
            await Shell.Current.DisplayAlert("Text Not Available", "Your device could not open messaging right now.", "OK");
    }

    [RelayCommand]
    public async Task OpenCrisisSupportAsync() =>
        await Shell.Current.GoToAsync("CrisisSupport");

    [RelayCommand]
    public async Task OpenSafetyPlanAsync() =>
        await Shell.Current.GoToAsync("SafetyPlan");

    [RelayCommand]
    public async Task GoBackAsync() =>
        await Shell.Current.GoToAsync("..");
}
