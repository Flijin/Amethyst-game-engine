using Amethyst_game_engine.Core.Light;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.New_classes;

public class Spotlight
{
    public string? Tag { get; set; }

    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Direction { get; set; } = Vector3.Zero;
    public Vector3 Color { get; set; } = Core.Color.White.ConvertColorToVector3();
    public float Intensity { get; set; } = 1.0f;
    public float InnerCutOff { get; set; } = 0.98f;
    public float OuterCutOff { get; set; } = 0.95f;
    public float Constant { get; set; } = 1.0f;
    public float Linear { get; set; } = 0.09f;
    public float Quadratic { get; set; } = 0.032f;


    internal SpotlightData GetLightData()
    {
        return new SpotlightData()
        {
            position = Position,
            direction = Direction,
            color = Color,
            intensity = Intensity,
            innerCutOff = InnerCutOff,
            outerCutOff = OuterCutOff,
            constant = Constant,
            linear = Linear,
            quadratic = Quadratic,
            radius = Mathematics.CalculateLightRadius(Constant, Linear, Quadratic),
            isActive = 1
        };
    }
}
