namespace M1ndLink.Controls;

public class AnimatedLogoView : ContentView
{
    private readonly Border _badge;
    private readonly Label _titleLabel;
    private readonly Label _subtitleLabel;
    private CancellationTokenSource? _animationCts;

    public AnimatedLogoView()
    {
        _badge = new Border
        {
            WidthRequest = 96,
            HeightRequest = 96,
            StrokeThickness = 0,
            Background = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb("#2563EB"), 0f),
                    new GradientStop(Color.FromArgb("#60A5FA"), 1f)
                },
                new Point(0, 0),
                new Point(1, 1)),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 32 },
            Shadow = new Shadow
            {
                Brush = Color.FromArgb("#662563EB"),
                Offset = new Point(0, 14),
                Radius = 28,
                Opacity = 0.55f
            },
            Content = new Image
            {
                Source = "appiconfg.png",
                Aspect = Aspect.AspectFit,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Margin = new Thickness(12)
            }
        };

        _titleLabel = new Label
        {
            Text = "MindLink",
            FontFamily = "SyneBold",
            FontSize = 30,
            TextColor = Color.FromArgb("#0F172A"),
            HorizontalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        _subtitleLabel = new Label
        {
            Text = "Breathe, reflect, and stay connected.",
            FontFamily = "JakartaRegular",
            FontSize = 14,
            TextColor = Color.FromArgb("#5B6B83"),
            HorizontalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        Content = new VerticalStackLayout
        {
            Spacing = 14,
            HorizontalOptions = LayoutOptions.Center,
            Children = { _badge, _titleLabel, _subtitleLabel }
        };
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler != null)
        {
            StartAnimation();
        }
        else
        {
            StopAnimation();
        }
    }

    private void StartAnimation()
    {
        StopAnimation();
        _animationCts = new CancellationTokenSource();
        _ = RunAnimationAsync(_animationCts.Token);
    }

    private void StopAnimation()
    {
        _animationCts?.Cancel();
        _animationCts?.Dispose();
        _animationCts = null;
    }

    private async Task RunAnimationAsync(CancellationToken token)
    {
        _badge.Scale = 0.92;
        _badge.Opacity = 0.88;
        _titleLabel.TranslationY = 10;
        _titleLabel.Opacity = 0;
        _subtitleLabel.Opacity = 0;

        await _titleLabel.FadeToAsync(1, 450, Easing.CubicOut);
        await _titleLabel.TranslateToAsync(0, 0, 450, Easing.CubicOut);
        await _subtitleLabel.FadeToAsync(1, 550, Easing.CubicOut);

        while (!token.IsCancellationRequested)
        {
            await Task.WhenAll(
                _badge.ScaleToAsync(1.02, 1400, Easing.SinInOut),
                _badge.FadeToAsync(1, 1400, Easing.SinInOut));

            if (token.IsCancellationRequested)
                break;

            await Task.WhenAll(
                _badge.ScaleToAsync(0.94, 1400, Easing.SinInOut),
                _badge.FadeToAsync(0.9, 1400, Easing.SinInOut));
        }
    }
}
