using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.Light;

[StructLayout(LayoutKind.Explicit, Size = 64)]
internal struct PointLightData
{
    [FieldOffset(0)]
    public Vector3 position;

    [FieldOffset(16)]
    public Vector3 color;

    [FieldOffset(28)]
    public float intensity;

    [FieldOffset(32)]
    public float constant;

    [FieldOffset(36)]
    public float linear;

    [FieldOffset(40)]
    public float quadratic;

    [FieldOffset(44)]
    public float radius;

    [FieldOffset(48)]
    public int isActive;
}
