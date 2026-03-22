using Amethyst_game_engine.Core.Light;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.New_classes;

public class PointLight
{
    public string? Tag { get; set; }

    public Vector3 Position { get; set; } = new();
    public Vector3 Color { get; set; } = Core.Color.White.ConvertColorToVector3();
    public float Intensity { get; set; } = 1.0f;
    public float Constant { get; set; } = 1.0f;
    public float Linear { get; set; } = 0.09f;
    public float Quadratic { get; set; } = 0.032f;

    internal PointLightData GetLightData()
    {
        return new PointLightData()
        {
            position = Position,
            color = Color,
            intensity = Intensity,
            constant = Constant,
            linear = Linear,
            quadratic = Quadratic,
            radius = Mathematics.CalculateLightRadius(Constant, Linear, Quadratic),
            isActive = 1
        };
    }
}
