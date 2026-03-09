using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Amethyst_game_engine.Core.New_classes;

public abstract class DrawableObject : IDisposable
{
    private BaseScene? _baseScene;

    private readonly Transform _transform = new();

    [AllowNull]
    private Mesh[] _meshes;
    private bool _useMeshMatrix;
    private bool _useCamera;
    
    public Transform Transform => _transform;
    public string? Tag { get; set; }
    public IBaseScene? BaseScene => _baseScene;
    public bool Visible { get; set; } = true;

    public bool UseCamera
    {
        get => _useCamera;
        set => _useCamera = value;
    }

    internal bool UseMeshMatrix
    {
        get => _useMeshMatrix;
        set => _useMeshMatrix = value;
    }

    internal Mesh[] Meshes
    {
        get => _meshes;
        set => _meshes = value;
    }

    public virtual void OnStart() { }
    public virtual void Update(float deltaTime) { }
    public virtual void FixedUpdate(float fixedDeltaTime) { }
    public virtual void OnExit() { }
    public virtual void OnPause() { }
    public virtual void OnResume() { }
    public virtual void OnKeyDown(KeyboardKeyEventArgs e) { }
    public virtual void OnKeyUp(KeyboardKeyEventArgs e) { }
    public virtual void OnMouseDown(MouseButtonEventArgs e) { }
    public virtual void OnMouseUp(MouseButtonEventArgs e) { }
    public virtual void OnMouseWheel(MouseWheelEventArgs e) { }

    [MemberNotNull(nameof(_baseScene))]
    internal void SetScene(BaseScene scene) => _baseScene = scene;

    internal unsafe void DrawObject()
    {
        var cams = _baseScene?.CameraManager.Cameras;

        if (_useCamera)
        {
            foreach (var camera in cams!)
            {
                DrawObject([camera.ViewMatrix, camera.ProjectionMatrix], camera.Position);
            }
        }
        else
        {
            float* viewMatrix = Mathematics.IDENTITY_MATRIX;
            float* projectionMatrix = Mathematics.IDENTITY_MATRIX;

            DrawObject([viewMatrix, projectionMatrix], Vector3.Zero);
        }

    }

    private unsafe void DrawObject(float*[] matrices, Vector3 camPosition)
    {
        foreach (var mesh in _meshes)
        {
            foreach (var primitive in mesh.primitives)
            {
                primitive.activeShader.Use();
                primitive.activeShader.SetMatrix4("modelMatrix", Transform.ModelMatrix);
                primitive.activeShader.SetMatrix4("viewMatrix", matrices[0]);
                primitive.activeShader.SetMatrix4("projectionMatrix", matrices[1]);

                if (_useMeshMatrix)
                    primitive.activeShader.SetMatrix4("_mesh", mesh.Matrix);

                primitive.DrawPrimitive(camPosition);
            }
        }
    }

#pragma warning disable CA1816
    internal void Cleanup()
    {
        _transform.Dispose();
        GC.SuppressFinalize(this);
    }

    void IDisposable.Dispose()
    {
        Cleanup();
    }
#pragma warning restore
}
