namespace Amethyst_game_engine.Core;

public readonly struct Color
{
    public int R { get; }
    public int G { get; }
    public int B { get; }
    public int A { get; }

    internal readonly float r;
    internal readonly float g;
    internal readonly float b;
    internal readonly float a;

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
