using Amethyst_game_engine.Core.Light;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.New_classes;

public class PointLight(string tag)
{
    public string Tag { get; } = tag;

    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Color { get; set; } = Core.Color.White.ConvertColorToVector3();
    public float Intensity { get; set; } = 1.0f;
    public float Constant { get; set; } = 1.0f;
    public float Linear { get; set; } = 0.09f;
    public float Quadratic { get; set; } = 0.032f;
    public float Radius { get; set; }

    public PointLight() : this(string.Empty) {    }

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
            radius = Radius,
        };
    }

    internal void CalculateRadius(float threshold = 0.001f)
    {
        float target = 1f / threshold;

        float a = Quadratic;
        float b = Linear;
        float c = Constant - target;

        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
            Radius = float.PositiveInfinity;

        float sqrtDisc = MathF.Sqrt(discriminant);
        float d1 = (-b + sqrtDisc) / (2 * a);
        float d2 = (-b - sqrtDisc) / (2 * a);

        float radius = MathF.Max(d1, d2);
        Radius =  radius > 0 ? radius : float.PositiveInfinity;
    }
}
