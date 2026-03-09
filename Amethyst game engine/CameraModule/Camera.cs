using Amethyst_game_engine.Core;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Amethyst_game_engine.CameraModule;

public sealed class Camera : IDisposable
{
    private float _aspectRatio;
    private float _yaw = -float.Pi / 2;
    private float _orthographicBorder;
    private float _fov;
    private float _pitch;

    private readonly CameraTypes _type;

    private readonly unsafe float* _viewMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);
    private readonly unsafe float* _projectionMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);

    public string? Tag { get; set; }
    public float Near { get; set; }
    public float Far { get; set; }
    public Vector3 Position { get; set; }

    public Vector3 Up { get; private set; } = Vector3.UnitY;
    public Vector3 RightVector { get; private set; } = Vector3.UnitX;
    public Vector3 Front { get; private set; } = -Vector3.UnitZ;

    public float RightSide { get; set; }
    public float LeftSide { get; set; }
    public float BottomSide { get; set; }
    public float TopSide { get; set; }

    public float AspectRatio
    {
        get => _aspectRatio;
        set => _aspectRatio = value;
    }

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

            LeftSide = -value * _aspectRatio;
            RightSide = value * _aspectRatio;
            TopSide = value / _aspectRatio;
            BottomSide = -value / _aspectRatio;
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
                *_projectionMatrix = 2 / (RightSide - LeftSide);
                *(_projectionMatrix + 3) = -((RightSide + LeftSide) / (RightSide - LeftSide));
                *(_projectionMatrix + 5) = 2 / (TopSide - BottomSide);
                *(_projectionMatrix + 7) = -((TopSide + BottomSide) / (TopSide - BottomSide));
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


    private void CalculateVectors()
    {
        var x = MathF.Cos(_pitch) * MathF.Cos(_yaw);
        var y = MathF.Sin(_pitch);
        var z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

        Front = Vector3.Normalize(new Vector3(x, y, z));
        RightVector = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Cross(RightVector, Front);
    }

    internal void Cleanup()
    {
        unsafe
        {
            Marshal.FreeHGlobal((nint)_viewMatrix);
            Marshal.FreeHGlobal((nint)_projectionMatrix);
        }
    }

    void IDisposable.Dispose()
    {
        Cleanup();
    }
}
