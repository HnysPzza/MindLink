using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M1ndLink.Models;
using M1ndLink.Services;
using M1ndLink.Views;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;

namespace M1ndLink.ViewModels;

public partial class OnboardingViewModel : BaseViewModel
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty] private ObservableCollection<OnboardingCard> _cards = new();
    [ObservableProperty] private int _currentIndex;

    public string PrimaryActionText => CurrentIndex >= Cards.Count - 1 ? "Get Started" : "Next";
    public bool IsLastCard => CurrentIndex >= Cards.Count - 1;

    public OnboardingViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Title = "Welcome";
        Cards = new ObservableCollection<OnboardingCard>
        {
            new OnboardingCard
            {
                Icon = "📈",
                AccentLabel = "Track your progress",
                Title = "Mental Health Tracking",
                Description = "Log your mood, keep your streak alive, and understand how your emotional wellness changes over time.",
                GradientStart = "#2563EB",
                GradientEnd = "#38BDF8"
            },
            new OnboardingCard
            {
                Icon = "🫁",
                AccentLabel = "Reset with breath",
                Title = "Breathing Exercises",
                Description = "Follow calming guided routines with immersive breathing visuals designed to slow things down when your mind feels full.",
                GradientStart = "#0EA5E9",
                GradientEnd = "#14B8A6"
            },
            new OnboardingCard
            {
                Icon = "🆘",
                AccentLabel = "Reach help faster",
                Title = "Crisis Support",
                Description = "Keep emergency contacts, crisis lines, and grounding steps close so support is one tap away when you need it most.",
                GradientStart = "#F97316",
                GradientEnd = "#EF4444"
            },
            new OnboardingCard
            {
                Icon = "🧠",
                AccentLabel = "Check in deeply",
                Title = "Assessment Tools",
                Description = "Use 10-question emotional check-ins to spot changes in pressure, focus, hope, and coping with richer personal insights.",
                GradientStart = "#7C3AED",
                GradientEnd = "#2563EB"
            }
        };
    }

    partial void OnCurrentIndexChanged(int value)
    {
        OnPropertyChanged(nameof(PrimaryActionText));
        OnPropertyChanged(nameof(IsLastCard));
    }

    [RelayCommand]
    private async Task AdvanceAsync()
    {
        if (CurrentIndex < Cards.Count - 1)
        {
            CurrentIndex++;
            return;
        }

        await CompleteAsync();
    }

    [RelayCommand]
    private async Task SkipAsync()
    {
        await CompleteAsync();
    }

    private Task CompleteAsync()
    {
        Application.Current!.MainPage = _serviceProvider.GetRequiredService<LoginPage>();
        return Task.CompletedTask;
    }
}
