using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;

namespace M1ndLink.ViewModels;

public partial class SafetyPlanViewModel : BaseViewModel
{
    private readonly IDatabaseService _db;
    private SafetyPlan? _currentPlan;

    [ObservableProperty] private string _warningSigns = string.Empty;
    [ObservableProperty] private string _copingStrategies = string.Empty;
    [ObservableProperty] private string _peopleToContact = string.Empty;
    [ObservableProperty] private string _professionalsToContact = string.Empty;
    [ObservableProperty] private string _safeEnvironments = string.Empty;

    public SafetyPlanViewModel(IDatabaseService db)
    {
        _db = db;
        Title = "Safety Plan";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var plans = await _db.GetAllAsync<SafetyPlan>();
            _currentPlan = plans.OrderByDescending(p => p.UpdatedAt).FirstOrDefault();

            if (_currentPlan != null)
            {
                WarningSigns = _currentPlan.WarningSigns;
                CopingStrategies = _currentPlan.CopingStrategies;
                PeopleToContact = _currentPlan.PeopleToContact;
                ProfessionalsToContact = _currentPlan.ProfessionalsToContact;
                SafeEnvironments = _currentPlan.SafeEnvironments;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            _currentPlan ??= new SafetyPlan();

            _currentPlan.WarningSigns = WarningSigns;
            _currentPlan.CopingStrategies = CopingStrategies;
            _currentPlan.PeopleToContact = PeopleToContact;
            _currentPlan.ProfessionalsToContact = ProfessionalsToContact;
            _currentPlan.SafeEnvironments = SafeEnvironments;
            _currentPlan.UpdatedAt = DateTime.Now;

            await _db.SaveAsync(_currentPlan);
            
            await Shell.Current.DisplayAlert("Saved", "Your safety plan has been updated safely.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", "Could not save your safety plan.", "OK");
            Console.WriteLine(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task GoBackAsync() =>
        await Shell.Current.GoToAsync("..");
}
