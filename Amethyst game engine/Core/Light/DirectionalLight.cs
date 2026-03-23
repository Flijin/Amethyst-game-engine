using Amethyst_game_engine.Core.Utilities;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.Light;

public class DirectionalLight
{
    private float _yaw = 0.0f;
    private float _pitch = 0.0f;
    private Vector3 _direction = new(1.0f, 0.0f, 0.0f);

    private float _intensity = 1.0f;

    public string? Tag { get; set; }
    public Color Color { get; set; } = Color.White;

    public float Intensity
    {
        get => _intensity;
        set => _intensity = MathF.Max(value, 0.0f);
    }

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

    internal DirectionalLightData GetLightData()
    {
        return new DirectionalLightData()
        {
            direction = _direction,
            color = Color.ConvertColorToVector3(),
            intensity = _intensity,
            isActive = 1
        };
    }
}
