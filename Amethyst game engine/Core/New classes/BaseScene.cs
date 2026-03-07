using System.Diagnostics.CodeAnalysis;
namespace Amethyst_game_engine.Core.New_classes;

public class BaseScene : IBaseScene, IDisposable
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

    public virtual void OnStart() => _gameObjectManager.OnStart();
    public virtual void Update(float deltaTime) => _gameObjectManager.Update(deltaTime);
    public virtual void FixedUpdate(float fixedDeltaTime) => _gameObjectManager.FixedUpdate(fixedDeltaTime);
    public virtual void OnExit() => _gameObjectManager.OnExit();
    public virtual void OnPause() => _gameObjectManager.OnPause();
    public virtual void OnResume() => _gameObjectManager.OnResume();


    [MemberNotNull(nameof(_sceneManager))]
    internal void SetSceneManager(SceneManager manager) => _sceneManager = manager;

    public void Dispose()
    {
        _gameObjectManager.Dispose();
        _cameraManager.Dispose();
        _lightManager.Dispose();

        GC.SuppressFinalize(this);
    }
}
