namespace Amethyst_game_engine.Core.New_classes;

public interface ISceneManager
{
    void LoadSceneByName(string name);
    void RegisterScene(BaseScene scene);
    void LoadScene(BaseScene scene);
    void UnloadScene();
    void PushScene(BaseScene scene);
    void PopScene();
}
