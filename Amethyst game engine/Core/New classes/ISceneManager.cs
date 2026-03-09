namespace Amethyst_game_engine.Core.New_classes;

public interface ISceneManager
{
    public void LoadSceneByName(string name);
    public void LoadScene(BaseScene scene);
    public void UnloadScene();
    public void RegisterScene(BaseScene scene);
    public void PushScene(BaseScene scene);
    public void PopScene();

    public bool ScenePaused { get; set; }
    public string? CurrentSceneName { get; }
    public IBaseScene? CurrentScene { get; }
    public IGameApplication? Application { get; }

    public event Action<BaseScene?>? SceneChanged;
    public event Action<BaseScene>? SceneLoaded;
    public event Action<BaseScene>? SceneUnloaded;
}
