using Game_Engine.Core.Render;
using Game_Engine.Interfaces;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
namespace Game_Engine.Core;

internal abstract class BaseScene : IScene, IDisposable
{
    private readonly List<GameObjectBase3D> _objects = [];
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Dictionary<string, Camera> _cameras = [];
    private readonly RenderCore _core = new();
    private readonly Vector2i _windowSize;

    private bool _disposed;

    public Vector3 BackgroundColor { get; set; } = new(0.3f, 0.3f, 0.3f);

    protected BaseScene(Vector2i windowSize, int sceneUpdateFPS = 60)
    {
        _windowSize = windowSize;

        OnSceneStart();
        var updateTime = TimeSpan.FromMilliseconds(1000d / sceneUpdateFPS);

        Task.Factory.StartNew(() =>
        {
            while (_cancellationTokenSource.IsCancellationRequested == false)
            {
                OnFixedTimeUpdate();
                Thread.Sleep(updateTime);
            }
        }, _cancellationTokenSource.Token);
    }

    ~BaseScene()
    {
        if (_disposed == false)
        {
            Program.ShowWindow(Program.WINDOW_DESCRIPTOR, Program.SW_SHOW);
            Console.Write("Не был использован метод Dispose утечка памяти GPU");
        }
    }

    public virtual void OnFrameUpdate() { }
    public virtual void OnFixedTimeUpdate() { }
    public virtual void OnSceneStart() { }
    public virtual void OnSceneExit() { }

    public void DrawScene()
    {
        foreach (var camera in _cameras)
        {
            foreach (var gameObj in _objects)
            {
                _core.DrawGameObject(gameObj, camera.Value);
            }
        }

        OnFrameUpdate();
    }

    private protected void AddCamera(Vector3 position, float fovy, string name)
    {
        _cameras.Add(name, new Camera(position, _windowSize, fovy));
    }

    private protected void AddGameObject(GameObjectBase3D obj)
    {
        _objects.Add(obj);
        RenderCore.LoadGameObjectInGPU(obj);
    }

    public virtual void Dispose(bool disposing)
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _core.Dispose();

        OnSceneExit();
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        foreach (var obj in _objects)
        {
            GL.DeleteBuffer(obj.VBO);
        }

        _disposed = true;
    }

    public void Dispose()
    {
        if (_disposed == false)
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
