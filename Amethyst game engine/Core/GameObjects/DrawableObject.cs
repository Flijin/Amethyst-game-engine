using System.Diagnostics.CodeAnalysis;
using Amethyst_game_engine.Core.CameraModule;
using Amethyst_game_engine.Core.GameObjects.Components;
using Amethyst_game_engine.Core.Render.Components;
using Amethyst_game_engine.Core.Render.Settings;
using Amethyst_game_engine.Core.Utilities;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Amethyst_game_engine.Core.GameObjects;

public abstract class DrawableObject : IDisposable
{
    private BaseScene? _scene;

    private readonly Transform _transform;

    [AllowNull]
    private Mesh[] _meshes;
    private bool _useCamera = true;
    
    public BaseScene? Scene => _scene;
    public Transform Transform => _transform;

    public string? Tag { get; set; }
    public bool Visible { get; set; } = true;

    [AllowNull]
    internal string ModelPath { get; set; }
    internal int ModelIndex { get; set; }
    internal Guid ID { get; } = Guid.NewGuid();

    public bool UseCamera
    {
        get => _useCamera;
        set => _useCamera = value;
    }

    private protected Mesh[] Meshes
    {
        get => _meshes;
        set => _meshes = value;
    }

    internal DrawableObject(BoundingBox box)
    {
        _transform = new(box)
        {
            Scale = Vector3.One
        };
    }

    protected internal virtual void OnStart() { }
    protected internal virtual void Update(float deltaTime) { }
    protected internal virtual void FixedUpdate(float fixedDeltaTime) { }
    protected internal virtual void OnExit() { }
    protected internal virtual void OnPause() { }
    protected internal virtual void OnResume() { }
    protected internal virtual void OnKeyDown(KeyboardKeyEventArgs e) { }
    protected internal virtual void OnKeyUp(KeyboardKeyEventArgs e) { }
    protected internal virtual void OnMouseDown(MouseButtonEventArgs e) { }
    protected internal virtual void OnMouseUp(MouseButtonEventArgs e) { }
    protected internal virtual void OnMouseWheel(MouseWheelEventArgs e) { }
    protected internal virtual void OnMouseMove(MouseMoveEventArgs e) { }

    [MemberNotNull(nameof(_scene))]
    internal void SetScene(BaseScene scene) => _scene = scene;

    public void ChangeRenderSettings(RenderSettings settings)
    {
        var app = _scene?.SceneManager?.Application;

        if (app is not null)
        {
            foreach (var mesh in _meshes)
            {
                mesh.BuildShaders(new ShaderBuildingProps()
                {
                    RenderSettings = settings,
                    ShadingModel = app.ShadingModel,
                    GlobalSettings = app.GlobalSettings,
                    UseMeshMatrix = mesh.UseMeshMatrix
                }, app.Settings);
            }
        }
        else
        {
            SystemCalls.PrintMessage("Warning. You can change render settings after adding an object to the scene and load scene", MessageTypes.WarningMessage);
        }
    }

    internal void UpdateRenderSettings()
    {
        var app = _scene!.SceneManager!.Application!;

        foreach (var mesh in _meshes)
        {
            mesh.UpdateShaders(new ShaderBuildingProps()
            {
                RenderSettings = app.Settings,
                ShadingModel = app.ShadingModel,
                GlobalSettings = app.GlobalSettings,
                UseMeshMatrix = mesh.UseMeshMatrix
            });
        }
    }

    internal unsafe void DrawObject()
    {
        var cams = _scene?.CameraManager.Cameras;

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

    internal unsafe void DrawObjectWithCamera(Camera cam) => DrawObject([cam.ViewMatrix, cam.ProjectionMatrix], cam.Position);

    private unsafe void DrawObject(float*[] matrices, Vector3 camPosition)
    {
        foreach (var mesh in _meshes)
        {
            foreach (var primitive in mesh.Primitives)
            {
                primitive.activeShader.Use();
                primitive.activeShader.SetMatrix4("modelMatrix", Transform.ModelMatrix);
                primitive.activeShader.SetMatrix4("viewMatrix", matrices[0]);
                primitive.activeShader.SetMatrix4("projectionMatrix", matrices[1]);

                if (mesh.UseMeshMatrix)
                    primitive.activeShader.SetMatrix4("meshMatrix", mesh.Matrix);

                primitive.DrawPrimitive(camPosition);
            }
        }
    }

#pragma warning disable CA1816
    internal void Cleanup()
    {
        _transform.Dispose();
        
        foreach (var mesh in _meshes)
            mesh.Dispose();

        GC.SuppressFinalize(this);
    }

    void IDisposable.Dispose()
    {
        Cleanup();
    }
#pragma warning restore
}
