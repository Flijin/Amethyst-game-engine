namespace Amethyst_game_engine.Core;

public readonly struct Color
{
    public static Color Red => new(255, 0, 0);
    public static Color Green => new(0, 255, 0);
    public static Color Blue => new(0, 0, 255);
    public static Color Cyan => new(0, 255, 255);
    public static Color Magenta => new(255, 0, 255);
    public static Color Yellow => new(0, 0, 255);
    public static Color Black => new(0, 0, 0);
    public static Color White => new(255, 255, 255);
    public static Color Gray => new(127, 127, 127);

    public int R { get; }
    public int G { get; }
    public int B { get; }
    public int A { get; }

    internal readonly float r;
    internal readonly float g;
    internal readonly float b;
    internal readonly float a;

    internal readonly bool isNoneColor = false;

    public Color() => isNoneColor = true;

    public Color(int r, int g, int b) : this(r, g, b, 255) { }

    public Color(int r, int g, int b, int a)
    {
        if (r is < 0 or > 255 || g is < 0 or > 255 || b is < 0 or > 255 || a is < 0 or > 255)
            throw new ArgumentException("The color values must be in the range 0 - 255");

        R = r;
        G = g;
        B = b;
        A = a;

        this.r = r / 255f;
        this.g = g / 255f;
        this.b = b / 255f;
        this.a = a / 255f;
    }
}
