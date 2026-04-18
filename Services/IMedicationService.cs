using M1ndLink.Models;

namespace M1ndLink.Services;

public interface IMedicationService
{
    Task<List<Medication>> GetActiveMedicationsAsync();
    Task SaveMedicationAsync(Medication medication);
    Task DeleteMedicationAsync(int id);
    Task<List<MedicationDose>> GetTodaysDosesAsync();
    Task MarkDoseTakenAsync(int medicationId);
    Task RefillMedicationAsync(int medicationId, int amount);
}
