namespace Amethyst_game_engine.Core.New_classes;

public interface ISceneManager
{
    public bool ScenePaused { get; set; }
    public string? CurrentSceneName { get; }
    public IBaseScene? CurrentScene { get; }

    public void LoadSceneByName(string name);
    public void LoadScene(BaseScene scene);
    public void UnloadScene();
    public void RegisterScene(BaseScene scene);
    public void PushScene(BaseScene scene);
    public void PopScene();
}
