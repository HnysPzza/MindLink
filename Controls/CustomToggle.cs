using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace M1ndLink.Controls;

public class CustomToggle : SKCanvasView
{
    public static readonly BindableProperty IsToggledProperty =
        BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(CustomToggle),
            false, BindingMode.TwoWay, propertyChanged: (b, _, _) => ((CustomToggle)b).Animate());

    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }

    public event EventHandler<bool>? Toggled;
    private float _thumb = 0f;

    public CustomToggle()
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { IsToggled = !IsToggled; Toggled?.Invoke(this, IsToggled); };
        GestureRecognizers.Add(tap);
    }

    private void Animate()
    {
        float target = IsToggled ? 1f : 0f;
        new Animation(v => { _thumb = (float)v; InvalidateSurface(); }, _thumb, target, Easing.SpringOut)
            .Commit(this, "Toggle", 16, 350);
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var (canvas, info) = (e.Surface.Canvas, e.Info);
        canvas.Clear();
        float w = info.Width, h = info.Height, rad = h / 2f;

        // Interpolate track: off=#DBEAFE -> on=#2563EB
        byte r = (byte)(0xDB + _thumb * (0x25 - 0xDB));
        byte g = (byte)(0xEA + _thumb * (0x63 - 0xEA));
        byte b = (byte)(0xFE + _thumb * (0xEB - 0xFE));

        using var track = new SKPaint { IsAntialias = true, Color = new SKColor(r, g, b) };
        canvas.DrawRoundRect(0, 0, w, h, rad, rad, track);

        float tx = rad + _thumb * (w - h);
        using var thumb = new SKPaint { IsAntialias = true, Color = SKColors.White };
        using var shadow = new SKPaint
        {
            IsAntialias = true, Color = new SKColor(0x25, 0x63, 0xEB, 55),
            ImageFilter  = SKImageFilter.CreateDropShadow(0, 2, 3, 3, new SKColor(0x25, 0x63, 0xEB, 70))
        };
        canvas.DrawCircle(tx, h / 2f, rad * 0.78f, shadow);
        canvas.DrawCircle(tx, h / 2f, rad * 0.78f, thumb);
    }
}
