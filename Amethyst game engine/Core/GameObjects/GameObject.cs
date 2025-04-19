using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Models;
using Amethyst_game_engine.Render;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.GameObjects;

public abstract class GameObject : DrawableObject
{
    private bool _disposed = false;
    private protected uint _localRenderSettings = (uint)RenderSettings.All;
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
        _useMeshMatrix = _objectModel.UseMeshMatrix();
    }

    internal override unsafe sealed void DrawObject(Camera? cam, int countOfDirLights, int countOfPointLights, int countOfSpotLights)
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
                primitive.activeShader.SetMatrix4("_model", ModelMatrix);
                primitive.activeShader.SetMatrix4("_view", viewMatrix);
                primitive.activeShader.SetMatrix4("_projection", projectionMatrix);

                if (_useMeshMatrix)
                    primitive.activeShader.SetMatrix4("_mesh", mesh.Matrix);

                primitive.DrawPrimitive(cam is not null ? cam.Position : Vector3.Zero,
                                        countOfDirLights,
                                        countOfPointLights,
                                        countOfSpotLights);
            }
        }
    }

    public override sealed void ChangeRenderSettings(RenderSettings settings)
    {
        _localRenderSettings = (uint)settings;
        _objectModel.RebuildShaders(_localRenderSettings);
    }

    internal override sealed void UpdateShaders()
    {
        _objectModel.RebuildShaders(_localRenderSettings);
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
