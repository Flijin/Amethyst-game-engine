using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Models.STLModule;
using OpenTK.Graphics.OpenGL4;

namespace Amethyst_game_engine.Core.GameObjects;

public class STLObject : GameObject
{
    private readonly STLModel _model;

    public STLObject(STLModel model, bool useCamera) : base([model.mesh], useCamera, model._renderProfile, 512)
    {
        _model = model;
    }

    internal override sealed unsafe void DrawObject(Camera? cam)
    {
        float* viewMatrix;
        float* projectionMatrix;

        if (cam is null || _useCamera == false)
        {
            viewMatrix = Mathematics.IDENTITY_MATRIX;
            projectionMatrix = Mathematics.IDENTITY_MATRIX;
        }
        else
        {
            viewMatrix = cam.ViewMatrix;
            projectionMatrix = cam.ProjectionMatrix;
        }

        var primitive = _meshes[0].primitives[0];

        GL.BindVertexArray(primitive.vao);
        _activeShader.Use();

        _activeShader.SetMatrix4("model", ModelMatrix);
        _activeShader.SetMatrix4("view", viewMatrix);
        _activeShader.SetMatrix4("projection", projectionMatrix);

        GL.DrawArrays((PrimitiveType)primitive.mode, 0, primitive.count);
    }
}
