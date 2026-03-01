using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Amethyst_game_engine.Core.Light;

internal struct DirectionalLightData
{
    public Vector3 direction;

    public Vector3 color;

    public float intensity;
}
