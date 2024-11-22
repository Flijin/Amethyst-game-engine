using Amethyst_game_engine.CameraModules;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.GameObjects;

public abstract class DrawableObject
{
    private protected Vector3 _position;
    private protected Vector3 _rotation;
    private protected Vector3 _scale;

    private float[,] _positionMatrix = Mathematics.IDENTITY_MATRIX;
    private float[,] _rotationMatrix = Mathematics.IDENTITY_MATRIX;
    private float[,] _scaleMatrix = Mathematics.IDENTITY_MATRIX;

    private Quaternion _rotationQuaternion;

    public virtual float[,] ModelMatrix
    {
        get
        {
            return Mathematics.MultiplyMatrices(Mathematics.MultiplyMatrices(_positionMatrix, _rotationMatrix), _scaleMatrix);
        }
    }

    public virtual Vector3 Position
    {
        get => _position;

        set
        {
            _position = value;
            _positionMatrix = Mathematics.CreateTranslationMatrix(value.X, value.Y, value.Z);
        }
    }

    public virtual Vector3 Rotation
    {
        get => _rotation;

        set
        {
            _rotation = value;
            _rotationQuaternion = new(value.X, value.Y, value.Z);
            _rotationMatrix = _rotationQuaternion.GetRotationMatrix();
        }
    }

    public virtual Vector3 Scale
    {
        get => _scale;

        set
        {
            _scale = value;
            _scaleMatrix = Mathematics.CreateScaleMatrix(value.X, value.Y, value.Z);
        }
    }

    internal abstract void DrawObject(Camera? cam);
    internal abstract void UploadFromMemory();

    public virtual void ModifyObject(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }
}
