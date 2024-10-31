using Amethyst_game_engine.Core;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.CameraModules;

public class Camera
{
    private readonly float _aspectRatio;
    private float _yaw = -float.Pi / 2;
    private float _orthographicBorder;
    private float _fov;
    private float _pitch;
    private readonly CameraTypes _type;

    public float Near { get; set; }
    public float Far { get; set; }
    public Vector3 Position { get; set; }

    internal Vector3 Up { get; private set; } = Vector3.UnitY;
    internal Vector3 RightVector { get; private set; } = Vector3.UnitX;
    internal Vector3 Front { get; private set; } = -Vector3.UnitZ;

    public float Right { get; set; }
    public float Left { get; set; }
    public float Bottom { get; set; }
    public float Top { get; set; }

    public float Fov
    {
        get => Mathematics.RadiansToDegrees(_fov);
        set => _fov = Mathematics.DegreesToRadians(Mathematics.Clamp(value, -180f, 180f));
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
            _pitch = Mathematics.DegreesToRadians(Mathematics.Clamp(value, -89.9f, 89.9f));
            CalculateVectors();
        }
    }

    public float OrthographicBorders
    {
        get => _orthographicBorder;

        set
        {
            _orthographicBorder = value;

            Left = -value * _aspectRatio;
            Right = value * _aspectRatio;
            Top = -value / _aspectRatio;
            Bottom = value / _aspectRatio;
        }
    }

    internal float[,] ProjectionMatrix
    {
        get
        {
            if (_type == CameraTypes.Perspective)
            {
                var scaleY = 1 / MathF.Tan(_fov / 2);
                var scaleX = scaleY / _aspectRatio;

                var item1 = -((Far + Near) / (Far - Near));
                var item2 = -(2 * Far * Near / (Far - Near));

                return new float[,]
                {
                    { scaleX, 0,   0,  0 },
                    { 0, scaleY,   0,  0 },
                    { 0, 0, item1, item2 },
                    { 0, 0,    -1,     0 },
                };
            }
            else
            {
                return new float[,]
                {
                    { 2 / (Right - Left), 0, 0, -((Right + Left) / (Right - Left))},
                    { 0, 2 / (Top - Bottom), 0, -((Top + Bottom) / (Top - Bottom))},
                    { 0, 0, -(2 / (Far - Near)),    -((Far + Near) / (Far - Near))},
                    { 0,                0,                   0,                  1},
                };
            }
        }
    }

    internal float[,] ViewMatrix
    {
        get
        {
            float[,] matrixA =
            {
                {  RightVector.X, RightVector.Y, RightVector.Z, 0 },
                {  Up.X,          Up.Y,          Up.Z,          0 },
                { -Front.X,      -Front.Y,      -Front.Z,       0 },
                {  0,             0,             0,             1 },
            };

            float[,] matrixB =
            {
                { 1, 0, 0, -Position.X },
                { 0, 1, 0, -Position.Y },
                { 0, 0, 1, -Position.Z },
                { 0, 0, 0,  1          },
            };

            return Mathematics.MultiplyMatrices(matrixA, matrixB);
        }
    }

    public Camera(CameraTypes type, Vector3 position, float aspectRatio)
    {
        _type = type;
        _aspectRatio = aspectRatio;
        Position = position;
        Near = 1f;
        Far = 5000f;

        if (type == CameraTypes.Orthographic)
            OrthographicBorders = 500f;
        else
            _fov = 0.7854f;
    }

    private void CalculateVectors()
    {
        var x = MathF.Cos(_pitch) * MathF.Cos(_yaw);
        var y = MathF.Sin(_pitch);
        var z = MathF.Cos(_pitch) * MathF.Sin(_yaw);
        Front = Vector3.Normalize(new Vector3(x, y, z));

        RightVector = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Cross(RightVector, Front);
    }
}
