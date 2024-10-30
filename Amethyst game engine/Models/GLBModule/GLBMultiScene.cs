namespace Amethyst_game_engine.Models.GLBModule;

internal class GLBMultiScene(GLBScene[] scenes, int defaultSceneIndex)
{
    private readonly int _defaultSceneIndex = defaultSceneIndex;
    private readonly GLBScene[] _scenes = scenes;

    public int ScenesCount { get; } = scenes.Length;

    public GLBScene? GetSceneByName(string name) => _scenes.FirstOrDefault(scene => scene.Name == name);
    public GLBScene GetSceneByIndex(int index) => _scenes[index];
    public GLBScene GetDefaultScene() => _scenes[_defaultSceneIndex];
}
