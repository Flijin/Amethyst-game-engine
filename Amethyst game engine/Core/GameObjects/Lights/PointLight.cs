using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Amethyst_game_engine.Core.GameObjects.Lights;

[StructLayout(LayoutKind.Explicit, Size = 48)]
internal struct PointLight
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
    private float _pad;
}
