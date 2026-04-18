using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace M1ndLink.Controls;

public class AnatomicalLungView : SKCanvasView
{
    public static readonly BindableProperty BreathProgressProperty = BindableProperty.Create(
        nameof(BreathProgress),
        typeof(double),
        typeof(AnatomicalLungView),
        0d,
        propertyChanged: OnVisualPropertyChanged);

    public double BreathProgress
    {
        get => (double)GetValue(BreathProgressProperty);
        set => SetValue(BreathProgressProperty, value);
    }

    public AnatomicalLungView()
    {
        EnableTouchEvents = false;
        IgnorePixelScaling = false;
        PaintSurface += OnPaintSurface;
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AnatomicalLungView view)
            view.InvalidateSurface();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var info = e.Info;
        float width = info.Width;
        float height = info.Height;
        float cx = width / 2f;
        float top = height * 0.10f;
        float bottom = height * 0.92f;
        float scale = 0.94f + ((float)Math.Clamp(BreathProgress, 0, 1) * 0.18f);
        float glowOpacity = 60f + ((float)Math.Clamp(BreathProgress, 0, 1) * 70f);

        canvas.Save();
        canvas.Translate(cx, height * 0.52f);
        canvas.Scale(scale, scale);
        canvas.Translate(-cx, -(height * 0.52f));

        using var glowPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColor.Parse("#FF8AA5").WithAlpha((byte)Math.Clamp(glowOpacity, 0, 255))
        };
        canvas.DrawOval(new SKRect(cx - width * 0.30f, top + height * 0.15f, cx + width * 0.30f, bottom - height * 0.08f), glowPaint);

        var leftLung = BuildLungPath(cx - width * 0.03f, top, bottom, width * 0.30f, true);
        var rightLung = BuildLungPath(cx + width * 0.03f, top, bottom, width * 0.30f, false);

        using var fillPaint = new SKPaint { Style = SKPaintStyle.Fill, IsAntialias = true };
        fillPaint.Shader = SKShader.CreateLinearGradient(
            new SKPoint(cx, top),
            new SKPoint(cx, bottom),
            new[]
            {
                SKColor.Parse("#FFB3C1"),
                Mix(SKColor.Parse("#F58CA8"), SKColor.Parse("#C7516B"), (float)Math.Clamp(1 - BreathProgress, 0, 1)),
                SKColor.Parse("#A43752")
            },
            null,
            SKShaderTileMode.Clamp);

        using var outlinePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            StrokeWidth = width * 0.013f,
            Color = SKColor.Parse("#8D314A")
        };

        canvas.DrawPath(leftLung, fillPaint);
        canvas.DrawPath(rightLung, fillPaint);
        canvas.DrawPath(leftLung, outlinePaint);
        canvas.DrawPath(rightLung, outlinePaint);

        using var fissurePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            StrokeWidth = width * 0.008f,
            Color = SKColor.Parse("#C34C68").WithAlpha(180)
        };
        DrawFissures(canvas, fissurePaint, cx, top, bottom, width);

        using var tracheaPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round,
            StrokeWidth = width * 0.038f,
            Color = SKColor.Parse("#C47B85")
        };
        using var airwayPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round,
            StrokeWidth = width * 0.018f,
            Color = SKColor.Parse("#B15369")
        };
        using var bronchiolePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round,
            StrokeWidth = width * 0.009f,
            Color = SKColor.Parse("#D46F89")
        };
        DrawAirways(canvas, tracheaPaint, airwayPaint, bronchiolePaint, cx, top, bottom, width);

        using var alveoliPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true,
            Color = SKColor.Parse("#FFD7E1").WithAlpha((byte)(110 + (int)(Math.Clamp(BreathProgress, 0, 1) * 80)))
        };
        DrawAlveoli(canvas, alveoliPaint, cx, top, bottom, width);

        canvas.Restore();
    }

    private static SKPath BuildLungPath(float anchorX, float top, float bottom, float width, bool isLeft)
    {
        float dir = isLeft ? -1f : 1f;
        float lungTop = top + (bottom - top) * 0.10f;
        float lungBottom = bottom - (bottom - top) * 0.06f;
        float midY = (lungTop + lungBottom) / 2f;
        float innerX = anchorX;
        float outerX = anchorX + (width * dir);

        var path = new SKPath();
        path.MoveTo(innerX, lungTop + 18f);
        path.CubicTo(innerX + (width * 0.10f * dir), lungTop - 8f, outerX, lungTop + 10f, outerX, midY - 18f);
        path.CubicTo(outerX + (width * 0.06f * dir), midY + 8f, anchorX + (width * 0.72f * dir), lungBottom, anchorX + (width * 0.12f * dir), lungBottom);
        path.CubicTo(anchorX - (width * 0.06f * dir), lungBottom - 18f, innerX - (width * 0.16f * dir), midY + 30f, innerX, midY + 8f);
        path.CubicTo(innerX + (width * 0.03f * dir), midY - 18f, innerX + (width * 0.02f * dir), lungTop + 32f, innerX, lungTop + 18f);
        path.Close();
        return path;
    }

    private static void DrawFissures(SKCanvas canvas, SKPaint paint, float cx, float top, float bottom, float width)
    {
        float upper = top + (bottom - top) * 0.40f;
        float mid = top + (bottom - top) * 0.56f;
        float lower = top + (bottom - top) * 0.67f;

        canvas.DrawLine(cx - width * 0.06f, upper, cx - width * 0.22f, mid, paint);
        canvas.DrawLine(cx + width * 0.05f, upper, cx + width * 0.23f, mid, paint);
        canvas.DrawLine(cx + width * 0.09f, lower - 8f, cx + width * 0.21f, mid + 26f, paint);
    }

    private static void DrawAirways(SKCanvas canvas, SKPaint tracheaPaint, SKPaint airwayPaint, SKPaint bronchiolePaint, float cx, float top, float bottom, float width)
    {
        float tracheaTop = top;
        float carinaY = top + (bottom - top) * 0.22f;

        canvas.DrawLine(cx, tracheaTop, cx, carinaY, tracheaPaint);
        canvas.DrawLine(cx, carinaY, cx - width * 0.12f, carinaY + width * 0.10f, airwayPaint);
        canvas.DrawLine(cx, carinaY, cx + width * 0.12f, carinaY + width * 0.10f, airwayPaint);

        DrawBranch(canvas, bronchiolePaint, cx - width * 0.12f, carinaY + width * 0.10f, -1, width);
        DrawBranch(canvas, bronchiolePaint, cx + width * 0.12f, carinaY + width * 0.10f, 1, width);
    }

    private static void DrawBranch(SKCanvas canvas, SKPaint paint, float startX, float startY, int dir, float width)
    {
        var trunkEnd = new SKPoint(startX + (width * 0.08f * dir), startY + width * 0.13f);
        canvas.DrawLine(startX, startY, trunkEnd.X, trunkEnd.Y, paint);
        canvas.DrawLine(trunkEnd.X, trunkEnd.Y, trunkEnd.X + (width * 0.10f * dir), trunkEnd.Y + width * 0.10f, paint);
        canvas.DrawLine(trunkEnd.X, trunkEnd.Y, trunkEnd.X + (width * 0.12f * dir), trunkEnd.Y + width * 0.02f, paint);
        canvas.DrawLine(trunkEnd.X, trunkEnd.Y, trunkEnd.X + (width * 0.06f * dir), trunkEnd.Y + width * 0.18f, paint);
        canvas.DrawLine(trunkEnd.X + (width * 0.10f * dir), trunkEnd.Y + width * 0.10f, trunkEnd.X + (width * 0.16f * dir), trunkEnd.Y + width * 0.16f, paint);
        canvas.DrawLine(trunkEnd.X + (width * 0.12f * dir), trunkEnd.Y + width * 0.02f, trunkEnd.X + (width * 0.18f * dir), trunkEnd.Y - width * 0.02f, paint);
        canvas.DrawLine(trunkEnd.X + (width * 0.06f * dir), trunkEnd.Y + width * 0.18f, trunkEnd.X + (width * 0.11f * dir), trunkEnd.Y + width * 0.24f, paint);
    }

    private static void DrawAlveoli(SKCanvas canvas, SKPaint paint, float cx, float top, float bottom, float width)
    {
        float[][] seeds =
        {
            new[] { cx - width * 0.18f, top + (bottom - top) * 0.40f },
            new[] { cx - width * 0.20f, top + (bottom - top) * 0.56f },
            new[] { cx - width * 0.11f, top + (bottom - top) * 0.67f },
            new[] { cx + width * 0.18f, top + (bottom - top) * 0.40f },
            new[] { cx + width * 0.20f, top + (bottom - top) * 0.56f },
            new[] { cx + width * 0.11f, top + (bottom - top) * 0.67f }
        };

        foreach (var seed in seeds)
        {
            for (int i = 0; i < 7; i++)
            {
                float dx = (i % 3 - 1) * width * 0.022f;
                float dy = (i / 3 - 1) * width * 0.022f;
                canvas.DrawCircle(seed[0] + dx, seed[1] + dy, width * 0.010f, paint);
            }
        }
    }

    private static SKColor Mix(SKColor start, SKColor end, float amount)
    {
        amount = Math.Clamp(amount, 0f, 1f);
        return new SKColor(
            (byte)(start.Red + ((end.Red - start.Red) * amount)),
            (byte)(start.Green + ((end.Green - start.Green) * amount)),
            (byte)(start.Blue + ((end.Blue - start.Blue) * amount)),
            (byte)(start.Alpha + ((end.Alpha - start.Alpha) * amount)));
    }
}
