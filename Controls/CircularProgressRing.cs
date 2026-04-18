using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace M1ndLink.Controls;

public class CircularProgressRing : SKCanvasView
{
    public static readonly BindableProperty ProgressProperty =
        BindableProperty.Create(nameof(Progress), typeof(double), typeof(CircularProgressRing),
            0.0, propertyChanged: (b, _, n) => ((CircularProgressRing)b).AnimateTo((double)n));
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(CircularProgressRing),
            string.Empty, propertyChanged: (b, _, _) => ((CircularProgressRing)b).InvalidateSurface());
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(int), typeof(CircularProgressRing),
            0, propertyChanged: (b, _, _) => ((CircularProgressRing)b).InvalidateSurface());

    public double Progress { get => (double)GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }
    public string Label    { get => (string)GetValue(LabelProperty);    set => SetValue(LabelProperty, value); }
    public int Value       { get => (int)GetValue(ValueProperty);       set => SetValue(ValueProperty, value); }

    private double _anim = 0;

    private void AnimateTo(double target)
    {
        new Animation(v => { _anim = v; InvalidateSurface(); }, _anim, target, Easing.CubicOut)
            .Commit(this, "Ring", 16, 800);
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var (canvas, info) = (e.Surface.Canvas, e.Info);
        canvas.Clear();
        float cx = info.Width / 2f, cy = info.Height / 2f;
        float r  = Math.Min(cx, cy) * 0.78f;
        float sw = Math.Min(cx, cy) * 0.14f;

        using var track = new SKPaint
        {
            IsAntialias = true, Style = SKPaintStyle.Stroke,
            Color = new SKColor(0xDB, 0xEA, 0xFE), StrokeWidth = sw, StrokeCap = SKStrokeCap.Round
        };
        canvas.DrawCircle(cx, cy, r, track);

        if (_anim > 0)
        {
            var rect = new SKRect(cx - r, cy - r, cx + r, cy + r);
            using var arc = new SKPaint
            {
                IsAntialias = true, Style = SKPaintStyle.Stroke,
                StrokeWidth = sw, StrokeCap = SKStrokeCap.Round
            };
            arc.Shader = SKShader.CreateSweepGradient(new SKPoint(cx, cy),
                new SKColor[] { new(0x3B, 0x82, 0xF6), new(0x25, 0x63, 0xEB) }, null);
            canvas.DrawArc(rect, -90f, (float)(_anim * 360.0), false, arc);
        }

        // Value text
        using var vPaint = new SKPaint
        {
            IsAntialias = true, Color = new SKColor(0x25, 0x63, 0xEB),
            TextSize = info.Width * 0.22f, TextAlign = SKTextAlign.Center,
            Typeface = SKTypeface.FromFamilyName("sans-serif",
                SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
        };
        canvas.DrawText(Value.ToString(), cx, cy + vPaint.TextSize * 0.35f, vPaint);

        // Label text
        using var lPaint = new SKPaint
        {
            IsAntialias = true, Color = new SKColor(0x3B, 0x6F, 0xA0),
            TextSize = info.Width * 0.10f, TextAlign = SKTextAlign.Center
        };
        canvas.DrawText(Label, cx, cy + vPaint.TextSize * 0.35f + lPaint.TextSize * 1.5f, lPaint);
    }
}
