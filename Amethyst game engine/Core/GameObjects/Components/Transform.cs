using System.Runtime.InteropServices;
using Amethyst_game_engine.Core.Utilities;
using OpenTK.Mathematics;
using Quaternion = Amethyst_game_engine.Core.Utilities.Quaternion;

namespace Amethyst_game_engine.Core.GameObjects.Components;

public sealed class Transform : IDisposable
{
    private Quaternion _rotationQuaternion;
    private bool _updateResultMatrix;

    private Vector3 _position;
    private Vector3 _rotation;
    private Vector3 _scale;

    private readonly BoundingBox _localBox;

    private readonly unsafe float* _positionMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);
    private readonly unsafe float* _rotationMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);
    private readonly unsafe float* _scaleMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);
    private readonly unsafe float* _resultMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);

    public unsafe BoundingBox Box
    {
        get => BoundingBox.TransformBox(_localBox, ModelMatrix);
    }

    public unsafe float* ModelMatrix
    {
        get
        {
            if (_updateResultMatrix)
            {
                float* temp = stackalloc float[16];
                Mathematics.MultiplyMatrices4(_positionMatrix, _rotationMatrix, temp);
                Mathematics.MultiplyMatrices4(temp, _scaleMatrix, _resultMatrix);

                _updateResultMatrix = false;
            }

            return _resultMatrix;
        }
    }

    public unsafe Vector3 Position
    {
        get => _position;

        set
        {
            _position = value;
            Mathematics.CreateTranslationMatrix4(value.X, value.Y, value.Z, _positionMatrix);
            _updateResultMatrix = true;
        }
    }

    public Vector3 Rotation
    {
        get => _rotation;

        set
        {
            _rotation = value;
            _rotationQuaternion = new(value.X, value.Y, value.Z);

            unsafe { _rotationQuaternion.GetRotationMatrix(_rotationMatrix); }

            _updateResultMatrix = true;
        }
    }

    public Vector3 Scale
    {
        get => _scale;

        set
        {
            _scale = value;

            unsafe { Mathematics.CreateScaleMatrix4(value.X, value.Y, value.Z, _scaleMatrix); }

            _updateResultMatrix = true;
        }
    }

    internal unsafe Transform(BoundingBox box)
    {
        _localBox = box;

        Buffer.MemoryCopy(Mathematics.IDENTITY_MATRIX, _positionMatrix, Mathematics.MATRIX_SIZE, Mathematics.MATRIX_SIZE);
        Buffer.MemoryCopy(Mathematics.IDENTITY_MATRIX, _rotationMatrix, Mathematics.MATRIX_SIZE, Mathematics.MATRIX_SIZE);
        Buffer.MemoryCopy(Mathematics.IDENTITY_MATRIX, _scaleMatrix, Mathematics.MATRIX_SIZE, Mathematics.MATRIX_SIZE);

        _updateResultMatrix = false;
    }

    public unsafe void Dispose()
    {
        Marshal.FreeHGlobal((nint)_positionMatrix);
        Marshal.FreeHGlobal((nint)_rotationMatrix);
        Marshal.FreeHGlobal((nint)_scaleMatrix);
        Marshal.FreeHGlobal((nint)_resultMatrix);
    }
}
