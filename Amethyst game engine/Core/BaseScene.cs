﻿using Amethyst_game_engine.CameraModules;
using Amethyst_game_engine.Core.GameObjects;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;

namespace Amethyst_game_engine.Core;

public abstract class BaseScene : IDisposable
{
    internal event Action<Vector3> ColorUpdate;

    private readonly List<DrawableObject> _objects = [];
    private readonly List<StandartCameraController> _cameraControllers = [];
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Dictionary<string, Camera> _cameras = [];
    private Vector3 _backgroundColor = new(0.3f, 0.3f, 0.3f);
    private bool _disposed;

    public float AspectRatio { get; } = Window.AspectRatio;

    public Vector3 BackgroundColor
    {
        get => _backgroundColor;

        set
        {
            _backgroundColor = value;
            ColorUpdate(value);
        }
    }

    protected BaseScene(int sceneUpdateFPS = 60)
    {
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
            SystemSettings.PrintErrorMessage("Warning. The Dispose method was not called, GPU memory leak");
    }

    protected virtual void OnFrameUpdate() { }
    protected virtual void OnFixedTimeUpdate() { }
    protected virtual void OnSceneStart() { }
    protected virtual void OnSceneExit() { }

    internal void DrawScene()
    {
        foreach (var camera in _cameras.Values)
        {
            foreach (var gameObj in _objects)
            {
                gameObj.DrawObject(camera);
            }
        }

        OnFrameUpdate();
    }

    #region group of methods with cameras
    protected void AddControllerToCamera(StandartCameraController controller, string cameraName)
    {
        if (_cameras.TryGetValue(cameraName, out var camera) == true)
        {
            _cameraControllers.Add(controller);
            controller.BindCamera(camera);
        }
        else
        {
            SystemSettings.PrintErrorMessage("Error. There is no camera with that name");
        }       
    }

    protected void AddCamera(Camera cam, string name)
    {
        if (_cameras.TryAdd(name, cam) == false)
            SystemSettings.PrintErrorMessage("Error. A camera with that name has already been added");
    }

    protected void RemoveCamera(string name)
    {
        if (_cameras.Remove(name) == false)
            SystemSettings.PrintErrorMessage("Error. There is no camera with that name");
    }

    protected Camera GetCamera(string name) => _cameras[name];
    #endregion

    #region group of methods with game objects
    protected void AddGameObject(DrawableObject obj)
    {
        _objects.Add(obj);;
    }

    protected void RemoveGameObjects(Predicate<DrawableObject> predicate)
    {
        foreach (var item in _objects)
        {
            if (predicate(item))
            {
                _objects.Remove(item);
                item.Dispose();
            }
        }
    }

    protected DrawableObject[] GetGameObject(Predicate<DrawableObject> predicate)
    {
        List<DrawableObject> result = [];

        foreach (var item in _objects)
        {
            if (predicate(item))
            {
                result.Add(item);
            }
        }

        return [.. result];
    }
    #endregion

    public void Dispose()
    {
        if (_disposed == false)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

            OnSceneExit();

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            foreach (var obj in _objects)
            {
                obj.Dispose();
            }

            foreach (var cam in _cameras.Values)
            {
                cam.Dispose();
            }

            ColorUpdate -= Window.ChangeBackgroundColor;
            Window.ClearInputHandlers();

            GC.SuppressFinalize(this);

            _disposed = true;
        }
    }
}
