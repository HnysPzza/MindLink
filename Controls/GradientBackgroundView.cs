using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace M1ndLink.Controls;

public class GradientBackgroundView : SKCanvasView
{
    private float _tick = 0f;
    private IDispatcherTimer? _timer;

    private static readonly SKColor C0 = new(0xEF, 0xF6, 0xFF); // #EFF6FF
    private static readonly SKColor C1 = new(0xDB, 0xEA, 0xFE); // #DBEAFE
    private static readonly SKColor C2 = new(0xBF, 0xDB, 0xFE); // #BFDBFE

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler != null) Start(); else Stop();
    }

    private void Start()
    {
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(33);
        _timer.Tick += (_, _) => { _tick += 0.004f; InvalidateSurface(); };
        _timer.Start();
    }

    private void Stop() { _timer?.Stop(); _timer = null; }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var (canvas, info) = (e.Surface.Canvas, e.Info);
        float w = info.Width, h = info.Height;
        canvas.Clear();

        // Base linear gradient top-to-bottom
        using var bg = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0), new SKPoint(0, h),
                new[] { C0, C1, C0 }, new[] { 0f, 0.5f, 1f },
                SKShaderTileMode.Clamp)
        };
        canvas.DrawRect(0, 0, w, h, bg);

        // Orb 1 — top-left drift
        Orb(canvas,
            w * (0.25f + 0.12f * MathF.Sin(_tick * 0.7f)),
            h * (0.22f + 0.08f * MathF.Cos(_tick * 0.5f)),
            w * 0.55f, C2.WithAlpha(95));

        // Orb 2 — bottom-right drift
        Orb(canvas,
            w * (0.78f + 0.10f * MathF.Cos(_tick * 0.6f)),
            h * (0.68f + 0.10f * MathF.Sin(_tick * 0.8f)),
            w * 0.48f, C1.WithAlpha(115));
    }

    private static void Orb(SKCanvas c, float cx, float cy, float r, SKColor color)
    {
        using var p = new SKPaint { IsAntialias = true };
        p.Shader = SKShader.CreateRadialGradient(
            new SKPoint(cx, cy), r,
            new[] { color, color.WithAlpha(0) }, null, SKShaderTileMode.Clamp);
        c.DrawCircle(cx, cy, r, p);
    }
}
