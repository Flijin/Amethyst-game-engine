using OpenTK.Mathematics;

namespace Game_Engine.Core.CameraModules;

internal class Camera
{
    private readonly float _aspectRatio;
    private float _yaw = -float.Pi / 2;
    private float _pitch;
    private float _fov;

    public float NearDistance { get; set; } = 1f;
    public float FarDistance { get; set; } = 5000f;
    public Vector3 Up { get; private set; } = Vector3.UnitY;
    public Vector3 Right { get; private set; } = Vector3.UnitX;
    public Vector3 Front { get; private set; } = -Vector3.UnitZ;
    public Vector3 Position { get; set; }

    public float[,] ProjectionMatrix
    {
        get
        {
            var scaleY = 1 / MathF.Tan(_fov / 2);
            var scaleX = scaleY / _aspectRatio;

            var item1 = -((FarDistance + NearDistance) / (FarDistance - NearDistance));
            var item2 = -(2 * FarDistance * NearDistance / (FarDistance - NearDistance));

            return new float[,]
            {
                { scaleX, 0,   0,  0 },
                { 0, scaleY,   0,  0 },
                { 0, 0, item1, item2 },
                { 0, 0,    -1,     0 },
            };
        }
    }

    public float[,] ViewMatrix
    {
        get
        {
            float[,] matrixA =
            {
                {  Right.X,  Right.Y,  Right.Z, 0 },
                {  Up.X,     Up.Y,     Up.Z,    0 },
                { -Front.X, -Front.Y, -Front.Z, 0 },
                {  0,        0,        0,       1 },
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

    public float Fov
    {
        get => Mathematics.RadiansToDegrees(_fov);
        set => _fov = Mathematics.DegreesToRadians(Mathematics.Clamp(value, -89f, 89f));
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

    public Camera(Vector3 position, float aspectRatio, float fov)
    {
        Position = position;
        Fov = fov;
        _aspectRatio = aspectRatio;
    }

    private void CalculateVectors()
    {
        var x = MathF.Cos(_pitch) * MathF.Cos(_yaw);
        var y = MathF.Sin(_pitch);
        var z = MathF.Cos(_pitch) * MathF.Sin(_yaw);
        Front = Vector3.Normalize(new Vector3(x, y, z));

        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Cross(Right, Front);
    }
}
