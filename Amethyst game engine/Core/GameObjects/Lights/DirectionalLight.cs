using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.GameObjects.Lights;

public class DirectionalLight
{
    internal Quaternion _direction;
    private Vector3 _directionVec;

    public Color Color { get; set; }
    public Vector3 Direction
    {
        get => _directionVec;

        set
        {
            _directionVec = value;
            _direction = new(value.X, value.Y, value.Z);
        }
    }

    public DirectionalLight(Color color, Vector3 direction)
    {
        Color = color;
        Direction = direction;
    }
}
