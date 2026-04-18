namespace M1ndLink.Models;

public class CrisisResource
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string SmsNumber { get; set; } = string.Empty;
    public string Availability { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool HasSms => !string.IsNullOrEmpty(SmsNumber);
    public bool HasPhone => !string.IsNullOrEmpty(PhoneNumber);
}
