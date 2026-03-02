using System.Diagnostics.CodeAnalysis;
using Amethyst_game_engine.CameraModule;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core.New_classes;

public abstract class DrawableObject : IDisposable
{
    public bool useCamera;
    public BaseScene? _baseScene;

    private readonly Mesh[] _meshes;
    private readonly Transform _transform;
    private bool _useMeshMatrix;
    private bool _useCamera;

    public Transform Transform => _transform;

    internal DrawableObject(Mesh[] meshes)
    {
        _meshes = meshes;
        _transform = new();
    }

    public bool UseCamera
    {
        get => _useCamera;

        set => _useCamera = value;
    }

    public abstract void OnStart();
    public abstract void Update(float deltaTime);
    public abstract void FixedUpdate(float fixedDeltaTime);
    public abstract void OnExit();
    public abstract void OnPause();
    public abstract void OnResume();

    [MemberNotNull(nameof(_baseScene))]
    internal void SetScene(BaseScene scene) => _baseScene = scene;

    internal unsafe void DrawObject(Camera? cam, int[] ssbo)
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

        foreach (var mesh in _meshes)
        {
            foreach (var primitive in mesh.primitives)
            {
                primitive.activeShader.Use();
                primitive.activeShader.SetMatrix4("modelMatrix", Transform.ModelMatrix);
                primitive.activeShader.SetMatrix4("viewMatrix", viewMatrix);
                primitive.activeShader.SetMatrix4("projectionMatrix", projectionMatrix);

                if (_useMeshMatrix)
                    primitive.activeShader.SetMatrix4("_mesh", mesh.Matrix);

                primitive.DrawPrimitive(cam is not null ? cam.Position : Vector3.Zero, ssbo);
            }
        }
    }

    public void Dispose()
    {
        _transform.Dispose();
        GC.SuppressFinalize(this);
    }
}
