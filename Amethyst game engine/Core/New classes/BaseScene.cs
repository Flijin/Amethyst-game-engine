using System.Diagnostics.CodeAnalysis;
using OpenTK.Windowing.Common;
namespace Amethyst_game_engine.Core.New_classes;

public abstract class BaseScene : IBaseScene, IDisposable
{
    private SceneManager? _sceneManager;
    private readonly GameObjectManager _gameObjectManager = new();
    private readonly CameraManager _cameraManager = new();
    private readonly LightManager _lightManager = new();

    public GameObjectManager GameObjectManager => _gameObjectManager;
    public CameraManager CameraManager => _cameraManager;
    public LightManager LightManager => _lightManager;
    public ISceneManager? SceneManager => _sceneManager;

    public BaseScene() => _gameObjectManager.SetBaseScene(this);
    
    #region Hooks

    public virtual void OnStart()
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnStart();
    }

    public virtual void Update(float deltaTime)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.Update(deltaTime);
    }

    public virtual void FixedUpdate(float fixedDeltaTime)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.FixedUpdate(fixedDeltaTime);
    }
    public virtual void OnExit()
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnExit();
    }

    public virtual void OnPause()
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnPause();
    }

    public virtual void OnResume()
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnResume();
    }

    public virtual void OnKeyDown(KeyboardKeyEventArgs e)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnKeyDown(e);
    }

    public virtual void OnKeyUp(KeyboardKeyEventArgs e)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnKeyUp(e);
    }

    public virtual void OnMouseDown(MouseButtonEventArgs e)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnMouseDown(e);
    }

    public virtual void OnMouseUp(MouseButtonEventArgs e)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnMouseUp(e);
    }

    public virtual void OnMouseWheel(MouseWheelEventArgs e)
    {
        foreach (var gameObj in _gameObjectManager.GameObjects)
            gameObj.OnMouseWheel(e);
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
