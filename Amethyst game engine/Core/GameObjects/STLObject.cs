using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Models.STLModule;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.ES30;

namespace Amethyst_game_engine.Core.GameObjects;

public class STLObject : GameObject
{
    private readonly STLModel _model;
    private Material? _material;

    public Material? Material
    {
        get => _material;

        set
        {
            _material = value;

            if (value != null)
            {
                var keys = RenderSettings.VertexColors | RenderSettings.Normals;

                if (value.BaseColorFactor.isNoneColor == false)
                    keys |= RenderSettings.BaseColorFactor;
                if (value.MetallicFactor != -1)
                    keys |= RenderSettings.MetallicFactor;
                if (value.RoughnessFactor != -1)
                    keys |= RenderSettings.RoughnessFactor;
                if (value.EmissiveFactor != -1)
                    keys |= RenderSettings.EmissiveFactor;
                if (value.NormalScale != -1)
                    keys |= RenderSettings.NormalScale;

                ChangeRenderSettings(keys);
            }
        }
    }

    public STLObject(STLModel model, bool useCamera) : base([model.mesh], useCamera, model.renderKeys)
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
