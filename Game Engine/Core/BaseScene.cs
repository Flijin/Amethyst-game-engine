using Game_Engine.Core.Render;
using Game_Engine.Interfaces;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Game_Engine.Core;

internal abstract class BaseScene : IScene, IDisposable
{
    public event Action<Vector3> ColorUpdate;

    private readonly List<GameObjectBase3D> _objects = [];
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

    #region group of methods with cameras
    private protected void AddControllerToCamera(StandartCameraController controller, string cameraName)
    {
        if (_cameras.TryGetValue(cameraName, out var camera) == false)
        {
            Program.ShowWindow(Program.WINDOW_DESCRIPTOR, Program.SW_SHOW);
            Console.WriteLine("Ошибка! Камеры с таким именем не существует");
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
            Program.ShowWindow(Program.WINDOW_DESCRIPTOR, Program.SW_SHOW);
            Console.WriteLine("Ошибка! Камера с таким именем уже добавлена");
        }
    }


    private protected void RemoveCamera(string name)
    {
        if (_cameras.Remove(name) == false)
        {
            Program.ShowWindow(Program.WINDOW_DESCRIPTOR, Program.SW_SHOW);
            Console.WriteLine("Ошибка! Камеры с таким именем не существует");
        }
    }

    private protected Camera GetCamera(string name) => _cameras[name];
    #endregion

    #region group of methods with game objects
    private protected void AddGameObject(GameObjectBase3D obj)
    {
        _objects.Add(obj);
        RenderCore.LoadGameObjectInGPU(obj);
    }

    private protected void RemoveGameObject(Predicate<GameObjectBase3D> obj)
    {
        foreach (var item in _objects)
        {
            if (obj(item))
            {
                _objects.Remove(item);
                GL.DeleteBuffer(item.VBO);
            }
        }
    }

    private protected GameObjectBase3D[] GetGameObject(Predicate<GameObjectBase3D> obj)
    {
        List<GameObjectBase3D> result = [];

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
            GL.DeleteBuffer(obj.VBO);
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
