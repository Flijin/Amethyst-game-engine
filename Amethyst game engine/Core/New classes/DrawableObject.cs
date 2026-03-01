using Amethyst_game_engine.CameraModule;
using Amethyst_game_engine.Render;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Amethyst_game_engine.Core.New_classes;

public abstract class DrawableObject : IDisposable
{
    private bool _disposed = false;
    private Quaternion _rotationQuaternion;

    private protected Vector3 _position;
    private protected Vector3 _rotation;
    private protected Vector3 _scale;

    private readonly unsafe float* _positionMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);
    private readonly unsafe float* _rotationMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);
    private readonly unsafe float* _scaleMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);
    private readonly unsafe float* _resultMatrix = (float*)Marshal.AllocHGlobal(Mathematics.MATRIX_SIZE);

    public virtual unsafe float* ModelMatrix
    {
        get
        {
            float* temp = stackalloc float[16];
            Mathematics.MultiplyMatrices4(_positionMatrix, _rotationMatrix, temp);
            Mathematics.MultiplyMatrices4(temp, _scaleMatrix, _resultMatrix);
            return _resultMatrix;
        }
    }

    public virtual unsafe Vector3 Position
    {
        get => _position;

        set
        {
            _position = value;
            Mathematics.CreateTranslationMatrix4(value.X, value.Y, value.Z, _positionMatrix);
        }
    }

    public virtual Vector3 Rotation
    {
        get => _rotation;

        set
        {
            _rotation = value;
            _rotationQuaternion = new(value.X, value.Y, value.Z);

            unsafe { _rotationQuaternion.GetRotationMatrix(_rotationMatrix); }
        }
    }

    public virtual Vector3 Scale
    {
        get => _scale;

        set
        {
            _scale = value;

            unsafe { Mathematics.CreateScaleMatrix4(value.X, value.Y, value.Z, _scaleMatrix); }
        }
    }

    unsafe protected DrawableObject()
    {
        Buffer.MemoryCopy(Mathematics.IDENTITY_MATRIX, _positionMatrix, Mathematics.MATRIX_SIZE, Mathematics.MATRIX_SIZE);
        Buffer.MemoryCopy(Mathematics.IDENTITY_MATRIX, _rotationMatrix, Mathematics.MATRIX_SIZE, Mathematics.MATRIX_SIZE);
        Buffer.MemoryCopy(Mathematics.IDENTITY_MATRIX, _scaleMatrix, Mathematics.MATRIX_SIZE, Mathematics.MATRIX_SIZE);
    }

    ~DrawableObject()
    {
        if (_disposed == false)
            System.PrintMessage("Warning. The Dispose method was not called, RAM memory leak", MessageTypes.WarningMessage);
    }

    public abstract void OnStart();
    public abstract void Update(float deltaTime);
    public abstract void FixedUpdate(float fixedDeltaTime);
    public abstract void OnExit();
    public abstract void OnPause();
    public abstract void OnResume();

    internal abstract void DrawObject(Camera? cam, int[] ssbo);
    public abstract void ChangeRenderSettings(RenderSettings settings);
    internal abstract void UpdateShaders();

    public virtual void ModifyObject(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }

    public virtual unsafe void Dispose()
    {
        if (_disposed == false)
        {
            Marshal.FreeHGlobal((nint)_positionMatrix);
            Marshal.FreeHGlobal((nint)_rotationMatrix);
            Marshal.FreeHGlobal((nint)_scaleMatrix);
            Marshal.FreeHGlobal((nint)_resultMatrix);

            GC.SuppressFinalize(this);

            _disposed = true;
        }
    }
}
