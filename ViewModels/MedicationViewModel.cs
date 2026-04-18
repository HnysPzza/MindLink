using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;
using System.Collections.ObjectModel;

namespace M1ndLink.ViewModels;

public partial class MedicationViewModel : BaseViewModel
{
    private readonly IMedicationService _medicationService;

    [ObservableProperty] private ObservableCollection<Medication> _medications = new();
    [ObservableProperty] private ObservableCollection<MedicationDose> _todaysDoses = new();
    
    [ObservableProperty] private string _newMedName = string.Empty;
    [ObservableProperty] private string _newMedDosage = string.Empty;
    [ObservableProperty] private TimeSpan _newMedReminderTime = new TimeSpan(8, 0, 0);
    [ObservableProperty] private int _newMedCurrentCount = 30;

    [ObservableProperty] private bool _isAddMedVisible = false;

    public MedicationViewModel(IMedicationService medicationService)
    {
        _medicationService = medicationService;
        Title = "My Medications";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var meds = await _medicationService.GetActiveMedicationsAsync();
            Medications = new ObservableCollection<Medication>(meds);
            
            var doses = await _medicationService.GetTodaysDosesAsync();
            TodaysDoses = new ObservableCollection<MedicationDose>(doses);
        }
        catch (Exception)
        {
            // Silently fail — DB may not be ready on first launch
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public void ShowAddMed() => IsAddMedVisible = true;

    [RelayCommand]
    public void CancelAddMed()
    {
        IsAddMedVisible = false;
        NewMedName = "";
        NewMedDosage = "";
    }

    [RelayCommand]
    public async Task SaveMedicationAsync()
    {
        if (string.IsNullOrWhiteSpace(NewMedName)) return;
        
        IsBusy = true;
        try
        {
            var med = new Medication
            {
                Name = NewMedName,
                Dosage = NewMedDosage,
                CurrentPillCount = NewMedCurrentCount,
                ReminderTime = NewMedReminderTime
            };
            await _medicationService.SaveMedicationAsync(med);
            CancelAddMed();
        }
        catch (System.Security.SecurityException)
        {
            await Shell.Current.DisplayAlert("Permission Required", 
                "To get exact time reminders, please allow 'Alarms & Reminders' for M1ndLink in your Android Settings.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to save medication: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            await LoadAsync(); // runs AFTER IsBusy=false so the guard doesn’t skip it
        }
    }

    [RelayCommand]
    public async Task MarkTakenAsync(Medication med)
    {
        if (med == null) return;
        try { await _medicationService.MarkDoseTakenAsync(med.Id); }
        catch (Exception) { }
        await LoadAsync();
    }

    [RelayCommand]
    public async Task DeleteMedicationAsync(Medication med)
    {
        if (med == null) return;
        try { await _medicationService.DeleteMedicationAsync(med.Id); }
        catch (Exception) { }
        await LoadAsync();
    }

    [RelayCommand]
    public async Task GoBackAsync() => await Shell.Current.GoToAsync("..");
}
