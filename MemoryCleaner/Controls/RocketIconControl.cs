using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace MemoryCleaner.Controls;

public sealed class RocketIconControl : Control
{
    public static readonly StyledProperty<double> FlameProgressProperty =
        AvaloniaProperty.Register<RocketIconControl, double>(nameof(FlameProgress));

    public double FlameProgress
    {
        get => GetValue(FlameProgressProperty);
        set => SetValue(FlameProgressProperty, value);
    }

    static RocketIconControl()
    {
        AffectsRender<RocketIconControl>(FlameProgressProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var size = Math.Min(Bounds.Width, Bounds.Height);
        if (size <= 0)
        {
            return;
        }

        var scale = size / 24;
        var offsetX = (Bounds.Width - size) / 2;
        var offsetY = (Bounds.Height - size) / 2;

        Point P(double x, double y) => new(offsetX + x * scale, offsetY + y * scale);

        var navy = new SolidColorBrush(Color.FromRgb(21, 54, 108));
        var blue = new SolidColorBrush(Color.FromRgb(55, 128, 233));
        var white = new SolidColorBrush(Color.FromRgb(249, 252, 255));
        var flame = Math.Clamp(FlameProgress, 0, 1);

        if (flame > 0)
        {
            DrawFlame(context, P, scale, flame);
        }

        var leftFin = new StreamGeometry();
        using (var fin = leftFin.Open())
        {
            fin.BeginFigure(P(8.4, 15.2), true);
            fin.LineTo(P(4.9, 19.4));
            fin.LineTo(P(9.5, 18.2));
            fin.EndFigure(true);
        }
        context.DrawGeometry(blue, null, leftFin);

        var rightFin = new StreamGeometry();
        using (var fin = rightFin.Open())
        {
            fin.BeginFigure(P(15.6, 15.2), true);
            fin.LineTo(P(19.1, 19.4));
            fin.LineTo(P(14.5, 18.2));
            fin.EndFigure(true);
        }
        context.DrawGeometry(blue, null, rightFin);

        var body = new StreamGeometry();
        using (var rocket = body.Open())
        {
            rocket.BeginFigure(P(12, 2.8), true);
            rocket.CubicBezierTo(P(16.8, 6.5), P(16.6, 13.2), P(13.7, 18.6));
            rocket.LineTo(P(10.3, 18.6));
            rocket.CubicBezierTo(P(7.4, 13.2), P(7.2, 6.5), P(12, 2.8));
            rocket.EndFigure(true);
        }
        context.DrawGeometry(white, new Pen(navy, 1.4 * scale), body);

        context.DrawEllipse(
            new SolidColorBrush(Color.FromRgb(112, 207, 255)),
            new Pen(navy, 1 * scale),
            P(12, 10),
            2.2 * scale,
            2.2 * scale);

        context.DrawLine(new Pen(navy, 1.2 * scale), P(10.4, 18.8), P(9.1, 21));
        context.DrawLine(new Pen(navy, 1.2 * scale), P(13.6, 18.8), P(14.9, 21));
    }

    private static void DrawFlame(DrawingContext context, Func<double, double, Point> p, double scale, double progress)
    {
        var outer = new StreamGeometry();
        using (var flame = outer.Open())
        {
            flame.BeginFigure(p(9.2, 18.6), true);
            flame.CubicBezierTo(p(9.4, 21.2), p(11.2, 21.9 + 1.7 * progress), p(12, 23));
            flame.CubicBezierTo(p(12.8, 21.9 + 1.7 * progress), p(14.6, 21.2), p(14.8, 18.6));
            flame.EndFigure(true);
        }
        context.DrawGeometry(new SolidColorBrush(Color.FromArgb((byte)(190 * progress), 255, 114, 39)), null, outer);

        var inner = new StreamGeometry();
        using (var flame = inner.Open())
        {
            flame.BeginFigure(p(10.7, 18.9), true);
            flame.CubicBezierTo(p(11, 20.6), p(11.7, 21.4 + progress), p(12, 22));
            flame.CubicBezierTo(p(12.3, 21.4 + progress), p(13, 20.6), p(13.3, 18.9));
            flame.EndFigure(true);
        }
        context.DrawGeometry(new SolidColorBrush(Color.FromArgb((byte)(220 * progress), 255, 221, 94)), null, inner);

        var sparkBrush = new SolidColorBrush(Color.FromArgb((byte)(150 * progress), 255, 198, 78));
        context.DrawEllipse(sparkBrush, null, p(8.8, 22.2), 0.7 * scale * progress, 0.7 * scale * progress);
        context.DrawEllipse(sparkBrush, null, p(15.2, 22.1), 0.55 * scale * progress, 0.55 * scale * progress);
        context.DrawEllipse(new SolidColorBrush(Color.FromArgb((byte)(110 * progress), 255, 116, 39)), null, p(12, 23.2), 0.6 * scale * progress, 0.6 * scale * progress);
    }
}
