using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace M1ndLink.Controls;

public class RiskIndicatorDot : SKCanvasView
{
    public static readonly BindableProperty RiskColorHexProperty =
        BindableProperty.Create(nameof(RiskColorHex), typeof(string), typeof(RiskIndicatorDot),
            "#22C55E", propertyChanged: (b, _, _) => ((RiskIndicatorDot)b).InvalidateSurface());

    public string RiskColorHex
    {
        get => (string)GetValue(RiskColorHexProperty);
        set => SetValue(RiskColorHexProperty, value);
    }

    private float _pulse = 0f;
    private IDispatcherTimer? _timer;

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler != null)
        {
            _timer = Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(30);
            _timer.Tick += (_, _) => { _pulse = (_pulse + 0.02f) % 1f; InvalidateSurface(); };
            _timer.Start();
        }
        else { _timer?.Stop(); _timer = null; }
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var (canvas, info) = (e.Surface.Canvas, e.Info);
        canvas.Clear();
        float cx = info.Width / 2f, cy = info.Height / 2f;
        float dot = Math.Min(cx, cy) * 0.38f, max = Math.Min(cx, cy) * 0.90f;

        SKColor.TryParse(RiskColorHex, out var color);

        // Pulse ring
        using var ring = new SKPaint { IsAntialias = true, Color = color.WithAlpha((byte)((1 - _pulse) * 70)) };
        canvas.DrawCircle(cx, cy, dot + (max - dot) * _pulse, ring);

        // Core dot
        using var core = new SKPaint { IsAntialias = true, Color = color };
        canvas.DrawCircle(cx, cy, dot, core);
    }
}
