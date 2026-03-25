namespace Amethyst_game_engine.Models.GLBModule;

public class GLBModelData
{
    private readonly GLBScene[] _scenes;

    public GLBScene[] Scenes => _scenes;
    public int ScenesCount => _scenes.Length;

    public string? Author { get; internal set; }
    public string? License { get; internal set; }
    public string? Sourse { get; internal set; }
    public string? Title { get; internal set; }

    internal GLBModelData(GLBScene[] scenes)
    {
        _scenes = scenes;
    }
}
