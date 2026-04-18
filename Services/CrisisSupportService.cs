using M1ndLink.Models;

namespace M1ndLink.Services;

public class CrisisSupportService : ICrisisSupportService
{
    private readonly IDatabaseService _db;

    public CrisisSupportService(IDatabaseService db)
    {
        _db = db;
    }

    public async Task<List<EmergencyContact>> GetEmergencyContactsAsync()
    {
        var contacts = await _db.GetAllAsync<EmergencyContact>();
        return contacts
            .OrderByDescending(contact => contact.IsPrimary)
            .ThenBy(contact => contact.Name)
            .ToList();
    }

    public async Task<EmergencyContact?> GetPrimaryEmergencyContactAsync()
    {
        var contacts = await GetEmergencyContactsAsync();
        return contacts.FirstOrDefault(contact => contact.IsPrimary) ?? contacts.FirstOrDefault();
    }

    public async Task SaveEmergencyContactAsync(EmergencyContact contact)
    {
        var phone = NormalizePhoneNumber(contact.PhoneNumber);
        if (string.IsNullOrWhiteSpace(contact.Name) || string.IsNullOrWhiteSpace(phone))
            throw new InvalidOperationException("Name and phone number are required.");

        if (contact.IsPrimary)
        {
            var existing = await _db.GetAllAsync<EmergencyContact>();
            foreach (var saved in existing.Where(item => item.Id != contact.Id && item.IsPrimary))
            {
                saved.IsPrimary = false;
                await _db.SaveAsync(saved);
            }
        }

        contact.PhoneNumber = phone;
        await _db.SaveAsync(contact);
    }

    public async Task DeleteEmergencyContactAsync(EmergencyContact contact)
    {
        await _db.DeleteAsync(contact);
    }

    public List<CrisisResource> GetCrisisResources() => new()
    {
        new()
        {
            Name = "National Center for Mental Health Crisis Hotline",
            Description = "Mental health crisis intervention and emotional support in the Philippines.",
            PhoneNumber = "1553",
            Availability = "24/7",
            Region = "Philippines"
        },
        new()
        {
            Name = "Hopeline PH Globe / TM",
            Description = "Immediate emotional crisis support from trained responders.",
            PhoneNumber = "09175584673",
            SmsNumber = "2919",
            Availability = "24/7",
            Region = "Philippines"
        },
        new()
        {
            Name = "Hopeline PH PLDT / Smart",
            Description = "For urgent support when you need to speak with someone now.",
            PhoneNumber = "09187346789",
            Availability = "24/7",
            Region = "Philippines"
        },
        new()
        {
            Name = "Emergency Services",
            Description = "If you or someone else is in immediate danger, contact emergency responders now.",
            PhoneNumber = "911",
            Availability = "24/7",
            Region = "Emergency"
        },
        new()
        {
            Name = "Crisis Text Line (Global)",
            Description = "Free, 24/7, high-quality text-based mental health support and crisis intervention.",
            PhoneNumber = "",
            SmsNumber = "741741",
            Availability = "24/7",
            Region = "Global"
        }
    };

    public List<GroundingStep> GetGroundingSteps() => new()
    {
        new() { Title = "5 things you can see", Prompt = "Look around and name five things you can see right now." },
        new() { Title = "4 things you can feel", Prompt = "Notice four sensations like the chair beneath you, your clothing, or the air on your skin." },
        new() { Title = "3 things you can hear", Prompt = "Listen for three sounds, near or far, without judging them." },
        new() { Title = "2 things you can smell", Prompt = "Identify two smells, or remember two scents that feel safe and familiar." },
        new() { Title = "1 thing you can taste", Prompt = "Notice one taste in your mouth, or take a sip of water and focus on it fully." }
    };

    public async Task<bool> TryCallAsync(string phoneNumber)
    {
        try
        {
            await Launcher.Default.OpenAsync($"tel:{NormalizePhoneNumber(phoneNumber)}");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> TryTextAsync(string phoneNumber, string message)
    {
        try
        {
            var smsUri = $"sms:{NormalizePhoneNumber(phoneNumber)}?body={Uri.EscapeDataString(message)}";
            await Launcher.Default.OpenAsync(smsUri);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizePhoneNumber(string input) =>
        new(input.Where(ch => char.IsDigit(ch) || ch == '+').ToArray());
}
