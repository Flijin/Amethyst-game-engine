using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Amethyst_game_engine.Core.GameObjects.Lights;

[StructLayout(LayoutKind.Explicit, Size = 80)]
internal struct Spotlight
{
    [FieldOffset(0)]
    public Vector3 position;

    [FieldOffset(16)]
    public Vector3 direction;

    [FieldOffset(32)]
    public Vector3 color;

    [FieldOffset(44)]
    public float intensity;

    [FieldOffset(48)]
    public float innerCutOff;

    [FieldOffset(52)]
    public float outerCutOff;

    [FieldOffset(56)]
    public float constant;

    [FieldOffset(60)]
    public float linear;

    [FieldOffset(64)]
    public float quadratic;
}
