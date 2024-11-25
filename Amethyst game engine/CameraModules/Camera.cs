using Amethyst_game_engine.Core;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Amethyst_game_engine.CameraModules;

public class Camera : IDisposable
{
    private bool _disposed = false;

    private float _yaw = -float.Pi / 2;
    private float _orthographicBorder;
    private float _fov;
    private float _pitch;

    private readonly float _aspectRatio;
    private readonly CameraTypes _type;

    private readonly unsafe float* _viewMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);
    private readonly unsafe float* _projectionMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);

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

    internal unsafe float* ProjectionMatrix
    {
        get
        {
            if (_type == CameraTypes.Perspective)
            {
                var scaleY = 1 / MathF.Tan(_fov / 2);
                var scaleX = scaleY / _aspectRatio;

                var item1 = -((Far + Near) / (Far - Near));
                var item2 = -(2 * Far * Near / (Far - Near));

                *_projectionMatrix = scaleX;
                *(_projectionMatrix + 5) = scaleY;
                *(_projectionMatrix + 10) = item1;
                *(_projectionMatrix + 11) = item2;
                *(_projectionMatrix + 14) = -1;
            }
            else
            {
                *_projectionMatrix = 2 / (Right - Left);
                *(_projectionMatrix + 3) = -((Right + Left) / (Right - Left));
                *(_projectionMatrix + 5) = 2 / (Top - Bottom);
                *(_projectionMatrix + 7) = -((Top + Bottom) / (Top - Bottom));
                *(_projectionMatrix + 10) = -(2 / (Far - Near));
                *(_projectionMatrix + 11) = -((Far + Near) / (Far - Near));
                *(_projectionMatrix + 15) = 1;
            }

            return _projectionMatrix;
        }
    }

    internal unsafe float* ViewMatrix
    {
        get
        {
            Vector3 row0 = new(RightVector.X, RightVector.Y, RightVector.Z);
            Vector3 row1 = new(Up.X, Up.Y, Up.Z);
            Vector3 row2 = new(-Front.X, -Front.Y, -Front.Z);

            float* matrixA = stackalloc float[16]
            {
                RightVector.X, RightVector.Y, RightVector.Z, 0,
                Up.X,          Up.Y,          Up.Z,          0,
               -Front.X,      -Front.Y,      -Front.Z,       0,
                0,             0,             0,             1
            };

            float* matrixB = stackalloc float[16]
            {
                 1, 0, 0, -Position.X,
                 0, 1, 0, -Position.Y,
                 0, 0, 1, -Position.Z,
                 0, 0, 0,  1
            };

            Mathematics.MultiplyMatrices4(matrixA, matrixB, _viewMatrix);

            return _viewMatrix;
        }
    }

    public Camera(CameraTypes type, Vector3 position, float aspectRatio)
    {
        unsafe
        {
            Unsafe.InitBlock(_viewMatrix, 0, Mathematics.MATRIX_SIZE);
            Unsafe.InitBlock(_projectionMatrix, 0, Mathematics.MATRIX_SIZE);
        }

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

    ~Camera()
    {
        if (_disposed == false)
            SystemSettings.PrintErrorMessage("Warning. The Dispose method was not called, RAM memory leak");
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

    public void Dispose()
    {
        if (_disposed == false)
        {
            unsafe
            {
                Marshal.FreeHGlobal((nint)_viewMatrix);
                Marshal.FreeHGlobal((nint)_projectionMatrix);
            }
            
            GC.SuppressFinalize(this);

            _disposed = true;
        }
    }
}
