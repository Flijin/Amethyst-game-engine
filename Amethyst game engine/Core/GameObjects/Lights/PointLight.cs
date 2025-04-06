using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.GameObjects.Lights;

class PointLight(Color color, Vector3 direction, float attenuation, Vector3 position) : DirectionalLight(color, direction)
{
    public float Attenuation { get; set; } = attenuation;
    public Vector3 Position { get; set; } = position;
}
