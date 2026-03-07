namespace Amethyst_game_engine.Core.New_classes;

public interface IBaseScene
{
    public GameObjectManager GameObjectManager { get; }

    public CameraManager CameraManager { get; }

    public LightManager LightManager { get; }

    public ISceneManager? SceneManager { get; }
}
