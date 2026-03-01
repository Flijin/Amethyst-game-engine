using System.Diagnostics.CodeAnalysis;
namespace Amethyst_game_engine.Core.New_classes;

public class BaseScene : IDisposable
{
    private ISceneManager? _sceneManager;
    private readonly GameObjectsManager _gameObjectManager = new();
    private readonly CameraManager _cameraManager = new();
    private readonly LightManager _lightManager = new();

    public GameObjectsManager GameObjectManager => _gameObjectManager;
    public CameraManager CameraManager => _cameraManager;
    public LightManager LightManager => _lightManager;
    public ISceneManager? SceneManager => _sceneManager;

    public virtual void OnStart() => _gameObjectManager.OnStart();
    public void Update(float deltaTime) => _gameObjectManager.Update(deltaTime);
    public void FixedUpdate(float fixedDeltaTime) => _gameObjectManager.FixedUpdate(fixedDeltaTime);
    public void OnExit() => _gameObjectManager.OnExit();
    public void OnPause() => _gameObjectManager.OnPause();
    public void OnResume() => _gameObjectManager.OnResume();


    [MemberNotNull(nameof(_sceneManager))]
    internal void SetSceneManager(ISceneManager manager) => _sceneManager = manager;

    public void Dispose()
    {
        _gameObjectManager.Dispose();
        GC.SuppressFinalize(this);
    }
}
