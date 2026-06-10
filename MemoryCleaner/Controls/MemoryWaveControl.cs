using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace MemoryCleaner.Controls;

public sealed class MemoryWaveControl : Control
{
    public static readonly StyledProperty<double> PercentProperty =
        AvaloniaProperty.Register<MemoryWaveControl, double>(nameof(Percent));

    public static readonly StyledProperty<double> PhaseProperty =
        AvaloniaProperty.Register<MemoryWaveControl, double>(nameof(Phase));

    public double Percent
    {
        get => GetValue(PercentProperty);
        set => SetValue(PercentProperty, value);
    }

    public double Phase
    {
        get => GetValue(PhaseProperty);
        set => SetValue(PhaseProperty, value);
    }

    static MemoryWaveControl()
    {
        AffectsRender<MemoryWaveControl>(PercentProperty, PhaseProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var size = Math.Min(Bounds.Width, Bounds.Height);
        if (size <= 0)
        {
            return;
        }

        var rect = new Rect((Bounds.Width - size) / 2, (Bounds.Height - size) / 2, size, size);
        var center = rect.Center;
        var radius = size / 2;
        var percent = Math.Clamp(Percent, 0, 100);
        var color = GetLevelColor(percent);
        var darkColor = Darken(color);

        using (context.PushGeometryClip(new EllipseGeometry(rect)))
        {
            context.DrawEllipse(new SolidColorBrush(darkColor), null, center, radius, radius);
            DrawWave(context, rect, percent, color, Phase, opacity: 0.88);
            DrawWave(context, rect, percent, Lighten(color), Phase + Math.PI, opacity: 0.42);
        }

        context.DrawEllipse(
            null,
            new Pen(new SolidColorBrush(Color.FromArgb(70, 255, 255, 255)), 1),
            center,
            radius - 0.5,
            radius - 0.5);
    }

    private static void DrawWave(
        DrawingContext context,
        Rect rect,
        double percent,
        Color color,
        double phase,
        double opacity)
    {
        var waterTop = rect.Bottom - rect.Height * percent / 100;
        var amplitude = Math.Max(3, rect.Height * 0.06);
        var wavelength = rect.Width * 0.9;

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(new Point(rect.Left, rect.Bottom), isFilled: true);
            ctx.LineTo(new Point(rect.Left, waterTop));

            const int segments = 28;
            for (var i = 0; i <= segments; i++)
            {
                var x = rect.Left + rect.Width * i / segments;
                var y = waterTop + Math.Sin((x / wavelength * Math.PI * 2) + phase) * amplitude;
                ctx.LineTo(new Point(x, y));
            }

            ctx.LineTo(new Point(rect.Right, rect.Bottom));
            ctx.EndFigure(isClosed: true);
        }

        context.DrawGeometry(new SolidColorBrush(color, opacity), null, geometry);
    }

    private static Color GetLevelColor(double percent)
    {
        if (percent < 70)
        {
            return Color.FromRgb(34, 197, 94);
        }

        if (percent < 90)
        {
            return Color.FromRgb(255, 111, 44);
        }

        return Color.FromRgb(239, 68, 68);
    }

    private static Color Darken(Color color)
    {
        return Color.FromRgb(
            (byte)(color.R * 0.32),
            (byte)(color.G * 0.32),
            (byte)(color.B * 0.36));
    }

    private static Color Lighten(Color color)
    {
        return Color.FromRgb(
            (byte)Math.Min(255, color.R + 42),
            (byte)Math.Min(255, color.G + 42),
            (byte)Math.Min(255, color.B + 42));
    }
}
