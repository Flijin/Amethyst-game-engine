using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using Vec4 = System.Numerics.Vector4;

namespace Amethyst_game_engine.Core;

public readonly struct Color
{
    public static Color NoneColor => new();
    public static Color Red => new(255, 0, 0);
    public static Color Green => new(0, 255, 0);
    public static Color Blue => new(0, 0, 255);
    public static Color Cyan => new(0, 255, 255);
    public static Color Magenta => new(255, 0, 255);
    public static Color Yellow => new(255, 255, 0);
    public static Color Black => new(0, 0, 0);
    public static Color White => new(255, 255, 255);
    public static Color Gray => new(127, 127, 127);
    public static Color Violet => new(127, 0, 255);
    public static Color Orange => new(255, 127, 0);

    public int R { get; }
    public int G { get; }
    public int B { get; }
    public int A { get; }

    internal readonly bool isNoneColor;

    internal readonly float r;
    internal readonly float g;
    internal readonly float b;
    internal readonly float a;

    public Color() => isNoneColor = true;

    public Color(float r, float g, float b) : this(r, g, b, 1.0f) { }

    public Color(float r, float g, float b, float a)
    {
        var colorVec = new Vec4(r, g, b, a);

        if (Vec4.Min(colorVec, Vec4.Zero) != Vec4.Zero || Vec4.Max(colorVec, Vec4.One) != Vec4.One)
            throw new ArgumentException("The normalize color values must be in the range 0.0 - 1.0");

        R = (int)(r * 255);
        G = (int)(g * 255);
        B = (int)(b * 255);
        A = (int)(a * 255);

        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    public Color(int r, int g, int b) : this(r, g, b, 255) { }

    public Color(int r, int g, int b, int a)
    {
        if ((r | g | b | a) is < 0 or > 255)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Vector4 GetColorInVectorForm() => new(r, g, b, a);
}
