using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.Light;

internal struct SpotlightData
{
    public Vector3 position;

    public Vector3 direction;

    public Vector3 color;

    public float intensity;

    public float innerCutOff;

    public float outerCutOff;

    public float constant;

    public float linear;

    public float quadratic;

    public float radius;
}
