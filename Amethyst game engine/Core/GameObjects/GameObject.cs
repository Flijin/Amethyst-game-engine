using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Models;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.ES30;

namespace Amethyst_game_engine.Core.GameObjects;

public abstract class GameObject : DrawableObject
{
    private bool _disposed = false;
    private protected uint _currentRenderState = (uint)RenderSettings.All;
    private protected bool _useCamera;
    private readonly bool _useMeshMatrix;

    private protected readonly IModel _objectModel;

    private protected GameObject(IModel model, bool useCamera, RenderSettings renderKeys) : this(model, useCamera)
    {
        ChangeRenderSettings(renderKeys);
    }

    private protected unsafe GameObject(IModel model, bool useCamera)
    {
        _useCamera = useCamera;
        _objectModel = model;

        if ((model.GetModelSettings() & (1 << 24)) != 0)
            _useMeshMatrix = true;
        else
            _useMeshMatrix = false;
    }

    internal override unsafe sealed void DrawObject(Camera? cam)
    {
        var meshes = _objectModel.GetMeshes();

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

                primitive.activeShader.SetMatrix4("model", ModelMatrix);
                primitive.activeShader.SetMatrix4("view", viewMatrix);
                primitive.activeShader.SetMatrix4("projection", projectionMatrix);

                if (_useMeshMatrix)
                    primitive.activeShader.SetMatrix4("mesh", mesh.Matrix);

                primitive.DrawPrimitive();
            }
        }
    }

    public void ChangeRenderSettings(RenderSettings settings)
    {
        _currentRenderState = (uint)settings;
        _objectModel.RebuildShaders(_currentRenderState & (uint)Window.RenderKeys);
    }

    internal void ChangeRenderSettings()
    {
        _objectModel.RebuildShaders(_currentRenderState & (uint)Window.RenderKeys);
    }

    public override sealed void Dispose()
    {
        if (_disposed == false)
        {
            base.Dispose();

            _objectModel.Dispose();
            GC.SuppressFinalize(this);

            _disposed = true;
        }
    }
}
