using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Models.STLModule;
using Amethyst_game_engine.Render;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core;

public class StaticGameObject3D(STLModel model) : DrawableObject(model.Vertices)
{
    private Vector3 _scale;
    private Vector3 _position;
    private float _rotation;

    internal float[,] ModelMatrix => _modelMatrix;
    internal STLModel Model { get; } = model;

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

    public float Rotation
    {
        get => _rotation;

        set
        {
            _rotation = value;
            _modelMatrix = Mathematics.MultiplyMatrices(_modelMatrix,
            Mathematics.CreateRotationXMatrix(
            Mathematics.DegreesToRadians(value)));
        }
    }

    internal override void DrawObject(RenderCore core, Camera cam)
    {
        core.DrawSTLMolel(this, cam);
    }
}
