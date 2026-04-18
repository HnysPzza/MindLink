namespace M1ndLink.Models;

public class DeviceContactOption
{
    public string DisplayName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string SearchText => $"{DisplayName} {PhoneNumber}";
}
