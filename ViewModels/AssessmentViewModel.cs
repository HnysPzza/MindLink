using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;
using System.Collections.ObjectModel;

namespace M1ndLink.ViewModels;

public partial class AssessmentViewModel : BaseViewModel
{
    private readonly IMoodService _mood;
    private readonly INotificationService _notifications;
    private readonly IReminderService _reminders;

    [ObservableProperty] private int  _anxietyIndex = -1;
    [ObservableProperty] private int  _sleepIndex   = -1;
    [ObservableProperty] private int  _energyIndex  = -1;
    [ObservableProperty] private int  _socialIndex  = -1;
    [ObservableProperty] private int  _concentrationIndex = -1;
    [ObservableProperty] private int  _appetiteIndex = -1;
    [ObservableProperty] private int  _physicalSymptomsIndex = -1;
    [ObservableProperty] private int  _hopeIndex = -1;
    [ObservableProperty] private int  _confidenceIndex = -1;
    [ObservableProperty] private int  _copingIndex = -1;
    [ObservableProperty] private bool _isSubmitted  = false;
    [ObservableProperty] private string _resultText = string.Empty;
    [ObservableProperty] private string _resultSummary = string.Empty;
    [ObservableProperty] private string _comparisonSummary = string.Empty;
    [ObservableProperty] private double _progressValue = 0;
    [ObservableProperty] private int _answeredCount = 0;

    public ObservableCollection<string> AnxietyOptions { get; } = new() { "Never","Rarely","Sometimes","Often","Always" };
    public ObservableCollection<string> SleepOptions   { get; } = new() { "Very Poor","Poor","Fair","Good","Excellent" };
    public ObservableCollection<string> EnergyOptions  { get; } = new() { "Exhausted","Low","Moderate","Good","Energized" };
    public ObservableCollection<string> SocialOptions  { get; } = new() { "Isolated","Mostly Alone","Neutral","Somewhat Connected","Very Connected" };
    public ObservableCollection<string> ConcentrationOptions { get; } = new() { "Very hard","Hard","Manageable","Clear","Focused" };
    public ObservableCollection<string> AppetiteOptions { get; } = new() { "Very poor","Poor","Okay","Good","Steady" };
    public ObservableCollection<string> PhysicalOptions { get; } = new() { "None","Mild","Noticeable","Strong","Overwhelming" };
    public ObservableCollection<string> HopeOptions { get; } = new() { "Hopeless","Low","Unsure","Hopeful","Optimistic" };
    public ObservableCollection<string> ConfidenceOptions { get; } = new() { "Very low","Low","Okay","Good","Strong" };
    public ObservableCollection<string> CopingOptions { get; } = new() { "Not coping","Barely","Managing","Steady","Confident" };

    public bool CanSubmit => AnxietyIndex >= 0
        && SleepIndex >= 0
        && EnergyIndex >= 0
        && SocialIndex >= 0
        && ConcentrationIndex >= 0
        && AppetiteIndex >= 0
        && PhysicalSymptomsIndex >= 0
        && HopeIndex >= 0
        && ConfidenceIndex >= 0
        && CopingIndex >= 0;

    public int QuestionCount => 10;
    public string ProgressText => $"{AnsweredCount} of {QuestionCount} answered";

    public AssessmentViewModel(IMoodService mood, INotificationService notifications, IReminderService reminders) 
    { 
        _mood = mood; 
        _notifications = notifications;
        _reminders = reminders;
        Title = "Assessment"; 
    }

    partial void OnAnxietyIndexChanged(int value) => UpdateProgress();
    partial void OnSleepIndexChanged(int value)   => UpdateProgress();
    partial void OnEnergyIndexChanged(int value)  => UpdateProgress();
    partial void OnSocialIndexChanged(int value)  => UpdateProgress();
    partial void OnConcentrationIndexChanged(int value) => UpdateProgress();
    partial void OnAppetiteIndexChanged(int value) => UpdateProgress();
    partial void OnPhysicalSymptomsIndexChanged(int value) => UpdateProgress();
    partial void OnHopeIndexChanged(int value) => UpdateProgress();
    partial void OnConfidenceIndexChanged(int value) => UpdateProgress();
    partial void OnCopingIndexChanged(int value) => UpdateProgress();

    [RelayCommand]
    public async Task SubmitAssessmentAsync()
    {
        if (!CanSubmit || IsBusy) return;
        IsBusy = true;
        try
        {
            var result = new AssessmentResult
            {
                AnxietyScore = AnxietyIndex + 1,
                SleepScore   = SleepIndex   + 1,
                EnergyScore  = EnergyIndex  + 1,
                SocialScore  = SocialIndex  + 1,
                ConcentrationScore = ConcentrationIndex + 1,
                AppetiteScore = AppetiteIndex + 1,
                PhysicalSymptomsScore = PhysicalSymptomsIndex + 1,
                HopeScore = HopeIndex + 1,
                ConfidenceScore = ConfidenceIndex + 1,
                CopingScore = CopingIndex + 1,
            };

            var previous = await _mood.GetLatestAssessmentAsync();
            await _mood.SaveAssessmentAsync(result);

            await _notifications.AddAsync(new AppNotification
            {
                Title    = "Assessment Completed",
                Message  = $"Your current risk level is: {result.RiskLevel}.",
                Icon     = "",
                Category = NotificationCategory.Assessment
            });

            await _reminders.EvaluateDueRemindersAsync();

            ResultText  = result.RiskLevel;
            ResultSummary = result.Recommendation;
            ComparisonSummary = previous == null
                ? "This is your first detailed assessment, so future check-ins will show changes over time."
                : result.RiskScore < previous.RiskScore
                    ? "You look a bit more settled than your last check-in."
                    : result.RiskScore > previous.RiskScore
                        ? "This check-in suggests more strain than last time. Consider taking a slower next step today."
                        : "Your overall load looks similar to your last check-in.";
            IsSubmitted = true;
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task OpenHistoryAsync() =>
        await Shell.Current.GoToAsync("AssessmentHistory");

    [RelayCommand]
    public void ResetAssessment()
    {
        AnxietyIndex = SleepIndex = EnergyIndex = SocialIndex = -1;
        ConcentrationIndex = AppetiteIndex = PhysicalSymptomsIndex = HopeIndex = ConfidenceIndex = CopingIndex = -1;
        ResultSummary = string.Empty;
        ComparisonSummary = string.Empty;
        IsSubmitted = false;
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        var answers = new[]
        {
            AnxietyIndex,
            SleepIndex,
            EnergyIndex,
            SocialIndex,
            ConcentrationIndex,
            AppetiteIndex,
            PhysicalSymptomsIndex,
            HopeIndex,
            ConfidenceIndex,
            CopingIndex
        };

        AnsweredCount = answers.Count(value => value >= 0);
        ProgressValue = AnsweredCount / (double)QuestionCount;
        OnPropertyChanged(nameof(CanSubmit));
        OnPropertyChanged(nameof(ProgressText));
    }
}
