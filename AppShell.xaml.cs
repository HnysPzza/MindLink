namespace M1ndLink;
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        Routing.RegisterRoute("CrisisSupport", typeof(Views.CrisisSupportPage));
        Routing.RegisterRoute("TodoList", typeof(Views.TodoListPage));
        Routing.RegisterRoute("AddEditTask", typeof(Views.AddEditTaskPage));
        Routing.RegisterRoute("AssessmentHistory", typeof(Views.AssessmentHistoryPage));
        Routing.RegisterRoute("EditProfile",    typeof(Views.EditProfilePage));
        Routing.RegisterRoute("GeneralSettings", typeof(Views.GeneralSettingsPage));
        Routing.RegisterRoute("MeditationLibrary", typeof(Views.MeditationLibraryPage));
        Routing.RegisterRoute("MeditationPlayer", typeof(Views.MeditationPlayerPage));
        Routing.RegisterRoute("Notifications",  typeof(Views.NotificationsPage));
        Routing.RegisterRoute("MoodJournal",    typeof(Views.MoodJournalPage));
        Routing.RegisterRoute("ExerciseSession",typeof(Views.ExerciseSessionPage));
        Routing.RegisterRoute("SafetyPlan",     typeof(Views.SafetyPlanPage));
        Routing.RegisterRoute("Habits", typeof(Views.HabitsPage));
        Routing.RegisterRoute("Medication", typeof(Views.MedicationPage));
        Routing.RegisterRoute("SleepDiary", typeof(Views.SleepDiaryPage));
        Routing.RegisterRoute("Triggers", typeof(Views.TriggersPage));
        Routing.RegisterRoute("AdvancedReporting", typeof(Views.AdvancedReportingPage));
    }
}
