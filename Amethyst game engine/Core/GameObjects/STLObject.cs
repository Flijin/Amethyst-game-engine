using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Models.STLModule;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.ES30;

namespace Amethyst_game_engine.Core.GameObjects;

public class STLObject(STLModel model, bool useCamera, RenderSettings settings) : GameObject(model, useCamera, settings)
{
    private readonly STLModel _model = model;

    public STLObject(STLModel model, bool useCamera) : this(model, useCamera, RenderSettings.All)
    {

    }

    internal override sealed unsafe void DrawObject(Camera? cam)
    {
        var primitive = objectModel.GetMeshes()[0].primitives[0];

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

        GL.BindVertexArray(primitive.vao);
        primitive.activeShader.Use();

        primitive.activeShader.SetMatrix4("model", ModelMatrix);
        primitive.activeShader.SetMatrix4("view", viewMatrix);
        primitive.activeShader.SetMatrix4("projection", projectionMatrix);

        GL.DrawArrays((PrimitiveType)primitive.mode, 0, primitive.count);
    }
}
