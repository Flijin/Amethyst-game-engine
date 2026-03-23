using Amethyst_game_engine.Core.Utilities;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.Light;

public class Spotlight
{
    private float _yaw;
    private float _pitch;
    private Vector3 _direction = new(1.0f, 0.0f, 0.0f);

    private float _innerCutOff = 30.0f;
    private float _outerCutOff = 45.0f;

    private float _intensity = 1.0f;

    private float _constant = 1.0f;
    private float _linear = 0.09f;
    private float _quadratic = 0.032f;

    public string? Tag { get; set; }
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Color { get; set; } = Utilities.Color.White.ConvertColorToVector3();

    public float Yaw
    {
        get => _yaw;

        set
        {
            _yaw = Mathematics.Clamp(value, 0.0f, 360.0f);
            _direction = Mathematics.GetDirectionFromEuler(_yaw, _pitch);
        }
    }

    public float Pitch
    {
        get => _pitch;

        set
        {
            _pitch = Mathematics.Clamp(value, -90.0f, 90.0f);
            _direction = Mathematics.GetDirectionFromEuler(_yaw, _pitch);
        }
    }

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

    public float InnerCutOff
    {
        get => _innerCutOff;
        set => _innerCutOff = Mathematics.Clamp(value, 0.0f, 90.0f);
    }

    public float OuterCutOff
    {
        get => _outerCutOff;
        set => _outerCutOff = Mathematics.Clamp(value, 0.0f, 90.0f);
    }

    internal SpotlightData GetLightData()
    {
        return new SpotlightData()
        {
            position = Position,
            direction = _direction,
            color = Color,
            intensity = _intensity,
            innerCutOff = MathF.Cos(_innerCutOff * MathF.PI / 180.0f),
            outerCutOff = MathF.Cos(_outerCutOff * MathF.PI / 180.0f),
            constant = _constant,
            linear = _linear,
            quadratic = _quadratic,
            radius = Mathematics.CalculateLightRadius(_constant, _linear, _quadratic),
            isActive = 1
        };
    }
}
