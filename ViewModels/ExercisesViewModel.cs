using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;
using System.Collections.ObjectModel;

namespace M1ndLink.ViewModels;

public partial class ExercisesViewModel : BaseViewModel
{
    private readonly IExerciseService _exercises;
    private readonly IMoodService _mood;

    [ObservableProperty] private ObservableCollection<Exercise> _exerciseList = new();
    [ObservableProperty] private Exercise? _recommendedExercise;
    [ObservableProperty] private string _recommendationReason = string.Empty;
    [ObservableProperty] private bool _hasRecommendation = false;

    // ── Custom exercise creation form ─────────────────────────────────────
    [ObservableProperty] private bool   _isAddFormVisible     = false;
    [ObservableProperty] private string _newExerciseTitle     = string.Empty;
    [ObservableProperty] private string _newExerciseDuration  = "5";
    [ObservableProperty] private string _newExerciseType      = "Breathing";

    public ExercisesViewModel(IExerciseService exercises, IMoodService mood)
    {
        _exercises = exercises;
        _mood = mood;
        Title = "Find Your Calm";
    }

    [RelayCommand]
    public async Task LoadRecommendationsAsync()
    {
        var all = await _exercises.GetAllExercisesAsync();
        var today = await _mood.GetTodaysMoodAsync();

        if (today == null)
        {
            HasRecommendation = false;
            ExerciseList = new ObservableCollection<Exercise>(all);
            return;
        }

        if (today.MoodLevel <= 2)
        {
            RecommendedExercise = all.FirstOrDefault(e => e.Id == 5) ?? all[0];
            RecommendationReason = "Recommended because you're feeling down today.";
        }
        else if (today.MoodLevel == 3)
        {
            RecommendedExercise = all.FirstOrDefault(e => e.Id == 3) ?? all[0];
            RecommendationReason = "Perfect for staying centered and balanced.";
        }
        else
        {
            RecommendedExercise = all.FirstOrDefault(e => e.Id == 4) ?? all[0];
            RecommendationReason = "A great way to carry that positive energy.";
        }

        HasRecommendation = true;
        ExerciseList = new ObservableCollection<Exercise>(
            all.Where(e => e.Id != RecommendedExercise!.Id));
    }

    [RelayCommand]
    public async Task StartExerciseAsync(Exercise exercise)
    {
        await Shell.Current.GoToAsync($"ExerciseSession?ExerciseId={exercise.Id}");
    }

    [RelayCommand]
    public async Task OpenMeditationLibraryAsync()
    {
        await Shell.Current.GoToAsync("MeditationLibrary");
    }

    // ── Favorite toggle ───────────────────────────────────────────────────
    [RelayCommand]
    public async Task ToggleFavoriteAsync(Exercise exercise)
    {
        await _exercises.ToggleFavoriteAsync(exercise.Id);
        await LoadRecommendationsAsync(); // refresh the list
    }

    // ── Custom exercise creation ──────────────────────────────────────────
    [RelayCommand]
    public void ShowAddForm()
    {
        IsAddFormVisible = true;
        NewExerciseTitle    = string.Empty;
        NewExerciseDuration = "5";
        NewExerciseType     = "Breathing";
    }

    [RelayCommand]
    public void HideAddForm() => IsAddFormVisible = false;

    [RelayCommand]
    public async Task SaveCustomExerciseAsync()
    {
        if (string.IsNullOrWhiteSpace(NewExerciseTitle))
        {
            await Shell.Current.DisplayAlert("Validation", "Please enter a title.", "OK");
            return;
        }

        if (!int.TryParse(NewExerciseDuration, out int dur) || dur < 1)
            dur = 5;

        var custom = new CustomExercise
        {
            Title           = NewExerciseTitle.Trim(),
            DurationMinutes = dur,
            TypeName        = NewExerciseType,
            IconEmoji       = NewExerciseType switch
            {
                "Meditation" => "🧘",
                "Grounding"  => "🌿",
                _            => "🌬️"
            }
        };

        await _exercises.SaveCustomExerciseAsync(custom);
        IsAddFormVisible = false;
        await LoadRecommendationsAsync();
    }
}
