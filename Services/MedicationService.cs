using M1ndLink.Models;
using Plugin.LocalNotification;

namespace M1ndLink.Services;

public class MedicationService : IMedicationService
{
    private readonly IDatabaseService _db;
    private readonly IPlatformNotificationService _notifications;

    public MedicationService(IDatabaseService db, IPlatformNotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<List<Medication>> GetActiveMedicationsAsync()
    {
        var meds = await _db.GetAllAsync<Medication>();
        return meds.Where(m => m.IsActive).ToList();
    }

    public async Task SaveMedicationAsync(Medication medication)
    {
        await _db.SaveAsync(medication);
        
        // Schedule reminder
        if (medication.IsActive)
        {
            var notifyTime = DateTime.Today.Add(medication.ReminderTime);
            if (notifyTime <= DateTime.Now) notifyTime = notifyTime.AddDays(1);
            
            // Generate deterministic ID
            int notifId = 10000 + medication.Id;
            await _notifications.ScheduleRepeatingAsync(notifId, "Medication Reminder", $"Time to take {medication.Name} ({medication.Dosage})", notifyTime, TimeSpan.FromDays(1), "MedicationPayload");
        }
    }

    public async Task DeleteMedicationAsync(int id)
    {
        var med = await _db.GetByIdAsync<Medication>(id);
        if (med != null)
        {
            await _db.DeleteAsync(med);
            await _notifications.CancelAsync(10000 + id);
        }
    }

    public async Task<List<MedicationDose>> GetTodaysDosesAsync()
    {
        var doses = await _db.GetAllAsync<MedicationDose>();
        return doses.Where(d => d.Date.Date == DateTime.Today).ToList();
    }

    public async Task MarkDoseTakenAsync(int medicationId)
    {
        var dose = new MedicationDose
        {
            MedicationId = medicationId,
            Date = DateTime.Now,
            IsTaken = true
        };
        await _db.SaveAsync(dose);

        var med = await _db.GetByIdAsync<Medication>(medicationId);
        if (med != null)
        {
            med.CurrentPillCount -= 1;
            if (med.CurrentPillCount < 0) med.CurrentPillCount = 0;
            await _db.SaveAsync(med);

            if (med.CurrentPillCount <= med.RefillThreshold)
            {
                await _notifications.ShowImmediateAsync(20000 + med.Id, "Prescription Refill Reminder", $"You are running low on {med.Name}. Only {med.CurrentPillCount} pills left.", "RefillPayload");
            }
        }
    }

    public async Task RefillMedicationAsync(int medicationId, int amount)
    {
        var med = await _db.GetByIdAsync<Medication>(medicationId);
        if (med != null)
        {
            med.CurrentPillCount += amount;
            await _db.SaveAsync(med);
        }
    }
}
