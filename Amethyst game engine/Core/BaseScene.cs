using System.Diagnostics.CodeAnalysis;
using Amethyst_game_engine.Core.Managers;
using OpenTK.Windowing.Common;
namespace Amethyst_game_engine.Core;

public abstract class BaseScene : IDisposable
{
    private SceneManager? _sceneManager;
    private readonly GameObjectManager _gameObjectManager = new();
    private readonly CameraManager _cameraManager = new();
    private readonly LightManager _lightManager = new();

    public GameObjectManager GameObjectManager => _gameObjectManager;
    public CameraManager CameraManager => _cameraManager;
    public LightManager LightManager => _lightManager;
    public SceneManager? SceneManager => _sceneManager;

    public BaseScene() => _gameObjectManager.SetBaseScene(this);
    
    #region Hooks

    protected internal virtual void OnStart()
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnStart();
    }

    protected internal virtual void Update(float deltaTime)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.Update(deltaTime);
    }

    protected internal virtual void FixedUpdate(float fixedDeltaTime)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.FixedUpdate(fixedDeltaTime);
    }
    protected internal virtual void OnExit()
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnExit();
    }

    protected internal virtual void OnPause()
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnPause();
    }

    protected internal virtual void OnResume()
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnResume();
    }

    protected internal virtual void OnKeyDown(KeyboardKeyEventArgs e)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnKeyDown(e);
    }

    protected internal virtual void OnKeyUp(KeyboardKeyEventArgs e)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnKeyUp(e);
    }

    protected internal virtual void OnMouseDown(MouseButtonEventArgs e)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnMouseDown(e);
    }

    protected internal virtual void OnMouseUp(MouseButtonEventArgs e)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnMouseUp(e);
    }

    protected internal virtual void OnMouseWheel(MouseWheelEventArgs e)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnMouseWheel(e);
    }

    protected internal virtual void OnMouseMove(MouseMoveEventArgs e)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnMouseMove(e);
    }

    #endregion

    [MemberNotNull(nameof(_sceneManager))]
    internal void SetSceneManager(SceneManager manager) => _sceneManager = manager;

#pragma warning disable CA1816
    internal void Cleanup()
    {
        _gameObjectManager.Cleanup();
        _cameraManager.Cleanup();
        _lightManager.Cleanup();

        GC.SuppressFinalize(this);
    }

    void IDisposable.Dispose()
    {
        Cleanup();
    }
#pragma warning restore
}
