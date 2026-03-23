using Amethyst_game_engine.Core.Utilities;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.Light;

public class PointLight
{
    private float _intensity = 1.0f;

    private float _constant = 1.0f;
    private float _linear = 0.09f;
    private float _quadratic = 0.032f;

    public string? Tag { get; set; }
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Color Color { get; set; } = Color.White;

    public float Intensity
    {
        get => _intensity;
        set => _intensity = MathF.Max(value, 0.0f);
    }

    public float Constant
    {
        get => _constant;
        set => _constant = MathF.Max(value, 0.0f);
    }

    public float Linear
    {
        get => _linear;
        set => _linear = MathF.Max(value, 0.0f);
    }

    public float Quadratic
    {
        get => _quadratic;
        set => _quadratic = MathF.Max(value, 0.0f);
    }

    internal PointLightData GetLightData()
    {
        return new PointLightData()
        {
            position = Position,
            color = Color.ConvertColorToVector3(),
            intensity = _intensity,
            constant = _constant,
            linear = _linear,
            quadratic = _quadratic,
            radius = Mathematics.CalculateLightRadius(_constant, _linear, _quadratic),
            isActive = 1
        };
    }
}
