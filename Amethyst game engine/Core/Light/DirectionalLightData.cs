using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.Light;

[StructLayout(LayoutKind.Explicit, Size = 48)]
internal struct DirectionalLightData
{
    [FieldOffset(0)]
    public Vector3 direction;

    [FieldOffset(16)]
    public Vector3 color;

    [FieldOffset(28)]
    public float intensity;

    [FieldOffset(32)]
    public int isActive;
}
