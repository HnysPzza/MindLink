using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace M1ndLink.Controls;

/// <summary>
/// Step-line chart showing risk level transitions over time.
/// Data values: 1 = Low, 2 = Medium, 3 = High.
/// </summary>
public class RiskLevelChart : SKCanvasView
{
    public static readonly BindableProperty DataProperty =
        BindableProperty.Create(nameof(Data), typeof(IList<float>), typeof(RiskLevelChart),
            null, propertyChanged: (b, _, _) => ((RiskLevelChart)b).AnimateIn());

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
            _progress = Math.Min(_progress + 0.025f, 1f);
            InvalidateSurface();
            if (_progress >= 1f) _timer?.Stop();
        };
        _timer.Start();
    }

    // Color for each risk level
    private static readonly SKColor LowColor    = new(34, 197, 94);    // #22C55E
    private static readonly SKColor MediumColor = new(245, 158, 11);   // #F59E0B
    private static readonly SKColor HighColor   = new(239, 68, 68);    // #EF4444

    private static SKColor GetRiskColor(float val)
    {
        if (val <= 1.5f) return LowColor;
        if (val <= 2.5f) return MediumColor;
        return HighColor;
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info   = e.Info;
        canvas.Clear();

        if (Data == null || Data.Count < 2) return;

        float w  = info.Width, h = info.Height;
        float pH = h * 0.14f, pW = w * 0.05f;
        float cH = h - pH * 2, cW = w - pW * 2;
        int   n  = Data.Count;
        float xs = cW / (n - 1);

        // Map risk values (1-3) to Y positions (bottom to top)
        SKPoint[] pts = Data
            .Select((v, i) => new SKPoint(
                pW + i * xs,
                pH + cH - ((v - 1f) / 2f) * cH))
            .ToArray();

        // Gridlines for the 3 levels
        using var gridPaint = new SKPaint
        {
            IsAntialias = true, Style = SKPaintStyle.Stroke,
            Color = new SKColor(0xE5, 0xE7, 0xEB, 100), StrokeWidth = 1f
        };
        string[] labels = { "High", "Medium", "Low" };
        using var labelPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(0x9C, 0xA3, 0xAF),
        };
        using var labelFont = new SKFont(SKTypeface.Default, 20);
        for (int i = 0; i < 3; i++)
        {
            float y = pH + (cH / 2f) * i;
            canvas.DrawLine(pW, y, w - pW, y, gridPaint);
        }

        // Clip for animation
        using var clip = new SKPath();
        clip.AddRect(SKRect.Create(0, 0, w * _progress, h));
        canvas.Save();
        canvas.ClipPath(clip);

        // Draw step segments with color per segment
        for (int i = 0; i < pts.Length - 1; i++)
        {
            var color = GetRiskColor(Data[i]);
            using var segPaint = new SKPaint
            {
                IsAntialias = true, Style = SKPaintStyle.Stroke,
                Color = color, StrokeWidth = 4f,
                StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round
            };

            // Step line: horizontal then vertical
            float midX = (pts[i].X + pts[i + 1].X) / 2f;
            canvas.DrawLine(pts[i].X, pts[i].Y, midX, pts[i].Y, segPaint);
            canvas.DrawLine(midX, pts[i].Y, midX, pts[i + 1].Y, segPaint);
            canvas.DrawLine(midX, pts[i + 1].Y, pts[i + 1].X, pts[i + 1].Y, segPaint);
        }

        // Dots at each data point
        foreach (var (pt, idx) in pts.Select((p, i) => (p, i)))
        {
            var color = GetRiskColor(Data[idx]);
            using var dotPaint = new SKPaint { IsAntialias = true, Color = color };
            canvas.DrawCircle(pt, 5f, dotPaint);

            // White inner dot
            using var inner = new SKPaint { IsAntialias = true, Color = SKColors.White };
            canvas.DrawCircle(pt, 2.5f, inner);
        }

        canvas.Restore();
    }
}
