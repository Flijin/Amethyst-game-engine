using OpenTK.Mathematics;

namespace Game_Engine.Core;

internal class Camera
{
    private readonly float _aspectRatio;
    private float _yaw = -float.Pi / 2;
    private float _pitch;
    private float _fovy;

    public Camera(Vector3 position, Vector2i windowSize, float fovy)
    {
        Position = position;
        Fovy = Mathematics.DegreesToRadians(fovy);
        _aspectRatio = (float)windowSize.X / windowSize.Y;
    }

    public float NearDistance { get; set; } = 1f;
    public float FarDistance { get; set; } = 5000f;
    public Vector3 Up { get; private set; } = Vector3.UnitY;
    public Vector3 Right { get; private set; } = Vector3.UnitX;
    public Vector3 Front { get; private set; } = -Vector3.UnitZ;
    public Vector3 Position { get; set; }
    public Matrix4 ViewMatrix => Matrix4.LookAt(Position, Position + Front, Up);
    public Matrix4 ProjectionMatrix => Matrix4.CreatePerspectiveFieldOfView(_fovy, _aspectRatio, NearDistance, FarDistance);

    public float Fovy
    {
        get => Mathematics.RadiansToDegrees(_fovy);
        set => _fovy = Mathematics.DegreesToRadians(Mathematics.Clamp(value, -89f, 89f));
    }

    public float Yaw
    {
        get => Mathematics.RadiansToDegrees(_yaw);

        set
        {
            _yaw = Mathematics.DegreesToRadians(value);
            CalculateVectors();
        }
    }

    public float Pitch
    {
        get => Mathematics.RadiansToDegrees(_pitch);

        set
        {
            _pitch = Mathematics.DegreesToRadians(Mathematics.Clamp(value, -89f, 89f));
            CalculateVectors();
        }
    }
    
    private void CalculateVectors()
    {
        var x = MathF.Cos(Pitch) * MathF.Cos(Yaw);
        var y = MathF.Sin(Pitch);
        var z = MathF.Cos(Pitch) * MathF.Sin(Yaw);
        Front = Vector3.Normalize(new Vector3(x, y, z));

        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Cross(Right, Front);
    }
}
