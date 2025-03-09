using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Models;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.ES30;

namespace Amethyst_game_engine.Core.GameObjects;

public abstract class GameObject : DrawableObject
{
    private bool _disposed = false;
    private protected bool _useCamera;
    private uint _currentRenderState = (uint)RenderSettings.All;

    private protected readonly IModel objectModel;

    private protected GameObject(IModel model, bool useCamera, RenderSettings renderKeys) : this(model, useCamera)
    {
        ChangeRenderSettings(renderKeys);
    }

    private protected GameObject(IModel model, bool useCamera)
    {
        _useCamera = useCamera;
        objectModel = model;
    }

    internal override unsafe void DrawObject(Camera? cam)
    {
        var meshes = objectModel.GetMeshes();

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

        foreach (var mesh in meshes)
        {
            foreach (var primitive in mesh.primitives)
            {
                GL.BindVertexArray(primitive.vao);
                primitive.activeShader.Use();
                primitive.DrawPrimitive(viewMatrix, projectionMatrix);
            }
        }
    }

    public void ChangeRenderSettings(RenderSettings settings)
    {
        _currentRenderState = (uint)settings;
        objectModel.RebuildShaders(_currentRenderState & (uint)Window.RenderKeys);
    }

    internal void ChangeRenderSettings()
    {
        objectModel.RebuildShaders(_currentRenderState & (uint)Window.RenderKeys);
    }

    public override sealed void Dispose()
    {
        if (_disposed == false)
        {
            base.Dispose();

            objectModel.Dispose();
            GC.SuppressFinalize(this);

            _disposed = true;
        }
    }
}
