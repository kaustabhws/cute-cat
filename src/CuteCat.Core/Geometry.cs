namespace CuteCat.Core;

public readonly record struct V2(double X, double Y)
{
    public double Length => Math.Sqrt(X * X + Y * Y);
    public static V2 operator +(V2 a, V2 b) => new(a.X + b.X, a.Y + b.Y);
    public static V2 operator -(V2 a, V2 b) => new(a.X - b.X, a.Y - b.Y);
    public static V2 operator *(V2 a, double b) => new(a.X * b, a.Y * b);
    public static V2 Lerp(V2 a, V2 b, double t) => a + (b - a) * t;
}

public readonly record struct Area(double Left, double Top, double Width, double Height)
{
    public double Right => Left + Width;
    public double Bottom => Top + Height;
    public V2 Center => new(Left + Width / 2, Top + Height / 2);
    public bool IsValid => Width > 0 && Height > 0 && double.IsFinite(Left + Top + Width + Height);
    public bool Contains(V2 p) => p.X >= Left && p.X <= Right && p.Y >= Top && p.Y <= Bottom;
    public V2 Clamp(V2 p, double xMargin = 0, double topMargin = 0, double bottomMargin = 0) => new(
        Math.Clamp(p.X, Left + Math.Min(xMargin, Width / 2), Right - Math.Min(xMargin, Width / 2)),
        Math.Clamp(p.Y, Top + Math.Min(topMargin, Height / 2), Bottom - Math.Min(bottomMargin, Height / 2)));
}

public static class Ease
{
    public static double Smooth(double t) { t = Math.Clamp(t, 0, 1); return t * t * t * (t * (t * 6 - 15) + 10); }
    public static double Mix(double a, double b, double t) => a + (b - a) * t;
    public static double Pulse(double t, double center, double radius) => Smooth(1 - Math.Abs(t - center) / radius);
}
