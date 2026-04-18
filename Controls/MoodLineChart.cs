using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace M1ndLink.Controls;

public class MoodLineChart : SKCanvasView
{
    public static readonly BindableProperty DataProperty =
        BindableProperty.Create(nameof(Data), typeof(IList<float>), typeof(MoodLineChart),
            null, propertyChanged: (b, _, _) => ((MoodLineChart)b).AnimateIn());

    public IList<float>? Data
    {
        get => (IList<float>?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    private float _progress = 0f;
    private IDispatcherTimer? _timer;

    private void AnimateIn()
    {
        _progress = 0f;
        _timer?.Stop();
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick += (_, _) =>
        {
            _progress = Math.Min(_progress + 0.022f, 1f);
            InvalidateSurface();
            if (_progress >= 1f) _timer?.Stop();
        };
        _timer.Start();
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info   = e.Info;
        canvas.Clear();

        if (Data == null || Data.Count < 2) return;

        float w  = info.Width, h = info.Height;
        float pH = h * 0.12f, pW = w * 0.04f;
        float cH = h - pH * 2, cW = w - pW * 2;
        int   n  = Data.Count;
        float xs = cW / (n - 1);

        SKPoint[] pts = Data
            .Select((v, i) => new SKPoint(
                pW + i * xs,
                pH + cH - ((v - 1f) / 4f) * cH))
            .ToArray();

        var linePath = Smooth(pts);

        // Clip draw-on reveal
        using var clip = new SKPath();
        clip.AddRect(SKRect.Create(0, 0, w * _progress, h));
        canvas.Save();
        canvas.ClipPath(clip);

        // Fill under curve
        var fill = new SKPath(linePath);
        fill.LineTo(pts[^1].X, h);
        fill.LineTo(pts[0].X, h);
        fill.Close();

        using var fillPaint = new SKPaint { IsAntialias = true };
        fillPaint.Shader = SKShader.CreateLinearGradient(
            new SKPoint(0, pH), new SKPoint(0, h),
            new SKColor[] { new(0x60, 0xA5, 0xFA, 100), new(0x60, 0xA5, 0xFA, 0) },
            null, SKShaderTileMode.Clamp);
        canvas.DrawPath(fill, fillPaint);

        // Line
        using var line = new SKPaint
        {
            IsAntialias = true, Style = SKPaintStyle.Stroke,
            Color = new SKColor(0x3B, 0x82, 0xF6),
            StrokeWidth = 3f, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round
        };
        canvas.DrawPath(linePath, line);

        // Dots
        using var dot = new SKPaint { IsAntialias = true, Color = new SKColor(0x25, 0x63, 0xEB) };
        foreach (var p in pts) canvas.DrawCircle(p, 4f, dot);

        canvas.Restore();

        // Gridlines (no clip)
        using var grid = new SKPaint
        {
            IsAntialias = true, Style = SKPaintStyle.Stroke,
            Color = new SKColor(0xBF, 0xDB, 0xFE, 100), StrokeWidth = 1f
        };
        for (int i = 1; i <= 4; i++)
        {
            float y = pH + (cH / 4f) * i;
            canvas.DrawLine(pW, y, w - pW, y, grid);
        }
    }

    private static SKPath Smooth(SKPoint[] pts)
    {
        var path = new SKPath();
        path.MoveTo(pts[0]);
        for (int i = 0; i < pts.Length - 1; i++)
        {
            var cp1 = new SKPoint((pts[i].X + pts[i+1].X) / 2f, pts[i].Y);
            var cp2 = new SKPoint((pts[i].X + pts[i+1].X) / 2f, pts[i+1].Y);
            path.CubicTo(cp1, cp2, pts[i+1]);
        }
        return path;
    }
}
