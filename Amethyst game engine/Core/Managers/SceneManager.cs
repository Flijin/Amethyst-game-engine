using Amethyst_game_engine.Core.Utilities;

namespace Amethyst_game_engine.Core.Managers;

public sealed class SceneManager : IDisposable
{
    public event Action<BaseScene?>? SceneChanged;
    public event Action<BaseScene>? SceneLoaded;
    public event Action<BaseScene>? SceneUnloaded;

    private GameApplication? _gameApplication;
    private BaseScene? _currentScene;
    private readonly Stack<BaseScene> _sceneHistory = new();
    private readonly Dictionary<string, BaseScene> _scenesRegistry = [];
    private bool _scenePaused;

    public string? CurrentSceneName => _currentScene?.GetType().Name;
    public BaseScene? CurrentScene => _currentScene;
    public GameApplication? Application => _gameApplication;

    public bool ScenePaused
    {
        get => _scenePaused;

        set
        {
            if (_currentScene is null || _scenePaused == value)
                return;

            _scenePaused = value;

            if (value)
                _currentScene?.OnPause();
            else
                _currentScene?.OnResume();
        }
    }

    internal void SetGameApplication(GameApplication app) => _gameApplication = app;

    public void LoadSceneByName(string name)
    {
        if (_scenesRegistry.TryGetValue(name, out BaseScene? result) == false)
            System.PrintMessage($"Error. Scene {name} not found", MessageTypes.ErrorMessage);
        else
            LoadScene(result);
    }

    public void LoadScene(BaseScene scene)
    {
        if (scene == _currentScene)
            return;

        _currentScene?.OnExit();
        _currentScene?.Cleanup();

        _currentScene = scene;
        _currentScene?.SetSceneManager(this);
        _currentScene?.OnStart();

        SceneLoaded?.Invoke(scene);
        SceneChanged?.Invoke(scene);
    }

    public void UnloadScene()
    {
        if (_currentScene is null)
            return;

        var unloadedScene = _currentScene;

        _currentScene?.OnExit();
        _currentScene?.Cleanup();
        _currentScene = null;

        SceneUnloaded?.Invoke(unloadedScene);
        SceneChanged?.Invoke(null);
    }

    public void RegisterScene(BaseScene scene)
    {
        string sceneName = scene.GetType().Name;

        if (_scenesRegistry.ContainsKey(sceneName))
        {
            System.PrintMessage($"Error. Scene {sceneName} already exist", MessageTypes.ErrorMessage);
            return;
        }

        _scenesRegistry.Add(sceneName, scene);
    }

    public void PushScene(BaseScene scene)
    {
        if (_currentScene is not null)
            _sceneHistory.Push(_currentScene);

        LoadScene(scene);
    }

    public void PopScene()
    {
        if (_sceneHistory.Count != 0)
            LoadScene(_sceneHistory.Pop());
        else
            System.PrintMessage("Warning. No scenes in history", MessageTypes.WarningMessage);
    }

    internal void CleanUp()
    {
        BaseScene poppedScene;

        while (_sceneHistory.Count > 0)
        {
            poppedScene = _sceneHistory.Pop();
            poppedScene.Cleanup();
        }

        foreach (var scene in _scenesRegistry.Values)
            scene.Cleanup();

        _currentScene?.Cleanup();

        SceneChanged = null;
        SceneLoaded = null;
        SceneUnloaded = null;
    }

    void IDisposable.Dispose()
    {
        CleanUp();
    }
}
