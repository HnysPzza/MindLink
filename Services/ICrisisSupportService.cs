using M1ndLink.Models;

namespace M1ndLink.Services;

public interface ICrisisSupportService
{
    Task<List<EmergencyContact>> GetEmergencyContactsAsync();
    Task<EmergencyContact?> GetPrimaryEmergencyContactAsync();
    Task SaveEmergencyContactAsync(EmergencyContact contact);
    Task DeleteEmergencyContactAsync(EmergencyContact contact);
    List<CrisisResource> GetCrisisResources();
    List<GroundingStep> GetGroundingSteps();
    Task<bool> TryCallAsync(string phoneNumber);
    Task<bool> TryTextAsync(string phoneNumber, string message);
}
