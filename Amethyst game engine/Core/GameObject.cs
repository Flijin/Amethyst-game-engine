using OpenTK.Mathematics;
using Amethyst_game_engine.Models.GLBModule;

namespace Amethyst_game_engine.Core;

public abstract class GameObject : DrawableObject
{
    private Vector3 _scale;
    private Vector3 _position;
    private Vector3 _rotation;
    private Quaternion _rotationQuaternion;

    internal bool useCamera;

    public Vector3 Scale
    {
        get => _scale;

        set
        {
            _scale = value;
            _modelMatrix = Mathematics.MultiplyMatrices(_modelMatrix,
                           Mathematics.CreateScaleMatrix(value.X, value.Y, value.Z));
        }
    }

    public Vector3 Position
    {
        get => _position;

        set
        {
            _position = value;
            _modelMatrix = Mathematics.MultiplyMatrices(_modelMatrix,
                           Mathematics.CreateTranslationMatrix(value.X, value.Y, value.Z));
        }
    }

    public Vector3 Rotation
    {
        get => _rotation;

        set
        {
            _rotation = value;
            _rotationQuaternion = new(value.X, value.Y, value.Z);
            _modelMatrix = Mathematics.MultiplyMatrices(_modelMatrix, _rotationQuaternion.GetRotationMatrix());
        }
    }

    private protected GameObject(Mesh[] meshes, bool useCamera, int shaderID) : base(meshes, shaderID)
    {
        this.useCamera = useCamera;
    }
}
