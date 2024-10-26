using Game_Engine.Core.CameraModules;
using Game_Engine.Core.Render;
using Game_Engine.Interfaces;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Game_Engine.Core;

internal abstract class BaseScene : IScene, IDisposable
{
    public event Action<Vector3> ColorUpdate;

    private readonly List<DrawableObject> _objects = [];
    private readonly List<StandartCameraController> _cameraControllers = [];
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Dictionary<string, Camera> _cameras = [];
    private readonly RenderCore _core = new();
    private readonly float _aspectRatio;

    private Vector3 _backgroundColor = new(0.3f, 0.3f, 0.3f);
    private bool _disposed;

    public Vector3 BackgroundColor
    {
        get => _backgroundColor;

        set
        {
            _backgroundColor = value;
            ColorUpdate(value);
        }
    }

    protected BaseScene(Vector2i windowSize, int sceneUpdateFPS = 60)
    {
        _aspectRatio = (float)windowSize.X / windowSize.Y;
        ColorUpdate += Window.ChangeBackgroundColor;
        ColorUpdate(_backgroundColor);

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
            Program.ShowWindow(Program.SW_SHOW);
            Console.Write("Error. The Dispose method was not Called GPU memory leak");
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
                gameObj.DrawObject(_core, camera.Value);
            }
        }

        OnFrameUpdate();
    }

    #region group of methods with cameras
    private protected void AddControllerToCamera(StandartCameraController controller, string cameraName)
    {
        if (_cameras.TryGetValue(cameraName, out var camera) == false)
        {
            Program.ShowWindow(Program.SW_SHOW);
            Console.WriteLine("Error. There is no camera with that name");
        }
        else
        {
            _cameraControllers.Add(controller);
            controller.BindCamera(camera);
        }
        
    }

    private protected void AddCamera(Vector3 position, float fov, string name, float aspectRatio = default)
    {
        float ratio = _aspectRatio;

        if (aspectRatio != default)
            ratio = aspectRatio;

        if (_cameras.TryAdd(name, new Camera(position, ratio, fov)) == false)
        {
            Program.ShowWindow(Program.SW_SHOW);
            Console.WriteLine("Error. A camera with that name has already been added");
        }
    }


    private protected void RemoveCamera(string name)
    {
        if (_cameras.Remove(name) == false)
        {
            Program.ShowWindow(Program.SW_SHOW);
            Console.WriteLine("Error. There is no camera with that name");
        }
    }

    private protected Camera GetCamera(string name) => _cameras[name];
    #endregion

    #region group of methods with game objects
    private protected void AddGameObject(StaticGameObject3D obj)
    {
        _objects.Add(obj);
    }

    private protected void RemoveGameObject(Predicate<DrawableObject> obj)
    {
        foreach (var item in _objects)
        {
            if (obj(item))
            {
                _objects.Remove(item);
                item.Dispose();
            }
        }
    }

    private protected DrawableObject[] GetGameObject(Predicate<DrawableObject> obj)
    {
        List<DrawableObject> result = [];

        foreach (var item in _objects)
        {
            if (obj(item))
            {
                result.Add(item);
            }
        }

        return [.. result];
    }
    #endregion

    public virtual void Dispose(bool disposing)
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _core.Dispose();

        OnSceneExit();
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        foreach (var obj in _objects)
        {
            obj.Dispose();
        }

        ColorUpdate -= Window.ChangeBackgroundColor;
        Window.ClearInputHandlers();

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
