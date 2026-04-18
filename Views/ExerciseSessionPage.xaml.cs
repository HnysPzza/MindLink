using M1ndLink.ViewModels;

namespace M1ndLink.Views;

public partial class ExerciseSessionPage : ContentPage
{
    private readonly ExerciseSessionViewModel _vm;

    public ExerciseSessionPage(ExerciseSessionViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;

        // When BreathProgress changes, invalidate the canvas
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ExerciseSessionViewModel.BreathProgress))
            {
                _ = SyncBreathingScaleAsync(vm.BreathPhase, vm.BreathProgress, animate: false);
            }
            if (e.PropertyName == nameof(ExerciseSessionViewModel.SessionProgress))
            {
                UpdateProgressBar(vm.SessionProgress);
            }
        };

        // Animation event from ViewModel start-of-phase
        vm.BreathAnimationRequested += OnBreathAnimationRequested;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Defensive: stop timers if user navigates away (back gesture)
        if (_vm.Phase == SessionPhase.Active)
            _vm.EndEarlyCommand.Execute(null);
    }

    private async Task SyncBreathingScaleAsync(BreathPhase phase, double progress, bool animate)
    {
        double minScale = 0.82;
        double maxScale = 1.08;
        double clampedProgress = Math.Clamp(progress, 0, 1);
        double targetScale = phase switch
        {
            BreathPhase.Inhale => minScale + ((maxScale - minScale) * clampedProgress),
            BreathPhase.Hold => maxScale,
            BreathPhase.Exhale => minScale + ((maxScale - minScale) * clampedProgress),
            _ => minScale
        };

        this.AbortAnimation("BreathScaleAnimation");

        if (!animate)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                BreathingAnimationHost.Scale = targetScale;
            });
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await BreathingAnimationHost.ScaleTo(targetScale, 320, Easing.CubicInOut);
        });
    }

    // ── Progress bar width ────────────────────────────────────────────────
    private void UpdateProgressBar(double progress)
    {
        // ProgressFill is a BoxView; set its HorizontalOptions width proportionally
        MainThread.BeginInvokeOnMainThread(() =>
        {
            double containerWidth = ProgressFill.Parent is View parent
                ? parent.Width : Width - 40;
            ProgressFill.WidthRequest = Math.Max(containerWidth * progress, 4);
        });
    }

    // ── Breath phase transition (optional: could trigger haptic here) ──────
    private void OnBreathAnimationRequested(double startProgress, BreathPhase phase)
    {
        _ = SyncBreathingScaleAsync(phase, startProgress, animate: true);
    }
}
