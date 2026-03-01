using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.Light;

internal struct PointLightData
{
    public Vector3 position;

    public Vector3 color;

    public float intensity;

    public float constant;

    public float linear;

    public float quadratic;
    public float radius;
}
